using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class RoleActualizarDTOValidator : AbstractValidator<RoleActualizarDTO>
    {
        public RoleActualizarDTOValidator()
        {
            RuleFor(x => x.RolId)
                .GreaterThan(0).WithMessage("El id del rol es obligatorio y debe ser mayor que 0.");

            // Nombre es opcional en la actualización, pero si se provee debe ser válido
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre no puede quedar vacío cuando se proporciona.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.")
                .When(x => x.Nombre != null);

            RuleFor(x => x.Descripcion)
                .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.")
                .When(x => x.Descripcion != null);
        }
    }
}
