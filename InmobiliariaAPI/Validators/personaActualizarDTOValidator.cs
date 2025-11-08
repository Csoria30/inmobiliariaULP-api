using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class PersonaActualizarDTOValidator : AbstractValidator<PersonaActualizarDTO>
    {
        public PersonaActualizarDTOValidator()
        {
            RuleFor(x => x.PersonaId)
                .GreaterThan(0).WithMessage("El ID de persona debe ser mayor a 0");

            RuleFor(x => x.Dni)
                .NotEmpty().WithMessage("El DNI es requerido")
                .Length(7, 8).WithMessage("El DNI debe tener entre 7 y 8 dígitos")
                .Matches(@"^\d+$").WithMessage("El DNI debe contener solo números");

            RuleFor(x => x.Apellido)
                .NotEmpty().WithMessage("El apellido es requerido")
                .MaximumLength(45).WithMessage("El apellido no puede exceder los 45 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El apellido solo puede contener letras");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(45).WithMessage("El nombre no puede exceder los 45 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras");

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .MaximumLength(45).WithMessage("El teléfono no puede exceder los 45 caracteres")
                .Matches(@"^[\d\+\-\s\(\)]+$").WithMessage("El teléfono contiene caracteres inválidos");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .MaximumLength(45).WithMessage("El email no puede exceder los 45 caracteres")
                .EmailAddress().WithMessage("El formato del email no es válido");
        }
    }
}
