using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlumExtract;
using PlumExtract.Application;
using PlumExtract.Application.Services;
using PlumExtract.Domain.Configuration;
using PlumExtract.Domain.Interfaces;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.RegisterStorageProviders();
builder.Services.RegisterOutputFormatters();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger());

builder.Services.AddTransient<Runner>();
builder.Services.AddSingleton<PdfStatementParser>();
builder.Services.AddSingleton<IBlobProviderFactory, BlobProviderFactory>();
builder.Services.AddSingleton<IOutputFormatterFactory, OutputFormatterFactory>();

builder.Services.Configure<AppSettings>(options =>
{
    builder.Configuration.Bind(options);

    var sourceSettingsSection = builder.Configuration.GetSection("Source:Settings");
    if (sourceSettingsSection.Exists())
    {
        var sourceSettingsDict = sourceSettingsSection
            .GetChildren()
            .ToDictionary(x => x.Key, x => x.Value);

        var sourceRawJson = JsonSerializer.Serialize(sourceSettingsDict);
        options.Source.Settings = JsonDocument.Parse(sourceRawJson).RootElement;
    }

    var targetSettingsSection = builder.Configuration.GetSection("Target:Settings");
    if (!targetSettingsSection.Exists())
    {
        return;
    }

    var targetSettingsDict = targetSettingsSection
        .GetChildren()
        .ToDictionary(x => x.Key, x => x.Value);

    var targetRawJson = JsonSerializer.Serialize(targetSettingsDict);
    options.Target.Settings = JsonDocument.Parse(targetRawJson).RootElement;
});

var host = builder.Build();


var runner = host.Services.GetRequiredService<Runner>();
await runner.RunAsync(args);

