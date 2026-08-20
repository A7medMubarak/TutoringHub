using FluentValidation;
using TutoringHub.Application.DTOs.Ai;

namespace TutoringHub.Application.Validators;

public class GenerateQuizRequestValidator : AbstractValidator<GenerateQuizRequest>
{
    public GenerateQuizRequestValidator()
    {
        RuleFor(x => x.Topic).NotEmpty().MaximumLength(200);
        RuleFor(x => x.QuestionCount).InclusiveBetween(1, 30);
        RuleFor(x => x.QuestionType).IsInEnum();
    }
}