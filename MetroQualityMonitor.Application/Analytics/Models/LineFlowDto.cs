namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Пассажиропоток по линии за последний квартал — для сравнительного графика.
/// </summary>
public class LineFlowDto
{
    /// <summary>Идентификатор линии.</summary>
    public short LineId { get; set; }

    /// <summary>Наименование линии.</summary>
    public required string LineName { get; set; }

    /// <summary>Суммарный входящий пассажиропоток за период.</summary>
    public long TotalIncoming { get; set; }

    /// <summary>Суммарный исходящий пассажиропоток за период.</summary>
    public long TotalOutgoing { get; set; }

    /// <summary>Количество станций на линии.</summary>
    public int StationCount { get; set; }
}
