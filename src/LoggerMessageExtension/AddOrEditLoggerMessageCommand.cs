using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using EnvDTE;
using LoggerMessageExtension.Views;
using EventGroups.Resx;
using EventGroups.Roslyn;
using Task = System.Threading.Tasks.Task;
using LoggerMessageExtension.Exceptions;
using LoggerMessages.Roslyn;

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

            var dte = (EnvDTE.DTE)this.ServiceProvider.GetServiceAsync(typeof(EnvDTE.DTE)).Result;
            var activeDocument = dte.ActiveDocument;

            var loggerPackage = this.package as LoggerMessageExtensionPackage;

            if (!loggerPackage.EventGroupService.Connected)
                throw new FailedConnectionException();

            var ws = loggerPackage.ImportedWorkspace;
            var currentProject = ws.GetProject(activeDocument.ProjectItem.ContainingProject.Name);
            var currentDocument = currentProject.GetDocument(activeDocument.Name);
            var textSelection = dte.ActiveWindow.Selection as EnvDTE.TextSelection;
            loggerPackage.EventGroupService.Solution = ws.CurrentSolution;

            var loggerMessage = LoggerMessage.Create("");

            var lmEditor = new LoggerMessageEditorWindow(loggerPackage, loggerMessage);
            var isCanceled = !lmEditor.ShowModal() ?? true;
            if (isCanceled)
                return;
            
            loggerMessage = lmEditor.Message;

            var prj = currentDocument.Project
                .GetOrCreateLoggerMessagesResx(out var resxDocument)
                .GetOrCreateLoggerMessagesExtensions(out var extensionsDocument);

            currentDocument = prj.GetDocument(currentDocument.Id)
                .GetOrCreateLoggerVariable(textSelection.CurrentLine, out var loggerVariable)
                .AddCall(loggerMessage, textSelection.CurrentLine, loggerVariable);


            prj = currentDocument.Project;

            ws.TryApplyChanges(prj.Solution);

            prj = prj.GenerateResxClass();
            
            ws.TryApplyChanges(prj.Solution);






            EnvDTE.TextSelection ts = dte.ActiveWindow.Selection as EnvDTE.TextSelection;
            if (ts == null)
                return;
            EnvDTE.CodeFunction func = ts.ActivePoint.CodeElement[vsCMElement.vsCMElementFunction] as EnvDTE.CodeFunction;
            if (func == null)
                return;

            string message = dte.ActiveWindow.Document.FullName + System.Environment.NewLine +
                             "Line " + ts.CurrentLine + System.Environment.NewLine +
                             func.FullName;

            string title = "GetLineNo";

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
