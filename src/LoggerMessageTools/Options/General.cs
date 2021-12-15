using System.Collections.Generic;
using System.ComponentModel;
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


        public override void Save()
        {
            var helper = new ConfigurationHelper(VS.Solutions.GetCurrentSolution().FullPath);

            var settings = new Dictionary<string, object>()
            {
                { Constants.IsShared, IsShared },
                { Constants.ServiceUrl, ServiceUrl },
                { Constants.ApiKey, ApiKey },
            };

            helper.StoreConfiguration(settings);
        }

        public override void Load()
        {
            var helper = new ConfigurationHelper(VS.Solutions.GetCurrentSolution().FullPath);

            var slnSettings = helper.GetConfiguration();

            IsShared = (bool)slnSettings[Constants.IsShared];
            ServiceUrl = (string)slnSettings[Constants.ServiceUrl];
            ApiKey = (string)slnSettings[Constants.ApiKey];
        }
    }
}
