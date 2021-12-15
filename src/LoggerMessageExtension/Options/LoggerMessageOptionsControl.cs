using LoggerMessageExtension.EventGroupsClientService;
using Microsoft.VisualStudio.Shell;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;
using LoggerMessage.Shared;

namespace LoggerMessageExtension.Options
{
    public partial class LoggerMessageOptionsControl : UserControl
    {
        public LoggerMessageOptionsControl()
        {
            InitializeComponent();
        }
        
        internal LoggerMessageOptions options;

        public void Initialize()
        {
            cbIsShared.Checked = options.Configuration.TryGetValue(Constants.IsShared, out var checkedValue) && Boolean.Parse(checkedValue.ToString());
            tbServiceUrl.Text = options.Configuration.TryGetValue(Constants.ServiceUrl, out var urlValue) ? urlValue.ToString() : "";
            tbApiKey.Text = options.Configuration.TryGetValue(Constants.ApiKey, out var apiKeyValue) ? apiKeyValue.ToString() : "";
            tbTestResults.Text = "";
            SetTextBoxServiceState(cbIsShared.Checked);
        }

        private void SetTextBoxServiceState(bool state)
        {
            lServiceUrl.Enabled = state;
            tbServiceUrl.Enabled = state;
            tbApiKey.Enabled = state;            
            bTestConnection.Enabled = state;
        }

        private void cbIsShared_CheckedChanged(object sender, EventArgs e)
        {
            SetTextBoxServiceState(cbIsShared.Checked);
            options.Configuration[Constants.IsShared] = cbIsShared.Checked;
        }

        private void tbServiceUrl_Leave(object sender, EventArgs e)
        {
            options.Configuration[Constants.ServiceUrl] = tbServiceUrl.Text;
        }

        private void bLogin_Click(object sender, EventArgs e)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Configuration[Constants.ApiKey].ToString());

            var apiClient = new EventGroupsClient(tbServiceUrl.Text, httpClient);

            try
            {
                ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    await apiClient.ApiEventgroupsTestconnectionAsync();
                });
                
                tbTestResults.Text = "Connected";
            }
            catch (AggregateException ex)
            {
                var sb = new StringBuilder();
                foreach (var exception in ex.InnerExceptions)
                    sb.AppendLine(exception.Message);                    

                tbTestResults.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                tbTestResults.Text = ex.Message;
            }
        }

        private void tbServiceUrl_TextChanged(object sender, EventArgs e)
        {            
            options.Configuration[Constants.ServiceUrl] = tbServiceUrl.Text;
        }

        private void tbApiKey_Leave(object sender, EventArgs e)
        {
            options.Configuration[Constants.ApiKey] = tbApiKey.Text;
        }
    }
}
