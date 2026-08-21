using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Quotas;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class QuotaService : IQuotaService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public QuotaService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<QuotaDto> CreateAsync(int studentId, CreateQuotaRequest request, CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        if (!await _context.Students.AnyAsync(s => s.Id == studentId && s.TeacherId == teacherId, cancellationToken))
            throw new ArgumentException("Student not found.");

        if (!await _context.ClassGroups.AnyAsync(c => c.Id == request.ClassGroupId && c.TeacherId == teacherId, cancellationToken))
            throw new ArgumentException("Class not found.");

        if (!await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.ClassGroupId == request.ClassGroupId, cancellationToken))
            throw new ArgumentException("Student is not enrolled in this class.");

        var quota = new QuotaRow
        {
            StudentId = studentId,
            ClassGroupId = request.ClassGroupId,
            TotalSessions = request.TotalSessions,
            RemainingSessions = request.TotalSessions,
            Price = request.Price,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd
        };

        _context.QuotaRows.Add(quota);
        await _context.SaveChangesAsync(cancellationToken);

        var className = await _context.ClassGroups
            .AsNoTracking()
            .Where(c => c.Id == request.ClassGroupId)
            .Select(c => c.Name)
            .SingleAsync(cancellationToken);

        var dto = MapToDto(quota);
        dto.ClassName = className;
        return dto;
    }

    public async Task<List<QuotaDto>> GetForStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        if (!await _context.Students.AnyAsync(s => s.Id == studentId && s.TeacherId == teacherId, cancellationToken))
            throw new ArgumentException("Student not found.");

        var quotas = await _context.QuotaRows
            .AsNoTracking()
            .Include(q => q.ClassGroup)
            .Where(q => q.StudentId == studentId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        return quotas.Select(q => MapToDto(q)).ToList();
    }

    public async Task<QuotaPaymentDto> PayAsync(int studentId, int quotaId, CancellationToken cancellationToken = default)
    {
        var teacherId = EnsureTeacher();

        if (!await _context.Students.AnyAsync(s => s.Id == studentId && s.TeacherId == teacherId, cancellationToken))
            throw new ArgumentException("Student not found.");

        var quota = await _context.QuotaRows
            .SingleOrDefaultAsync(q => q.Id == quotaId && q.StudentId == studentId, cancellationToken)
            ?? throw new ArgumentException("Quota not found.");

        if (quota.PaidAt is not null)
            throw new ArgumentException("Quota is already paid.");

        var unpaid = await _context.Attendances
            .Include(a => a.ClassSession)
            .Where(a => a.StudentId == studentId && a.QuotaRowId == null)
            .OrderBy(a => a.ClassSession.Date)
            .ThenBy(a => a.ClassSessionId)
            .ToListAsync(cancellationToken);

        var ownClassGroupIds = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.ClassGroupId)
            .ToListAsync(cancellationToken);

        var eligible = unpaid
            .Where(a => EffectiveClassGroupId(a, ownClassGroupIds) == quota.ClassGroupId)
            .Take(quota.TotalSessions)
            .ToList();

        quota.PaidAt = DateTime.UtcNow;
        quota.RemainingSessions = quota.TotalSessions - eligible.Count;

        foreach (var attendance in eligible)
            attendance.QuotaRowId = quota.Id;

        await _context.SaveChangesAsync(cancellationToken);

        return new QuotaPaymentDto
        {
            QuotaId = quota.Id,
            CoveredCount = eligible.Count,
            RemainingUnpaidCount = await _context.Attendances
                .CountAsync(a => a.StudentId == studentId && a.QuotaRowId == null, cancellationToken)
        };
    }

    private int EnsureTeacher()
    {
        return _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can manage quotas.");
    }

    private static int EffectiveClassGroupId(Attendance attendance, List<int> ownClassGroupIds)
    {
        var attendedClassGroupId = attendance.ClassSession.ClassGroupId;
        return ownClassGroupIds.Contains(attendedClassGroupId)
            ? attendedClassGroupId
            : ownClassGroupIds.FirstOrDefault();
    }

    private static QuotaDto MapToDto(QuotaRow quota)
    {
        return new QuotaDto
        {
            Id = quota.Id,
            ClassGroupId = quota.ClassGroupId,
            ClassName = quota.ClassGroup?.Name ?? string.Empty,
            TotalSessions = quota.TotalSessions,
            RemainingSessions = quota.RemainingSessions,
            Price = quota.Price,
            PaidAt = quota.PaidAt,
            PeriodStart = quota.PeriodStart,
            PeriodEnd = quota.PeriodEnd
        };
    }
}