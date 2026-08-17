namespace TutoringHub.Application.DTOs.Sessions;

public class ClassSessionDto
{
    public int Id { get; set; }
    public int ClassGroupId { get; set; }
    public DateOnly Date { get; set; }
    public int PresentCount { get; set; }
}