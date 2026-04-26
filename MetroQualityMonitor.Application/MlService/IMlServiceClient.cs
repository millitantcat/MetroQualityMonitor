using MetroQualityMonitor.Application.MlService.Models;

namespace MetroQualityMonitor.Application.MlService;

/// <summary>
/// Клиент для взаимодействия с Python ML-сервисом через HTTP.
/// </summary>
public interface IMlServiceClient
{
    /// <summary>
    /// Проверяет доступность ML-сервиса.
    /// </summary>
    Task<bool> HealthCheckAsync(CancellationToken ct = default);

    /// <summary>
    /// Запускает batch-пересчёт прогнозов пассажиропотока (SARIMA).
    /// </summary>
    Task<MlBatchResultDto> RunForecastBatchAsync(CancellationToken ct = default);

    /// <summary>
    /// Запускает детекцию аномалий (Z-score, YoY, Isolation Forest).
    /// </summary>
    Task<MlBatchResultDto> RunAnomalyDetectionAsync(CancellationToken ct = default);

    /// <summary>
    /// Запускает пересчёт кластеризации станций (K-Means).
    /// </summary>
    Task<MlBatchResultDto> RecomputeClustersAsync(CancellationToken ct = default);

    /// <summary>
    /// Заполняет таблицу HourlyProfiles эмпирическими профилями.
    /// При overwrite = false пропускает, если таблица уже заполнена.
    /// </summary>
    Task<MlBatchResultDto> SeedProfilesAsync(bool overwrite = false, CancellationToken ct = default);
}
