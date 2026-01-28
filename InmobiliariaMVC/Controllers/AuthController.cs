using InmobiliariaDTO;
using InmobiliariaDTO.Models.DTO;
using InmobiliariaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;


namespace InmobiliariaMVC.Controllers
{
    public class AuthController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthService _authService;
        public AuthController(IHttpClientFactory httpClientFactory, IAuthService authService)
        {
            _httpClientFactory = httpClientFactory;
            _authService = authService;
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

            var (success, error, claims) = await _authService.AuthenticateAsync(model);
            if (!success)
            {
                ViewBag.Error = error ?? "Error de autenticación";
                return View(model);
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }


        //POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Limpia token/estado local
            HttpContext.Session.Remove("ApiToken");
            HttpContext.Session.Clear();

            // Si tu servicio necesita una llamada para invalidar token en API remota:
            if (_authService != null)
            {
                await _authService.LogoutAsync();
            }

            // Desloguea la cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

    }
}
