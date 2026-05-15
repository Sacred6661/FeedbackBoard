using FeedbackBoard.Core.Events;

namespace FeedbackBoard.Api.Services.Interfaces;

public interface IServiceBusPublisher
{
    Task PublishAsync(FeedbackCreatedEvent eventData);
    Task PublishAsync(StatusChangedEvent eventData);
    Task PublishAsync(FeedbackVotedEvent eventData);
}
