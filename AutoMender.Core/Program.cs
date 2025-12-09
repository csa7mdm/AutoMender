using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AutoMender.Core;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register Configuration Service (Singleton)
builder.Services.AddSingleton<ConfigurationService>();

// Register Incident Store (Singleton) for Live Feed
builder.Services.AddSingleton<IncidentStore>();

// Register Agent (Scoped)
builder.Services.AddScoped<ICodeFixerAgent, CodeFixerAgent>();

builder.Build().Run();
