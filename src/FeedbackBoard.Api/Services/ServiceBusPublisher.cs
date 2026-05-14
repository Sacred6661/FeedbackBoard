using Azure.Messaging.ServiceBus;
using FeedbackBoard.Api.Services.Interfaces;
using FeedbackBoard.Core.Events;
using Newtonsoft.Json;

namespace FeedbackBoard.Api.Services;

public class ServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public ServiceBusPublisher(IConfiguration configuration, ILogger<ServiceBusPublisher> logger)
    {
        _logger = logger;

        var connectionString = configuration["FeedbackBoard:ServiceBus:ConnectionString"]
            ?? configuration.GetConnectionString("ServiceBus")
            ?? throw new InvalidOperationException("Service Bus connection string not found");

        var queueName = configuration["ServiceBus:Queues:FeedbackSubmitted"] ?? "feedback-submitted";

        var clientOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpTcp
        };

        _client = new ServiceBusClient(connectionString, clientOptions);
        _sender = _client.CreateSender(queueName);

        _logger.LogInformation("Service Bus SDK publisher initialized for queue: {Queue}", queueName);
    }

    public async Task PublishFeedbackSubmittedAsync(string feedbackId, string title, string categoryId, string userId)
    {
        try
        {
            var eventData = new FeedbackSubmittedEvent
            {
                FeedbackId = feedbackId,
                Title = title,
                CategoryId = categoryId,
                UserId = userId,
                SubmittedAt = DateTime.UtcNow
            };

            var json = JsonConvert.SerializeObject(eventData);
            var message = new ServiceBusMessage(json)
            {
                MessageId = feedbackId,
                ContentType = "application/json",
                Subject = "FeedbackSubmitted"
            };

            _logger.LogInformation("Sending message to Service Bus via SDK...");
            await _sender.SendMessageAsync(message);
            _logger.LogInformation("Message sent to Service Bus: {FeedbackId}", feedbackId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to Service Bus");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}