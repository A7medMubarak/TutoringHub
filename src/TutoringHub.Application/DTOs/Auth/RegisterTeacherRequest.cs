namespace TutoringHub.Application.DTOs.Auth;

public class RegisterTeacherRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}