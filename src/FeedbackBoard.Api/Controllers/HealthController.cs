using FeedbackBoard.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IFeedbackMetadataService _metadata;

    public HealthController(IHttpClientFactory httpClientFactory, IConfiguration configuration, 
        IFeedbackMetadataService metadata)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _metadata = metadata;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var client = _httpClientFactory.CreateClient("miniblue");
        var baseUrl = _configuration["Azure:Endpoint"] ?? "http://localhost:4566";
        var results = new Dictionary<string, string>();

        // local healthcheck, should be changed to allow prod and other env
        var functionUrlHealt = "http://localhost:7221/api/Health";

        // 1. Key Vault
        results["key-vault"] = await CheckUrl(client, $"{baseUrl}/keyvault/feedbackboard-kv/secrets?api-version=7.4");

        // 2. Service Bus
        results["service-bus"] = await CheckUrl(client, $"{baseUrl}/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/feedbackboard-rg/providers/Microsoft.ServiceBus/namespaces/feedbackboard-sb/queues?api-version=2021-11-01");

        // 3. Cosmos DB
        results["cosmos-db"] = await CheckUrl(client, $"{baseUrl}/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/feedbackboard-rg/providers/Microsoft.DocumentDB/databaseAccounts/feedbackboard-cosmos?api-version=2023-04-15");

        // 4. сheck functions
        results["azure-functions"] = await CheckUrl(client, functionUrlHealt);

        // Check SQL
        try
        {
            var statuses = await _metadata.GetStatusesAsync();
            results["sql-server"] = statuses.Count > 0 ? "OK" : "EMPTY";
        }
        catch
        {
            results["sql-server"] = "UNREACHABLE";
        }

        // Configuration source
        var storageConn = _configuration["FeedbackBoard:Storage:ConnectionString"];
        var fallback = _configuration["ConnectionStrings:Storage"];
        var configSource = (!string.IsNullOrEmpty(storageConn) && storageConn != fallback)
            ? "Key Vault"
            : "appsettings";

        var allOk = results.Values.All(v => v == "OK");

        return Ok(new
        {
            status = allOk ? "Healthy" : "Degraded",
            configSource,
            services = results,
            timestamp = DateTime.UtcNow
        });
    }

    private async Task<string> CheckUrl(HttpClient client, string url)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await client.GetAsync(url, cts.Token);
            return response.IsSuccessStatusCode ? "OK" : $"HTTP {(int)response.StatusCode}";
        }
        catch
        {
            return "UNREACHABLE";
        }
    }
}