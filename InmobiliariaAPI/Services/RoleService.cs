using InmobiliariaAPI.Data;
using InmobiliariaAPI.Mappers;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly RoleMapeo _roleMapeo;
        private readonly DataContext _dataContext;
        private readonly IPersonaService _personaService;
        private readonly IPersonaRepository _personaRepository;

        public RoleService(
            IRoleRepository roleRepository,
            RoleMapeo roleMapeo,
            DataContext dataContext,
            IPersonaService personaService,
            IPersonaRepository personaRepository
            )
        {
            _roleRepository = roleRepository;
            _roleMapeo = roleMapeo;
            _dataContext = dataContext;
            _personaService = personaService;
            _personaRepository = personaRepository;
        }

        public async Task<object> AssignRoleAsync(int personaId, int rolId)
        {
            if (personaId <= 0 || rolId <= 0)
                throw new InvalidOperationException("PersonaId y RolId deben ser mayores que 0.");

            // Verificar existencia de persona
            var personaDto = await _personaService.ExistsAsync(personaId);
            if (personaDto == null)
                throw new InvalidOperationException($"La persona con ID {personaId} no existe.");

            // Verificar existencia de rol
            var rol = await _roleRepository.GetByIdAsync(rolId);
            if (rol == null)
                throw new InvalidOperationException($"El rol con ID {rolId} no existe.");

            // Obtener roles activos actuales 
            var activos = await _personaRepository.GetActiveRolesAsync(personaId);

            // Determinar si el rol quedó activo
            var asociadoActivo = activos.FirstOrDefault(pr => pr.RolId == rolId);

            var result = new
            {
                personaId,
                rolId,
                nombreRol = rol.Nombre,
                estado = asociadoActivo != null && asociadoActivo.Estado,
                fechaAlta = asociadoActivo?.FechaAlta,
                fechaBaja = asociadoActivo?.FechaBaja,
                rolesActivos = activos.Select(pr => new
                {
                    pr.PersonaId,
                    pr.RolId,
                    nombre = pr.Role?.Nombre,
                    pr.FechaAlta,
                    pr.FechaBaja,
                    pr.Estado
                }).ToList(),
                message = asociadoActivo != null ? "Rol asignado/activado" : "Rol desactivado/quitado"
            };

            return result;
        }

        public async Task<RoleObtenerDTO> CreateAsync(RoleCrearDTO entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var role = new Role
            {
                Nombre = entity.Nombre?.Trim(),
                Descripcion = entity.Descripcion?.Trim()
            };

            var creado = await _roleRepository.AddAsync(role);
            return _roleMapeo.MapToObtenerDTO(creado);
        }

        public async Task<RoleObtenerDTO> DeleteAsync(int id, bool estado)
        {
            // Verificar: existe el rol
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return null;

            // Evita eliminar roles que estén asociados a personas
            var tieneAsociaciones = await _dataContext.PersonaRoles
                .AsNoTracking()
                .AnyAsync(pr => pr.RolId == id && pr.Estado);

            if (tieneAsociaciones)
                throw new InvalidOperationException("No se puede eliminar el rol porque tiene asociaciones activas con personas.");

            var eliminado = await _roleRepository.DeleteAsync(id);
            return _roleMapeo.MapToObtenerDTO(eliminado);
        }

        public async Task<RoleObtenerDTO> ExistsAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return null;
            return _roleMapeo.MapToObtenerDTO(role);
        }

        public async Task<ICollection<RoleObtenerDTO>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(_roleMapeo.MapToObtenerDTO).ToList();
        }

        public async Task<RoleObtenerDTO> GetByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return null;
            return _roleMapeo.MapToObtenerDTO(role);
        }

        public async Task<RoleObtenerDTO> UpdateAsync(int id, RoleActualizarDTO entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var existente = await _roleRepository.GetByIdAsync(id);
            if (existente == null) return null;

            existente.Nombre = entity.Nombre?.Trim();
            existente.Descripcion = entity.Descripcion?.Trim();

            var actualizado = await _roleRepository.UpdateAsync(id, existente);
            return _roleMapeo.MapToObtenerDTO(actualizado);
        }
    }
}
