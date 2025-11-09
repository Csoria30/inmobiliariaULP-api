using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaAPI.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int UsuarioId { get; set; }

        [Column("id_persona")]
        public int PersonaId { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("avatar")]
        public string? Avatar { get; set; }

        [ForeignKey(nameof(PersonaId))]
        public virtual Persona? Persona { get; set; }
    }

}
