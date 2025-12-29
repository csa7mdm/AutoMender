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
        private readonly string _rabbitConnectionString;

        public IncidentsController(IncidentStore store, ILogger<IncidentsController> logger, IConfiguration config)
        {
            _store = store;
            _logger = logger;
            _rabbitConnectionString = config["RabbitMQConnection"] ?? "amqp://guest:guest@localhost:5672";
        }

        [HttpGet]
        public IEnumerable<Incident> Get()
        {
            return _store.GetIncidents();
        }

        [HttpPost("simulate")]
        public async Task<IActionResult> Simulate([FromBody] JsonElement payload)
        {
            try
            {
                var message = payload.ToString();
                _logger.LogInformation($"Publishing simulation: {message}");

                var factory = new ConnectionFactory { Uri = new Uri(_rabbitConnectionString) };
                
                // create connection and channel
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: "incidents-queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(message);
                
                var props = new BasicProperties();
                await channel.BasicPublishAsync(exchange: "", routingKey: "incidents-queue", mandatory: false, basicProperties: props, body: body);

                _logger.LogInformation("✅ Message published to queue");
                return Ok("Simulation Sent");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to publish: {ex.Message}");
                return StatusCode(500, $"Failed to publish: {ex.Message}");
            }
        }
    }
}
