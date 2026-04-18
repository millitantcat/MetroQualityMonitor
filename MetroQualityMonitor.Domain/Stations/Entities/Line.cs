using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Stations.Entities;

/// <summary>
/// Линия метро.
/// </summary>
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(MosDataId), IsUnique = true)]
[Index(nameof(GlobalId), IsUnique = true)]
[Comment("Линия метро")]
public class Line
{
    /// <summary>
    /// Конструктор, инициализирующий новый экземпляр класса <see cref="Line"/>.
    /// </summary>
    public Line()
    {
        Stations = new List<Station>();
    }

    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    [Key, Comment("Идентификатор записи")]
    public short Id { get; set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    [MaxLength(100), Comment("Наименование")]
    public required string Name { get; set; }

    /// <summary>
    /// Идентификатор в реестре открытых данных Москвы.
    /// </summary>
    [Comment("Идентификатор в реестре открытых данных Москвы")]
    public int? MosDataId { get; set; }

    /// <summary>
    /// Статус линии.
    /// </summary>
    [MaxLength(50), Comment("Статус линии")]
    public string? Status { get; set; }

    /// <summary>
    /// Глобальный идентификатор записи.
    /// </summary>
    [Comment("Глобальный идентификатор записи")]
    public long? GlobalId { get; set; }

    /// <summary>
    /// Станции метро.
    /// </summary>
    [InverseProperty(nameof(Station.Lines))]
    public ICollection<Station>? Stations { get; }
}