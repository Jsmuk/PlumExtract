namespace PlumExtract.Domain.Interfaces;

public interface IBlobContainer
{
    Task<IEnumerable<string>> ListBlobsAsync(string pattern = "*", CancellationToken cancellationToken = default);
    Task<Stream> ReadAsync(string blobName, CancellationToken cancellationToken = default);
    Task WriteAsync(string blobName, Stream content, CancellationToken cancellationToken = default);
}