using Microsoft.AspNetCore.Mvc;
using AutoMender.Core;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace AutoMender.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentsController : ControllerBase
    {
        private readonly IncidentStore _store;
        private readonly ILogger<IncidentsController> _logger;
        private readonly IConnection? _rabbitConnection;

        public IncidentsController(IncidentStore store, ILogger<IncidentsController> logger, IConfiguration config)
        {
            _store = store;
            _logger = logger;
            
            // Initialize RabbitMQ Connection for Publishing
            // In a real app, inject a singleton IConnection or Publisher service
            try 
            {
                var factory = new ConnectionFactory { Uri = new Uri(config["RabbitMQConnection"] ?? "amqp://guest:guest@localhost:5672") };
                _rabbitConnection = factory.CreateConnectionAsync().Result; // Sync over async for simplicity in constructor POC
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ: {ex.Message}");
            }
        }

        [HttpGet]
        public IEnumerable<Incident> Get()
        {
            return _store.GetIncidents();
        }

        [HttpPost("simulate")]
        public async Task<IActionResult> Simulate([FromBody] JsonElement payload)
        {
            if (_rabbitConnection == null) 
                return StatusCode(503, "RabbitMQ not available");

            var message = payload.ToString();
            _logger.LogInformation($"Publishing simulation: {message}");

            using var channel = await _rabbitConnection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: "incidents-queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange: "", routingKey: "incidents-queue", body: body);

            return Ok("Simulation Sent");
        }
    }
}
