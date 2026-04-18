using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Vestibules.Entities;

/// <summary>
/// Сведения о ремонте эскалаторов вестибюля.
/// </summary>
[Index(nameof(GlobalId), IsUnique = true)]
[Index(nameof(VestibuleId))]
[Comment("Сведения о ремонте эскалаторов вестибюля")]
public class EscalatorRepair
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
    /// Период ремонта в формате «27.02.2023-30.05.2023».
    /// </summary>
    [MaxLength(50), Comment("Период ремонта (вида \"27.02.2023-30.05.2023\")")]
    public required string RepairPeriod { get; set; }

    /// <summary>
    /// Признак удалённой записи.
    /// </summary>
    [Comment("Признак удалённой записи")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Идентификатор вестибюля.
    /// </summary>
    [Comment("Идентификатор вестибюля")]
    public int VestibuleId { get; set; }

    /// <summary>
    /// Вестибюль.
    /// </summary>
    [ForeignKey(nameof(VestibuleId))]
    public Vestibule? Vestibule { get; set; }
}
