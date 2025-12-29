using Xunit;
using AutoMender.Core;
using System.Threading.Tasks;

namespace AutoMender.Tests
{
    public class CodeFixerAgentTests
    {
        [Fact]
        public async Task CompileCheck_ShouldReturnError_ForInvalidCode()
        {
            // Arrange
            // We can pass null for configService as CompileCheck doesn't use it
            var agent = new CodeFixerAgent(null); 
            string badCode = "public class Test { void Method() { int x = ; } }";

            // Act
            var result = await agent.CompileCheck(badCode);

            // Assert
            Assert.Contains("Compilation Failed", result);
            Assert.Contains("CS1525", result); // Expected Roslyn error code for invalid expression
        }

        [Fact]
        public async Task CompileCheck_ShouldReturnSuccess_ForValidCode()
        {
            // Arrange
            var agent = new CodeFixerAgent(null);
            string goodCode = "public class Test { void Method() { int x = 5; } }";

            // Act
            var result = await agent.CompileCheck(goodCode);

            // Assert
            Assert.Contains("Compilation Success", result);
        }
    }
}
