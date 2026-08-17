using FluentValidation;
using TutoringHub.Application.DTOs.Auth;

namespace TutoringHub.Application.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}