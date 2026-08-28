using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.DTOs.Auth;

public class CurrentUserResponse
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? TeacherId { get; set; }
}
