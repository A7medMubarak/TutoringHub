using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutoringHub.Application.DTOs.Attendance;
using TutoringHub.Application.DTOs.Classes;
using TutoringHub.Application.DTOs.Quotas;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/me")]
[Authorize(Roles = "Student")]
public class MeController : ControllerBase
{
    private readonly IMeService _meService;

    public MeController(IMeService meService)
    {
        _meService = meService;
    }

    [HttpGet("classes")]
    public async Task<ActionResult<List<ClassGroupDto>>> GetClasses(CancellationToken cancellationToken)
    {
        var result = await _meService.GetClassesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<List<MyAttendanceDto>>> GetAttendance(CancellationToken cancellationToken)
    {
        var result = await _meService.GetAttendanceAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("quotas")]
    public async Task<ActionResult<List<QuotaDto>>> GetQuotas(CancellationToken cancellationToken)
    {
        var result = await _meService.GetQuotasAsync(cancellationToken);
        return Ok(result);
    }
}