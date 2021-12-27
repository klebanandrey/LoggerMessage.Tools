using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using LoggerMessage.Shared;

namespace LoggerMessageTools.Options
{
    internal partial class OptionsProvider
    {
        // Register the options with these attributes on your package class:
        // [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "LoggerMessageTools", "General", 0, 0, true)]
        // [ProvideProfile(typeof(OptionsProvider.GeneralOptions), "LoggerMessageTools", "General", 0, 0, true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("General")]
        [DisplayName("Solution is shared")]
        [Description("Current An informative description.")]
        [DefaultValue(false)]
        public bool IsShared { get; set; }


        [Category("Shared")]
        [DisplayName("Service url")]
        [Description("")]
        [DefaultValue("")]
        public string ServiceUrl { get; set; }


        [Category("Shared")]
        [DisplayName("ApiKey")]
        [Description("")]
        [DefaultValue("")]
        public string ApiKey { get; set; }

        [Category("Log")]
        [DisplayName("LogsFolder")]
        [Description("")]
        [DefaultValue("")]
        public string LogsFolder { get; set; } = $"{Path.GetTempPath()}LoggerMessage\\{VS.Solutions.GetCurrentSolution().Name}\\";

        [Category("Log")]
        [DisplayName("Minimal log level")]
        [Description("")]
        [DefaultValue(Level.Off)]
        [TypeConverter(typeof(EnumConverter))]
        public Level Level { get; set; }


        public override void Save()
        {
            var helper = new ConfigurationHelper(VS.Solutions.GetCurrentSolution().FullPath);

            var settings = new Dictionary<string, object>()
            {
                { Constants.IsShared, IsShared },
                { Constants.ServiceUrl, ServiceUrl },
                { Constants.ApiKey, ApiKey},
                { Constants.LogFolder, LogsFolder },
                { Constants.Level, Level}
            };

            helper.StoreConfiguration(settings);
        }

        public override void Load()
        {
            var helper = new ConfigurationHelper(VS.Solutions.GetCurrentSolution().FullPath);

            var slnSettings = helper.GetConfiguration();

            if (slnSettings.TryGetValue(Constants.IsShared, out var isShared))
                IsShared = (bool)isShared;
            if (slnSettings.TryGetValue(Constants.ServiceUrl, out var serviceUrl))
                ServiceUrl = (string)serviceUrl;
            if (slnSettings.TryGetValue(Constants.ApiKey, out var apiKey))
                ApiKey = (string)apiKey;
            if (slnSettings.TryGetValue(Constants.LogFolder, out var logFolder))
                LogsFolder = (string)logFolder;
            if (slnSettings.TryGetValue(Constants.Level, out var level))
                Level = (Level)Enum.Parse(typeof(Level), level.ToString());
        }
    }
}
