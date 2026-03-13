namespace MetroQualityMonitor.Application.Common.Abstractions.Storage;

/// <summary>
/// Базовый интерфейс для работы с файловым хранилищем.
/// </summary>
public interface IBlobStorage
{
    /// <summary>
    /// Загрузить файл.
    /// </summary>
    /// <param name="storageName">Наименование хранилища.</param>
    /// <param name="fileStream">Поток с данными файла.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <param name="cancellationToken">Экземпляр класса <see cref="CancellationToken"/> для отмены операции.</param>
    /// <returns>Идентификатор файла в хранилище.</returns>
    Task<string> UploadAsync(
        string storageName,
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить файл из хранилища.
    /// </summary>
    /// <param name="storageName">Наименование хранилища.</param>
    /// <param name="fileId">Идентификатор файла в хранилище.</param>
    /// <param name="cancellationToken">Экземпляр класса <see cref="CancellationToken"/> для отмены операции.</param>
    /// <returns>Поток с данными файла.</returns>
    Task<Stream> DownloadAsync(string storageName, string fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить файл из хранилища
    /// </summary>
    /// <param name="storageName">Наименование хранилища.</param>
    /// <param name="fileId">Идентификатор файла в хранилище.</param>
    /// <param name="cancellationToken">Экземпляр класса <see cref="CancellationToken"/> для отмены операции.</param>
    Task DeleteAsync(string storageName, string fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Проверить существование файла.
    /// </summary>
    /// <param name="storageName">Наименование хранилища.</param>
    /// <param name="fileId">Идентификатор файла в хранилище.</param>
    /// <param name="cancellationToken">Экземпляр класса <see cref="CancellationToken"/> для отмены операции.</param>
    /// <returns>Признак наличия файла.</returns>
    Task<bool> ExistsAsync(string storageName, string fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Копировать файл между хранилищами.
    /// </summary>
    /// <param name="sourceStorageName">Наименование исходного хранилища.</param>
    /// <param name="sourceFileId">Идентификатор исходного файла.</param>
    /// <param name="targetStorageName">Наименование целевого хранилища.</param>
    /// <param name="targetFileName">Новое имя файла (если нужно переименовать).</param>
    /// <param name="cancellationToken">Экземпляр класса <see cref="CancellationToken"/> для отмены операции.</param>
    /// <returns>Идентификатор файла в хранилище.</returns>
    Task<string> CopyAsync(
        string sourceStorageName,
        string sourceFileId,
        string? targetStorageName,
        string? targetFileName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Переместить файл между хранилищами.
    /// </summary>
    /// <param name="sourceStorageName">Наименование исходного хранилища.</param>
    /// <param name="sourceFileId">Идентификатор исходного файла.</param>
    /// <param name="targetStorageName">Наименование целевого хранилища.</param>
    /// <param name="targetFileName">Новое имя файла (если нужно переименовать).</param>
    /// <param name="cancellationToken">Экземпляр класса <see cref="CancellationToken"/> для отмены операции.</param>
    /// <returns>Идентификатор файла в хранилище.</returns>
    Task<string> MoveAsync(
        string sourceStorageName,
        string sourceFileId,
        string? targetStorageName,
        string? targetFileName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить перечень последних загруженных файлов из хранилища.
    /// </summary>
    /// <param name="storageName">Наименование хранилища.</param>
    /// <param name="count">Количество файлов.</param>
    /// <param name="cancellationToken">Экземпляр класса <see cref="CancellationToken"/> для отмены операции.</param>
    /// <returns>Идентификаторы файлов.</returns>
    Task<string[]> FetchAsync(string storageName, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Подсчет количества файлов в хранилище.
    /// </summary>
    /// <param name="storageName">Наименование хранилища.</param>
    /// <param name="cancellationToken">Экземпляр класса <see cref="CancellationToken"/> для отмены операции.</param>
    /// <returns>Количество файлов в хранилище.</returns>
    Task<int> CountAsync(string storageName, CancellationToken cancellationToken);

    /// <summary>
    /// Удаление хранилища (вместе с содержимым).
    /// </summary>
    /// <param name="storageName">Наименование хранилища.</param>
    /// <param name="cancellationToken">Экземпляр класса <see cref="CancellationToken"/> для отмены операции.</param>
    Task DeleteStorageAsync(string storageName, CancellationToken cancellationToken);
}