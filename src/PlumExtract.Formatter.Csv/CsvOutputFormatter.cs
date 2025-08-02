using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using PlumExtract.Domain.Attributes;
using PlumExtract.Domain.Interfaces;
using PlumExtract.Domain.Models;
using PlumExtract.Formatter.Csv.Models;

namespace PlumExtract.Formatter.Csv;

[OutputFormatter("Csv")]
public class CsvOutputFormatter : IOutputFormatter
{
    public async Task<FormatResult> FormatSummaryAsync(List<Statement> statements)
    {
        var summaries = new List<CsvExportSummary>();
        foreach (var statement in statements)
        {
            summaries.Add(new CsvExportSummary
            {
                Pocket = statement.BalanceSummary!.Pocket,
                StartDate = statement.StartDate,
                EndDate = statement.EndDate,
                ClosingBalance = statement.BalanceSummary!.ClosingBalance,
                TotalTransactions = statement.Transactions.Count,
            });
        }
        
        summaries = summaries.OrderBy(x => x.StartDate).ToList();

        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<CsvExportSummaryMap>();
        
        await csv.WriteRecordsAsync(summaries);
        await writer.FlushAsync();
        ms.Position = 0;

        return new FormatResult
        {
            BlobName = "output.csv",
            OutputStream = ms
        };
    }

    public async Task<FormatResult> FormatTransactionsAsync(List<Statement> statements)
    {
        var exportTransactions = new List<CsvTransactionsExport>();
        foreach (var statement in statements)
        {
            exportTransactions.AddRange(statement.Transactions.Select(transaction => new CsvTransactionsExport
            {
                Pocket = statement.BalanceSummary!.Pocket,
                Description = transaction.Description,
                Date = DateOnly.FromDateTime(transaction.Date),
                MoneyIn = transaction.MoneyIn,
                MoneyOut = transaction.MoneyOut
            }));
        }
        
        exportTransactions = exportTransactions.OrderBy(x => x.Date).ToList();

        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<CsvTransactionsExportMap>();
        
        await csv.WriteRecordsAsync(exportTransactions);
        await writer.FlushAsync();
        ms.Position = 0;

        return new FormatResult
        {
            BlobName = "output_transactions.csv",
            OutputStream = ms
        };
    }
}

public sealed class CsvExportSummaryMap : ClassMap<CsvExportSummary>
{
    public CsvExportSummaryMap()
    {
        Map(m => m.Pocket).Name("Account");
        Map(m => m.StartDate).TypeConverterOption.Format("yyyy-MM-dd");
        Map(m => m.EndDate).TypeConverterOption.Format("yyyy-MM-dd");
        Map(m => m.ClosingBalance);
        Map(m => m.TotalTransactions);
    }
}

public sealed class CsvTransactionsExportMap : ClassMap<CsvTransactionsExport>
{
    public CsvTransactionsExportMap()
    {
        Map(m => m.Pocket).Name("Account");
        Map(m => m.Date).TypeConverterOption.Format("yyyy-MM-dd");
        Map(m => m.Description);
        Map(m => m.MoneyIn);
        Map(m => m.MoneyOut);
    }
}