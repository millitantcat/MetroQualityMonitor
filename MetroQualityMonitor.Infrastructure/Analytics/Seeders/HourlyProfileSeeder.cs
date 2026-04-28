using MetroQualityMonitor.Domain.Analytics.Entities;
using MetroQualityMonitor.Domain.Analytics.Enums;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Infrastructure.Analytics.Seeders;

/// <summary>
/// Заполняет таблицу <see cref="HourlyProfile"/> стартовыми эмпирическими коэффициентами
/// для model-based деагрегации квартального пассажиропотока в часовой.
/// Запускается при первом старте; повторный запуск пропускается, если таблица непустая.
/// </summary>
public class HourlyProfileSeeder(MetroQualityMonitorDbContext db)
{
    /// <summary>
    /// Засевает таблицу часовых профилей, если она ещё пуста.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.HourlyProfiles.AnyAsync(cancellationToken))
            return;

        db.HourlyProfiles.AddRange(GenerateProfiles());
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Генерирует записи <see cref="HourlyProfile"/> для всех комбинаций категории станции и типа дня.
    /// Сырые веса нормируются так, чтобы сумма долей по каждому дню была равна 1.0.
    /// </summary>
    private static IEnumerable<HourlyProfile> GenerateProfiles()
    {
        foreach (var ((category, dayType), (rawIn, rawOut)) in GetRawWeights())
        {
            var normIn  = Normalize(rawIn);
            var normOut = Normalize(rawOut);

            for (var hour = 0; hour < 24; hour++)
            {
                yield return new HourlyProfile
                {
                    StationCategory = category,
                    DayType         = dayType,
                    Hour            = hour,
                    IncomingShare   = Math.Round(normIn[hour],  6),
                    OutgoingShare   = Math.Round(normOut[hour], 6)
                };
            }
        }
    }

    /// <summary>Нормирует массив весов так, чтобы их сумма равнялась 1.0.</summary>
    private static double[] Normalize(double[] raw)
    {
        var sum = raw.Sum();
        return sum == 0 ? raw : raw.Select(v => v / sum).ToArray();
    }

    /// <summary>
    /// Возвращает сырые веса (не нормированные) для каждой комбинации категория × тип дня.
    /// Индекс массива соответствует часу суток (0–23).
    /// Значения нормируются в <see cref="Normalize"/>, поэтому важны только соотношения.
    /// </summary>
    private static Dictionary<(StationCategories, DayTypes), (double[] In, double[] Out)> GetRawWeights()
    {
        // Часы:                                    0  1  2  3  4  5  6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23

        // Спальные районы: утренний пик вход (7–9), вечерний пик выход (18–19)
        double[] resWdIn  = [0, 0, 0, 0, 0, 1, 2,  8, 14, 10,  5,  4,  4,  4,  4,  4,  4,  5,  6,  5,  4,  3,  2, 1];
        double[] resWdOut = [0, 0, 0, 0, 0, 1, 1,  2,  2,  3,  4,  4,  4,  4,  4,  4,  5,  9, 13, 10,  7,  5,  3, 1];

        // Центральные: вечерний пик вход (18–19), утренний пик выход (8–9)
        double[] cenWdIn  = [0, 0, 0, 0, 0, 1, 2,  4,  5,  5,  6,  7,  8,  7,  7,  6,  6,  7, 12, 10,  7,  5,  3, 1];
        double[] cenWdOut = [0, 0, 0, 0, 0, 1, 2,  5, 13, 10,  7,  7,  7,  6,  6,  6,  6,  7,  5,  4,  3,  2,  2, 1];

        // Пересадочные: два симметричных пика (8–9 и 18–19)
        double[] traWdIn  = [0, 0, 0, 0, 0, 1, 2,  6, 10, 10,  6,  5,  5,  5,  5,  5,  5,  8, 10,  8,  6,  4,  3, 1];
        double[] traWdOut = [0, 0, 0, 0, 0, 1, 2,  6, 10, 10,  6,  5,  5,  5,  5,  5,  5,  8, 10,  8,  6,  4,  3, 1];

        // Смешанные: усреднённый профиль
        double[] mixWdIn  = [0, 0, 0, 0, 0, 1, 2,  6,  9,  8,  6,  5,  5,  5,  5,  5,  5,  7,  9,  8,  6,  4,  2, 1];
        double[] mixWdOut = [0, 0, 0, 0, 0, 1, 1,  4,  8,  7,  6,  5,  5,  5,  5,  5,  5,  7,  9,  8,  6,  4,  2, 1];

        // Суббота: нет выраженного утреннего пика, умеренный вечерний
        double[] satIn  = [0, 0, 0, 0, 0, 1, 2,  3,  4,  5,  6,  7,  7,  7,  6,  6,  6,  7,  7,  6,  5,  4,  2, 1];
        double[] satOut = [0, 0, 0, 0, 0, 1, 1,  2,  3,  5,  6,  7,  7,  7,  6,  6,  6,  7,  7,  7,  5,  4,  2, 1];

        // Воскресенье/праздник: более поздний старт, вечерний пик смещён на 18–20
        double[] sunIn  = [0, 0, 0, 0, 0, 0, 1,  2,  3,  5,  6,  7,  7,  7,  7,  6,  6,  7,  8,  7,  5,  4,  2, 1];
        double[] sunOut = [0, 0, 0, 0, 0, 0, 1,  2,  3,  5,  6,  7,  7,  7,  7,  6,  6,  7,  8,  7,  5,  4,  2, 1];

        return new Dictionary<(StationCategories, DayTypes), (double[], double[])>
        {
            [(StationCategories.Residential, DayTypes.Weekday)]  = (resWdIn, resWdOut),
            [(StationCategories.Residential, DayTypes.Saturday)] = (satIn,   satOut),
            [(StationCategories.Residential, DayTypes.Sunday)]   = (sunIn,   sunOut),
            [(StationCategories.Residential, DayTypes.Holiday)]  = (sunIn,   sunOut),

            [(StationCategories.Central, DayTypes.Weekday)]      = (cenWdIn, cenWdOut),
            [(StationCategories.Central, DayTypes.Saturday)]     = (satIn,   satOut),
            [(StationCategories.Central, DayTypes.Sunday)]       = (sunIn,   sunOut),
            [(StationCategories.Central, DayTypes.Holiday)]      = (sunIn,   sunOut),

            [(StationCategories.Transfer, DayTypes.Weekday)]     = (traWdIn, traWdOut),
            [(StationCategories.Transfer, DayTypes.Saturday)]    = (satIn,   satOut),
            [(StationCategories.Transfer, DayTypes.Sunday)]      = (sunIn,   sunOut),
            [(StationCategories.Transfer, DayTypes.Holiday)]     = (sunIn,   sunOut),

            [(StationCategories.Mixed, DayTypes.Weekday)]        = (mixWdIn, mixWdOut),
            [(StationCategories.Mixed, DayTypes.Saturday)]       = (satIn,   satOut),
            [(StationCategories.Mixed, DayTypes.Sunday)]         = (sunIn,   sunOut),
            [(StationCategories.Mixed, DayTypes.Holiday)]        = (sunIn,   sunOut),
        };
    }
}
