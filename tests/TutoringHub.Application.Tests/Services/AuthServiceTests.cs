using FluentAssertions;
using Microsoft.Extensions.Options;
using TutoringHub.Application.Common.Options;
using TutoringHub.Application.DTOs.Auth;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Infrastructure.Persistence;
using TutoringHub.Infrastructure.Security;

namespace TutoringHub.Application.Tests.Services;

public class AuthServiceTests
{
    private const string TeacherPassword = "StrongPass123";

    private static JwtOptions TestJwtOptions => new()
    {
        Key = "test-secret-key-with-at-least-32-characters!!",
        Issuer = "TutoringHub.Tests",
        Audience = "TutoringHub.Tests",
        AccessTokenMinutes = 60,
        RefreshTokenDays = 14
    };

    private static ITokenService CreateTokenService() =>
        new JwtTokenService(Options.Create(TestJwtOptions));

    private static AuthService CreateService(ApplicationDbContext context) =>
        new(context, CreateTokenService(), Options.Create(TestJwtOptions));

    private static RegisterTeacherRequest ValidTeacherRequest() => new()
    {
        FullName = "Ahmed Teacher",
        Username = "ahmed",
        Phone = "01000000001",
        Password = TeacherPassword
    };

    [Fact]
    public async Task RegisterTeacherAsync_ValidRequest_CreatesTeacherAndReturnsTokens()
    {
        var ctx = MockDbContext.Create();
        var service = CreateService(ctx);

        var result = await service.RegisterTeacherAsync(ValidTeacherRequest());

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Role.Should().Be(Role.Teacher);
        result.UserId.Should().BeGreaterThan(0);
        ctx.Teachers.Should().ContainSingle(t => t.Username == "ahmed");
    }

    [Fact]
    public async Task RegisterTeacherAsync_DuplicateUsername_ThrowsArgumentException()
    {
        var existing = new Teacher { FullName = "Old", Username = "ahmed", Phone = "01000000002", PasswordHash = "x" };
        var ctx = MockDbContext.Create(teachers: [existing]);
        var service = CreateService(ctx);

        await service.Invoking(s => s.RegisterTeacherAsync(ValidTeacherRequest()))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Username is already taken.");
    }

    [Fact]
    public async Task RegisterTeacherAsync_DuplicatePhone_ThrowsArgumentException()
    {
        var existing = new Teacher { FullName = "Old", Username = "other", Phone = "01000000001", PasswordHash = "x" };
        var ctx = MockDbContext.Create(teachers: [existing]);
        var service = CreateService(ctx);

        await service.Invoking(s => s.RegisterTeacherAsync(ValidTeacherRequest()))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Phone number is already registered.");
    }

    [Fact]
    public async Task LoginTeacherAsync_ValidCredentials_ReturnsTokens()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(TeacherPassword);
        var existing = new Teacher { FullName = "Ahmed", Username = "ahmed", Phone = "01000000001", PasswordHash = passwordHash };
        var ctx = MockDbContext.Create(teachers: [existing]);
        var service = CreateService(ctx);

        var result = await service.LoginTeacherAsync(new TeacherLoginRequest { Username = "ahmed", Password = TeacherPassword });

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Role.Should().Be(Role.Teacher);
        result.TeacherId.Should().Be(existing.Id);
    }

    [Theory]
    [InlineData("ahmed", "WrongPass123")]
    [InlineData("nobody", "StrongPass123")]
    public async Task LoginTeacherAsync_InvalidCredentials_ThrowsUnauthorized(string username, string password)
    {
        var existing = new Teacher { FullName = "Ahmed", Username = "ahmed", Phone = "01000000001", PasswordHash = BCrypt.Net.BCrypt.HashPassword(TeacherPassword) };
        var ctx = MockDbContext.Create(teachers: [existing]);
        var service = CreateService(ctx);

        await service.Invoking(s => s.LoginTeacherAsync(new TeacherLoginRequest { Username = username, Password = password }))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginStudentAsync_ValidPhoneAndPin_ReturnsTokensWithTeacherId()
    {
        var teacher = new Teacher { FullName = "Ahmed", Username = "ahmed", Phone = "01000000001", PasswordHash = "x" };
        var ctx = MockDbContext.Create(teachers: [teacher]);
        var student = new Student { FullName = "Omar", Phone = "01000000009", PinHash = BCrypt.Net.BCrypt.HashPassword("4321"), TeacherId = teacher.Id };
        ctx.Students.Add(student);
        await ctx.SaveChangesAsync();
        var service = CreateService(ctx);

        var result = await service.LoginStudentAsync(new StudentLoginRequest { Phone = "01000000009", Pin = "4321" });

        result.Role.Should().Be(Role.Student);
        result.UserId.Should().Be(student.Id);
        result.TeacherId.Should().Be(teacher.Id);
    }

    [Fact]
    public async Task LoginStudentAsync_WrongPin_ThrowsUnauthorized()
    {
        var student = new Student { FullName = "Omar", Phone = "01000000009", PinHash = BCrypt.Net.BCrypt.HashPassword("4321"), TeacherId = 1 };
        var ctx = MockDbContext.Create(students: [student]);
        var service = CreateService(ctx);

        await service.Invoking(s => s.LoginStudentAsync(new StudentLoginRequest { Phone = "01000000009", Pin = "9999" }))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_RotatesAndRejectsReplayOfOldToken()
    {
        var teacher = new Teacher { FullName = "Ahmed", Username = "ahmed", Phone = "01000000001", PasswordHash = BCrypt.Net.BCrypt.HashPassword(TeacherPassword) };
        var ctx = MockDbContext.Create(teachers: [teacher]);
        var service = CreateService(ctx);

        var initial = await service.LoginTeacherAsync(new TeacherLoginRequest { Username = "ahmed", Password = TeacherPassword });

        var refreshed = await service.RefreshAsync(new RefreshRequest { RefreshToken = initial.RefreshToken });

        refreshed.RefreshToken.Should().NotBe(initial.RefreshToken);

        await service.Invoking(s => s.RefreshAsync(new RefreshRequest { RefreshToken = initial.RefreshToken }))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token has already been used.");
    }

    [Fact]
    public async Task RefreshAsync_UnknownToken_ThrowsUnauthorized()
    {
        var ctx = MockDbContext.Create();
        var service = CreateService(ctx);

        await service.Invoking(s => s.RefreshAsync(new RefreshRequest { RefreshToken = "garbage-token" }))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ThrowsUnauthorized()
    {
        var tokenService = CreateTokenService();
        var rawToken = tokenService.GenerateRefreshToken();
        var ctx = MockDbContext.Create(refreshTokens:
        [
            new RefreshToken
            {
                UserType = Role.Teacher,
                UserId = 1,
                TokenHash = tokenService.HashRefreshToken(rawToken),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-1)
            }
        ]);
        var service = CreateService(ctx);

        await service.Invoking(s => s.RefreshAsync(new RefreshRequest { RefreshToken = rawToken }))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token has expired.");
    }

    [Fact]
    public async Task LogoutAsync_RevokesTokenAndRejectsFurtherRefresh()
    {
        var teacher = new Teacher { FullName = "Ahmed", Username = "ahmed", Phone = "01000000001", PasswordHash = BCrypt.Net.BCrypt.HashPassword(TeacherPassword) };
        var ctx = MockDbContext.Create(teachers: [teacher]);
        var service = CreateService(ctx);

        var login = await service.LoginTeacherAsync(new TeacherLoginRequest { Username = "ahmed", Password = TeacherPassword });

        await service.LogoutAsync(new LogoutRequest { RefreshToken = login.RefreshToken });

        await service.Invoking(s => s.RefreshAsync(new RefreshRequest { RefreshToken = login.RefreshToken }))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token has already been used.");
    }

    [Fact]
    public async Task RefreshTokenStored_IsSha256Hash_NotTheRawToken()
    {
        var ctx = MockDbContext.Create();
        var service = CreateService(ctx);

        var result = await service.RegisterTeacherAsync(ValidTeacherRequest());

        var stored = ctx.RefreshTokens.Single();
        stored.TokenHash.Should().NotBe(result.RefreshToken);

        var expectedHash = CreateTokenService().HashRefreshToken(result.RefreshToken);
        stored.TokenHash.Should().Be(expectedHash);
    }
}