using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
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
            try
            {
                var auth = await _usuarioService.AuthenticateAsync(dto);
                return Ok(auth);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UsuarioCrearDTO dto)
        {
            try
            {
                var creado = await _usuarioService.CreateAsync(dto);
                var location = $"/api/usuarios/{creado.UsuarioId}"; 
                return Created(location, creado); // Devolver 201 Created con Location
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
