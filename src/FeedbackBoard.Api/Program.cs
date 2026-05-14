using FeedbackBoard.Api.Configuration;
using FeedbackBoard.Api.Services;
using FeedbackBoard.Api.Services.Interfaces;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIG ==========
if (builder.Environment.IsDevelopment())
{
    // Download secrets from miniblue Key Vault
    var endpoint = builder.Configuration.GetValue<string>("Azure:Endpoint") ?? "http://localhost:4566";
    builder.Configuration.AddMiniblueKeyVault(endpoint, "feedbackboard-kv");
    Console.WriteLine("✅ Secrets loaded from miniblue Key Vault");
}

// ========== SERVICES ==========
builder.Services.AddHttpClient("miniblue", client =>
{
    var endpoint = builder.Configuration.GetValue<string>("Azure:Endpoint") ?? "http://localhost:4566";
    client.BaseAddress = new Uri(endpoint);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// services refistrations
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();