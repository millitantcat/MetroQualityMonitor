using MetroQualityMonitor.Application.Analytics.Models;

namespace MetroQualityMonitor.Application.Analytics.Services;

/// <summary>
/// Сервис данных для главного экрана (дашборд).
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Возвращает KPI-карточки дашборда.
    /// </summary>
    Task<DashboardKpiDto> GetKpiAsync(CancellationToken ct = default);

    /// <summary>
    /// Возвращает топ-N станций по заданной метрике пассажиропотока.
    /// </summary>
    /// <param name="n">Количество станций в топе.</param>
    /// <param name="metric">Метрика: «incoming» или «outgoing».</param>
    Task<IReadOnlyCollection<TopStationDto>> GetTopStationsAsync(int n, string metric, CancellationToken ct = default);

    /// <summary>
    /// Возвращает агрегированный пассажиропоток по кварталам (для графика сезонности).
    /// </summary>
    Task<IReadOnlyCollection<SeasonalityPointDto>> GetSeasonalityAsync(CancellationToken ct = default);

    /// <summary>
    /// Возвращает статистику аномалий (по severity и типу) для диаграмм дашборда.
    /// </summary>
    Task<AnomalyStatsDto> GetAnomalyStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// Возвращает пассажиропоток по линиям за последний доступный квартал.
    /// </summary>
    Task<IReadOnlyCollection<LineFlowDto>> GetLinesFlowAsync(CancellationToken ct = default);
}
