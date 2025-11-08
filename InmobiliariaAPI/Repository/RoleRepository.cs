using InmobiliariaAPI.Data;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private DataContext _dataContext;

        public RoleRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        //Metodos Base
        public async Task<Role> AddAsync(Role entity)
        {
            await _dataContext.Roles.AddAsync(entity);
            await _dataContext.SaveChangesAsync();
            return entity;
        }

        public Task<Role> CambiarEstadoAsync(int id, bool estado)
        {
            throw new NotSupportedException("No corresponde a la entidad Role");
        }

        public async Task<Role> DeleteAsync(int id)
        {
            var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.RolId == id);

            _dataContext.Roles.Remove(role);
            await _dataContext.SaveChangesAsync();
            return role;
        }

        public async Task<ICollection<Role>> GetAllAsync()
        {
            return await _dataContext.Roles
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            return await _dataContext.Roles
                .FirstOrDefaultAsync(r => r.RolId == id);
        }

        public async Task Save()
        {
            await _dataContext.SaveChangesAsync();
        }

        public async Task<Role> UpdateAsync(int id, Role entity)
        {
            var role = await _dataContext.Roles
                .FirstOrDefaultAsync(r => r.RolId == id);

            role.Nombre = entity.Nombre;
            role.Descripcion = entity.Descripcion;

            _dataContext.Roles.Update(role);
            await _dataContext.SaveChangesAsync();

            return role;
        }


        //Metodos Adicionales

    }
}
