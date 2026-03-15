using MetroQualityMonitor.Domain.DataImports.Entities;
using MetroQualityMonitor.Domain.Stations.Entities;
using MetroQualityMonitor.Domain.Test.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetroQualityMonitor.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных приложения.
/// </summary>
/// <param name="options">Настройки контекста БД.</param>
public class MetroQualityMonitorDbContext(DbContextOptions<MetroQualityMonitorDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Тестовая Entity.
    /// </summary>
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    
    /// <summary>
    /// Станция.
    /// </summary>
    public DbSet<Station> Stations => Set<Station>();
    
    /// <summary>
    /// Линия.
    /// </summary>
    public DbSet<Line> Lines => Set<Line>();
    
    /// <summary>
    /// Журнал загрузок исходных наборов данных.
    /// </summary>
    public DbSet<DataImportRun> DataImportRuns => Set<DataImportRun>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Регистрация профилей маппинга.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetroQualityMonitorDbContext).Assembly);
    }
}