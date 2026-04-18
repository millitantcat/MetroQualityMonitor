using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetroQualityMonitor.Domain.Stations.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Vestibules.Entities;

/// <summary>
/// Вестибюль (вход/выход) станции метро.
/// </summary>
[Index(nameof(MosDataId), IsUnique = true)]
[Index(nameof(GlobalId), IsUnique = true)]
[Index(nameof(StationId))]
[Index(nameof(LineId))]
[Comment("Вестибюль (вход/выход) станции метро")]
public class Vestibule
{
    /// <summary>
    /// Конструктор, инициализирующий новый экземпляр класса <see cref="Vestibule"/>.
    /// </summary>
    public Vestibule()
    {
        EscalatorRepairs = new List<EscalatorRepair>();
    }

    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    [Key, Comment("Идентификатор записи")]
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор в реестре открытых данных Москвы.
    /// </summary>
    [Comment("Идентификатор в реестре открытых данных Москвы")]
    public int MosDataId { get; set; }

    /// <summary>
    /// Глобальный идентификатор записи.
    /// </summary>
    [Comment("Глобальный идентификатор записи")]
    public long GlobalId { get; set; }

    /// <summary>
    /// Наименование вестибюля.
    /// </summary>
    [MaxLength(255), Comment("Наименование вестибюля")]
    public required string Name { get; set; }

    /// <summary>
    /// Расположен на территории Москвы.
    /// </summary>
    [Comment("Расположен на территории Москвы")]
    public bool? OnTerritoryOfMoscow { get; set; }

    /// <summary>
    /// Административный округ.
    /// </summary>
    [MaxLength(150), Comment("Административный округ")]
    public string? AdmArea { get; set; }

    /// <summary>
    /// Район.
    /// </summary>
    [MaxLength(150), Comment("Район")]
    public string? District { get; set; }

    /// <summary>
    /// Долгота (WGS84).
    /// </summary>
    [Comment("Долгота (WGS84)")]
    public double? LongitudeWgs84 { get; set; }

    /// <summary>
    /// Широта (WGS84).
    /// </summary>
    [Comment("Широта (WGS84)")]
    public double? LatitudeWgs84 { get; set; }

    /// <summary>
    /// Тип вестибюля.
    /// </summary>
    [MaxLength(100), Comment("Тип вестибюля")]
    public string? VestibuleType { get; set; }

    /// <summary>
    /// Статус объекта культурного наследия.
    /// </summary>
    [MaxLength(300), Comment("Статус объекта культурного наследия")]
    public string? CulturalHeritageSiteStatus { get; set; }

    /// <summary>
    /// Режим работы по чётным дням.
    /// </summary>
    [MaxLength(1000), Comment("Режим работы по чётным дням")]
    public string? ModeOnEvenDays { get; set; }

    /// <summary>
    /// Режим работы по нечётным дням.
    /// </summary>
    [MaxLength(1000), Comment("Режим работы по нечётным дням")]
    public string? ModeOnOddDays { get; set; }

    /// <summary>
    /// Количество полнофункциональных БПА.
    /// </summary>
    [Comment("Количество полнофункциональных БПА")]
    public int? FullFeaturedBPAAmount { get; set; }

    /// <summary>
    /// Количество малофункциональных БПА.
    /// </summary>
    [Comment("Количество малофункциональных БПА")]
    public int? LittleFunctionalBPAAmount { get; set; }

    /// <summary>
    /// Общее количество БПА.
    /// </summary>
    [Comment("Общее количество БПА")]
    public int? BPAAmount { get; set; }

    /// <summary>
    /// Статус объекта.
    /// </summary>
    [MaxLength(50), Comment("Статус объекта")]
    public string? ObjectStatus { get; set; }

    /// <summary>
    /// Номер релиза набора данных.
    /// </summary>
    [Comment("Номер релиза набора данных")]
    public int? ReleaseNumber { get; set; }

    /// <summary>
    /// Номер версии набора данных.
    /// </summary>
    [Comment("Номер версии набора данных")]
    public int? VersionNumber { get; set; }

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

    /// <summary>
    /// Сведения о ремонтах эскалаторов.
    /// </summary>
    [InverseProperty(nameof(EscalatorRepair.Vestibule))]
    public ICollection<EscalatorRepair> EscalatorRepairs { get; }
}
