using FluentValidation;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonasController : ApiControllerBase
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
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> CrearPersona([FromBody] PersonaCrearDTO personaCrearDTO)
        {
            var validationResult = await _personaCrearDTOValidacion.ValidateAsync(personaCrearDTO);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ApiError(HttpStatusCode.BadRequest, "Validación inválida", messages);
            }

            var nuevaPersona = await _personaService.CreateAsync(personaCrearDTO);
            var location = $"/api/personas/{nuevaPersona.PersonaId}";
            return ApiCreated(location, nuevaPersona);
        }

        // GET: api/personas
        [HttpGet]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> GetAllPersonas([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var paged = await _personaService.GetAllPagedAsync(page, pageSize);
            return Ok(new { result = paged });
        }

        // GET: api/personas/{id}
        [HttpGet("{personaId:int}")]
        [Authorize(Policy = "PropietarioOrAdmin")]
        public async Task<IActionResult> GetPersonaPorId(int personaId)
        {
            var persona = await _personaService.GetByIdAsync(personaId);
            return ApiOk(persona);
        }

        // PUT: api/personas/{id}
        [HttpPut("{personaId:int}")]
        [Authorize(Policy = "PropietarioOrAdmin")]
        public async Task<IActionResult> ActualizarPersona(int personaId, [FromBody] PersonaActualizarDTO personaActualizarDTO)
        {
            var validationResult = await _personaActualizarDTOValidacion.ValidateAsync(personaActualizarDTO);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ApiError(HttpStatusCode.BadRequest, "Validación inválida", messages);
            }

            var personaActualizada = await _personaService.UpdateAsync(personaId, personaActualizarDTO);
            return ApiOk(personaActualizada);
        }

        // DELETE: api/personas/{id}
        [HttpDelete("{personaId:int}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> EliminarPersona(int personaId)
        {
            var personaEliminada = await _personaService.DeleteAsync(personaId, false);
            return ApiOk(personaEliminada);
        }

        // PUT: api/personas/{id}/habilitar
        [HttpPut("habilitar/{personaId:int}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> HabilitarPersona(int personaId)
        {
            var personaHabilitada = await _personaService.DeleteAsync(personaId, true);
            return ApiOk(personaHabilitada);
        }

        // GET: api/personas/existe/{id}
        [HttpGet("existe/{personaId:int}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> ExistePersona(int personaId)
        {
            var persona = await _personaService.ExistsAsync(personaId);
            return ApiOk(persona);
        }

        // GET: api/personas/dni/{dni}
        [HttpGet("dni/{dni:int}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> BuscarPorDni(string dni)
        {
            var persona = await _personaService.GetByDniAsync(dni);
            return ApiOk(persona);
        }

        // GET: api/personas/email/{email}
        [HttpGet("email/{email}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> BuscarPorEmail(string email)
        {
            var persona = await _personaService.GetByEmailAsync(email);
            return ApiOk(persona);
        }


        // POST: api/personas/datatable
        [HttpPost("datatable")]
        public async Task<IActionResult> DataTable()
        {
            var draw = int.TryParse(Request.Form["draw"].FirstOrDefault(), out var d) ? d : 0;
            var start = int.TryParse(Request.Form["start"].FirstOrDefault(), out var s) ? s : 0;
            var length = int.TryParse(Request.Form["length"].FirstOrDefault(), out var l) ? l : 10;
            var search = Request.Form["search[value]"].FirstOrDefault();

            int page = (length > 0) ? (start / length) + 1 : 1;
            var paged = await _personaService.GetAllPagedAsync(page, length, search);

            return Ok(new
            {
                draw,
                recordsTotal = paged.Total,
                recordsFiltered = paged.TotalFiltered,
                data = paged.Items
            });
        }
    }
}
