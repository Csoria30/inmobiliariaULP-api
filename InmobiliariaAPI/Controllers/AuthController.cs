using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ApiControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public AuthController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDTO dto)
        {
            var auth = await _usuarioService.AuthenticateAsync(dto);
            return ApiOk(auth);
        }

        [HttpPost("register")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Register([FromBody] UsuarioCrearDTO dto)
        {
            var creado = await _usuarioService.CreateAsync(dto);
            var location = $"/api/usuarios/{creado.UsuarioId}";
            return ApiCreated(location, creado); 
        }
    }
}
