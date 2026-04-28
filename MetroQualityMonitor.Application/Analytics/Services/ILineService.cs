using MetroQualityMonitor.Application.Analytics.Models;

namespace MetroQualityMonitor.Application.Analytics.Services;

/// <summary>
/// Сервис аналитики линий метро.
/// </summary>
public interface ILineService
{
    /// <summary>
    /// Возвращает список всех линий с агрегированной статистикой за последний квартал.
    /// </summary>
    Task<IReadOnlyCollection<LineDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает детальную информацию о линии по идентификатору.
    /// </summary>
    Task<LineDetailsDto?> GetByIdAsync(short id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает список станций линии.
    /// </summary>
    Task<IReadOnlyCollection<StationLiteDto>> GetStationsAsync(short lineId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает квартальный пассажиропоток по линии с возможностью фильтрации по диапазону лет.
    /// </summary>
    Task<IReadOnlyCollection<FlowRecordDto>> GetFlowAsync(short lineId, int? fromYear, int? toYear, CancellationToken cancellationToken = default);
}
