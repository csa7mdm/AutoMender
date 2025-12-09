using AutoMender.Core;
using AutoMender.Core.Workers;
using Microsoft.SemanticKernel; // For AddOpenAIChatCompletion extension

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Core Services
builder.Services.AddSingleton<ConfigurationService>();
builder.Services.AddSingleton<IncidentStore>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder.SetIsOriginAllowed(origin => true) // Allow any origin for dev
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

// Register AI Agent
builder.Services.AddSingleton<ICodeFixerAgent, CodeFixerAgent>();
// Start Kernel builder as singleton too or just let agent build it internally (Agent builds it internally now)

// Register Background Worker for RabbitMQ
builder.Services.AddHostedService<RabbitMQWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Run on port 7071 to match previous Azure Functions port
app.Run("http://localhost:7071");
