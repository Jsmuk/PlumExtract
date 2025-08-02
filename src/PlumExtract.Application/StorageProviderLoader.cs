using System.Reflection;

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
            // TODO: Error handling
            Assembly.LoadFrom(dll);
        }
    }
}