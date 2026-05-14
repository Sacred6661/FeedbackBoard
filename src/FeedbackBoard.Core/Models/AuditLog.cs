using Newtonsoft.Json;

namespace FeedbackBoard.Core.Models;

public class AuditLog
{
    [JsonProperty("PartitionKey")]
    public string PartitionKey { get; set; } = string.Empty;  // "YYYY-MM"

    [JsonProperty("RowKey")]
    public string RowKey { get; set; } = string.Empty;  // Guid

    public string FeedbackId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;  // "Created", "Processed"
    public string Status { get; set; } = string.Empty;   // "Success", "Error"
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}