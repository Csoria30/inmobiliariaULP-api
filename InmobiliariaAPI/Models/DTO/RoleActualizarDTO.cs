using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAPI.Models.DTO
{
    public class RoleActualizarDTO
    {
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }
}
