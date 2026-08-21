using FluentAssertions;
using TutoringHub.Application.DTOs.Quotas;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.Services;

public class QuotaServiceTests
{
    private static QuotaService CreateService(ApplicationDbContext context, int? teacherId) =>
        new(context, new StubCurrentUser(teacherId));

    private static AttendanceService CreateAttendanceService(ApplicationDbContext context, int? teacherId) =>
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

    private static Student NewStudent(string phone, int teacherId) =>
        new() { FullName = "Omar", Phone = phone, PinHash = "x", TeacherId = teacherId };

    private static CreateQuotaRequest ValidQuotaRequest(int classGroupId) => new()
    {
        ClassGroupId = classGroupId,
        TotalSessions = 4,
        Price = 500,
        PeriodStart = new DateOnly(2026, 8, 1),
        PeriodEnd = new DateOnly(2026, 8, 31)
    };

    [Fact]
    public async Task CreateAsync_EnrolledStudent_CreatesRowWithFullRemaining()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        var result = await service.CreateAsync(student.Id, ValidQuotaRequest(cls.Id));

        result.TotalSessions.Should().Be(4);
        result.RemainingSessions.Should().Be(4);
        result.PaidAt.Should().BeNull();
        result.Price.Should().Be(500);
        result.ClassName.Should().Be("Math Saturday 4PM");
    }

    [Fact]
    public async Task CreateAsync_NotEnrolledStudent_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.CreateAsync(student.Id, ValidQuotaRequest(cls.Id)))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Student is not enrolled in this class.");
    }

    [Fact]
    public async Task CreateAsync_StudentOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var cls = NewClass(center, 3);
        var foreign = NewStudent("01000000001", 9);
        var ctx = MockDbContext.Create(centers: [center], students: [foreign], classGroups: [cls]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.CreateAsync(foreign.Id, ValidQuotaRequest(cls.Id)))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Student not found.");
    }

    [Fact]
    public async Task CreateAsync_ClassOfAnotherTeacher_ThrowsArgumentException()
    {
        var center = new Center { Name = "Other", LocationDetails = "Giza", TeacherId = 9 };
        var student = NewStudent("01000000001", 3);
        var cls = NewClass(center, 9);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.CreateAsync(student.Id, ValidQuotaRequest(cls.Id)))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Class not found.");
    }

    [Fact]
    public async Task PayAsync_CoversOldestUnpaidSessionsFirst()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        await CreateAttendanceService(ctx, teacherId: 3).TickAsync(cls.Id, student.Id, new DateOnly(2026, 8, 10));
        await CreateAttendanceService(ctx, teacherId: 3).TickAsync(cls.Id, student.Id, new DateOnly(2026, 8, 17));
        await CreateAttendanceService(ctx, teacherId: 3).TickAsync(cls.Id, student.Id, new DateOnly(2026, 8, 24));
        var request = ValidQuotaRequest(cls.Id);
        request.TotalSessions = 2;
        var quota = await service.CreateAsync(student.Id, request);

        var payment = await service.PayAsync(student.Id, quota.Id);

        payment.CoveredCount.Should().Be(2);
        payment.RemainingUnpaidCount.Should().Be(1);
        ctx.QuotaRows.Single().RemainingSessions.Should().Be(0);
        ctx.Attendances.Count(a => a.QuotaRowId == quota.Id).Should().Be(2);
        ctx.Attendances.Where(a => a.QuotaRowId == quota.Id)
            .Should().OnlyContain(a => a.ClassSession.Date != new DateOnly(2026, 8, 24));
    }

    [Fact]
    public async Task PayAsync_LargerQuota_LeavesRemainingSessions()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);

        await CreateAttendanceService(ctx, teacherId: 3).TickAsync(cls.Id, student.Id, new DateOnly(2026, 8, 10));
        await CreateAttendanceService(ctx, teacherId: 3).TickAsync(cls.Id, student.Id, new DateOnly(2026, 8, 17));
        var quota = await service.CreateAsync(student.Id, ValidQuotaRequest(cls.Id));

        var payment = await service.PayAsync(student.Id, quota.Id);

        payment.CoveredCount.Should().Be(2);
        payment.RemainingUnpaidCount.Should().Be(0);
        ctx.QuotaRows.Single().RemainingSessions.Should().Be(2);
        ctx.QuotaRows.Single().PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PayAsync_MakeUpConsumesOwnClassQuota()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var own = NewClass(center, 3, "Own Math");
        var visited = NewClass(center, 3, "Visited English");
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [own, visited],
            enrollments: [new Enrollment { Student = student, ClassGroup = own }]);
        var service = CreateService(ctx, teacherId: 3);

        await CreateAttendanceService(ctx, teacherId: 3).TickAsync(visited.Id, student.Id, new DateOnly(2026, 8, 10));
        var quota = await service.CreateAsync(student.Id, ValidQuotaRequest(own.Id));

        var payment = await service.PayAsync(student.Id, quota.Id);

        payment.CoveredCount.Should().Be(1);
        ctx.Attendances.Single().QuotaRowId.Should().Be(quota.Id);
        ctx.Attendances.Single().ClassSession.ClassGroupId.Should().Be(visited.Id);
    }

    [Fact]
    public async Task PayAsync_AlreadyPaid_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [cls],
            enrollments: [new Enrollment { Student = student, ClassGroup = cls }]);
        var service = CreateService(ctx, teacherId: 3);
        var quota = await service.CreateAsync(student.Id, ValidQuotaRequest(cls.Id));
        await service.PayAsync(student.Id, quota.Id);

        await service.Invoking(s => s.PayAsync(student.Id, quota.Id))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quota is already paid.");
    }

    [Fact]
    public async Task PayAsync_QuotaOfAnotherStudent_ThrowsArgumentException()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var omar = NewStudent("01000000001", 3);
        var ali = NewStudent("01000000002", 3);
        var cls = NewClass(center, 3);
        var ctx = MockDbContext.Create(centers: [center], students: [omar, ali], classGroups: [cls],
            enrollments:
            [
                new Enrollment { Student = omar, ClassGroup = cls },
                new Enrollment { Student = ali, ClassGroup = cls }
            ]);
        var service = CreateService(ctx, teacherId: 3);
        var aliQuota = await service.CreateAsync(ali.Id, ValidQuotaRequest(cls.Id));

        await service.Invoking(s => s.PayAsync(omar.Id, aliQuota.Id))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quota not found.");
    }

    [Fact]
    public async Task GetForStudentAsync_ReturnsOwnQuotasWithClassName()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var student = NewStudent("01000000001", 3);
        var math = NewClass(center, 3, "Math");
        var english = NewClass(center, 3, "English");
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [math, english],
            enrollments:
            [
                new Enrollment { Student = student, ClassGroup = math },
                new Enrollment { Student = student, ClassGroup = english }
            ]);
        var service = CreateService(ctx, teacherId: 3);
        var first = await service.CreateAsync(student.Id, ValidQuotaRequest(math.Id));
        var second = await service.CreateAsync(student.Id, ValidQuotaRequest(english.Id));

        var result = await service.GetForStudentAsync(student.Id);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(second.Id);
        result[0].ClassName.Should().Be("English");
        result[1].Id.Should().Be(first.Id);
        result[1].ClassName.Should().Be("Math");
    }

    private sealed class StubCurrentUser(int? teacherId) : ICurrentUserService
    {
        public int? UserId => teacherId;
        public Domain.Enums.Role? Role => teacherId.HasValue ? Domain.Enums.Role.Teacher : null;
        public int? TeacherId => teacherId;
    }
}