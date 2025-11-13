using FluentValidation;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private IRoleService _roleService;
        private readonly IValidator<RoleCrearDTO> _roleCrearValidator;
        private readonly IValidator<RoleActualizarDTO> _roleActualizarValidator;
        private readonly IValidator<PersonaRoleAsignarDTO> _asignarRolValidator;

        public RolesController(
            IRoleService roleService,
            IValidator<RoleCrearDTO> roleCrearValidator,
            IValidator<RoleActualizarDTO> roleActualizarValidator,
            IValidator<PersonaRoleAsignarDTO> asignarRolValidator
            )
        {
            _roleService = roleService;
            _roleCrearValidator = roleCrearValidator;
            _roleActualizarValidator = roleActualizarValidator;
            _asignarRolValidator = asignarRolValidator;
        }

        // POST: api/roles
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> CrearRol([FromBody] RoleCrearDTO dto)
        {
            var validation = await _roleCrearValidator.ValidateAsync(dto);
            if (!validation.IsValid) return BadRequest(validation.Errors);

            try
            {
                var creado = await _roleService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetRolPorId), new { id = creado.RolId }, creado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/roles
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        // GET: api/roles/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetRolPorId(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        // PUT: api/roles/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> ActualizarRol(int id, [FromBody] RoleActualizarDTO dto)
        {
            var validation = await _roleActualizarValidator.ValidateAsync(dto);
            if (!validation.IsValid) return BadRequest(validation.Errors);

            if (dto.RolId != 0 && dto.RolId != id)
                return BadRequest(new { error = "El id del DTO no coincide con el id de la ruta." });

            try
            {
                var actualizado = await _roleService.UpdateAsync(id, dto);
                if (actualizado == null) return NotFound();
                return Ok(actualizado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/roles/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> EliminarRol(int id)
        {
            try
            {
                var eliminado = await _roleService.DeleteAsync(id, false);
                if (eliminado == null) return NotFound();
                return Ok(eliminado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/roles/existe/{id}
        [HttpGet("existe/{id:int}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> ExisteRol(int id)
        {
            var existe = await _roleService.ExistsAsync(id);
            if (existe == null) return NotFound();
            return Ok(existe);
        }

        // POST: api/roles/asignar
        [HttpPost("asignar")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> AsignarRol([FromBody] PersonaRoleAsignarDTO dto)
        {
            // FluentValidation
            var validationResult = await _asignarRolValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new { error = messages });
            }

            try
            {
                var resultado = await _roleService.AssignRoleAsync(dto.PersonaId, dto.RolId);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno al asignar rol." });
            }

        }

    }
}
