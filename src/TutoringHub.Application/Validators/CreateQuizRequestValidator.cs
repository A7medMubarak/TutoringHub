using FluentValidation;
using TutoringHub.Application.DTOs.Quizzes;

namespace TutoringHub.Application.Validators;

public class CreateQuizRequestValidator : AbstractValidator<CreateQuizRequest>
{
    public CreateQuizRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Questions).NotEmpty().WithMessage("At least one question is required.");
        RuleFor(x => x.Questions).Must(q => q.Count <= 30).WithMessage("A quiz can have at most 30 questions.");
    }
}