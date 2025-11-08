using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaAPI.Models
{
    [Table("pagos")]
    public class Pago
    {
        [Key]
        [Column("id_pago")]
        public int PagoId { get; set; }

        [Column("id_contrato")]
        public int ContratoId { get; set; }

        [Column("id_usuario")]
        public int UsuarioId { get; set; }

        [Column("fecha_pago")]
        public DateTime FechaPago { get; set; }

        [Column("numero_pago")]
        public string NumeroPago { get; set; }

        [Column("importe", TypeName = "decimal(10,2)")]
        public decimal Importe { get; set; }

        [Column("concepto")]
        public string Concepto { get; set; }

        // ENUM('aprobado','anulado') -> mapeado a string
        [Column("estado_pago")]
        public string EstadoPago { get; set; }

        [ForeignKey(nameof(ContratoId))]
        public virtual Contrato Contrato { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; }
    }
}
