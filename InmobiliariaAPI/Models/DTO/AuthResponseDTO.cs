namespace InmobiliariaAPI.Models.DTO
{
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int UsuarioId { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
    }
}
