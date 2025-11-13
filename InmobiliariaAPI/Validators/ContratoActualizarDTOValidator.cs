using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class ContratoActualizarDTOValidator : AbstractValidator<ContratoActualizarDTO>
    {
        public ContratoActualizarDTOValidator()
        {
            RuleFor(x => x.ContratoId)
                .GreaterThan(0).WithMessage("El id del contrato es obligatorio y debe ser mayor que 0.");

            // Si se proveen fechas, asegurar coherencia
            When(x => x.FechaInicio.HasValue && x.FechaFin.HasValue, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.FechaFin.Value.Date >= x.FechaInicio.Value.Date)
                    .WithMessage("La fecha de fin debe ser igual o posterior a la fecha de inicio.");
            });

            When(x => x.MontoMensual.HasValue, () =>
            {
                RuleFor(x => x.MontoMensual.Value)
                    .GreaterThanOrEqualTo(0m).WithMessage("El monto mensual debe ser mayor o igual a 0.");
            });

            When(x => x.InmuebleId.HasValue, () =>
            {
                RuleFor(x => x.InmuebleId.Value)
                    .GreaterThan(0).WithMessage("El id del inmueble debe ser mayor que 0.");
            });

            When(x => x.InquilinoId.HasValue, () =>
            {
                RuleFor(x => x.InquilinoId.Value)
                    .GreaterThan(0).WithMessage("El id del inquilino debe ser mayor que 0.");
            });
        }
    }
}
