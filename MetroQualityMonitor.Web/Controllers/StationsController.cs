using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using MetroQualityMonitor.Domain.Analytics.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MetroQualityMonitor.Web.Controllers;

[ApiController]
[Route("api/stations")]
public class StationsController(IStationService stationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<StationLiteDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await stationService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StationDetailsDto>> GetByIdAsync(short id, CancellationToken cancellationToken)
    {
        var result = await stationService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:int}/flow")]
    public async Task<ActionResult<IReadOnlyCollection<FlowRecordDto>>> GetFlowAsync(
        short id,
        [FromQuery] int? fromYear,
        [FromQuery] int? toYear,
        CancellationToken cancellationToken)
    {
        var result = await stationService.GetFlowAsync(id, fromYear, toYear, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/forecast")]
    public async Task<ActionResult<IReadOnlyCollection<ForecastDto>>> GetForecastAsync(
        short id, CancellationToken cancellationToken)
    {
        var result = await stationService.GetForecastAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/hourly")]
    public async Task<ActionResult<HourlyHeatmapDto>> GetHourlyAsync(
        short id,
        [FromQuery] DayTypes dayType = DayTypes.Weekday,
        CancellationToken cancellationToken = default)
    {
        var result = await stationService.GetHourlyAsync(id, dayType, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:int}/anomalies")]
    public async Task<ActionResult<IReadOnlyCollection<AnomalyDto>>> GetAnomaliesAsync(
        short id, CancellationToken cancellationToken)
    {
        var result = await stationService.GetAnomaliesAsync(id, cancellationToken);
        return Ok(result);
    }
}
