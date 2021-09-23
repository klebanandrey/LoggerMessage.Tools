using System;
using System.Windows;
using System.Windows.Input;
using LoggerMessageExtension.Exceptions;
using LoggerMessages.Common;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace LoggerMessageExtension.Views
{
    /// <summary>
    /// Логика взаимодействия для NewScopeWindow.xaml
    /// </summary>
    public partial class NewScopeWindow : DialogWindow
    {
        private readonly LoggerMessageExtensionPackage _package;

        private EventGroupViewObject _eventGroupViewObject;
        public EventGroupViewObject EventGroupViewObject
        {
            get { return _eventGroupViewObject; }
        }

        public NewScopeWindow(LoggerMessageExtensionPackage package)
        {
            _package = package;
            InitializeComponent();
            _eventGroupViewObject = new EventGroupViewObject();
            this.AbbrTextBox.MaxLength = Constants.AbbrLength;
        }

        private void ScopeCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AbbrTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextBoxTextAllowed(e.Text);
        }

        private void textBoxValue_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text1 = e.DataObject.GetData(typeof(string)) as string;
                if (!TextBoxTextAllowed(text1)) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        private bool TextBoxTextAllowed(string inputText)
        {
            return Array.TrueForAll(inputText.ToCharArray(), c => Char.IsLetter(c));
        }

        private void ScopeAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_package.EventGroupService.Connected)
                throw new FailedConnectionException();

            _eventGroupViewObject.Abbreviation = this.AbbrTextBox.Text;
            _eventGroupViewObject.Description = this.DescrTextBox.Text;

            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await _package.EventGroupService.TryAddEventGroupAsync(_eventGroupViewObject);
            });
            
            e.Handled = true;
            Close();
        }
    }
}
