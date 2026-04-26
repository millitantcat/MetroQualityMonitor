using System.ComponentModel;

namespace MetroQualityMonitor.Domain.Analytics.Enums;

/// <summary>
/// Категория станции метро по характеру пассажиропотока.
/// </summary>
public enum StationCategories : short
{
    /// <summary>
    /// Спальная станция (жилые районы на периферии).
    /// </summary>
    [Description("Спальная")]
    Residential = 1,

    /// <summary>
    /// Центральная станция (деловой и туристический центр).
    /// </summary>
    [Description("Центральная")]
    Central = 2,

    /// <summary>
    /// Пересадочная станция (у вокзалов, крупных транспортных узлов).
    /// </summary>
    [Description("Пересадочная")]
    Transfer = 3,

    /// <summary>
    /// Смешанная категория.
    /// </summary>
    [Description("Смешанная")]
    Mixed = 4
}
