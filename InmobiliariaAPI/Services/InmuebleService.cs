using FluentValidation;
using FluentValidation.Results;
using InmobiliariaAPI.Data;
using InmobiliariaAPI.InmobiliariaMappers;
using InmobiliariaAPI.Mappers;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Services
{
    public class InmuebleService : IInmuebleService
    {
        private readonly IInmuebleRepository _inmuebleRepository;
        private readonly InmuebleMapeo _inmuebleMapeo;
        private readonly DataContext _dataContext;

        public InmuebleService(
            IInmuebleRepository inmuebleRepository,
            InmuebleMapeo inmuebleMapeo,
            DataContext dataContext
            )
        {
            _inmuebleRepository = inmuebleRepository;
            _inmuebleMapeo = inmuebleMapeo;
            _dataContext = dataContext;
        }

        public async Task<InmuebleObtenerDTO> CreateAsync(InmuebleCrearDTO entity)
        {

            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            // Normalizar valores
            var direccionTrim = entity.Direccion?.Trim();

            // Verificar si existe el propietario,  cargar sus roles y estado Activo
            var propietario = await _dataContext.Personas
                .Include(p => p.PersonaRoles)
                    .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(p => p.PersonaId == entity.PropietarioId && p.Estado);

            if (propietario == null)    
                throw new InvalidOperationException($"La persona con id {entity.PropietarioId} no existe o está inactiva.");

            // Verificar Rol de Propietario y estado Activo
            var tieneRolPropietario = propietario.PersonaRoles?
                .Any(pr => pr.Estado && pr.Role != null &&
                           string.Equals(pr.Role.Nombre, "propietario", StringComparison.OrdinalIgnoreCase)) == true;

            if (!tieneRolPropietario)
                throw new InvalidOperationException($"La persona con id {entity.PropietarioId} no tiene el rol de propietario.");

            //Verificar Tipo de inmueble
            var tipoExiste = await _dataContext.TipoInmuebles
                .AnyAsync(t => t.TipoId == entity.TipoId);

            if (!tipoExiste)
                throw new InvalidOperationException($"El tipo de inmueble {entity.TipoId} no existe.");

            //Verificar Direccion unica
            var direccionDuplicada = await _dataContext.Inmuebles
                .AnyAsync(i => i.Direccion == direccionTrim && i.Estado);

            if (direccionDuplicada)
                throw new InvalidOperationException($"Ya existe un inmueble con la dirección '{direccionTrim}'.");

            //Mapeo
            var entidad = _inmuebleMapeo.MapToEntidadDesdeCrear(entity);
            var creado = await _inmuebleRepository.AddAsync(entidad);

            return _inmuebleMapeo.MapToObtenerDTO(creado);

        }

        public async Task<InmuebleObtenerDTO> DeleteAsync(int id, bool estado)
        {
            var actualizado = await _inmuebleRepository.CambiarEstadoAsync(id, estado);
            if (actualizado == null) return null;

            return _inmuebleMapeo.MapToObtenerDTO(actualizado);
        }

        public async Task<InmuebleObtenerDTO> ExistsAsync(int id)
        {
            var inmueble = await _inmuebleRepository.GetByIdAsync(id);
            if (inmueble == null || !inmueble.Estado) return null;

            return _inmuebleMapeo.MapToObtenerDTO(inmueble);
        }

        public async Task<ICollection<InmuebleObtenerDTO>> GetAllAsync()
        {
            var inmuebles = await _inmuebleRepository.GetAllAsync();
            var activos = inmuebles?.Where(i => i.Estado).ToList() ?? new List<Inmueble>();

            return activos.Select(i => _inmuebleMapeo.MapToObtenerDTO(i)).ToList();
        }

        public async Task<InmuebleObtenerDTO> GetByIdAsync(int id)
        {
            var inmueble = await _inmuebleRepository.GetByIdAsync(id);
            if (inmueble == null || !inmueble.Estado) return null;
            return _inmuebleMapeo.MapToObtenerDTO(inmueble);
        }

        public async Task<InmuebleObtenerDTO> UpdateAsync(int id, InmuebleActualizarDTO entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var existente = await _inmuebleRepository.GetByIdAsync(id);
            if (existente == null) return null;

            // Normalizar
            var direccionTrim = entity.Direccion?.Trim();

            // Verificar: Propietario cambio? , Existencia y estado Activo
            if (entity.PropietarioId != existente.PropietarioId)
            {
                var propietario = await _dataContext.Personas
                    .Include(p => p.PersonaRoles)
                        .ThenInclude(pr => pr.Role)
                    .FirstOrDefaultAsync(p => p.PersonaId == entity.PropietarioId && p.Estado);

                if (propietario == null)
                    throw new InvalidOperationException($"La persona con id {entity.PropietarioId} no existe o está inactiva.");

                var tieneRolPropietario = propietario.PersonaRoles?
                    .Any(pr => pr.Estado && pr.Role != null &&
                               string.Equals(pr.Role.Nombre, "propietario", StringComparison.OrdinalIgnoreCase)) == true;

                if (!tieneRolPropietario)
                    throw new InvalidOperationException($"La persona con id {entity.PropietarioId} no tiene el rol de propietario.");
            }

            // Verificar tipo
            var tipoExiste = await _dataContext.TipoInmuebles
                .AnyAsync(t => t.TipoId == entity.TipoId);

            if (!tipoExiste)
                throw new InvalidOperationException($"El tipo de inmueble {entity.TipoId} no existe.");

            // Verificar duplicado de direccion excluyendo el mismo registro
            var direccionDuplicada = await _dataContext.Inmuebles
                .AnyAsync(i => i.Direccion == direccionTrim && i.InmuebleId != id && i.Estado);

            if (direccionDuplicada)
                throw new InvalidOperationException($"Ya existe un inmueble con la dirección '{direccionTrim}'.");

            // Aplicar cambios sobre la entidad existente y persistir
            _inmuebleMapeo.MapToEntidadDesdeActualizar(entity, existente);
            var actualizado = await _inmuebleRepository.UpdateAsync(id, existente);

            return _inmuebleMapeo.MapToObtenerDTO(actualizado);
        }
    }
}
