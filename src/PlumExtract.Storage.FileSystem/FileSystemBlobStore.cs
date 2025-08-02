using System.Text.Json;
using PlumExtract.Domain.Attributes;
using PlumExtract.Domain.Exceptions;
using PlumExtract.Domain.Interfaces;

namespace PlumExtract.Storage.FileSystem;

[BlobProvider("FileSystem")]
public class FileSystemBlobStore : IBlobStore
{
    private readonly string _basePath;

    public FileSystemBlobStore(JsonElement config)
    {
        var providerConfig = config.Deserialize<FileSystemBlobProviderConfig>();
        if (providerConfig is null)
        {
            throw new MissingStorageProviderConfigException();
        }
        _basePath = providerConfig.BasePath;
    }
    public IBlobContainer GetContainer(string containerName = "") => new FileSystemBlobContainer(Path.Combine(_basePath, containerName));
}