using MetroQualityMonitor.Application.Analytics.Services;
using MetroQualityMonitor.Application.Common.Abstractions.Storage;
using MetroQualityMonitor.Application.MlService;
using MetroQualityMonitor.Application.Test.Services;
using MetroQualityMonitor.Infrastructure.Analytics.Services;
using MetroQualityMonitor.Infrastructure.Common.Storage;
using MetroQualityMonitor.Infrastructure.MlService;
using MetroQualityMonitor.Infrastructure.Persistence;
using MetroQualityMonitor.Infrastructure.Test.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MetroQualityMonitor.Infrastructure;

/// <summary>
/// Класс-расширение <see cref="IServiceCollection"/> для регистрации зависимостей слоя инфраструктуры.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация сервисов слоя инфраструктуры.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="configuration">Конфигурация приложения, содержащая строку подключения и другие настройки.</param>
    /// <returns>Обновлённая коллекция сервисов.</returns>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если в конфигурации отсутствует строка подключения <c>DefaultConnection</c>.
    /// </exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не указана в конфигурации.");

        services.AddDbContext<MetroQualityMonitorDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddApplication();
        services.AddFileStorage();
        services.AddMlService(configuration);

        return services;
    }

    /// <summary>
    /// Регистрация сервисов прикладного слоя.
    /// </summary>
    private static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITestAppService, TestAppService>();

        services.AddScoped<ILineService, LineService>();
        services.AddScoped<IStationService, StationService>();
        services.AddScoped<IAnomalyService, AnomalyService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IVestibuleService, VestibuleService>();
        services.AddScoped<IClusterService, ClusterService>();

        return services;
    }

    /// <summary>
    /// Регистрация сервисов файлового хранилища.
    /// </summary>
    private static IServiceCollection AddFileStorage(this IServiceCollection services)
    {
        services.AddSingleton<IBlobStorage, FileSystemBlobStorage>();
        return services;
    }

    /// <summary>
    /// Регистрация HTTP-клиента ML-сервиса и фонового сервиса пересчёта.
    /// Если <c>MlService:BaseUrl</c> не задан, клиент не регистрируется.
    /// </summary>
    private static IServiceCollection AddMlService(
        this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["MlService:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            return services;

        services.AddHttpClient<IMlServiceClient, MlServiceClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout     = TimeSpan.FromMinutes(10);
        });

        services.AddSingleton(TimeProvider.System);
        services.AddHostedService<MlRecomputeBackgroundService>();

        return services;
    }
}
