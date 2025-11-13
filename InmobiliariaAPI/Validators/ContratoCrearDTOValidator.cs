using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class ContratoCrearDTOValidator : AbstractValidator<ContratoCrearDTO>
    {
        public ContratoCrearDTOValidator()
        {
            RuleFor(x => x.InmuebleId)
                .GreaterThan(0).WithMessage("El id del inmueble es obligatorio y debe ser mayor que 0.");

            RuleFor(x => x.InquilinoId)
                .GreaterThan(0).WithMessage("El id del inquilino es obligatorio y debe ser mayor que 0.");

            RuleFor(x => x.FechaInicio)
                .NotEmpty().WithMessage("La fecha de inicio es obligatoria.");

            RuleFor(x => x.FechaFin)
                .NotEmpty().WithMessage("La fecha de fin es obligatoria.");

            // FechaFin >= FechaInicio
            RuleFor(x => x)
                .Must(x => x.FechaFin.Date >= x.FechaInicio.Date)
                .WithMessage("La fecha de fin debe ser igual o posterior a la fecha de inicio.");

            RuleFor(x => x.MontoMensual)
                .GreaterThanOrEqualTo(0m).WithMessage("El monto mensual debe ser mayor o igual a 0.");

            // UsuarioId puede venir desde el controlador para auditoría; si viene validar no negativo
            RuleFor(x => x.UsuarioId)
                .GreaterThanOrEqualTo(0).WithMessage("UsuarioId no puede ser negativo.")
                .When(x => x.UsuarioId != 0);
        }
    }
}
