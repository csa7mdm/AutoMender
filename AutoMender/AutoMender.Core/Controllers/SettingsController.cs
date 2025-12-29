using Microsoft.AspNetCore.Mvc;
using AutoMender.Core;

namespace AutoMender.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ConfigurationService _configService;

        public SettingsController(ConfigurationService configService)
        {
            _configService = configService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var settings = _configService.GetSettings();
            // Mask API Key for security
            var safeSettings = new 
            {
                settings.Provider,
                ApiKey = string.IsNullOrEmpty(settings.ApiKey) ? "" : "***",
                settings.ModelId,
                settings.BaseUrl,
                GitHubToken = string.IsNullOrEmpty(settings.GitHubToken) ? "" : "***"
            };
            return Ok(safeSettings);
        }

        [HttpPost]
        public IActionResult Update([FromBody] AISettings newSettings)
        {
            _configService.UpdateSettings(newSettings);
            return Ok("Settings updated");
        }
    }
}
