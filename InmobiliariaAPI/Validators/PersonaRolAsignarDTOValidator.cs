using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class PersonaRolAsignarDTOValidator : AbstractValidator<PersonaRoleAsignarDTO>
    {
        public PersonaRolAsignarDTOValidator()
        {
            RuleFor(x => x.PersonaId)
                .GreaterThan(0).WithMessage("El ID de la persona debe ser mayor que cero.");
            RuleFor(x => x.RolId)
                .GreaterThan(0).WithMessage("El ID del rol debe ser mayor que cero.");
        }
    }
}
