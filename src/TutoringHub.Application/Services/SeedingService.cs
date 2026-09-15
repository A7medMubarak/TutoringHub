using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Enums;
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

    public async Task SeedDemoDataAsync(CancellationToken cancellationToken = default)
    {
        var admin = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == "admin", cancellationToken);
        if (admin is null)
            return;
        if (await _context.Centers.AnyAsync(c => c.TeacherId == admin.Id, cancellationToken))
            return;
        if (await _context.Students.AnyAsync(s => s.TeacherId == admin.Id, cancellationToken))
            return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var center = new Center
        {
            Name = "المركز الرئيسي - Main Center",
            LocationDetails = "Cairo - 15 May",
            TeacherId = admin.Id
        };

        var groupA = new ClassGroup
        {
            Name = "الصف أ - Group A",
            TeacherId = admin.Id,
            Center = center,
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = new TimeSpan(16, 0, 0),
            Frequency = Frequency.OncePerWeek,
            IsActive = true
        };

        var groupB = new ClassGroup
        {
            Name = "المجموعة ب - Group B",
            TeacherId = admin.Id,
            Center = center,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(18, 0, 0),
            Frequency = Frequency.TwicePerWeek,
            IsActive = true
        };

        var students = new[]
        {
            new Student { FullName = "أحمد حسن - Ahmed Hassan", Phone = "01055557777", PinHash = BCrypt.Net.BCrypt.HashPassword("2468"), TeacherId = admin.Id },
            new Student { FullName = "سارة محمود - Sara Mahmoud", Phone = "01066667777", PinHash = BCrypt.Net.BCrypt.HashPassword("4242"), TeacherId = admin.Id },
            new Student { FullName = "Omar Khaled - عمر خالد", Phone = "01077778888", PinHash = BCrypt.Net.BCrypt.HashPassword("3579"), TeacherId = admin.Id },
            new Student { FullName = "ليلى إبراهيم - Layla Ibrahim", Phone = "01011112222", PinHash = BCrypt.Net.BCrypt.HashPassword("1122"), TeacherId = admin.Id },
            new Student { FullName = "Youssef Adel - يوسف عادل", Phone = "01098765432", PinHash = BCrypt.Net.BCrypt.HashPassword("7788"), TeacherId = admin.Id }
        };

        _context.Centers.Add(center);
        _context.ClassGroups.Add(groupA);
        _context.ClassGroups.Add(groupB);
        foreach (var student in students)
            _context.Students.Add(student);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var student in students)
            _context.Enrollments.Add(new Enrollment { Student = student, ClassGroup = groupA });
        _context.Enrollments.Add(new Enrollment { Student = students[0], ClassGroup = groupB });

        var periodStart = new DateOnly(today.Year, today.Month, 1);
        var quotaRows = students.Select(student => new QuotaRow
        {
            Student = student,
            ClassGroup = groupA,
            TotalSessions = 8,
            RemainingSessions = 7,
            Price = 400.00m,
            PeriodStart = periodStart,
            PeriodEnd = periodStart.AddMonths(1).AddDays(-1)
        }).ToList();
        foreach (var quotaRow in quotaRows)
            _context.QuotaRows.Add(quotaRow);

        var sessionA1 = new ClassSession { ClassGroup = groupA, Date = today.AddDays(-10) };
        var sessionA2 = new ClassSession { ClassGroup = groupA, Date = today.AddDays(-3) };
        var sessionB1 = new ClassSession { ClassGroup = groupB, Date = today.AddDays(-6) };
        var sessionB2 = new ClassSession { ClassGroup = groupB, Date = today.AddDays(2) };
        _context.ClassSessions.Add(sessionA1);
        _context.ClassSessions.Add(sessionA2);
        _context.ClassSessions.Add(sessionB1);
        _context.ClassSessions.Add(sessionB2);

        _context.Attendances.Add(new Attendance { ClassSession = sessionA1, Student = students[0], QuotaRow = quotaRows[0] });
        _context.Attendances.Add(new Attendance { ClassSession = sessionA1, Student = students[1], QuotaRow = quotaRows[1] });
        _context.Attendances.Add(new Attendance { ClassSession = sessionA2, Student = students[3] });
        _context.Attendances.Add(new Attendance { ClassSession = sessionB1, Student = students[3], QuotaRow = quotaRows[3] });

        await _context.SaveChangesAsync(cancellationToken);
    }
}