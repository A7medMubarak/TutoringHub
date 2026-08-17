using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Centers;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Entities;
using TutoringHub.Domain.Interfaces;

namespace TutoringHub.Application.Services;

public class CenterService : ICenterService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CenterService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CenterDto> CreateAsync(CreateCenterRequest request, CancellationToken cancellationToken = default)
    {
        var teacherId = _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can create centers.");

        var center = new Center
        {
            Name = request.Name.Trim(),
            LocationDetails = request.LocationDetails.Trim(),
            TeacherId = teacherId
        };

        _context.Centers.Add(center);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(center);
    }

    public async Task<List<CenterDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var teacherId = _currentUser.TeacherId
            ?? throw new UnauthorizedAccessException("Only teachers can view centers.");

        return await _context.Centers
            .AsNoTracking()
            .Where(c => c.TeacherId == teacherId)
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync(cancellationToken);
    }

    private static CenterDto MapToDto(Center center)
    {
        return new CenterDto
        {
            Id = center.Id,
            Name = center.Name,
            LocationDetails = center.LocationDetails
        };
    }
}