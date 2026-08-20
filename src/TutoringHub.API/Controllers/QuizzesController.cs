using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutoringHub.Application.DTOs.Quizzes;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/quizzes")]
[Authorize(Roles = "Teacher")]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizzesController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuizDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _quizService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{quizId:int}")]
    public async Task<ActionResult<QuizDetailDto>> GetById(int quizId, CancellationToken cancellationToken)
    {
        var result = await _quizService.GetDetailAsync(quizId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{quizId:int}/results")]
    public async Task<ActionResult<List<QuizScoreDto>>> GetResults(int quizId, CancellationToken cancellationToken)
    {
        var result = await _quizService.GetResultsAsync(quizId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<QuizDetailDto>> Create([FromBody] CreateQuizRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{quizId:int}/publish")]
    public async Task<ActionResult<QuizDetailDto>> Publish(int quizId, [FromBody] PublishQuizRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.PublishAsync(quizId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{quizId:int}/unpublish/{classGroupId:int}")]
    public async Task<ActionResult<QuizDetailDto>> Unpublish(int quizId, int classGroupId, CancellationToken cancellationToken)
    {
        var result = await _quizService.UnpublishAsync(quizId, classGroupId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{quizId:int}")]
    public async Task<IActionResult> Delete(int quizId, CancellationToken cancellationToken)
    {
        await _quizService.DeleteAsync(quizId, cancellationToken);
        return NoContent();
    }
}