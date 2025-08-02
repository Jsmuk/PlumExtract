using System.Text;

namespace PlumExtract.Storage.FileSystem.Tests;

public class FileSystemBlobContainerTests : IDisposable
{
    private readonly string _testRoot;
    private readonly FileSystemBlobContainer _container;

    public FileSystemBlobContainerTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _container = new FileSystemBlobContainer(_testRoot);
    }

    [Fact]
    public async Task WriteAsync_WritesAndReadsFileCorrectly()
    {
        // Arrange
        var blobName = "test.txt";
        var testContent = "Hello world!";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        await _container.WriteAsync(blobName, stream);
        await using var resultStream = await _container.ReadAsync(blobName);
        using var reader = new StreamReader(resultStream);
        var result = await reader.ReadToEndAsync();

        // Assert
        Assert.Equal(testContent, result);
    }

    [Fact]
    public async Task ReadAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        var blobName = "does-not-exist.txt";
        await Assert.ThrowsAsync<FileNotFoundException>(() => _container.ReadAsync(blobName));
    }
    
    [Fact]
    public async Task WriteAsync_Overwrites_Existing_File()
    {
        var blobName = "overwrite.txt";
        await using var firstStream = new MemoryStream(Encoding.UTF8.GetBytes("first"));
        await _container.WriteAsync(blobName, firstStream);

        await using var secondStream = new MemoryStream(Encoding.UTF8.GetBytes("second"));
        await _container.WriteAsync(blobName, secondStream);

        await using var resultStream = await _container.ReadAsync(blobName);
        using var reader = new StreamReader(resultStream);
        var result = await reader.ReadToEndAsync();

        Assert.Equal("second", result);
    }
    
    [Fact]
    public async Task WriteAsync_With_Empty_Stream_Creates_ZeroByteFile()
    {
        var blobName = "empty.txt";
        await using var stream = new MemoryStream();
        await _container.WriteAsync(blobName, stream);

        var info = new FileInfo(Path.Combine(_testRoot, blobName));
        Assert.True(info.Exists);
        Assert.Equal(0, info.Length);
    }
    
    [Fact]
    public async Task WriteAsync_Creates_Nested_Directories()
    {
        var blobName = "nested/inside/test.txt";
        var content = "Nested content";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        await _container.WriteAsync(blobName, stream);

        var fullPath = Path.Combine(_testRoot, blobName.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(fullPath));
    }
    
    [Theory]
    [InlineData("space in name.txt")]
    [InlineData("üñîçødę.txt")]
    [InlineData("name-with-@#%&.txt")]
    public async Task WriteAsync_With_SpecialCharacters_DoesNotThrow(string blobName)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var ex = await Record.ExceptionAsync(() => _container.WriteAsync(blobName, stream));
        Assert.Null(ex);
    }
    
    [Fact]
    public async Task ListBlobsAsync_Returns_Empty_If_No_Files()
    {
        var container = new FileSystemBlobContainer(Path.Combine(_testRoot, "empty-folder"));
        var result = await container.ListBlobsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListBlobsAsync_Correctly_Lists_Files()
    {
        // Arrange
        var containerName = Guid.NewGuid().ToString();
        var path = Path.Combine(_testRoot, containerName);
        var blobNames = new List<string>();
        Directory.CreateDirectory(path);
        for (var i = 0; i < 10; i++)
        {
            var blobName = $"test_{i}.txt";
            var testContent = "Hello world!";
            await File.WriteAllTextAsync(Path.Combine(path, blobName), testContent);
            blobNames.Add(blobName);
        }
        var container = new FileSystemBlobContainer(path);
        
        // Act
        var blobs = await container.ListBlobsAsync();
        
        // Assert
        Assert.Equal(blobNames, blobs);
        Assert.Equal(10, blobs.Count());

    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, recursive: true);
        }
    }
}