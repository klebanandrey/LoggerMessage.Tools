using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LoggerMessage.Shared
{
    public class ConfigurationHelper
    {
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented};
        private readonly string _solutionPath;

        public ConfigurationHelper(string solutionPath)
        {
            _solutionPath = solutionPath;
            if (!File.Exists(GetConfigPath()))
                CreateDefaultConfig();
        }

        public string GetConfigFolder()
        {
            return Path.Combine(Path.GetDirectoryName(_solutionPath), Constants.SettingsFolder);
        }


        private string GetConfigPath()
        {
            return Path.Combine(GetConfigFolder(), Constants.ConfigFileName);
        }

        private bool CreateDefaultConfig()
        {
            try
            {
                var defaultConfiguration = new Dictionary<string, object>()
                {
                    {Constants.IsShared, false},
                    {Constants.ServiceUrl, ""},
                    {Constants.ApiKey, ""}
                };

                var solutionFolder = Path.GetDirectoryName(_solutionPath);

                var str = JsonConvert.SerializeObject(defaultConfiguration, _jsonSettings);
                if (!Directory.Exists(solutionFolder))
                    Directory.CreateDirectory(solutionFolder);
                File.WriteAllText(GetConfigPath(), str);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public Dictionary<string, object> GetConfiguration()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(GetConfigPath()), _jsonSettings);
        }

        public void StoreConfiguration(Dictionary<string, object> configuration)
        {
            var str = JsonConvert.SerializeObject(configuration, _jsonSettings);
            File.WriteAllText(GetConfigPath(), str);
        }
    }
}
