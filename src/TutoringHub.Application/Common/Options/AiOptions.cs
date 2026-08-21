namespace TutoringHub.Application.Common.Options;

public class AiOptions
{
    public const string SectionName = "Ai";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.6-flash";
    public int MaxOutputTokens { get; set; } = 4096;
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
}