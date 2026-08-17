namespace TutoringHub.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int ClassGroupId { get; set; }
    public ClassGroup ClassGroup { get; set; } = null!;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}