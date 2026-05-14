using Azure.Data.Tables;
using FeedbackBoard.Core.Models;
using FeedbackBoard.Functions.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FeedbackBoard.Functions.Services;

public class TableStorageService : ITableStorageService
{
    private readonly TableClient _tableClient;
    private readonly ILogger<TableStorageService> _logger;

    public TableStorageService(IConfiguration configuration, ILogger<TableStorageService> logger)
    {
        _logger = logger;

        var connectionString = configuration["FeedbackBoard:Storage:ConnectionString"]
            ?? configuration.GetConnectionString("Storage")
            ?? configuration["StorageConnection"]
            ?? "UseDevelopmentStorage=true";

        _tableClient = new TableClient(connectionString, "AuditLogs");

        _logger.LogInformation("Table Storage client initialized (Azurite)");
    }

    public async Task LogAuditAsync(AuditLog log)
    {
        try
        {
            // Table create if it not exists
            await _tableClient.CreateIfNotExistsAsync();

            log.PartitionKey = DateTime.UtcNow.ToString("yyyy-MM");
            log.RowKey = Guid.NewGuid().ToString();
            log.CreatedAt = DateTime.UtcNow;

            await _tableClient.AddEntityAsync(log);

            _logger.LogInformation("Audit log saved: {FeedbackId} - {Action}", log.FeedbackId, log.Action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving audit log for {FeedbackId}", log.FeedbackId);
        }
    }

    public async Task<List<AuditLog>> GetAllAuditLogsAsync()
    {
        try
        {
            await _tableClient.CreateIfNotExistsAsync();

            var logs = new List<AuditLog>();
            await foreach (var log in _tableClient.QueryAsync<AuditLog>())
            {
                logs.Add(log);
            }

            _logger.LogInformation("Found {Count} audit logs", logs.Count);
            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading audit logs");
            return new List<AuditLog>();
        }
    }
}