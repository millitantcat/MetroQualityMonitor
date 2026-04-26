using System.ComponentModel;

namespace MetroQualityMonitor.Domain.Analytics.Enums;

/// <summary>
/// Уровень серьёзности аномалии.
/// </summary>
public enum AnomalySeverities : short
{
    /// <summary>
    /// Низкий уровень.
    /// </summary>
    [Description("Низкий")]
    Low = 1,

    /// <summary>
    /// Средний уровень.
    /// </summary>
    [Description("Средний")]
    Medium = 2,

    /// <summary>
    /// Высокий уровень.
    /// </summary>
    [Description("Высокий")]
    High = 3
}
