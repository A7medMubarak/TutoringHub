namespace TutoringHub.Domain.Entities;

public class ClassSession
{
    public int Id { get; set; }
    public int ClassGroupId { get; set; }
    public ClassGroup ClassGroup { get; set; } = null!;
    public DateOnly Date { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}