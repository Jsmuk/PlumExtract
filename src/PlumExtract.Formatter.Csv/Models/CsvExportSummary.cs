namespace PlumExtract.Formatter.Csv.Models;

public record CsvExportSummary
{
    public string Pocket { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal ClosingBalance { get; init; }
    public int TotalTransactions { get; init; }
}