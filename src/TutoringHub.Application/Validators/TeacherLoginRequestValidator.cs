using FluentValidation;
using TutoringHub.Application.DTOs.Auth;

namespace TutoringHub.Application.Validators;

public class TeacherLoginRequestValidator : AbstractValidator<TeacherLoginRequest>
{
    public TeacherLoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}