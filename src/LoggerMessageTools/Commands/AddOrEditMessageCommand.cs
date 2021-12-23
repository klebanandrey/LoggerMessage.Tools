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

            messageService.Initialize(currentProject, currentDocument.Id, textSelection.CurrentLine, textSelection.CurrentColumn);

            var loggerMessage = await messageService.GetLoggerMessage(textSelection.CurrentLine, textSelection.CurrentColumn);

            var w = new LoggerMessageEditorWindow(this.Package, new ViewParams(loggerMessage.Abbr, loggerMessage.Level, loggerMessage.MessageTemplate));
            await VS.Windows.ShowDialogAsync(w);

            var invocation = messageService.GetLoggerMessageMethodInvocation(loggerMessage, w.ViewParams, out var loggerFieldDeclaration);

            await messageService.WriteExtensionsToFiles();
            await messageService.AddMessageToResource(loggerMessage);

            messageService.AddInvocation(invocation, loggerFieldDeclaration, textSelection.CurrentLine, textSelection.CurrentColumn);
            workspace.TryApplyChanges(messageService.CurrentProject.Solution);
        }
    }
}
