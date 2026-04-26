using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetroQualityMonitor.Web.Controllers;

[ApiController]
[Route("api/lines")]
public class LinesController(ILineService lineService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LineDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await lineService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LineDetailsDto>> GetByIdAsync(short id, CancellationToken cancellationToken)
    {
        var result = await lineService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:int}/stations")]
    public async Task<ActionResult<IReadOnlyCollection<StationLiteDto>>> GetStationsAsync(
        short id, CancellationToken cancellationToken)
    {
        var result = await lineService.GetStationsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/flow")]
    public async Task<ActionResult<IReadOnlyCollection<FlowRecordDto>>> GetFlowAsync(
        short id,
        [FromQuery] int? fromYear,
        [FromQuery] int? toYear,
        CancellationToken cancellationToken)
    {
        var result = await lineService.GetFlowAsync(id, fromYear, toYear, cancellationToken);
        return Ok(result);
    }
}
