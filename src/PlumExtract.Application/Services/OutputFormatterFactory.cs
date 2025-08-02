using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PlumExtract.Domain.Attributes;
using PlumExtract.Domain.Interfaces;

namespace PlumExtract.Application.Services;

public class OutputFormatterFactory : IOutputFormatterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public OutputFormatterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IOutputFormatter Create(string type)
    {
        var allOutputFormatters = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IOutputFormatter).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .ToList();

        var matchingFormatter =
            allOutputFormatters.FirstOrDefault(t => t.GetCustomAttribute<OutputFormatterAttribute>()?.Type == type);

        if (matchingFormatter is null)
        {
            throw new Exception(); // TODO: Custom exception
        }

        var instance = (IOutputFormatter)_serviceProvider.GetRequiredService(matchingFormatter);
        return instance;
    }
}