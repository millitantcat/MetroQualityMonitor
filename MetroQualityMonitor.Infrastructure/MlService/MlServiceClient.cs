using System.Net.Http.Json;
using MetroQualityMonitor.Application.MlService;
using MetroQualityMonitor.Application.MlService.Models;

namespace MetroQualityMonitor.Infrastructure.MlService;

/// <summary>
/// HTTP-клиент для взаимодействия с Python ML-сервисом.
/// Базовый адрес задаётся через <c>MlService:BaseUrl</c> в конфигурации.
/// </summary>
public class MlServiceClient(HttpClient http) : IMlServiceClient
{
    /// <inheritdoc/>
    public async Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await http.GetAsync("/health", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<MlBatchResultDto> RunForecastBatchAsync(CancellationToken ct = default)
        => await PostAsync("/forecast/run-batch", ct);

    /// <inheritdoc/>
    public async Task<MlBatchResultDto> RunAnomalyDetectionAsync(CancellationToken ct = default)
        => await PostAsync("/anomalies/detect", ct);

    /// <inheritdoc/>
    public async Task<MlBatchResultDto> RecomputeClustersAsync(CancellationToken ct = default)
        => await PostAsync("/clusters/recompute", ct);

    /// <inheritdoc/>
    public async Task<MlBatchResultDto> SeedProfilesAsync(bool overwrite = false, CancellationToken ct = default)
        => await PostAsync($"/profiles/seed?overwrite={overwrite}", ct);

    // -----------------------------------------------------------------------

    private async Task<MlBatchResultDto> PostAsync(string path, CancellationToken ct)
    {
        try
        {
            var response = await http.PostAsync(path, content: null, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                return new MlBatchResultDto
                {
                    Success = false,
                    Error   = $"HTTP {(int)response.StatusCode}: {body}",
                };
            }

            var raw = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(ct);
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
