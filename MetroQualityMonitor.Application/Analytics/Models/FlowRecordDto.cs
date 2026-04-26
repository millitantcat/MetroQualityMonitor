namespace MetroQualityMonitor.Application.Analytics.Models;

/// <summary>
/// Запись квартального пассажиропотока.
/// </summary>
public class FlowRecordDto
{
    /// <summary>Год.</summary>
    public int Year { get; set; }

    /// <summary>Квартал (например, «Q1»).</summary>
    public required string Quarter { get; set; }

    /// <summary>Входящий пассажиропоток.</summary>
    public int IncomingPassengers { get; set; }

    /// <summary>Исходящий пассажиропоток.</summary>
    public int OutgoingPassengers { get; set; }
}
