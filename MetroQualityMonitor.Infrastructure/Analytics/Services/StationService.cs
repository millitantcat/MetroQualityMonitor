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
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<StationLiteDto>> GetAllAsync(CancellationToken cancellationToken = default)
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
            .ToListAsync(cancellationToken);

        return stations.Select(s => new StationLiteDto
        {
            Id    = s.Id,
            Name  = s.Name,
            Lines = s.Lines,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<StationDetailsDto?> GetByIdAsync(short id, CancellationToken cancellationToken = default)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (station is null)
            return null;

        var clusterLabel = await db.StationClusters
            .AsNoTracking()
            .Where(c => c.StationId == id)
            .OrderByDescending(c => c.ComputedAtDateTimeUtc)
            .Select(c => c.ClusterLabel)
            .FirstOrDefaultAsync(cancellationToken);

        var vestibuleCount = await db.Vestibules.CountAsync(v => v.StationId == id, cancellationToken);

        var activeRepairCount = await db.EscalatorRepairs
            .CountAsync(r => r.Vestibule!.StationId == id && !r.IsDeleted, cancellationToken);

        var latestPeriod = await db.PassengerFlowRecords
            .AsNoTracking()
            .Where(r => r.StationId == id)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Select(r => new { r.Year, r.Quarter, r.IncomingPassengers, r.OutgoingPassengers })
            .FirstOrDefaultAsync(cancellationToken);

        double? yoyGrowth = null;
        if (latestPeriod is not null)
        {
            var prevYearRecord = await db.PassengerFlowRecords
                .AsNoTracking()
                .Where(r => r.StationId == id
                            && r.Year == latestPeriod.Year - 1
                            && r.Quarter == latestPeriod.Quarter)
                .Select(r => r.IncomingPassengers)
                .FirstOrDefaultAsync(cancellationToken);

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

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<FlowRecordDto>> GetFlowAsync(
        short id, int? fromYear, int? toYear, CancellationToken cancellationToken = default)
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
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<ForecastDto>> GetForecastAsync(short id, CancellationToken cancellationToken = default)
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
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<HourlyHeatmapDto?> GetHourlyAsync(short id, DayTypes dayType, CancellationToken cancellationToken = default)
    {
        var exists = await db.Stations.AnyAsync(s => s.Id == id, cancellationToken);
        if (!exists)
            return null;

        var clusterLabel = await db.StationClusters
            .AsNoTracking()
            .Where(c => c.StationId == id)
            .OrderByDescending(c => c.ComputedAtDateTimeUtc)
            .Select(c => c.ClusterLabel)
            .FirstOrDefaultAsync(cancellationToken);

        var category = ParseCategory(clusterLabel);

        var profiles = await db.HourlyProfiles
            .AsNoTracking()
            .Where(p => p.StationCategory == category && p.DayType == dayType)
            .OrderBy(p => p.Hour)
            .ToListAsync(cancellationToken);

        if (profiles.Count == 0)
            return null;

        var latestFlow = await db.PassengerFlowRecords
            .AsNoTracking()
            .Where(r => r.StationId == id)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .Select(r => new { r.IncomingPassengers, r.OutgoingPassengers })
            .FirstOrDefaultAsync(cancellationToken);

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

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AnomalyDto>> GetAnomaliesAsync(short id, CancellationToken cancellationToken = default)
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
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Преобразует строковую метку кластера из БД в перечисление <see cref="StationCategories"/>.
    /// Неизвестная или отсутствующая метка маппируется в <see cref="StationCategories.Mixed"/>.
    /// </summary>
    private static StationCategories ParseCategory(string? label) =>
        label switch
        {
            "Central"     => StationCategories.Central,
            "Transfer"    => StationCategories.Transfer,
            "Residential" => StationCategories.Residential,
            _             => StationCategories.Mixed,
        };

    /// <summary>
    /// Возвращает коэффициент загруженности дня относительно среднего будня (=1.0).
    /// Используется в формуле деагрегации квартального потока в часовой.
    /// </summary>
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
