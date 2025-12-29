using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoMender.Core.Workers
{
    public class RabbitMQWorker : BackgroundService
    {
        private readonly ILogger<RabbitMQWorker> _logger;
        private readonly ICodeFixerAgent _agent;
        private readonly IncidentStore _store;
        private readonly string _connectionString;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQWorker(ILogger<RabbitMQWorker> logger, IConfiguration config, ICodeFixerAgent agent, IncidentStore store)
        {
            _logger = logger;
            _agent = agent;
            _store = store;
            _connectionString = config["RabbitMQConnection"] ?? "amqp://guest:guest@localhost:5672";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("🐰 Worker is attempting to connect to RabbitMQ...");
                    
                    var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
                    _connection = await factory.CreateConnectionAsync(stoppingToken); // Sync to Async conversion if needed, but CreateConnectionAsync is standard
                    _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                    await _channel.QueueDeclareAsync(queue: "incidents-queue", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

                    _logger.LogInformation("✅ RabbitMQ Worker Connected! Listening for 'incidents-queue'...");

                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation($"📥 Received Simulation: {message}");

                        await ProcessMessageAsync(message);
                    };

                    await _channel.BasicConsumeAsync(queue: "incidents-queue", autoAck: true, consumer: consumer, cancellationToken: stoppingToken);
                    
                    // Keep the service alive and connected
                    while (!stoppingToken.IsCancellationRequested && _connection.IsOpen)
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ RabbitMQ Worker connection broken or failed: {ex.Message}. Retrying in 5s...");
                    try { await Task.Delay(5000, stoppingToken); } catch { /* ignore cancel during delay */ }
                }
            }
        }

        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                var request = JsonSerializer.Deserialize<FixCodeRequest>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (request != null)
                {
                    var analysisJson = await _agent.FixCode(request.SourceCode, request.ErrorLog);
                    
                    var incident = new Incident
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = $"Incident from RabbitMQ: {request.ErrorLog.Split(':')[0]}",
                        Severity = request.ErrorLog.Contains("Crash") || request.ErrorLog.Contains("Exception") ? "Crash" : "Warning",
                        RootCauseAnalysis = analysisJson,
                        Timestamp = DateTime.UtcNow
                    };

                    _store.AddIncident(incident);
                    _logger.LogInformation("✅ Incident processed and stored.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing incident logic: {ex.Message}");
            }
        }

        public class FixCodeRequest
        {
            public string SourceCode { get; set; } = "";
            public string ErrorLog { get; set; } = "";
        }
    }
}
