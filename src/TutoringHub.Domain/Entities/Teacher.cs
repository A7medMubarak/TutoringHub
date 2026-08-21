namespace TutoringHub.Domain.Entities;

public class Teacher
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Center> Centers { get; set; } = new List<Center>();
    public ICollection<ClassGroup> ClassGroups { get; set; } = new List<ClassGroup>();
}