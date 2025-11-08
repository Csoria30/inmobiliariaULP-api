using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class InmuebleCrearDTOValidator : AbstractValidator<InmuebleCrearDTO>
    {
        public InmuebleCrearDTOValidator()
        {
            RuleFor(x => x.Direccion)
                .NotEmpty().WithMessage("La dirección es obligatoria.")
                .MaximumLength(250).WithMessage("La dirección no puede exceder 250 caracteres.");

            RuleFor(x => x.Uso)
                .NotEmpty().WithMessage("El uso es obligatorio.")
                .Must(u => string.Equals(u, "comercial", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(u, "residencial", StringComparison.OrdinalIgnoreCase))
                .WithMessage("El uso debe ser 'comercial' o 'residencial'.");

            RuleFor(x => x.Ambientes)
                .GreaterThanOrEqualTo(0).WithMessage("Los ambientes debe ser mayor o igual a 0.")
                .LessThanOrEqualTo(100).WithMessage("Los ambientes no pueden exceder 100.");

            RuleFor(x => x.Coordenadas)
                .MaximumLength(200).WithMessage("Las coordenadas no pueden exceder 200 caracteres.")
                .Matches(@"^\s*-?\d+(\.\d+)?\s*,\s*-?\d+(\.\d+)?\s*$")
                .When(x => !string.IsNullOrWhiteSpace(x.Coordenadas))
                .WithMessage("Las coordenadas deben tener el formato 'latitud,longitud' (ej. -34.6037,-58.3816).");

            RuleFor(x => x.PrecioBase)
                .GreaterThanOrEqualTo(0m).WithMessage("El precio base debe ser mayor o igual a 0.");

            RuleFor(x => x.PropietarioId)
                .GreaterThan(0).WithMessage("El id del propietario es obligatorio y debe ser mayor que 0.");

            RuleFor(x => x.TipoId)
                .GreaterThan(0).WithMessage("El id del tipo de inmueble es obligatorio y debe ser mayor que 0.");
        }
    }
}
