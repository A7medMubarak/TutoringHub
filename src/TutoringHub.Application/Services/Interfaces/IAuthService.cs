using TutoringHub.Application.DTOs.Auth;

namespace TutoringHub.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterTeacherAsync(RegisterTeacherRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginTeacherAsync(TeacherLoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginStudentAsync(StudentLoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}