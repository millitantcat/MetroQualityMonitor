using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using MetroQualityMonitor.Domain.Analytics.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MetroQualityMonitor.Web.Controllers;

[ApiController]
[Route("api/anomalies")]
public class AnomaliesController(IAnomalyService anomalyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AnomalyDto>>> GetAllAsync(
        [FromQuery] AnomalySeverities? severity,
        [FromQuery] bool? isAcknowledged,
        CancellationToken cancellationToken)
    {
        var result = await anomalyService.GetAllAsync(severity, isAcknowledged, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAsync(Guid id, CancellationToken cancellationToken)
    {
        var found = await anomalyService.AcknowledgeAsync(id, cancellationToken);
        return found ? NoContent() : NotFound();
    }
}
