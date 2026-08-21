namespace TutoringHub.Application.DTOs.Attendance;

public class TickAttendanceRequest
{
    public int StudentId { get; set; }
    public DateOnly Date { get; set; }
}