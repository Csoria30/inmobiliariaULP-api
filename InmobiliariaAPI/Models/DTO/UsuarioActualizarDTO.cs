using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAPI.Models.DTO
{
    public class UsuarioActualizarDTO
    {
        public int UsuarioId { get; set; }
        public int? PersonaId { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
    }
}
