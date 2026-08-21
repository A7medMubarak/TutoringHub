using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutoringHub.Application.DTOs.Classes;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/classes")]
[Authorize(Roles = "Teacher")]
public class ClassesController : ControllerBase
{
    private readonly IClassGroupService _classGroupService;

    public ClassesController(IClassGroupService classGroupService)
    {
        _classGroupService = classGroupService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClassGroupDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _classGroupService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ClassGroupDto>> Create([FromBody] CreateClassRequest request, CancellationToken cancellationToken)
    {
        var result = await _classGroupService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }
}