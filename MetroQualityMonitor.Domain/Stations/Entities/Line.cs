using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Stations.Entities;

/// <summary>
/// Линия метро.
/// </summary>
[Index(nameof(Name), IsUnique = true)]
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
    /// Станции метро.
    /// </summary>
    [InverseProperty(nameof(Station.Lines))]
    public ICollection<Station>? Stations { get; }
}