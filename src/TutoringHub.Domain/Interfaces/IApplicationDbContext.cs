using Microsoft.EntityFrameworkCore;
using TutoringHub.Domain.Entities;

namespace TutoringHub.Domain.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Teacher> Teachers { get; }
    DbSet<Student> Students { get; }
    DbSet<Center> Centers { get; }
    DbSet<ClassGroup> ClassGroups { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<ClassSession> ClassSessions { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}