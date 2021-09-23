using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
