using PlumExtract.Domain.Configuration;

namespace PlumExtract.Storage.FileSystem;

public record FileSystemBlobProviderConfig(string BasePath) : BlobProviderConfig;