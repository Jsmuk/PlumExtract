using PlumExtract.Domain.Models;

namespace PlumExtract.Domain.Interfaces;

public interface IOutputFormatter
{
    public Task<FormatResult> FormatSummaryAsync(List<Statement> statements);
    public Task<FormatResult> FormatTransactionsAsync(List<Statement> statements);
}

public record FormatResult : IAsyncDisposable
{
    public required string BlobName { get; set; }
    public required Stream OutputStream { get; set; }

    public async ValueTask DisposeAsync()
    {
        await OutputStream.DisposeAsync();
    }
}