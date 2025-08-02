namespace PlumExtract.Domain.Models;

public record Statement
{
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
    public BalanceSummary? BalanceSummary { get; set; }
    public required string OriginalFileName { get; init; }
}