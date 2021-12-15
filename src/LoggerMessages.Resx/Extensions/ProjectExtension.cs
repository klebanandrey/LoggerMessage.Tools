using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using LoggerMessage.Shared;
using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;

namespace LoggerMessages.Resx.Extensions
{
    public static class ProjectExtension
    {
        public static Project GetOrCreateLoggerMessagesResx(this Project project, out TextDocument document)
        {
            var existFile = project.AdditionalDocuments.FirstOrDefault(d =>
                d.Name.Equals(Constants.LoggerMessagesResxFileName, StringComparison.OrdinalIgnoreCase));
            if (existFile != null)
                document = existFile;
            else
            {
                var tmpFilePath = Path.Combine(Path.GetTempPath(), Constants.LoggerMessagesResxFileName);
                using (ResXResourceWriter resx = new ResXResourceWriter(tmpFilePath))
                {
                    resx.Generate();
                    resx.Close();
                }

                document = project.AddAdditionalDocument(Constants.LoggerMessagesResxFileName,
                    File.ReadAllText(tmpFilePath), new[] {Constants.LoggerMessagesResxFolderName});
            }

            var csproj = ProjectRootElement.Open(project.FilePath);
            AddItems(csproj, "EmbeddedResource", $"{Constants.LoggerMessagesResxFolderName}\\{Constants.LoggerMessagesResxFileName}");
            csproj.Save();
            return document.Project;
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
            var filePath = Path.Combine(Path.GetDirectoryName(project.FilePath), Constants.LoggerMessagesResxFolderName, Constants.LoggerMessagesResxFileName);

            try
            {
                var process = new Process();
                process.StartInfo.FileName = "resgen";
                process.StartInfo.Arguments = $"{filePath} /str:cs,{Constants.DefaultNamespace}.Properties";
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

            var from = Path.Combine(Path.GetDirectoryName(filePath), Constants.LoggerMessagesFileName);
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

            return project.AddDocument($"{Path.GetFileNameWithoutExtension(filePath)}.Designer.cs", File.ReadAllText(to)).Project;
        }

        private static void Process_OutputDataReceived1(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static Project AddResource(this Project project, LoggerMessage.Shared.LoggerMessage message, ref TextDocument document)
        {

            using (ResXResourceWriter resx = new ResXResourceWriter(Path.Combine(Path.GetDirectoryName(project.FilePath), Constants.LoggerMessagesResxFolderName, Constants.LoggerMessagesResxFileName)))
            {
                resx.AddResource(message.Group.Abbreviation, message.MessageTemplate);
                resx.Close();
            }

            return document.Project;
        }
    }
}
