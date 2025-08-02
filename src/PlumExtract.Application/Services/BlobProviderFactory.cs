using System.Reflection;
using System.Text.Json;
using PlumExtract.Domain.Attributes;
using PlumExtract.Domain.Configuration;
using PlumExtract.Domain.Exceptions;
using PlumExtract.Domain.Interfaces;

namespace PlumExtract.Application.Services;

public class BlobProviderFactory : IBlobProviderFactory
{
    public IBlobStore Create(string type, JsonElement config)
    {
        var allBlobProviders = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IBlobStore).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .ToList();

        var matchingBlobProvider =
            allBlobProviders.FirstOrDefault(t => t.GetCustomAttribute<BlobProviderAttribute>()?.Type == type);

        if (matchingBlobProvider is null)
        {
            throw new BlobProviderNotFoundException(type);
        }

        var ctor = matchingBlobProvider
            .GetConstructors()
            .FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 &&
                       parameters[0].ParameterType == typeof(JsonElement) &&
                       parameters[0].Name == "config";
            });

        if (ctor is null)
        {
            throw new InvalidOperationException($"No suitable constructor found for {matchingBlobProvider.Name}");
        }
        
        var instance = (IBlobStore)ctor.Invoke(new object[] { config });
        
        return instance;
    }
}