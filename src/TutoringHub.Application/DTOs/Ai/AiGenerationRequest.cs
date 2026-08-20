namespace TutoringHub.Application.DTOs.Ai;

public class AiGenerationRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public byte[]? Attachment { get; set; }
    public string? MimeType { get; set; }
    public bool JsonMode { get; set; }
}