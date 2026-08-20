namespace TutoringHub.Application.DTOs.Quizzes;

public class QuizScoreDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool HasTaken { get; set; }
    public int? FirstAttemptCorrect { get; set; }
    public int? FirstAttemptTotal { get; set; }
    public DateTime? FirstAttemptAtUtc { get; set; }
    public int AttemptCount { get; set; }
}