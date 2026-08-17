using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(int userId, Role role, int? teacherId = null);
    string GenerateRefreshToken();
    string HashRefreshToken(string rawToken);
}