using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace LoggerMessage.Tools.Extensions
{
    public static class WorkspaceExtensions
    {
        public static Project GetProject(this Workspace workspace, string projectName)
        {
            return workspace.CurrentSolution.Projects.FirstOrDefault(p =>
                p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
