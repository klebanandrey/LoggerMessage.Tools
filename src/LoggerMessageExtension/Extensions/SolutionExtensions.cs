using System.IO;
using Microsoft.CodeAnalysis;

namespace LoggerMessageExtension.Extensions
{
    public static class SolutionExtensions
    {
        public static string GetSolutionName(this Solution solution)
        {
            return Path.GetFileName(solution.FilePath);
        }
    }
}
