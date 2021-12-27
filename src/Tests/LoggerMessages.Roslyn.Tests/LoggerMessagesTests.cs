using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Build.Locator;
using LoggerMessage.Shared;

namespace LoggerMessages.Roslyn.Tests
{
    [TestClass]
    public class LoggerMessagesTests
    {
        private IConfiguration _configuration;

        public TestContext TestContext { get; set; }

        private static VisualStudioInstance _instance;

        [TestInitialize]
        public void TestInit()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            _instance ??= MSBuildLocator.RegisterDefaults();
        }

        //[TestMethod]
        //[DataRow("Some text", "Some_text()")]
        //[DataRow("Some text {param1} {param2}", "Some_text_0_1_(string param1, string param2)")]
        //[DataRow("Some {param1} {param2} text", "Some_0_1_text(string param1, string param2)")]
        //[DataRow("Text {param1} and {param2} text", "Text_0_and_1_text(string param1, string param2)")]
        //public void CheckGettingSignature(string input, string result)
        //{
        //    var loggerMessage = MessageMethod.Create(input);

        //    Assert.AreEqual(loggerMessage.GetMethodSignature(), result);
        //}


        [TestMethod]
        [DataRow("logger", "Some text", "logger.Some_text()")]
        [DataRow("_logger", "Some text {param1} {param2}", "_logger.Some_text_0_1_(string param1, string param2)")]
        public void CheckGettingCall(string loggerName, string input, string result)
        {
            //var loggerMessage = LoggerMessage.Shared.MessageMethod.Create(input);
            //loggerMessage.LoggerVariable = loggerName;

            //Assert.AreEqual(loggerMessage.GetMethodCall(), result);
        }


        [TestMethod]
        [DataRow(5, 5, "CheckLoggerEmpty.cs", "_logger")]
        [DataRow(7, 5, "CheckLoggerField.cs", "Logger")]
        [DataRow(8, 5, "CheckLoggerProperty.cs", "Logger")]
        public void CheckGettingLoggerVariable(int rowNumber, int columnNumber, string fileName, string result)
        {
            var _slnPath = Path.Combine(TestContext.TestResultsDirectory, "sln");
            DirectoryCopy(_configuration["TestSolutionPath"], _slnPath, true);

            var workspace = MSBuildWorkspace.Create();
            workspace.LoadMetadataForReferencedProjects = true;
            var solution = workspace.OpenSolutionAsync(Path.Combine(_slnPath, "TestLoggerMessages.sln")).Result;
            var project = solution.Projects.FirstOrDefault(p => p.Name == "TestLoggerMessages");
            var document = project.Documents.FirstOrDefault(d => d.Name == fileName);

            //document.GetOrCreateLoggerField(rowNumber, columnNumber, out var loggerVariable);
            //Assert.AreEqual(result, loggerVariable);
        }


        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }
            
            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
