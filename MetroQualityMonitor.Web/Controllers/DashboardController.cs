using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetroQualityMonitor.Web.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("kpi")]
    public async Task<ActionResult<DashboardKpiDto>> GetKpiAsync(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetKpiAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-stations")]
    public async Task<ActionResult<IReadOnlyCollection<TopStationDto>>> GetTopStationsAsync(
        [FromQuery] int n = 10,
        [FromQuery] string metric = "incoming",
        CancellationToken cancellationToken = default)
    {
        var result = await dashboardService.GetTopStationsAsync(n, metric, cancellationToken);
        return Ok(result);
    }

    [HttpGet("seasonality")]
    public async Task<ActionResult<IReadOnlyCollection<SeasonalityPointDto>>> GetSeasonalityAsync(
        CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetSeasonalityAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("anomaly-stats")]
    public async Task<ActionResult<AnomalyStatsDto>> GetAnomalyStatsAsync(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetAnomalyStatsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("lines-flow")]
    public async Task<ActionResult<IReadOnlyCollection<LineFlowDto>>> GetLinesFlowAsync(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetLinesFlowAsync(cancellationToken);
        return Ok(result);
    }
}
