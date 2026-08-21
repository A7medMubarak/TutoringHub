using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Enrollments;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public EnrollmentService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<EnrollmentDto> EnrollAsync(int classGroupId, int studentId, CancellationToken cancellationToken = default)
    {
        var teacherId = await EnsureOwnedClassAsync(classGroupId, cancellationToken);

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TeacherId == teacherId, cancellationToken)
            ?? throw new ArgumentException("Student not found.");

        if (await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.ClassGroupId == classGroupId, cancellationToken))
            throw new ArgumentException("Student is already enrolled.");

        var enrollment = new Enrollment { StudentId = studentId, ClassGroupId = classGroupId };
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        return new EnrollmentDto
        {
            StudentId = student.Id,
            StudentName = student.FullName,
            ClassGroupId = classGroupId,
            EnrolledAt = enrollment.EnrolledAt
        };
    }

    public async Task UnenrollAsync(int classGroupId, int studentId, CancellationToken cancellationToken = default)
    {
        await EnsureOwnedClassAsync(classGroupId, cancellationToken);

        var enrollment = await _context.Enrollments
            .SingleOrDefaultAsync(e => e.StudentId == studentId && e.ClassGroupId == classGroupId, cancellationToken);

        if (enrollment is not null)
        {
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<int> EnsureOwnedClassAsync(int classGroupId, CancellationToken cancellationToken)
    {
        var teacherId = _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can manage enrollments.");

        if (!await _context.ClassGroups.AnyAsync(c => c.Id == classGroupId && c.TeacherId == teacherId, cancellationToken))
            throw new ArgumentException("Class not found.");

        return teacherId;
    }
}