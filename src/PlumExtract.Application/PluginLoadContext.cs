using System.Reflection;
using System.Runtime.Loader;

namespace PlumExtract.Application;

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly string _pluginDirectory;

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _pluginDirectory = Path.GetDirectoryName(pluginPath)!;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Don't try to load framework assemblies
        if (assemblyName.Name!.StartsWith("System.") ||
            assemblyName.Name.StartsWith("Microsoft.") ||
            assemblyName.Name == "netstandard")
        {
            return null;
        }
        
        var dependencyPath = Path.Combine(_pluginDirectory, $"{assemblyName.Name}.dll");

        if (File.Exists(dependencyPath))
        {
            return LoadFromAssemblyPath(dependencyPath);
        }

        return null; // fallback to default context (or fail)
    }
}