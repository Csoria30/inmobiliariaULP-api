using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class UsuarioCrearDTOValidator : AbstractValidator<UsuarioCrearDTO>
    {
        public UsuarioCrearDTOValidator()
        {
            RuleFor(x => x.PersonaId)
                .GreaterThan(0).WithMessage("El id de la persona es obligatorio.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .MaximumLength(128).WithMessage("La contraseña no puede exceder 128 caracteres.");

        }
    }
}
