using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace FeedbackBoard.Core.Entities;

public class AuditLog : ITableEntity
{
    // Обов'язкові поля ITableEntity
    [JsonProperty("PartitionKey")]
    public string PartitionKey { get; set; } = string.Empty;

    [JsonProperty("RowKey")]
    public string RowKey { get; set; } = string.Empty;

    [JsonProperty("Timestamp")]
    public DateTimeOffset? Timestamp { get; set; }

    [JsonProperty("ETag")]
    public ETag ETag { get; set; }

    // Наші кастомні поля
    [JsonProperty("FeedbackId")]
    public string FeedbackId { get; set; } = string.Empty;

    [JsonProperty("Action")]
    public string Action { get; set; } = string.Empty;

    [JsonProperty("Status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("Details")]
    public string Details { get; set; } = string.Empty;

    [JsonProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}