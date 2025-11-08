using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAPI.Models.DTO
{
    public class UsuarioCrearDTO
    {
        public int PersonaId { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
    }
}
