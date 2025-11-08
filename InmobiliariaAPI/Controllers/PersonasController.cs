using FluentValidation;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonasController : ControllerBase
    {
        private IPersonaService _personaService;
        private readonly IValidator<PersonaCrearDTO> _personaCrearDTOValidacion;
        private readonly IValidator<PersonaActualizarDTO> _personaActualizarDTOValidacion;

        public PersonasController(
            IPersonaService personaService,
            IValidator<PersonaCrearDTO> personaCrearDTOValidacion,
            IValidator<PersonaActualizarDTO> personaActualizarDTOValidacion

            )
        {
            _personaService = personaService;
            _personaCrearDTOValidacion = personaCrearDTOValidacion;
            _personaActualizarDTOValidacion = personaActualizarDTOValidacion;
        }

        [HttpPost]
        public async Task<IActionResult> CrearPersona([FromBody] PersonaCrearDTO personaCrearDTO)
        {
            var validationResult = await _personaCrearDTOValidacion.ValidateAsync(personaCrearDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var nuevaPersona = await _personaService.CreateAsync(personaCrearDTO);
            return CreatedAtAction(nameof(GetPersonaPorId), new { id = nuevaPersona.PersonaId }, nuevaPersona);
        }

        // GET: api/personas
        [HttpGet]
        public async Task<IActionResult> GetAllPersonas()
        {
            var personas = await _personaService.GetAllAsync();
            return Ok(personas);
        }

        // GET: api/personas/{id}
        [HttpGet("{personaId}")]
        public async Task<IActionResult> GetPersonaPorId(int personaId)
        {
            var persona = await _personaService.GetByIdAsync(personaId);
            if (persona == null)
                return NotFound();

            return Ok(persona);
        }

        // PUT: api/personas/{id}
        [HttpPut("{personaId}")]
        public async Task<IActionResult> ActualizarPersona(int personaId, [FromBody] PersonaActualizarDTO personaActualizarDTO)
        {
            var validationResult = await _personaActualizarDTOValidacion.ValidateAsync(personaActualizarDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var personaActualizada = await _personaService.UpdateAsync(personaId, personaActualizarDTO);
            if (personaActualizada == null)
                return NotFound();

            return Ok(personaActualizada);
        }

        // DELETE: api/personas/{id}
        [HttpDelete("{personaId}")]
        public async Task<IActionResult> EliminarPersona(int personaId)
        {
            var personaEliminada = await _personaService.DeleteAsync(personaId, false);
            if (personaEliminada == null)
                return BadRequest();

            return Ok(personaEliminada);
        }

        // PUT: api/personas/{id}/habilitar
        [HttpPut("habilitar/{personaId}")]
        public async Task<IActionResult> HabilitarPersona(int personaId)
        {
            var personaHabilitada = await _personaService.DeleteAsync(personaId, true);
            if (personaHabilitada == null)
                return NotFound();

            return Ok(personaHabilitada);
        }

        // GET: api/personas/existe/{id}
        [HttpGet("existe/{personaId}")]
        public async Task<IActionResult> ExistePersona(int personaId)
        {
            var persona = await _personaService.ExistsAsync(personaId);
            if (persona == null)
                return NotFound();

            return Ok(persona);
        }

        // GET: api/personas/dni/{dni}
        [HttpGet("dni/{dni}")]
        public async Task<IActionResult> BuscarPorDni(string dni)
        {
            var persona = await _personaService.GetByDniAsync(dni);
            if (persona == null) return NotFound();
            return Ok(persona);
        }

        // GET: api/personas/email/{email}
        [HttpGet("email/{email}")]
        public async Task<IActionResult> BuscarPorEmail(string email)
        {
            var persona = await _personaService.GetByEmailAsync(email);
            if (persona == null) return NotFound();
            return Ok(persona);
        }



    }
}
