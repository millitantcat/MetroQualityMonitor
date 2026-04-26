using System.ComponentModel;

namespace MetroQualityMonitor.Domain.Analytics.Enums;

/// <summary>
/// Тип обнаруженной аномалии.
/// </summary>
public enum AnomalyTypes : short
{
    /// <summary>
    /// Статистическая аномалия (Z-score).
    /// </summary>
    [Description("Статистическая")]
    Statistical = 1,

    /// <summary>
    /// Аномалия, обнаруженная алгоритмом Isolation Forest.
    /// </summary>
    [Description("Isolation Forest")]
    IsolationForest = 2,

    /// <summary>
    /// Отклонение год-к-году.
    /// </summary>
    [Description("Отклонение год-к-году")]
    YoYDeviation = 3
}
