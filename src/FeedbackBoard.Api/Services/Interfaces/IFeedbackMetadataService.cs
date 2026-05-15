using FeedbackBoard.Core.Entities;
using FeedbackBoard.Core.Models;

namespace FeedbackBoard.Api.Services.Interfaces
{
    public interface IFeedbackMetadataService
    {
        Task<List<FeedbackStatusEntity>> GetStatusesAsync();
        Task<List<CategoryEntity>> GetCategoriesAsync();
        Task<CategoryEntity?> GetCategoryAsync(int id);
        Task<FeedbackStatusEntity?> GetStatusAsync(int id);
        Task<FeedbackStatusInfo> GetStatusInfo(Feedback feedback);
    }
}
