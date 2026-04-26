using System.ComponentModel.DataAnnotations;
using MetroQualityMonitor.Domain.Analytics.Enums;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.Analytics.Entities;

/// <summary>
/// Справочник эмпирических часовых профилей пассажиропотока.
/// Используется для model-based деагрегации квартальных данных в часовые.
/// </summary>
[Index(nameof(StationCategory), nameof(DayType), nameof(Hour), IsUnique = true)]
[Comment("Справочник эмпирических часовых профилей пассажиропотока по категории станции и типу дня")]
public class HourlyProfile
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    [Key, Comment("Идентификатор записи")]
    public int Id { get; set; }

    /// <summary>
    /// Категория станции.
    /// </summary>
    [Comment("Категория станции")]
    public StationCategories StationCategory { get; set; }

    /// <summary>
    /// Тип дня.
    /// </summary>
    [Comment("Тип дня")]
    public DayTypes DayType { get; set; }

    /// <summary>
    /// Час суток (0–23).
    /// </summary>
    [Comment("Час суток (0–23)")]
    public int Hour { get; set; }

    /// <summary>
    /// Доля входящих пассажиров для данного часа (сумма по всем часам дня = 1.0).
    /// </summary>
    [Comment("Доля входящих пассажиров для данного часа (сумма по всем часам дня = 1.0)")]
    public double IncomingShare { get; set; }

    /// <summary>
    /// Доля исходящих пассажиров для данного часа (сумма по всем часам дня = 1.0).
    /// </summary>
    [Comment("Доля исходящих пассажиров для данного часа (сумма по всем часам дня = 1.0)")]
    public double OutgoingShare { get; set; }
}
