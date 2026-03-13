using MetroQualityMonitor.Application.Common.Abstractions.Storage;
using MetroQualityMonitor.Application.Test.Services;
using MetroQualityMonitor.Infrastructure.Common.Storage;
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
        
        return services;
    }

    /// <summary>
    /// Регистрация сервисов прикладного слоя.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <returns>Обновлённая коллекция сервисов.</returns>
    private static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITestAppService, TestAppService>();
        return services;
    }
    
    /// <summary>
    /// Регистрация сервисов файлового хранилища.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <returns>Обновлённая коллекция сервисов.</returns>
    private static IServiceCollection AddFileStorage(this IServiceCollection services)
    {
        services.AddSingleton<IBlobStorage, FileSystemBlobStorage>();
        return services;
    }
}