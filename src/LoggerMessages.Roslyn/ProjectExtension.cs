using System;
using System.Linq;
using LoggerMessages.Roslyn;
using Microsoft.CodeAnalysis;

namespace EventGroups.Roslyn
{
    public static class ProjectExtension
    {
        public static Project GetOrCreateLoggerMessagesExtensions(this Project project, out Document document)
        {
            var existFile = project.Documents.FirstOrDefault(d =>
                d.Name.Equals(Constants.LoggerMessagesFileName, StringComparison.OrdinalIgnoreCase));
            if (existFile != null)
                document = existFile;
            else
            {
                var newDoc = project.AddDocument(Constants.LoggerMessagesFileName, Constants.DefaultContent,
                    new[] {Constants.LoggerMessagesFolderName});
                document = newDoc.WithSyntaxRoot(newDoc.CreateLoggerMessageClass());
            }

            return document.Project;
        }

        public static Document GetDocument(this Project project, string documentName)
        {
            return project.Documents.FirstOrDefault(d => d.Name == documentName);
        }
    }
}
