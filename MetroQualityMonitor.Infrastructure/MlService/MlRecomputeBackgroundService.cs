using MetroQualityMonitor.Application.MlService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MetroQualityMonitor.Infrastructure.MlService;

/// <summary>
/// Фоновый сервис, запускающий пересчёт ML-моделей раз в сутки.
/// </summary>
public sealed class MlRecomputeBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<MlRecomputeBackgroundService> logger,
    TimeProvider timeProvider)
    : BackgroundService
{
    /// <summary>Задержка перед первым запуском после старта приложения.</summary>
    private static readonly TimeSpan InitialDelay   = TimeSpan.FromMinutes(1);

    /// <summary>Интервал между полными циклами пересчёта.</summary>
    private static readonly TimeSpan RecomputePeriod = TimeSpan.FromHours(24);

    /// <summary>Максимальное время выполнения одного шага ML-пересчёта.</summary>
    private static readonly TimeSpan StepTimeout = TimeSpan.FromMinutes(10);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "ML BackgroundService запущен. Первый пересчёт через {Delay}.",
            InitialDelay);

        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCycleAsync(stoppingToken);
            await Task.Delay(RecomputePeriod, stoppingToken);
        }
    }

    /// <summary>
    /// Выполняет один полный цикл пересчёта.
    /// При недоступности ML-сервиса цикл пропускается без исключения.
    /// </summary>
    private async Task RunCycleAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ML пересчёт: цикл запущен.");
        var cycleStart = timeProvider.GetUtcNow();

        using var scope = scopeFactory.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IMlServiceClient>();

        // Healthcheck
        var isAlive = await client.HealthCheckAsync(stoppingToken);
        if (!isAlive)
        {
            logger.LogWarning("ML-сервис недоступен — пересчёт пропущен.");
            return;
        }

        // Прогнозы
        await RunStepAsync("Прогнозы (SARIMA)", () => client.RunForecastBatchAsync(stoppingToken));

        // Аномалии
        await RunStepAsync("Аномалии (Z-score + IF + YoY)", () => client.RunAnomalyDetectionAsync(stoppingToken));

        // Кластеры
        await RunStepAsync("Кластеризация (K-Means)", () => client.RecomputeClustersAsync(stoppingToken));

        var elapsed = timeProvider.GetUtcNow() - cycleStart;
        logger.LogInformation("ML пересчёт завершён за {Elapsed:mm\\:ss}.", elapsed);
    }

    /// <summary>
    /// Запускает один шаг ML-пересчёта с тайм-аутом <see cref="StepTimeout"/> и структурированным логированием.
    /// </summary>
    /// <param name="stepName">Читаемое название шага для лога.</param>
    /// <param name="action">Функция, вызывающая соответствующий метод ML-клиента.</param>
    private async Task RunStepAsync(string stepName, Func<Task<Application.MlService.Models.MlBatchResultDto>> action)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var cts = new CancellationTokenSource(StepTimeout);
            var result = await action();

            sw.Stop();
            if (result.Success)
            {
                logger.LogInformation(
                    "ML шаг [{Step}] выполнен за {Ms} мс. Сохранено: {Saved}.",
                    stepName, sw.ElapsedMilliseconds, result.Saved?.ToString() ?? "—");
            }
            else
            {
                logger.LogError(
                    "ML шаг [{Step}] завершился ошибкой за {Ms} мс: {Error}",
                    stepName, sw.ElapsedMilliseconds, result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            logger.LogError(
                "ML шаг [{Step}] превысил таймаут {Timeout} мин.",
                stepName, StepTimeout.TotalMinutes);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "ML шаг [{Step}] упал с исключением.", stepName);
        }
    }
}
