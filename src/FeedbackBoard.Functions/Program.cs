using FeedbackBoard.Functions.Configuration;
using FeedbackBoard.Functions.Services;
using FeedbackBoard.Functions.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        // add for other envinonments
        if (context.HostingEnvironment.IsDevelopment())
        {
            config.AddMiniblueKeyVault("http://localhost:4566", "feedbackboard-kv");
        }
    })
    .ConfigureServices((context, services) =>
    {
        // Реєструємо сервіси
        services.AddSingleton<ITableStorageService, TableStorageService>();
    })
    .Build();


host.Run();
