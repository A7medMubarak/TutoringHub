using TutoringHub.Domain.Enums;

namespace TutoringHub.Domain.Entities;

public class Quiz
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAtUtc { get; set; }
    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    public ICollection<QuizAssignment> Assignments { get; set; } = new List<QuizAssignment>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}