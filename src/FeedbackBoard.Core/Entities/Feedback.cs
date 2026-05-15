using Newtonsoft.Json;

namespace FeedbackBoard.Core.Entities;

public class Feedback
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Category (int → foreign key on the Categories table)
    [JsonProperty("categoryId")]
    public int CategoryId { get; set; }

    public string UserId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;

    // Status
    public FeedbackStatusEnum Status { get; set; } = FeedbackStatusEnum.New;
    public List<StatusChange> StatusHistory { get; set; } = new();

    // AI analisys
    public FeedbackAiAnalysis? AiAnalysis { get; set; }

    // Voting
    public int VoteCount { get; set; }
    public List<string> VoterIds { get; set; } = new();

    // Responsible
    public string? AssignedTo { get; set; }

    // Dates
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Attachments
    public List<string> AttachmentUrls { get; set; } = new();
}