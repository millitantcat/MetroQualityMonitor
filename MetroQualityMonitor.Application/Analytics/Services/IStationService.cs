using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Domain.Analytics.Enums;

namespace MetroQualityMonitor.Application.Analytics.Services;

/// <summary>
/// Сервис аналитики станций метро.
/// </summary>
public interface IStationService
{
    /// <summary>
    /// Возвращает список всех станций (краткое представление).
    /// </summary>
    Task<IReadOnlyCollection<StationLiteDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Возвращает детальную информацию о станции по идентификатору.
    /// </summary>
    Task<StationDetailsDto?> GetByIdAsync(short id, CancellationToken ct = default);

    /// <summary>
    /// Возвращает квартальный пассажиропоток по станции с возможностью фильтрации по диапазону лет.
    /// </summary>
    Task<IReadOnlyCollection<FlowRecordDto>> GetFlowAsync(short id, int? fromYear, int? toYear, CancellationToken ct = default);

    /// <summary>
    /// Возвращает прогнозы пассажиропотока для станции.
    /// </summary>
    Task<IReadOnlyCollection<ForecastDto>> GetForecastAsync(short id, CancellationToken ct = default);

    /// <summary>
    /// Возвращает часовой профиль пассажиропотока для станции на заданный тип дня.
    /// </summary>
    Task<HourlyHeatmapDto?> GetHourlyAsync(short id, DayTypes dayType, CancellationToken ct = default);

    /// <summary>
    /// Возвращает аномалии пассажиропотока по станции.
    /// </summary>
    Task<IReadOnlyCollection<AnomalyDto>> GetAnomaliesAsync(short id, CancellationToken ct = default);
}
