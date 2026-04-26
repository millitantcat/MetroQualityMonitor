using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Infrastructure.Analytics.Services;

/// <summary>
/// Реализация сервиса данных для главного экрана (дашборд).
/// </summary>
public class DashboardService(MetroQualityMonitorDbContext db) : IDashboardService
{
    public async Task<DashboardKpiDto> GetKpiAsync(CancellationToken ct = default)
    {
        var latestPeriod = await db.PassengerFlowRecords
            .AsNoTracking()
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Select(r => new { r.Year, r.Quarter })
            .FirstOrDefaultAsync(ct);

        long totalPassengers = 0;
        if (latestPeriod is not null)
        {
            totalPassengers = await db.PassengerFlowRecords
                .Where(r => r.Year == latestPeriod.Year && r.Quarter == latestPeriod.Quarter)
                .SumAsync(r => (long)r.IncomingPassengers, ct);
        }

        var stationCount       = await db.Stations.CountAsync(ct);
        var activeAnomalyCount = await db.Anomalies.CountAsync(a => !a.IsAcknowledged, ct);
        var activeRepairCount  = await db.EscalatorRepairs.CountAsync(r => !r.IsDeleted, ct);

        return new DashboardKpiDto
        {
            TotalPassengersLastQuarter = totalPassengers,
            StationCount               = stationCount,
            ActiveAnomalyCount         = activeAnomalyCount,
            ActiveRepairCount          = activeRepairCount,
            LatestQuarter              = latestPeriod?.Quarter,
            LatestYear                 = latestPeriod?.Year,
        };
    }

    public async Task<IReadOnlyCollection<TopStationDto>> GetTopStationsAsync(
        int n, string metric, CancellationToken ct = default)
    {
        var latestPeriod = await db.PassengerFlowRecords
            .AsNoTracking()
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Select(r => new { r.Year, r.Quarter })
            .FirstOrDefaultAsync(ct);

        if (latestPeriod is null)
            return [];

        var isIncoming = !string.Equals(metric, "outgoing", StringComparison.OrdinalIgnoreCase);

        var records = await db.PassengerFlowRecords
            .AsNoTracking()
            .Where(r => r.StationId != null
                        && r.Year    == latestPeriod.Year
                        && r.Quarter == latestPeriod.Quarter)
            .Select(r => new
            {
                r.StationId,
                r.Station!.Name,
                Lines = r.Station!.Lines!.Select(l => l.Name).ToList(),
                Value = isIncoming ? (long)r.IncomingPassengers : (long)r.OutgoingPassengers,
            })
            .OrderByDescending(r => r.Value)
            .Take(n)
            .ToListAsync(ct);

        return records.Select(r => new TopStationDto
        {
            StationId   = r.StationId!.Value,
            StationName = r.Name,
            Lines       = r.Lines,
            Value       = r.Value,
        }).ToList();
    }

    public async Task<IReadOnlyCollection<SeasonalityPointDto>> GetSeasonalityAsync(CancellationToken ct = default)
    {
        return await db.PassengerFlowRecords
            .AsNoTracking()
            .GroupBy(r => new { r.Year, r.Quarter })
            .Select(g => new SeasonalityPointDto
            {
                Year          = g.Key.Year,
                Quarter       = g.Key.Quarter,
                TotalIncoming = g.Sum(r => (long)r.IncomingPassengers),
                TotalOutgoing = g.Sum(r => (long)r.OutgoingPassengers),
            })
            .OrderBy(p => p.Year)
            .ThenBy(p => p.Quarter)
            .ToListAsync(ct);
    }

    public async Task<AnomalyStatsDto> GetAnomalyStatsAsync(CancellationToken ct = default)
    {
        var anomalies = await db.Anomalies
            .AsNoTracking()
            .Where(a => !a.IsAcknowledged)
            .Select(a => new { a.Severity, a.AnomalyType })
            .ToListAsync(ct);

        var bySeverity = anomalies
            .GroupBy(a => a.Severity.ToString())
            .Select(g => new AnomalyCountItem { Label = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        var byType = anomalies
            .GroupBy(a => a.AnomalyType.ToString())
            .Select(g => new AnomalyCountItem { Label = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new AnomalyStatsDto
        {
            BySeverity  = bySeverity,
            ByType      = byType,
            TotalActive = anomalies.Count,
        };
    }

    public async Task<IReadOnlyCollection<LineFlowDto>> GetLinesFlowAsync(CancellationToken ct = default)
    {
        var latestPeriod = await db.PassengerFlowRecords
            .AsNoTracking()
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Select(r => new { r.Year, r.Quarter })
            .FirstOrDefaultAsync(ct);

        if (latestPeriod is null)
            return [];

        var records = await db.PassengerFlowRecords
            .AsNoTracking()
            .Where(r => r.LineId != null
                        && r.Year    == latestPeriod.Year
                        && r.Quarter == latestPeriod.Quarter)
            .Select(r => new
            {
                r.LineId,
                LineName       = r.Line!.Name,
                StationCount   = r.Line!.Stations!.Count,
                r.IncomingPassengers,
                r.OutgoingPassengers,
            })
            .ToListAsync(ct);

        return records
            .GroupBy(r => new { r.LineId, r.LineName, r.StationCount })
            .Select(g => new LineFlowDto
            {
                LineId        = g.Key.LineId!.Value,
                LineName      = g.Key.LineName,
                StationCount  = g.Key.StationCount,
                TotalIncoming = g.Sum(r => (long)r.IncomingPassengers),
                TotalOutgoing = g.Sum(r => (long)r.OutgoingPassengers),
            })
            .OrderByDescending(l => l.TotalIncoming)
            .ToList();
    }
}
