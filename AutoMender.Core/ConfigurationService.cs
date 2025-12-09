using System;

namespace AutoMender.Core
{
    public class AISettings
    {
        public string Provider { get; set; } = "OpenAI"; // OpenAI, Groq, DeepSeek
        public string ApiKey { get; set; } = string.Empty;
        public string ModelId { get; set; } = "gpt-4";
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class ConfigurationService
    {
        private AISettings _currentSettings;

        public ConfigurationService()
        {
            // Initialize with environment variables as default
            _currentSettings = new AISettings
            {
                Provider = "Groq", 
                ApiKey = Environment.GetEnvironmentVariable("OpenAI_ApiKey") ?? "",
                ModelId = Environment.GetEnvironmentVariable("OpenAI_ModelId") ?? "meta-llama/llama-4-maverick-17b-128e-instruct",
                BaseUrl = Environment.GetEnvironmentVariable("OpenAI_BaseUrl") ?? "https://api.groq.com/openai/v1"
            };
        }

        public AISettings GetSettings()
        {
            return _currentSettings;
        }

        public void UpdateSettings(AISettings settings)
        {
            _currentSettings = settings;
        }
    }
}
