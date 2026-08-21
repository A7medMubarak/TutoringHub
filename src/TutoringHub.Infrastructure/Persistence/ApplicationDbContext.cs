using Microsoft.EntityFrameworkCore;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Center> Centers => Set<Center>();
    public DbSet<ClassGroup> ClassGroups => Set<ClassGroup>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<QuotaRow> QuotaRows => Set<QuotaRow>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizAssignment> QuizAssignments => Set<QuizAssignment>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}