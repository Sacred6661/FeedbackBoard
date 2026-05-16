using FeedbackBoard.Api.Configuration;
using FeedbackBoard.Api.Data;
using FeedbackBoard.Api.Hubs;
using FeedbackBoard.Api.Services;
using FeedbackBoard.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIG ==========
if (builder.Environment.IsDevelopment())
{
    // Download secrets from miniblue Key Vault
    var endpoint = builder.Configuration.GetValue<string>("Azure:Endpoint") ?? "http://localhost:4566";
    builder.Configuration.AddMiniblueKeyVault(endpoint, "feedbackboard-kv");
    Console.WriteLine("Secrets loaded from miniblue Key Vault");
}

// ========== SQL Server ==========
var sqlConnectionString = builder.Configuration.GetValue<string>("FeedbackBoard:SqlServer:ConnectionString")
    ?? builder.Configuration.GetConnectionString("SqlServer");

builder.Services.AddDbContext<FeedbackBoardDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

// ========== Memory Cache ==========
builder.Services.AddMemoryCache();

// ========== SERVICES ==========
builder.Services.AddHttpClient("miniblue", client =>
{
    var endpoint = builder.Configuration.GetValue<string>("Azure:Endpoint") ?? "http://localhost:4566";
    client.BaseAddress = new Uri(endpoint);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var frontendUrl = builder.Configuration["FrontendUrl"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// services refistrations
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IFeedbackMetadataService, FeedbackMetadataService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

var app = builder.Build();

// ========== DB INIT ==========
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FeedbackBoardDbContext>();
    await DbInitializer.InitializeAsync(db, recreate: true); // change to false if dont want recreate existed db
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.MapHub<FeedbackHub>("/hubs/feedback");

app.Run();