using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetroQualityMonitor.Web.Controllers;

[ApiController]
[Route("api/vestibules")]
public class VestibulesController(IVestibuleService vestibuleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<VestibuleDto>>> GetAllAsync(
        [FromQuery] short? stationId,
        CancellationToken cancellationToken)
    {
        var result = await vestibuleService.GetAllAsync(stationId, cancellationToken);
        return Ok(result);
    }
}
