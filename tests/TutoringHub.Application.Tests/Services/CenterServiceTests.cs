using FluentAssertions;
using TutoringHub.Application.DTOs.Centers;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Infrastructure.Persistence;

namespace TutoringHub.Application.Tests.Services;

public class CenterServiceTests
{
    private static CenterService CreateService(ApplicationDbContext context, int? teacherId) =>
        new(context, new StubCurrentUser(teacherId));

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesCenterForCurrentTeacher()
    {
        var ctx = MockDbContext.Create();
        var service = CreateService(ctx, teacherId: 5);

        var result = await service.CreateAsync(new CreateCenterRequest { Name = "Nasr City Center", LocationDetails = "Cairo" });

        result.Name.Should().Be("Nasr City Center");
        ctx.Centers.Single().TeacherId.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyOwnCenters()
    {
        var mine = new Center { Name = "Mine", LocationDetails = "A", TeacherId = 1 };
        var other = new Center { Name = "Other", LocationDetails = "B", TeacherId = 2 };
        var ctx = MockDbContext.Create(centers: [mine, other]);
        var service = CreateService(ctx, teacherId: 1);

        var result = await service.GetAllAsync();

        result.Should().ContainSingle();
        result.Single().Name.Should().Be("Mine");
    }

    private sealed class StubCurrentUser(int? teacherId) : ICurrentUserService
    {
        public int? UserId => teacherId;
        public Domain.Enums.Role? Role => teacherId.HasValue ? Domain.Enums.Role.Teacher : null;
        public int? TeacherId => teacherId;
    }
}