using System;
using System.Linq;
using System.Windows;
using LoggerMessageExtension.Exceptions;
using LoggerMessages.Common;
using LoggerMessages.Roslyn;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Constants = LoggerMessages.Common.Constants;

namespace LoggerMessageExtension.Views
{
    /// <summary>
    /// Логика взаимодействия для LoggerMessageEditorWindow.xaml
    /// </summary>
    public partial class LoggerMessageEditorWindow : DialogWindow
    {        
        private readonly LoggerMessageExtensionPackage _package;

        public LoggerMessage Message { get; set; }

        public LoggerMessageEditorWindow(LoggerMessageExtensionPackage package, LoggerMessage message = null)
        {
            InitializeComponent();
            _package = package;
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                ScopesComboBox.ItemsSource = await _package.EventGroupService.GetEventGroupsAsync();
            });

            Message = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_package.EventGroupService.Connected)
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
