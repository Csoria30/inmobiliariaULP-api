using InmobiliariaDTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net.Http.Json;
using InmobiliariaDTO.Models.DTO;
using System.IdentityModel.Tokens.Jwt;


namespace InmobiliariaMVC.Controllers
{
    public class AccountController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        //GET: Account/Login
        [HttpGet]
        public IActionResult Login() => View(new UsuarioLoginDTO());


        //POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UsuarioLoginDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var resp = await client.PostAsJsonAsync("auth/login", model);

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas o error del servidor.");
                return View(model);
            }


            var apiResp = await resp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDTO>>(new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var token = apiResp?.Result?.Token;
            if (string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError(string.Empty, "No se recibió token.");
                return View(model);
            }

            
            // Guardar token en Session para TokenHandler
            HttpContext.Session.SetString("ApiToken", token);

            // Decodificar JWT y construir claims para la cookie
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claims = jwt.Claims.ToList();

            // Asegurar identificador y email
            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier) && apiResp.Result != null)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, apiResp.Result.UsuarioId.ToString()));

            if (!claims.Any(c => c.Type == ClaimTypes.Email) && apiResp.Result != null)
                claims.Add(new Claim(ClaimTypes.Email, apiResp.Result.Email ?? string.Empty));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Persona");
        }


        //POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("ApiToken");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

    }
}
