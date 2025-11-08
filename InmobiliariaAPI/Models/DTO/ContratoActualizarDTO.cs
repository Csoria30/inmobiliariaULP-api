using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAPI.Models.DTO
{
    public class ContratoActualizarDTO
    {
        public int ContratoId { get; set; }
        public int? InmuebleId { get; set; }
        public int? InquilinoId { get; set; }
        public int? UsuarioId { get; set; }
        public int? UsuarioFinalizaId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal? MontoMensual { get; set; }
        public DateTime? FechaFinalizacionAnticipada { get; set; }
        public decimal? Multa { get; set; }
        public string Estado { get; set; }
    }
}
