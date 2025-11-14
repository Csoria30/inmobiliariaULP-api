using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;

namespace InmobiliariaAPI.Services.IServices
{
    public interface IPersonaService : ICommonService<PersonaObtenerDTO, PersonaCrearDTO, PersonaActualizarDTO>
    {
        Task<PersonaObtenerDTO> GetByDniAsync(string dni);
        Task<PersonaObtenerDTO> GetByEmailAsync(string email);
        Task<PersonaObtenerDTO> GetByIdWithRolesAsync(int id);
        Task<bool> HasValidRoleForUserAsync(int personaId);
        Task<PersonaObtenerDTO> GetByEmailWithRolesAsync(string email);
        Task<string[]> GetActiveRoleNamesAsync(int personaId);
    }
}
