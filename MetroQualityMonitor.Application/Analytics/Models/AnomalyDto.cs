namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Обнаруженная аномалия пассажиропотока.
/// </summary>
public class AnomalyDto
{
    /// <summary>Идентификатор аномалии.</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор станции.</summary>
    public short StationId { get; set; }

    /// <summary>Наименование станции.</summary>
    public required string StationName { get; set; }

    /// <summary>Год квартала с аномалией.</summary>
    public int Year { get; set; }

    /// <summary>Квартал с аномалией (например, «Q3»).</summary>
    public required string Quarter { get; set; }

    /// <summary>Тип аномалии (Statistical/IsolationForest/YoYDeviation).</summary>
    public required string AnomalyType { get; set; }

    /// <summary>Степень серьёзности (Low/Medium/High).</summary>
    public required string Severity { get; set; }

    /// <summary>Оценочный балл аномальности.</summary>
    public double Score { get; set; }

    /// <summary>Фактическое значение пассажиропотока.</summary>
    public int ActualValue { get; set; }

    /// <summary>Ожидаемое значение пассажиропотока (если рассчитано).</summary>
    public int? ExpectedValue { get; set; }

    /// <summary>Описание аномалии.</summary>
    public string? Description { get; set; }

    /// <summary>Аномалия подтверждена оператором.</summary>
    public bool IsAcknowledged { get; set; }

    /// <summary>Дата и время подтверждения аномалии (UTC).</summary>
    public DateTime? AcknowledgedDateTimeUtc { get; set; }

    /// <summary>Дата и время обнаружения аномалии (UTC).</summary>
    public DateTime CreateDateTimeUtc { get; set; }
}
