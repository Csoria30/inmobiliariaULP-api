using InmobiliariaDTO;
using InmobiliariaMVC.Services.Interfaces;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace InmobiliariaMVC.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
        public RoleService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<List<RoleObtenerDTO>> GetAllAsync()
        {
            var resp = await _http.GetFromJsonAsync<List<RoleObtenerDTO>>("Roles");
            return resp ?? new List<RoleObtenerDTO>();
        }
    }
}
