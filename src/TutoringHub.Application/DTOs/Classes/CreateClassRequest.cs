using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.DTOs.Classes;

public class CreateClassRequest
{
    public string Name { get; set; } = string.Empty;
    public int CenterId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public Frequency Frequency { get; set; }
}