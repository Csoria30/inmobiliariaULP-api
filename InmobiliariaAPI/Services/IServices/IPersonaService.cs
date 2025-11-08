using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;

namespace InmobiliariaAPI.Services.IServices
{
    public interface IPersonaService : ICommonService<PersonaObtenerDTO, PersonaCrearDTO, PersonaActualizarDTO>
    {
        Task<PersonaObtenerDTO> GetByDniAsync(string dni);
        Task<PersonaObtenerDTO> GetByEmailAsync(string email);
    }
}
