namespace TutoringHub.Application.DTOs.Auth;

public class StudentLoginRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
}