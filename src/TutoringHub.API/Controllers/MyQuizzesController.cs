using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutoringHub.Application.DTOs.Quizzes;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/me/quizzes")]
[Authorize(Roles = "Student")]
public class MyQuizzesController : ControllerBase
{
    private readonly IStudentQuizService _studentQuizService;

    public MyQuizzesController(IStudentQuizService studentQuizService)
    {
        _studentQuizService = studentQuizService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StudentQuizSummaryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _studentQuizService.ListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{quizId:int}")]
    public async Task<ActionResult<StudentQuizDto>> GetById(int quizId, CancellationToken cancellationToken)
    {
        var result = await _studentQuizService.GetAsync(quizId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{quizId:int}/attempts")]
    public async Task<ActionResult<AttemptResultDto>> Submit(int quizId, [FromBody] SubmitAttemptRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentQuizService.SubmitAsync(quizId, request, cancellationToken);
        return Ok(result);
    }
}