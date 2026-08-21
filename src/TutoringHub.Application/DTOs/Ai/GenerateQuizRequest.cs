using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.DTOs.Ai;

public class GenerateQuizRequest
{
    public string Topic { get; set; } = string.Empty;
    public int QuestionCount { get; set; } = 10;
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
}