namespace PlumExtract.Domain.Models;

public record Transaction
{
    public required DateTime Date { get; init; }
    public required string Description { get; init; }
    public required decimal MoneyIn { get; init; }
    public required decimal MoneyOut { get; init; }
}