using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Infrastructure.Analytics.Services;

/// <summary>
/// Реализация сервиса вестибюлей станций метро.
/// </summary>
public class VestibuleService(MetroQualityMonitorDbContext db) : IVestibuleService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<VestibuleDto>> GetAllAsync(short? stationId, CancellationToken cancellationToken = default)
    {
        var query = db.Vestibules.AsNoTracking().AsQueryable();

        if (stationId.HasValue)
            query = query.Where(v => v.StationId == stationId.Value);

        return await query
            .OrderBy(v => v.Name)
            .Select(v => new VestibuleDto
            {
                Id           = v.Id,
                Name         = v.Name,
                StationId    = v.StationId,
                StationName  = v.Station != null ? v.Station.Name : null,
                Longitude    = v.LongitudeWgs84,
                Latitude     = v.LatitudeWgs84,
                VestibuleType = v.VestibuleType,
                AdmArea      = v.AdmArea,
                District     = v.District,
            })
            .ToListAsync(cancellationToken);
    }
}
