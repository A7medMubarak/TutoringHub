namespace TutoringHub.Domain.Entities;

public class Center
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LocationDetails { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;
    public ICollection<ClassGroup> ClassGroups { get; set; } = new List<ClassGroup>();
}