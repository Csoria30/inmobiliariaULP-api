using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;

namespace InmobiliariaAPI.Services.IServices
{
    public interface IPersonaService : ICommonService<PersonaObtenerDTO, PersonaCrearDTO, PersonaActualizarDTO>
    {
        // Recupera la persona por dni
        Task<PersonaObtenerDTO> GetByDniAsync(string dni);

        //Recupera la persona por email
        Task<PersonaObtenerDTO> GetByEmailAsync(string email);

        //Recupera la persona por id incluyendo sus roles activos
        Task<PersonaObtenerDTO> GetByIdWithRolesAsync(int id);

        // Comprueba si la persona tiene un rol válido para crear/registrar un usuario
        Task<bool> HasValidRoleForUserAsync(int personaId);

        // Busca la persona por email y devuelve sus roles activos junto con los datos de persona
        Task<PersonaObtenerDTO> GetByEmailWithRolesAsync(string email);

        // Devuelve un array con los nombres de los roles activos de la persona
        Task<string[]> GetActiveRoleNamesAsync(int personaId);

        // Comprueba si la persona tiene un rol concreto
        Task<bool> HasRoleAsync(int personaId, string roleName);

        //Comprueba si la persona tiene al menos uno de los roles pasados
        Task<bool> HasAnyRoleAsync(int personaId, params string[] roleNames);
    }
}
