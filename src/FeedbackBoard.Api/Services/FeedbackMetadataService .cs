using FeedbackBoard.Api.Data;
using FeedbackBoard.Api.Services.Interfaces;
using FeedbackBoard.Core.Entities;
using FeedbackBoard.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FeedbackBoard.Api.Services
{
    public class FeedbackMetadataService(FeedbackBoardDbContext db, IMemoryCache cache) : IFeedbackMetadataService
    {
        public async Task<List<FeedbackStatusEntity>> GetStatusesAsync()
        {
            return await cache.GetOrCreateAsync("statuses", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await db.FeedbackStatuses
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Order)
                    .ToListAsync();
            }) ?? new();
        }

        public async Task<List<CategoryEntity>> GetCategoriesAsync()
        {
            return await cache.GetOrCreateAsync("categories", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await db.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Order)
                    .ToListAsync();
            }) ?? new();
        }

        public async Task<CategoryEntity?> GetCategoryAsync(int id)
            => await db.Categories.FindAsync(id);

        public async Task<FeedbackStatusEntity?> GetStatusAsync(int id)
            => await db.FeedbackStatuses.FindAsync(id);

        public async Task<FeedbackStatusInfo> GetStatusInfo(Feedback feedback)
        {
            var statuses = await GetStatusesAsync();
            var status = statuses.FirstOrDefault(s => s.Name == feedback.Status.ToString());

            return new FeedbackStatusInfo
            {
                Id = status?.Id ?? 0,
                Name = feedback.Status.ToString(),
                DisplayName = status?.DisplayName ?? feedback.Status.ToString(),
                Color = status?.Color ?? "#808080"
            };
        }
    }
}
