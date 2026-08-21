using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.DTOs.Quizzes;

public class StudentQuizSummaryDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public int QuestionCount { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public int? LastCorrectCount { get; set; }
    public int? LastTotalCount { get; set; }
    public bool Taken => AttemptCount > 0;
}