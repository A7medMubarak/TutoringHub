using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutoringHub.Application.DTOs.Sessions;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/classes/{classGroupId:int}/sessions")]
[Authorize(Roles = "Teacher")]
public class SessionsController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public SessionsController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClassSessionDto>>> GetSessions(
        int classGroupId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetSessionsAsync(classGroupId, fromDate, toDate, cancellationToken);
        return Ok(result);
    }
}