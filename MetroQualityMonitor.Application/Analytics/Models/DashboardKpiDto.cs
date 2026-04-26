namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// KPI-карточки для главного экрана дашборда.
/// </summary>
public class DashboardKpiDto
{
    /// <summary>Суммарный пассажиропоток за последний квартал.</summary>
    public long TotalPassengersLastQuarter { get; set; }

    /// <summary>Общее количество станций.</summary>
    public int StationCount { get; set; }

    /// <summary>Количество неподтверждённых аномалий.</summary>
    public int ActiveAnomalyCount { get; set; }

    /// <summary>Количество активных ремонтов эскалаторов.</summary>
    public int ActiveRepairCount { get; set; }

    /// <summary>Последний квартал с данными (например, «Q4»).</summary>
    public string? LatestQuarter { get; set; }

    /// <summary>Год последнего квартала с данными.</summary>
    public int? LatestYear { get; set; }
}
