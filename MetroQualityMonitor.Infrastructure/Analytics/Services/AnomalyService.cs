using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using MetroQualityMonitor.Domain.Analytics.Enums;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Infrastructure.Analytics.Services;

/// <summary>
/// Реализация сервиса управления аномалиями пассажиропотока.
/// </summary>
public class AnomalyService(MetroQualityMonitorDbContext db) : IAnomalyService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AnomalyDto>> GetAllAsync(
        AnomalySeverities? severity,
        bool? isAcknowledged,
        CancellationToken cancellationToken = default)
    {
        var query = db.Anomalies
            .AsNoTracking()
            .AsQueryable();

        if (severity.HasValue)
            query = query.Where(a => a.Severity == severity.Value);

        if (isAcknowledged.HasValue)
            query = query.Where(a => a.IsAcknowledged == isAcknowledged.Value);

        return await query
            .OrderByDescending(a => a.CreateDateTimeUtc)
            .Select(a => new AnomalyDto
            {
                Id                      = a.Id,
                StationId               = a.StationId,
                StationName             = a.Station!.Name,
                Year                    = a.Year,
                Quarter                 = a.Quarter,
                AnomalyType             = a.AnomalyType.ToString(),
                Severity                = a.Severity.ToString(),
                Score                   = a.Score,
                ActualValue             = a.ActualValue,
                ExpectedValue           = a.ExpectedValue,
                Description             = a.Description,
                IsAcknowledged          = a.IsAcknowledged,
                AcknowledgedDateTimeUtc = a.AcknowledgedDateTimeUtc,
                CreateDateTimeUtc       = a.CreateDateTimeUtc,
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> AcknowledgeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var anomaly = await db.Anomalies.FindAsync([id], cancellationToken);
        if (anomaly is null)
            return false;

        anomaly.IsAcknowledged          = true;
        anomaly.AcknowledgedDateTimeUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
