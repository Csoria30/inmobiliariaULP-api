using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class personaCrearDTOValidator : AbstractValidator<PersonaCrearDTO>
    {
        public personaCrearDTOValidator()
        {
            RuleFor(x => x.Dni)
                .NotEmpty().WithMessage("El DNI es obligatorio.")
                .Matches(@"^\d{6,20}$").WithMessage("El DNI debe contener solo dígitos (6-20 caracteres).");

            RuleFor(x => x.Apellido)
                .NotEmpty().WithMessage("El apellido es obligatorio.")
                .MaximumLength(45).WithMessage("El apellido no puede exceder 45 caracteres.");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(45).WithMessage("El nombre no puede exceder 45 caracteres.");

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es obligatorio.")
                .MinimumLength(8).WithMessage("El teléfono debe tener al menos 8 dígitos.")
                .MaximumLength(20).WithMessage("El teléfono no puede exceder 45 caracteres.")
                .Matches(@"^[1-9][0-9]{7,44}$").WithMessage("El teléfono debe tener solo números, mínimo 8 dígitos, y no comenzar con cero.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .MaximumLength(45).WithMessage("El email no puede exceder 45 caracteres.")
                .EmailAddress().WithMessage("El email no tiene un formato válido.");
        }
    }
}
