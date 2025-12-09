using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace AutoMender.Core
{
    public class CodeFixerFunction
    {
        private readonly ILogger _logger;
        private readonly ICodeFixerAgent _codeFixerAgent;

        public CodeFixerFunction(ILoggerFactory loggerFactory, ICodeFixerAgent codeFixerAgent)
        {
            _logger = loggerFactory.CreateLogger<CodeFixerFunction>();
            _codeFixerAgent = codeFixerAgent;
        }

        [Function("CompileCheck")]
        public async Task<HttpResponseData> RunCompileCheck([HttpTrigger(AuthorizationLevel.Function, "post", Route = "compile-check")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            var result = await _codeFixerAgent.CompileCheck(requestBody);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync(result);

            return response;
        }

        [Function("FixCode")]
        public async Task<HttpResponseData> RunFixCode([HttpTrigger(AuthorizationLevel.Function, "post", Route = "fix-code")] HttpRequestData req)
        {
            _logger.LogInformation("Processing FixCode request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonSerializer.Deserialize<FixCodeRequest>(requestBody);

            if (input == null || string.IsNullOrWhiteSpace(input.SourceCode) || string.IsNullOrWhiteSpace(input.ErrorLog))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Please provide sourceCode and errorLog in JSON body.");
                return badRequest;
            }

            var result = await _codeFixerAgent.FixCode(input.SourceCode, input.ErrorLog);

            var response = req.CreateResponse(HttpStatusCode.OK);
            // Result is expected to be JSON from the prompt instructions
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync(result);

            return response;
        }
        
        public class FixCodeRequest
        {
            public string SourceCode { get; set; } = string.Empty;
            public string ErrorLog { get; set; } = string.Empty;
        }
    }
}
