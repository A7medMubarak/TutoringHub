using FluentAssertions;
using Moq;
using TutoringHub.Application.DTOs.Students;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.Services;

public class StudentServiceTests
{
    private static ICurrentUserService CreateCurrentUser(int? teacherId) =>
        new MockCurrentUser(teacherId);

    private static StudentService CreateService(ApplicationDbContext context, int? teacherId) =>
        new(context, CreateCurrentUser(teacherId));

    private static CreateStudentRequest ValidRequest() => new()
    {
        FullName = "Omar Khaled",
        Phone = "01012345678",
        Pin = "4321"
    };

    [Fact]
    public async Task CreateAsync_ValidRequest_AssignsStudentToCurrentTeacher()
    {
        var ctx = MockDbContext.Create();
        var service = CreateService(ctx, teacherId: 7);

        var result = await service.CreateAsync(ValidRequest());

        result.Id.Should().BeGreaterThan(0);
        result.FullName.Should().Be("Omar Khaled");
        var saved = ctx.Students.Single();
        saved.TeacherId.Should().Be(7);
        saved.PinHash.Should().NotBe("4321", "PIN must be hashed, never stored in plain text.");
    }

    [Fact]
    public async Task CreateAsync_DuplicatePhone_ThrowsArgumentException()
    {
        var existing = new Student { FullName = "Old", Phone = "01012345678", PinHash = "x", TeacherId = 1 };
        var ctx = MockDbContext.Create(students: [existing]);
        var service = CreateService(ctx, teacherId: 1);

        await service.Invoking(s => s.CreateAsync(ValidRequest()))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("A student with this phone number already exists.");
    }

    [Fact]
    public async Task CreateAsync_NotAuthenticatedAsTeacher_ThrowsUnauthorized()
    {
        var ctx = MockDbContext.Create();
        var service = CreateService(ctx, teacherId: null);

        await service.Invoking(s => s.CreateAsync(ValidRequest()))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyStudentsOfCurrentTeacher()
    {
        var mine = new Student { FullName = "My Student", Phone = "01011111111", PinHash = "x", TeacherId = 1 };
        var other = new Student { FullName = "Other Teacher Student", Phone = "01022222222", PinHash = "x", TeacherId = 2 };
        var ctx = MockDbContext.Create(students: [mine, other]);
        var service = CreateService(ctx, teacherId: 1);

        var result = await service.GetAllAsync();

        result.Should().ContainSingle();
        result.Single().FullName.Should().Be("My Student");
    }

    [Fact]
    public async Task GetByIdAsync_StudentOfAnotherTeacher_ThrowsKeyNotFound()
    {
        var mine = new Student { FullName = "My Student", Phone = "01011111111", PinHash = "x", TeacherId = 1 };
        var other = new Student { FullName = "Other Student", Phone = "01022222222", PinHash = "x", TeacherId = 2 };
        var ctx = MockDbContext.Create(students: [mine, other]);
        var service = CreateService(ctx, teacherId: 1);

        await service.Invoking(s => s.GetByIdAsync(other.Id))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    private sealed class MockCurrentUser(int? teacherId) : ICurrentUserService
    {
        public int? UserId => teacherId;
        public Domain.Enums.Role? Role => teacherId.HasValue ? Domain.Enums.Role.Teacher : null;
        public int? TeacherId => teacherId;
    }
}