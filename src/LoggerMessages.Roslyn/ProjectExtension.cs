using System;
using System.Linq;
using EventGroups.Resx;
using Microsoft.CodeAnalysis;

namespace EventGroups.Roslyn
{
    public static class ProjectExtension
    {
        public static Project GetOrCreateLoggerMessages(this Project project, out Document document)
        {
            var existFile = project.Documents.FirstOrDefault(d =>
                d.Name.Equals(Constants.LoggerMessagesFileName, StringComparison.OrdinalIgnoreCase));
            if (existFile != null)
                document = existFile;
            else
            {
                project = project.GetOrCreateLoggerMessagesResx(out _);
                var newDoc = project.AddDocument(Constants.LoggerMessagesFileName, Constants.DefaultContent,
                    new[] {Constants.LoggerMessagesFolderName});
                document = newDoc.WithSyntaxRoot(newDoc.CreateLoggerMessageClass());
            }

            return document.Project;
        }
    }
}
