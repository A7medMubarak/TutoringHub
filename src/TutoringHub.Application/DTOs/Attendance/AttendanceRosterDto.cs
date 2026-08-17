namespace TutoringHub.Application.DTOs.Attendance;

public class AttendanceRosterDto
{
    public int ClassGroupId { get; set; }
    public DateOnly Date { get; set; }
    public int? SessionId { get; set; }
    public List<AttendanceEntryDto> Entries { get; set; } = new();
}