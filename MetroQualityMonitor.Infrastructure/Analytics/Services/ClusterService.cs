using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Infrastructure.Analytics.Services;

/// <summary>
/// Реализация сервиса кластеризации станций метро.
/// </summary>
public class ClusterService(MetroQualityMonitorDbContext db) : IClusterService
{
    public async Task<IReadOnlyCollection<StationWithClusterDto>> GetAllWithClustersAsync(CancellationToken ct = default)
    {
        var stations = await db.Stations
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                Lines = s.Lines!.Select(l => l.Name).ToList(),
            })
            .ToListAsync(ct);

        var stationIds = stations.Select(s => s.Id).ToList();

        // Загружаем все вестибюли с координатами, группируем в памяти
        var vestibules = await db.Vestibules
            .AsNoTracking()
            .Where(v => stationIds.Contains(v.StationId!.Value)
                        && v.LatitudeWgs84 != null
                        && v.LongitudeWgs84 != null)
            .Select(v => new
            {
                StationId = v.StationId!.Value,
                v.LatitudeWgs84,
                v.LongitudeWgs84,
            })
            .ToListAsync(ct);

        var coordMap = vestibules
            .GroupBy(v => v.StationId)
            .ToDictionary(g => g.Key, g => g.First());

        // Аномалии и ремонты по станциям
        var anomalyCounts = await db.Anomalies
            .AsNoTracking()
            .Where(a => !a.IsAcknowledged)
            .GroupBy(a => a.StationId)
            .Select(g => new { StationId = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var anomalyMap = anomalyCounts.ToDictionary(a => a.StationId, a => a.Count);

        var repairCounts = await db.EscalatorRepairs
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Vestibule != null && r.Vestibule.StationId != null)
            .GroupBy(r => r.Vestibule!.StationId!.Value)
            .Select(g => new { StationId = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var repairMap = repairCounts.ToDictionary(r => r.StationId, r => r.Count);

        // Загружаем кластеры, выбираем последний по станции в памяти
        var allClusters = await db.StationClusters
            .AsNoTracking()
            .Select(c => new { c.StationId, c.ClusterLabel, c.ClusterId, c.ComputedAtDateTimeUtc })
            .ToListAsync(ct);

        var clusterMap = allClusters
            .GroupBy(c => c.StationId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(c => c.ComputedAtDateTimeUtc).First());

        return stations
            .OrderBy(s => s.Name)
            .Select(s =>
            {
                clusterMap.TryGetValue(s.Id, out var cluster);
                coordMap.TryGetValue(s.Id, out var coord);
                return new StationWithClusterDto
                {
                    StationId         = s.Id,
                    StationName       = s.Name,
                    Lines             = s.Lines,
                    Latitude          = coord?.LatitudeWgs84,
                    Longitude         = coord?.LongitudeWgs84,
                    ClusterLabel      = cluster?.ClusterLabel,
                    ClusterId         = cluster?.ClusterId,
                    ActiveAnomalyCount = anomalyMap.GetValueOrDefault(s.Id, 0),
                    ActiveRepairCount  = repairMap.GetValueOrDefault(s.Id, 0),
                };
            })
            .ToList();
    }
}
