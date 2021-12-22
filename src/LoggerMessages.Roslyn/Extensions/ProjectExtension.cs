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
                d.Name.Equals(Constants.LoggerMessagesExtensionsFileName, StringComparison.OrdinalIgnoreCase));
            if (existFile != null)
                document = existFile;
            else
            {
                var newDoc = project.AddDocument(Constants.LoggerMessagesExtensionsFileName, Constants.DefaultContent,
                    new[] {Constants.LoggerMessagesFolderName});
                document = newDoc.WithSyntaxRoot(newDoc.CreateLoggerMessageClass());
            }

            return document.Project;
        }


        public static Document GetDocument(this Project project, string filePath)
        {
            return project.Documents.FirstOrDefault(d => d.FilePath == filePath);
        }
    }
}
