namespace TutoringHub.Domain.Entities;

public class QuizAttempt
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
}