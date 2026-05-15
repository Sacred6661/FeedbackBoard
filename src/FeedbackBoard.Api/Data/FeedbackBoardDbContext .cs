using FeedbackBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackBoard.Api.Data;

public class FeedbackBoardDbContext : DbContext
{
    public DbSet<FeedbackStatusEntity> FeedbackStatuses { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }

    public FeedbackBoardDbContext(DbContextOptions<FeedbackBoardDbContext> options)
        : base(options) { }
}