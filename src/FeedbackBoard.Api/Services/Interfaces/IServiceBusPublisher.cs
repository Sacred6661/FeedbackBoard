namespace FeedbackBoard.Api.Services.Interfaces;

public interface IServiceBusPublisher
{
    Task PublishFeedbackSubmittedAsync(string feedbackId, string title, string categoryId, string userId);
}
