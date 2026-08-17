using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Students;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class StudentService : IStudentService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public StudentService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var teacherId = _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can create students.");

        var phone = request.Phone.Trim();
        if (await _context.Students.AnyAsync(s => s.Phone == phone, cancellationToken))
            throw new ArgumentException("A student with this phone number already exists.");

        var student = new Student
        {
            FullName = request.FullName.Trim(),
            Phone = phone,
            PinHash = BCrypt.Net.BCrypt.HashPassword(request.Pin),
            TeacherId = teacherId
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(student);
    }

    public async Task<StudentDto> GetByIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var teacherId = _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can view students.");

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TeacherId == teacherId, cancellationToken);

        return student is null
            ? throw new KeyNotFoundException("Student not found.")
            : MapToDto(student);
    }

    public async Task<List<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var teacherId = _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can view students.");

        return await _context.Students
            .AsNoTracking()
            .Where(s => s.TeacherId == teacherId)
            .OrderBy(s => s.FullName)
            .Select(s => MapToDto(s))
            .ToListAsync(cancellationToken);
    }

    private static StudentDto MapToDto(Student student)
    {
        return new StudentDto
        {
            Id = student.Id,
            FullName = student.FullName,
            Phone = student.Phone,
            CreatedAt = student.CreatedAt
        };
    }
}