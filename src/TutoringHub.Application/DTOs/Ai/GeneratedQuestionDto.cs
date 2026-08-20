namespace TutoringHub.Application.DTOs.Ai;

public class GeneratedQuestionDto
{
    public string Question { get; set; } = string.Empty;
    public List<string>? Options { get; set; }
    public int? CorrectIndex { get; set; }
    public bool? IsTrue { get; set; }
}