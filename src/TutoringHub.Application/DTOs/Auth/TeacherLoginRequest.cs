namespace TutoringHub.Application.DTOs.Auth;

public class TeacherLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}