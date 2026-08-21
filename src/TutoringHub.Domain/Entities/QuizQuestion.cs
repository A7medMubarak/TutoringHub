namespace TutoringHub.Domain.Entities;

public class QuizQuestion
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public int SortOrder { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty;
    public int? CorrectIndex { get; set; }
    public bool? IsTrue { get; set; }
}