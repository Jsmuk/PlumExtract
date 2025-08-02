using PlumExtract.Domain.Models;
using System.Text;

namespace PlumExtract.Formatter.Csv.Tests;

public class CsvOutputFormatterTests
{
    [Fact]
    public async Task FormatAsync_GeneratesCorrectCsvOutput()
    {
        // Arrange
        var formatter = new CsvOutputFormatter();
        var statements = new List<Statement>
        {
            new Statement
            {
                StartDate = DateOnly.FromDateTime(new DateTime(2025,
                    6,
                    1)),
                EndDate = DateOnly.FromDateTime(new DateTime(2025,
                    6,
                    30)),
                BalanceSummary = new BalanceSummary
                {
                    ClosingBalance = 123.45m,
                    Pocket = "Saving",
                    MoneyIn = 0,
                    MoneyOut = 0
                },
                Transactions = new List<Transaction>
                {
                    new()
                    {
                        Date = new DateTime(2025,
                            6,
                            5),
                        Description = "Transaction",
                        MoneyIn = 223.45m,
                        MoneyOut = 0
                    },
                    new()
                    {
                        Date = new DateTime(2025,
                            6,
                            6),
                        Description = "Transaction",
                        MoneyIn = 0,
                        MoneyOut = 100
                    }
                },
                OriginalFileName = "Test"
            },
            new Statement
            {
                StartDate = DateOnly.FromDateTime(new DateTime(2025,
                    7,
                    1)),
                EndDate = DateOnly.FromDateTime(new DateTime(2025,
                    7,
                    31)),
                BalanceSummary = new BalanceSummary
                {
                    ClosingBalance = 678.90m,
                    Pocket = "Saving",
                    MoneyIn = 0,
                    MoneyOut = 0
                },
                Transactions = new List<Transaction>
                {
                    new()
                    {
                        Date = new DateTime(2025,
                            7,
                            7),
                        Description = "Transaction",
                        MoneyIn = 678.90m,
                        MoneyOut = 0
                    }
                },
                OriginalFileName = "Test"
            }
        };

        // Act
        var result = await formatter.FormatSummaryAsync(statements);

        // Assert
        Assert.Equal("output.csv", result.BlobName);
        Assert.NotNull(result.OutputStream);
        Assert.Equal(0, result.OutputStream.Position);

        using var reader = new StreamReader(result.OutputStream, Encoding.UTF8, leaveOpen: true);
        var csvText = await reader.ReadToEndAsync();

        var expectedCsv = """
                          Account,StartDate,EndDate,ClosingBalance,TotalTransactions
                          Saving,2025-06-01,2025-06-30,123.45,2
                          Saving,2025-07-01,2025-07-31,678.90,1
                          
                          """;

        Assert.Equal(expectedCsv, csvText);
    }
    
    [Fact]
    public async Task FormatAsync_ProducesExpectedCsvOutput()
    {
        // Arrange
        var formatter = new CsvOutputFormatter();
        var statements = new List<Statement>
        {
            new Statement
            {
                StartDate = DateOnly.FromDateTime(new DateTime(2025, 07, 1)),
                EndDate = DateOnly.FromDateTime(new DateTime(2025, 07, 30)),
                BalanceSummary = new BalanceSummary
                {
                    ClosingBalance = 524.41M,
                    Pocket = "Saving",
                    MoneyIn = 1337.42M,
                    MoneyOut = 420M
                },
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Date = new DateTime(2025,
                            7,
                            7),
                        Description = "From Monzo",
                        MoneyIn = 1337.42M,
                        MoneyOut = 0
                    },
                    new Transaction
                    {
                        Date = new DateTime(2025,
                            7,
                            8),
                        Description = "To Primary Pocket",
                        MoneyIn = 0,
                        MoneyOut = 420M
                    }
                },
                OriginalFileName = "test"
            }
        };

        var expectedCsv = """
                          Account,Date,Description,MoneyIn,MoneyOut
                          Saving,2025-07-07,From Monzo,1337.42,0
                          Saving,2025-07-08,To Primary Pocket,0,420
                          
                          """;

        // Act
        var result = await formatter.FormatTransactionsAsync(statements);
        using var reader = new StreamReader(result.OutputStream, Encoding.UTF8);
        var actualCsv = await reader.ReadToEndAsync();

        // Assert
        Assert.Equal(expectedCsv, actualCsv);
    }
}