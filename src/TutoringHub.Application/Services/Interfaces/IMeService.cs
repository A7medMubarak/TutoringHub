using TutoringHub.Application.DTOs.Attendance;
using TutoringHub.Application.DTOs.Classes;
using TutoringHub.Application.DTOs.Quotas;

namespace TutoringHub.Application.Services.Interfaces;

public interface IMeService
{
    Task<List<ClassGroupDto>> GetClassesAsync(CancellationToken cancellationToken = default);
    Task<List<MyAttendanceDto>> GetAttendanceAsync(CancellationToken cancellationToken = default);
    Task<List<QuotaDto>> GetQuotasAsync(CancellationToken cancellationToken = default);
}