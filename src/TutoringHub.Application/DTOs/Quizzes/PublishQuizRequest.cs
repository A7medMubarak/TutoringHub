namespace TutoringHub.Application.DTOs.Quizzes;

public class PublishQuizRequest
{
    public List<int> ClassGroupIds { get; set; } = new();
}