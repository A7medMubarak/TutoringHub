using System.Text.Json;
using TutoringHub.Application.DTOs.Ai;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.Services;

public class AiService : IAiService
{
    private const int MaxAttachmentBytes = 10 * 1024 * 1024;
    private const string SystemPrompt =
        "You are a careful quiz generator for a tutoring center. " +
        "Respond only with valid JSON, no markdown, no surrounding text.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> AllowedMimeTypes = new()
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "text/plain"
    };

    private readonly IAiClient _aiClient;

    public AiService(IAiClient aiClient)
    {
        _aiClient = aiClient;
    }

    public async Task<AiGenerationResult> GenerateAsync(
        GenerateAiRequest request,
        byte[]? attachment,
        string? mimeType,
        CancellationToken cancellationToken = default)
    {
        ValidateAttachment(attachment, mimeType);

        return await _aiClient.GenerateAsync(new AiGenerationRequest
        {
            SystemPrompt = "You are a helpful assistant for a tutoring center.",
            Prompt = request.Prompt,
            Attachment = attachment,
            MimeType = mimeType,
            JsonMode = request.JsonMode
        }, cancellationToken);
    }

    public async Task<GeneratedQuizDto> GenerateQuizAsync(
        GenerateQuizRequest request,
        byte[]? attachment,
        string? mimeType,
        CancellationToken cancellationToken = default)
    {
        ValidateAttachment(attachment, mimeType);

        var prompt = request.QuestionType == QuestionType.TrueFalse
            ? BuildTrueFalsePrompt(request, attachment is not null)
            : BuildMultipleChoicePrompt(request, attachment is not null);

        var result = await _aiClient.GenerateAsync(new AiGenerationRequest
        {
            SystemPrompt = SystemPrompt,
            Prompt = prompt,
            Attachment = attachment,
            MimeType = mimeType,
            JsonMode = true
        }, cancellationToken);

        var questions = ParseQuestions(result.Text);

        if (questions.Count == 0)
            throw new AiClientException("The AI service returned no usable questions. Please try again.");

        return new GeneratedQuizDto { Questions = questions };
    }

    private static string BuildMultipleChoicePrompt(GenerateQuizRequest request, bool hasAttachment)
    {
        var attachmentHint = hasAttachment ? " Use the attached material as the primary source." : string.Empty;
        return $"Generate exactly {request.QuestionCount} multiple-choice questions about \"{request.Topic}\"." +
               $" Each question must have exactly 4 options and exactly one correct answer.{attachmentHint}" +
               " Return a JSON array only, no markdown: " +
               "[{\"question\":\"...\",\"options\":[\"a\",\"b\",\"c\",\"d\"],\"correctIndex\":0}]";
    }

    private static string BuildTrueFalsePrompt(GenerateQuizRequest request, bool hasAttachment)
    {
        var attachmentHint = hasAttachment ? " Use the attached material as the primary source." : string.Empty;
        return $"Generate exactly {request.QuestionCount} true/false questions about \"{request.Topic}\".{attachmentHint}" +
               " Return a JSON array only, no markdown: " +
               "[{\"question\":\"...\",\"isTrue\":true}]";
    }

    private static List<GeneratedQuestionDto> ParseQuestions(string text)
    {
        var content = StripCodeFence(text);

        List<GeneratedQuestionDto>? questions = null;

        try
        {
            questions = JsonSerializer.Deserialize<List<GeneratedQuestionDto>>(content, JsonOptions);
        }
        catch (JsonException)
        {
        }

        if (questions is null)
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                if (document.RootElement.TryGetProperty("questions", out var wrapper))
                    questions = wrapper.Deserialize<List<GeneratedQuestionDto>>(JsonOptions);
            }
            catch (JsonException)
            {
                return new List<GeneratedQuestionDto>();
            }
        }

        return questions?.Where(IsValid).ToList() ?? new List<GeneratedQuestionDto>();
    }

    private static bool IsValid(GeneratedQuestionDto question)
    {
        if (string.IsNullOrWhiteSpace(question.Question))
            return false;

        return question.Options is not null
            ? question.Options.Count == 4 &&
              question.Options.All(o => !string.IsNullOrWhiteSpace(o)) &&
              question.CorrectIndex is >= 0 and <= 3
            : question.IsTrue is not null;
    }

    private static string StripCodeFence(string text)
    {
        var trimmed = text.Trim();

        if (trimmed.StartsWith("```"))
        {
            var start = trimmed.IndexOf('\n');
            if (start >= 0)
                trimmed = trimmed[(start + 1)..];

            var end = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (end >= 0)
                trimmed = trimmed[..end];
        }

        return trimmed.Trim();
    }

    private static void ValidateAttachment(byte[]? attachment, string? mimeType)
    {
        if (attachment is null)
            return;

        if (attachment.Length > MaxAttachmentBytes)
            throw new ArgumentException("File is too large. Maximum size is 10 MB.");

        if (NormalizeMimeType(mimeType) is not { } normalized || !AllowedMimeTypes.Contains(normalized))
            throw new ArgumentException("Only PDF, JPG, PNG or TXT files are supported.");
    }

    private static string? NormalizeMimeType(string? mimeType)
    {
        return mimeType?.ToLowerInvariant() switch
        {
            "application/pdf" => "application/pdf",
            "image/jpeg" or "image/jpg" => "image/jpeg",
            "image/png" => "image/png",
            "text/plain" => "text/plain",
            _ => mimeType?.ToLowerInvariant()
        };
    }
}