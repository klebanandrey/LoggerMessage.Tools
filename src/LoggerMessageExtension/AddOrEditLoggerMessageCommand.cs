using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using LoggerMessageExtension.Views;
using EventGroups.Resx;
using EventGroups.Roslyn;
using Task = System.Threading.Tasks.Task;
using LoggerMessageExtension.Exceptions;

namespace LoggerMessageExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AddOrEditLoggerMessageCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("71408823-1b8a-45b4-8085-253f522c13d7");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddOrEditLoggerMessageCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AddOrEditLoggerMessageCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AddOrEditLoggerMessageCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in AddOrEditLoggerMessageCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AddOrEditLoggerMessageCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "AddOrEditLoggerMessageCommand";

            EnvDTE.DTE dte;
            EnvDTE.Document activeDocument;

            dte = (EnvDTE.DTE)this.ServiceProvider.GetServiceAsync(typeof(EnvDTE.DTE)).Result;
            activeDocument = dte.ActiveDocument;
            message = $"Called on {activeDocument.FullName}";

            var loggerPackage = this.package as LoggerMessageExtensionPackage;

            if (!loggerPackage.EventGroupService.Connected)
                throw new FailedConnectionException();

            var ws = loggerPackage.ImportedWorkspace;
            var prj = ws
                .GetProject(activeDocument.ProjectItem.ContainingProject.Name)
                .GetOrCreateLoggerMessages(out _);

            ws.TryApplyChanges(prj.Solution);

            loggerPackage.EventGroupService.Solution = ws.CurrentSolution;           

            var lmEditor = new LoggerMessageEditorWindow(loggerPackage);
            lmEditor.ShowModal();

            prj = prj.GenerateResxClass();
            
            ws.TryApplyChanges(prj.Solution);

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
