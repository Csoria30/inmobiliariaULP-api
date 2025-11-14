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
        private readonly IPersonaService _personaService;

        public InmuebleService(
            IInmuebleRepository inmuebleRepository,
            InmuebleMapeo inmuebleMapeo,
            DataContext dataContext,
            IPersonaService personaService
            )
        {
            _inmuebleRepository = inmuebleRepository;
            _inmuebleMapeo = inmuebleMapeo;
            _dataContext = dataContext;
            _personaService = personaService;
        }

        public async Task<InmuebleObtenerDTO> CreateAsync(InmuebleCrearDTO inmuebleCrearDTO)
        {            
            
            // Normalizar valores
            var direccionTrim = inmuebleCrearDTO.Direccion?.Trim();

            // Verificar si existe el propietario,  cargar sus roles y estado Activo
            var propietarioDto = await _personaService.GetByIdWithRolesAsync(inmuebleCrearDTO.PropietarioId);
            if (propietarioDto == null)
                throw new InvalidOperationException($"La persona con id {inmuebleCrearDTO.PropietarioId} no existe o está inactiva.");

            // Verificar Rol de Propietario y estado Activo
            var tieneRolPropietario = await _personaService.HasRoleAsync(propietarioDto.PersonaId, "propietario");
            if (!tieneRolPropietario)
                throw new InvalidOperationException($"La persona con id {inmuebleCrearDTO.PropietarioId} no tiene el rol de propietario.");

            //Verificar Tipo de inmueble
            var tipoExiste = await _inmuebleRepository.TipoExistsAsync(inmuebleCrearDTO.TipoId);
            if (!tipoExiste)
                throw new InvalidOperationException($"El tipo de inmueble {inmuebleCrearDTO.TipoId} no existe.");

            //Verificar Direccion unica y estado Activo
            var direccionDuplicada = await _inmuebleRepository.DireccionExistsAsync(direccionTrim);
            if (direccionDuplicada)
                throw new InvalidOperationException($"Ya existe un inmueble con la dirección '{direccionTrim}'.");

            //Mapeo
            var entidad = _inmuebleMapeo.MapToEntidadDesdeCrear(inmuebleCrearDTO);
            var creado = await _inmuebleRepository.AddAsync(entidad);

            return _inmuebleMapeo.MapToObtenerDTO(creado);
        }

        public async Task<InmuebleObtenerDTO> DeleteAsync(int id, bool estado)
        {
            var existe = await ExistsAsync(id);
            if (existe == null)
                throw new InvalidOperationException($"El inmueble con ID {id} no existe");

            var actualizado = await _inmuebleRepository.CambiarEstadoAsync(id, estado);
            
            return _inmuebleMapeo.MapToObtenerDTO(actualizado);
        }

        public async Task<InmuebleObtenerDTO> ExistsAsync(int id)
        {
            var inmueble = await _inmuebleRepository.GetByIdAsync(id);
            if (inmueble == null || !inmueble.Estado) 
                throw new InvalidOperationException($"El inmueble con ID {id} no existe o está inactivo.");

            return _inmuebleMapeo.MapToObtenerDTO(inmueble);
        }

        public async Task<ICollection<InmuebleObtenerDTO>> GetAllAsync()
        {
            var inmuebles = await _inmuebleRepository.GetAllAsync();
            return inmuebles.Select(i => _inmuebleMapeo.MapToObtenerDTO(i)).ToList();
        }

        public async Task<ICollection<InmuebleObtenerDTO>> GetActiveAsync()
        {
            var activos = await _inmuebleRepository.GetActiveAsync();
            return activos.Select(i => _inmuebleMapeo.MapToObtenerDTO(i)).ToList();
        }

        public async Task<InmuebleObtenerDTO> GetByIdAsync(int id)
        {
            var inmueble = await _inmuebleRepository.GetByIdAsync(id);
            if (inmueble == null || !inmueble.Estado) 
                throw new InvalidOperationException("El inmueble no existe o está inactivo.");

            return _inmuebleMapeo.MapToObtenerDTO(inmueble);
        }

        public async Task<InmuebleObtenerDTO> UpdateAsync(int id, InmuebleActualizarDTO inmuebleActualizarDTO)
        {
            var existente = await _inmuebleRepository.GetByIdAsync(id);
            if (existente == null) 
                throw new InvalidOperationException($"El inmueble con ID {id} no existe.");

            // Normalizar
            var direccionTrim = inmuebleActualizarDTO.Direccion?.Trim();

            // Verificar: Propietario cambio? , Existencia y estado Activo
            if (inmuebleActualizarDTO.PropietarioId != existente.PropietarioId)
            {
                var propietarioDto = await _personaService.GetByIdWithRolesAsync(inmuebleActualizarDTO.PropietarioId);
                if (propietarioDto == null)
                    throw new InvalidOperationException($"La persona con id {inmuebleActualizarDTO.PropietarioId} no existe o está inactiva.");

                var tieneRolPropietario = await _personaService.HasRoleAsync(propietarioDto.PersonaId, "propietario");
                if (!tieneRolPropietario)
                    throw new InvalidOperationException($"La persona con id {inmuebleActualizarDTO.PropietarioId} no tiene el rol de propietario.");
            }

            // Verificar tipo
            var tipoExiste = await _inmuebleRepository.TipoExistsAsync(inmuebleActualizarDTO.TipoId);
            if (!tipoExiste)
                throw new InvalidOperationException($"El tipo de inmueble {inmuebleActualizarDTO.TipoId} no existe.");

            // Verificar duplicado de direccion excluyendo el mismo registro
            var direccionDuplicada = await _inmuebleRepository.DireccionExistsAsync(direccionTrim);
            if (direccionDuplicada)
                throw new InvalidOperationException($"Ya existe un inmueble con la dirección '{direccionTrim}'.");

            // Aplicar cambios sobre la entidad existente y persistir
            var entidadActualizada = _inmuebleMapeo.MapToEntidadDesdeActualizarAndReturn(inmuebleActualizarDTO, existente);
            var actualizado = await _inmuebleRepository.UpdateAsync(id, entidadActualizada);

            return _inmuebleMapeo.MapToObtenerDTO(actualizado);
        }

        public async Task<ICollection<InmuebleObtenerDTO>> GetByPropietarioAsync(int propietarioId)
        {
            if (propietarioId <= 0) 
                throw new InvalidOperationException("El ID del propietario no es válido.");

            var inmuebles = await _inmuebleRepository.GetByPropietarioAsync(propietarioId);
            return inmuebles.Select(i => _inmuebleMapeo.MapToObtenerDTO(i)).ToList();

        }
    }
}
