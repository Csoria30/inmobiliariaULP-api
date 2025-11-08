using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class RoleCrearDTOValidator : AbstractValidator<RoleCrearDTO>
    {
        public RoleCrearDTOValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

            RuleFor(x => x.Descripcion)
                .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
        }
    }
}
