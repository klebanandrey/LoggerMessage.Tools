global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using LoggerMessage.Shared;
using LoggerMessage.Shared.Services;
using LoggerMessage.Tools;
using LoggerMessageTools.Extensions;
using LoggerMessageTools.Options;
using NLog;
using NLog.Layouts;
using Constants = LoggerMessage.Shared.Constants;
using Level = LoggerMessageTools.Options.Level;

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
        private LogLevel GelNlogLevel(Level toolsLevel)
        {
            switch (toolsLevel)
            {
                case Level.Off:
                    return LogLevel.Off;
                case Level.Trace:
                    return LogLevel.Trace;
                case Level.Debug:
                    return LogLevel.Debug;
                case Level.Information:
                    return LogLevel.Info;
                case Level.Warning:
                    return LogLevel.Warn;
                case Level.Error:
                    return LogLevel.Error;
                case Level.Critical:
                    return LogLevel.Fatal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(toolsLevel), toolsLevel, null);
            }
        }

        private void ConfigureInternalLogger(ConfigurationHelper configHelper)
        {
            var extensionConfig = configHelper.GetConfiguration();

            var level = (Level)Enum.Parse(typeof(Level), extensionConfig[Constants.Level].ToString());
            var logFolder = extensionConfig[Constants.LogFolder].ToString();

            var config = new NLog.Config.LoggingConfiguration();

            var jsonLayout = new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("time", "${longdate}"),
                    new JsonAttribute("level", "${level:upperCase=true}"),
                    new JsonAttribute("message", "${message}"),
                    new JsonAttribute("exception", "${exception:format=tostring}", false)
                }
            };


            // Targets where to log to: File and Console
            var internalLog = new NLog.Targets.FileTarget("internalLog") { FileName = $"{logFolder}\\{DateTime.Today:yyyyMMdd}.txt", Layout = jsonLayout};
            //var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            //config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(GelNlogLevel(level), LogLevel.Fatal, internalLog);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }


        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            this.AddService(typeof(ConfigurationHelper), async (container, token, type) => new ConfigurationHelper((await VS.Solutions.GetCurrentSolutionAsync()).FullPath));
            this.AddService(typeof(IEventGroupService), async (container, token, type) => EventGroupServiceCreator.Create((await VS.Solutions.GetCurrentSolutionAsync()).FullPath));
            this.AddService(typeof(MessageService), async (container, token, type) => new MessageService(await this.GetPackageServiceAsync<IEventGroupService>()));


            var configHelper = await this.GetPackageServiceAsync<ConfigurationHelper>();
            ConfigureInternalLogger(configHelper);
        }
    }
}