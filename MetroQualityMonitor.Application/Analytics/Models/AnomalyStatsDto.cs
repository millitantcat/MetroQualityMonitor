namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Статистика аномалий для графиков дашборда.
/// </summary>
public class AnomalyStatsDto
{
    /// <summary>Количество аномалий по степени серьёзности.</summary>
    public required IReadOnlyCollection<AnomalyCountItem> BySeverity { get; set; }

    /// <summary>Количество аномалий по типу детектора.</summary>
    public required IReadOnlyCollection<AnomalyCountItem> ByType { get; set; }

    /// <summary>Суммарное количество неподтверждённых аномалий.</summary>
    public int TotalActive { get; set; }
}

/// <summary>
/// Пара «метка — количество» для диаграммы.
/// </summary>
public class AnomalyCountItem
{
    /// <summary>Метка (например, «High», «Statistical»).</summary>
    public required string Label { get; set; }

    /// <summary>Количество аномалий.</summary>
    public int Count { get; set; }
}
