using MetroQualityMonitor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MetroQualityMonitor API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "MetroQualityMonitor API";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();