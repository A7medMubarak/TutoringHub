namespace TutoringHub.Domain.Entities;

public class QuotaRow
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int ClassGroupId { get; set; }
    public ClassGroup ClassGroup { get; set; } = null!;
    public int TotalSessions { get; set; }
    public int RemainingSessions { get; set; }
    public decimal Price { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}