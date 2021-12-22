using System.Linq;
using System.Windows;
using LoggerMessage.Shared;
using LoggerMessage.Shared.Exceptions;
using LoggerMessage.Shared.Services;
using LoggerMessageTools.Extensions;
using Microsoft.VisualStudio.PlatformUI;

namespace LoggerMessageTools.Views
{
    /// <summary>
    /// Логика взаимодействия для LoggerMessageEditorWindow.xaml
    /// </summary>
    public partial class LoggerMessageEditorWindow : DialogWindow
    {
        public ViewParams ViewParams { get; }
        private readonly AsyncPackage _package;
        private IEventGroupService _eventGroupService;

        public LoggerMessageEditorWindow(AsyncPackage package, ViewParams viewParams = null)
        {
            ViewParams = viewParams;
            InitializeComponent();
            _package = package;
            
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                _eventGroupService = await _package.GetPackageServiceAsync<IEventGroupService>();
                ScopesComboBox.ItemsSource = await _eventGroupService.GetEventGroupsAsync();
            });
            if (viewParams != null)
            {
                ViewParams = viewParams;
                MessageTextBox.Text = viewParams.OldTemplate;
                LevelsComboBox.SelectedIndex = (int)viewParams.OldLevel;
                if (viewParams.OldAbbr != null)
                {
                    var items = ScopesComboBox.Items.Cast<EventGroupViewObject>().ToList();
                    var createdItem = items.FirstOrDefault(i => i.Abbreviation == viewParams.OldAbbr);
                    ScopesComboBox.SelectedIndex = items.IndexOf(createdItem);
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_eventGroupService.Connected)
                throw new FailedConnectionException();

            ViewParams.NewAbbr = ((IEventGroup)ScopesComboBox.SelectedItem).Abbreviation;
            ViewParams.NewLevel = (Level)LevelsComboBox.SelectedIndex;
            ViewParams.NewTemplate = MessageTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
