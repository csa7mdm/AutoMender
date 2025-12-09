using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace AutoMender.Core
{
    public class SettingsFunction
    {
        private readonly ILogger _logger;
        private readonly ConfigurationService _configService;

        public SettingsFunction(ILoggerFactory loggerFactory, ConfigurationService configService)
        {
            _logger = loggerFactory.CreateLogger<SettingsFunction>();
            _configService = configService;
        }

        [Function("GetSettings")]
        public async Task<HttpResponseData> GetSettings([HttpTrigger(AuthorizationLevel.Function, "get", Route = "settings")] HttpRequestData req)
        {
            var settings = _configService.GetSettings();
            
            // Mask API Key for security when returning to UI
            var safeSettings = new AISettings
            {
                Provider = settings.Provider,
                ModelId = settings.ModelId,
                BaseUrl = settings.BaseUrl,
                ApiKey = string.IsNullOrEmpty(settings.ApiKey) ? "" : "*****" // Indicate if set but don't show it
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(safeSettings);
            return response;
        }

        [Function("UpdateSettings")]
        public async Task<HttpResponseData> UpdateSettings([HttpTrigger(AuthorizationLevel.Function, "post", Route = "settings")] HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newSettings = JsonSerializer.Deserialize<AISettings>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (newSettings == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid settings data.");
                return badRequest;
            }

            // If API key is masked (unchanged in UI), preserve existing one
            if (newSettings.ApiKey == "*****")
            {
                newSettings.ApiKey = _configService.GetSettings().ApiKey;
            }

            _configService.UpdateSettings(newSettings);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Settings updated.");
            return response;
        }
    }
}
