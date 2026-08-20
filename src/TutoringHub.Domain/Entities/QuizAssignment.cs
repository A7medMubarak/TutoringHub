namespace TutoringHub.Domain.Entities;

public class QuizAssignment
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    public int ClassGroupId { get; set; }
    public ClassGroup ClassGroup { get; set; } = null!;
}