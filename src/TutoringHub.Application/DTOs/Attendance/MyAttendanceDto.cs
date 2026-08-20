namespace TutoringHub.Application.DTOs.Attendance;

public class MyAttendanceDto
{
    public int ClassGroupId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsMakeUp { get; set; }
}