using System.Text.Json;
using System.Text.Json.Serialization;
using MetroQualityMonitor.Domain.PassengerFlow.Entities;
using MetroQualityMonitor.Domain.Stations.Entities;
using MetroQualityMonitor.Domain.Vestibules.Entities;
using MetroQualityMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.local.json", optional: true)
    .Build();

var connectionString = config.GetConnectionString("DefaultConnection")!;
var dbOptions = new DbContextOptionsBuilder<MetroQualityMonitorDbContext>()
    .UseNpgsql(connectionString)
    .Options;

var datasetsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "datasets"));
var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// Линии 
Console.WriteLine("Импорт линий метро.");
{
    await using var db = new MetroQualityMonitorDbContext(dbOptions);

    var existing = await db.Lines.ToDictionaryAsync(l => l.MosDataId ?? -1);

    var linesJson = await File.ReadAllTextAsync(Path.Combine(datasetsDir, "metro_lines.json"));
    var collection = JsonSerializer.Deserialize<GeoJsonCollection<LineAttributes>>(linesJson, jsonOptions)!;

    foreach (var feature in collection.Features)
    {
        var attr = feature.Properties.Attributes;
        if (!existing.TryGetValue(attr.ID, out var line))
        {
            line = new Line { Name = attr.Line };
            db.Lines.Add(line);
        }

        line.Name = attr.Line;
        line.MosDataId = attr.ID;
        line.Status = attr.Status;
        line.GlobalId = attr.GlobalId;
    }

    var saved = await db.SaveChangesAsync();
    Console.WriteLine($"  Линий сохранено: {saved}");
}

// Станции + связи Станция-Линия
Console.WriteLine("Импорт станций метро.");
{
    await using var db = new MetroQualityMonitorDbContext(dbOptions);

    var existingStations = await db.Stations
        .Include(s => s.Lines)
        .ToDictionaryAsync(s => s.Name);

    var existingLines = await db.Lines.ToDictionaryAsync(l => l.Name);

    var linesJson = await File.ReadAllTextAsync(Path.Combine(datasetsDir, "metro_lines.json"));
    var vestibulesJson = await File.ReadAllTextAsync(Path.Combine(datasetsDir, "vestibules.json"));

    var vestibulesCollection = JsonSerializer.Deserialize<GeoJsonCollection<VestibuleAttributes>>(vestibulesJson, jsonOptions)!;

    var stationLineMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
    foreach (var feature in vestibulesCollection.Features)
    {
        var attr = feature.Properties.Attributes;
        if (string.IsNullOrWhiteSpace(attr.NameOfStation)) continue;
        if (!stationLineMap.TryGetValue(attr.NameOfStation, out var lines))
            stationLineMap[attr.NameOfStation] = lines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(attr.Line))
            lines.Add(attr.Line);
    }

    foreach (var (stationName, lineNames) in stationLineMap)
    {
        if (!existingStations.TryGetValue(stationName, out var station))
        {
            station = new Station { Name = stationName };
            db.Stations.Add(station);
            existingStations[stationName] = station;
        }

        foreach (var lineName in lineNames)
        {
            if (!existingLines.TryGetValue(lineName, out var line)) continue;
            // Add M2M link only if not already present (avoid duplicate junction rows)
            if (station.Lines != null && !station.Lines.Any(l => l.Id == line.Id))
                station.Lines!.Add(line);
        }
    }

    var saved = await db.SaveChangesAsync();
    Console.WriteLine($"  Станций/связей сохранено: {saved}");
}

// Вестибюли + ремонты эскалаторов
Console.WriteLine("Импорт вестибюлей.");
{
    await using var db = new MetroQualityMonitorDbContext(dbOptions);

    var existingVestibules = await db.Vestibules.ToDictionaryAsync(v => v.MosDataId);
    var existingRepairs = await db.EscalatorRepairs.ToDictionaryAsync(r => r.GlobalId);

    var stationsByName = await db.Stations.ToDictionaryAsync(s => s.Name);
    var linesByName = await db.Lines.ToDictionaryAsync(l => l.Name);

    var vestibulesJson = await File.ReadAllTextAsync(Path.Combine(datasetsDir, "vestibules.json"));
    var collection = JsonSerializer.Deserialize<GeoJsonCollection<VestibuleAttributes>>(vestibulesJson, jsonOptions)!;

    foreach (var feature in collection.Features)
    {
        var attr = feature.Properties.Attributes;
        var props = feature.Properties;

        if (!existingVestibules.TryGetValue(attr.ID, out var vestibule))
        {
            vestibule = new Vestibule { Name = attr.Name, MosDataId = attr.ID, GlobalId = attr.GlobalId };
            db.Vestibules.Add(vestibule);
            existingVestibules[attr.ID] = vestibule;
        }

        vestibule.Name = attr.Name;
        vestibule.GlobalId = attr.GlobalId;
        vestibule.OnTerritoryOfMoscow = attr.OnTerritoryOfMoscow?.Equals("да", StringComparison.OrdinalIgnoreCase);
        vestibule.AdmArea = attr.AdmArea;
        vestibule.District = attr.District;
        vestibule.LongitudeWgs84 = attr.LongitudeWgs84 is not null ? double.Parse(attr.LongitudeWgs84, System.Globalization.CultureInfo.InvariantCulture) : null;
        vestibule.LatitudeWgs84 = attr.LatitudeWgs84 is not null ? double.Parse(attr.LatitudeWgs84, System.Globalization.CultureInfo.InvariantCulture) : null;
        vestibule.VestibuleType = attr.VestibuleType;
        vestibule.CulturalHeritageSiteStatus = attr.CulturalHeritageSiteStatus;
        vestibule.ModeOnEvenDays = attr.ModeOnEvenDays;
        vestibule.ModeOnOddDays = attr.ModeOnOddDays;
        vestibule.FullFeaturedBPAAmount = attr.FullFeaturedBPAAmount;
        vestibule.LittleFunctionalBPAAmount = attr.LittleFunctionalBPAAmount;
        vestibule.BPAAmount = attr.BPAAmount;
        vestibule.ObjectStatus = attr.ObjectStatus;
        vestibule.ReleaseNumber = props.ReleaseNumber;
        vestibule.VersionNumber = props.VersionNumber;

        if (!string.IsNullOrWhiteSpace(attr.NameOfStation) && stationsByName.TryGetValue(attr.NameOfStation, out var station))
            vestibule.StationId = station.Id;
        if (!string.IsNullOrWhiteSpace(attr.Line) && linesByName.TryGetValue(attr.Line, out var line))
            vestibule.LineId = line.Id;

        foreach (var repairDto in attr.RepairOfEscalators ?? [])
        {
            if (!existingRepairs.TryGetValue(repairDto.GlobalId, out var repair))
            {
                repair = new EscalatorRepair
                {
                    GlobalId = repairDto.GlobalId,
                    RepairPeriod = repairDto.RepairOfEscalatorsValue,
                    IsDeleted = repairDto.IsDeleted != 0,
                };
                // Attach via navigation so EF resolves FK after identity generation
                vestibule.EscalatorRepairs.Add(repair);
                existingRepairs[repairDto.GlobalId] = repair;
            }
            else
            {
                repair.RepairPeriod = repairDto.RepairOfEscalatorsValue;
                repair.IsDeleted = repairDto.IsDeleted != 0;
            }
        }
    }

    var saved = await db.SaveChangesAsync();
    Console.WriteLine($"  Вестибюлей/ремонтов сохранено: {saved}");
}

// Пассажиропоток
Console.WriteLine("Импорт пассажиропотока.");
{
    var flowJson = await File.ReadAllTextAsync(Path.Combine(datasetsDir, "passenger_flow.json"));

    GeoJsonCollection<PassengerFlowAttributes>? collection = null;
    try { collection = JsonSerializer.Deserialize<GeoJsonCollection<PassengerFlowAttributes>>(flowJson, jsonOptions); }
    catch { /* unexpected format */ }

    if (collection is null || collection.Features.Count == 0)
    {
        Console.WriteLine("  passenger_flow.json не содержит данных. Запустите DataFetcher заново.");
    }
    else
    {
        await using var db = new MetroQualityMonitorDbContext(dbOptions);

        var existingRecords = await db.PassengerFlowRecords.ToDictionaryAsync(r => r.GlobalId);
        var stationsByName = await db.Stations.ToDictionaryAsync(s => s.Name);
        var linesByName = await db.Lines.ToDictionaryAsync(l => l.Name);

        foreach (var feature in collection.Features)
        {
            var attr = feature.Properties.Attributes;

            if (!existingRecords.TryGetValue(attr.GlobalId, out var record))
            {
                record = new PassengerFlowRecord
                {
                    GlobalId = attr.GlobalId,
                    Year = attr.Year,
                    Quarter = attr.Quarter ?? string.Empty,
                };
                db.PassengerFlowRecords.Add(record);
                existingRecords[attr.GlobalId] = record;
            }

            record.Year = attr.Year;
            record.Quarter = attr.Quarter ?? string.Empty;
            record.IncomingPassengers = attr.IncomingPassengers;
            record.OutgoingPassengers = attr.OutgoingPassengers;

            if (!string.IsNullOrWhiteSpace(attr.NameOfStation) && stationsByName.TryGetValue(attr.NameOfStation, out var station))
                record.StationId = station.Id;
            if (!string.IsNullOrWhiteSpace(attr.Line) && linesByName.TryGetValue(attr.Line, out var line))
                record.LineId = line.Id;
        }

        var saved = await db.SaveChangesAsync();
        Console.WriteLine($"  Записей пассажиропотока сохранено: {saved}");
    }
}

Console.WriteLine("Готово.");
return 0;

#region DTOs

record GeoJsonCollection<TAttr>(
    [property: JsonPropertyName("features")] List<GeoJsonFeature<TAttr>> Features);

record GeoJsonFeature<TAttr>(
    [property: JsonPropertyName("properties")] GeoJsonProperties<TAttr> Properties);

record GeoJsonProperties<TAttr>(
    [property: JsonPropertyName("attributes")] TAttr Attributes,
    [property: JsonPropertyName("releaseNumber")] int? ReleaseNumber,
    [property: JsonPropertyName("versionNumber")] int? VersionNumber);

record LineAttributes(
    [property: JsonPropertyName("Line")]      string Line,
    [property: JsonPropertyName("ID")]        int ID,
    [property: JsonPropertyName("Status")]    string? Status,
    [property: JsonPropertyName("global_id")] long GlobalId);

record VestibuleAttributes(
    [property: JsonPropertyName("ID")]                        int ID,
    [property: JsonPropertyName("Name")]                      string Name,
    [property: JsonPropertyName("OnTerritoryOfMoscow")]       string? OnTerritoryOfMoscow,
    [property: JsonPropertyName("AdmArea")]                   string? AdmArea,
    [property: JsonPropertyName("District")]                  string? District,
    [property: JsonPropertyName("Longitude_WGS84")]           string? LongitudeWgs84,
    [property: JsonPropertyName("Latitude_WGS84")]            string? LatitudeWgs84,
    [property: JsonPropertyName("VestibuleType")]             string? VestibuleType,
    [property: JsonPropertyName("NameOfStation")]             string? NameOfStation,
    [property: JsonPropertyName("Line")]                      string? Line,
    [property: JsonPropertyName("CulturalHeritageSiteStatus")]string? CulturalHeritageSiteStatus,
    [property: JsonPropertyName("ModeOnEvenDays")]            string? ModeOnEvenDays,
    [property: JsonPropertyName("ModeOnOddDays")]             string? ModeOnOddDays,
    [property: JsonPropertyName("FullFeaturedBPAAmount")]     int? FullFeaturedBPAAmount,
    [property: JsonPropertyName("LittleFunctionalBPAAmount")] int? LittleFunctionalBPAAmount,
    [property: JsonPropertyName("BPAAmount")]                 int? BPAAmount,
    [property: JsonPropertyName("ObjectStatus")]              string? ObjectStatus,
    [property: JsonPropertyName("global_id")]                 long GlobalId,
    [property: JsonPropertyName("RepairOfEscalators")]        List<EscalatorRepairDto>? RepairOfEscalators);

record EscalatorRepairDto(
    [property: JsonPropertyName("is_deleted")]        int IsDeleted,
    [property: JsonPropertyName("RepairOfEscalators")]string RepairOfEscalatorsValue,
    [property: JsonPropertyName("global_id")]         long GlobalId);

record PassengerFlowAttributes(
    [property: JsonPropertyName("NameOfStation")]      string? NameOfStation,
    [property: JsonPropertyName("Line")]               string? Line,
    [property: JsonPropertyName("Year")]               int Year,
    [property: JsonPropertyName("Quarter")]            string? Quarter,
    [property: JsonPropertyName("IncomingPassengers")] int IncomingPassengers,
    [property: JsonPropertyName("OutgoingPassengers")] int OutgoingPassengers,
    [property: JsonPropertyName("global_id")]          long GlobalId);
    
#endregion