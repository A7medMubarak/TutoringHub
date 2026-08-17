using FluentValidation;
using TutoringHub.Application.DTOs.Centers;

namespace TutoringHub.Application.Validators;

public class CreateCenterRequestValidator : AbstractValidator<CreateCenterRequest>
{
    public CreateCenterRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LocationDetails).MaximumLength(300);
    }
}