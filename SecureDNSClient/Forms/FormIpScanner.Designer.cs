namespace SecureDNSClient
{
    partial class FormIpScanner
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormIpScanner));
            this.CustomRadioButtonSourceCloudflare = new CustomControls.CustomRadioButton();
            this.CustomLabelDelay = new CustomControls.CustomLabel();
            this.CustomNumericUpDownDelay = new CustomControls.CustomNumericUpDown();
            this.CustomButtonStartStop = new CustomControls.CustomButton();
            this.CustomDataGridViewResult = new CustomControls.CustomDataGridView();
            this.RealDelay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TCPDelay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PingDelay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CleanIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CustomLabelChecking = new CustomControls.CustomLabel();
            this.CustomContextMenuStripMain = new CustomControls.CustomContextMenuStrip();
            this.CustomLabelCheckWebsite = new CustomControls.CustomLabel();
            this.CustomTextBoxCheckWebsite = new CustomControls.CustomTextBox();
            this.CustomLabelProxyPort = new CustomControls.CustomLabel();
            this.CustomNumericUpDownProxyPort = new CustomControls.CustomNumericUpDown();
            this.CustomCheckBoxRandomScan = new CustomControls.CustomCheckBox();
            this.CustomLabelCheckIpWithThisPort = new CustomControls.CustomLabel();
            this.CustomNumericUpDownCheckIpWithThisPort = new CustomControls.CustomNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomDataGridViewResult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownProxyPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownCheckIpWithThisPort)).BeginInit();
            this.SuspendLayout();
            // 
            // CustomRadioButtonSourceCloudflare
            // 
            this.CustomRadioButtonSourceCloudflare.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSourceCloudflare.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSourceCloudflare.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSourceCloudflare.Checked = true;
            this.CustomRadioButtonSourceCloudflare.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSourceCloudflare.Location = new System.Drawing.Point(12, 10);
            this.CustomRadioButtonSourceCloudflare.Name = "CustomRadioButtonSourceCloudflare";
            this.CustomRadioButtonSourceCloudflare.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSourceCloudflare.Size = new System.Drawing.Size(77, 17);
            this.CustomRadioButtonSourceCloudflare.TabIndex = 0;
            this.CustomRadioButtonSourceCloudflare.TabStop = true;
            this.CustomRadioButtonSourceCloudflare.Text = "Cloudflare";
            this.CustomRadioButtonSourceCloudflare.UseVisualStyleBackColor = false;
            // 
            // CustomLabelDelay
            // 
            this.CustomLabelDelay.AutoSize = true;
            this.CustomLabelDelay.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelDelay.Border = false;
            this.CustomLabelDelay.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelDelay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelDelay.ForeColor = System.Drawing.Color.White;
            this.CustomLabelDelay.Location = new System.Drawing.Point(105, 10);
            this.CustomLabelDelay.Name = "CustomLabelDelay";
            this.CustomLabelDelay.RoundedCorners = 0;
            this.CustomLabelDelay.Size = new System.Drawing.Size(95, 17);
            this.CustomLabelDelay.TabIndex = 1;
            this.CustomLabelDelay.Text = "Real Delay (Sec):";
            // 
            // CustomNumericUpDownDelay
            // 
            this.CustomNumericUpDownDelay.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownDelay.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownDelay.Location = new System.Drawing.Point(206, 8);
            this.CustomNumericUpDownDelay.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.CustomNumericUpDownDelay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownDelay.Name = "CustomNumericUpDownDelay";
            this.CustomNumericUpDownDelay.Size = new System.Drawing.Size(45, 23);
            this.CustomNumericUpDownDelay.TabIndex = 2;
            this.CustomNumericUpDownDelay.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // CustomButtonStartStop
            // 
            this.CustomButtonStartStop.AutoSize = true;
            this.CustomButtonStartStop.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonStartStop.Location = new System.Drawing.Point(276, 8);
            this.CustomButtonStartStop.Name = "CustomButtonStartStop";
            this.CustomButtonStartStop.RoundedCorners = 5;
            this.CustomButtonStartStop.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonStartStop.Size = new System.Drawing.Size(75, 27);
            this.CustomButtonStartStop.TabIndex = 3;
            this.CustomButtonStartStop.Text = "Start/Stop";
            this.CustomButtonStartStop.UseVisualStyleBackColor = true;
            this.CustomButtonStartStop.Click += new System.EventHandler(this.CustomButtonStartStop_Click);
            // 
            // CustomDataGridViewResult
            // 
            this.CustomDataGridViewResult.AllowUserToAddRows = false;
            this.CustomDataGridViewResult.AllowUserToDeleteRows = false;
            this.CustomDataGridViewResult.AllowUserToResizeRows = false;
            this.CustomDataGridViewResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomDataGridViewResult.BorderColor = System.Drawing.Color.Blue;
            this.CustomDataGridViewResult.CheckColor = System.Drawing.Color.Blue;
            this.CustomDataGridViewResult.ColumnHeadersBorder = true;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(73)))), ((int)(((byte)(73)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(73)))), ((int)(((byte)(73)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.CustomDataGridViewResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.CustomDataGridViewResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CustomDataGridViewResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RealDelay,
            this.TCPDelay,
            this.PingDelay,
            this.CleanIP});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.DimGray;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(97)))), ((int)(((byte)(177)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.CustomDataGridViewResult.DefaultCellStyle = dataGridViewCellStyle2;
            this.CustomDataGridViewResult.GridColor = System.Drawing.Color.LightBlue;
            this.CustomDataGridViewResult.Location = new System.Drawing.Point(12, 170);
            this.CustomDataGridViewResult.MultiSelect = false;
            this.CustomDataGridViewResult.Name = "CustomDataGridViewResult";
            this.CustomDataGridViewResult.ReadOnly = true;
            this.CustomDataGridViewResult.RowHeadersVisible = false;
            this.CustomDataGridViewResult.RowTemplate.Height = 25;
            this.CustomDataGridViewResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CustomDataGridViewResult.SelectionColor = System.Drawing.Color.DodgerBlue;
            this.CustomDataGridViewResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.CustomDataGridViewResult.SelectionModeFocus = false;
            this.CustomDataGridViewResult.ShowCellErrors = false;
            this.CustomDataGridViewResult.ShowEditingIcon = false;
            this.CustomDataGridViewResult.ShowRowErrors = false;
            this.CustomDataGridViewResult.Size = new System.Drawing.Size(360, 229);
            this.CustomDataGridViewResult.TabIndex = 7;
            this.CustomDataGridViewResult.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CustomDataGridViewResult_MouseClick);
            // 
            // RealDelay
            // 
            this.RealDelay.HeaderText = "Real Delay";
            this.RealDelay.MinimumWidth = 60;
            this.RealDelay.Name = "RealDelay";
            this.RealDelay.ReadOnly = true;
            this.RealDelay.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.RealDelay.Width = 60;
            // 
            // TCPDelay
            // 
            this.TCPDelay.HeaderText = "TCP Delay";
            this.TCPDelay.MinimumWidth = 60;
            this.TCPDelay.Name = "TCPDelay";
            this.TCPDelay.ReadOnly = true;
            this.TCPDelay.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.TCPDelay.Width = 60;
            // 
            // PingDelay
            // 
            this.PingDelay.HeaderText = "Ping Delay";
            this.PingDelay.MinimumWidth = 60;
            this.PingDelay.Name = "PingDelay";
            this.PingDelay.ReadOnly = true;
            this.PingDelay.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.PingDelay.Width = 60;
            // 
            // CleanIP
            // 
            this.CleanIP.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.CleanIP.HeaderText = "Clean IP";
            this.CleanIP.Name = "CleanIP";
            this.CleanIP.ReadOnly = true;
            // 
            // CustomLabelChecking
            // 
            this.CustomLabelChecking.AutoSize = true;
            this.CustomLabelChecking.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelChecking.Border = false;
            this.CustomLabelChecking.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelChecking.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelChecking.ForeColor = System.Drawing.Color.White;
            this.CustomLabelChecking.Location = new System.Drawing.Point(12, 150);
            this.CustomLabelChecking.Name = "CustomLabelChecking";
            this.CustomLabelChecking.RoundedCorners = 0;
            this.CustomLabelChecking.Size = new System.Drawing.Size(146, 17);
            this.CustomLabelChecking.TabIndex = 8;
            this.CustomLabelChecking.Text = "Checking: 255.255.255.255";
            // 
            // CustomContextMenuStripMain
            // 
            this.CustomContextMenuStripMain.BackColor = System.Drawing.Color.DimGray;
            this.CustomContextMenuStripMain.BorderColor = System.Drawing.Color.Blue;
            this.CustomContextMenuStripMain.ForeColor = System.Drawing.Color.White;
            this.CustomContextMenuStripMain.Name = "CustomContextMenuStripMain";
            this.CustomContextMenuStripMain.SameColorForSubItems = true;
            this.CustomContextMenuStripMain.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomContextMenuStripMain.Size = new System.Drawing.Size(61, 4);
            // 
            // CustomLabelCheckWebsite
            // 
            this.CustomLabelCheckWebsite.AutoSize = true;
            this.CustomLabelCheckWebsite.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelCheckWebsite.Border = false;
            this.CustomLabelCheckWebsite.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelCheckWebsite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelCheckWebsite.ForeColor = System.Drawing.Color.White;
            this.CustomLabelCheckWebsite.Location = new System.Drawing.Point(12, 45);
            this.CustomLabelCheckWebsite.Name = "CustomLabelCheckWebsite";
            this.CustomLabelCheckWebsite.RoundedCorners = 0;
            this.CustomLabelCheckWebsite.Size = new System.Drawing.Size(90, 17);
            this.CustomLabelCheckWebsite.TabIndex = 9;
            this.CustomLabelCheckWebsite.Text = "Check Website:";
            // 
            // CustomTextBoxCheckWebsite
            // 
            this.CustomTextBoxCheckWebsite.AcceptsReturn = false;
            this.CustomTextBoxCheckWebsite.AcceptsTab = false;
            this.CustomTextBoxCheckWebsite.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxCheckWebsite.Border = true;
            this.CustomTextBoxCheckWebsite.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxCheckWebsite.BorderSize = 1;
            this.CustomTextBoxCheckWebsite.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxCheckWebsite.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxCheckWebsite.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxCheckWebsite.HideSelection = true;
            this.CustomTextBoxCheckWebsite.Location = new System.Drawing.Point(108, 43);
            this.CustomTextBoxCheckWebsite.MaxLength = 32767;
            this.CustomTextBoxCheckWebsite.Multiline = false;
            this.CustomTextBoxCheckWebsite.Name = "CustomTextBoxCheckWebsite";
            this.CustomTextBoxCheckWebsite.ReadOnly = false;
            this.CustomTextBoxCheckWebsite.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxCheckWebsite.ShortcutsEnabled = true;
            this.CustomTextBoxCheckWebsite.Size = new System.Drawing.Size(200, 23);
            this.CustomTextBoxCheckWebsite.TabIndex = 0;
            this.CustomTextBoxCheckWebsite.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxCheckWebsite.Texts = "https://www.cloudflare.com";
            this.CustomTextBoxCheckWebsite.UnderlinedStyle = true;
            this.CustomTextBoxCheckWebsite.UsePasswordChar = false;
            this.CustomTextBoxCheckWebsite.WordWrap = true;
            // 
            // CustomLabelProxyPort
            // 
            this.CustomLabelProxyPort.AutoSize = true;
            this.CustomLabelProxyPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelProxyPort.Border = false;
            this.CustomLabelProxyPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelProxyPort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelProxyPort.ForeColor = System.Drawing.Color.White;
            this.CustomLabelProxyPort.Location = new System.Drawing.Point(12, 115);
            this.CustomLabelProxyPort.Name = "CustomLabelProxyPort";
            this.CustomLabelProxyPort.RoundedCorners = 0;
            this.CustomLabelProxyPort.Size = new System.Drawing.Size(67, 17);
            this.CustomLabelProxyPort.TabIndex = 11;
            this.CustomLabelProxyPort.Text = "Proxy Port:";
            // 
            // CustomNumericUpDownProxyPort
            // 
            this.CustomNumericUpDownProxyPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownProxyPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownProxyPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownProxyPort.Location = new System.Drawing.Point(85, 113);
            this.CustomNumericUpDownProxyPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.CustomNumericUpDownProxyPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownProxyPort.Name = "CustomNumericUpDownProxyPort";
            this.CustomNumericUpDownProxyPort.Size = new System.Drawing.Size(55, 23);
            this.CustomNumericUpDownProxyPort.TabIndex = 12;
            this.CustomNumericUpDownProxyPort.Value = new decimal(new int[] {
            8090,
            0,
            0,
            0});
            // 
            // CustomCheckBoxRandomScan
            // 
            this.CustomCheckBoxRandomScan.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxRandomScan.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxRandomScan.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxRandomScan.Checked = true;
            this.CustomCheckBoxRandomScan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxRandomScan.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxRandomScan.Location = new System.Drawing.Point(167, 115);
            this.CustomCheckBoxRandomScan.Name = "CustomCheckBoxRandomScan";
            this.CustomCheckBoxRandomScan.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxRandomScan.Size = new System.Drawing.Size(131, 17);
            this.CustomCheckBoxRandomScan.TabIndex = 13;
            this.CustomCheckBoxRandomScan.Text = "Check IPs Randomly";
            this.CustomCheckBoxRandomScan.UseVisualStyleBackColor = false;
            // 
            // CustomLabelCheckIpWithThisPort
            // 
            this.CustomLabelCheckIpWithThisPort.AutoSize = true;
            this.CustomLabelCheckIpWithThisPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelCheckIpWithThisPort.Border = false;
            this.CustomLabelCheckIpWithThisPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelCheckIpWithThisPort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelCheckIpWithThisPort.ForeColor = System.Drawing.Color.White;
            this.CustomLabelCheckIpWithThisPort.Location = new System.Drawing.Point(12, 80);
            this.CustomLabelCheckIpWithThisPort.Name = "CustomLabelCheckIpWithThisPort";
            this.CustomLabelCheckIpWithThisPort.RoundedCorners = 0;
            this.CustomLabelCheckIpWithThisPort.Size = new System.Drawing.Size(136, 17);
            this.CustomLabelCheckIpWithThisPort.TabIndex = 15;
            this.CustomLabelCheckIpWithThisPort.Text = "Check IPs with this Port:";
            // 
            // CustomNumericUpDownCheckIpWithThisPort
            // 
            this.CustomNumericUpDownCheckIpWithThisPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownCheckIpWithThisPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownCheckIpWithThisPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownCheckIpWithThisPort.Location = new System.Drawing.Point(154, 78);
            this.CustomNumericUpDownCheckIpWithThisPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.CustomNumericUpDownCheckIpWithThisPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownCheckIpWithThisPort.Name = "CustomNumericUpDownCheckIpWithThisPort";
            this.CustomNumericUpDownCheckIpWithThisPort.Size = new System.Drawing.Size(55, 23);
            this.CustomNumericUpDownCheckIpWithThisPort.TabIndex = 16;
            this.CustomNumericUpDownCheckIpWithThisPort.Value = new decimal(new int[] {
            443,
            0,
            0,
            0});
            // 
            // FormIpScanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(384, 411);
            this.Controls.Add(this.CustomNumericUpDownCheckIpWithThisPort);
            this.Controls.Add(this.CustomLabelCheckIpWithThisPort);
            this.Controls.Add(this.CustomCheckBoxRandomScan);
            this.Controls.Add(this.CustomNumericUpDownProxyPort);
            this.Controls.Add(this.CustomLabelProxyPort);
            this.Controls.Add(this.CustomTextBoxCheckWebsite);
            this.Controls.Add(this.CustomLabelCheckWebsite);
            this.Controls.Add(this.CustomLabelChecking);
            this.Controls.Add(this.CustomDataGridViewResult);
            this.Controls.Add(this.CustomButtonStartStop);
            this.Controls.Add(this.CustomNumericUpDownDelay);
            this.Controls.Add(this.CustomLabelDelay);
            this.Controls.Add(this.CustomRadioButtonSourceCloudflare);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormIpScanner";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Clean IP Scanner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormIpScanner_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomDataGridViewResult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownProxyPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownCheckIpWithThisPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CustomControls.CustomRadioButton CustomRadioButtonSourceCloudflare;
        private CustomControls.CustomLabel CustomLabelDelay;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownDelay;
        private CustomControls.CustomButton CustomButtonStartStop;
        private CustomControls.CustomDataGridView CustomDataGridViewResult;
        private CustomControls.CustomLabel CustomLabelChecking;
        private CustomControls.CustomContextMenuStrip CustomContextMenuStripMain;
        private DataGridViewTextBoxColumn RealDelay;
        private DataGridViewTextBoxColumn TCPDelay;
        private DataGridViewTextBoxColumn PingDelay;
        private DataGridViewTextBoxColumn CleanIP;
        private CustomControls.CustomLabel CustomLabelCheckWebsite;
        private CustomControls.CustomTextBox CustomTextBoxCheckWebsite;
        private CustomControls.CustomLabel CustomLabelProxyPort;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownProxyPort;
        private CustomControls.CustomCheckBox CustomCheckBoxRandomScan;
        private CustomControls.CustomLabel CustomLabelCheckIpWithThisPort;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownCheckIpWithThisPort;
    }
}