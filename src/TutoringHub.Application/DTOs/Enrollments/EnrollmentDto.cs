namespace TutoringHub.Application.DTOs.Enrollments;

public class EnrollmentDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int ClassGroupId { get; set; }
    public DateTime EnrolledAt { get; set; }
}