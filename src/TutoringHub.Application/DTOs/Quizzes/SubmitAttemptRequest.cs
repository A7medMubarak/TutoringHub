namespace TutoringHub.Application.DTOs.Quizzes;

public class SubmitAttemptRequest
{
    public List<AnswerItemDto> Answers { get; set; } = new();
}