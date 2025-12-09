using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
// NOTE: RabbitMQ triggers typically use byte[] or string. We'll use string for simplicity.

namespace AutoMender.Core
{
    public class RabbitMQFunctions
    {
        private readonly ILogger _logger;
        private readonly ICodeFixerAgent _agent; // Injected Interface
        private readonly IncidentStore _store;

        public RabbitMQFunctions(ILoggerFactory loggerFactory, ICodeFixerAgent agent, IncidentStore store)
        {
            _logger = loggerFactory.CreateLogger<RabbitMQFunctions>();
            _agent = agent;
            _store = store;
        }

        // --- PUBLISHER ---
        // Simulates an incident report coming into the system.
        // It writes a message to the 'incidents-queue' in RabbitMQ.
        [Function("SimulateIncident")]
        [RabbitMQOutput(QueueName = "incidents-queue", ConnectionStringSetting = "RabbitMQConnection")]
        public async Task<string> SimulateIncident([HttpTrigger(AuthorizationLevel.Function, "post", Route = "simulate")] HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Received simulation request: {requestBody}");
            return requestBody; // This string is written to RabbitMQ
        }

        // --- CONSUMER ---
        // Listens to 'incidents-queue', analyzes the bug with AI, and stores the result.
        [Function("ProcessIncident")]
        public async Task ProcessIncident([RabbitMQTrigger("incidents-queue", ConnectionStringSetting = "RabbitMQConnection")] string message)
        {
             _logger.LogInformation($"Processing RabbitMQ message: {message}");

             try
             {
                 var request = JsonSerializer.Deserialize<FixCodeRequest>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                 if (request != null)
                 {
                     // call AI Agent
                     var analysisJson = await _agent.FixCode(request.SourceCode, request.ErrorLog);
                     
                     // Parse AI response (which is JSON) to get details
                     // For simplicity, we'll store the raw JSON in RootCauseAnalysis if parsing fails, or extract if possible.
                     // A robust impl would have a structured AI response object. 
                     
                     var incident = new Incident
                     {
                         Id = Guid.NewGuid().ToString(),
                         Title = $"Incident from RabbitMQ: {request.ErrorLog.Split(':')[0]}", // e.g. NullReferenceException
                         Severity = request.ErrorLog.Contains("Crash") || request.ErrorLog.Contains("Exception") ? "Crash" : "Warning",
                         RootCauseAnalysis = analysisJson,
                         Timestamp = DateTime.UtcNow
                     };

                     _store.AddIncident(incident);
                     _logger.LogInformation("Incident processed and stored.");
                 }
             }
             catch (Exception ex)
             {
                 _logger.LogError($"Error processing incident: {ex.Message}");
             }
        }
        
        public class FixCodeRequest
        {
            public string SourceCode { get; set; } = "";
            public string ErrorLog { get; set; } = "";
        }
    }
}
