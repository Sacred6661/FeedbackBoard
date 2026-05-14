using FeedbackBoard.Api.Services.Interfaces;
using FeedbackBoard.Core.Models;

namespace FeedbackBoard.Api.Services;

public class FeedbackService : IFeedbackService
{
    private readonly ICosmosDbService _cosmosDb;
    private readonly IServiceBusPublisher _serviceBus;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        ICosmosDbService cosmosDb,
        IServiceBusPublisher serviceBus,
        ILogger<FeedbackService> logger)
    {
        _cosmosDb = cosmosDb;
        _serviceBus = serviceBus;
        _logger = logger;
    }

    public async Task<FeedbackResponse> CreateFeedbackAsync(CreateFeedbackRequest request)
    {
        var feedback = new Feedback
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UserId = request.UserId,
            Status = FeedbackStatusEnum.New,
            CreatedAt = DateTime.UtcNow
        };

        // 1. Save to Cosmos DB
        var created = await _cosmosDb.CreateFeedbackAsync(feedback);
        _logger.LogInformation("Feedback saved to Cosmos DB: {Id}", created?.Id);

        // 2. Sending a message to Service Bus
        await _serviceBus.PublishFeedbackSubmittedAsync(
            created.Id,
            created.Title,
            created.CategoryId,
            created.UserId);

        _logger.LogInformation("Feedback event published to Service Bus: {Id}", created.Id);

        return new FeedbackResponse
        {
            Id = created.Id,
            Title = created.Title,
            Status = created.Status.ToString(),
            CreatedAt = created.CreatedAt,
            Message = "Feedback submitted successfully"
        };
    }

    public async Task<Feedback?> GetFeedbackAsync(string id)
    {
        return await _cosmosDb.GetFeedbackAsync(id);
    }
}