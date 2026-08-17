using TutoringHub.Application.DTOs.Centers;

namespace TutoringHub.Application.Services.Interfaces;

public interface ICenterService
{
    Task<CenterDto> CreateAsync(CreateCenterRequest request, CancellationToken cancellationToken = default);
    Task<List<CenterDto>> GetAllAsync(CancellationToken cancellationToken = default);
}