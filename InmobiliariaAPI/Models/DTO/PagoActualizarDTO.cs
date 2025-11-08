using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAPI.Models.DTO
{
    public class PagoActualizarDTO
    {
        public int PagoId { get; set; }
        public int? ContratoId { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime? FechaPago { get; set; }
        public string NumeroPago { get; set; }
        public decimal? Importe { get; set; }
        public string Concepto { get; set; }
        public string EstadoPago { get; set; }
    }
}
