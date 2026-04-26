namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Вестибюль станции метро с координатами для отображения на карте.
/// </summary>
public class VestibuleDto
{
    /// <summary>Идентификатор вестибюля.</summary>
    public int Id { get; set; }

    /// <summary>Наименование вестибюля.</summary>
    public required string Name { get; set; }

    /// <summary>Идентификатор станции.</summary>
    public short? StationId { get; set; }

    /// <summary>Наименование станции.</summary>
    public string? StationName { get; set; }

    /// <summary>Долгота WGS84.</summary>
    public double? Longitude { get; set; }

    /// <summary>Широта WGS84.</summary>
    public double? Latitude { get; set; }

    /// <summary>Тип вестибюля.</summary>
    public string? VestibuleType { get; set; }

    /// <summary>Административный округ.</summary>
    public string? AdmArea { get; set; }

    /// <summary>Район.</summary>
    public string? District { get; set; }
}
