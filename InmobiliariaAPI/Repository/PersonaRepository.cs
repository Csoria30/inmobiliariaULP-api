using InmobiliariaAPI.Data;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Repository.IRepository
{
    public class PersonaRepository : IPersonaRepository
    {
        private DataContext _dataContext;

        public PersonaRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        //Metodos Base
        public async Task<ICollection<Persona>> GetAllAsync()
        {
            return await _dataContext.Personas
                .AsNoTracking()
                .Where(p => p.Estado)
                .Include(p => p.PersonaRoles)
                     .ThenInclude(pr => pr.Role)
                .ToListAsync();
        }
        public async Task<Persona> GetByIdAsync(int id)
        {
            return await _dataContext.Personas
                .Include(p => p.PersonaRoles)
                    .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(p => p.PersonaId == id && p.Estado);
        }
        public async Task<Persona> AddAsync(Persona entity)
        {
            await _dataContext.Personas.AddAsync(entity);
            await _dataContext.SaveChangesAsync();
            return entity;
        }
        public async Task<Persona> UpdateAsync(int id, Persona entity)
        {
            var persona = await _dataContext.Personas
            .Include(p => p.PersonaRoles)
                .ThenInclude(pr => pr.Role)
            .FirstOrDefaultAsync(p => p.PersonaId == id && p.Estado);


            // Actualizar propiedades
            persona.Dni = entity.Dni;
            persona.Apellido = entity.Apellido;
            persona.Nombre = entity.Nombre;
            persona.Telefono = entity.Telefono;
            persona.Email = entity.Email;
            persona.Estado = entity.Estado;

            _dataContext.Personas.Update(persona);
            await Save();

            // Vuelve a cargar los roles actualizados
            await _dataContext.Entry(persona)
                .Collection(p => p.PersonaRoles)
                .Query()
                .Include(pr => pr.Role)
                .LoadAsync();

            return persona;
        }
        public async Task<Persona> CambiarEstadoAsync(int id, bool estado)
        {
            var persona = await _dataContext.Personas
                .Include(p => p.PersonaRoles)
                .FirstOrDefaultAsync(p => p.PersonaId == id);

            // Cambia el estado de la persona
            persona.Estado = estado;

            // Cambia el estado de los roles asociados
            foreach (var personaRol in persona.PersonaRoles)
            {
                personaRol.Estado = estado;
                personaRol.FechaBaja = estado ? null : DateTime.Now;
                personaRol.FechaAlta = estado ? DateTime.Now : personaRol.FechaAlta;
            }

            _dataContext.Personas.Update(persona);
            await Save();
            return persona;
        }
        public Task<Persona> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task Save()
        {
            await _dataContext.SaveChangesAsync();
        }

        //Metodos Adicionales

        public async Task AddPersonaRoleAsync(PersonaRole personaRole)
        {
            await _dataContext.PersonaRoles.AddAsync(personaRole);
            await _dataContext.SaveChangesAsync();
        }
        public async Task<ICollection<PersonaRole>> GetActiveRolesAsync(int personaId)
        {
            return await _dataContext.PersonaRoles
                .Include(pr => pr.Role)
                .Where(pr => pr.PersonaId == personaId && pr.Estado)
                .ToListAsync();
        }
        public async Task UpdatePersonaRolesAsync(int personaId, int rolId)
        {
            var personaRole = await _dataContext.PersonaRoles
                .FirstOrDefaultAsync(pr => pr.PersonaId == personaId && pr.RolId == rolId);

            if (personaRole == null)
            {
                personaRole = new PersonaRole
                {
                    PersonaId = personaId,
                    RolId = rolId,
                    FechaAlta = DateTime.Now,
                    Estado = true
                };
                await _dataContext.PersonaRoles.AddAsync(personaRole);
            }
            else
            {
                personaRole.Estado = !personaRole.Estado;
                personaRole.FechaBaja = personaRole.Estado ? null : DateTime.Now;
                _dataContext.PersonaRoles.Update(personaRole);
            }

            await Save();
        }
        public async Task<bool> ExistsByDniAsync(string dni)
        {
            return await _dataContext.Personas
                .AsNoTracking()
                .AnyAsync(p => p.Dni == dni && p.Estado);
        }
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dataContext.Personas
                .AsNoTracking()
                .AnyAsync(p => p.Email == email && p.Estado);
        }
        public async Task<Persona> GetByDniAsync(string dni)
        {
            return await _dataContext.Personas
                .Include(p => p.PersonaRoles)
                .FirstOrDefaultAsync(p => p.Dni == dni && p.Estado);
        }
        public async Task<Persona> GetByEmailAsync(string email)
        {
            return await _dataContext.Personas
                .Include(p => p.PersonaRoles)
                .FirstOrDefaultAsync(p => p.Email == email && p.Estado);
        }

    }
}
