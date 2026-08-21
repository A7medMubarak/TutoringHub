using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Classes;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class ClassGroupService : IClassGroupService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ClassGroupService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ClassGroupDto> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        var teacherId = _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can create classes.");

        if (!await _context.Centers.AnyAsync(c => c.Id == request.CenterId && c.TeacherId == teacherId, cancellationToken))
            throw new ArgumentException("Center not found.");

        var classGroup = new ClassGroup
        {
            Name = request.Name.Trim(),
            TeacherId = teacherId,
            CenterId = request.CenterId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            Frequency = request.Frequency
        };

        _context.ClassGroups.Add(classGroup);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(classGroup, request);
    }

    public async Task<List<ClassGroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var teacherId = _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can view classes.");

        var classes = await _context.ClassGroups
            .AsNoTracking()
            .Include(c => c.Center)
            .Include(c => c.Enrollments)
            .Where(c => c.TeacherId == teacherId)
            .OrderBy(c => c.DayOfWeek)
            .ThenBy(c => c.StartTime)
            .ToListAsync(cancellationToken);

        return classes
            .Select(c => new ClassGroupDto
            {
                Id = c.Id,
                Name = c.Name,
                CenterId = c.CenterId,
                CenterName = c.Center?.Name ?? string.Empty,
                DayOfWeek = c.DayOfWeek,
                StartTime = c.StartTime,
                Frequency = c.Frequency,
                IsActive = c.IsActive,
                StudentCount = c.Enrollments.Count
            })
            .ToList();
    }

    private static ClassGroupDto MapToDto(ClassGroup classGroup, CreateClassRequest request)
    {
        return new ClassGroupDto
        {
            Id = classGroup.Id,
            Name = classGroup.Name,
            CenterId = classGroup.CenterId,
            DayOfWeek = classGroup.DayOfWeek,
            StartTime = classGroup.StartTime,
            Frequency = classGroup.Frequency,
            IsActive = classGroup.IsActive,
            StudentCount = 0
        };
    }
}