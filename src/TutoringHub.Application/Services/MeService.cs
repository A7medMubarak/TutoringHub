using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Attendance;
using TutoringHub.Application.DTOs.Classes;
using TutoringHub.Application.DTOs.Quotas;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class MeService : IMeService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MeService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ClassGroupDto>> GetClassesAsync(CancellationToken cancellationToken = default)
    {
        var studentId = EnsureStudent();

        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => new ClassGroupDto
            {
                Id = e.ClassGroup.Id,
                Name = e.ClassGroup.Name,
                CenterId = e.ClassGroup.CenterId,
                CenterName = e.ClassGroup.Center.Name,
                DayOfWeek = e.ClassGroup.DayOfWeek,
                StartTime = e.ClassGroup.StartTime,
                Frequency = e.ClassGroup.Frequency,
                IsActive = e.ClassGroup.IsActive,
                StudentCount = e.ClassGroup.Enrollments.Count
            })
            .OrderBy(c => c.DayOfWeek)
            .ThenBy(c => c.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MyAttendanceDto>> GetAttendanceAsync(CancellationToken cancellationToken = default)
    {
        var studentId = EnsureStudent();

        var ownClassIds = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.ClassGroupId)
            .ToListAsync(cancellationToken);

        var attendances = await _context.Attendances
            .AsNoTracking()
            .Include(a => a.ClassSession)
            .ThenInclude(s => s.ClassGroup)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.ClassSession.Date)
            .ThenByDescending(a => a.ClassSessionId)
            .ToListAsync(cancellationToken);

        return attendances
            .Select(a => new MyAttendanceDto
            {
                ClassGroupId = a.ClassSession.ClassGroupId,
                ClassName = a.ClassSession.ClassGroup.Name,
                Date = a.ClassSession.Date,
                IsMakeUp = !ownClassIds.Contains(a.ClassSession.ClassGroupId)
            })
            .ToList();
    }

    public async Task<List<QuotaDto>> GetQuotasAsync(CancellationToken cancellationToken = default)
    {
        var studentId = EnsureStudent();

        return await _context.QuotaRows
            .AsNoTracking()
            .Include(q => q.ClassGroup)
            .Where(q => q.StudentId == studentId)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuotaDto
            {
                Id = q.Id,
                ClassGroupId = q.ClassGroupId,
                ClassName = q.ClassGroup.Name,
                TotalSessions = q.TotalSessions,
                RemainingSessions = q.RemainingSessions,
                Price = q.Price,
                PaidAt = q.PaidAt,
                PeriodStart = q.PeriodStart,
                PeriodEnd = q.PeriodEnd
            })
            .ToListAsync(cancellationToken);
    }

    private int EnsureStudent()
    {
        if (_currentUser.Role != Role.Student)
            throw new UnauthorizedAccessException("Only students can access this resource.");

        return _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Only students can access this resource.");
    }
}