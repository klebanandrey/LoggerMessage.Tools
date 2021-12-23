using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources.NetStandard;
using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;

namespace LoggerMessage.Tools.Extensions
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
                document = newDoc.WithSyntaxRoot(newDoc.CreateExtensionsDocument());
            }

            return document.Project;
        }


        public static Document GetDocument(this Project project, string filePath)
        {
            return project.Documents.FirstOrDefault(d => d.FilePath == filePath);
        }

        public static Project GetOrCreateLoggerMessagesResx(this Project project, out TextDocument document)
        {
            var existFile = project.AdditionalDocuments.FirstOrDefault(d =>
                d.Name.Equals(Shared.Constants.LoggerMessagesResxFileName, StringComparison.OrdinalIgnoreCase));
            if (existFile != null)
                document = existFile;
            else
            {
                var tmpFilePath = Path.Combine(Path.GetTempPath(), Shared.Constants.LoggerMessagesResxFileName);
                using (ResXResourceWriter resx = new ResXResourceWriter(tmpFilePath))
                {
                    resx.Generate();
                    resx.Close();
                }

                document = project.AddAdditionalDocument(Shared.Constants.LoggerMessagesResxFileName,
                    File.ReadAllText(tmpFilePath), new[] { Shared.Constants.LoggerMessagesResxFolderName });
            }

            var csproj = ProjectRootElement.Open(project.FilePath);
            AddItems(csproj, "EmbeddedResource", $"{Shared.Constants.LoggerMessagesResxFolderName}\\{Shared.Constants.LoggerMessagesResxFileName}");
            csproj.Save();
            return document.Project;
        }

        public static TextDocument GetOrCreateLoggerMessagesResx(this Project project)
        {
            var existFile = project.AdditionalDocuments.FirstOrDefault(d =>
                d.Name.Equals(Shared.Constants.LoggerMessagesResxFileName, StringComparison.OrdinalIgnoreCase));
            if (existFile != null)
                return existFile;
            else
            {
                var tmpFilePath = Path.Combine(Path.GetTempPath(), Shared.Constants.LoggerMessagesResxFileName);
                using (ResXResourceWriter resx = new ResXResourceWriter(tmpFilePath))
                {
                    resx.Generate();
                    resx.Close();
                }

                return project.AddAdditionalDocument(Shared.Constants.LoggerMessagesResxFileName,
                    File.ReadAllText(tmpFilePath), new[] { Shared.Constants.LoggerMessagesResxFolderName });
            }
        }

        public static string GetNamespace(this Project project)
        {
            return string.IsNullOrWhiteSpace(project.DefaultNamespace)
                ? Constants.DefaultNamespace
                : project.DefaultNamespace;
        }

        private static void AddItems(ProjectRootElement elem, string groupName, params string[] items)
        {
            var group = elem.AddItemGroup();
            foreach (var item in items)
            {
                group.AddItem(groupName, item);
            }
        }

        public static Project GenerateResxClass(this Project project)
        {
            var filePath = Path.Combine(Path.GetDirectoryName(project.FilePath), Shared.Constants.LoggerMessagesResxFolderName, Shared.Constants.LoggerMessagesResxFileName);

            try
            {
                var process = new Process();
                process.StartInfo.FileName = "resgen";
                process.StartInfo.Arguments = $"{filePath} /str:cs,{project.GetNamespace()}.Properties";
                //process.StartInfo.RedirectStandardError = true;
                //process.StartInfo.RedirectStandardOutput = true;
                //process.StartInfo.UseShellExecute = false;

                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.OutputDataReceived += Process_OutputDataReceived1;

                process.Start();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var from = Path.Combine(Path.GetDirectoryName(filePath), Shared.Constants.LoggerMessagesFileName);
            var to = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}.Designer.cs");
            var toDelete = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}.resources");

            if (!File.Exists(from))
                throw new FileNotFoundException($"File {from} can't be found", from);

            if (!File.Exists(toDelete))
                throw new FileNotFoundException($"File {toDelete} can't be found", toDelete);

            if (File.Exists(to))
                File.Delete(to);

            File.Move(from, to);
            File.Delete(toDelete);

            return project.AddDocument($"{Path.GetFileNameWithoutExtension(filePath)}.Designer.cs", File.ReadAllText(to), new[] { Shared.Constants.LoggerMessagesResxFolderName }, to).Project;
        }

        private static void Process_OutputDataReceived1(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static Project AddOrUpdateResource(this Project project, MessageMethod messageMethod, ref TextDocument document)
        {
            var resx = new List<DictionaryEntry>();

            if (!File.Exists(document.FilePath))
            {
                resx.Add(new DictionaryEntry(messageMethod.Id, messageMethod.MessageTemplate));
            }
            else
            {
                using (var reader = new ResXResourceReader(document.FilePath))
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

            using (var writer = new ResXResourceWriter(document.FilePath))
            {
                foreach (var r in resx)
                    writer.AddResource(r.Key.ToString(), r.Value.ToString());
                writer.Generate();
            }

            project = project.RemoveAdditionalDocument(document.Id);
            document = project.AddAdditionalDocument(Shared.Constants.LoggerMessagesResxFileName,
                File.ReadAllText(document.FilePath), new[] { Shared.Constants.LoggerMessagesResxFolderName }, document.FilePath);

            //using (ResXResourceReader resx = new ResXResourceReader(document.FilePath))
            //{
            //    foreach (DictionaryEntry d in resx)
            //    {
            //        Console.WriteLine(d.Key.ToString() + ":\t" + d.Value.ToString());
            //    }
            //}

            return document.Project;
        }
    }
}
