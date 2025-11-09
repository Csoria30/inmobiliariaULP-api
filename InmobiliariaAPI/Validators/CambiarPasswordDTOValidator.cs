using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class CambiarPasswordDTOValidator : AbstractValidator<CambiarPasswordDTO>
    {
        public CambiarPasswordDTOValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("La contraseña actual es obligatoria.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
                .MaximumLength(128).WithMessage("La nueva contraseña no puede exceder 128 caracteres.");
        }
    }
}
