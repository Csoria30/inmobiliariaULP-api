using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaAPI.Models
{
    [Table("tipos_inmueble")]
    public class TipoInmueble
    {
        [Key]
        [Column("id_tipo")]
        public int TipoId { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        public virtual ICollection<Inmueble> Inmuebles { get; set; }
    }
}
