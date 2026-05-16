using FeedbackBoard.Api.Hubs;
using FeedbackBoard.Api.Services.Interfaces;
using FeedbackBoard.Core.Entities;
using FeedbackBoard.Core.Events;
using FeedbackBoard.Core.Models;
using Mapster;
using Microsoft.AspNetCore.SignalR;

namespace FeedbackBoard.Api.Services;

public class FeedbackService(ICosmosDbService cosmosDb, IServiceBusPublisher serviceBus, IFeedbackMetadataService metadata,
    IHubContext<FeedbackHub> feedbackHub, ILogger<FeedbackService> logger) : IFeedbackService
{
    public async Task<FeedbackResponse> CreateFeedbackAsync(CreateFeedbackRequest request)
    {
        var feedback = request.Adapt<Feedback>();
        feedback.Id = Guid.NewGuid().ToString();
        feedback.Status = FeedbackStatusEnum.New;
        feedback.StatusHistory = new List<StatusChange>
        {
            new() { OldStatus = FeedbackStatusEnum.New, NewStatus = FeedbackStatusEnum.New, ChangedBy = request.UserId }
        };
        feedback.CreatedAt = DateTime.UtcNow;

        await cosmosDb.CreateFeedbackAsync(feedback);

        await serviceBus.PublishAsync(new FeedbackCreatedEvent
        {
            FeedbackId = feedback.Id,
            Title = feedback.Title,
            Description = feedback.Description,
            UserId = feedback.UserId,
            CreatedAt = feedback.CreatedAt
        });

        logger.LogInformation("Feedback created and event published: {Id}", feedback.Id);
        return await MapToResponseAsync(feedback);
    }

    public async Task<FeedbackResponse> ChangeStatusAsync(string feedbackId, FeedbackStatusEnum newStatus, string changedBy, string? reason = null)
    {
        var feedback = await cosmosDb.GetFeedbackAsync(feedbackId);
        if (feedback == null) throw new InvalidOperationException("Feedback not found");

        var oldStatus = feedback.Status;
        feedback.StatusHistory.Add(new StatusChange
        {
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = changedBy,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        });

        feedback.Status = newStatus;
        feedback.UpdatedAt = DateTime.UtcNow;
        if (newStatus == FeedbackStatusEnum.Completed) feedback.CompletedAt = DateTime.UtcNow;

        await cosmosDb.UpdateFeedbackAsync(feedback);

        await serviceBus.PublishAsync(new StatusChangedEvent
        {
            FeedbackId = feedback.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = changedBy,
            Reason = reason
        });

        var response = await MapToResponseAsync(feedback);

        // signalR sending 
        await feedbackHub.Clients.Group("FeedbackWatchers").SendAsync("StatusChanged", new
        {
            feedbackId,
            status = response.StatusInfo
        });

        logger.LogInformation("Feedback status changed: {Id} {OldStatus} → {NewStatus}", feedback.Id, oldStatus, newStatus);
        return response;
    }

    public async Task<FeedbackResponse> VoteAsync(string feedbackId, string userId)
    {
        var feedback = await cosmosDb.GetFeedbackAsync(feedbackId);
        if (feedback == null) throw new InvalidOperationException("Feedback not found");

        if (!feedback.VoterIds.Contains(userId))
        {
            feedback.VoterIds.Add(userId);
            feedback.VoteCount = feedback.VoterIds.Count;
            await cosmosDb.UpdateFeedbackAsync(feedback);

            await serviceBus.PublishAsync(new FeedbackVotedEvent
            {
                FeedbackId = feedbackId,
                UserId = userId,
                NewVoteCount = feedback.VoteCount
            });
        }

        var response = MapToResponseAsync(feedback);

        await feedbackHub.Clients.Group("FeedbackWatchers").SendAsync("VoteUpdated", new
        {
            feedbackId,
            voteCount = feedback.VoteCount,
            hasVoted = true
        });

        return await MapToResponseAsync(feedback);
    }

    public async Task<FeedbackResponse?> GetFeedbackAsync(string id)
    {
        var feedback = await cosmosDb.GetFeedbackAsync(id);
        return feedback == null ? null : await MapToResponseAsync(feedback);
    }

    public async Task<List<FeedbackResponse>> GetFeedbacksByCategoryAsync(int categoryId)
    {
        var feedbacks = await cosmosDb.GetFeedbacksByCategoryAsync(categoryId);
        var responses = new List<FeedbackResponse>();
        foreach (var feedback in feedbacks)
        {
            responses.Add(await MapToResponseAsync(feedback));
        }
        return responses;
    }

    public async Task<List<FeedbackResponse>> GetFeedbacksByStatusAsync(FeedbackStatusEnum status)
    {
        var feedbacks = await cosmosDb.GetFeedbacksByStatusAsync(status);
        var responses = new List<FeedbackResponse>();
        foreach (var feedback in feedbacks)
        {
            responses.Add(await MapToResponseAsync(feedback));
        }
        return responses;
    }

    private async Task<FeedbackResponse> MapToResponseAsync(Feedback feedback)
    {
        var response = feedback.Adapt<FeedbackResponse>();

        response.StatusInfo = await metadata.GetStatusInfo(feedback);

        var category = await metadata.GetCategoryAsync(feedback.CategoryId);
        if (category != null)
        {
            response.CategoryName = category.DisplayName;
            response.CategoryIcon = category.Icon;
        }

        if (feedback.AiAnalysis?.SuggestedCategoryId != null)
        {
            var suggestedCategory = await metadata.GetCategoryAsync(feedback.AiAnalysis.SuggestedCategoryId.Value);
            response.SuggestedCategoryName = suggestedCategory?.DisplayName;
        }

        return response;
    }
}