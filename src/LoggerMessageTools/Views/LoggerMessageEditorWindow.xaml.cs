using System.Linq;
using System.Windows;
using LoggerMessage.Shared;
using LoggerMessage.Shared.Exceptions;
using LoggerMessage.Shared.Services;
using Microsoft.VisualStudio.PlatformUI;

namespace LoggerMessageTools.Views
{
    /// <summary>
    /// Логика взаимодействия для LoggerMessageEditorWindow.xaml
    /// </summary>
    public partial class LoggerMessageEditorWindow : DialogWindow
    {        
        private readonly AsyncPackage _package;
        private readonly IEventGroupService _eventGroupService;

        public LoggerMessage.Shared.LoggerMessage Message { get; set; }

        public LoggerMessageEditorWindow(AsyncPackage package, LoggerMessage.Shared.LoggerMessage message = null)
        {
            InitializeComponent();
            _package = package;

            _eventGroupService = _package.GetServiceAsync(typeof(IEventGroupService)).Result as IEventGroupService;
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                ScopesComboBox.ItemsSource = await _eventGroupService.GetEventGroupsAsync();
            });

            Message = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_eventGroupService.Connected)
                throw new FailedConnectionException();

            Message.MessageTemplate = MessageTextBox.Text;
            Message.Group = ScopesComboBox.SelectedItem as IEventGroup;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Message = null;
            Close();
        }

        private void AddScopeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_eventGroupService.Connected)
                throw new FailedConnectionException();

            var newScopeEditor = new NewScopeWindow(_package);
            if (this.ScopesComboBox.SelectedItem == null)
            {
                if (this.ScopesComboBox.Text != string.Empty)
                    newScopeEditor.AbbrTextBox.Text = this.ScopesComboBox.Text.Substring(0, Math.Min(Constants.AbbrLength, this.ScopesComboBox.Text.Length)).ToUpper();
            }
            newScopeEditor.ShowModal();

            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                ScopesComboBox.ItemsSource = await _eventGroupService.GetEventGroupsAsync();
            });

            var items = ScopesComboBox.Items.Cast<EventGroupViewObject>().ToList();
            var createdItem = items.FirstOrDefault(i => i.Abbreviation == newScopeEditor.EventGroupViewObject.Abbreviation);
            ScopesComboBox.SelectedIndex = items.IndexOf(createdItem);
        }
    }
}
