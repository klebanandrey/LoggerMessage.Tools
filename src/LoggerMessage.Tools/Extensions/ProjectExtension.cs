using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources.NetStandard;
using Microsoft.CodeAnalysis;

namespace LoggerMessage.Tools.Extensions
{
    public static class ProjectExtension
    {
        public static Document GetDocument(this Project project, string filePath)
        {
            return project.Documents.FirstOrDefault(d => d.FilePath == filePath);
        }

        public static string GetNamespace(this Project project)
        {
            return string.IsNullOrWhiteSpace(project.DefaultNamespace)
                ? Constants.DefaultNamespace
                : project.DefaultNamespace;
        }

        public static Project GenerateResxClass(this Project project)
        {
            var filePath = Path.Combine(Path.GetDirectoryName(project.FilePath), Constants.LoggerMessagesResxFolderName, Constants.LoggerMessagesResxFileName);

            try
            {
                var process = new Process();
                process.StartInfo.FileName = "resgen";
                process.StartInfo.Arguments = $"{filePath} /str:cs,{project.GetNamespace()}.Properties";
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var directory = Path.GetDirectoryName(filePath);

            var from = Path.Combine(directory, Constants.LoggerMessagesFileName);
            var to = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(filePath)}.Designer.cs");
            var toDelete = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(filePath)}.resources");

            if (!File.Exists(from))
                throw new FileNotFoundException($"File {from} can't be found", from);

            if (!File.Exists(toDelete))
                throw new FileNotFoundException($"File {toDelete} can't be found", toDelete);

            if (File.Exists(to))
                File.Delete(to);

            File.Move(from, to);
            File.Delete(toDelete);

            return project.AddDocument($"{Path.GetFileNameWithoutExtension(filePath)}.Designer.cs", File.ReadAllText(to), new[] { Constants.LoggerMessagesResxFolderName }, to).Project;
        }

        public static string GetResxPath(this Project project)
        {
            return Path.Combine(Path.GetDirectoryName(project.FilePath), Constants.LoggerMessagesResxFolderName, Constants.LoggerMessagesResxFileName);
        }

        public static void AddOrUpdateResource(this Project project, MessageMethod messageMethod)
        {
            var resx = new List<DictionaryEntry>();

            var resxFilePath = project.GetResxPath();

            if (!File.Exists(resxFilePath))
            {
                resx.Add(new DictionaryEntry(messageMethod.Id, messageMethod.MessageTemplate));
            }
            else
            {
                using (var reader = new ResXResourceReader(resxFilePath))
                {
                    resx = reader.Cast<DictionaryEntry>().ToList();
                    var existingResource = resx.FirstOrDefault(r => r.Key.ToString() == messageMethod.Id);
                    if (existingResource.Key == null && existingResource.Value == null) // NEW!
                    {
                        resx.Add(new DictionaryEntry()
                            { Key = messageMethod.Id, Value = messageMethod.MessageTemplate });
                    }
                    else // MODIFIED RESOURCE!
                    {
                        var modifiedResx = new DictionaryEntry()
                            { Key = existingResource.Key, Value = messageMethod.MessageTemplate };
                        resx.Remove(existingResource); // REMOVING RESOURCE!
                        resx.Add(modifiedResx); // AND THEN ADDING RESOURCE!
                    }
                }
            }

            using (var writer = new ResXResourceWriter(resxFilePath))
            {
                foreach (var r in resx)
                    writer.AddResource(r.Key.ToString(), r.Value.ToString());
                writer.Generate();
            }
        }
    }
}
