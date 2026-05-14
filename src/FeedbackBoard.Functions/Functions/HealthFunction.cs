using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FeedbackBoard.Functions.Functions;

public class HealthFunction
{
    private readonly ILogger<HealthFunction> _logger;

    public HealthFunction(ILogger<HealthFunction> logger)
    {
        _logger = logger;
    }

    [Function("Health")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Health check called");

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        var health = new
        {
            status = "Healthy",
            service = "FeedbackBoard.Functions",
            timestamp = DateTime.UtcNow
        };

        // WriteAsJsonAsync САМ встановлює Content-Type: application/json
        await response.WriteAsJsonAsync(health);

        return response;
    }
}