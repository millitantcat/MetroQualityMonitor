using MetroQualityMonitor.Application.Common.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace MetroQualityMonitor.Infrastructure.Common.Storage;

/// <summary>
/// Файловое хранилище в файловой системе.
/// </summary>
public class FileSystemBlobStorage : IBlobStorage
{
    private readonly FileSystemBlobStorageConfiguration _configuration;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="FileSystemBlobStorage"/>.
    /// </summary>
    public FileSystemBlobStorage(IOptions<FileSystemBlobStorageConfiguration> options)
    {
        _configuration = options.Value ??
            throw new NullReferenceException("Параметры файлового хранилища в файловой системе должны быть заполнены.");
        if (_configuration.RootPath is null)
            throw new NullReferenceException("Не указан RootPath в настройках файлового хранилища.");
    }

    /// <inheritdoc />
    public Task<string> CopyAsync(
        string sourceStorageName,
        string sourceFileId,
        string? targetStorageName,
        string? targetFileName = null,
        CancellationToken cancellationToken = default)
    {
        var sourceStoragePath = Path.Combine(_configuration.RootPath, sourceStorageName);
        Directory.CreateDirectory(sourceStoragePath);

        var sourceFilePath = Path.Combine(sourceStoragePath, sourceFileId);

        var targetStorage = Path.Combine(_configuration.RootPath, targetStorageName ?? sourceStorageName);
        Directory.CreateDirectory(targetStorage);

        var targetFilePath = Path.Combine(_configuration.RootPath, targetStorageName ?? sourceStorageName, targetFileName ?? sourceFileId);

        File.Copy(sourceFilePath, targetFilePath);

        return Task.FromResult(targetFileName ?? sourceFileId);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(string storageName, CancellationToken cancellationToken)
    {
        var storagePath = Path.Combine(_configuration.RootPath, storageName);
        Directory.CreateDirectory(storagePath);

        var result = Directory.EnumerateFiles(storagePath)
            .Count();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string storageName, string fileId, CancellationToken cancellationToken)
    {
        var storagePath = Path.Combine(_configuration.RootPath, storageName);
        Directory.CreateDirectory(storagePath);

        var filePath = Path.Combine(storagePath, fileId);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteStorageAsync(string storageName, CancellationToken cancellationToken)
    {
        var storagePath = Path.Combine(_configuration.RootPath, storageName);
        if (Directory.Exists(storagePath))
        {
            Directory.Delete(storagePath, true);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Stream> DownloadAsync(string storageName, string fileId, CancellationToken cancellationToken)
    {
        var storagePath = Path.Combine(_configuration.RootPath, storageName);
        Directory.CreateDirectory(storagePath);

        var filePath = Path.Combine(storagePath, fileId);
        return Task.FromResult((Stream)File.OpenRead(filePath));
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string storageName, string fileId, CancellationToken cancellationToken)
    {
        var storagePath = Path.Combine(_configuration.RootPath, storageName);
        Directory.CreateDirectory(storagePath);

        var filePath = Path.Combine(storagePath, fileId);
        return Task.FromResult(File.Exists(filePath));
    }

    /// <inheritdoc />
    public Task<string[]> FetchAsync(string storageName, int count, CancellationToken cancellationToken)
    {
        var storagePath = Path.Combine(_configuration.RootPath, storageName);
        Directory.CreateDirectory(storagePath);

        var directoryInformation = new DirectoryInfo(storagePath);
        var result = directoryInformation.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
            .OrderBy(x => x.CreationTimeUtc)
            .ThenBy(x => x.LastWriteTimeUtc)
            .Take(count)
            .Select(x => x.Name)
            .ToArray();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<string> MoveAsync(string sourceStorageName, string sourceFileId, string? targetStorageName, string? targetFileName = null, CancellationToken cancellationToken = default)
    {
        var sourceStoragePath = Path.Combine(_configuration.RootPath, sourceStorageName);
        Directory.CreateDirectory(sourceStoragePath);

        var sourceFilePath = Path.Combine(sourceStoragePath, sourceFileId);
        var targetStoragePath = Path.Combine(_configuration.RootPath, targetStorageName ?? sourceStorageName);
        Directory.CreateDirectory(targetStoragePath);

        var targetFilePath = Path.Combine(targetStoragePath, targetFileName ?? sourceFileId);

        File.Move(sourceFilePath, targetFilePath);

        return Task.FromResult(targetFileName ?? sourceFileId);
    }

    /// <inheritdoc />
    public Task<string> UploadAsync(
        string storageName,
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var storagePath = Path.Combine(_configuration.RootPath, storageName);
        Directory.CreateDirectory(storagePath);

        var filePath = Path.Combine(storagePath, fileName);
        using (var storageFileStream = File.Create(filePath))
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.CopyTo(storageFileStream);
            fileStream.Flush();
        }

        return Task.FromResult(fileName);
    }
}
