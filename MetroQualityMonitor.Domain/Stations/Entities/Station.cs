using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Stations.Entities;

/// <summary>
/// Станция метро.
/// </summary>
[Index(nameof(Name), IsUnique = true)]
[Comment("Станция метро")]
public class Station
{
    /// <summary>
    /// Конструктор, инициализирующий новый экземпляр класса <see cref="Station"/>.
    /// </summary>
    public Station()
    {
        Lines = new List<Line>();
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
    /// Линии  метро.
    /// </summary>
    [InverseProperty(nameof(Line.Stations))]
    public ICollection<Line>? Lines { get; }
}