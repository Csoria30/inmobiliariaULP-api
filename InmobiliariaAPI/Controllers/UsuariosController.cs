using FluentValidation;
using FluentValidation.Results;
using InmobiliariaAPI.Helpers;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IPersonaService _personaService;
        private readonly IWebHostEnvironment _env;
        private readonly IValidator<PersonaActualizarDTO> _personaActualizarValidator;
        private readonly IValidator<CambiarPasswordDTO> _passwordValidator;
        private readonly IValidator<AvatarUploadDTO> _avatarValidator;

        public UsuariosController(
            IUsuarioService usuarioService,
            IPersonaService personaService,
            IWebHostEnvironment env,
            IValidator<PersonaActualizarDTO> personaActualizarValidator,
            IValidator<CambiarPasswordDTO> passwordValidator,
            IValidator<AvatarUploadDTO> avatarValidator
            
            )
        {
            _usuarioService = usuarioService;
            _personaService = personaService;
            _env = env;
            _personaActualizarValidator = personaActualizarValidator;
            _passwordValidator = passwordValidator;
            _avatarValidator = avatarValidator;
        }

        // GET: /api/usuarios/miPerfil
        [HttpGet("miPerfil")]
        public async Task<IActionResult> miPerfil()
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            var persona = await _personaService.GetByIdAsync(personaId.Value);
            var usuario = await _usuarioService.GetByPersonaIdAsync(personaId.Value);

            return Ok(new { persona, usuario });
        }

        // PUT: /api/usuarios/miPerfil  
        [HttpPut("miPerfil")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] PersonaActualizarDTO dto)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            ValidationResult vr = await _personaActualizarValidator.ValidateAsync(dto);
            if (!vr.IsValid) return BadRequest(vr.Errors);

            // No aceptar personaId del body: forzar el claim
            var actualizado = await _personaService.UpdateAsync(personaId.Value, dto);
            if (actualizado == null) return NotFound();
            return Ok(actualizado);
        }

        // PUT: /api/usuarios/miPerfil/password
        [HttpPut("miPerfil/password")]
        public async Task<IActionResult> ChangePassword([FromBody] CambiarPasswordDTO dto)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            ValidationResult vr = await _passwordValidator.ValidateAsync(dto);
            if (!vr.IsValid) return BadRequest(vr.Errors);

            var resultado = await _usuarioService.ChangePasswordAsync(personaId.Value, dto.CurrentPassword, dto.NewPassword);
            if (!resultado) return BadRequest(new { error = "Contraseña actual inválida o usuario no encontrado." });

            return Ok(new { message = "Contraseña actualizada." });
        }

        // PATCH: /api/usuarios/miPerfil/avatar  (multipart/form-data) 
        [HttpPatch("miPerfil/avatar")]
        public async Task<IActionResult> ActualizarAvatar([FromForm] AvatarUploadDTO dto)
        {
            // Obtener el personaId del token JWT (claim "personaId")
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            // FluentValidation
            ValidationResult resultado = await _avatarValidator.ValidateAsync(dto);
            if (!resultado.IsValid) return BadRequest(resultado.Errors);

            // Extraer el archivo validado del DTO
            var avatar = dto.Avatar;

            // Construir ruta wwwroot/uploads/avatars
            // _env.WebRootPath apunta a wwwroot
            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatars");
            Directory.CreateDirectory(uploadsRoot);

            // Obtener la extensión
            var ext = Path.GetExtension(avatar.FileName);
            // Generar un nombre por persona (sobrescribe avatar)
            var fileName = $"avatar_{personaId.Value}{ext}";
            // Ruta completa 
            var filePath = Path.Combine(uploadsRoot, fileName);

            // Crear/abrir archivo 
            using (var stream = System.IO.File.Create(filePath))
            {
                await avatar.CopyToAsync(stream);
            }

            // Construir la URL relativa que se guardará en la base de datos
            var relativeUrl = $"/uploads/avatars/{fileName}";

            // Actualizar en la BD el campo Avatar del usuario asociado a personaId
            var usuarioDto = await _usuarioService.UpdateAvatarAsync(personaId.Value, relativeUrl);
            if (usuarioDto == null) return NotFound();

            return Ok(usuarioDto);
        }

    }
}
