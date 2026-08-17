using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutoringHub.Application.DTOs.Quotas;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.API.Controllers;

[ApiController]
[Route("api/students/{studentId:int}/quotas")]
[Authorize(Roles = "Teacher")]
public class QuotasController : ControllerBase
{
    private readonly IQuotaService _quotaService;

    public QuotasController(IQuotaService quotaService)
    {
        _quotaService = quotaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuotaDto>>> GetForStudent(int studentId, CancellationToken cancellationToken)
    {
        var result = await _quotaService.GetForStudentAsync(studentId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<QuotaDto>> Create(int studentId, [FromBody] CreateQuotaRequest request, CancellationToken cancellationToken)
    {
        var result = await _quotaService.CreateAsync(studentId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{quotaId:int}/pay")]
    public async Task<ActionResult<QuotaPaymentDto>> Pay(int studentId, int quotaId, CancellationToken cancellationToken)
    {
        var result = await _quotaService.PayAsync(studentId, quotaId, cancellationToken);
        return Ok(result);
    }
}