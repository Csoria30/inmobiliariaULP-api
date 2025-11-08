using InmobiliariaAPI.Models;

namespace InmobiliariaAPI.Repository.IRepository
{
    public interface IPersonaRepository : ICommonRepository<Persona>
    {
        Task AddPersonaRoleAsync(PersonaRole personaRole);
        Task UpdatePersonaRolesAsync(int personaId, int rolId);
        Task<ICollection<PersonaRole>> GetActiveRolesAsync(int personaId);
        Task<bool> ExistsByDniAsync(string dni);
        Task<bool> ExistsByEmailAsync(string email);
        Task<Persona> GetByDniAsync(string dni);
        Task<Persona> GetByEmailAsync(string email);
    }
}
