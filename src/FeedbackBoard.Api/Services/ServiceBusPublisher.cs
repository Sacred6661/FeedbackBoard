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
            ?? throw new InvalidOperationException("Service Bus connection string not found in Key Vault or appsettings");

        var queueName = configuration["ServiceBus:Queues:FeedbackSubmitted"] ?? "feedback-submitted";

        var clientOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpTcp
        };

        _client = new ServiceBusClient(connectionString, clientOptions);
        _sender = _client.CreateSender(queueName);

        var source = configuration["FeedbackBoard:ServiceBus:ConnectionString"] != null
            ? "Key Vault"
            : "appsettings.json (fallback)";

        _logger.LogInformation("Service Bus publisher initialized. Queue: {Queue}, Source: {Source}", queueName, source);
    }

    public async Task PublishAsync(FeedbackCreatedEvent eventData)
    {
        await SendMessageAsync(eventData, eventData.FeedbackId, "FeedbackCreated");
    }

    public async Task PublishAsync(StatusChangedEvent eventData)
    {
        await SendMessageAsync(eventData, eventData.FeedbackId, "StatusChanged");
    }

    public async Task PublishAsync(FeedbackVotedEvent eventData)
    {
        await SendMessageAsync(eventData, eventData.FeedbackId, "FeedbackVoted");
    }

    private async Task SendMessageAsync<T>(T eventData, string messageId, string subject)
    {
        try
        {
            var json = JsonConvert.SerializeObject(eventData);
            var message = new ServiceBusMessage(json)
            {
                MessageId = messageId,
                ContentType = "application/json",
                Subject = subject
            };

            await _sender.SendMessageAsync(message);

            _logger.LogInformation("Published {Subject} event for {MessageId}", subject, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {Subject} event for {MessageId}", subject, messageId);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}