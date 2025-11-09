using FluentValidation;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Validators
{
    public class AvatarUploadDTOValidator : AbstractValidator<AvatarUploadDTO>
    {
        private readonly string[] _allowedExt = new[] { ".jpg", ".jpeg", ".png" };
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        public AvatarUploadDTOValidator()
        {
            RuleFor(x => x.Avatar)
                .NotNull().WithMessage("El archivo avatar es obligatorio.")
                .Must(f => f.Length > 0).WithMessage("El archivo está vacío.")
                .Must(f => _allowedExt.Contains(Path.GetExtension(f.FileName).ToLower()))
                    .WithMessage("Formato inválido. Solo se permiten: .jpg, .jpeg, .png.")
                .Must(f => f.Length <= MaxBytes)
                    .WithMessage("El archivo excede el tamaño máximo permitido (5 MB).");
        }
    }
}
