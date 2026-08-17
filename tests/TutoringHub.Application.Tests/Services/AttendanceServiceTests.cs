using FluentAssertions;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.Services;

public class AttendanceServiceTests
{
    private static readonly DateOnly SessionDate = new(2026, 8, 18);

    private static AttendanceService CreateService(ApplicationDbContext context, int? teacherId) =>
        new(context, new StubCurrentUser(teacherId));

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
    public async Task TickAsync_EnrolledStudent_CreatesSessionAndAttendance()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("Omar", "01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        var entry = await service.TickAsync(cls.Id, student.Id, SessionDate);

        entry.IsPresent.Should().BeTrue();
        entry.IsMakeUp.Should().BeFalse();
        entry.StudentId.Should().Be(student.Id);
        ctx.ClassSessions.Should().ContainSingle(s => s.ClassGroupId == cls.Id && s.Date == SessionDate);
        ctx.Attendances.Should().ContainSingle(a => a.StudentId == student.Id);
    }

    [Fact]
    public async Task TickAsync_NonEnrolledStudent_RecordsMakeUp()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var visitor = NewStudent("Huda", "01000000002", 3);
        var ctx = MockDbContext.Create(centers: [center], students: [visitor], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        var entry = await service.TickAsync(cls.Id, visitor.Id, SessionDate);

        entry.IsMakeUp.Should().BeTrue();
        ctx.Attendances.Should().ContainSingle(a => a.StudentId == visitor.Id)
            .Which.ClassSessionId.Should().Be(ctx.ClassSessions.Single().Id);
    }

    [Fact]
    public async Task TickAsync_Twice_IsIdempotent()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("Omar", "01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        await service.TickAsync(cls.Id, student.Id, SessionDate);
        await service.TickAsync(cls.Id, student.Id, SessionDate);

        ctx.ClassSessions.Should().ContainSingle();
        ctx.Attendances.Should().ContainSingle();
    }

    [Fact]
    public async Task TickAsync_ClassOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "Other", LocationDetails = "Giza", TeacherId = 9 };
        var cls = NewClass(center, 9);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.TickAsync(cls.Id, 1, SessionDate))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Class not found.");
    }

    [Fact]
    public async Task TickAsync_StudentOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var foreign = NewStudent("Ali", "01000000002", 9);
        var ctx = MockDbContext.Create(centers: [center], students: [foreign], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.TickAsync(cls.Id, foreign.Id, SessionDate))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Student not found.");
    }

    [Fact]
    public async Task GetRosterAsync_ShowsEnrolledAndMakeUpEntries()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var omar = NewStudent("Omar", "01000000001", 3);
        var ali = NewStudent("Ali", "01000000002", 3);
        var huda = NewStudent("Huda", "01000000003", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [omar, ali, huda], classGroups: [cls],
            enrollments: [new Enrollment { Student = omar, ClassGroup = cls }, new Enrollment { Student = ali, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        await service.TickAsync(cls.Id, omar.Id, SessionDate);
        await service.TickAsync(cls.Id, huda.Id, SessionDate);

        var roster = await service.GetRosterAsync(cls.Id, SessionDate);

        roster.SessionId.Should().NotBeNull();
        roster.Entries.Should().HaveCount(3);
        roster.Entries.Should().ContainSingle(e => e.StudentId == omar.Id)
            .Which.IsPresent.Should().BeTrue();
        roster.Entries.Should().ContainSingle(e => e.StudentId == ali.Id)
            .Which.IsPresent.Should().BeFalse();
        roster.Entries.Should().ContainSingle(e => e.StudentId == huda.Id)
            .Which.IsMakeUp.Should().BeTrue();
    }

    [Fact]
    public async Task GetRosterAsync_NoSessionYet_ShowsEveryoneAbsent()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var omar = NewStudent("Omar", "01000000001", 3);
        var ali = NewStudent("Ali", "01000000002", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [omar, ali], classGroups: [cls],
            enrollments: [new Enrollment { Student = omar, ClassGroup = cls }, new Enrollment { Student = ali, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        var roster = await service.GetRosterAsync(cls.Id, SessionDate);

        roster.SessionId.Should().BeNull();
        roster.Entries.Should().HaveCount(2);
        roster.Entries.Should().OnlyContain(e => !e.IsPresent && !e.IsMakeUp);
    }

    [Fact]
    public async Task GetRosterAsync_ClassOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "Other", LocationDetails = "Giza", TeacherId = 9 };
        var cls = NewClass(center, 9);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.GetRosterAsync(cls.Id, SessionDate))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UntickAsync_RemovesAttendanceAndKeepsSession()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("Omar", "01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);
        await service.TickAsync(cls.Id, student.Id, SessionDate);

        await service.UntickAsync(cls.Id, student.Id, SessionDate);

        ctx.Attendances.Should().BeEmpty();
        ctx.ClassSessions.Should().ContainSingle();
    }

    [Fact]
    public async Task UntickAsync_WithoutSession_DoesNotThrow()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        await service.UntickAsync(cls.Id, 1, SessionDate);
    }

    [Fact]
    public async Task GetSessionsAsync_ReturnsOwnSessionsWithPresentCountInDateOrder()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var omar = NewStudent("Omar", "01000000001", 3);
        var ali = NewStudent("Ali", "01000000002", 3);
        var cls = NewClass(center, 3);
        var otherClass = NewClass(center, 3, "English Sunday 3PM");
        var ctx = MockDbContext.Create(centers: [center], students: [omar, ali], classGroups: [cls, otherClass],
            enrollments:
            [
                new Enrollment { Student = omar, ClassGroup = cls },
                new Enrollment { Student = ali, ClassGroup = cls },
                new Enrollment { Student = omar, ClassGroup = otherClass }
            ]);
        var service = CreateService(ctx, teacherId: 3);

        var earlier = new DateOnly(2026, 8, 15);
        await service.TickAsync(cls.Id, omar.Id, earlier);
        await service.TickAsync(cls.Id, ali.Id, earlier);
        await service.TickAsync(cls.Id, omar.Id, SessionDate);
        await service.TickAsync(otherClass.Id, omar.Id, earlier);

        var sessions = await service.GetSessionsAsync(cls.Id, null, null);

        sessions.Should().HaveCount(2);
        sessions[0].Date.Should().Be(earlier);
        sessions[0].PresentCount.Should().Be(2);
        sessions[1].Date.Should().Be(SessionDate);
        sessions[1].PresentCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSessionsAsync_DateRange_Filters()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var omar = NewStudent("Omar", "01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [omar], classGroups: [cls],
            enrollments: [new Enrollment { Student = omar, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        await service.TickAsync(cls.Id, omar.Id, new DateOnly(2026, 8, 15));
        await service.TickAsync(cls.Id, omar.Id, new DateOnly(2026, 8, 18));
        await service.TickAsync(cls.Id, omar.Id, new DateOnly(2026, 8, 22));

        var sessions = await service.GetSessionsAsync(cls.Id, new DateOnly(2026, 8, 16), new DateOnly(2026, 8, 20));

        sessions.Should().ContainSingle().Which.Date.Should().Be(new DateOnly(2026, 8, 18));
    }

    private sealed class StubCurrentUser(int? teacherId) : ICurrentUserService
    {
        public int? UserId => teacherId;
        public Domain.Enums.Role? Role => teacherId.HasValue ? Domain.Enums.Role.Teacher : null;
        public int? TeacherId => teacherId;
    }
}