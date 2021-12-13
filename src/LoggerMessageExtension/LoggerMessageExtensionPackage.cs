using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Task = System.Threading.Tasks.Task;
using LoggerMessageExtension.Options;
using LoggerMessageExtension.Services;

namespace LoggerMessageExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(LoggerMessageExtensionPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(LoggerMessageOptions), "Logger messages", "General", 0, 0, true)]
    public sealed class LoggerMessageExtensionPackage : AsyncPackage
    {
        /// <summary>
        /// LoggerMessageExtensionPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "9ec19837-d751-471b-9ecd-3dff91418455";

        #region Package Members

        public VisualStudioWorkspace ImportedWorkspace { get; set; }

        public LoggerMessageOptions LoggerMessageOptions { get; private set; }

        public IEventGroupService EventGroupService { get; set;}


        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await AddOrEditLoggerMessageCommand.InitializeAsync(this);

            var componentModel = (IComponentModel)await this.GetServiceAsync(typeof(SComponentModel));
            if (componentModel == null)
                throw new Exception();

            ImportedWorkspace = componentModel.GetService<VisualStudioWorkspace>();            
            LoggerMessageOptions = (LoggerMessageOptions)GetDialogPage(typeof(LoggerMessageOptions));
            LoggerMessageOptions.Package = this;
            EventGroupService = EventGroupServiceCreator.Create(ImportedWorkspace);
        }

        #endregion
    }
}
