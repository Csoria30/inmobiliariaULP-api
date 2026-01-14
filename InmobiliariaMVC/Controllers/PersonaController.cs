using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using InmobiliariaDTO;

namespace InmobiliariaMVC.Controllers
{
    public class PersonaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PersonaController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        } 


        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            using var resp = await client.GetAsync("personas");
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode);

            await using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            if (doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                var personas = resultElement.Deserialize<List<PersonaObtenerDTO>>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<PersonaObtenerDTO>();

                return View(personas);
            }

            return View();
        }
    }
}
