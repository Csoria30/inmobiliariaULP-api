namespace InmobiliariaAPI.Models.DTO
{
    public class PersonaActualizarDTO
    {
        public int PersonaId { get; set; }
        public string Dni { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Estado { get; set; }
    }
}
