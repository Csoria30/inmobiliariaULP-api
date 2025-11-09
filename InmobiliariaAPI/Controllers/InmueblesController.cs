using FluentValidation;
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
    public class InmueblesController : ControllerBase
    {
        private IInmuebleService _inmuebleService;
        private readonly IValidator<InmuebleCrearDTO> _inmuebleCrearDTOValidacion;
        private readonly IValidator<InmuebleActualizarDTO> _inmuebleActualizarDTOValidacion;
        private readonly IWebHostEnvironment _env;

        public InmueblesController(
            IInmuebleService inmuebleService,
            IValidator<InmuebleCrearDTO> inmuebleCrearDTOValidacion,
            IValidator<InmuebleActualizarDTO> inmuebleActualizarDTOValidacion,
            IWebHostEnvironment env
            )
        {
            _inmuebleService = inmuebleService;
            _inmuebleCrearDTOValidacion = inmuebleCrearDTOValidacion;
            _inmuebleActualizarDTOValidacion = inmuebleActualizarDTOValidacion;
            _env = env;
        }

        // POST: api/inmuebles
        [HttpPost]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> CrearInmueble([FromForm] InmuebleCrearDTO inmuebleCrearDTO, [FromForm] IFormFile? foto)
        {
            // Obtener personaId desde el token (claim "personaId")
            var personaId = User.GetPersonaId();
            if (personaId == null)
                return Unauthorized(new { error = "No se pudo identificar al propietario desde el token." });

            // Forzar propietario desde token: ignorar cualquier PropietarioId enviado por el cliente
            inmuebleCrearDTO.PropietarioId = personaId.Value;

            // FluentValidation
            var validationResult = await _inmuebleCrearDTOValidacion.ValidateAsync(inmuebleCrearDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            // Envio foto, wwwroot/uploads/inmuebles/
            if (foto != null && foto.Length > 0)
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "inmuebles");
                Directory.CreateDirectory(uploadsRoot);

                var ext = Path.GetExtension(foto.FileName);
                var fileName = $"inmueble_{personaId.Value}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await foto.CopyToAsync(stream);
                }

                // Si tu DTO/entidad tiene un campo para imagen, asignarlo.
                // ejemplo: inmuebleCrearDTO.FotoUrl = $"/uploads/inmuebles/{fileName}";
                // si no existe, lo puedes omitir o gestionar desde el servicio.

            }

            try
            {
                var nuevoInmueble = await _inmuebleService.CreateAsync(inmuebleCrearDTO);
                return CreatedAtAction(nameof(GetInmueblePorId), new { id = nuevoInmueble.InmuebleId }, nuevoInmueble);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/inmuebles
        [HttpGet]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> GetAllInmuebles()
        {
            var inmuebles = await _inmuebleService.GetAllAsync();
            return Ok(inmuebles);
        }

        // GET: api/inmueblesUsuario
        [HttpGet]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> GetAllInmueblesUsuario()
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            var inmuebles = await _inmuebleService.GetAllAsync();
            // Filtrar inmuebles
            var propios = inmuebles.Where(i => i.PropietarioId == personaId.Value).ToList();
            return Ok(propios);
        }

        // GET: api/inmuebles/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> GetInmueblePorId(int id)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            var inmueble = await _inmuebleService.GetByIdAsync(id);
            if (inmueble == null) return NotFound();

            // validar pertenencia
            if (inmueble.PropietarioId != personaId.Value) return Forbid();

            return Ok(inmueble);
        }

        // PUT: api/inmuebles/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> ActualizarInmueble(int id, [FromBody] InmuebleActualizarDTO inmuebleActualizarDTO)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            var validationResult = await _inmuebleActualizarDTOValidacion.ValidateAsync(inmuebleActualizarDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            // Coherencia entre id de ruta y DTO
            if (inmuebleActualizarDTO.InmuebleId != 0 && inmuebleActualizarDTO.InmuebleId != id)
                return BadRequest(new { error = "El id del DTO no coincide con el id de la ruta." });

            // Validar que el inmueble pertenece al propietario antes de actualizar
            var existente = await _inmuebleService.GetByIdAsync(id);
            if (existente == null) return NotFound();
            if (existente.PropietarioId != personaId.Value) return Forbid();

            try
            {
                var actualizado = await _inmuebleService.UpdateAsync(id, inmuebleActualizarDTO);
                if (actualizado == null) return NotFound();
                return Ok(actualizado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/inmuebles/{id} -> desactivar
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> EliminarInmueble(int id)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            var existente = await _inmuebleService.GetByIdAsync(id);
            if (existente == null) return NotFound();
            if (existente.PropietarioId != personaId.Value) return Forbid();

            try
            {
                var eliminado = await _inmuebleService.DeleteAsync(id, false);
                if (eliminado == null) return BadRequest();
                return Ok(eliminado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/inmuebles/habilitar/{id}
        [HttpPut("habilitar/{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> HabilitarInmueble(int id)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            var existente = await _inmuebleService.GetByIdAsync(id);
            if (existente == null) return NotFound();
            if (existente.PropietarioId != personaId.Value) return Forbid();

            try
            {
                var habilitado = await _inmuebleService.DeleteAsync(id, true);
                if (habilitado == null) return NotFound();
                return Ok(habilitado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/inmuebles/existe/{id}
        [HttpGet("existe/{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> ExisteInmueble(int id)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) return Unauthorized();

            var existe = await _inmuebleService.ExistsAsync(id);
            if (existe == null) return NotFound();

            // Validar que el inmueble pertenece al propietario
            if (existe.PropietarioId != personaId.Value) return Forbid();

            return Ok(existe);
        }




    }
}
