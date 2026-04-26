using System.ComponentModel;

namespace MetroQualityMonitor.Domain.Analytics.Enums;

/// <summary>
/// Тип дня для часового профиля пассажиропотока.
/// </summary>
public enum DayTypes : short
{
    /// <summary>
    /// Рабочий день.
    /// </summary>
    [Description("Рабочий день")]
    Weekday = 1,

    /// <summary>
    /// Суббота.
    /// </summary>
    [Description("Суббота")]
    Saturday = 2,

    /// <summary>
    /// Воскресенье.
    /// </summary>
    [Description("Воскресенье")]
    Sunday = 3,

    /// <summary>
    /// Праздничный день.
    /// </summary>
    [Description("Праздник")]
    Holiday = 4
}
