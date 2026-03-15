using System.ComponentModel.DataAnnotations;
using MetroQualityMonitor.Domain.DataImports.Enums;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Domain.DataImports.Entities;

/// <summary>
/// Журнал загрузок исходных наборов данных.
/// </summary>
[Index(nameof(Status))]
[Index(nameof(DataType))]
[Index(nameof(LoadedDateTimeUtc))]
[Index(nameof(Sha256Hash))]
[Comment("Журнал загрузок исходных наборов данных")]
public class DataImportRun
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    [Key, Comment("Идентификатор записи")]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Наименование исходного файла.
    /// </summary>
    [MaxLength(255), Comment("Наименование исходного файла")]
    public required string FileName  { get; set; }
    
    /// <summary>
    /// Ключ файла в хранилище.
    /// </summary>
    [MaxLength(500),  Comment("Ключ файла в хранилище")]
    public required string BlobName { get; set; }
    
    /// <summary>
    /// Метод загрузки данных.
    /// </summary>
    [Comment("Метод загрузки данных")]
    public DataImportSourceTypes SourceType { get; set; }
    
    /// <summary>
    /// Тип загруженных данных.
    /// </summary>
    [Comment("Тип загруженного справочника")]
    public DataImportTypes DataType { get; set; }
    
    /// <summary>
    /// Статус загрузки и обработки данных.
    /// </summary>
    [Comment("Статус загрузки и обработки данных")]
    public DataImportStatuses Status { get; set; }
    
    /// <summary>
    /// Сообщение ошибки.
    /// </summary>
    [MaxLength(2000), Comment("Сообщение ошибки")]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Количество загруженных строк.
    /// </summary>
    [Comment("Количество загруженных строк")]
    public int? RowCount  { get; set; }
    
    /// <summary>
    /// SHA256 хэш.
    /// </summary>
    [MaxLength(64), Comment("SHA256 хэш")]
    public required string Sha256Hash { get; set; }
    
    /// <summary>
    /// Дата и время (UTC) загрузки файла.
    /// </summary>
    [Comment("Дата и время (UTC) загрузки файла")]
    public DateTime LoadedDateTimeUtc { get; set; }   
    
    /// <summary>
    /// Дата и время (UTC) начала обработки.
    /// </summary>
    [Comment("Дата и время (UTC) начала обработки")]
    public DateTime? ProcessingStartDateTimeUtc { get; set; }
    
    /// <summary>
    /// Дата и время (UTC) завершения обработки.
    /// </summary>
    [Comment("Дата и время (UTC) завершения обработки")]
    public DateTime? ProcessingFinishedDateTimeUtc { get; set; }
    
    /// <summary>
    /// Дата и время (UTC) создания записи.
    /// </summary>
    [Comment("Дата и время (UTC) создания записи")]
    public DateTime CreateDateTimeUtc { get; set; }
}