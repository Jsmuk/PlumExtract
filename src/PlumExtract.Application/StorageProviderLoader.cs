using System.Reflection;
using Serilog;
using Serilog.Core;

namespace PlumExtract.Application;

public static class StorageProviderLoader
{
    public static void LoadStorageProviders(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        foreach (var dll in Directory.EnumerateFiles(folder, "*.dll"))
        {
            try
            {
                Assembly.LoadFrom(dll);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to load storage provider {DllName}", dll);
            }
        }
    }
}