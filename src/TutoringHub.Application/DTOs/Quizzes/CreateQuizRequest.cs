using TutoringHub.Application.DTOs.Ai;

namespace TutoringHub.Application.DTOs.Quizzes;

public class CreateQuizRequest
{
    public string Title { get; set; } = string.Empty;
    public List<GeneratedQuestionDto> Questions { get; set; } = new();
}