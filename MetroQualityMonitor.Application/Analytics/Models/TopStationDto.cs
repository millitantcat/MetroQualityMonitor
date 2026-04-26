namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Станция из топа по заданной метрике пассажиропотока.
/// </summary>
public class TopStationDto
{
    /// <summary>Идентификатор станции.</summary>
    public short StationId { get; set; }

    /// <summary>Наименование станции.</summary>
    public required string StationName { get; set; }

    /// <summary>Линии, на которых находится станция.</summary>
    public required IReadOnlyCollection<string> Lines { get; set; }

    /// <summary>Значение метрики за последний квартал.</summary>
    public long Value { get; set; }
}
