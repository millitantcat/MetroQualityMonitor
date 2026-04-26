namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Станция с результатами кластеризации и координатами для отображения на карте.
/// </summary>
public class StationWithClusterDto
{
    /// <summary>Идентификатор станции.</summary>
    public short StationId { get; set; }

    /// <summary>Наименование станции.</summary>
    public required string StationName { get; set; }

    /// <summary>Линии, на которых находится станция.</summary>
    public required IReadOnlyCollection<string> Lines { get; set; }

    /// <summary>Широта WGS84 (координата первого вестибюля).</summary>
    public double? Latitude { get; set; }

    /// <summary>Долгота WGS84 (координата первого вестибюля).</summary>
    public double? Longitude { get; set; }

    /// <summary>Метка кластера (Residential/Central/Transfer/Mixed). Null, если кластеризация не запускалась.</summary>
    public string? ClusterLabel { get; set; }

    /// <summary>Числовой идентификатор кластера.</summary>
    public int? ClusterId { get; set; }

    /// <summary>Количество активных (неподтверждённых) аномалий на станции.</summary>
    public int ActiveAnomalyCount { get; set; }

    /// <summary>Количество активных ремонтов эскалаторов на станции.</summary>
    public int ActiveRepairCount { get; set; }
}
