using FluentValidation;
using InmobiliariaAPI.Data;
using InmobiliariaAPI.Helpers;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Services;
using InmobiliariaAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContratosController : ApiControllerBase
    {

        private readonly IContratoService _contratoService;
        private readonly IUsuarioService _usuarioService;
        private readonly IValidator<ContratoActualizarDTO> _actualizarValidator;
        private readonly IValidator<ContratoCrearDTO> _crearValidator;

        public ContratosController(
            IContratoService contratoService, 
            IUsuarioService usuarioService,
            IValidator<ContratoCrearDTO> crearValidator,
            IValidator<ContratoActualizarDTO> actualizarValidator
            )
        {
            _contratoService = contratoService;
            _usuarioService = usuarioService;
            _crearValidator = crearValidator;
            _actualizarValidator = actualizarValidator;
        }


        // POST: api/contratos
        [HttpPost]
        [Authorize(Roles = "EMPLEADO,ADMINISTRADOR")]
        public async Task<IActionResult> CrearContrato([FromBody] ContratoCrearDTO dto)
        {
            // FluentValidation
            var validationResult = await _crearValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var messages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new InvalidOperationException(messages);
            }

            var creado = await _contratoService.CreateAsync(dto);
            var location = $"/api/contratos/{creado.ContratoId}";
            return ApiCreated(location, creado);

        }


    }
}
