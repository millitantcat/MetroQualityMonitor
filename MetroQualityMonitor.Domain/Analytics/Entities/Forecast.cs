using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroQualityMonitor.Domain.Stations.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Analytics.Entities;

/// <summary>
/// Прогноз пассажиропотока на квартал по линии или станции.
/// </summary>
[Index(nameof(LineId), nameof(Year), nameof(Quarter))]
[Index(nameof(StationId), nameof(Year), nameof(Quarter))]
[Index(nameof(CreateDateTimeUtc))]
[Comment("Прогноз пассажиропотока на квартал по линии или станции")]
public class Forecast
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    [Key, Comment("Идентификатор записи")]
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор линии метро (заполняется для прогноза на уровне линии).
    /// </summary>
    [Comment("Идентификатор линии метро (заполняется для прогноза на уровне линии)")]
    public short? LineId { get; set; }

    /// <summary>
    /// Идентификатор станции метро (заполняется для прогноза на уровне станции).
    /// </summary>
    [Comment("Идентификатор станции метро (заполняется для прогноза на уровне станции)")]
    public short? StationId { get; set; }

    /// <summary>
    /// Год прогноза.
    /// </summary>
    [Comment("Год прогноза")]
    public int Year { get; set; }

    /// <summary>
    /// Квартал прогноза (например, «Q1», «Q2»).
    /// </summary>
    [MaxLength(20), Comment("Квартал прогноза (например, «Q1», «Q2»)")]
    public required string Quarter { get; set; }

    /// <summary>
    /// Прогнозируемое количество входящих пассажиров.
    /// </summary>
    [Comment("Прогнозируемое количество входящих пассажиров")]
    public int PredictedIncoming { get; set; }

    /// <summary>
    /// Прогнозируемое количество исходящих пассажиров.
    /// </summary>
    [Comment("Прогнозируемое количество исходящих пассажиров")]
    public int PredictedOutgoing { get; set; }

    /// <summary>
    /// Нижняя граница доверительного интервала для входящих пассажиров.
    /// </summary>
    [Comment("Нижняя граница доверительного интервала для входящих пассажиров")]
    public int? ConfidenceLowerIncoming { get; set; }

    /// <summary>
    /// Верхняя граница доверительного интервала для входящих пассажиров.
    /// </summary>
    [Comment("Верхняя граница доверительного интервала для входящих пассажиров")]
    public int? ConfidenceUpperIncoming { get; set; }

    /// <summary>
    /// Нижняя граница доверительного интервала для исходящих пассажиров.
    /// </summary>
    [Comment("Нижняя граница доверительного интервала для исходящих пассажиров")]
    public int? ConfidenceLowerOutgoing { get; set; }

    /// <summary>
    /// Верхняя граница доверительного интервала для исходящих пассажиров.
    /// </summary>
    [Comment("Верхняя граница доверительного интервала для исходящих пассажиров")]
    public int? ConfidenceUpperOutgoing { get; set; }

    /// <summary>
    /// Название модели прогнозирования (например, «SARIMA», «Prophet»).
    /// </summary>
    [MaxLength(50), Comment("Название модели прогнозирования (например, «SARIMA», «Prophet»)")]
    public required string ModelName { get; set; }

    /// <summary>
    /// Версия модели прогнозирования (например, «v1.0»).
    /// </summary>
    [MaxLength(20), Comment("Версия модели прогнозирования (например, «v1.0»)")]
    public required string ModelVersion { get; set; }

    /// <summary>
    /// Дата и время создания записи (UTC).
    /// </summary>
    [Comment("Дата и время создания записи (UTC)")]
    public DateTime CreateDateTimeUtc { get; set; }

    /// <summary>
    /// Линия метро.
    /// </summary>
    [ForeignKey(nameof(LineId))]
    public Line? Line { get; set; }

    /// <summary>
    /// Станция метро.
    /// </summary>
    [ForeignKey(nameof(StationId))]
    public Station? Station { get; set; }
}
