using FluentAssertions;
using FluentValidation.TestHelper;
using TutoringHub.Application.DTOs.Auth;
using TutoringHub.Application.DTOs.Attendance;
using TutoringHub.Application.DTOs.Centers;
using TutoringHub.Application.DTOs.Classes;
using TutoringHub.Application.DTOs.Students;
using TutoringHub.Application.DTOs.Enrollments;
using TutoringHub.Application.DTOs.Quotas;
using TutoringHub.Application.Validators;
using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.Tests.Validators;

public class ValidatorTests
{
    private readonly RegisterTeacherRequestValidator _registerTeacher = new();
    private readonly StudentLoginRequestValidator _studentLogin = new();
    private readonly CreateStudentRequestValidator _createStudent = new();
    private readonly CreateCenterRequestValidator _createCenter = new();
    private readonly CreateClassRequestValidator _createClass = new();
    private readonly TickAttendanceRequestValidator _tickAttendance = new();
    private readonly EnrollStudentRequestValidator _enrollStudent = new();
    private readonly CreateQuotaRequestValidator _createQuota = new();

    [Fact]
    public void RegisterTeacher_ValidRequest_Passes()
    {
        var result = _registerTeacher.TestValidate(new RegisterTeacherRequest
        {
            FullName = "Ahmed",
            Username = "ahmed",
            Phone = "01012345678",
            Password = "StrongPass123"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegisterTeacher_WeakPassword_HasErrors()
    {
        var result = _registerTeacher.TestValidate(new RegisterTeacherRequest
        {
            FullName = "Ahmed",
            Username = "ahmed",
            Phone = "01012345678",
            Password = "weak"
        });

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcd")]
    [InlineData("12345")]
    public void CreateStudent_InvalidPin_HasErrors(string pin)
    {
        var result = _createStudent.TestValidate(new CreateStudentRequest
        {
            FullName = "Omar",
            Phone = "01012345678",
            Pin = pin
        });

        result.ShouldHaveValidationErrorFor(x => x.Pin);
    }

    [Fact]
    public void CreateStudent_ValidRequest_Passes()
    {
        var result = _createStudent.TestValidate(new CreateStudentRequest
        {
            FullName = "Omar",
            Phone = "01012345678",
            Pin = "4321"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void StudentLogin_InvalidPhone_HasErrors()
    {
        var result = _studentLogin.TestValidate(new StudentLoginRequest { Phone = "abc", Pin = "4321" });

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void CreateCenter_EmptyName_HasErrors()
    {
        var result = _createCenter.TestValidate(new CreateCenterRequest { Name = "", LocationDetails = "" });

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateClass_InvalidFrequency_HasErrors()
    {
        var result = _createClass.TestValidate(new CreateClassRequest
        {
            Name = "Math",
            CenterId = 1,
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = new TimeSpan(16, 0, 0),
            Frequency = (Frequency)99
        });

        result.ShouldHaveValidationErrorFor(x => x.Frequency);
    }

    [Fact]
    public void CreateClass_InvalidStartTime_HasErrors()
    {
        var result = _createClass.TestValidate(new CreateClassRequest
        {
            Name = "Math",
            CenterId = 1,
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = new TimeSpan(30, 0, 0),
            Frequency = Frequency.OncePerWeek
        });

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void TickAttendance_ValidRequest_Passes()
    {
        var result = _tickAttendance.TestValidate(new TickAttendanceRequest
        {
            StudentId = 10,
            Date = new DateOnly(2026, 8, 18)
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void TickAttendance_ZeroStudentId_HasErrors()
    {
        var result = _tickAttendance.TestValidate(new TickAttendanceRequest
        {
            StudentId = 0,
            Date = new DateOnly(2026, 8, 18)
        });

        result.ShouldHaveValidationErrorFor(x => x.StudentId);
    }

    [Fact]
    public void TickAttendance_DefaultDate_HasErrors()
    {
        var result = _tickAttendance.TestValidate(new TickAttendanceRequest
        {
            StudentId = 10,
            Date = default
        });

        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void EnrollStudent_ZeroStudentId_HasErrors()
    {
        var result = _enrollStudent.TestValidate(new EnrollStudentRequest { StudentId = 0 });

        result.ShouldHaveValidationErrorFor(x => x.StudentId);
    }

    [Fact]
    public void EnrollStudent_ValidRequest_Passes()
    {
        var result = _enrollStudent.TestValidate(new EnrollStudentRequest { StudentId = 10 });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateQuota_ValidRequest_Passes()
    {
        var result = _createQuota.TestValidate(new CreateQuotaRequest
        {
            ClassGroupId = 1,
            TotalSessions = 4,
            Price = 500,
            PeriodStart = new DateOnly(2026, 8, 1),
            PeriodEnd = new DateOnly(2026, 8, 31)
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateQuota_ZeroTotalSessions_HasErrors()
    {
        var result = _createQuota.TestValidate(new CreateQuotaRequest
        {
            ClassGroupId = 1,
            TotalSessions = 0,
            Price = 500,
            PeriodStart = new DateOnly(2026, 8, 1),
            PeriodEnd = new DateOnly(2026, 8, 31)
        });

        result.ShouldHaveValidationErrorFor(x => x.TotalSessions);
    }

    [Fact]
    public void CreateQuota_NegativePrice_HasErrors()
    {
        var result = _createQuota.TestValidate(new CreateQuotaRequest
        {
            ClassGroupId = 1,
            TotalSessions = 4,
            Price = -10,
            PeriodStart = new DateOnly(2026, 8, 1),
            PeriodEnd = new DateOnly(2026, 8, 31)
        });

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void CreateQuota_ReversedPeriod_HasErrors()
    {
        var result = _createQuota.TestValidate(new CreateQuotaRequest
        {
            ClassGroupId = 1,
            TotalSessions = 4,
            Price = 500,
            PeriodStart = new DateOnly(2026, 8, 31),
            PeriodEnd = new DateOnly(2026, 8, 1)
        });

        result.ShouldHaveValidationErrorFor(x => x.PeriodEnd);
    }
}