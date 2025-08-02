namespace PlumExtract.Domain.Exceptions;

public class BlobProviderNotFoundException(string blobProviderName)
    : Exception($"Blob provider '{blobProviderName}' not found")
{
    public string BlobProviderName { get; } = blobProviderName;
}