namespace PlumExtract.Domain.Models;

public record BalanceSummary
{
    public required string Pocket { get; init; }
    public required decimal MoneyIn { get; init; }
    public required decimal MoneyOut { get; init; }
    public required decimal ClosingBalance { get; init; }
}