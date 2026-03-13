using System.ComponentModel;

namespace MetroQualityMonitor.Domain.DataImports.Entities;

/// <summary>
/// Тип загруженных данных.
/// </summary>
public enum DataImportTypes : short
{
    /// <summary>
    /// Неизвестно.
    /// </summary>
    [Description("Неизвестно")]
    Unknown = -1,
    
    /// <summary>
    /// 
    /// </summary>
    [Description("")]
    MetroEntrances = 1,
    
    /// <summary>
    /// 
    /// </summary>
    [Description("")]
    StationPassengerFlow = 2,
    
    /// <summary>
    /// 
    /// </summary>
    [Description("")]
    TransportHubs = 3
}