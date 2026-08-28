using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutoringHub.Application.DTOs.Auth;
using TutoringHub.Domain.Enums;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthMeController : ControllerBase
{
    private readonly TutoringHub.Domain.Interfaces.IApplicationDbContext _context;

    public AuthMeController(TutoringHub.Domain.Interfaces.IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserResponse>> GetMe(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirstValue(ClaimTypes.Role);

        if (userIdClaim is null || roleClaim is null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        if (roleClaim == Role.Teacher.ToString())
        {
            var teacher = await _context.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == userId, cancellationToken);

            if (teacher is null)
                return NotFound();

            return Ok(new CurrentUserResponse
            {
                UserId = teacher.Id,
                Role = Role.Teacher.ToString(),
                Name = teacher.FullName,
                TeacherId = teacher.Id
            });
        }

        if (roleClaim == Role.Student.ToString())
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);

            if (student is null)
                return NotFound();

            return Ok(new CurrentUserResponse
            {
                UserId = student.Id,
                Role = Role.Student.ToString(),
                Name = student.FullName,
                TeacherId = student.TeacherId
            });
        }

        return Unauthorized();
    }
}
