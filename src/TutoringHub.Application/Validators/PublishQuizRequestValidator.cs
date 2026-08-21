using FluentValidation;
using TutoringHub.Application.DTOs.Quizzes;

namespace TutoringHub.Application.Validators;

public class PublishQuizRequestValidator : AbstractValidator<PublishQuizRequest>
{
    public PublishQuizRequestValidator()
    {
        RuleFor(x => x.ClassGroupIds).NotEmpty().WithMessage("At least one class is required.");
    }
}