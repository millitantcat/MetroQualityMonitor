namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Детальная информация о линии метро.
/// </summary>
public class LineDetailsDto
{
    /// <summary>Идентификатор линии.</summary>
    public short Id { get; set; }

    /// <summary>Наименование линии.</summary>
    public required string Name { get; set; }

    /// <summary>Количество станций на линии.</summary>
    public int StationCount { get; set; }

    /// <summary>Количество вестибюлей на линии.</summary>
    public int VestibuleCount { get; set; }

    /// <summary>Суммарный входящий пассажиропоток за последний квартал.</summary>
    public long TotalIncoming { get; set; }

    /// <summary>Суммарный исходящий пассажиропоток за последний квартал.</summary>
    public long TotalOutgoing { get; set; }

    /// <summary>Последний квартал, за который есть данные.</summary>
    public string? LatestQuarter { get; set; }

    /// <summary>Год последнего квартала с данными.</summary>
    public int? LatestYear { get; set; }
}
