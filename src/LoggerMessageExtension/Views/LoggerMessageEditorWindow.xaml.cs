using System;
using System.Linq;
using System.Windows;
using LoggerMessageExtension.Exceptions;
using Microsoft.CodeAnalysis;
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

        public LoggerMessageEditorWindow(LoggerMessageExtensionPackage package)
        {
            InitializeComponent();
            _package = package;
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                ScopesComboBox.ItemsSource = await _package.EventGroupService.GetEventGroupsAsync();
            });            
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_package.EventGroupService.Connected)
                throw new FailedConnectionException();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
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
