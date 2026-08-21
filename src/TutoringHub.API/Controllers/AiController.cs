using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TutoringHub.Application.DTOs.Ai;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize(Roles = "Teacher")]
[EnableRateLimiting("ai")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("generate")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<AiGenerationResult>> Generate(
        [FromForm] GenerateAiRequest request, IFormFile? file, CancellationToken cancellationToken)
    {
        var (bytes, mimeType) = await ReadFileAsync(file, cancellationToken);
        var result = await _aiService.GenerateAsync(request, bytes, mimeType, cancellationToken);
        return Ok(result);
    }

    [HttpPost("quiz")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<GeneratedQuizDto>> GenerateQuiz(
        [FromForm] GenerateQuizRequest request, IFormFile? file, CancellationToken cancellationToken)
    {
        var (bytes, mimeType) = await ReadFileAsync(file, cancellationToken);
        var result = await _aiService.GenerateQuizAsync(request, bytes, mimeType, cancellationToken);
        return Ok(result);
    }

    private static async Task<(byte[]? Bytes, string? MimeType)> ReadFileAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
            return (null, null);

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        return (stream.ToArray(), file.ContentType);
    }
}