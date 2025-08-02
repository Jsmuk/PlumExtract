using System.Text.Json;
using PlumExtract.Domain.Exceptions;
using PlumExtract.Domain.Interfaces;

namespace PlumExtract.Storage.FileSystem.Tests;

public class FileSystemBlobStoreTests
{
    [Fact]
    public void Constructor_WithValidConfig_SetsBasePathCorrectly()
    {
        // Arrange
        var json = """
                       {
                           "BasePath": "C:\\test"
                       }
                   """;
        var config = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var store = new FileSystemBlobStore(config);

        // Assert
        var container = store.GetContainer("my-container");
        var expectedPath = Path.Combine("C:\\test", "my-container");

        Assert.IsType<FileSystemBlobContainer>(container);
        Assert.Equal(expectedPath, GetPrivatePath(container)); // see helper below
    }

    [Fact]
    public void Constructor_WithNullConfig_ThrowsMissingStorageProviderConfigException()
    {
        // Arrange
        var json = "null";
        var config = JsonSerializer.Deserialize<JsonElement>(json);

        // Act + Assert
        Assert.Throws<MissingStorageProviderConfigException>(() => new FileSystemBlobStore(config));
    }

    // Optional helper to inspect private fields (not strictly unit test best practice, but handy)
    private string GetPrivatePath(IBlobContainer container)
    {
        var field = typeof(FileSystemBlobContainer).GetField("_path", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(container)?.ToString() ?? throw new InvalidOperationException("Could not get _path");
    }
}