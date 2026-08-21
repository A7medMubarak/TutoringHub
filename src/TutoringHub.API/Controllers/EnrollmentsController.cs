using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutoringHub.Application.DTOs.Enrollments;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/classes/{classGroupId:int}/enrollments")]
[Authorize(Roles = "Teacher")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    public async Task<ActionResult<EnrollmentDto>> Enroll(int classGroupId, [FromBody] EnrollStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.EnrollAsync(classGroupId, request.StudentId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{studentId:int}")]
    public async Task<IActionResult> Unenroll(int classGroupId, int studentId, CancellationToken cancellationToken)
    {
        await _enrollmentService.UnenrollAsync(classGroupId, studentId, cancellationToken);
        return NoContent();
    }
}