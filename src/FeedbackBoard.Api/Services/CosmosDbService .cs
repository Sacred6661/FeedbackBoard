using Azure;
using FeedbackBoard.Api.Services.Interfaces;
using FeedbackBoard.Core.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace FeedbackBoard.Api.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _client;
    private readonly string _databaseName;
    private readonly ILogger<CosmosDbService> _logger;
    private Container? _container;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public CosmosDbService(IConfiguration configuration, ILogger<CosmosDbService> logger)
    {
        _logger = logger;

        var connectionString = configuration["FeedbackBoard:CosmosDb:ConnectionString"]
            ?? configuration.GetConnectionString("CosmosDb");

        _databaseName = configuration["CosmosDb:DatabaseName"] ?? "FeedbackBoard";

        _client = new CosmosClient(connectionString, new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            HttpClientFactory = () =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                return new HttpClient(handler);
            }
        });

        var source = configuration["FeedbackBoard:CosmosDb:ConnectionString"] != null
            ? "Key Vault"
            : "appsettings.json (fallback)";
        _logger.LogInformation("Cosmos DB client created. Source: {Source}", source);
    }

    // Lazy loading for container
    private async Task<Container> GetContainerAsync()
    {
        if (_initialized && _container != null)
            return _container;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized && _container != null)
                return _container;

            _logger.LogInformation("Initializing Cosmos DB database and container...");

            // Create a database if it doesn't exist
            var databaseResponse = await _client.CreateDatabaseIfNotExistsAsync(_databaseName);
            _logger.LogInformation("Database '{Database}' ready", _databaseName);

            // Create a container if it doesn't exist
            var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties
                {
                    Id = "Feedbacks",
                    PartitionKeyPath = "/categoryId"
                });

            _container = containerResponse.Container;
            _initialized = true;

            _logger.LogInformation("Container 'Feedbacks' ready");
        }
        finally
        {
            _initLock.Release();
        }

        return _container;
    }

    public async Task<Feedback> CreateFeedbackAsync(Feedback feedback)
    {
        var container = await GetContainerAsync();

        try
        {
            var response = await container.CreateItemAsync(
                feedback,
                new PartitionKey(feedback.CategoryId));

            _logger.LogInformation("Feedback created in Cosmos DB: {Id}", feedback.Id);
            return response.Resource;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error creating feedback {Id}", feedback.Id);
            return null;
        }
    }

    public async Task<Feedback?> GetFeedbackAsync(string id)
    {
        var container = await GetContainerAsync();

        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                .WithParameter("@id", id);

            var iterator = container.GetItemQueryIterator<Feedback>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var feedback = response.FirstOrDefault();
                if (feedback != null) return feedback;
            }

            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error getting feedback {Id}", id);
            return null;
        }
    }

    public async Task<Feedback> UpdateFeedbackAsync(Feedback feedback)
    {
        var container = await GetContainerAsync();

        var response = await container.ReplaceItemAsync(
            feedback,
            feedback.Id,
            new PartitionKey(feedback.CategoryId));

        _logger.LogInformation("Feedback updated in Cosmos DB: {Id}", feedback.Id);
        return response.Resource;
    }

    public async Task<List<Feedback>> GetFeedbacksByCategoryAsync(int categoryId)
    {
        var container = await GetContainerAsync();

        var query = new QueryDefinition("SELECT * FROM c WHERE c.categoryId = @categoryId")
            .WithParameter("@categoryId", categoryId);

        var iterator = container.GetItemQueryIterator<Feedback>(query);
        var results = new List<Feedback>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<List<Feedback>> GetFeedbacksByStatusAsync(FeedbackStatusEnum status)
    {
        var container = await GetContainerAsync();

        var iterator = container.GetItemLinqQueryable<Feedback>()
            .Where(f => f.Status == status)
            .ToFeedIterator();

        var results = new List<Feedback>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }
}