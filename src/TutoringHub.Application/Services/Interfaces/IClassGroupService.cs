using TutoringHub.Application.DTOs.Classes;

namespace TutoringHub.Application.Services.Interfaces;

public interface IClassGroupService
{
    Task<ClassGroupDto> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default);
    Task<List<ClassGroupDto>> GetAllAsync(CancellationToken cancellationToken = default);
}