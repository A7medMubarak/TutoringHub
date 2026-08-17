using FluentAssertions;
using TutoringHub.Application.DTOs.Classes;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.Services;

public class ClassGroupServiceTests
{
    private static ClassGroupService CreateService(ApplicationDbContext context, int? teacherId) =>
        new(context, new StubCurrentUser(teacherId));

    private static CreateClassRequest ValidRequest(int centerId) => new()
    {
        Name = "Math Saturday 4PM",
        CenterId = centerId,
        DayOfWeek = DayOfWeek.Saturday,
        StartTime = new TimeSpan(16, 0, 0),
        Frequency = Frequency.OncePerWeek
    };

    [Fact]
    public async Task CreateAsync_WithOwnedCenter_CreatesClassGroup()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 3 };
        var ctx = MockDbContext.Create(centers: [center]);
        var service = CreateService(ctx, teacherId: 3);

        var result = await service.CreateAsync(ValidRequest(center.Id));

        result.Name.Should().Be("Math Saturday 4PM");
        result.DayOfWeek.Should().Be(DayOfWeek.Saturday);
        result.CenterId.Should().Be(center.Id);
        ctx.ClassGroups.Single().TeacherId.Should().Be(3);
    }

    [Fact]
    public async Task CreateAsync_WithCenterOfAnotherTeacher_ThrowsArgumentException()
    {
        var foreignCenter = new Center { Name = "Other Center", LocationDetails = "Giza", TeacherId = 9 };
        var ctx = MockDbContext.Create(centers: [foreignCenter]);
        var service = CreateService(ctx, teacherId: 3);

        await service.Invoking(s => s.CreateAsync(ValidRequest(foreignCenter.Id)))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Center not found.");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyOwnClassesWithCenterNameAndStudentCount()
    {
        var center = new Center { Name = "C1", LocationDetails = "Cairo", TeacherId = 1 };
        var student = new Student { FullName = "S", Phone = "01000000000", PinHash = "x", TeacherId = 1 };
        var mine = new ClassGroup { Name = "Mine", TeacherId = 1, Center = center, DayOfWeek = DayOfWeek.Saturday, StartTime = new TimeSpan(16, 0, 0), Frequency = Frequency.OncePerWeek, IsActive = true };
        var ctx = MockDbContext.Create(centers: [center], students: [student], classGroups: [mine]);
        ctx.Enrollments.Add(new Enrollment { StudentId = student.Id, ClassGroupId = mine.Id });
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx, teacherId: 1);

        var result = await service.GetAllAsync();

        result.Should().ContainSingle();
        result.Single().CenterName.Should().Be("C1");
        result.Single().StudentCount.Should().Be(1);
    }

    private sealed class StubCurrentUser(int? teacherId) : ICurrentUserService
    {
        public int? UserId => teacherId;
        public Domain.Enums.Role? Role => teacherId.HasValue ? Domain.Enums.Role.Teacher : null;
        public int? TeacherId => teacherId;
    }
}