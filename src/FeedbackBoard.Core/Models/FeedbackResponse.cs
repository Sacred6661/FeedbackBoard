namespace FeedbackBoard.Core.Models;

public class FeedbackResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Categoty
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryIcon { get; set; }

    // Status
    public FeedbackStatusInfo? StatusInfo { get; set; }
    public List<StatusChangeResponse> StatusHistory { get; set; } = new();

    // AI
    public string? Sentiment { get; set; }
    public string? SuggestedCategoryName { get; set; }
    public bool HasDuplicates { get; set; }

    // Author
    public string AuthorName { get; set; } = string.Empty;

    // Votes
    public int VoteCount { get; set; }
    public bool HasVoted { get; set; }

    // Dates
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Вкладення
    public List<string> AttachmentUrls { get; set; } = new();
}