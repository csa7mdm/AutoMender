using Microsoft.AspNetCore.Mvc;
using AutoMender.Core;

namespace AutoMender.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FixCodeController : ControllerBase
    {
        private readonly ICodeFixerAgent _agent;

        public FixCodeController(ICodeFixerAgent agent)
        {
            _agent = agent;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RequestModel request)
        {
            var result = await _agent.FixCode(request.SourceCode, request.ErrorLog);
            return Ok(result); // Returns the JSON directly
        }

        [HttpPost("check")]
        public async Task<IActionResult> Check([FromBody] string sourceCode)
        {
            var result = await _agent.CompileCheck(sourceCode);
            return Ok(result);
        }

        public class RequestModel
        {
            public string SourceCode { get; set; } = "";
            public string ErrorLog { get; set; } = "";
        }
    }
}
