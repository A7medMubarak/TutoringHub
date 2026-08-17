namespace TutoringHub.Application.DTOs.Students;

public class CreateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
}