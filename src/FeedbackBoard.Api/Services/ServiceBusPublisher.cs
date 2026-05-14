using FeedbackBoard.Api.Services.Interfaces;
using FeedbackBoard.Core.Events;
using Newtonsoft.Json;
using System.Text;

namespace FeedbackBoard.Api.Services;

public class ServiceBusPublisher : IServiceBusPublisher
{
    private readonly HttpClient _httpClient;
    private readonly string _queueEndpoint;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public ServiceBusPublisher(IConfiguration configuration, ILogger<ServiceBusPublisher> logger)
    {
        _logger = logger;

        var queueName = configuration["ServiceBus:Queues:FeedbackSubmitted"] ?? "feedback-submitted";
        var endpoint = configuration["Azure:Endpoint"] ?? "http://localhost:4566";

        _queueEndpoint = $"{endpoint}/feedbackboard-sb/queues/{queueName}/messages";
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        _logger.LogInformation("Service Bus HTTP publisher initialized for: {Endpoint}", _queueEndpoint);
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
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending message to Service Bus via HTTP...");

            var response = await _httpClient.PostAsync(_queueEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Message sent to Service Bus: {FeedbackId}", feedbackId);
            }
            else
            {
                _logger.LogWarning("Service Bus responded with: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not send to Service Bus (this is OK for local dev)");
        }
    }
}