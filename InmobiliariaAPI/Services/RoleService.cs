using InmobiliariaAPI.Data;
using InmobiliariaAPI.Mappers;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
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

        public RoleService(
            IRoleRepository roleRepository,
            RoleMapeo roleMapeo,
            DataContext dataContext
            )
        {
            _dataContext = dataContext;
            _roleMapeo = roleMapeo;
            _roleRepository = roleRepository;
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
