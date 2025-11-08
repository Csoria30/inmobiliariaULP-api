using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaAPI.Models
{
    [Table("contratos")]
    public class Contrato
    {
        [Key]
        [Column("id_contrato")]
        public int ContratoId { get; set; }

        [Column("id_inmueble")]
        public int InmuebleId { get; set; }

        [Column("id_inquilino")]
        public int InquilinoId { get; set; }

        [Column("id_usuario")]
        public int UsuarioId { get; set; }

        [Column("id_usuario_finaliza")]
        public int? UsuarioFinalizaId { get; set; }

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_fin")]
        public DateTime FechaFin { get; set; }

        [Column("monto_mensual", TypeName = "decimal(10,2)")]
        public decimal MontoMensual { get; set; }

        [Column("fecha_finalizacion_anticipada")]
        public DateTime? FechaFinalizacionAnticipada { get; set; }

        [Column("multa", TypeName = "decimal(10,2)")]
        public decimal? Multa { get; set; }

        // ENUM('vigente','finalizado','rescindido') -> mapeado a string
        [Column("estado")]
        public string Estado { get; set; }

        [ForeignKey(nameof(InmuebleId))]
        public virtual Inmueble Inmueble { get; set; }

        [ForeignKey(nameof(InquilinoId))]
        public virtual Persona Inquilino { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey(nameof(UsuarioFinalizaId))]
        public virtual Usuario UsuarioFinaliza { get; set; }
    }
}
