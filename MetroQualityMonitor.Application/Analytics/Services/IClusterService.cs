using MetroQualityMonitor.Application.Analytics.Models;

namespace MetroQualityMonitor.Application.Analytics.Services;

/// <summary>
/// Сервис кластеризации станций метро.
/// </summary>
public interface IClusterService
{
    /// <summary>
    /// Возвращает все станции с результатами кластеризации и координатами для карты.
    /// </summary>
    Task<IReadOnlyCollection<StationWithClusterDto>> GetAllWithClustersAsync(CancellationToken cancellationToken = default);
}
