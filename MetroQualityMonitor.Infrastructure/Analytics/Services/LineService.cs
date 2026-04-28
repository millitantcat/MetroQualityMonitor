using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Infrastructure.Analytics.Services;

/// <summary>
/// Реализация сервиса аналитики линий метро.
/// </summary>
public class LineService(MetroQualityMonitorDbContext db) : ILineService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<LineDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var latestPeriod = await db.PassengerFlowRecords
            .AsNoTracking()
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Select(r => new { r.Year, r.Quarter })
            .FirstOrDefaultAsync(cancellationToken);

        var lines = await db.Lines
            .AsNoTracking()
            .Select(l => new
            {
                l.Id,
                l.Name,
                StationCount = l.Stations!.Count,
                TotalIncoming = latestPeriod == null ? 0L :
                    (long)db.PassengerFlowRecords
                        .Where(r => r.LineId == l.Id
                                    && r.Year == latestPeriod.Year
                                    && r.Quarter == latestPeriod.Quarter)
                        .Sum(r => r.IncomingPassengers),
                TotalOutgoing = latestPeriod == null ? 0L :
                    (long)db.PassengerFlowRecords
                        .Where(r => r.LineId == l.Id
                                    && r.Year == latestPeriod.Year
                                    && r.Quarter == latestPeriod.Quarter)
                        .Sum(r => r.OutgoingPassengers),
            })
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);

        return lines.Select(l => new LineDto
        {
            Id           = l.Id,
            Name         = l.Name,
            StationCount = l.StationCount,
            TotalIncoming = l.TotalIncoming,
            TotalOutgoing = l.TotalOutgoing,
            LatestQuarter = latestPeriod?.Quarter,
            LatestYear    = latestPeriod?.Year,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<LineDetailsDto?> GetByIdAsync(short id, CancellationToken cancellationToken = default)
    {
        var line = await db.Lines
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new { l.Id, l.Name, StationCount = l.Stations!.Count })
            .FirstOrDefaultAsync(cancellationToken);

        if (line is null)
            return null;

        var latestPeriod = await db.PassengerFlowRecords
            .AsNoTracking()
            .Where(r => r.LineId == id)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Select(r => new { r.Year, r.Quarter })
            .FirstOrDefaultAsync(cancellationToken);

        long totalIn = 0, totalOut = 0;
        if (latestPeriod is not null)
        {
            totalIn  = await db.PassengerFlowRecords
                .Where(r => r.LineId == id && r.Year == latestPeriod.Year && r.Quarter == latestPeriod.Quarter)
                .SumAsync(r => (long)r.IncomingPassengers, cancellationToken);
            totalOut = await db.PassengerFlowRecords
                .Where(r => r.LineId == id && r.Year == latestPeriod.Year && r.Quarter == latestPeriod.Quarter)
                .SumAsync(r => (long)r.OutgoingPassengers, cancellationToken);
        }

        var vestibuleCount = await db.Vestibules
            .CountAsync(v => v.LineId == id, cancellationToken);

        return new LineDetailsDto
        {
            Id             = line.Id,
            Name           = line.Name,
            StationCount   = line.StationCount,
            VestibuleCount = vestibuleCount,
            TotalIncoming  = totalIn,
            TotalOutgoing  = totalOut,
            LatestQuarter  = latestPeriod?.Quarter,
            LatestYear     = latestPeriod?.Year,
        };
    }
    
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<StationLiteDto>> GetStationsAsync(short lineId, CancellationToken cancellationToken = default)
    {
        var stations = await db.Lines
            .AsNoTracking()
            .Where(l => l.Id == lineId)
            .SelectMany(l => l.Stations!)
            .Select(s => new
            {
                s.Id,
                s.Name,
                Lines = s.Lines!.Select(ln => ln.Name).ToList(),
            })
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return stations.Select(s => new StationLiteDto
        {
            Id    = s.Id,
            Name  = s.Name,
            Lines = s.Lines,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FlowRecordDto>> GetFlowAsync(
        short lineId, int? fromYear, int? toYear, CancellationToken cancellationToken = default)
    {
        var query = db.PassengerFlowRecords
            .AsNoTracking()
            .Where(r => r.LineId == lineId);

        if (fromYear.HasValue)
            query = query.Where(r => r.Year >= fromYear.Value);
        if (toYear.HasValue)
            query = query.Where(r => r.Year <= toYear.Value);

        var aggregated = await query
            .GroupBy(r => new { r.Year, r.Quarter })
            .Select(g => new FlowRecordDto
            {
                Year               = g.Key.Year,
                Quarter            = g.Key.Quarter,
                IncomingPassengers = (int)g.Sum(r => (long)r.IncomingPassengers),
                OutgoingPassengers = (int)g.Sum(r => (long)r.OutgoingPassengers),
            })
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Quarter)
            .ToListAsync(cancellationToken);

        return aggregated;
    }
}
