using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaAPI.Models
{
    [Table("personas_roles")]
    public class PersonaRole
    {
        [Column("id_persona")]
        public int PersonaId { get; set; }

        [Column("id_rol")]
        public int RolId { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        [Column("fecha_baja")]
        public DateTime? FechaBaja { get; set; }

        [Column("estado")]
        public bool Estado { get; set; }

        // Navegaciones
        [ForeignKey(nameof(PersonaId))]
        public virtual Persona Persona { get; set; }

        [ForeignKey(nameof(RolId))]
        public virtual Role Role { get; set; }
    }
}
