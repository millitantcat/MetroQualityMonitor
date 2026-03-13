using System.ComponentModel;

namespace MetroQualityMonitor.Domain.DataImports.Entities;

/// <summary>
/// Методы загрузки справочников.
/// </summary>
public enum DataImportSourceTypes : short
{
    /// <summary>
    /// Неизвестно.
    /// </summary>
    [Description("Неизвестно")]
    Unknown = -1,
    
    /// <summary>
    /// Загрузкам API.
    /// </summary>
    [Description("API")]
    Api = 1,
    
    /// <summary>
    /// Загрузка CSV.
    /// </summary>
    [Description("CSV")]
    Csv = 2,
    
    /// <summary>
    /// Загрузка JSON.
    /// </summary>
    [Description("JSON")]
    Json = 3,
    
    /// <summary>
    /// Ручная загрузка.
    /// </summary>
    [Description("Ручная загрузка")]
    Manual = 4
}