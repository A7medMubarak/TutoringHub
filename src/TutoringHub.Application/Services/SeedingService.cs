using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class SeedingService : ISeedingService
{
    private readonly IApplicationDbContext _context;

    public SeedingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedDefaultTeacherAsync(string defaultAdminPassword, CancellationToken cancellationToken = default)
    {
        if (await _context.Teachers.AnyAsync(cancellationToken))
            return;

        _context.Teachers.Add(new Teacher
        {
            FullName = "Admin Teacher",
            Username = "admin",
            Phone = "0000000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultAdminPassword)
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}