using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace FeedbackBoard.Core.Models;

public class Feedback
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("categoryId")]
    public string CategoryId { get; set; } = string.Empty;  // В JSON буде "categoryId"

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty("status")]
    public FeedbackStatusEnum Status { get; set; } = FeedbackStatusEnum.New;

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("voteCount")]
    public int VoteCount { get; set; }

    [JsonProperty("attachmentUrls")]
    public List<string> AttachmentUrls { get; set; } = new();
}