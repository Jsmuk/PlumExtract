using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PlumExtract.Domain.Models;
using UglyToad.PdfPig;

namespace PlumExtract.Application.Services;

public partial class PdfStatementParser
{
    private readonly ILogger<PdfStatementParser> _logger;

    public PdfStatementParser(ILogger<PdfStatementParser> logger)
    {
        _logger = logger;
    }

    public Statement ExtractStatement(Stream stream, string fileName)
    {
        var patterns = GetLinePatterns();
        var statement = new Statement
        {
            OriginalFileName = fileName
        };
        
        var inferredPeriod = GetPeriodFromFilename(fileName);
        if (inferredPeriod.HasValue)
        {
            statement.StartDate = inferredPeriod.Value.Start;
            statement.EndDate = inferredPeriod.Value.End;
        }
        
        using var document = PdfDocument.Open(stream);
        foreach (var page in document.GetPages())
        {
            // I'm not going to pretend I can write this sort of logic, thanks ChatGPT for fixing my attempt 
            var words = page.GetWords();
            var lines = words
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1)) 
                .OrderByDescending(g => g.Key); 

            foreach (var line in lines)
            {
                var lineText = string.Join(" ", line.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text));
                foreach (var pattern in patterns)
                {
                    var match = pattern.Pattern.Match(lineText);
                    if (match.Success)
                    {
                        pattern.Handler(match, statement);
                    }
                }
            }
        }
        
        return statement;
    }

    private List<LinePattern> GetLinePatterns()
    {
        return
        [
            new()
            {
                Pattern = BalanceSummaryRegex(),
                Section = StatementSection.BalanceSummary,
                Handler = (match, statement) =>
                {
                    var pocket = match.Groups[1].Value;
                    var inSuccess = decimal.TryParse(match.Groups[2].Value, out var inAmt);
                    var outSuccess = decimal.TryParse(match.Groups[3].Value, out var outAmt);
                    var closingSuccess = decimal.TryParse(match.Groups[4].Value, out var closing);
                    if (!inSuccess || !outSuccess || !closingSuccess)
                    {
                        _logger.LogError("Failed to parse inAmt/outAmt/closing");
                    }
                    else
                    {
                        statement.BalanceSummary = new BalanceSummary
                        {
                            Pocket = pocket,
                            MoneyIn = inAmt,
                            MoneyOut = outAmt,
                            ClosingBalance = closing
                        };
                    }
                }
            },
            new()
            {
                Pattern = TransactionRegex(),
                Section = StatementSection.Transactions,
                Handler = (match, statement) =>
                {
                    var dateStr = match.Groups[1].Value;
                    var description = match.Groups[2].Value;
                    var amountStr1 = match.Groups[3].Value;
                    var amountStr2 = match.Groups[4].Value;

                    DateTime.TryParseExact(dateStr + $" {statement.StartDate.Year}", "d MMM yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
                    var amount1Success = decimal.TryParse(amountStr1, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount1);
                    var amount2Success = decimal.TryParse(amountStr2, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount2);

                    if (!amount1Success || !amount2Success)
                    {
                        _logger.LogError("Failed to parse amount1/amount2");
                    }
                    
                    decimal moneyIn = 0;
                    decimal moneyOut = 0;

                    bool amount1Present = !string.IsNullOrWhiteSpace(amountStr1);
                    bool amount2Present = !string.IsNullOrWhiteSpace(amountStr2);

                    if (amount1Present && amount2Present)
                    {
                        moneyIn = amount1;
                        moneyOut = amount2;
                    }
                    else if (amount1Present)
                    {
                        if (IsMoneyOut(description))
                        {
                            moneyOut = amount1;
                        }
                        else
                        {
                            moneyIn = amount1;
                        }
                    }
                    else if (amount2Present)
                    {
                        if (IsMoneyOut(description))
                        {
                            moneyOut = amount2;
                        }
                        else
                        {
                            moneyIn = amount2;
                        }
                    }

                    statement.Transactions.Add(new()
                    {
                        Date = date,
                        Description = description,
                        MoneyIn = moneyIn,
                        MoneyOut = moneyOut
                    });
                }
            }
        ];
    }

    private static bool IsMoneyOut(string description)
    {
        return description.StartsWith("To ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMoneyIn(string description)
    {
        return description.StartsWith("From ", StringComparison.OrdinalIgnoreCase)
               || description.Equals("Interest", StringComparison.OrdinalIgnoreCase);
    }
    
    private static (DateOnly Start, DateOnly End)? GetPeriodFromFilename(string filePath)
    {
        var filename = Path.GetFileNameWithoutExtension(filePath); 
        // Edge case: Plum uses Sept for September, breaking the pattern for the other 11.. e.g. Sept2021_Plum_Saving
        var match = FilenameRegex().Match(filename);

        if (!match.Success)
        {
            return null;
        }

        var monthStr = match.Groups["month"].Value;
        var yearStr = match.Groups["year"].Value;

        if (monthStr.Equals("Sept", StringComparison.InvariantCultureIgnoreCase))
        {
            monthStr = "Sep";
        }

        if (!DateTime.TryParseExact(monthStr, "MMM", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var monthDt))
        {
            return null;
        }

        int year = int.Parse(yearStr);
        int month = monthDt.Month;

        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1); 

        return (start, end);
    }

    [GeneratedRegex(@"^(?<month>[A-Za-z]{3,5})(?<year>\d{4})")]
    private static partial Regex FilenameRegex();
    [GeneratedRegex(@"^(.+?)\s+([\d,.]+)\s+([\d,.]+)\s+([\d,.]+)$")]
    private static partial Regex BalanceSummaryRegex();
    [GeneratedRegex(@"^(\d{1,2} \w{3})\s+(.+?)\s+([\d,.]+)?\s*([\d,.]*)$")]
    private static partial Regex TransactionRegex();
}

public class LinePattern
{
    public required Regex Pattern { get; init; }
    public required Action<Match, Statement> Handler { get; init; }
    public required StatementSection Section { get; init; }
}

public enum StatementSection
{
    Header,
    BalanceSummary,
    Transactions
}