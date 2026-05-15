using Microsoft.EntityFrameworkCore;

namespace FeedbackBoard.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(FeedbackBoardDbContext db, bool recreate = false)
    {
        if (recreate)
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        if (!await db.FeedbackStatuses.AnyAsync())
        {
            db.FeedbackStatuses.AddRange(SeedData.GetStatuses());
            await db.SaveChangesAsync();
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(SeedData.GetCategories());
            await db.SaveChangesAsync();
        }
    }
}