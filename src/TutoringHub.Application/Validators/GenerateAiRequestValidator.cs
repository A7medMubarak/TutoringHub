using FluentValidation;
using TutoringHub.Application.DTOs.Ai;

namespace TutoringHub.Application.Validators;

public class GenerateAiRequestValidator : AbstractValidator<GenerateAiRequest>
{
    public GenerateAiRequestValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().MaximumLength(2000);
    }
}