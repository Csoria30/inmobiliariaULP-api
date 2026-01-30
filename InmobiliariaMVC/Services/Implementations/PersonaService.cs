using InmobiliariaDTO;
using InmobiliariaMVC.Services.Interfaces;
using System.Text.Json;

namespace InmobiliariaMVC.Services.Implementations
{
    public class PersonaService : IPersonaService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public PersonaService(HttpClient http)
        {
            _http = http;
        }

        public async Task<PersonaObtenerDTO> CreateAsync(PersonaCrearDTO dto)
        {
            if(dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "El objeto dto no puede ser nulo.");
            }

            var resp = await _http.PostAsJsonAsync("api/Personas", dto);
            if (resp.IsSuccessStatusCode)
            {
                var persona = await resp.Content.ReadFromJsonAsync<PersonaObtenerDTO>(_jsonOptions);
                return persona!;
            }

            var body = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error al crear persona (API): {resp.StatusCode} - {body}");
        }
    }
}
