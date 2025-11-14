using FluentValidation;
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
    public class InmueblesController : ApiControllerBase
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CrearInmueble([FromForm] InmuebleCrearFormDTO formDto)
        {

            // Mapear a DTO de servicio
            var personaId = User.GetPersonaId();
            if (personaId == null)
                return ApiError(HttpStatusCode.Unauthorized, "No autorizado", "No se pudo identificar al propietario desde el token.");


            var crearDto = new InmuebleCrearDTO
            {
                Direccion = formDto.Direccion,
                Uso = formDto.Uso,
                Ambientes = formDto.Ambientes,
                Coordenadas = formDto.Coordenadas,
                PrecioBase = formDto.PrecioBase,
                TipoId = formDto.TipoId,
                PropietarioId = personaId.Value
            };

            // Validación con FluentValidation
            var validation = await _inmuebleCrearDTOValidacion.ValidateAsync(crearDto);
            if (!validation.IsValid)
            {
                var messages = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return ApiError(HttpStatusCode.BadRequest, "Validación inválida", messages);
            }

            // Guardar foto si viene en el formulario
            if (formDto.Foto != null && formDto.Foto.Length > 0)
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "inmuebles");
                Directory.CreateDirectory(uploadsRoot);

                var ext = Path.GetExtension(formDto.Foto.FileName);
                var fileName = $"inmueble_{personaId.Value}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await formDto.Foto.CopyToAsync(stream);
                }

                crearDto.Imagen = $"/uploads/inmuebles/{fileName}";
            }

            var creado = await _inmuebleService.CreateAsync(crearDto);
            var location = $"/api/inmuebles/{creado.InmuebleId}";
            return ApiCreated(location, creado);
        }

        // GET: api/inmuebles  -> devuelve todos los inmuebles 
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetAllInmuebles()
        {
            var inmuebles = await _inmuebleService.GetAllAsync();
            return ApiOk(inmuebles);
        }

        // GET: api/inmuebles/propios  -> devuelve solo los inmuebles del propietario autenticado
        [HttpGet("propios")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> GetAllInmueblesUsuario()
        {
            var personaId = User.GetPersonaId();
            if (personaId == null) 
                return ApiError(HttpStatusCode.Unauthorized, "No autorizado", "No se pudo identificar al usuario desde el token.");

            var propios = await _inmuebleService.GetByPropietarioAsync(personaId.Value);
            return ApiOk(propios);
        }

        // GET: api/inmuebles/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> GetInmueblePorId(int id)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null)
                return ApiError(HttpStatusCode.Unauthorized, "No autorizado", "No se pudo identificar al usuario desde el token.");

            var inmueble = await _inmuebleService.GetByIdAsync(id);
            return Ok(inmueble);
        }

        // PUT: api/inmuebles/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> ActualizarInmueble(int id, [FromBody] InmuebleActualizarDTO inmuebleActualizarDTO)
        {
            // Validación con FluentValidation
            var validation = await _inmuebleActualizarDTOValidacion.ValidateAsync(inmuebleActualizarDTO);
            if (!validation.IsValid)
            {
                var messages = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return ApiError(HttpStatusCode.BadRequest, "Validación inválida", messages);
            }

            var personaId = User.GetPersonaId();
            if (personaId == null)
                return ApiError(HttpStatusCode.Unauthorized, "No autorizado", "No se pudo identificar al usuario desde el token.");

            if (inmuebleActualizarDTO.InmuebleId != 0 && inmuebleActualizarDTO.InmuebleId != id)
                return ApiError(HttpStatusCode.BadRequest, "Id inválido", "El id del DTO no coincide con el id de la ruta.");

            var actualizado = await _inmuebleService.UpdateAsync(id, inmuebleActualizarDTO);
            return ApiOk(actualizado);
        }

        // DELETE: api/inmuebles/{id} -> desactivar
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> EliminarInmueble(int id)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null)
                return ApiError(HttpStatusCode.Unauthorized, "No autorizado", "No se pudo identificar al usuario desde el token.");

            var eliminado = await _inmuebleService.DeleteAsync(id, false);
            return ApiOk(eliminado);
        }

        // PUT: api/inmuebles/habilitar/{id}
        [HttpPut("habilitar/{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> HabilitarInmueble(int id)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null)
                return ApiError(HttpStatusCode.Unauthorized, "No autorizado", "No se pudo identificar al usuario desde el token.");

            var habilitado = await _inmuebleService.DeleteAsync(id, true);
            return ApiOk(habilitado);
        }

        // GET: api/inmuebles/existe/{id}
        [HttpGet("existe/{id:int}")]
        [Authorize(Roles = "PROPIETARIO,ADMINISTRADOR")]
        public async Task<IActionResult> ExisteInmueble(int id)
        {
            var personaId = User.GetPersonaId();
            if (personaId == null)
                return ApiError(HttpStatusCode.Unauthorized, "No autorizado", "No se pudo identificar al usuario desde el token.");

            var existe = await _inmuebleService.ExistsAsync(id);
            if (existe == null) 
                return ApiError(HttpStatusCode.NotFound, "No encontrado", "Inmueble no encontrado");

            return ApiOk(existe);
        }




    }
}
