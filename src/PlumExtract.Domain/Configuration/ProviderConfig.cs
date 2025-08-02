using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlumExtract.Domain.Configuration;

public class AppSettings
{
    public required ProviderConfig Source { get; set; }
    public required ProviderConfig Target { get; set; }
}

public class ProviderConfig
{
    public required string Type { get; set; }
    public required string ContainerName { get; set; }
    public string? OutputFormat { get; set; }
    public JsonElement Settings { get; set; }
}