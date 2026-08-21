using FluentValidation;
using TutoringHub.Application.DTOs.Quotas;

namespace TutoringHub.Application.Validators;

public class CreateQuotaRequestValidator : AbstractValidator<CreateQuotaRequest>
{
    public CreateQuotaRequestValidator()
    {
        RuleFor(x => x.ClassGroupId).GreaterThan(0);
        RuleFor(x => x.TotalSessions).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PeriodStart).NotEmpty().WithMessage("Period start is required.");
        RuleFor(x => x.PeriodEnd).NotEmpty().WithMessage("Period end is required.")
            .GreaterThanOrEqualTo(x => x.PeriodStart).WithMessage("Period end must not be before period start.");
    }
}