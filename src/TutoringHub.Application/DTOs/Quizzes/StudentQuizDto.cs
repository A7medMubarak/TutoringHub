using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.DTOs.Quizzes;

public class StudentQuizDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public List<StudentQuizQuestionDto> Questions { get; set; } = new();
}