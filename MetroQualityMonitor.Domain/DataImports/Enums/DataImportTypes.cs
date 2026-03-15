using System.ComponentModel;

namespace MetroQualityMonitor.Domain.DataImports.Enums;

/// <summary>
/// Тип загруженных данных.
/// </summary>
public enum DataImportTypes : short
{
    /// <summary>
    /// Неизвестно.
    /// </summary>
    [Description("Неизвестно")]
    Unknown = -1,

    /// <summary>
    /// Входы метрополитена.
    /// </summary>
    [Description("Входы метрополитена")]
    MetroEntrances = 1,

    /// <summary>
    /// Пассажиропоток станций.
    /// </summary>
    [Description("Пассажиропоток станций")]
    StationPassengerFlow = 2,

    /// <summary>
    /// Транспортные зоны.
    /// </summary>
    [Description("Транспортные зоны")]
    TransportHubs = 3
}