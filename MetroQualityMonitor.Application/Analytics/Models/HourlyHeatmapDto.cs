namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Один слот часового профиля пассажиропотока.
/// </summary>
public class HourlySlotDto
{
    /// <summary>Час суток (0–23).</summary>
    public int Hour { get; set; }

    /// <summary>Доля входящих пассажиров (сумма по дню = 1.0).</summary>
    public double IncomingShare { get; set; }

    /// <summary>Доля исходящих пассажиров (сумма по дню = 1.0).</summary>
    public double OutgoingShare { get; set; }

    /// <summary>Расчётный входящий пассажиропоток за час (на основе деагрегации).</summary>
    public int? EstimatedIncoming { get; set; }

    /// <summary>Расчётный исходящий пассажиропоток за час (на основе деагрегации).</summary>
    public int? EstimatedOutgoing { get; set; }
}

/// <summary>
/// Часовой профиль пассажиропотока станции на заданный тип дня (для heatmap).
/// </summary>
public class HourlyHeatmapDto
{
    /// <summary>Идентификатор станции.</summary>
    public short StationId { get; set; }

    /// <summary>Тип дня (Weekday/Saturday/Sunday/Holiday).</summary>
    public required string DayType { get; set; }

    /// <summary>Категория станции, использованная для профиля.</summary>
    public required string StationCategory { get; set; }

    /// <summary>Слоты по 24 часам суток.</summary>
    public required IReadOnlyCollection<HourlySlotDto> Slots { get; set; }
}
