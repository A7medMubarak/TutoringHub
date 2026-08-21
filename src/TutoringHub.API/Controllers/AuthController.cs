using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TutoringHub.Application.DTOs.Auth;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("teachers/register")]
    public async Task<ActionResult<AuthResponse>> RegisterTeacher([FromBody] RegisterTeacherRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterTeacherAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("teachers/login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> LoginTeacher([FromBody] TeacherLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginTeacherAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("students/login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> LoginStudent([FromBody] StudentLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginStudentAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);
        return NoContent();
    }
}