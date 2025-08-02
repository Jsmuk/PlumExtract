using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlumExtract.Application.Services;
using PlumExtract.Domain.Configuration;
using PlumExtract.Domain.Interfaces;
using PlumExtract.Domain.Models;

namespace PlumExtract;

public class Runner
{
    private readonly IBlobProviderFactory _blobProviderFactory;
    private readonly AppSettings _appSettings;
    private readonly ILogger<Runner> _logger;
    private readonly PdfStatementParser _parser;

    
    public Runner(IBlobProviderFactory blobProviderFactory, IOptions<AppSettings> appSettings, ILogger<Runner> logger, PdfStatementParser parser)
    {
        _appSettings = appSettings.Value;
        _blobProviderFactory = blobProviderFactory;
        _logger = logger;
        _parser = parser;
    }

    public async Task RunAsync(string[] args)
    {
        var sourceBlobStore = _blobProviderFactory.Create(_appSettings.Source.Type, _appSettings.Source.Settings);
        var sourceContainer = sourceBlobStore.GetContainer(_appSettings.Source.ContainerName);
        var files = await sourceContainer.ListBlobsAsync("*.pdf");

        var statements = new List<Statement>();

        foreach (var file in files)
        {
            try
            {
                _logger.LogInformation("Parsing {File}", file);

                var fileStream = await sourceContainer.ReadAsync(file);
                
                var statement = _parser.ExtractStatement(fileStream, file);

                _logger.LogInformation("Account: {Name}, {Start} - {End}, {Count} transactions", statement.BalanceSummary!.Pocket, statement.StartDate, statement.EndDate, statement.Transactions.Count);
                
                statements.Add(statement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse {File}", file);
            }
        }
        
        var summaries = new List<CsvExportSummary>();
        foreach (var statement in statements)
        {
            summaries.Add(new CsvExportSummary
            {
                StartDate = statement.StartDate,
                EndDate = statement.EndDate,
                ClosingBalance = statement.BalanceSummary!.ClosingBalance,
                TotalTransactions = statement.Transactions.Count,
            });
        }
        
        summaries = summaries.OrderBy(x => x.StartDate).ToList();
        
        var targetBlobStore = _blobProviderFactory.Create(_appSettings.Target.Type, _appSettings.Target.Settings);
        var targetContainer = targetBlobStore.GetContainer(_appSettings.Target.ContainerName);

        // TODO (Eventually) this should follow the same pattern as the storage providers and be an output provider 
        
        await using var ms = new MemoryStream();
        await using var writer = new StreamWriter(ms);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        await csv.WriteRecordsAsync(summaries);
        await writer.FlushAsync();
        ms.Position = 0;
        
        await targetContainer.WriteAsync("output.csv", ms);
        
        await Task.CompletedTask;
    }
}