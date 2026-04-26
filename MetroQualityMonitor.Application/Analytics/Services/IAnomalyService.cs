using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Domain.Analytics.Enums;

namespace MetroQualityMonitor.Application.Analytics.Services;

/// <summary>
/// Сервис управления аномалиями пассажиропотока.
/// </summary>
public interface IAnomalyService
{
    /// <summary>
    /// Возвращает список аномалий с возможностью фильтрации.
    /// </summary>
    Task<IReadOnlyCollection<AnomalyDto>> GetAllAsync(
        AnomalySeverities? severity,
        bool? isAcknowledged,
        CancellationToken ct = default);

    /// <summary>
    /// Подтверждает аномалию оператором.
    /// </summary>
    /// <returns>True, если аномалия найдена и подтверждена; false, если не найдена.</returns>
    Task<bool> AcknowledgeAsync(Guid id, CancellationToken ct = default);
}
