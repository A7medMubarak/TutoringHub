using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.DTOs.Classes;

public class ClassGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CenterId { get; set; }
    public string CenterName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public Frequency Frequency { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
}