using System.Net.Http.Json;
using MetroQualityMonitor.Application.MlService;
using MetroQualityMonitor.Application.MlService.Models;

namespace MetroQualityMonitor.Infrastructure.MlService;

/// <summary>
/// HTTP-клиент для взаимодействия с Python ML-сервисом.
/// </summary>
public class MlServiceClient(HttpClient http) : IMlServiceClient
{
    /// <inheritdoc/>
    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await http.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<MlBatchResultDto> RunForecastBatchAsync(CancellationToken cancellationToken = default)
        => await PostAsync("/forecast/run-batch", cancellationToken);

    /// <inheritdoc/>
    public async Task<MlBatchResultDto> RunAnomalyDetectionAsync(CancellationToken cancellationToken = default)
        => await PostAsync("/anomalies/detect", cancellationToken);

    /// <inheritdoc/>
    public async Task<MlBatchResultDto> RecomputeClustersAsync(CancellationToken cancellationToken = default)
        => await PostAsync("/clusters/recompute", cancellationToken);

    /// <inheritdoc/>
    public async Task<MlBatchResultDto> SeedProfilesAsync(bool overwrite = false, CancellationToken cancellationToken = default)
        => await PostAsync($"/profiles/seed?overwrite={overwrite}", cancellationToken);
    
    /// <summary>
    /// Выполняет POST-запрос к ML-сервису и преобразует ответ в <see cref="MlBatchResultDto"/>.
    /// </summary>
    private async Task<MlBatchResultDto> PostAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var response = await http.PostAsync(path, content: null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new MlBatchResultDto
                {
                    Success = false,
                    Error   = $"HTTP {(int)response.StatusCode}: {body}",
                };
            }

            var raw = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken);
            var savedRaw = raw?.GetValueOrDefault("saved");
            int? saved = savedRaw is System.Text.Json.JsonElement elem && elem.ValueKind == System.Text.Json.JsonValueKind.Number
                ? elem.GetInt32()
                : null;

            return new MlBatchResultDto { Success = true, Saved = saved, Raw = raw };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new MlBatchResultDto
            {
                Success = false,
                Error   = ex.Message,
            };
        }
    }
}
