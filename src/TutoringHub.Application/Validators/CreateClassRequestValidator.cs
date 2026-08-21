using FluentValidation;
using TutoringHub.Application.DTOs.Classes;

namespace TutoringHub.Application.Validators;

public class CreateClassRequestValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CenterId).GreaterThan(0);
        RuleFor(x => x.DayOfWeek).IsInEnum();
        RuleFor(x => x.StartTime).GreaterThanOrEqualTo(TimeSpan.Zero)
            .LessThan(TimeSpan.FromHours(24));
        RuleFor(x => x.Frequency).IsInEnum();
    }
}