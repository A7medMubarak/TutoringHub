namespace TutoringHub.Application.DTOs.Quotas;

public class CreateQuotaRequest
{
    public int ClassGroupId { get; set; }
    public int TotalSessions { get; set; }
    public decimal Price { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}