using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace EventGroups.Roslyn
{
    public static class WorkspaceExtensions
    {
        private static readonly JsonSerializerOptions jsonSettings = new JsonSerializerOptions() { WriteIndented = true };

        public static Project GetProject(this Workspace workspace, string projectName)
        {
            return workspace.CurrentSolution.Projects.FirstOrDefault(p =>
                p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));
        }

        public static Dictionary<string, object> GetConfiguration(this Workspace workspace)
        {
            if (!File.Exists(workspace.GetConfigPath()))
                workspace.CreateDefaultConfig();

            return JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(workspace.GetConfigPath()));
        }

        private static string GetConfigPath(this Workspace workspace)
        {
            return Path.Combine(workspace.GetConfigFolder(), LoggerMessages.Common.Constants.ConfigFileName);
        }

        public static string GetConfigFolder(this Workspace workspace)
        {
            return Path.Combine(Path.GetDirectoryName(workspace.CurrentSolution.FilePath),
                LoggerMessages.Common.Constants.SettingsFolder);
        }

        private static bool CreateDefaultConfig(this Workspace workspace)
        {
            try
            {
                var defaultConfiguration = new Dictionary<string, object>()
                {
                    {LoggerMessages.Common.Constants.IsShared, false},
                    {LoggerMessages.Common.Constants.ServiceUrl, ""},
                    {LoggerMessages.Common.Constants.ApiKey, ""}
                };

                var str = JsonSerializer.Serialize(defaultConfiguration, defaultConfiguration.GetType(), jsonSettings);
                if (!Directory.Exists(Path.GetDirectoryName(workspace.GetConfigPath())))
                    Directory.CreateDirectory(Path.GetDirectoryName(workspace.GetConfigPath()));
                File.WriteAllText(workspace.GetConfigPath(), str);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void StoreConfiguration(this Workspace workspace, Dictionary<string, object> configuration)
        {
            var str = JsonSerializer.Serialize(configuration, configuration.GetType(), jsonSettings);
            File.WriteAllText(workspace.GetConfigPath(), str);
        }
    }
}
