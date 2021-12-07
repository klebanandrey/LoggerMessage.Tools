using EventGroups.Roslyn;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LoggerMessageExtension.Services;
using LoggerMessages.Roslyn;

namespace LoggerMessageExtension.Options
{
    [Guid("2CD1924C-6DC3-4946-9E77-8ADC667C48B4")]
    public class LoggerMessageOptions : DialogPage
    {
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();     

        public LoggerMessageExtensionPackage Package;


        protected override IWin32Window Window
        {
            get
            {
                LoggerMessageOptionsControl page = new LoggerMessageOptionsControl();
                page.options = this;
                page.Initialize();
                return page;
            }
        }

        public override void LoadSettingsFromStorage()
        {
            if (Package != null && Package.ImportedWorkspace != null)
                Configuration = Package.ImportedWorkspace.GetConfiguration();
        }

        public override void SaveSettingsToStorage()
        {
            if (Package != null && Package.ImportedWorkspace != null)
                Package.ImportedWorkspace.StoreConfiguration(Configuration);

            Package.EventGroupService = EventGroupServiceCreator.Create(Package.ImportedWorkspace);
            Package.EventGroupService.Solution = Package.ImportedWorkspace.CurrentSolution;
        }
    }
}
