using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaAPI.Models
{
    [Table("personas")]
    public class Persona
    {
        [Key]
        [Column("id_persona")]
        public int PersonaId { get; set; }

        [Column("dni")]
        public string? Dni { get; set; }

        [Column("apellido")]
        public string? Apellido { get; set; }

        [Column("nombre")]
        public string? Nombre { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("estado")]
        public bool Estado { get; set; }

        // Navegaciones
        public virtual ICollection<PersonaRole>? PersonaRoles { get; set; }
        public virtual ICollection<Usuario>? Usuarios { get; set; }
        public virtual ICollection<Inmueble>? Inmuebles { get; set; } //  propietario
        public virtual ICollection<Contrato>? ContratosComoInquilino { get; set; }
    }

}
