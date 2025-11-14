using FluentValidation;
using FluentValidation.Results;
using InmobiliariaAPI.Helpers;
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
    [Authorize]
    public class UsuariosController : ApiControllerBase
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
            var persona = await _personaService.GetByIdAsync(personaId.Value);
            var usuario = await _usuarioService.GetByPersonaIdAsync(personaId.Value);

            return ApiOk(new { persona, usuario });
        }

        // PUT: /api/usuarios/miPerfil  
        [HttpPut("miPerfil")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] PersonaActualizarDTO dto)
        {
            ValidationResult validationResult = await _personaActualizarValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ApiError(HttpStatusCode.BadRequest, "Validación inválida", messages);
            }

            var personaId = User.GetPersonaId();

            var actualizado = await _personaService.UpdateAsync(personaId.Value, dto);
            return ApiOk(actualizado);
        }

        // PUT: /api/usuarios/miPerfil/password
        [HttpPut("miPerfil/password")]
        public async Task<IActionResult> ChangePassword([FromBody] CambiarPasswordDTO dto)
        {
            ValidationResult validationResult = await _passwordValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ApiError(HttpStatusCode.BadRequest, "Validación inválida", messages);
            }

            var personaId = User.GetPersonaId();
            var resultado = await _usuarioService.ChangePasswordAsync(personaId.Value, dto.CurrentPassword, dto.NewPassword);
            return ApiOk(new { message = "Contraseña actualizada." });
        }

        // PATCH: /api/usuarios/miPerfil/avatar  (multipart/form-data) 
        [HttpPatch("miPerfil/avatar")]
        public async Task<IActionResult> ActualizarAvatar([FromForm] AvatarUploadDTO dto)
        {
            ValidationResult validationResult = await _avatarValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ApiError(HttpStatusCode.BadRequest, "Validación inválida", messages);
            }

            var personaId = User.GetPersonaId();

            var avatar = dto.Avatar;

            // Construir ruta  - _env.WebRootPath apunta a wwwroot
            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatars");
            Directory.CreateDirectory(uploadsRoot);
            var ext = Path.GetExtension(avatar.FileName); // Obtener la extensión
            var fileName = $"avatar_{personaId.Value}{ext}"; // Generar un nombre por persona (sobrescribe avatar)
            
            var filePath = Path.Combine(uploadsRoot, fileName); // Ruta completa 

            // Crear/abrir archivo 
            using (var stream = System.IO.File.Create(filePath))
            {
                await avatar.CopyToAsync(stream);
            }

            // Construir la URL relativa que se guardará en la base de datos
            var relativeUrl = $"/uploads/avatars/{fileName}";

            // Actualizar en la BD el campo Avatar del usuario asociado a personaId
            var usuarioDto = await _usuarioService.UpdateAvatarAsync(personaId.Value, relativeUrl);

            return Ok(usuarioDto);
        }

    }
}
