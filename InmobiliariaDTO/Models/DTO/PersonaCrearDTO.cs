namespace InmobiliariaDTO
{
    public class PersonaCrearDTO
    {
        public string Dni { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        
        //Lista de roles, porque puede tener mas de uno 
        public List<int> IdRoles { get; set; } = new();
    }
}
