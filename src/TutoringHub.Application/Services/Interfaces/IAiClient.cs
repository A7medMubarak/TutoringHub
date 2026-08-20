using TutoringHub.Application.DTOs.Ai;

namespace TutoringHub.Application.Services.Interfaces;

public interface IAiClient
{
    Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default);
}