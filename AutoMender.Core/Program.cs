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

// Register AI Agent
builder.Services.AddScoped<ICodeFixerAgent, CodeFixerAgent>();
builder.Services.AddScoped(sp =>
{
    // Same dynamic config logic as before, just adapted for DI scope
    // The Kernel is lightweight to build
    // Note: CodeFixerAgent builds its own kernel now due to dynamic settings requirement, 
    // so we might not need to register Kernel here globally unless other services use it.
    // However, keeping the builder available is good practice.
    var kernelBuilder = Kernel.CreateBuilder();
    return kernelBuilder.Build(); 
});

// Register Background Worker for RabbitMQ
builder.Services.AddHostedService<RabbitMQWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Run on port 7071 to match previous Azure Functions port
app.Run("http://localhost:7071");
