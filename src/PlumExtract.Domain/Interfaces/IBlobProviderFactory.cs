using System.Text.Json;
using PlumExtract.Domain.Configuration;

namespace PlumExtract.Domain.Interfaces;

public interface IBlobProviderFactory
{
    IBlobStore Create(string type, JsonElement config);
}