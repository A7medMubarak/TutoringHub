using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Attendance;
using TutoringHub.Application.DTOs.Sessions;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AttendanceService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AttendanceRosterDto> GetRosterAsync(int classGroupId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var teacherId = await EnsureOwnedClassAsync(classGroupId, cancellationToken);

        var session = await _context.ClassSessions
            .AsNoTracking()
            .Include(s => s.Attendances)
            .ThenInclude(a => a.Student)
            .SingleOrDefaultAsync(s => s.ClassGroupId == classGroupId && s.Date == date, cancellationToken);

        var attendances = session?.Attendances ?? new List<Attendance>();
        var attendanceByStudentId = attendances.ToDictionary(a => a.StudentId);

        var enrolled = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Where(e => e.ClassGroupId == classGroupId)
            .OrderBy(e => e.Student.FullName)
            .ToListAsync(cancellationToken);

        var enrolledIds = enrolled.Select(e => e.StudentId).ToHashSet();

        var entries = enrolled
            .Select(e => new AttendanceEntryDto
            {
                StudentId = e.Student.Id,
                StudentName = e.Student.FullName,
                IsPresent = attendanceByStudentId.ContainsKey(e.StudentId),
                IsMakeUp = false
            })
            .ToList();

        entries.AddRange(attendances
            .Where(a => !enrolledIds.Contains(a.StudentId))
            .Select(a => new AttendanceEntryDto
            {
                StudentId = a.Student.Id,
                StudentName = a.Student.FullName,
                IsPresent = true,
                IsMakeUp = true
            }));

        return new AttendanceRosterDto
        {
            ClassGroupId = classGroupId,
            Date = date,
            SessionId = session?.Id,
            Entries = entries
        };
    }

    public async Task<AttendanceEntryDto> TickAsync(int classGroupId, int studentId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var teacherId = await EnsureOwnedClassAsync(classGroupId, cancellationToken);

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TeacherId == teacherId, cancellationToken)
            ?? throw new ArgumentException("Student not found.");

        var session = await GetOrCreateSessionAsync(classGroupId, date, cancellationToken);

        var existing = await _context.Attendances
            .SingleOrDefaultAsync(a => a.ClassSessionId == session.Id && a.StudentId == studentId, cancellationToken);

        if (existing is null)
        {
            _context.Attendances.Add(new Attendance { ClassSessionId = session.Id, StudentId = studentId });
            await _context.SaveChangesAsync(cancellationToken);
        }

        var isMakeUp = !await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.ClassGroupId == classGroupId, cancellationToken);

        return new AttendanceEntryDto
        {
            StudentId = student.Id,
            StudentName = student.FullName,
            IsPresent = true,
            IsMakeUp = isMakeUp
        };
    }

    public async Task UntickAsync(int classGroupId, int studentId, DateOnly date, CancellationToken cancellationToken = default)
    {
        await EnsureOwnedClassAsync(classGroupId, cancellationToken);

        var session = await _context.ClassSessions
            .SingleOrDefaultAsync(s => s.ClassGroupId == classGroupId && s.Date == date, cancellationToken);

        if (session is null)
            return;

        var attendance = await _context.Attendances
            .SingleOrDefaultAsync(a => a.ClassSessionId == session.Id && a.StudentId == studentId, cancellationToken);

        if (attendance is not null)
        {
            if (attendance.QuotaRowId.HasValue)
            {
                var quota = await _context.QuotaRows
                    .SingleOrDefaultAsync(q => q.Id == attendance.QuotaRowId.Value, cancellationToken);

                if (quota is not null)
                    quota.RemainingSessions++;
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<ClassSessionDto>> GetSessionsAsync(int classGroupId, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        await EnsureOwnedClassAsync(classGroupId, cancellationToken);

        var query = _context.ClassSessions
            .AsNoTracking()
            .Include(s => s.Attendances)
            .Where(s => s.ClassGroupId == classGroupId);

        if (fromDate.HasValue)
            query = query.Where(s => s.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.Date <= toDate.Value);

        var sessions = await query
            .OrderBy(s => s.Date)
            .ToListAsync(cancellationToken);

        return sessions
            .Select(s => new ClassSessionDto
            {
                Id = s.Id,
                ClassGroupId = s.ClassGroupId,
                Date = s.Date,
                PresentCount = s.Attendances.Count
            })
            .ToList();
    }

    private async Task<int> EnsureOwnedClassAsync(int classGroupId, CancellationToken cancellationToken)
    {
        var teacherId = _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can manage attendance.");

        if (!await _context.ClassGroups.AnyAsync(c => c.Id == classGroupId && c.TeacherId == teacherId, cancellationToken))
            throw new ArgumentException("Class not found.");

        return teacherId;
    }

    private async Task<ClassSession> GetOrCreateSessionAsync(int classGroupId, DateOnly date, CancellationToken cancellationToken)
    {
        var session = await _context.ClassSessions
            .SingleOrDefaultAsync(s => s.ClassGroupId == classGroupId && s.Date == date, cancellationToken);

        if (session is not null)
            return session;

        session = new ClassSession { ClassGroupId = classGroupId, Date = date };
        _context.ClassSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return session;
    }
}