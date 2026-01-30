using InmobiliariaDTO;
using InmobiliariaMVC.Services.Interfaces;
using static System.Net.WebRequestMethods;

namespace InmobiliariaMVC.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly HttpClient _httpClient;
        public RoleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RoleObtenerDTO>> GetAllAsync()
        {
            var resp = await _httpClient.GetFromJsonAsync<List<RoleObtenerDTO>>("api/Roles");
            return resp ?? new List<RoleObtenerDTO>();
        }
    }
}
