namespace MetroQualityMonitor.Infrastructure.Common.Storage;

/// <summary>
/// Параметры файлового хранилища в файловой системе.
/// </summary>
public class FileSystemBlobStorageConfiguration
{
    /// <summary>
    /// Корневая директория.
    /// </summary>
    public required string RootPath { get; set; }
}
