using FluentValidation;
using TutoringHub.Application.DTOs.Attendance;

namespace TutoringHub.Application.Validators;

public class TickAttendanceRequestValidator : AbstractValidator<TickAttendanceRequest>
{
    public TickAttendanceRequestValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
    }
}