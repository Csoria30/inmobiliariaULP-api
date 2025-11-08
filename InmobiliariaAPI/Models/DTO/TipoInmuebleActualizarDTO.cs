using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAPI.Models.DTO
{
    public class TipoInmuebleActualizarDTO
    {
        public int TipoId { get; set; }
        public string Descripcion { get; set; }
    }
}
