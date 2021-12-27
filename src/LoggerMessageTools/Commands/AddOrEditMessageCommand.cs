using System.Threading.Tasks;
using LoggerMessage.Shared;
using LoggerMessage.Tools;
using LoggerMessage.Tools.Extensions;
using LoggerMessageTools.Exceptions;
using LoggerMessageTools.Extensions;
using LoggerMessageTools.Views;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Project = Microsoft.CodeAnalysis.Project;

namespace LoggerMessageTools.Commands
{
    [Command(PackageIds.AddOrEditMessageCommand)]
    internal sealed class AddOrEditMessageCommand : BaseCommand<AddOrEditMessageCommand>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private async Task<Project> GetProjectAsync(Workspace ws, string fileName)
        {
            PhysicalFile file = await PhysicalFile.FromFileAsync(fileName);
            if (file?.ContainingProject == null)
                throw new Exception($"File {file} can't be loaded");

            return ws.GetProject(file.ContainingProject.Name);
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Logger.Trace("ExecuteAsync");

            try
            {
                var messageService = await Package.GetPackageServiceAsync<MessageService>();
                if (messageService == null)
                    throw new ServiceLoadingException(typeof(MessageService));

                var componentModel = await Package.GetPackageServiceAsync<SComponentModel>() as IComponentModel;
                if (componentModel == null)
                    throw new ServiceLoadingException(typeof(SComponentModel));

                var workspace = componentModel.GetService<VisualStudioWorkspace>();
                if (workspace == null)
                    throw new ServiceLoadingException(typeof(VisualStudioWorkspace));

                var dte = await Package.GetPackageServiceAsync<EnvDTE.DTE>();
                if (dte == null)
                    throw new ServiceLoadingException(typeof(EnvDTE.DTE));
                
                var docView = await VS.Documents.GetActiveDocumentViewAsync();
                string fileName = docView.TextBuffer.GetFileName();
                var textSelection = dte.ActiveWindow.Selection as EnvDTE.TextSelection;

                var currentProject = await GetProjectAsync(workspace, fileName);
                var currentDocument = currentProject.GetDocument(fileName);
                
                messageService.Initialize(currentProject, currentDocument.Id, textSelection.CurrentLine, textSelection.CurrentColumn);

                var loggerMessage = messageService.GetLoggerMessage(textSelection.CurrentLine, textSelection.CurrentColumn, out var currentStatement);

                var editorWindow = new LoggerMessageEditorWindow(this.Package, new ViewParams(loggerMessage.Abbr, loggerMessage.Level, loggerMessage.MessageTemplate));
                var windowResult = await VS.Windows.ShowDialogAsync(editorWindow);
                if (!windowResult.HasValue)
                    throw new EditorException("Editor didn't return any result");
                if (!windowResult.Value)
                    return;

                var invocation = messageService.GetLoggerMessageMethodInvocation(loggerMessage, editorWindow.ViewParams, out var loggerFieldDeclaration);

                await messageService.WriteExtensionsToFiles();

                messageService.PrepareCurrentDocument(loggerFieldDeclaration, textSelection.CurrentLine, textSelection.CurrentColumn);
                if (currentStatement != null)
                    messageService.ReplaceCurrentStatement(currentStatement, invocation);

                workspace.TryApplyChanges(messageService.CurrentProject.Solution);

                messageService.AddMessageToResource(loggerMessage);

                if (currentStatement == null)
                {
                    var textView = docView.TextView;
                    SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
                    textView.TextBuffer.Insert(caretPosition, invocation.ToFullString());
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }
    }
}
