namespace TutoringHub.Application.DTOs.Ai;

public class AiGenerationResult
{
    public string Text { get; set; } = string.Empty;
    public string? FinishReason { get; set; }
}