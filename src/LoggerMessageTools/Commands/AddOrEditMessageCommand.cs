using LoggerMessageTools.Views;

namespace LoggerMessageTools.Commands
{
    [Command(PackageIds.AddOrEditMessageCommand)]
    internal sealed class AddOrEditMessageCommand : BaseCommand<AddOrEditMessageCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {

            await VS.Windows.ShowDialogAsync(new LoggerMessageEditorWindow(this.Package));
        }
    }
}
