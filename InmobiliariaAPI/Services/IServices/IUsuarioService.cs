using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Services.IServices
{
    public interface IUsuarioService
    {
        Task<UsuarioObtenerDTO> CreateAsync(UsuarioCrearDTO dto);
        Task<AuthResponseDTO> AuthenticateAsync(UsuarioLoginDTO dto);
        Task<UsuarioObtenerDTO> GetByPersonaIdAsync(int personaId);
        Task<UsuarioObtenerDTO> GetByIdAsync(int id);

        //Editar perfil y passwords
        Task<bool> ChangePasswordAsync(int personaId, string currentPassword, string newPassword);
        Task<UsuarioObtenerDTO> UpdateAvatarAsync(int personaId, string avatarUrl);
    }
}
