using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TutoringHub.Application.Common.Options;
using TutoringHub.Application.DTOs.Auth;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(IApplicationDbContext context, ITokenService tokenService, IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterTeacherAsync(RegisterTeacherRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.Teachers.AnyAsync(t => t.Username == request.Username, cancellationToken))
            throw new ArgumentException("Username is already taken.");

        if (await _context.Teachers.AnyAsync(t => t.Phone == request.Phone, cancellationToken))
            throw new ArgumentException("Phone number is already registered.");

        var teacher = new Teacher
        {
            FullName = request.FullName.Trim(),
            Username = request.Username.Trim(),
            Phone = request.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync(cancellationToken);

        return await IssueTokenPairAsync(teacher.Id, Role.Teacher, teacher.Id, cancellationToken: cancellationToken);
    }

    public async Task<AuthResponse> LoginTeacherAsync(TeacherLoginRequest request, CancellationToken cancellationToken = default)
    {
        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Username == request.Username, cancellationToken);

        if (teacher is null || !BCrypt.Net.BCrypt.Verify(request.Password, teacher.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        return await IssueTokenPairAsync(teacher.Id, Role.Teacher, teacher.Id, cancellationToken: cancellationToken);
    }

    public async Task<AuthResponse> LoginStudentAsync(StudentLoginRequest request, CancellationToken cancellationToken = default)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Phone == request.Phone, cancellationToken);

        if (student is null || !BCrypt.Net.BCrypt.Verify(request.Pin, student.PinHash))
            throw new UnauthorizedAccessException("Invalid phone number or PIN.");

        return await IssueTokenPairAsync(student.Id, Role.Student, student.TeacherId, cancellationToken: cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        if (storedToken.RevokedAtUtc is not null)
            throw new UnauthorizedAccessException("Refresh token has already been used.");

        if (storedToken.ExpiresAtUtc <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired.");

        var oldTokenId = storedToken.Id;
        storedToken.RevokedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        int? teacherId = storedToken.UserType == Role.Teacher
            ? storedToken.UserId
            : await GetStudentTeacherIdAsync(storedToken.UserId, cancellationToken);

        return await IssueTokenPairAsync(storedToken.UserId, storedToken.UserType, teacherId, oldTokenId, cancellationToken);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken is not null && storedToken.RevokedAtUtc is null)
        {
            storedToken.RevokedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AuthResponse> IssueTokenPairAsync(
        int userId,
        Role role,
        int? teacherId,
        int? replacedByTokenId = null,
        CancellationToken cancellationToken = default)
    {
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var accessToken = _tokenService.GenerateAccessToken(userId, role, teacherId);
        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserType = role,
            UserId = userId,
            TokenHash = _tokenService.HashRefreshToken(rawRefreshToken),
            ExpiresAtUtc = refreshExpiresAtUtc,
            ReplacedByTokenId = replacedByTokenId
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            RefreshExpiresAtUtc = refreshExpiresAtUtc,
            Role = role,
            UserId = userId,
            TeacherId = teacherId
        };
    }

    private async Task<int?> GetStudentTeacherIdAsync(int studentId, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        return student?.TeacherId;
    }
}