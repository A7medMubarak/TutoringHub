using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutoringHub.Application.DTOs.Attendance;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/classes/{classGroupId:int}/attendance")]
[Authorize(Roles = "Teacher")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<ActionResult<AttendanceRosterDto>> GetRoster(int classGroupId, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetRosterAsync(classGroupId, date, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceEntryDto>> Tick(int classGroupId, [FromBody] TickAttendanceRequest request, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.TickAsync(classGroupId, request.StudentId, request.Date, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{studentId:int}")]
    public async Task<IActionResult> Untick(int classGroupId, int studentId, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        await _attendanceService.UntickAsync(classGroupId, studentId, date, cancellationToken);
        return NoContent();
    }
}