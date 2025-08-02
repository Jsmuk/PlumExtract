namespace PlumExtract.Domain.Models;

public record CsvExportSummary
{
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal ClosingBalance { get; init; }
    public int TotalTransactions { get; init; }
}