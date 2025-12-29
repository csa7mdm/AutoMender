using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection; // For ServiceCollection to build kernel
using Microsoft.SemanticKernel.Connectors.OpenAI; // Correct namespace for kernel builder extensions

namespace AutoMender.Core
{
    public class CodeFixerAgent : ICodeFixerAgent
    {
        private readonly ConfigurationService _configService;

        private const string FixerPrompt = @"You are a Senior .NET Backend Engineer and Architect.
Your goal is to analyze a stack trace and a source code file, identify the bug, and provide a robust fix.

RULES:
1. Do not just patch the line; understand the context.
2. If the fix requires a new library, DO NOT add it. Stick to standard .NET libraries.
3. Prefer ""Guard Clauses"" over nested ""if"" statements.
4. Your output must be a JSON object containing:
   - ""explanation"": A technical explanation of the root cause.
   - ""fixedCode"": The complete, corrected method (not just the snippet).
   - ""confidenceScore"": A number 0-100.
   - ""riskLevel"": ""Low"", ""Medium"", or ""High"".

INPUT DATA:
- Error: {{error_log}}
- Source Code: {{source_code}}";

        public CodeFixerAgent(ConfigurationService configService)
        {
            _configService = configService;
        }

        public Task<string> CompileCheck(string sourceCode)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                return Task.FromResult("Error: Source code is empty.");
            }

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var diagnostics = syntaxTree.GetDiagnostics();

            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                var errors = string.Join(Environment.NewLine, diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => $"Line {d.Location.GetLineSpan().StartLinePosition.Line + 1}: [{d.Id}] {d.GetMessage()}"));

                return Task.FromResult($"Compilation Failed:{Environment.NewLine}{errors}");
            }

            return Task.FromResult("Compilation Success");
        }

        public async Task<string> FixCode(string sourceCode, string errorLog)
        {
            // Build Kernel dynamically based on current settings
            var settings = _configService.GetSettings();
            
            if (string.IsNullOrEmpty(settings.ApiKey))
            {
                return JsonSerializerSafe("Error: API Key is not configured. Please go to Settings.");
            }

            var builder = Kernel.CreateBuilder();
            
            // Basic logic: if BaseUrl is set, use generic OpenAI compatible with custom endpoint
            // Otherwise use standard OpenAI
            if (!string.IsNullOrEmpty(settings.BaseUrl))
            {
                 builder.AddOpenAIChatCompletion(
                    modelId: settings.ModelId,
                    apiKey: settings.ApiKey,
                    httpClient: new System.Net.Http.HttpClient { BaseAddress = new Uri(settings.BaseUrl) }
                );
            }
            else
            {
                builder.AddOpenAIChatCompletion(settings.ModelId, settings.ApiKey);
            }

            var kernel = builder.Build();
            
            var function = kernel.CreateFunctionFromPrompt(FixerPrompt);
            
            var arguments = new KernelArguments
            {
                ["source_code"] = sourceCode,
                ["error_log"] = errorLog
            };

            try 
            {
                var result = await function.InvokeAsync(kernel, arguments);
                return result.GetValue<string>() ?? string.Empty;
            }
            catch (Exception ex)
            {
                return JsonSerializerSafe($"Error invoking AI: {ex.Message}");
            }
        }

        private string JsonSerializerSafe(string message)
        {
             // Simple manual JSON creation to avoid dependency on serialization for simple error string
             return $"{{\"explanation\": \"{message}\", \"fixedCode\": \"\", \"confidenceScore\": 0, \"riskLevel\": \"High\"}}";
        }
    }
}
