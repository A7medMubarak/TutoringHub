namespace TutoringHub.Domain.Entities;

public class Attendance
{
    public int Id { get; set; }
    public int ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int? QuotaRowId { get; set; }
    public QuotaRow? QuotaRow { get; set; }
}