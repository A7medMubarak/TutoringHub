using FluentValidation;
using TutoringHub.Application.DTOs.Enrollments;

namespace TutoringHub.Application.Validators;

public class EnrollStudentRequestValidator : AbstractValidator<EnrollStudentRequest>
{
    public EnrollStudentRequestValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
    }
}