using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using MetroQualityMonitor.Domain.Analytics.Enums;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Infrastructure.Analytics.Services;

/// <summary>
/// Реализация сервиса аналитики станций метро.
/// </summary>
public class StationService(MetroQualityMonitorDbContext db) : IStationService
{
    public async Task<IReadOnlyCollection<StationLiteDto>> GetAllAsync(CancellationToken ct = default)
    {
        var stations = await db.Stations
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                Lines = s.Lines!.Select(l => l.Name).ToList(),
            })
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return stations.Select(s => new StationLiteDto
        {
            Id    = s.Id,
            Name  = s.Name,
            Lines = s.Lines,
        }).ToList();
    }

    public async Task<StationDetailsDto?> GetByIdAsync(short id, CancellationToken ct = default)
    {
        var station = await db.Stations
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.Name,
                Lines = s.Lines!.Select(l => l.Name).ToList(),
            })
            .FirstOrDefaultAsync(ct);

        if (station is null)
            return null;

        var clusterLabel = await db.StationClusters
            .AsNoTracking()
            .Where(c => c.StationId == id)
            .OrderByDescending(c => c.ComputedAtDateTimeUtc)
            .Select(c => c.ClusterLabel)
            .FirstOrDefaultAsync(ct);

        var vestibuleCount = await db.Vestibules.CountAsync(v => v.StationId == id, ct);

        var activeRepairCount = await db.EscalatorRepairs
            .CountAsync(r => r.Vestibule!.StationId == id && !r.IsDeleted, ct);

        var latestPeriod = await db.PassengerFlowRecords
            .AsNoTracking()
            .Where(r => r.StationId == id)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Select(r => new { r.Year, r.Quarter, r.IncomingPassengers, r.OutgoingPassengers })
            .FirstOrDefaultAsync(ct);

        double? yoyGrowth = null;
        if (latestPeriod is not null)
        {
            var prevYearRecord = await db.PassengerFlowRecords
                .AsNoTracking()
                .Where(r => r.StationId == id
                            && r.Year == latestPeriod.Year - 1
                            && r.Quarter == latestPeriod.Quarter)
                .Select(r => r.IncomingPassengers)
                .FirstOrDefaultAsync(ct);

            if (prevYearRecord > 0)
                yoyGrowth = (double)(latestPeriod.IncomingPassengers - prevYearRecord) / prevYearRecord;
        }

        return new StationDetailsDto
        {
            Id               = station.Id,
            Name             = station.Name,
            Lines            = station.Lines,
            Category         = clusterLabel,
            VestibuleCount   = vestibuleCount,
            ActiveRepairCount = activeRepairCount,
            LatestIncoming   = latestPeriod?.IncomingPassengers,
            LatestOutgoing   = latestPeriod?.OutgoingPassengers,
            YoyGrowth        = yoyGrowth,
        };
    }

    public async Task<IReadOnlyCollection<FlowRecordDto>> GetFlowAsync(
        short id, int? fromYear, int? toYear, CancellationToken ct = default)
    {
        var query = db.PassengerFlowRecords
            .AsNoTracking()
            .Where(r => r.StationId == id);

        if (fromYear.HasValue)
            query = query.Where(r => r.Year >= fromYear.Value);
        if (toYear.HasValue)
            query = query.Where(r => r.Year <= toYear.Value);

        return await query
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Quarter)
            .Select(r => new FlowRecordDto
            {
                Year               = r.Year,
                Quarter            = r.Quarter,
                IncomingPassengers = r.IncomingPassengers,
                OutgoingPassengers = r.OutgoingPassengers,
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<ForecastDto>> GetForecastAsync(short id, CancellationToken ct = default)
    {
        return await db.Forecasts
            .AsNoTracking()
            .Where(f => f.StationId == id)
            .OrderBy(f => f.Year)
            .ThenBy(f => f.Quarter)
            .Select(f => new ForecastDto
            {
                Id                       = f.Id,
                Year                     = f.Year,
                Quarter                  = f.Quarter,
                PredictedIncoming        = f.PredictedIncoming,
                PredictedOutgoing        = f.PredictedOutgoing,
                ConfidenceLowerIncoming  = f.ConfidenceLowerIncoming,
                ConfidenceUpperIncoming  = f.ConfidenceUpperIncoming,
                ConfidenceLowerOutgoing  = f.ConfidenceLowerOutgoing,
                ConfidenceUpperOutgoing  = f.ConfidenceUpperOutgoing,
                ModelName                = f.ModelName,
                ModelVersion             = f.ModelVersion,
            })
            .ToListAsync(ct);
    }

    public async Task<HourlyHeatmapDto?> GetHourlyAsync(short id, DayTypes dayType, CancellationToken ct = default)
    {
        var exists = await db.Stations.AnyAsync(s => s.Id == id, ct);
        if (!exists)
            return null;

        var clusterLabel = await db.StationClusters
            .AsNoTracking()
            .Where(c => c.StationId == id)
            .OrderByDescending(c => c.ComputedAtDateTimeUtc)
            .Select(c => c.ClusterLabel)
            .FirstOrDefaultAsync(ct);

        var category = ParseCategory(clusterLabel);

        var profiles = await db.HourlyProfiles
            .AsNoTracking()
            .Where(p => p.StationCategory == category && p.DayType == dayType)
            .OrderBy(p => p.Hour)
            .ToListAsync(ct);

        if (profiles.Count == 0)
            return null;

        var latestFlow = await db.PassengerFlowRecords
            .AsNoTracking()
            .Where(r => r.StationId == id)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Select(r => new { r.IncomingPassengers, r.OutgoingPassengers })
            .FirstOrDefaultAsync(ct);

        var factor = WeekdayFactor(dayType);

        var slots = profiles.Select(p =>
        {
            int? estIn = null, estOut = null;
            if (latestFlow is not null)
            {
                estIn  = (int)Math.Round(latestFlow.IncomingPassengers / 91.0 * factor * p.IncomingShare);
                estOut = (int)Math.Round(latestFlow.OutgoingPassengers / 91.0 * factor * p.OutgoingShare);
            }

            return new HourlySlotDto
            {
                Hour              = p.Hour,
                IncomingShare     = p.IncomingShare,
                OutgoingShare     = p.OutgoingShare,
                EstimatedIncoming = estIn,
                EstimatedOutgoing = estOut,
            };
        }).ToList();

        return new HourlyHeatmapDto
        {
            StationId       = id,
            DayType         = dayType.ToString(),
            StationCategory = category.ToString(),
            Slots           = slots,
        };
    }

    public async Task<IReadOnlyCollection<AnomalyDto>> GetAnomaliesAsync(short id, CancellationToken ct = default)
    {
        return await db.Anomalies
            .AsNoTracking()
            .Where(a => a.StationId == id)
            .OrderByDescending(a => a.CreateDateTimeUtc)
            .Select(a => new AnomalyDto
            {
                Id                     = a.Id,
                StationId              = a.StationId,
                StationName            = a.Station!.Name,
                Year                   = a.Year,
                Quarter                = a.Quarter,
                AnomalyType            = a.AnomalyType.ToString(),
                Severity               = a.Severity.ToString(),
                Score                  = a.Score,
                ActualValue            = a.ActualValue,
                ExpectedValue          = a.ExpectedValue,
                Description            = a.Description,
                IsAcknowledged         = a.IsAcknowledged,
                AcknowledgedDateTimeUtc = a.AcknowledgedDateTimeUtc,
                CreateDateTimeUtc      = a.CreateDateTimeUtc,
            })
            .ToListAsync(ct);
    }

    private static StationCategories ParseCategory(string? label) =>
        label switch
        {
            "Central"     => StationCategories.Central,
            "Transfer"    => StationCategories.Transfer,
            "Residential" => StationCategories.Residential,
            _             => StationCategories.Mixed,
        };

    private static double WeekdayFactor(DayTypes dayType) =>
        dayType switch
        {
            DayTypes.Weekday  => 1.15,
            DayTypes.Saturday => 0.70,
            DayTypes.Sunday   => 0.55,
            DayTypes.Holiday  => 0.55,
            _                 => 1.0,
        };
}
