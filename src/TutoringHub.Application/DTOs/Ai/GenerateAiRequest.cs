namespace TutoringHub.Application.DTOs.Ai;

public class GenerateAiRequest
{
    public string Prompt { get; set; } = string.Empty;
    public bool JsonMode { get; set; }
}