using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TutoringHub.Application;
using TutoringHub.Application.Common.Options;
using TutoringHub.Application.DTOs.Ai;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.Infrastructure.External;

public class GeminiAiClient : IAiClient
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;

    public GeminiAiClient(HttpClient httpClient, IOptions<AiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Ai:ApiKey is not configured.");
    }

    public async Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            system_instruction = new { parts = new[] { new { text = request.SystemPrompt } } },
            contents = new[]
            {
                new
                {
                    parts = BuildParts(request)
                }
            },
            generation_config = new
            {
                response_mime_type = request.JsonMode ? "application/json" : "text/plain",
                max_output_tokens = _options.MaxOutputTokens
            }
        };

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var url = $"{_options.BaseUrl.TrimEnd('/')}/models/{_options.Model}:generateContent" +
                      $"?key={Uri.EscapeDataString(_options.ApiKey)}";

            using var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);
                var part = body?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault();

                if (part?.Text is null or "")
                    throw new AiClientException("The AI service returned an empty response. Please try again.");

                return new AiGenerationResult
                {
                    Text = part.Text,
                    FinishReason = body!.Candidates![0].FinishReason
                };
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)response.StatusCode;

            if ((statusCode is 429 or 503) && attempt == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                continue;
            }

            throw new AiClientException(MapErrorMessage(statusCode, errorBody));
        }

        throw new AiClientException("The AI service is unavailable. Please try again.");
    }

    private static object[] BuildParts(AiGenerationRequest request)
    {
        var parts = new List<object> { new { text = request.Prompt } };

        if (request.Attachment is not null && !string.IsNullOrWhiteSpace(request.MimeType))
        {
            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = request.MimeType,
                    data = Convert.ToBase64String(request.Attachment)
                }
            });
        }

        return parts.ToArray();
    }

    private static string MapErrorMessage(int statusCode, string errorBody)
    {
        var detail = ExtractErrorMessage(errorBody);

        return statusCode switch
        {
            400 => $"The AI service rejected the request. {detail}".TrimEnd(),
            401 or 403 => "The AI service rejected the API key. Check the Ai:ApiKey setting.",
            404 => "The AI model was not found. Check the Ai:Model setting.",
            429 => "The AI service rate limit was reached. Please try again in a minute.",
            >= 500 => "The AI service is temporarily unavailable. Please try again later.",
            _ => $"The AI service failed with status {statusCode}. {detail}".TrimEnd()
        };
    }

    private static string ExtractErrorMessage(string errorBody)
    {
        try
        {
            using var document = JsonDocument.Parse(errorBody);
            return document.RootElement
                .GetProperty("error")
                .GetProperty("message")
                .GetString() ?? string.Empty;
        }
        catch
        {
            return errorBody.Length > 200 ? errorBody[..200] : errorBody;
        }
    }

    private sealed class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
        public string? FinishReason { get; set; }
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        public string? Text { get; set; }
    }
}