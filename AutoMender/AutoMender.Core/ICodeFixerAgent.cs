using System.Threading.Tasks;

namespace AutoMender.Core
{
    public interface ICodeFixerAgent
    {
        Task<string> CompileCheck(string sourceCode);
        Task<string> FixCode(string sourceCode, string errorLog);
    }
}
