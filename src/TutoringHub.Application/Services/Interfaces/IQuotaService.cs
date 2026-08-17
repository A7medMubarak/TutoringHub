using TutoringHub.Application.DTOs.Quotas;

namespace TutoringHub.Application.Services.Interfaces;

public interface IQuotaService
{
    Task<QuotaDto> CreateAsync(int studentId, CreateQuotaRequest request, CancellationToken cancellationToken = default);
    Task<List<QuotaDto>> GetForStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<QuotaPaymentDto> PayAsync(int studentId, int quotaId, CancellationToken cancellationToken = default);
}