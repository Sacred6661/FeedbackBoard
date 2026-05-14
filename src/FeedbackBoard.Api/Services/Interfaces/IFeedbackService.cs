using FeedbackBoard.Core.Models;

namespace FeedbackBoard.Api.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<FeedbackResponse> CreateFeedbackAsync(CreateFeedbackRequest request);
        Task<Feedback?> GetFeedbackAsync(string id);
    }
}
