using FeedbackBoard.Core.Entities;

namespace FeedbackBoard.Api.Data;

public static class SeedData
{
    public static List<FeedbackStatusEntity> GetStatuses() => new()
    {
        new() { Id = 1, Name = "New", DisplayName = "New", Order = 1, Color = "#3B82F6", Description = "Just created" },
        new() { Id = 2, Name = "UnderReview", DisplayName = "Under review", Order = 2, Color = "#F59E0B", Description = "Under review by the team" },
        new() { Id = 3, Name = "Planned", DisplayName = "Planned", Order = 3, Color = "#8B5CF6", Description = "Added to the plan" },
        new() { Id = 4, Name = "InProgress", DisplayName = "In Progress", Order = 4, Color = "#EC4899", Description = "Currently under active development" },
        new() { Id = 5, Name = "Completed", DisplayName = "Completed", Order = 5, Color = "#10B981", Description = "Successfully completed" },
        new() { Id = 6, Name = "Declined", DisplayName = "Declined", Order = 6, Color = "#EF4444", Description = "Not accepted" },
        new() { Id = 7, Name = "Duplicate", DisplayName = "Duplicate", Order = 7, Color = "#6B7280", Description = "Merged with the existing one" }
    };

    public static List<CategoryEntity> GetCategories() => new()
    {
        new() { Name = "ui-ux", DisplayName = "UI/UX", Icon = "🎨", Order = 1, Description = "User Interface and User Experience" },
        new() { Name = "performance", DisplayName = "Performance", Icon = "⚡", Order = 2, Description = "Performance and optimization" },
        new() { Name = "bug", DisplayName = "Bugs", Icon = "🐛", Order = 3, Description = "Bug-reports" },
        new() { Name = "feature", DisplayName = "Features", Icon = "✨", Order = 4, Description = "New features" },
        new() { Name = "security", DisplayName = "Security", Icon = "🔒", Order = 5, Description = "Security questions" },
        new() { Name = "other", DisplayName = "Other", Icon = "📁", Order = 99, IsSystem = true, Description = "Does not fall under any other category" }
    };
}