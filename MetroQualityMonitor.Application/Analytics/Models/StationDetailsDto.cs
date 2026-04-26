namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Детальная информация о станции метро.
/// </summary>
public class StationDetailsDto
{
    /// <summary>Идентификатор станции.</summary>
    public short Id { get; set; }

    /// <summary>Наименование станции.</summary>
    public required string Name { get; set; }

    /// <summary>Линии, на которых находится станция.</summary>
    public required IReadOnlyCollection<string> Lines { get; set; }

    /// <summary>Категория станции (Residential/Central/Transfer/Mixed) из кластеризации.</summary>
    public string? Category { get; set; }

    /// <summary>Количество вестибюлей станции.</summary>
    public int VestibuleCount { get; set; }

    /// <summary>Количество активных ремонтов эскалаторов.</summary>
    public int ActiveRepairCount { get; set; }

    /// <summary>Входящий пассажиропоток за последний квартал.</summary>
    public int? LatestIncoming { get; set; }

    /// <summary>Исходящий пассажиропоток за последний квартал.</summary>
    public int? LatestOutgoing { get; set; }

    /// <summary>Рост пассажиропотока год-к-году (доля, например 0.05 = +5%).</summary>
    public double? YoyGrowth { get; set; }
}
