using System;
using System.Linq;
using System.Windows;
using LoggerMessage.Shared;
using LoggerMessageExtension.Exceptions;
using LoggerMessages.Roslyn;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace LoggerMessageExtension.Views
{
    /// <summary>
    /// Логика взаимодействия для LoggerMessageEditorWindow.xaml
    /// </summary>
    public partial class LoggerMessageEditorWindow : DialogWindow
    {        
        private readonly LoggerMessageExtensionPackage _package;

        public MessageMethod MessageMethod { get; set; }

        public LoggerMessageEditorWindow(LoggerMessageExtensionPackage package, MessageMethod messageMethod = null)
        {
            InitializeComponent();
            _package = package;
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                ScopesComboBox.ItemsSource = await _package.EventGroupService.GetEventGroupsAsync();
            });

            MessageMethod = messageMethod;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_package.EventGroupService.Connected)
                throw new FailedConnectionException();

            MessageMethod.MessageTemplate = MessageTextBox.Text;
            MessageMethod.Group = ScopesComboBox.SelectedItem as IEventGroup;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            MessageMethod = null;
            Close();
        }

        private void AddScopeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_package.EventGroupService.Connected)
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
                ScopesComboBox.ItemsSource = await _package.EventGroupService.GetEventGroupsAsync();
            });

            var items = ScopesComboBox.Items.Cast<EventGroupViewObject>().ToList();
            var createdItem = items.FirstOrDefault(i => i.Abbreviation == newScopeEditor.EventGroupViewObject.Abbreviation);
            ScopesComboBox.SelectedIndex = items.IndexOf(createdItem);
        }
    }
}
