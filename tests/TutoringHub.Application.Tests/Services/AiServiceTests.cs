using FluentAssertions;
using TutoringHub.Application.DTOs.Ai;
using TutoringHub.Application.Services;
using TutoringHub.Application.Tests.TestCommon;
using TutoringHub.Domain.Enums;

namespace TutoringHub.Application.Tests.Services;

public class AiServiceTests
{
    private readonly FakeAiClient _fakeClient = new();
    private readonly AiService _service;

    public AiServiceTests()
    {
        _service = new AiService(_fakeClient);
    }

    [Fact]
    public async Task GenerateAsync_PlainPrompt_PassesThrough()
    {
        var result = await _service.GenerateAsync(
            new GenerateAiRequest { Prompt = "Explain gravity", JsonMode = false },
            null,
            null);

        result.Text.Should().Be("[]");
        _fakeClient.Requests.Should().HaveCount(1);

        var request = _fakeClient.Requests[0];
        request.Prompt.Should().Be("Explain gravity");
        request.JsonMode.Should().BeFalse();
        request.SystemPrompt.Should().NotBeNullOrWhiteSpace();
        request.Attachment.Should().BeNull();
    }

    [Fact]
    public async Task GenerateAsync_JsonMode_ForwardsFlag()
    {
        await _service.GenerateAsync(
            new GenerateAiRequest { Prompt = "List names as JSON", JsonMode = true },
            null,
            null);

        _fakeClient.Requests[0].JsonMode.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_WithAttachment_PassesBytesAndMimeThrough()
    {
        var bytes = new byte[] { 1, 2, 3 };

        await _service.GenerateAsync(
            new GenerateAiRequest { Prompt = "Summarize this file" },
            bytes,
            "application/pdf");

        var request = _fakeClient.Requests[0];
        request.Attachment.Should().BeSameAs(bytes);
        request.MimeType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GenerateAsync_FileLargerThanTenMb_Throws()
    {
        var tooBig = new byte[10 * 1024 * 1024 + 1];
        var action = () => _service.GenerateAsync(
            new GenerateAiRequest { Prompt = "Hello" },
            tooBig,
            "application/pdf");

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*10 MB*");

        _fakeClient.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateAsync_UnsupportedMimeType_Throws()
    {
        var action = () => _service.GenerateAsync(
            new GenerateAiRequest { Prompt = "Hello" },
            new byte[] { 1 },
            "application/octet-stream");

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*PDF, JPG, PNG or TXT*");

        _fakeClient.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateQuizAsync_MultipleChoice_ParsesQuestions()
    {
        _fakeClient.Handler = _ => new AiGenerationResult
        {
            Text = """
                [{"question":"What is 2+2?","options":["3","4","5","6"],"correctIndex":1},
                 {"question":"Is the sky blue?","options":["Yes","No","Maybe","Sometimes"],"correctIndex":0}]
                """,
            FinishReason = "STOP"
        };

        var result = await _service.GenerateQuizAsync(
            new GenerateQuizRequest { Topic = "Math basics", QuestionCount = 2, QuestionType = QuestionType.MultipleChoice },
            null,
            null);

        result.Questions.Should().HaveCount(2);
        result.Questions[0].Question.Should().Be("What is 2+2?");
        result.Questions[0].CorrectIndex.Should().Be(1);

        var request = _fakeClient.Requests[0];
        request.JsonMode.Should().BeTrue();
        request.Prompt.Should().Contain("Math basics");
        request.Prompt.Should().Contain("2");
    }

    [Fact]
    public async Task GenerateQuizAsync_TrueFalse_ParsesQuestions()
    {
        _fakeClient.Handler = _ => new AiGenerationResult
        {
            Text = """[{"question":"The Earth orbits the Sun.","isTrue":true}]""",
            FinishReason = "STOP"
        };

        var result = await _service.GenerateQuizAsync(
            new GenerateQuizRequest { Topic = "Astronomy", QuestionCount = 1, QuestionType = QuestionType.TrueFalse },
            null,
            null);

        result.Questions.Should().HaveCount(1);
        result.Questions[0].IsTrue.Should().BeTrue();

        var request = _fakeClient.Requests[0];
        request.JsonMode.Should().BeTrue();
        request.Prompt.Should().Contain("true/false");
        request.Prompt.Should().Contain("isTrue");
    }

    [Fact]
    public async Task GenerateQuizAsync_CodeFencedJson_Parses()
    {
        _fakeClient.Handler = _ => new AiGenerationResult
        {
            Text = "```json\r\n[{\"question\":\"Q?\",\"options\":[\"a\",\"b\",\"c\",\"d\"],\"correctIndex\":2}]\r\n```",
            FinishReason = "STOP"
        };

        var result = await _service.GenerateQuizAsync(
            new GenerateQuizRequest { Topic = "T", QuestionCount = 1 },
            null,
            null);

        result.Questions.Should().HaveCount(1);
        result.Questions[0].CorrectIndex.Should().Be(2);
    }

    [Fact]
    public async Task GenerateQuizAsync_WrappedJson_Parses()
    {
        _fakeClient.Handler = _ => new AiGenerationResult
        {
            Text = """{"questions":[{"question":"Q?","options":["a","b","c","d"],"correctIndex":3}]}""",
            FinishReason = "STOP"
        };

        var result = await _service.GenerateQuizAsync(
            new GenerateQuizRequest { Topic = "T", QuestionCount = 1 },
            null,
            null);

        result.Questions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateQuizAsync_WithAttachment_AddsHintToPrompt()
    {
        _fakeClient.Handler = _ => new AiGenerationResult
        {
            Text = """[{"question":"Q?","options":["a","b","c","d"],"correctIndex":0}]""",
            FinishReason = "STOP"
        };

        await _service.GenerateQuizAsync(
            new GenerateQuizRequest { Topic = "T", QuestionCount = 1 },
            new byte[] { 1, 2 },
            "text/plain");

        _fakeClient.Requests[0].Prompt.Should().Contain("attached material");
        _fakeClient.Requests[0].Attachment.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateQuizAsync_InvalidItems_AreDropped()
    {
        _fakeClient.Handler = _ => new AiGenerationResult
        {
            Text = """
                [{"question":"Good","options":["a","b","c","d"],"correctIndex":0},
                 {"question":"Only three options","options":["a","b","c"],"correctIndex":0},
                 {"question":"","options":["a","b","c","d"],"correctIndex":0},
                 {"question":"Broken","options":["a","b","c","d"],"correctIndex":9},
                 {"question":"No answer marker"}]
                """,
            FinishReason = "STOP"
        };

        var result = await _service.GenerateQuizAsync(
            new GenerateQuizRequest { Topic = "T", QuestionCount = 5 },
            null,
            null);

        result.Questions.Should().HaveCount(1);
        result.Questions[0].Question.Should().Be("Good");
    }

    [Fact]
    public async Task GenerateQuizAsync_NoUsableQuestions_Throws()
    {
        _fakeClient.Handler = _ => new AiGenerationResult
        {
            Text = "not json at all",
            FinishReason = "STOP"
        };

        var action = () => _service.GenerateQuizAsync(
            new GenerateQuizRequest { Topic = "T", QuestionCount = 3 },
            null,
            null);

        await action.Should().ThrowAsync<AiClientException>()
            .WithMessage("*no usable questions*");
    }

    [Fact]
    public async Task GenerateQuizAsync_HugeAttachment_ThrowsBeforeCallingClient()
    {
        var tooBig = new byte[10 * 1024 * 1024 + 1];
        var action = () => _service.GenerateQuizAsync(
            new GenerateQuizRequest { Topic = "T", QuestionCount = 1 },
            tooBig,
            "image/jpeg");

        await action.Should().ThrowAsync<ArgumentException>();

        _fakeClient.Requests.Should().BeEmpty();
    }
}