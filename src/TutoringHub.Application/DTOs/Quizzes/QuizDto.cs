using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.DTOs.Quizzes;

public class QuizDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public int QuestionCount { get; set; }
    public int AttemptCount { get; set; }
    public List<string> PublishedClassNames { get; set; } = new();
}