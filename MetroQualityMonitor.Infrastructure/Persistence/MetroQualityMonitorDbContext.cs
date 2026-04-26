using MetroQualityMonitor.Domain.Analytics.Entities;
using MetroQualityMonitor.Domain.DataImports.Entities;
using MetroQualityMonitor.Domain.PassengerFlow.Entities;
using MetroQualityMonitor.Domain.Stations.Entities;
using MetroQualityMonitor.Domain.Test.Entities;
using MetroQualityMonitor.Domain.Vestibules.Entities;
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
    /// Вестибюли станций метро.
    /// </summary>
    public DbSet<Vestibule> Vestibules => Set<Vestibule>();

    /// <summary>
    /// Сведения о ремонтах эскалаторов.
    /// </summary>
    public DbSet<EscalatorRepair> EscalatorRepairs => Set<EscalatorRepair>();

    /// <summary>
    /// Пассажиропоток по станциям метро.
    /// </summary>
    public DbSet<PassengerFlowRecord> PassengerFlowRecords => Set<PassengerFlowRecord>();

    /// <summary>
    /// Журнал загрузок исходных наборов данных.
    /// </summary>
    public DbSet<DataImportRun> DataImportRuns => Set<DataImportRun>();

    /// <summary>
    /// Прогнозы пассажиропотока.
    /// </summary>
    public DbSet<Forecast> Forecasts => Set<Forecast>();

    /// <summary>
    /// Обнаруженные аномалии пассажиропотока.
    /// </summary>
    public DbSet<Anomaly> Anomalies => Set<Anomaly>();

    /// <summary>
    /// Справочник эмпирических часовых профилей пассажиропотока.
    /// </summary>
    public DbSet<HourlyProfile> HourlyProfiles => Set<HourlyProfile>();

    /// <summary>
    /// Результаты кластеризации станций.
    /// </summary>
    public DbSet<StationCluster> StationClusters => Set<StationCluster>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Регистрация профилей маппинга.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetroQualityMonitorDbContext).Assembly);
    }
}