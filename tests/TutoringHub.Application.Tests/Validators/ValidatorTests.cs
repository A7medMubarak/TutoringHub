using FluentAssertions;
using FluentValidation.TestHelper;
using TutoringHub.Application.DTOs.Auth;
using TutoringHub.Application.DTOs.Centers;
using TutoringHub.Application.DTOs.Classes;
using TutoringHub.Application.DTOs.Students;
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
}