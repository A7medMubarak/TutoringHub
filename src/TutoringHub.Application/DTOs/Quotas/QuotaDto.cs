namespace TutoringHub.Application.DTOs.Quotas;

public class QuotaDto
{
    public int Id { get; set; }
    public int ClassGroupId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int RemainingSessions { get; set; }
    public decimal Price { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}