namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Станция метро — краткое представление для списков.
/// </summary>
public class StationLiteDto
{
    /// <summary>Идентификатор станции.</summary>
    public short Id { get; set; }

    /// <summary>Наименование станции.</summary>
    public required string Name { get; set; }

    /// <summary>Линии, на которых находится станция.</summary>
    public required IReadOnlyCollection<string> Lines { get; set; }
}
