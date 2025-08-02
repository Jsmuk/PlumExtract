namespace PlumExtract.Formatter.Csv.Models;

public record CsvTransactionsExport
{
    public required string Pocket { get; init; }
    public required DateOnly Date { get; init; }
    public required string Description { get; init; }
    public required decimal MoneyIn { get; init; }
    public required decimal MoneyOut { get; init; }
}