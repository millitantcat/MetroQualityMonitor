using MetroQualityMonitor.Application.Analytics.Models;

namespace MetroQualityMonitor.Application.Analytics.Services;

/// <summary>
/// Сервис вестибюлей станций метро.
/// </summary>
public interface IVestibuleService
{
    /// <summary>
    /// Возвращает список вестибюлей с координатами, опционально фильтрует по станции.
    /// </summary>
    Task<IReadOnlyCollection<VestibuleDto>> GetAllAsync(short? stationId, CancellationToken cancellationToken = default);
}
