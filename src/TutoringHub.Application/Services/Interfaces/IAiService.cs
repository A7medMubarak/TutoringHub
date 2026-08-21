using TutoringHub.Application.DTOs.Ai;

namespace TutoringHub.Application.Services.Interfaces;

public interface IAiService
{
    Task<AiGenerationResult> GenerateAsync(GenerateAiRequest request, byte[]? attachment, string? mimeType, CancellationToken cancellationToken = default);
    Task<GeneratedQuizDto> GenerateQuizAsync(GenerateQuizRequest request, byte[]? attachment, string? mimeType, CancellationToken cancellationToken = default);
}