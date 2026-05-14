using FeedbackBoard.Core.Events;
using FeedbackBoard.Core.Models;
using FeedbackBoard.Functions.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FeedbackBoard.Functions.Functions;

public class ProcessFeedbackFunction
{
    private readonly ITableStorageService _auditService;
    private readonly ILogger<ProcessFeedbackFunction> _logger;

    public ProcessFeedbackFunction(
        ITableStorageService auditService,
        ILogger<ProcessFeedbackFunction> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [Function("ProcessFeedback")]
    public async Task Run([ServiceBusTrigger("feedback-submitted", Connection = "ServiceBusConnection")] string message)
    {
        _logger.LogInformation("Processing feedback from Service Bus: {Message}", message);

        try
        {
            var eventData = JsonConvert.DeserializeObject<FeedbackSubmittedEvent>(message);
            if (eventData == null) return;

            var auditLog = new AuditLog
            {
                PartitionKey = DateTime.UtcNow.ToString("yyyy-MM"),
                RowKey = Guid.NewGuid().ToString(),
                FeedbackId = eventData.FeedbackId,
                Action = "Processed",
                Status = "Success",
                Details = $"Feedback '{eventData.Title}' processed via Service Bus Trigger",
                CreatedAt = DateTime.UtcNow
            };

            await _auditService.LogAuditAsync(auditLog);
            _logger.LogInformation("Feedback processed: {FeedbackId} - {Title}", eventData.FeedbackId, eventData.Title);

            var allLogs = await _auditService.GetAllAuditLogsAsync();
            _logger.LogInformation("Total audit logs in table: {Count}", allLogs.Count);
            foreach (var audit in allLogs)
            {
                _logger.LogInformation("  - {PartitionKey}/{RowKey}: {FeedbackId} - {Action}",
                    audit.PartitionKey, audit.RowKey, audit.FeedbackId, audit.Action);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Service Bus message");
        }
    }
}