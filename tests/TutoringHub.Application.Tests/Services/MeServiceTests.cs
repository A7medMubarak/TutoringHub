using FluentAssertions;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.Services;

public class MeServiceTests
{
    private static MeService CreateService(ApplicationDbContext context, int studentId) =>
        new(context, new StubStudentUser(studentId));

    private static MeService CreateTeacherService(ApplicationDbContext context) =>
        new(context, new StubTeacherUser());

    private static ClassGroup NewClass(Center center, int teacherId, string name = "Math Saturday 4PM") =>
        new()
        {
            Name = name,
            TeacherId = teacherId,
            Center = center,
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = new TimeSpan(16, 0, 0),
            Frequency = Frequency.OncePerWeek,
            IsActive = true
        };

    private static Student NewStudent(string fullName, string phone, int teacherId) =>
        new() { FullName = fullName, Phone = phone, PinHash = "x", TeacherId = teacherId };

    [Fact]
    public async Task GetClassesAsync_ReturnsOwnEnrolledClassesWithStudentCounts()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var math = NewClass(center, 3, "Math");
        var english = NewClass(center, 3, "English");
        var omar = NewStudent("Omar", "01000000001", 3);
        var ali = NewStudent("Ali", "01000000002", 3);
        var ctx = MockDbContext.Create(centers: [center], students: [omar, ali], classGroups: [math, english],
            enrollments:
            [
                new Enrollment { Student = omar, ClassGroup = math },
                new Enrollment { Student = omar, ClassGroup = english },
                new Enrollment { Student = ali, ClassGroup = math }
            ]);
        var service = CreateService(ctx, omar.Id);

        var result = await service.GetClassesAsync();

        result.Should().HaveCount(2);
        var mathDto = result.Single(c => c.Id == math.Id);
        mathDto.CenterName.Should().Be("C1");
        mathDto.StudentCount.Should().Be(2);
        result.Single(c => c.Id == english.Id).StudentCount.Should().Be(1);
    }

    [Fact]
    public async Task GetClassesAsync_NotEnrolled_ReturnsEmpty()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var math = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [math]);
        var service = CreateService(ctx, 5);

        var result = await service.GetClassesAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClassesAsync_NotStudent_ThrowsUnauthorizedAccessException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateTeacherService(ctx);

        await service.Invoking(s => s.GetClassesAsync())
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetAttendanceAsync_FlagsOtherClassSessionsAsMakeUpAndOrdersByDateDesc()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var own = NewClass(center, 3, "Math");
        var visited = NewClass(center, 3, "English");
        var omar = NewStudent("Omar", "01000000001", 3);
        var ownSession = new ClassSession { ClassGroup = own, Date = new DateOnly(2026, 8, 20) };
        var visitSession = new ClassSession { ClassGroup = visited, Date = new DateOnly(2026, 8, 18) };
        var ctx = MockDbContext.Create(centers: [center], students: [omar], classGroups: [own, visited],
            enrollments: [new Enrollment { Student = omar, ClassGroup = own }],
            classSessions: [ownSession, visitSession],
            attendances:
            [
                new Attendance { Student = omar, ClassSession = ownSession },
                new Attendance { Student = omar, ClassSession = visitSession }
            ]);
        var service = CreateService(ctx, omar.Id);

        var result = await service.GetAttendanceAsync();

        result.Should().HaveCount(2);
        result[0].Date.Should().Be(new DateOnly(2026, 8, 20));
        result[0].ClassName.Should().Be("Math");
        result[0].IsMakeUp.Should().BeFalse();
        result[1].Date.Should().Be(new DateOnly(2026, 8, 18));
        result[1].ClassName.Should().Be("English");
        result[1].IsMakeUp.Should().BeTrue();
    }

    [Fact]
    public async Task GetAttendanceAsync_ReturnsOwnRowsOnly()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var omar = NewStudent("Omar", "01000000001", 3);
        var ali = NewStudent("Ali", "01000000002", 3);
        var session = new ClassSession { ClassGroup = cls, Date = new DateOnly(2026, 8, 20) };
        var ctx = MockDbContext.Create(centers: [center], students: [omar, ali], classGroups: [cls],
            enrollments: [new Enrollment { Student = omar, ClassGroup = cls }],
            classSessions: [session],
            attendances:
            [
                new Attendance { Student = omar, ClassSession = session },
                new Attendance { Student = ali, ClassSession = session }
            ]);
        var service = CreateService(ctx, omar.Id);

        var result = await service.GetAttendanceAsync();

        result.Should().ContainSingle().Which.ClassName.Should().Be("Math Saturday 4PM");
    }

    [Fact]
    public async Task GetQuotasAsync_ReturnsOwnQuotaRowsWithClassName()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3, "Math");
        var omar = NewStudent("Omar", "01000000001", 3);
        var ali = NewStudent("Ali", "01000000002", 3);
        var ctx = MockDbContext.Create(centers: [center], students: [omar, ali], classGroups: [cls],
            enrollments: [new Enrollment { Student = omar, ClassGroup = cls }],
            quotaRows:
            [
                new QuotaRow
                {
                    Student = omar,
                    ClassGroup = cls,
                    TotalSessions = 4,
                    RemainingSessions = 2,
                    Price = 500,
                    PeriodStart = new DateOnly(2026, 8, 1),
                    PeriodEnd = new DateOnly(2026, 8, 31)
                },
                new QuotaRow
                {
                    Student = ali,
                    ClassGroup = cls,
                    TotalSessions = 4,
                    RemainingSessions = 4,
                    Price = 400,
                    PeriodStart = new DateOnly(2026, 8, 1),
                    PeriodEnd = new DateOnly(2026, 8, 31)
                }
            ]);
        var service = CreateService(ctx, omar.Id);

        var result = await service.GetQuotasAsync();

        result.Should().ContainSingle();
        result[0].ClassName.Should().Be("Math");
        result[0].RemainingSessions.Should().Be(2);
        result[0].TotalSessions.Should().Be(4);
        result[0].Price.Should().Be(500);
    }

    [Fact]
    public async Task GetQuotasAsync_NotStudent_ThrowsUnauthorizedAccessException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateTeacherService(ctx);

        await service.Invoking(s => s.GetQuotasAsync())
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class StubStudentUser(int studentId) : ICurrentUserService
    {
        public int? UserId => studentId;
        public Domain.Enums.Role? Role => Domain.Enums.Role.Student;
        public int? TeacherId => null;
    }

    private sealed class StubTeacherUser : ICurrentUserService
    {
        public int? UserId => 3;
        public Domain.Enums.Role? Role => Domain.Enums.Role.Teacher;
        public int? TeacherId => 3;
    }
}