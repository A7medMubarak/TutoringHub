namespace TutoringHub.Application.DTOs.Quizzes;

public class AttemptResultDto
{
    public int QuizId { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public double Percentage { get; set; }
    public bool IsFirstAttempt { get; set; }
    public List<GradedQuestionDto> Results { get; set; } = new();
}