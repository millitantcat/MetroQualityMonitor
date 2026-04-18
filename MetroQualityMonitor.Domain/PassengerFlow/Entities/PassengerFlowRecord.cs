using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroQualityMonitor.Domain.Stations.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.PassengerFlow.Entities;

/// <summary>
/// Запись о пассажиропотоке станции метро за квартал.
/// </summary>
[Index(nameof(GlobalId), IsUnique = true)]
[Index(nameof(StationId), nameof(Year), nameof(Quarter))]
[Comment("Пассажиропоток по станции метро за квартал")]
public class PassengerFlowRecord
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    [Key, Comment("Идентификатор записи")]
    public int Id { get; set; }

    /// <summary>
    /// Глобальный идентификатор записи.
    /// </summary>
    [Comment("Глобальный идентификатор записи")]
    public long GlobalId { get; set; }

    /// <summary>
    /// Год.
    /// </summary>
    [Comment("Год")]
    public int Year { get; set; }

    /// <summary>
    /// Квартал.
    /// </summary>
    [MaxLength(20), Comment("Квартал")]
    public required string Quarter { get; set; }

    /// <summary>
    /// Количество входящих пассажиров.
    /// </summary>
    [Comment("Количество входящих пассажиров")]
    public int IncomingPassengers { get; set; }

    /// <summary>
    /// Количество исходящих пассажиров.
    /// </summary>
    [Comment("Количество исходящих пассажиров")]
    public int OutgoingPassengers { get; set; }

    /// <summary>
    /// Идентификатор станции метро.
    /// </summary>
    [Comment("Идентификатор станции метро")]
    public short? StationId { get; set; }

    /// <summary>
    /// Идентификатор линии метро.
    /// </summary>
    [Comment("Идентификатор линии метро")]
    public short? LineId { get; set; }

    /// <summary>
    /// Станция метро.
    /// </summary>
    [ForeignKey(nameof(StationId))]
    public Station? Station { get; set; }

    /// <summary>
    /// Линия метро.
    /// </summary>
    [ForeignKey(nameof(LineId))]
    public Line? Line { get; set; }
}
