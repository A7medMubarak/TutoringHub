using FluentAssertions;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.Services;

public class EnrollmentServiceTests
{
    private static EnrollmentService CreateService(ApplicationDbContext context, int? teacherId) =>
        new(context, new StubCurrentUser(teacherId));

    private static ClassGroup NewClass(Center center, int teacherId) =>
        new()
        {
            Name = "Math Saturday 4PM",
            TeacherId = teacherId,
            Center = center,
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = new TimeSpan(16, 0, 0),
            Frequency = Frequency.OncePerWeek,
            IsActive = true
        };

    private static Student NewStudent(string phone, int teacherId) =>
        new() { FullName = "Omar", Phone = phone, PinHash = "x", TeacherId = teacherId };

    [Fact]
    public async Task EnrollAsync_ValidStudent_AddsEnrollment()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        var result = await service.EnrollAsync(cls.Id, student.Id);

        result.StudentId.Should().Be(student.Id);
        result.StudentName.Should().Be("Omar");
        ctx.Enrollments.Should().ContainSingle(e => e.StudentId == student.Id && e.ClassGroupId == cls.Id);
    }

    [Fact]
    public async Task EnrollAsync_DuplicateEnrollment_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.EnrollAsync(cls.Id, student.Id))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Student is already enrolled.");
    }

    [Fact]
    public async Task EnrollAsync_ClassOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "Other", LocationDetails = "Giza", TeacherId = 9 };
        var cls = NewClass(center, 9);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.EnrollAsync(cls.Id, 1))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Class not found.");
    }

    [Fact]
    public async Task EnrollAsync_StudentOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var foreign = NewStudent("01000000002", 9);
        var ctx = MockDbContext.Create(centers: [center], students: [foreign], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.EnrollAsync(cls.Id, foreign.Id))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Student not found.");
    }

    [Fact]
    public async Task UnenrollAsync_RemovesEnrollment()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        await service.UnenrollAsync(cls.Id, student.Id);

        ctx.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public async Task UnenrollAsync_NotEnrolled_DoesNotThrow()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        await service.UnenrollAsync(cls.Id, 1);
    }

    private sealed class StubCurrentUser(int? teacherId) : ICurrentUserService
    {
        public int? UserId => teacherId;
        public Domain.Enums.Role? Role => teacherId.HasValue ? Domain.Enums.Role.Teacher : null;
        public int? TeacherId => teacherId;
    }
}