namespace TutoringHub.Application.DTOs.Attendance;

public class AttendanceEntryDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool IsPresent { get; set; }
    public bool IsMakeUp { get; set; }
}