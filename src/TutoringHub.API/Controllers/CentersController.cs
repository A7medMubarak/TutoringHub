using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutoringHub.Application.DTOs.Centers;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/centers")]
[Authorize(Roles = "Teacher")]
public class CentersController : ControllerBase
{
    private readonly ICenterService _centerService;

    public CentersController(ICenterService centerService)
    {
        _centerService = centerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CenterDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _centerService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CenterDto>> Create([FromBody] CreateCenterRequest request, CancellationToken cancellationToken)
    {
        var result = await _centerService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }
}