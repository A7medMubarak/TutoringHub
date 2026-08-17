using FluentValidation;
using TutoringHub.Application.DTOs.Auth;

namespace TutoringHub.Application.Validators;

public class StudentLoginRequestValidator : AbstractValidator<StudentLoginRequest>
{
    public StudentLoginRequestValidator()
    {
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20)
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Phone must be 10-15 digits, optionally prefixed with +.");
        RuleFor(x => x.Pin).NotEmpty().Matches(@"^\d{4}$").WithMessage("PIN must be exactly 4 digits.");
    }
}