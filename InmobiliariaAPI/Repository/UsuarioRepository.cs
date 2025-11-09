using InmobiliariaAPI.Data;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DataContext _dataContext;

        public UsuarioRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<ICollection<Usuario>> GetAllAsync()
        {
            return await _dataContext.Usuarios
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Usuario> GetByIdAsync(int id)
        {
            return await _dataContext.Usuarios
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);
        }

        public async Task<Usuario> GetByPersonaIdAsync(int personaId)
        {
            return await _dataContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PersonaId == personaId);
        }

        public async Task<Usuario> GetByPersonaIdIncludingPersonaAsync(int personaId)
        {
            return await _dataContext.Usuarios
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.PersonaId == personaId);
        }

        public async Task<Usuario> AddAsync(Usuario entity)
        {
            await _dataContext.Usuarios.AddAsync(entity);
            await _dataContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Usuario> UpdateAsync(int id, Usuario entity)
        {
            var user = await _dataContext.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == id);
            if (user == null) return null;

            user.Password = entity.Password;
            user.Avatar = entity.Avatar;
            user.PersonaId = entity.PersonaId;

            _dataContext.Usuarios.Update(user);
            await Save();
            return user;
        }

        public async Task<Usuario> CambiarEstadoAsync(int id, bool estado)
        {
            throw new NotSupportedException("No aplica");
        }

        public async Task<Usuario> DeleteAsync(int id)
        {
            var user = await _dataContext.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == id);
            if (user == null) return null;

            _dataContext.Usuarios.Remove(user);
            await Save();
            return user;
        }

        public async Task Save()
        {
            await _dataContext.SaveChangesAsync();
        }
    }
}
