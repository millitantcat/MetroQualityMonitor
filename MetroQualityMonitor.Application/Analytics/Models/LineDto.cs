namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Линия метро — краткое представление со статистикой за последний квартал.
/// </summary>
public class LineDto
{
    /// <summary>Идентификатор линии.</summary>
    public short Id { get; set; }

    /// <summary>Наименование линии.</summary>
    public required string Name { get; set; }

    /// <summary>Количество станций на линии.</summary>
    public int StationCount { get; set; }

    /// <summary>Суммарный входящий пассажиропоток за последний квартал.</summary>
    public long TotalIncoming { get; set; }

    /// <summary>Суммарный исходящий пассажиропоток за последний квартал.</summary>
    public long TotalOutgoing { get; set; }

    /// <summary>Последний квартал, за который есть данные (например, «Q2»).</summary>
    public string? LatestQuarter { get; set; }

    /// <summary>Год последнего квартала с данными.</summary>
    public int? LatestYear { get; set; }
}
