using System.ComponentModel;

namespace MetroQualityMonitor.Domain.DataImports.Enums;

/// <summary>
/// Статус загрузки и обработки данных.
/// </summary>
public enum DataImportStatuses : short
{
    /// <summary>
    /// Ошибка.
    /// </summary>
    [Description("Ошибка")]
    Failed = -1,
        
    /// <summary>
    /// Новый.
    /// </summary>
    [Description("Новый")]
    New = 1,
    
    /// <summary>
    /// Загружен.
    /// </summary>
    [Description("Загружен")]
    Uploaded = 2,
    
    /// <summary>
    /// В обработке.
    /// </summary>
    [Description("В обработке")]
    Processing = 3,
    
    /// <summary>
    /// Обработан.
    /// </summary>
    [Description("Обработан")]
    Processed = 4
}