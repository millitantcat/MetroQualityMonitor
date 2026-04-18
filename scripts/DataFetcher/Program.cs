using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;

const string FeaturesUrl = "https://apidata.mos.ru/v1/features";
const int PageSize = 1000;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.local.json", optional: true)
    .Build();

var apiKey = config["MosApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("API key is not set. Fill in MosApiKey in appsettings.local.json.");
    return 1;
}

var outputDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "datasets"));
Directory.CreateDirectory(outputDir);

using var http = new HttpClient();
http.Timeout = TimeSpan.FromMinutes(5);
var keyParam = $"api_key={apiKey}";

var datasets = new[]
{
    new DatasetConfig(FeaturesUrl, "624",   "vestibules.json",     IsPaginated: true),
    new DatasetConfig(FeaturesUrl, "2278",  "metro_lines.json",    IsPaginated: false),
    new DatasetConfig(FeaturesUrl, "62743", "passenger_flow.json", IsPaginated: true),
};

foreach (var ds in datasets)
{
    Console.WriteLine($"Fetching dataset {ds.Id} ({ds.FileName})...");
    try
    {
        var json = ds.IsPaginated
            ? await FetchPaginatedFeatures(ds)
            : await FetchAll(ds);

        var outputPath = Path.Combine(outputDir, ds.FileName);
        await File.WriteAllTextAsync(outputPath, json);
        Console.WriteLine($"  Saved → {outputPath}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"  ERROR [{ds.Id}]: {ex.Message}");
    }
}

Console.WriteLine("Done.");
return 0;

// Fetches a GeoJSON FeatureCollection across multiple pages.
// The API rejects $skip=0, so the first page is fetched without $skip.
async Task<string> FetchPaginatedFeatures(DatasetConfig ds)
{
    var allFeatures = new JsonArray();
    int skip = 0;

    while (true)
    {
        var url = skip == 0
            ? $"{ds.BaseUrl}/{ds.Id}?{keyParam}&$top={PageSize}"
            : $"{ds.BaseUrl}/{ds.Id}?{keyParam}&$top={PageSize}&$skip={skip}";
        Console.WriteLine($"  GET {url}");

        var response = await http.GetStringAsync(url);
        var collection = JsonNode.Parse(response)?.AsObject();
        var page = collection?["features"]?.AsArray();

        if (page is null || page.Count == 0)
            break;

        foreach (var item in page)
            allFeatures.Add(item?.DeepClone());

        Console.WriteLine($"  +{page.Count} features (total: {allFeatures.Count})");

        if (page.Count < PageSize)
            break;

        skip += PageSize;
    }

    var result = new JsonObject
    {
        ["type"] = "FeatureCollection",
        ["features"] = allFeatures,
    };
    return result.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
}

async Task<string> FetchAll(DatasetConfig ds)
{
    var url = $"{ds.BaseUrl}/{ds.Id}?{keyParam}";
    Console.WriteLine($"  GET {url}");
    var response = await http.GetStringAsync(url);
    var node = JsonNode.Parse(response);
    return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? "{}";
}

record DatasetConfig(string BaseUrl, string Id, string FileName, bool IsPaginated);
