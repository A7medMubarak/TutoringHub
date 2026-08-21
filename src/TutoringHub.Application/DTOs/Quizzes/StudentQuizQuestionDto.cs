namespace TutoringHub.Application.DTOs.Quizzes;

public class StudentQuizQuestionDto
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}