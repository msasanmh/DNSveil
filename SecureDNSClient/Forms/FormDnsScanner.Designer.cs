namespace SecureDNSClient
{
    partial class FormDnsScanner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDnsScanner));
            SplitContainerMain = new SplitContainer();
            CustomButtonDnssFromURLs = new CustomControls.CustomButton();
            CustomRadioButtonDnssFromURLs = new CustomControls.CustomRadioButton();
            CustomCheckBoxCheckInsecure = new CustomControls.CustomCheckBox();
            CustomCheckBoxClearExportData = new CustomControls.CustomCheckBox();
            CustomCheckBoxSmartDNS = new CustomControls.CustomCheckBox();
            CustomButtonExport = new CustomControls.CustomButton();
            CustomLabelSmartDnsStatus = new CustomControls.CustomLabel();
            CustomButtonSmartDnsSelect = new CustomControls.CustomButton();
            CustomNumericUpDownBootstrapPort = new CustomControls.CustomNumericUpDown();
            CustomLabelBootstrapPort = new CustomControls.CustomLabel();
            CustomTextBoxBootstrapIpPort = new CustomControls.CustomTextBox();
            CustomLabelBootstrapIp = new CustomControls.CustomLabel();
            CustomNumericUpDownDnsTimeout = new CustomControls.CustomNumericUpDown();
            CustomLabelDnsTimeout = new CustomControls.CustomLabel();
            CustomLabelDnssFromFiles = new CustomControls.CustomLabel();
            CustomButtonScan = new CustomControls.CustomButton();
            CustomButtonDnssFromFiles = new CustomControls.CustomButton();
            CustomRadioButtonCustomServers = new CustomControls.CustomRadioButton();
            CustomRadioButtonDnssFromFiles = new CustomControls.CustomRadioButton();
            CustomTextBoxDnsUrl = new CustomControls.CustomTextBox();
            CustomRadioButtonDnsUrl = new CustomControls.CustomRadioButton();
            CustomRichTextBoxLog = new CustomControls.CustomRichTextBox();
            ((System.ComponentModel.ISupportInitialize)SplitContainerMain).BeginInit();
            SplitContainerMain.Panel1.SuspendLayout();
            SplitContainerMain.Panel2.SuspendLayout();
            SplitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownBootstrapPort).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownDnsTimeout).BeginInit();
            SuspendLayout();
            // 
            // SplitContainerMain
            // 
            SplitContainerMain.Dock = DockStyle.Fill;
            SplitContainerMain.Location = new Point(0, 0);
            SplitContainerMain.Name = "SplitContainerMain";
            SplitContainerMain.Orientation = Orientation.Horizontal;
            // 
            // SplitContainerMain.Panel1
            // 
            SplitContainerMain.Panel1.Controls.Add(CustomButtonDnssFromURLs);
            SplitContainerMain.Panel1.Controls.Add(CustomRadioButtonDnssFromURLs);
            SplitContainerMain.Panel1.Controls.Add(CustomCheckBoxCheckInsecure);
            SplitContainerMain.Panel1.Controls.Add(CustomCheckBoxClearExportData);
            SplitContainerMain.Panel1.Controls.Add(CustomCheckBoxSmartDNS);
            SplitContainerMain.Panel1.Controls.Add(CustomButtonExport);
            SplitContainerMain.Panel1.Controls.Add(CustomLabelSmartDnsStatus);
            SplitContainerMain.Panel1.Controls.Add(CustomButtonSmartDnsSelect);
            SplitContainerMain.Panel1.Controls.Add(CustomNumericUpDownBootstrapPort);
            SplitContainerMain.Panel1.Controls.Add(CustomLabelBootstrapPort);
            SplitContainerMain.Panel1.Controls.Add(CustomTextBoxBootstrapIpPort);
            SplitContainerMain.Panel1.Controls.Add(CustomLabelBootstrapIp);
            SplitContainerMain.Panel1.Controls.Add(CustomNumericUpDownDnsTimeout);
            SplitContainerMain.Panel1.Controls.Add(CustomLabelDnsTimeout);
            SplitContainerMain.Panel1.Controls.Add(CustomLabelDnssFromFiles);
            SplitContainerMain.Panel1.Controls.Add(CustomButtonScan);
            SplitContainerMain.Panel1.Controls.Add(CustomButtonDnssFromFiles);
            SplitContainerMain.Panel1.Controls.Add(CustomRadioButtonCustomServers);
            SplitContainerMain.Panel1.Controls.Add(CustomRadioButtonDnssFromFiles);
            SplitContainerMain.Panel1.Controls.Add(CustomTextBoxDnsUrl);
            SplitContainerMain.Panel1.Controls.Add(CustomRadioButtonDnsUrl);
            SplitContainerMain.Panel1MinSize = 250;
            // 
            // SplitContainerMain.Panel2
            // 
            SplitContainerMain.Panel2.Controls.Add(CustomRichTextBoxLog);
            SplitContainerMain.Panel2MinSize = 200;
            SplitContainerMain.Size = new Size(684, 561);
            SplitContainerMain.SplitterDistance = 290;
            SplitContainerMain.TabIndex = 0;
            // 
            // CustomButtonDnssFromURLs
            // 
            CustomButtonDnssFromURLs.BorderColor = Color.Blue;
            CustomButtonDnssFromURLs.FlatStyle = FlatStyle.Flat;
            CustomButtonDnssFromURLs.Location = new Point(136, 74);
            CustomButtonDnssFromURLs.Name = "CustomButtonDnssFromURLs";
            CustomButtonDnssFromURLs.RoundedCorners = 5;
            CustomButtonDnssFromURLs.SelectionColor = Color.LightBlue;
            CustomButtonDnssFromURLs.Size = new Size(75, 23);
            CustomButtonDnssFromURLs.TabIndex = 34;
            CustomButtonDnssFromURLs.Text = "Browse";
            CustomButtonDnssFromURLs.UseVisualStyleBackColor = true;
            CustomButtonDnssFromURLs.Click += CustomButtonDnssFromURLs_Click;
            // 
            // CustomRadioButtonDnssFromURLs
            // 
            CustomRadioButtonDnssFromURLs.BackColor = Color.DimGray;
            CustomRadioButtonDnssFromURLs.BorderColor = Color.Blue;
            CustomRadioButtonDnssFromURLs.CheckColor = Color.Blue;
            CustomRadioButtonDnssFromURLs.ForeColor = Color.White;
            CustomRadioButtonDnssFromURLs.Location = new Point(12, 78);
            CustomRadioButtonDnssFromURLs.Name = "CustomRadioButtonDnssFromURLs";
            CustomRadioButtonDnssFromURLs.SelectionColor = Color.LightBlue;
            CustomRadioButtonDnssFromURLs.Size = new Size(118, 17);
            CustomRadioButtonDnssFromURLs.TabIndex = 33;
            CustomRadioButtonDnssFromURLs.Text = "DNSs From URLs:";
            CustomRadioButtonDnssFromURLs.UseVisualStyleBackColor = false;
            CustomRadioButtonDnssFromURLs.CheckedChanged += CustomRadioButton_CheckedChanged;
            // 
            // CustomCheckBoxCheckInsecure
            // 
            CustomCheckBoxCheckInsecure.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CustomCheckBoxCheckInsecure.BackColor = Color.DimGray;
            CustomCheckBoxCheckInsecure.BorderColor = Color.Blue;
            CustomCheckBoxCheckInsecure.CheckColor = Color.Blue;
            CustomCheckBoxCheckInsecure.Checked = true;
            CustomCheckBoxCheckInsecure.CheckState = CheckState.Checked;
            CustomCheckBoxCheckInsecure.ForeColor = Color.White;
            CustomCheckBoxCheckInsecure.Location = new Point(487, 134);
            CustomCheckBoxCheckInsecure.Name = "CustomCheckBoxCheckInsecure";
            CustomCheckBoxCheckInsecure.SelectionColor = Color.LightBlue;
            CustomCheckBoxCheckInsecure.Size = new Size(174, 17);
            CustomCheckBoxCheckInsecure.TabIndex = 30;
            CustomCheckBoxCheckInsecure.Text = "Check insecure if secure fails";
            CustomCheckBoxCheckInsecure.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxClearExportData
            // 
            CustomCheckBoxClearExportData.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CustomCheckBoxClearExportData.BackColor = Color.DimGray;
            CustomCheckBoxClearExportData.BorderColor = Color.Blue;
            CustomCheckBoxClearExportData.CheckColor = Color.Blue;
            CustomCheckBoxClearExportData.Checked = true;
            CustomCheckBoxClearExportData.CheckState = CheckState.Checked;
            CustomCheckBoxClearExportData.ForeColor = Color.White;
            CustomCheckBoxClearExportData.Location = new Point(487, 111);
            CustomCheckBoxClearExportData.Name = "CustomCheckBoxClearExportData";
            CustomCheckBoxClearExportData.SelectionColor = Color.LightBlue;
            CustomCheckBoxClearExportData.Size = new Size(184, 17);
            CustomCheckBoxClearExportData.TabIndex = 27;
            CustomCheckBoxClearExportData.Text = "Clear export data on new scan";
            CustomCheckBoxClearExportData.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxSmartDNS
            // 
            CustomCheckBoxSmartDNS.BackColor = Color.DimGray;
            CustomCheckBoxSmartDNS.BorderColor = Color.Blue;
            CustomCheckBoxSmartDNS.CheckColor = Color.Blue;
            CustomCheckBoxSmartDNS.ForeColor = Color.White;
            CustomCheckBoxSmartDNS.Location = new Point(12, 149);
            CustomCheckBoxSmartDNS.Name = "CustomCheckBoxSmartDNS";
            CustomCheckBoxSmartDNS.SelectionColor = Color.LightBlue;
            CustomCheckBoxSmartDNS.Size = new Size(232, 17);
            CustomCheckBoxSmartDNS.TabIndex = 24;
            CustomCheckBoxSmartDNS.Text = "Select domains to check for SmartDNS:";
            CustomCheckBoxSmartDNS.UseVisualStyleBackColor = false;
            // 
            // CustomButtonExport
            // 
            CustomButtonExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CustomButtonExport.BorderColor = Color.Blue;
            CustomButtonExport.FlatStyle = FlatStyle.Flat;
            CustomButtonExport.Location = new Point(571, 78);
            CustomButtonExport.Name = "CustomButtonExport";
            CustomButtonExport.RoundedCorners = 5;
            CustomButtonExport.SelectionColor = Color.LightBlue;
            CustomButtonExport.Size = new Size(101, 27);
            CustomButtonExport.TabIndex = 21;
            CustomButtonExport.Text = "Export";
            CustomButtonExport.UseVisualStyleBackColor = true;
            CustomButtonExport.Click += CustomButtonExport_Click;
            // 
            // CustomLabelSmartDnsStatus
            // 
            CustomLabelSmartDnsStatus.AutoSize = true;
            CustomLabelSmartDnsStatus.BackColor = Color.DimGray;
            CustomLabelSmartDnsStatus.Border = false;
            CustomLabelSmartDnsStatus.BorderColor = Color.Blue;
            CustomLabelSmartDnsStatus.FlatStyle = FlatStyle.Flat;
            CustomLabelSmartDnsStatus.ForeColor = Color.White;
            CustomLabelSmartDnsStatus.Location = new Point(115, 176);
            CustomLabelSmartDnsStatus.Name = "CustomLabelSmartDnsStatus";
            CustomLabelSmartDnsStatus.RoundedCorners = 0;
            CustomLabelSmartDnsStatus.Size = new Size(118, 17);
            CustomLabelSmartDnsStatus.TabIndex = 20;
            CustomLabelSmartDnsStatus.Text = "No domain selected.";
            // 
            // CustomButtonSmartDnsSelect
            // 
            CustomButtonSmartDnsSelect.BorderColor = Color.Blue;
            CustomButtonSmartDnsSelect.FlatStyle = FlatStyle.Flat;
            CustomButtonSmartDnsSelect.Location = new Point(32, 172);
            CustomButtonSmartDnsSelect.Name = "CustomButtonSmartDnsSelect";
            CustomButtonSmartDnsSelect.RoundedCorners = 5;
            CustomButtonSmartDnsSelect.SelectionColor = Color.LightBlue;
            CustomButtonSmartDnsSelect.Size = new Size(75, 23);
            CustomButtonSmartDnsSelect.TabIndex = 19;
            CustomButtonSmartDnsSelect.Text = "Select";
            CustomButtonSmartDnsSelect.UseVisualStyleBackColor = true;
            CustomButtonSmartDnsSelect.Click += CustomButtonSmartDnsSelect_Click;
            // 
            // CustomNumericUpDownBootstrapPort
            // 
            CustomNumericUpDownBootstrapPort.BackColor = Color.DimGray;
            CustomNumericUpDownBootstrapPort.BorderColor = Color.Blue;
            CustomNumericUpDownBootstrapPort.BorderStyle = BorderStyle.FixedSingle;
            CustomNumericUpDownBootstrapPort.Location = new Point(311, 210);
            CustomNumericUpDownBootstrapPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            CustomNumericUpDownBootstrapPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            CustomNumericUpDownBootstrapPort.Name = "CustomNumericUpDownBootstrapPort";
            CustomNumericUpDownBootstrapPort.RoundedCorners = 5;
            CustomNumericUpDownBootstrapPort.Size = new Size(60, 23);
            CustomNumericUpDownBootstrapPort.TabIndex = 15;
            CustomNumericUpDownBootstrapPort.Value = new decimal(new int[] { 53, 0, 0, 0 });
            // 
            // CustomLabelBootstrapPort
            // 
            CustomLabelBootstrapPort.AutoSize = true;
            CustomLabelBootstrapPort.BackColor = Color.DimGray;
            CustomLabelBootstrapPort.Border = false;
            CustomLabelBootstrapPort.BorderColor = Color.Blue;
            CustomLabelBootstrapPort.FlatStyle = FlatStyle.Flat;
            CustomLabelBootstrapPort.ForeColor = Color.White;
            CustomLabelBootstrapPort.Location = new Point(220, 212);
            CustomLabelBootstrapPort.Name = "CustomLabelBootstrapPort";
            CustomLabelBootstrapPort.RoundedCorners = 0;
            CustomLabelBootstrapPort.Size = new Size(88, 17);
            CustomLabelBootstrapPort.TabIndex = 14;
            CustomLabelBootstrapPort.Text = "Bootstrap Port:";
            // 
            // CustomTextBoxBootstrapIpPort
            // 
            CustomTextBoxBootstrapIpPort.AcceptsReturn = false;
            CustomTextBoxBootstrapIpPort.AcceptsTab = false;
            CustomTextBoxBootstrapIpPort.BackColor = Color.DimGray;
            CustomTextBoxBootstrapIpPort.Border = true;
            CustomTextBoxBootstrapIpPort.BorderColor = Color.Blue;
            CustomTextBoxBootstrapIpPort.BorderSize = 1;
            CustomTextBoxBootstrapIpPort.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxBootstrapIpPort.Font = new Font("Segoe UI", 9F);
            CustomTextBoxBootstrapIpPort.ForeColor = Color.White;
            CustomTextBoxBootstrapIpPort.HideSelection = true;
            CustomTextBoxBootstrapIpPort.Location = new Point(104, 210);
            CustomTextBoxBootstrapIpPort.MaxLength = 32767;
            CustomTextBoxBootstrapIpPort.Multiline = false;
            CustomTextBoxBootstrapIpPort.Name = "CustomTextBoxBootstrapIpPort";
            CustomTextBoxBootstrapIpPort.ReadOnly = false;
            CustomTextBoxBootstrapIpPort.RoundedCorners = 0;
            CustomTextBoxBootstrapIpPort.ScrollBars = ScrollBars.None;
            CustomTextBoxBootstrapIpPort.ShortcutsEnabled = true;
            CustomTextBoxBootstrapIpPort.Size = new Size(100, 23);
            CustomTextBoxBootstrapIpPort.TabIndex = 0;
            CustomTextBoxBootstrapIpPort.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxBootstrapIpPort.Texts = "8.8.8.8";
            CustomTextBoxBootstrapIpPort.UnderlinedStyle = true;
            CustomTextBoxBootstrapIpPort.UsePasswordChar = false;
            CustomTextBoxBootstrapIpPort.WordWrap = true;
            // 
            // CustomLabelBootstrapIp
            // 
            CustomLabelBootstrapIp.AutoSize = true;
            CustomLabelBootstrapIp.BackColor = Color.DimGray;
            CustomLabelBootstrapIp.Border = false;
            CustomLabelBootstrapIp.BorderColor = Color.Blue;
            CustomLabelBootstrapIp.FlatStyle = FlatStyle.Flat;
            CustomLabelBootstrapIp.ForeColor = Color.White;
            CustomLabelBootstrapIp.Location = new Point(12, 212);
            CustomLabelBootstrapIp.Name = "CustomLabelBootstrapIp";
            CustomLabelBootstrapIp.RoundedCorners = 0;
            CustomLabelBootstrapIp.Size = new Size(88, 17);
            CustomLabelBootstrapIp.TabIndex = 12;
            CustomLabelBootstrapIp.Text = "Bootstrap IPv4:";
            // 
            // CustomNumericUpDownDnsTimeout
            // 
            CustomNumericUpDownDnsTimeout.BackColor = Color.DimGray;
            CustomNumericUpDownDnsTimeout.BorderColor = Color.Blue;
            CustomNumericUpDownDnsTimeout.BorderStyle = BorderStyle.FixedSingle;
            CustomNumericUpDownDnsTimeout.DecimalPlaces = 1;
            CustomNumericUpDownDnsTimeout.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            CustomNumericUpDownDnsTimeout.Location = new Point(106, 249);
            CustomNumericUpDownDnsTimeout.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            CustomNumericUpDownDnsTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 65536 });
            CustomNumericUpDownDnsTimeout.Name = "CustomNumericUpDownDnsTimeout";
            CustomNumericUpDownDnsTimeout.RoundedCorners = 5;
            CustomNumericUpDownDnsTimeout.Size = new Size(60, 23);
            CustomNumericUpDownDnsTimeout.TabIndex = 10;
            CustomNumericUpDownDnsTimeout.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // CustomLabelDnsTimeout
            // 
            CustomLabelDnsTimeout.AutoSize = true;
            CustomLabelDnsTimeout.BackColor = Color.DimGray;
            CustomLabelDnsTimeout.Border = false;
            CustomLabelDnsTimeout.BorderColor = Color.Blue;
            CustomLabelDnsTimeout.FlatStyle = FlatStyle.Flat;
            CustomLabelDnsTimeout.ForeColor = Color.White;
            CustomLabelDnsTimeout.Location = new Point(15, 251);
            CustomLabelDnsTimeout.Name = "CustomLabelDnsTimeout";
            CustomLabelDnsTimeout.RoundedCorners = 0;
            CustomLabelDnsTimeout.Size = new Size(85, 17);
            CustomLabelDnsTimeout.TabIndex = 9;
            CustomLabelDnsTimeout.Text = "Timeout (Sec):";
            // 
            // CustomLabelDnssFromFiles
            // 
            CustomLabelDnssFromFiles.AutoSize = true;
            CustomLabelDnssFromFiles.BackColor = Color.DimGray;
            CustomLabelDnssFromFiles.Border = false;
            CustomLabelDnssFromFiles.BorderColor = Color.Blue;
            CustomLabelDnssFromFiles.FlatStyle = FlatStyle.Flat;
            CustomLabelDnssFromFiles.ForeColor = Color.White;
            CustomLabelDnssFromFiles.Location = new Point(217, 44);
            CustomLabelDnssFromFiles.Name = "CustomLabelDnssFromFiles";
            CustomLabelDnssFromFiles.RoundedCorners = 0;
            CustomLabelDnssFromFiles.Size = new Size(93, 17);
            CustomLabelDnssFromFiles.TabIndex = 7;
            CustomLabelDnssFromFiles.Text = "No file selected.";
            // 
            // CustomButtonScan
            // 
            CustomButtonScan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CustomButtonScan.BorderColor = Color.Blue;
            CustomButtonScan.FlatStyle = FlatStyle.Flat;
            CustomButtonScan.Location = new Point(571, 45);
            CustomButtonScan.Name = "CustomButtonScan";
            CustomButtonScan.RoundedCorners = 5;
            CustomButtonScan.SelectionColor = Color.LightBlue;
            CustomButtonScan.Size = new Size(101, 27);
            CustomButtonScan.TabIndex = 5;
            CustomButtonScan.Text = "Scan";
            CustomButtonScan.UseVisualStyleBackColor = true;
            CustomButtonScan.Click += CustomButtonScan_Click;
            // 
            // CustomButtonDnssFromFiles
            // 
            CustomButtonDnssFromFiles.BorderColor = Color.Blue;
            CustomButtonDnssFromFiles.FlatStyle = FlatStyle.Flat;
            CustomButtonDnssFromFiles.Location = new Point(136, 40);
            CustomButtonDnssFromFiles.Name = "CustomButtonDnssFromFiles";
            CustomButtonDnssFromFiles.RoundedCorners = 5;
            CustomButtonDnssFromFiles.SelectionColor = Color.LightBlue;
            CustomButtonDnssFromFiles.Size = new Size(75, 23);
            CustomButtonDnssFromFiles.TabIndex = 4;
            CustomButtonDnssFromFiles.Text = "Browse";
            CustomButtonDnssFromFiles.UseVisualStyleBackColor = true;
            CustomButtonDnssFromFiles.Click += CustomButtonDnssFromFiles_Click;
            // 
            // CustomRadioButtonCustomServers
            // 
            CustomRadioButtonCustomServers.BackColor = Color.DimGray;
            CustomRadioButtonCustomServers.BorderColor = Color.Blue;
            CustomRadioButtonCustomServers.CheckColor = Color.Blue;
            CustomRadioButtonCustomServers.ForeColor = Color.White;
            CustomRadioButtonCustomServers.Location = new Point(12, 113);
            CustomRadioButtonCustomServers.Name = "CustomRadioButtonCustomServers";
            CustomRadioButtonCustomServers.SelectionColor = Color.LightBlue;
            CustomRadioButtonCustomServers.Size = new Size(136, 17);
            CustomRadioButtonCustomServers.TabIndex = 3;
            CustomRadioButtonCustomServers.Text = "Custom DNS Servers";
            CustomRadioButtonCustomServers.UseVisualStyleBackColor = false;
            CustomRadioButtonCustomServers.CheckedChanged += CustomRadioButton_CheckedChanged;
            // 
            // CustomRadioButtonDnssFromFiles
            // 
            CustomRadioButtonDnssFromFiles.BackColor = Color.DimGray;
            CustomRadioButtonDnssFromFiles.BorderColor = Color.Blue;
            CustomRadioButtonDnssFromFiles.CheckColor = Color.Blue;
            CustomRadioButtonDnssFromFiles.ForeColor = Color.White;
            CustomRadioButtonDnssFromFiles.Location = new Point(12, 42);
            CustomRadioButtonDnssFromFiles.Name = "CustomRadioButtonDnssFromFiles";
            CustomRadioButtonDnssFromFiles.SelectionColor = Color.LightBlue;
            CustomRadioButtonDnssFromFiles.Size = new Size(118, 17);
            CustomRadioButtonDnssFromFiles.TabIndex = 2;
            CustomRadioButtonDnssFromFiles.Text = "DNSs From Files:";
            CustomRadioButtonDnssFromFiles.UseVisualStyleBackColor = false;
            CustomRadioButtonDnssFromFiles.CheckedChanged += CustomRadioButtonDnssFromFiles_CheckedChanged;
            // 
            // CustomTextBoxDnsUrl
            // 
            CustomTextBoxDnsUrl.AcceptsReturn = false;
            CustomTextBoxDnsUrl.AcceptsTab = false;
            CustomTextBoxDnsUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            CustomTextBoxDnsUrl.BackColor = Color.DimGray;
            CustomTextBoxDnsUrl.Border = true;
            CustomTextBoxDnsUrl.BorderColor = Color.Blue;
            CustomTextBoxDnsUrl.BorderSize = 1;
            CustomTextBoxDnsUrl.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxDnsUrl.Font = new Font("Segoe UI", 9F);
            CustomTextBoxDnsUrl.ForeColor = Color.White;
            CustomTextBoxDnsUrl.HideSelection = true;
            CustomTextBoxDnsUrl.Location = new Point(66, 10);
            CustomTextBoxDnsUrl.MaxLength = 32767;
            CustomTextBoxDnsUrl.Multiline = false;
            CustomTextBoxDnsUrl.Name = "CustomTextBoxDnsUrl";
            CustomTextBoxDnsUrl.ReadOnly = false;
            CustomTextBoxDnsUrl.RoundedCorners = 0;
            CustomTextBoxDnsUrl.ScrollBars = ScrollBars.None;
            CustomTextBoxDnsUrl.ShortcutsEnabled = true;
            CustomTextBoxDnsUrl.Size = new Size(606, 23);
            CustomTextBoxDnsUrl.TabIndex = 0;
            CustomTextBoxDnsUrl.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxDnsUrl.Texts = "";
            CustomTextBoxDnsUrl.UnderlinedStyle = true;
            CustomTextBoxDnsUrl.UsePasswordChar = false;
            CustomTextBoxDnsUrl.WordWrap = true;
            // 
            // CustomRadioButtonDnsUrl
            // 
            CustomRadioButtonDnsUrl.BackColor = Color.DimGray;
            CustomRadioButtonDnsUrl.BorderColor = Color.Blue;
            CustomRadioButtonDnsUrl.CheckColor = Color.Blue;
            CustomRadioButtonDnsUrl.Checked = true;
            CustomRadioButtonDnsUrl.ForeColor = Color.White;
            CustomRadioButtonDnsUrl.Location = new Point(12, 12);
            CustomRadioButtonDnsUrl.Name = "CustomRadioButtonDnsUrl";
            CustomRadioButtonDnsUrl.SelectionColor = Color.LightBlue;
            CustomRadioButtonDnsUrl.Size = new Size(51, 17);
            CustomRadioButtonDnsUrl.TabIndex = 0;
            CustomRadioButtonDnsUrl.TabStop = true;
            CustomRadioButtonDnsUrl.Text = "DNS:";
            CustomRadioButtonDnsUrl.UseVisualStyleBackColor = false;
            // 
            // CustomRichTextBoxLog
            // 
            CustomRichTextBoxLog.AcceptsTab = false;
            CustomRichTextBoxLog.AutoWordSelection = false;
            CustomRichTextBoxLog.BackColor = Color.DimGray;
            CustomRichTextBoxLog.Border = true;
            CustomRichTextBoxLog.BorderColor = Color.Blue;
            CustomRichTextBoxLog.BorderSize = 1;
            CustomRichTextBoxLog.BulletIndent = 0;
            CustomRichTextBoxLog.DetectUrls = false;
            CustomRichTextBoxLog.Dock = DockStyle.Fill;
            CustomRichTextBoxLog.EnableAutoDragDrop = false;
            CustomRichTextBoxLog.Font = new Font("Segoe UI", 9F);
            CustomRichTextBoxLog.ForeColor = Color.White;
            CustomRichTextBoxLog.HideSelection = false;
            CustomRichTextBoxLog.Location = new Point(0, 0);
            CustomRichTextBoxLog.MaxLength = int.MaxValue;
            CustomRichTextBoxLog.MinimumSize = new Size(0, 23);
            CustomRichTextBoxLog.Multiline = true;
            CustomRichTextBoxLog.Name = "CustomRichTextBoxLog";
            CustomRichTextBoxLog.ReadOnly = true;
            CustomRichTextBoxLog.RightMargin = 0;
            CustomRichTextBoxLog.RoundedCorners = 5;
            CustomRichTextBoxLog.ScrollBars = ScrollBars.Vertical;
            CustomRichTextBoxLog.ScrollToBottom = false;
            CustomRichTextBoxLog.SelectionColor = Color.White;
            CustomRichTextBoxLog.SelectionLength = 0;
            CustomRichTextBoxLog.SelectionStart = 0;
            CustomRichTextBoxLog.ShortcutsEnabled = true;
            CustomRichTextBoxLog.Size = new Size(684, 267);
            CustomRichTextBoxLog.TabIndex = 0;
            CustomRichTextBoxLog.Texts = "";
            CustomRichTextBoxLog.UnderlinedStyle = false;
            CustomRichTextBoxLog.WordWrap = true;
            CustomRichTextBoxLog.ZoomFactor = 1F;
            // 
            // FormDnsScanner
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.DimGray;
            ClientSize = new Size(684, 561);
            Controls.Add(SplitContainerMain);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FormDnsScanner";
            Text = "DNS Scanner";
            FormClosing += FormDnsScanner_FormClosing;
            SplitContainerMain.Panel1.ResumeLayout(false);
            SplitContainerMain.Panel1.PerformLayout();
            SplitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SplitContainerMain).EndInit();
            SplitContainerMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownBootstrapPort).EndInit();
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownDnsTimeout).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer SplitContainerMain;
        private CustomControls.CustomRichTextBox CustomRichTextBoxLog;
        private CustomControls.CustomRadioButton CustomRadioButtonDnsUrl;
        private CustomControls.CustomRadioButton CustomRadioButtonCustomServers;
        private CustomControls.CustomRadioButton CustomRadioButtonDnssFromFiles;
        private CustomControls.CustomTextBox CustomTextBoxDnsUrl;
        private CustomControls.CustomButton CustomButtonDnssFromFiles;
        private CustomControls.CustomButton CustomButtonScan;
        private CustomControls.CustomLabel CustomLabelDnssFromFiles;
        private CustomControls.CustomLabel CustomLabelDnsTimeout;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownDnsTimeout;
        private CustomControls.CustomTextBox CustomTextBoxBootstrapIpPort;
        private CustomControls.CustomLabel CustomLabelBootstrapIp;
        private CustomControls.CustomLabel CustomLabelBootstrapPort;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownBootstrapPort;
        private CustomControls.CustomButton CustomButtonSmartDnsSelect;
        private CustomControls.CustomLabel CustomLabelSmartDnsStatus;
        private CustomControls.CustomButton CustomButtonExport;
        private CustomControls.CustomCheckBox CustomCheckBoxSmartDNS;
        private CustomControls.CustomCheckBox CustomCheckBoxClearExportData;
        private CustomControls.CustomCheckBox CustomCheckBoxCheckInsecure;
        private CustomControls.CustomRadioButton CustomRadioButtonDnssFromURLs;
        private CustomControls.CustomButton CustomButtonDnssFromURLs;
    }
}