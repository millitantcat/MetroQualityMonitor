namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Прогноз пассажиропотока на квартал.
/// </summary>
public class ForecastDto
{
    /// <summary>Идентификатор прогноза.</summary>
    public Guid Id { get; set; }

    /// <summary>Год прогноза.</summary>
    public int Year { get; set; }

    /// <summary>Квартал прогноза (например, «Q3»).</summary>
    public required string Quarter { get; set; }

    /// <summary>Прогнозируемый входящий пассажиропоток.</summary>
    public int PredictedIncoming { get; set; }

    /// <summary>Прогнозируемый исходящий пассажиропоток.</summary>
    public int PredictedOutgoing { get; set; }

    /// <summary>Нижняя граница доверительного интервала (входящие).</summary>
    public int? ConfidenceLowerIncoming { get; set; }

    /// <summary>Верхняя граница доверительного интервала (входящие).</summary>
    public int? ConfidenceUpperIncoming { get; set; }

    /// <summary>Нижняя граница доверительного интервала (исходящие).</summary>
    public int? ConfidenceLowerOutgoing { get; set; }

    /// <summary>Верхняя граница доверительного интервала (исходящие).</summary>
    public int? ConfidenceUpperOutgoing { get; set; }

    /// <summary>Название модели прогнозирования (например, «SARIMA»).</summary>
    public required string ModelName { get; set; }

    /// <summary>Версия модели прогнозирования.</summary>
    public required string ModelVersion { get; set; }
}
