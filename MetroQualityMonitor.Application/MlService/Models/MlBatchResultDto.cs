namespace MetroQualityMonitor.Application.MlService.Models;

/// <summary>
/// Результат batch-запроса к Python ML-сервису.
/// </summary>
public class MlBatchResultDto
{
    /// <summary>
    /// Признак успешного выполнения.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Сообщение об ошибке (заполняется при Success = false).
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Количество сохранённых записей (если возвращает ML-сервис).
    /// </summary>
    public int? Saved { get; init; }

    /// <summary>
    /// Сырой JSON-ответ ML-сервиса (для диагностики).
    /// </summary>
    public object? Raw { get; init; }
}
