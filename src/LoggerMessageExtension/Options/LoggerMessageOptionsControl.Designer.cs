
namespace LoggerMessageExtension.Options
{
    partial class LoggerMessageOptionsControl
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbServiceUrl = new System.Windows.Forms.TextBox();
            this.cbIsShared = new System.Windows.Forms.CheckBox();
            this.lServiceUrl = new System.Windows.Forms.Label();
            this.bTestConnection = new System.Windows.Forms.Button();
            this.tbTestResults = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.tbApiKey = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbServiceUrl
            // 
            this.tbServiceUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbServiceUrl.Location = new System.Drawing.Point(111, 23);
            this.tbServiceUrl.Name = "tbServiceUrl";
            this.tbServiceUrl.Size = new System.Drawing.Size(408, 20);
            this.tbServiceUrl.TabIndex = 0;
            this.tbServiceUrl.TextChanged += new System.EventHandler(this.tbServiceUrl_TextChanged);
            this.tbServiceUrl.Leave += new System.EventHandler(this.tbServiceUrl_Leave);
            // 
            // cbIsShared
            // 
            this.cbIsShared.AutoSize = true;
            this.cbIsShared.Location = new System.Drawing.Point(3, 3);
            this.cbIsShared.Name = "cbIsShared";
            this.cbIsShared.Size = new System.Drawing.Size(109, 17);
            this.cbIsShared.TabIndex = 1;
            this.cbIsShared.Text = "Solution is shared";
            this.cbIsShared.UseVisualStyleBackColor = true;
            this.cbIsShared.CheckedChanged += new System.EventHandler(this.cbIsShared_CheckedChanged);
            // 
            // lServiceUrl
            // 
            this.lServiceUrl.AutoSize = true;
            this.lServiceUrl.Location = new System.Drawing.Point(22, 26);
            this.lServiceUrl.Name = "lServiceUrl";
            this.lServiceUrl.Size = new System.Drawing.Size(83, 13);
            this.lServiceUrl.TabIndex = 2;
            this.lServiceUrl.Text = "Service address";            
            // 
            // bTestConnection
            // 
            this.bTestConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bTestConnection.Location = new System.Drawing.Point(111, 75);
            this.bTestConnection.Name = "bTestConnection";
            this.bTestConnection.Size = new System.Drawing.Size(123, 25);
            this.bTestConnection.TabIndex = 3;
            this.bTestConnection.Text = "Test connection";
            this.bTestConnection.UseVisualStyleBackColor = true;
            this.bTestConnection.Click += new System.EventHandler(this.bLogin_Click);
            // 
            // tbTestResults
            // 
            this.tbTestResults.AcceptsReturn = true;
            this.tbTestResults.AcceptsTab = true;
            this.tbTestResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTestResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbTestResults.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbTestResults.Location = new System.Drawing.Point(3, 106);
            this.tbTestResults.Multiline = true;
            this.tbTestResults.Name = "tbTestResults";
            this.tbTestResults.ReadOnly = true;
            this.tbTestResults.Size = new System.Drawing.Size(592, 247);
            this.tbTestResults.TabIndex = 5;
            this.tbTestResults.Text = "TEST CONNECTION RESULT";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.tbApiKey);
            this.panel1.Controls.Add(this.cbIsShared);
            this.panel1.Controls.Add(this.tbTestResults);
            this.panel1.Controls.Add(this.lServiceUrl);
            this.panel1.Controls.Add(this.bTestConnection);
            this.panel1.Controls.Add(this.tbServiceUrl);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(598, 353);
            this.panel1.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "ApiKey";            
            // 
            // tbApiKey
            // 
            this.tbApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbApiKey.Location = new System.Drawing.Point(111, 49);
            this.tbApiKey.Name = "tbApiKey";
            this.tbApiKey.Size = new System.Drawing.Size(408, 20);
            this.tbApiKey.TabIndex = 10;
            this.tbApiKey.Leave += new System.EventHandler(this.tbApiKey_Leave);
            // 
            // LoggerMessageOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "LoggerMessageOptionsControl";
            this.Size = new System.Drawing.Size(598, 353);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tbServiceUrl;
        private System.Windows.Forms.CheckBox cbIsShared;
        private System.Windows.Forms.Label lServiceUrl;
        private System.Windows.Forms.Button bTestConnection;
        private System.Windows.Forms.TextBox tbTestResults;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbApiKey;
    }
}
