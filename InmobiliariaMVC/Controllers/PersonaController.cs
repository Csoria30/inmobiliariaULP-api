using InmobiliariaAPI.Models.DTO;
using InmobiliariaDTO;
using InmobiliariaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace InmobiliariaMVC.Controllers
{
    [Authorize]
    public class PersonaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IPersonaService _personaService;
        private readonly IRoleService _roleService;

        public PersonaController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            IPersonaService personaService,
            IRoleService roleService
            )
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _personaService = personaService;
            _roleService = roleService;
        }

        //public async Task<IActionResult> Index()
        //{
        //    var client = _httpClientFactory.CreateClient("ApiClient");

        //    using var resp = await client.GetAsync("personas");
        //    if (!resp.IsSuccessStatusCode)
        //        return StatusCode((int)resp.StatusCode);

        //    await using var stream = await resp.Content.ReadAsStreamAsync();
        //    using var doc = await JsonDocument.ParseAsync(stream);

        //    if (doc.RootElement.TryGetProperty("result", out var resultElement))
        //    {
        //        var personas = resultElement.Deserialize<List<PersonaObtenerDTO>>(new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        }) ?? new List<PersonaObtenerDTO>();

        //        return View(personas);
        //    }

        //    return View();
        //}

        public async Task<IActionResult> Index(int page = 1, int pageSize = 2, string? search = null)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Construir query incluyendo search si existe
            var url = $"personas?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";

            using var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode);

            await using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            ViewBag.Search = search; // para que la vista mantenga el valor del input

            if (!doc.RootElement.TryGetProperty("result", out var resultElement))
                return View(new List<PersonaObtenerDTO>());

            // Caso esperado: result es un objeto paginado con "items"
            if (resultElement.ValueKind == JsonValueKind.Object && resultElement.TryGetProperty("items", out _))
            {
                var paged = resultElement.Deserialize<PagedResult<PersonaObtenerDTO>>(options)
                            ?? new PagedResult<PersonaObtenerDTO> { Items = new List<PersonaObtenerDTO>(), Page = page, PageSize = pageSize, Total = 0, TotalPages = 1 };

                ViewBag.Page = paged.Page;
                ViewBag.PageSize = paged.PageSize;
                ViewBag.Total = paged.Total;
                ViewBag.TotalPages = paged.TotalPages;

                return View(paged.Items ?? new List<PersonaObtenerDTO>());
            }

            return View(new List<PersonaObtenerDTO>());
        }

        // GET: PersonaController/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Usar el servicio de roles centralizado
            var roles = await _roleService.GetAllAsync() ?? new List<RoleObtenerDTO>();
            ViewBag.Roles = roles;

            // DTO inicial para evitar NRE en la vista
            var model = new PersonaCrearDTO
            {
                IdRoles = new List<int>()
            };

            return View(model);

        }

        //! POST: PersonaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Buena práctica para prevenir ataques CSRF
        public async Task<IActionResult> Create(PersonaCrearDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(dto);

                // Validar que se haya seleccionado al menos un rol
                if (dto.IdRoles == null || dto.IdRoles.Count == 0)
                {
                    ModelState.AddModelError(nameof(dto.IdRoles), "Seleccione al menos un rol.");
                    
                    // repoblar ViewBag/Model de roles si usas la vista que los muestra
                    ViewBag.Roles = await _roleService.GetAllAsync();
                    return View(dto);
                }

                var personaCreada = await _personaService.CreateAsync(dto);

                TempData["Notificacion"] = "Persona creada correctamente.";
                TempData["NotificacionTipo"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) // errores esperados del API (duplicados, rol inexistente, etc.)
            {
                TempData["Notificacion"] = ex.Message;
                TempData["NotificacionTipo"] = "danger";
                return View(dto);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error interno al crear la persona.";
                return View("Error");
            }
        }

    
    }
}
