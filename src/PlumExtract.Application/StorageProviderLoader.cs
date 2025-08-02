using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PlumExtract.Domain.Interfaces;
using Serilog;

namespace PlumExtract.Application;

public static class StorageProviderLoader
{
    public static IServiceCollection RegisterStorageProviders(this IServiceCollection services)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "StorageProviders");
        
        if (!Directory.Exists(path))
        {
            return services;
        }

        foreach (var dll in Directory.EnumerateFiles(path, "PlumExtract.Storage.*.dll", SearchOption.AllDirectories))
        {
            try
            {
                var loadContext = new PluginLoadContext(dll);
                var assembly = loadContext.LoadFromAssemblyPath(dll);

                var providerTypes = assembly.GetTypes()
                    .Where(t => typeof(IBlobStore).IsAssignableFrom(t) &&
                                t is { IsInterface: false, IsAbstract: false })
                    .ToList();
                
                foreach (var type in providerTypes)
                {
                    services.AddTransient(type);
                }
                
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to load storage provider {DllName}", dll);
            }
        }
        
        return services;
    }
}