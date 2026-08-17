using TutoringHub.Domain.Enums;

namespace TutoringHub.Domain.Entities;

public class ClassGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;
    public int CenterId { get; set; }
    public Center Center { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public Frequency Frequency { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}