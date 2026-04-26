using MetroQualityMonitor.Infrastructure.Analytics.Seeders;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<MetroQualityMonitorDbContext>(options =>
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
#if DEBUG
        .AddJsonFile("appsettings.Development.json", optional: true)
#endif
        .Build();
    var connectionString = configuration.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не указана в конфигурации.");

    options.UseNpgsql(connectionString);
});

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetRequiredService<MetroQualityMonitorDbContext>();

Console.WriteLine("Applying migrations...");
await dbContext.Database.MigrateAsync();
Console.WriteLine("Migrations applied successfully.");

Console.WriteLine("Seeding hourly profiles...");
var seeder = new HourlyProfileSeeder(dbContext);
await seeder.SeedAsync();
Console.WriteLine("Seeding completed.");