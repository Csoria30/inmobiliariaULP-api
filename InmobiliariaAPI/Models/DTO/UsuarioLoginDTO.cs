using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAPI.Models.DTO
{
    public class UsuarioLoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
