namespace TutoringHub.Application.DTOs.Quizzes;

public class GradedQuestionDto
{
    public string Question { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int? YourIndex { get; set; }
    public bool? YourAnswer { get; set; }
    public int? CorrectIndex { get; set; }
    public bool? CorrectAnswer { get; set; }
}