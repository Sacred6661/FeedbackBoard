using FeedbackBoard.Core.Entities;
using FeedbackBoard.Core.Models;

namespace FeedbackBoard.Api.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<FeedbackResponse> CreateFeedbackAsync(CreateFeedbackRequest request);
        Task<FeedbackResponse> ChangeStatusAsync(string feedbackId, FeedbackStatusEnum newStatus, string changedBy, string? reason = null);
        Task<FeedbackResponse> VoteAsync(string feedbackId, string userId);
        Task<FeedbackResponse?> GetFeedbackAsync(string id);
        Task<List<FeedbackResponse>> GetFeedbacksByCategoryAsync(int categoryId);
        Task<List<FeedbackResponse>> GetFeedbacksByStatusAsync(FeedbackStatusEnum status);
    }
}
