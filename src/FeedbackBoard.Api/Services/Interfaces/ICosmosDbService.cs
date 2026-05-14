using FeedbackBoard.Core.Models;

namespace FeedbackBoard.Api.Services.Interfaces
{
    public interface ICosmosDbService
    {
        Task<Feedback?> CreateFeedbackAsync(Feedback feedback);
        Task<Feedback?> GetFeedbackAsync(string id);
        Task<List<Feedback>> GetFeedbacksByCategoryAsync(string categoryId);
    }
}
