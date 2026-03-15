using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Test.Entities;

[Index(nameof(Name), IsUnique = true, Name = "IX_TestEntities_Name")]
public class TestEntity
{
    [Comment("Идентификатор записи")]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Comment("Наименование тестовой записи")]
    public required string Name { get; set; }
    
    [Comment("Дата и время создания записи в UTC")]
    public DateTime CreateDateTimeUtc { get; set; }
}