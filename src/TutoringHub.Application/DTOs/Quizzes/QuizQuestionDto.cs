namespace TutoringHub.Application.DTOs.Quizzes;

public class QuizQuestionDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int? CorrectIndex { get; set; }
    public bool? IsTrue { get; set; }
}