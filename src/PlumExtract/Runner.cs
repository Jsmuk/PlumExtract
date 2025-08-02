using System.Globalization;
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
    private readonly IOutputFormatterFactory _outputFormatterFactory;
    private readonly AppSettings _appSettings;
    private readonly ILogger<Runner> _logger;
    private readonly PdfStatementParser _parser;

    
    public Runner(IBlobProviderFactory blobProviderFactory, IOptions<AppSettings> appSettings, ILogger<Runner> logger, PdfStatementParser parser, IOutputFormatterFactory outputFormatterFactory)
    {
        _appSettings = appSettings.Value;
        _blobProviderFactory = blobProviderFactory;
        _logger = logger;
        _parser = parser;
        _outputFormatterFactory = outputFormatterFactory;
    }

    public async Task RunAsync(string[] args)
    {
        if (_appSettings.Target.OutputFormat is null)
        {
            throw new Exception("Target.OutputFormat is null");
        }
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
        

        
        var targetBlobStore = _blobProviderFactory.Create(_appSettings.Target.Type, _appSettings.Target.Settings);
        var targetContainer = targetBlobStore.GetContainer(_appSettings.Target.ContainerName);

        var outputFormatter = _outputFormatterFactory.Create(_appSettings.Target.OutputFormat);
        
        await using var formatResult = await outputFormatter.FormatSummaryAsync(statements);
        await targetContainer.WriteAsync(formatResult.BlobName, formatResult.OutputStream);    
        
        await using var formatResultTransactions = await outputFormatter.FormatTransactionsAsync(statements);
        await targetContainer.WriteAsync(formatResultTransactions.BlobName, formatResultTransactions.OutputStream); 
        
        await Task.CompletedTask;
    }
}