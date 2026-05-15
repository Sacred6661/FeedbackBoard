using FeedbackBoard.Core.Entities;

namespace FeedbackBoard.Api.Services.Interfaces
{
    public interface ICosmosDbService
    {
        Task<Feedback> CreateFeedbackAsync(Feedback feedback);
        Task<Feedback?> GetFeedbackAsync(string id);
        Task<Feedback> UpdateFeedbackAsync(Feedback feedback);
        Task<List<Feedback>> GetFeedbacksByCategoryAsync(int categoryId);
        Task<List<Feedback>> GetFeedbacksByStatusAsync(FeedbackStatusEnum status);
    }
}
