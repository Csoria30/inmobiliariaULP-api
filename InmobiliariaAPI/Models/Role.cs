using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaAPI.Models
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("id_rol")]
        public int RolId { get; set; }

        [Column("nombre")]
        public string? Nombre { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        // Navegación
        public virtual ICollection<PersonaRole>? PersonaRoles { get; set; }
    }
}
