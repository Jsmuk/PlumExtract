using PlumExtract.Domain.Interfaces;

namespace PlumExtract.Storage.FileSystem;

public class FileSystemBlobContainer : IBlobContainer
{
    private readonly string _path;
    public FileSystemBlobContainer(string path)
    {
        _path = path;
    }

    public Task<IEnumerable<string>> ListBlobsAsync(string pattern = "*", CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_path))
        {
            return Task.FromResult<IEnumerable<string>>([]);
        }
        return Task.FromResult(Directory.EnumerateFiles(_path, pattern).Select(Path.GetFileName))!;
    }

    public Task<Stream> ReadAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_path, blobName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Blob {blobName} not found.");
        }
        return Task.FromResult<Stream>(File.OpenRead(Path.Combine(_path, blobName)));
    }

    public async Task WriteAsync(string blobName, Stream content, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_path, blobName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await using var fs = File.Create(filePath);
        await content.CopyToAsync(fs, cancellationToken);
    }
}