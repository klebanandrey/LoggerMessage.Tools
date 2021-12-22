using System.Threading.Tasks;
using LoggerMessage.Shared;
using LoggerMessage.Tools;
using LoggerMessage.Tools.Extensions;
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
        private async Task<Project> GetProjectAsync(Workspace ws, string fileName)
        {
            PhysicalFile file = await PhysicalFile.FromFileAsync(fileName);
            return ws.GetProject(file.ContainingProject.Name);
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var messageService = await Package.GetPackageServiceAsync<MessageService>();
            if (messageService == null)
                throw new Exception();

            var componentModel = await Package.GetPackageServiceAsync<SComponentModel>() as IComponentModel;
            if (componentModel == null)
                throw new Exception();

            var workspace = componentModel.GetService<VisualStudioWorkspace>();
            if (workspace == null)
                throw new Exception();

            var dte = await Package.GetPackageServiceAsync<EnvDTE.DTE>();
            if (dte == null)
                throw new Exception();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var currentLineText = docView.TextView.Caret.Position.BufferPosition.GetContainingLine().GetTextIncludingLineBreak();
            string fileName = docView.TextBuffer.GetFileName();
            var textSelection = dte.ActiveWindow.Selection as EnvDTE.TextSelection;

            var currentProject = await GetProjectAsync(workspace, fileName);
            var currentDocument = currentProject.GetDocument(fileName);
            var currentClassDeclaration = currentDocument.GetClassDeclaration(textSelection.CurrentLine, textSelection.CurrentColumn);

            var resxFile = messageService.GetOrCreateLoggerMessagesResx(currentDocument.Project);
            var extensionsFile = messageService.GetOrCreateLoggerMessagesExtensions(currentDocument.Project);

            var loggerExtensions = LoggerExtensions.Init(extensionsFile);

            var loggerMessage = await messageService.GetLoggerMessage(loggerExtensions, currentClassDeclaration, textSelection.CurrentLine, textSelection.CurrentColumn);

            var w = new LoggerMessageEditorWindow(this.Package, new ViewParams(loggerMessage.Abbr, loggerMessage.Level, loggerMessage.MessageTemplate));
            await VS.Windows.ShowDialogAsync(w);

            var invocation = messageService.GetLoggerMessageMethodInvocation(currentDocument, currentClassDeclaration, loggerMessage, loggerExtensions, w.ViewParams);

            extensionsFile = await loggerExtensions.FillExtensionsFile(extensionsFile);
            
            workspace.TryApplyChanges(extensionsFile.Project.Solution);

            currentClassDeclaration = currentClassDeclaration.AddCall(invocation, textSelection.CurrentLine, textSelection.CurrentColumn, ref currentDocument);

            workspace.TryApplyChanges(currentDocument.Project.Solution);
        }
    }
}
