using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MetroQualityMonitor.Infrastructure.Persistence;

/// <summary>
/// Фабрика контекста базы данных <see cref="MetroQualityMonitorDbContext"/>.
/// </summary>
public class MetroQualityMonitorDbContextFactory : IDesignTimeDbContextFactory<MetroQualityMonitorDbContext>
{
    /// <summary>
    /// Создание контекста БД приложения.
    /// </summary>
    /// <param name="args">Входные аргументы.</param>
    /// <returns>Контекст БД <see cref="MetroQualityMonitorDbContext"/>.</returns>
    public MetroQualityMonitorDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
#if DEBUG
            .AddJsonFile("appsettings.Development.json", optional: true)
#endif
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не указана в конфигурации.");

        var optionsBuilder = new DbContextOptionsBuilder<MetroQualityMonitorDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new MetroQualityMonitorDbContext(optionsBuilder.Options);
    }
}