using InmobiliariaAPI.Models;

namespace InmobiliariaAPI.Repository.IRepository
{
    public interface IUsuarioRepository : ICommonRepository<Usuario>
    {
        Task<Usuario> GetByPersonaIdAsync(int personaId);
        Task<Usuario> GetByIdAsync(int id);
        Task<Usuario> GetByPersonaIdIncludingPersonaAsync(int personaId);

    }
}
