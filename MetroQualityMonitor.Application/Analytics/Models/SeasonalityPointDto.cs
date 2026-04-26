namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Точка агрегированного пассажиропотока за квартал (для графика сезонности).
/// </summary>
public class SeasonalityPointDto
{
    /// <summary>Год.</summary>
    public int Year { get; set; }

    /// <summary>Квартал (например, «Q1»).</summary>
    public required string Quarter { get; set; }

    /// <summary>Суммарный входящий пассажиропоток по всем станциям.</summary>
    public long TotalIncoming { get; set; }

    /// <summary>Суммарный исходящий пассажиропоток по всем станциям.</summary>
    public long TotalOutgoing { get; set; }
}
