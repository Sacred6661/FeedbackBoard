using Newtonsoft.Json;

namespace FeedbackBoard.Core.Entities;

public class FeedbackAiAnalysis
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string FeedbackId { get; set; } = string.Empty;

    // Sentiment
    public string? Sentiment { get; set; }
    public double? SentimentConfidence { get; set; }

    // Category suggestion
    public int? SuggestedCategoryId { get; set; }
    public double? CategoryConfidence { get; set; }

    // Duplicates
    public bool HasDuplicates { get; set; }
    public List<string>? DuplicateFeedbackIds { get; set; }

    // Language
    public string? DetectedLanguage { get; set; }

    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}