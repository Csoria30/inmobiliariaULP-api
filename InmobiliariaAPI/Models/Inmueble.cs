using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaAPI.Models
{
    [Table("inmuebles")]
    public class Inmueble
    {
        [Key]
        [Column("id_inmueble")]
        public int InmuebleId { get; set; }

        [Column("direccion")]
        public string Direccion { get; set; }

        // ENUM('comercial','residencial') en la BD -> mapeado a string
        [Column("uso")]
        public string Uso { get; set; }

        [Column("ambientes")]
        public int Ambientes { get; set; }

        [Column("coordenadas")]
        public string Coordenadas { get; set; }

        [Column("precio_base", TypeName = "decimal(10,2)")]
        public decimal PrecioBase { get; set; }

        [Column("estado")]
        public bool Estado { get; set; }

        [Column("id_propietario")]
        public int PropietarioId { get; set; }

        [Column("id_tipo")]
        public int TipoId { get; set; }
        
        [Column("imagen")]
        public string? Imagen { get; set; }

        [ForeignKey(nameof(PropietarioId))]
        public virtual Persona Propietario { get; set; }

        [ForeignKey(nameof(TipoId))]
        public virtual TipoInmueble TipoInmueble { get; set; }
    }
}
