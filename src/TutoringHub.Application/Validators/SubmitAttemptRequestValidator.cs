using FluentValidation;
using TutoringHub.Application.DTOs.Quizzes;

namespace TutoringHub.Application.Validators;

public class SubmitAttemptRequestValidator : AbstractValidator<SubmitAttemptRequest>
{
    public SubmitAttemptRequestValidator()
    {
        RuleFor(x => x.Answers).NotEmpty().WithMessage("Answers are required.");
    }
}