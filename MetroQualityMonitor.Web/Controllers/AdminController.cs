using MetroQualityMonitor.Application.MlService;
using Microsoft.AspNetCore.Mvc;

namespace MetroQualityMonitor.Web.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IMlServiceClient? mlServiceClient = null) : ControllerBase
{
    /// <summary>
    /// Принудительный запуск пересчёта ML-моделей: прогнозы → аномалии → кластеры.
    /// Если ML-сервис не сконфигурирован (MlService:BaseUrl пуст), возвращает 503.
    /// </summary>
    [HttpPost("recompute")]
    public async Task<IActionResult> RecomputeAsync(CancellationToken cancellationToken)
    {
        if (mlServiceClient is null)
        {
            return StatusCode(503, new
            {
                message = "ML-сервис не сконфигурирован. Укажите MlService:BaseUrl в appsettings."
            });
        }

        var isAlive = await mlServiceClient.HealthCheckAsync(cancellationToken);
        if (!isAlive)
        {
            return StatusCode(503, new { message = "ML-сервис недоступен." });
        }

        var forecast  = await mlServiceClient.RunForecastBatchAsync(cancellationToken);
        var anomalies = await mlServiceClient.RunAnomalyDetectionAsync(cancellationToken);
        var clusters  = await mlServiceClient.RecomputeClustersAsync(cancellationToken);

        return Ok(new
        {
            forecast  = new { forecast.Success,  forecast.Saved,  forecast.Error },
            anomalies = new { anomalies.Success, anomalies.Saved, anomalies.Error },
            clusters  = new { clusters.Success,  clusters.Saved,  clusters.Error },
        });
    }
}
