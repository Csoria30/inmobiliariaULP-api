namespace InmobiliariaAPI.Models.DTO
{
    public class InmuebleCrearFormDTO
    {
        public string Direccion { get; set; }
        public string Uso { get; set; }
        public int Ambientes { get; set; }
        public string Coordenadas { get; set; }
        public decimal PrecioBase { get; set; }
        public int TipoId { get; set; }

        public IFormFile? Foto { get; set; }
    }
}
