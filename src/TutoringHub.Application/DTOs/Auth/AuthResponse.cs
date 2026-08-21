using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime RefreshExpiresAtUtc { get; set; }
    public Role Role { get; set; }
    public int UserId { get; set; }
    public int? TeacherId { get; set; }
}