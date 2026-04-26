using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroQualityMonitor.Domain.Analytics.Enums;
using MetroQualityMonitor.Domain.Stations.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Analytics.Entities;

/// <summary>
/// Обнаруженная аномалия пассажиропотока по станции за квартал.
/// </summary>
[Index(nameof(StationId), nameof(Year), nameof(Quarter))]
[Index(nameof(Severity))]
[Index(nameof(IsAcknowledged))]
[Index(nameof(CreateDateTimeUtc))]
[Comment("Обнаруженная аномалия пассажиропотока по станции за квартал")]
public class Anomaly
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    [Key, Comment("Идентификатор записи")]
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор станции метро.
    /// </summary>
    [Comment("Идентификатор станции метро")]
    public short StationId { get; set; }

    /// <summary>
    /// Год наблюдения аномалии.
    /// </summary>
    [Comment("Год наблюдения аномалии")]
    public int Year { get; set; }

    /// <summary>
    /// Квартал наблюдения аномалии (например, «Q1», «Q2»).
    /// </summary>
    [MaxLength(20), Comment("Квартал наблюдения аномалии (например, «Q1», «Q2»)")]
    public required string Quarter { get; set; }

    /// <summary>
    /// Тип аномалии.
    /// </summary>
    [Comment("Тип аномалии")]
    public AnomalyTypes AnomalyType { get; set; }

    /// <summary>
    /// Уровень серьёзности аномалии.
    /// </summary>
    [Comment("Уровень серьёзности аномалии")]
    public AnomalySeverities Severity { get; set; }

    /// <summary>
    /// Числовой показатель аномальности (например, Z-score или IF score).
    /// </summary>
    [Comment("Числовой показатель аномальности (например, Z-score или IF score)")]
    public double Score { get; set; }

    /// <summary>
    /// Фактическое значение пассажиропотока.
    /// </summary>
    [Comment("Фактическое значение пассажиропотока")]
    public int ActualValue { get; set; }

    /// <summary>
    /// Ожидаемое значение пассажиропотока (расчётное).
    /// </summary>
    [Comment("Ожидаемое значение пассажиропотока (расчётное)")]
    public int? ExpectedValue { get; set; }

    /// <summary>
    /// Текстовое описание аномалии.
    /// </summary>
    [MaxLength(500), Comment("Текстовое описание аномалии")]
    public string? Description { get; set; }

    /// <summary>
    /// Признак подтверждения аномалии оператором.
    /// </summary>
    [Comment("Признак подтверждения аномалии оператором")]
    public bool IsAcknowledged { get; set; }

    /// <summary>
    /// Дата и время подтверждения аномалии (UTC).
    /// </summary>
    [Comment("Дата и время подтверждения аномалии (UTC)")]
    public DateTime? AcknowledgedDateTimeUtc { get; set; }

    /// <summary>
    /// Дата и время создания записи (UTC).
    /// </summary>
    [Comment("Дата и время создания записи (UTC)")]
    public DateTime CreateDateTimeUtc { get; set; }

    /// <summary>
    /// Станция метро.
    /// </summary>
    [ForeignKey(nameof(StationId))]
    public Station? Station { get; set; }
}
