using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PlumExtract.Domain.Interfaces;
using Serilog;

namespace PlumExtract.Application;

public static class OutputFormatterLoader
{
    private static readonly List<PluginLoadContext> _pluginContexts = new();
    public static IServiceCollection RegisterOutputFormatters(this IServiceCollection services)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "FormatProviders");
        if (!Directory.Exists(path))
        {
            return services;
        }

        foreach (var dll in Directory.EnumerateFiles(path, "PlumExtract.Formatter.*.dll", SearchOption.AllDirectories))
        {
            try
            {
                var loadContext = new PluginLoadContext(dll);
                var assembly = loadContext.LoadFromAssemblyPath(dll);
                _pluginContexts.Add(loadContext); 
                
                var formatterTypes = assembly.GetTypes()
                    .Where(t => typeof(IOutputFormatter).IsAssignableFrom(t)
                                && !t.IsInterface && !t.IsAbstract)
                    .ToList();

                foreach (var type in formatterTypes)
                {
                    services.AddTransient(type); 
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to load output formatter from {DllName}", dll);
            }
        }

        return services;
    }
}

public interface IPluginLoader
{
    IEnumerable<string> GetPluginPaths(string path, string searchPattern);
    Assembly LoadPluginAssembly(string path);
}