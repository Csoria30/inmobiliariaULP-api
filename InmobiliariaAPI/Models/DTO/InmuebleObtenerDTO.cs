namespace InmobiliariaAPI.Models.DTO
{
    public class InmuebleObtenerDTO
    {
        public int InmuebleId { get; set; }

        public string Direccion { get; set; }

        public string Uso { get; set; }

        public int Ambientes { get; set; }

        public string Coordenadas { get; set; }

        public decimal PrecioBase { get; set; }

        public bool Estado { get; set; }

        public int PropietarioId { get; set; }

        public int TipoId { get; set; }
    }
}
