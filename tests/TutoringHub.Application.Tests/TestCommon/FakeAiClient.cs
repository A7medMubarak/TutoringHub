using TutoringHub.Application.DTOs.Ai;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.Application.Tests.TestCommon;

public class FakeAiClient : IAiClient
{
    public Func<AiGenerationRequest, AiGenerationResult>? Handler { get; set; }
    public List<AiGenerationRequest> Requests { get; } = new();

    public Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        Requests.Add(request);

        if (Handler is null)
            return Task.FromResult(new AiGenerationResult { Text = "[]", FinishReason = "STOP" });

        return Task.FromResult(Handler(request));
    }
}