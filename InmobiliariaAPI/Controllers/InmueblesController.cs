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

        public InmueblesController(
            IInmuebleService inmuebleService,
            IValidator<InmuebleCrearDTO> inmuebleCrearDTOValidacion,
            IValidator<InmuebleActualizarDTO> inmuebleActualizarDTOValidacion
            )
        {
            _inmuebleService = inmuebleService;
            _inmuebleCrearDTOValidacion = inmuebleCrearDTOValidacion;
            _inmuebleActualizarDTOValidacion = inmuebleActualizarDTOValidacion;
        }

        // POST: api/inmuebles
        [HttpPost]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> CrearInmueble([FromBody] InmuebleCrearDTO inmuebleCrearDTO)
        {
            // Obtener personaId desde el token (claim "personaId")
            var personaId = User.GetPersonaId();
            if (personaId == null)
                return Unauthorized(new { error = "No se pudo identificar al propietario desde el token." });

            // Forzar propietario desde token: ignorar cualquier PropietarioId enviado por el cliente
            inmuebleCrearDTO.PropietarioId = personaId.Value;

            var validationResult = await _inmuebleCrearDTOValidacion.ValidateAsync(inmuebleCrearDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

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

        // GET: api/inmuebles/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> GetInmueblePorId(int id)
        {
            var inmueble = await _inmuebleService.GetByIdAsync(id);
            if (inmueble == null) return NotFound();
            return Ok(inmueble);
        }

        // PUT: api/inmuebles/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> ActualizarInmueble(int id, [FromBody] InmuebleActualizarDTO inmuebleActualizarDTO)
        {
            var validationResult = await _inmuebleActualizarDTOValidacion.ValidateAsync(inmuebleActualizarDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            // Coherencia entre id de ruta y DTO
            if (inmuebleActualizarDTO.InmuebleId != 0 && inmuebleActualizarDTO.InmuebleId != id)
                return BadRequest(new { error = "El id del DTO no coincide con el id de la ruta." });

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

        // DELETE: api/inmuebles/{id}
        // Desactiva el inmueble (estado = false)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> EliminarInmueble(int id)
        {
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
            var existe = await _inmuebleService.ExistsAsync(id);
            if (existe == null) return NotFound();
            return Ok(existe);
        }




    }
}
