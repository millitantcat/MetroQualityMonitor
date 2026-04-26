using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroQualityMonitor.Domain.Stations.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Analytics.Entities;

/// <summary>
/// Результат кластеризации станции метро по характеру пассажиропотока.
/// </summary>
[Index(nameof(StationId))]
[Index(nameof(ComputedAtDateTimeUtc))]
[Comment("Результат кластеризации станции метро по характеру пассажиропотока")]
public class StationCluster
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    [Key, Comment("Идентификатор записи")]
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор станции метро.
    /// </summary>
    [Comment("Идентификатор станции метро")]
    public short StationId { get; set; }

    /// <summary>
    /// Числовой идентификатор кластера.
    /// </summary>
    [Comment("Числовой идентификатор кластера")]
    public int ClusterId { get; set; }

    /// <summary>
    /// Метка кластера (например, «Residential», «Central»).
    /// </summary>
    [MaxLength(50), Comment("Метка кластера (например, «Residential», «Central»)")]
    public required string ClusterLabel { get; set; }

    /// <summary>
    /// Дата и время вычисления кластеризации (UTC).
    /// </summary>
    [Comment("Дата и время вычисления кластеризации (UTC)")]
    public DateTime ComputedAtDateTimeUtc { get; set; }

    /// <summary>
    /// Станция метро.
    /// </summary>
    [ForeignKey(nameof(StationId))]
    public Station? Station { get; set; }
}
