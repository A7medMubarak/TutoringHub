namespace TutoringHub.Application.DTOs.Quotas;

public class QuotaPaymentDto
{
    public int QuotaId { get; set; }
    public int CoveredCount { get; set; }
    public int RemainingUnpaidCount { get; set; }
}