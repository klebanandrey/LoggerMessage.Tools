global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using LoggerMessage.Shared;
using LoggerMessage.Shared.Services;
using LoggerMessageTools.Services;

namespace LoggerMessageTools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.LoggerMessageToolsString)]
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "LoggerMessageTools", "General", 0, 0, true)]
    [ProvideProfile(typeof(OptionsProvider.GeneralOptions), "LoggerMessageTools", "General", 0, 0, true)]

    public sealed class LoggerMessageToolsPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            this.AddService(typeof(ConfigurationHelper), async (container, token, type) => new ConfigurationHelper(VS.Solutions.GetCurrentSolution().FullPath));
            this.AddService(typeof(Service), async (container, token, type) => new Service());
            this.AddService(typeof(IEventGroupService), async (container, token, type) => EventGroupServiceCreator.Create(VS.Solutions.GetCurrentSolution().FullPath));
        }
    }
}