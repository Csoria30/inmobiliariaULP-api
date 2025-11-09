using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Services.IServices
{
    public interface IUsuarioService
    {
        Task<UsuarioObtenerDTO> CreateAsync(UsuarioCrearDTO dto);
        Task<AuthResponseDTO> AuthenticateAsync(UsuarioLoginDTO dto);
        Task<UsuarioObtenerDTO> GetByPersonaIdAsync(int personaId);
        Task<UsuarioObtenerDTO> GetByIdAsync(int id);
    }
}
