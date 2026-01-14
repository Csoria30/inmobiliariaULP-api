using InmobiliariaDTO;
using System.Security.Claims;

namespace InmobiliariaMVC.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Error, IEnumerable<Claim> Claims)> AuthenticateAsync(UsuarioLoginDTO model);
        Task LogoutAsync();
    }
}
