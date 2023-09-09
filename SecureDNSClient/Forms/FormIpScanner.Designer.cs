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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormIpScanner));
            CustomRadioButtonSourceCloudflare = new CustomControls.CustomRadioButton();
            CustomLabelDelay = new CustomControls.CustomLabel();
            CustomNumericUpDownDelay = new CustomControls.CustomNumericUpDown();
            CustomButtonStartStop = new CustomControls.CustomButton();
            CustomDataGridViewResult = new CustomControls.CustomDataGridView();
            RealDelay = new DataGridViewTextBoxColumn();
            TCPDelay = new DataGridViewTextBoxColumn();
            PingDelay = new DataGridViewTextBoxColumn();
            CleanIP = new DataGridViewTextBoxColumn();
            CustomLabelChecking = new CustomControls.CustomLabel();
            CustomContextMenuStripMain = new CustomControls.CustomContextMenuStrip();
            CustomLabelCheckWebsite = new CustomControls.CustomLabel();
            CustomTextBoxCheckWebsite = new CustomControls.CustomTextBox();
            CustomLabelProxyPort = new CustomControls.CustomLabel();
            CustomNumericUpDownProxyPort = new CustomControls.CustomNumericUpDown();
            CustomCheckBoxRandomScan = new CustomControls.CustomCheckBox();
            CustomLabelCheckIpWithThisPort = new CustomControls.CustomLabel();
            CustomNumericUpDownCheckIpWithThisPort = new CustomControls.CustomNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CustomDataGridViewResult).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownProxyPort).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownCheckIpWithThisPort).BeginInit();
            SuspendLayout();
            // 
            // CustomRadioButtonSourceCloudflare
            // 
            CustomRadioButtonSourceCloudflare.BackColor = Color.DimGray;
            CustomRadioButtonSourceCloudflare.BorderColor = Color.Blue;
            CustomRadioButtonSourceCloudflare.CheckColor = Color.Blue;
            CustomRadioButtonSourceCloudflare.Checked = true;
            CustomRadioButtonSourceCloudflare.ForeColor = Color.White;
            CustomRadioButtonSourceCloudflare.Location = new Point(12, 10);
            CustomRadioButtonSourceCloudflare.Name = "CustomRadioButtonSourceCloudflare";
            CustomRadioButtonSourceCloudflare.SelectionColor = Color.LightBlue;
            CustomRadioButtonSourceCloudflare.Size = new Size(77, 17);
            CustomRadioButtonSourceCloudflare.TabIndex = 0;
            CustomRadioButtonSourceCloudflare.TabStop = true;
            CustomRadioButtonSourceCloudflare.Text = "Cloudflare";
            CustomRadioButtonSourceCloudflare.UseVisualStyleBackColor = false;
            // 
            // CustomLabelDelay
            // 
            CustomLabelDelay.AutoSize = true;
            CustomLabelDelay.BackColor = Color.DimGray;
            CustomLabelDelay.Border = false;
            CustomLabelDelay.BorderColor = Color.Blue;
            CustomLabelDelay.FlatStyle = FlatStyle.Flat;
            CustomLabelDelay.ForeColor = Color.White;
            CustomLabelDelay.Location = new Point(105, 10);
            CustomLabelDelay.Name = "CustomLabelDelay";
            CustomLabelDelay.RoundedCorners = 0;
            CustomLabelDelay.Size = new Size(95, 17);
            CustomLabelDelay.TabIndex = 1;
            CustomLabelDelay.Text = "Real Delay (Sec):";
            // 
            // CustomNumericUpDownDelay
            // 
            CustomNumericUpDownDelay.BackColor = Color.DimGray;
            CustomNumericUpDownDelay.BorderColor = Color.Blue;
            CustomNumericUpDownDelay.BorderStyle = BorderStyle.FixedSingle;
            CustomNumericUpDownDelay.Location = new Point(206, 8);
            CustomNumericUpDownDelay.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            CustomNumericUpDownDelay.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            CustomNumericUpDownDelay.Name = "CustomNumericUpDownDelay";
            CustomNumericUpDownDelay.Size = new Size(45, 23);
            CustomNumericUpDownDelay.TabIndex = 2;
            CustomNumericUpDownDelay.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // CustomButtonStartStop
            // 
            CustomButtonStartStop.AutoSize = true;
            CustomButtonStartStop.BorderColor = Color.Blue;
            CustomButtonStartStop.FlatStyle = FlatStyle.Flat;
            CustomButtonStartStop.Location = new Point(276, 8);
            CustomButtonStartStop.Name = "CustomButtonStartStop";
            CustomButtonStartStop.RoundedCorners = 5;
            CustomButtonStartStop.SelectionColor = Color.LightBlue;
            CustomButtonStartStop.Size = new Size(75, 27);
            CustomButtonStartStop.TabIndex = 3;
            CustomButtonStartStop.Text = "Start/Stop";
            CustomButtonStartStop.UseVisualStyleBackColor = true;
            CustomButtonStartStop.Click += CustomButtonStartStop_Click;
            // 
            // CustomDataGridViewResult
            // 
            CustomDataGridViewResult.AllowUserToAddRows = false;
            CustomDataGridViewResult.AllowUserToDeleteRows = false;
            CustomDataGridViewResult.AllowUserToResizeRows = false;
            CustomDataGridViewResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            CustomDataGridViewResult.BorderColor = Color.Blue;
            CustomDataGridViewResult.CheckColor = Color.Blue;
            CustomDataGridViewResult.ColumnHeadersBorder = true;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(73, 73, 73);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(73, 73, 73);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            CustomDataGridViewResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            CustomDataGridViewResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            CustomDataGridViewResult.Columns.AddRange(new DataGridViewColumn[] { RealDelay, TCPDelay, PingDelay, CleanIP });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.DimGray;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(97, 177, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            CustomDataGridViewResult.DefaultCellStyle = dataGridViewCellStyle2;
            CustomDataGridViewResult.GridColor = Color.LightBlue;
            CustomDataGridViewResult.Location = new Point(12, 170);
            CustomDataGridViewResult.MultiSelect = false;
            CustomDataGridViewResult.Name = "CustomDataGridViewResult";
            CustomDataGridViewResult.ReadOnly = true;
            CustomDataGridViewResult.RowHeadersVisible = false;
            CustomDataGridViewResult.RowTemplate.Height = 25;
            CustomDataGridViewResult.ScrollBars = ScrollBars.Vertical;
            CustomDataGridViewResult.SelectionColor = Color.DodgerBlue;
            CustomDataGridViewResult.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            CustomDataGridViewResult.SelectionModeFocus = false;
            CustomDataGridViewResult.ShowCellErrors = false;
            CustomDataGridViewResult.ShowEditingIcon = false;
            CustomDataGridViewResult.ShowRowErrors = false;
            CustomDataGridViewResult.Size = new Size(360, 229);
            CustomDataGridViewResult.TabIndex = 7;
            CustomDataGridViewResult.MouseClick += CustomDataGridViewResult_MouseClick;
            // 
            // RealDelay
            // 
            RealDelay.HeaderText = "Real Delay";
            RealDelay.MinimumWidth = 60;
            RealDelay.Name = "RealDelay";
            RealDelay.ReadOnly = true;
            RealDelay.Resizable = DataGridViewTriState.False;
            RealDelay.Width = 60;
            // 
            // TCPDelay
            // 
            TCPDelay.HeaderText = "TCP Delay";
            TCPDelay.MinimumWidth = 60;
            TCPDelay.Name = "TCPDelay";
            TCPDelay.ReadOnly = true;
            TCPDelay.Resizable = DataGridViewTriState.False;
            TCPDelay.Width = 60;
            // 
            // PingDelay
            // 
            PingDelay.HeaderText = "Ping Delay";
            PingDelay.MinimumWidth = 60;
            PingDelay.Name = "PingDelay";
            PingDelay.ReadOnly = true;
            PingDelay.Resizable = DataGridViewTriState.False;
            PingDelay.Width = 60;
            // 
            // CleanIP
            // 
            CleanIP.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            CleanIP.HeaderText = "Clean IP";
            CleanIP.Name = "CleanIP";
            CleanIP.ReadOnly = true;
            // 
            // CustomLabelChecking
            // 
            CustomLabelChecking.AutoSize = true;
            CustomLabelChecking.BackColor = Color.DimGray;
            CustomLabelChecking.Border = false;
            CustomLabelChecking.BorderColor = Color.Blue;
            CustomLabelChecking.FlatStyle = FlatStyle.Flat;
            CustomLabelChecking.ForeColor = Color.White;
            CustomLabelChecking.Location = new Point(12, 150);
            CustomLabelChecking.Name = "CustomLabelChecking";
            CustomLabelChecking.RoundedCorners = 0;
            CustomLabelChecking.Size = new Size(146, 17);
            CustomLabelChecking.TabIndex = 8;
            CustomLabelChecking.Text = "Checking: 255.255.255.255";
            // 
            // CustomContextMenuStripMain
            // 
            CustomContextMenuStripMain.BackColor = Color.DimGray;
            CustomContextMenuStripMain.BorderColor = Color.Blue;
            CustomContextMenuStripMain.ForeColor = Color.White;
            CustomContextMenuStripMain.Name = "CustomContextMenuStripMain";
            CustomContextMenuStripMain.SameColorForSubItems = true;
            CustomContextMenuStripMain.SelectionColor = Color.LightBlue;
            CustomContextMenuStripMain.Size = new Size(61, 4);
            // 
            // CustomLabelCheckWebsite
            // 
            CustomLabelCheckWebsite.AutoSize = true;
            CustomLabelCheckWebsite.BackColor = Color.DimGray;
            CustomLabelCheckWebsite.Border = false;
            CustomLabelCheckWebsite.BorderColor = Color.Blue;
            CustomLabelCheckWebsite.FlatStyle = FlatStyle.Flat;
            CustomLabelCheckWebsite.ForeColor = Color.White;
            CustomLabelCheckWebsite.Location = new Point(12, 45);
            CustomLabelCheckWebsite.Name = "CustomLabelCheckWebsite";
            CustomLabelCheckWebsite.RoundedCorners = 0;
            CustomLabelCheckWebsite.Size = new Size(90, 17);
            CustomLabelCheckWebsite.TabIndex = 9;
            CustomLabelCheckWebsite.Text = "Check Website:";
            // 
            // CustomTextBoxCheckWebsite
            // 
            CustomTextBoxCheckWebsite.AcceptsReturn = false;
            CustomTextBoxCheckWebsite.AcceptsTab = false;
            CustomTextBoxCheckWebsite.BackColor = Color.DimGray;
            CustomTextBoxCheckWebsite.Border = true;
            CustomTextBoxCheckWebsite.BorderColor = Color.Blue;
            CustomTextBoxCheckWebsite.BorderSize = 1;
            CustomTextBoxCheckWebsite.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxCheckWebsite.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxCheckWebsite.ForeColor = Color.White;
            CustomTextBoxCheckWebsite.HideSelection = true;
            CustomTextBoxCheckWebsite.Location = new Point(108, 43);
            CustomTextBoxCheckWebsite.MaxLength = 32767;
            CustomTextBoxCheckWebsite.Multiline = false;
            CustomTextBoxCheckWebsite.Name = "CustomTextBoxCheckWebsite";
            CustomTextBoxCheckWebsite.ReadOnly = false;
            CustomTextBoxCheckWebsite.ScrollBars = ScrollBars.None;
            CustomTextBoxCheckWebsite.ShortcutsEnabled = true;
            CustomTextBoxCheckWebsite.Size = new Size(200, 23);
            CustomTextBoxCheckWebsite.TabIndex = 0;
            CustomTextBoxCheckWebsite.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxCheckWebsite.Texts = "https://www.cloudflare.com";
            CustomTextBoxCheckWebsite.UnderlinedStyle = true;
            CustomTextBoxCheckWebsite.UsePasswordChar = false;
            CustomTextBoxCheckWebsite.WordWrap = true;
            // 
            // CustomLabelProxyPort
            // 
            CustomLabelProxyPort.AutoSize = true;
            CustomLabelProxyPort.BackColor = Color.DimGray;
            CustomLabelProxyPort.Border = false;
            CustomLabelProxyPort.BorderColor = Color.Blue;
            CustomLabelProxyPort.FlatStyle = FlatStyle.Flat;
            CustomLabelProxyPort.ForeColor = Color.White;
            CustomLabelProxyPort.Location = new Point(12, 115);
            CustomLabelProxyPort.Name = "CustomLabelProxyPort";
            CustomLabelProxyPort.RoundedCorners = 0;
            CustomLabelProxyPort.Size = new Size(67, 17);
            CustomLabelProxyPort.TabIndex = 11;
            CustomLabelProxyPort.Text = "Proxy Port:";
            // 
            // CustomNumericUpDownProxyPort
            // 
            CustomNumericUpDownProxyPort.BackColor = Color.DimGray;
            CustomNumericUpDownProxyPort.BorderColor = Color.Blue;
            CustomNumericUpDownProxyPort.BorderStyle = BorderStyle.FixedSingle;
            CustomNumericUpDownProxyPort.Location = new Point(85, 113);
            CustomNumericUpDownProxyPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            CustomNumericUpDownProxyPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            CustomNumericUpDownProxyPort.Name = "CustomNumericUpDownProxyPort";
            CustomNumericUpDownProxyPort.Size = new Size(55, 23);
            CustomNumericUpDownProxyPort.TabIndex = 12;
            CustomNumericUpDownProxyPort.Value = new decimal(new int[] { 8090, 0, 0, 0 });
            // 
            // CustomCheckBoxRandomScan
            // 
            CustomCheckBoxRandomScan.BackColor = Color.DimGray;
            CustomCheckBoxRandomScan.BorderColor = Color.Blue;
            CustomCheckBoxRandomScan.CheckColor = Color.Blue;
            CustomCheckBoxRandomScan.Checked = true;
            CustomCheckBoxRandomScan.CheckState = CheckState.Checked;
            CustomCheckBoxRandomScan.ForeColor = Color.White;
            CustomCheckBoxRandomScan.Location = new Point(167, 115);
            CustomCheckBoxRandomScan.Name = "CustomCheckBoxRandomScan";
            CustomCheckBoxRandomScan.SelectionColor = Color.LightBlue;
            CustomCheckBoxRandomScan.Size = new Size(131, 17);
            CustomCheckBoxRandomScan.TabIndex = 13;
            CustomCheckBoxRandomScan.Text = "Check IPs Randomly";
            CustomCheckBoxRandomScan.UseVisualStyleBackColor = false;
            // 
            // CustomLabelCheckIpWithThisPort
            // 
            CustomLabelCheckIpWithThisPort.AutoSize = true;
            CustomLabelCheckIpWithThisPort.BackColor = Color.DimGray;
            CustomLabelCheckIpWithThisPort.Border = false;
            CustomLabelCheckIpWithThisPort.BorderColor = Color.Blue;
            CustomLabelCheckIpWithThisPort.FlatStyle = FlatStyle.Flat;
            CustomLabelCheckIpWithThisPort.ForeColor = Color.White;
            CustomLabelCheckIpWithThisPort.Location = new Point(12, 80);
            CustomLabelCheckIpWithThisPort.Name = "CustomLabelCheckIpWithThisPort";
            CustomLabelCheckIpWithThisPort.RoundedCorners = 0;
            CustomLabelCheckIpWithThisPort.Size = new Size(136, 17);
            CustomLabelCheckIpWithThisPort.TabIndex = 15;
            CustomLabelCheckIpWithThisPort.Text = "Check IPs with this Port:";
            // 
            // CustomNumericUpDownCheckIpWithThisPort
            // 
            CustomNumericUpDownCheckIpWithThisPort.BackColor = Color.DimGray;
            CustomNumericUpDownCheckIpWithThisPort.BorderColor = Color.Blue;
            CustomNumericUpDownCheckIpWithThisPort.BorderStyle = BorderStyle.FixedSingle;
            CustomNumericUpDownCheckIpWithThisPort.Location = new Point(154, 78);
            CustomNumericUpDownCheckIpWithThisPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            CustomNumericUpDownCheckIpWithThisPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            CustomNumericUpDownCheckIpWithThisPort.Name = "CustomNumericUpDownCheckIpWithThisPort";
            CustomNumericUpDownCheckIpWithThisPort.Size = new Size(55, 23);
            CustomNumericUpDownCheckIpWithThisPort.TabIndex = 16;
            CustomNumericUpDownCheckIpWithThisPort.Value = new decimal(new int[] { 443, 0, 0, 0 });
            // 
            // FormIpScanner
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.DimGray;
            ClientSize = new Size(384, 411);
            Controls.Add(CustomNumericUpDownCheckIpWithThisPort);
            Controls.Add(CustomLabelCheckIpWithThisPort);
            Controls.Add(CustomCheckBoxRandomScan);
            Controls.Add(CustomNumericUpDownProxyPort);
            Controls.Add(CustomLabelProxyPort);
            Controls.Add(CustomTextBoxCheckWebsite);
            Controls.Add(CustomLabelCheckWebsite);
            Controls.Add(CustomLabelChecking);
            Controls.Add(CustomDataGridViewResult);
            Controls.Add(CustomButtonStartStop);
            Controls.Add(CustomNumericUpDownDelay);
            Controls.Add(CustomLabelDelay);
            Controls.Add(CustomRadioButtonSourceCloudflare);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FormIpScanner";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Clean IP Scanner";
            FormClosing += FormIpScanner_FormClosing;
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)CustomDataGridViewResult).EndInit();
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownProxyPort).EndInit();
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownCheckIpWithThisPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
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