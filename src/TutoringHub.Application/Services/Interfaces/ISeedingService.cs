namespace TutoringHub.Application.Services.Interfaces;

public interface ISeedingService
{
    Task SeedDefaultTeacherAsync(string defaultAdminPassword, CancellationToken cancellationToken = default);
    Task SeedDemoDataAsync(CancellationToken cancellationToken = default);
}