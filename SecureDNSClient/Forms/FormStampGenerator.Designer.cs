namespace SecureDNSClient
{
    partial class FormStampGenerator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormStampGenerator));
            CustomLabelProtocol = new CustomControls.CustomLabel();
            CustomComboBoxProtocol = new CustomControls.CustomComboBox();
            CustomLabelIP = new CustomControls.CustomLabel();
            CustomTextBoxIP = new CustomControls.CustomTextBox();
            CustomLabelHost = new CustomControls.CustomLabel();
            CustomTextBoxHost = new CustomControls.CustomTextBox();
            CustomLabelPort = new CustomControls.CustomLabel();
            CustomNumericUpDownPort = new CustomControls.CustomNumericUpDown();
            CustomLabelPath = new CustomControls.CustomLabel();
            CustomTextBoxPath = new CustomControls.CustomTextBox();
            CustomLabelProviderName = new CustomControls.CustomLabel();
            CustomTextBoxProviderName = new CustomControls.CustomTextBox();
            CustomLabelPublicKey = new CustomControls.CustomLabel();
            CustomTextBoxPublicKey = new CustomControls.CustomTextBox();
            CustomLabelHash = new CustomControls.CustomLabel();
            CustomTextBoxHash = new CustomControls.CustomTextBox();
            CustomCheckBoxIsDnsSec = new CustomControls.CustomCheckBox();
            CustomCheckBoxIsNoFilter = new CustomControls.CustomCheckBox();
            CustomCheckBoxIsNoLog = new CustomControls.CustomCheckBox();
            CustomTextBoxStamp = new CustomControls.CustomTextBox();
            CustomButtonClear = new CustomControls.CustomButton();
            CustomButtonDecode = new CustomControls.CustomButton();
            CustomLabelStatus = new CustomControls.CustomLabel();
            CustomButtonEncode = new CustomControls.CustomButton();
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownPort).BeginInit();
            SuspendLayout();
            // 
            // CustomLabelProtocol
            // 
            CustomLabelProtocol.AutoSize = true;
            CustomLabelProtocol.BackColor = Color.DimGray;
            CustomLabelProtocol.Border = false;
            CustomLabelProtocol.BorderColor = Color.Blue;
            CustomLabelProtocol.FlatStyle = FlatStyle.Flat;
            CustomLabelProtocol.ForeColor = Color.White;
            CustomLabelProtocol.Location = new Point(12, 10);
            CustomLabelProtocol.Name = "CustomLabelProtocol";
            CustomLabelProtocol.RoundedCorners = 0;
            CustomLabelProtocol.Size = new Size(57, 17);
            CustomLabelProtocol.TabIndex = 0;
            CustomLabelProtocol.Text = "Protocol:";
            // 
            // CustomComboBoxProtocol
            // 
            CustomComboBoxProtocol.BackColor = Color.DimGray;
            CustomComboBoxProtocol.BorderColor = Color.Blue;
            CustomComboBoxProtocol.DrawMode = DrawMode.OwnerDrawVariable;
            CustomComboBoxProtocol.ForeColor = Color.White;
            CustomComboBoxProtocol.FormattingEnabled = true;
            CustomComboBoxProtocol.ItemHeight = 17;
            CustomComboBoxProtocol.Items.AddRange(new object[] { "Plain DNS", "DNSCrypt", "DNS-Over-HTTPS", "DNS-Over-TLS", "DNS-Over-Quic", "Oblivious DoH Target", "Anonymized DNSCrypt Relay", "Oblivious DoH Relay" });
            CustomComboBoxProtocol.Location = new Point(75, 8);
            CustomComboBoxProtocol.Name = "CustomComboBoxProtocol";
            CustomComboBoxProtocol.SelectionColor = Color.DodgerBlue;
            CustomComboBoxProtocol.Size = new Size(287, 23);
            CustomComboBoxProtocol.TabIndex = 1;
            CustomComboBoxProtocol.SelectedIndexChanged += CustomComboBoxProtocol_SelectedIndexChanged;
            // 
            // CustomLabelIP
            // 
            CustomLabelIP.AutoSize = true;
            CustomLabelIP.BackColor = Color.DimGray;
            CustomLabelIP.Border = false;
            CustomLabelIP.BorderColor = Color.Blue;
            CustomLabelIP.FlatStyle = FlatStyle.Flat;
            CustomLabelIP.ForeColor = Color.White;
            CustomLabelIP.Location = new Point(12, 50);
            CustomLabelIP.Name = "CustomLabelIP";
            CustomLabelIP.RoundedCorners = 0;
            CustomLabelIP.Size = new Size(274, 17);
            CustomLabelIP.TabIndex = 2;
            CustomLabelIP.Text = "IP Address (IPv6 addresses must be in [ ] brackets):";
            // 
            // CustomTextBoxIP
            // 
            CustomTextBoxIP.AcceptsReturn = false;
            CustomTextBoxIP.AcceptsTab = false;
            CustomTextBoxIP.BackColor = Color.DimGray;
            CustomTextBoxIP.Border = true;
            CustomTextBoxIP.BorderColor = Color.Blue;
            CustomTextBoxIP.BorderSize = 1;
            CustomTextBoxIP.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxIP.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxIP.ForeColor = Color.White;
            CustomTextBoxIP.HideSelection = true;
            CustomTextBoxIP.Location = new Point(12, 70);
            CustomTextBoxIP.MaxLength = 32767;
            CustomTextBoxIP.Multiline = false;
            CustomTextBoxIP.Name = "CustomTextBoxIP";
            CustomTextBoxIP.ReadOnly = false;
            CustomTextBoxIP.ScrollBars = ScrollBars.None;
            CustomTextBoxIP.ShortcutsEnabled = true;
            CustomTextBoxIP.Size = new Size(350, 23);
            CustomTextBoxIP.TabIndex = 0;
            CustomTextBoxIP.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxIP.Texts = "";
            CustomTextBoxIP.UnderlinedStyle = true;
            CustomTextBoxIP.UsePasswordChar = false;
            CustomTextBoxIP.WordWrap = true;
            // 
            // CustomLabelHost
            // 
            CustomLabelHost.AutoSize = true;
            CustomLabelHost.BackColor = Color.DimGray;
            CustomLabelHost.Border = false;
            CustomLabelHost.BorderColor = Color.Blue;
            CustomLabelHost.FlatStyle = FlatStyle.Flat;
            CustomLabelHost.ForeColor = Color.White;
            CustomLabelHost.Location = new Point(12, 110);
            CustomLabelHost.Name = "CustomLabelHost";
            CustomLabelHost.RoundedCorners = 0;
            CustomLabelHost.Size = new Size(140, 17);
            CustomLabelHost.TabIndex = 4;
            CustomLabelHost.Text = "Host Name (vHost+SNI):";
            // 
            // CustomTextBoxHost
            // 
            CustomTextBoxHost.AcceptsReturn = false;
            CustomTextBoxHost.AcceptsTab = false;
            CustomTextBoxHost.BackColor = Color.DimGray;
            CustomTextBoxHost.Border = true;
            CustomTextBoxHost.BorderColor = Color.Blue;
            CustomTextBoxHost.BorderSize = 1;
            CustomTextBoxHost.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxHost.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxHost.ForeColor = Color.White;
            CustomTextBoxHost.HideSelection = true;
            CustomTextBoxHost.Location = new Point(12, 130);
            CustomTextBoxHost.MaxLength = 32767;
            CustomTextBoxHost.Multiline = false;
            CustomTextBoxHost.Name = "CustomTextBoxHost";
            CustomTextBoxHost.ReadOnly = false;
            CustomTextBoxHost.ScrollBars = ScrollBars.None;
            CustomTextBoxHost.ShortcutsEnabled = true;
            CustomTextBoxHost.Size = new Size(350, 23);
            CustomTextBoxHost.TabIndex = 0;
            CustomTextBoxHost.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxHost.Texts = "";
            CustomTextBoxHost.UnderlinedStyle = true;
            CustomTextBoxHost.UsePasswordChar = false;
            CustomTextBoxHost.WordWrap = true;
            // 
            // CustomLabelPort
            // 
            CustomLabelPort.AutoSize = true;
            CustomLabelPort.BackColor = Color.DimGray;
            CustomLabelPort.Border = false;
            CustomLabelPort.BorderColor = Color.Blue;
            CustomLabelPort.FlatStyle = FlatStyle.Flat;
            CustomLabelPort.ForeColor = Color.White;
            CustomLabelPort.Location = new Point(12, 170);
            CustomLabelPort.Name = "CustomLabelPort";
            CustomLabelPort.RoundedCorners = 0;
            CustomLabelPort.Size = new Size(81, 17);
            CustomLabelPort.TabIndex = 7;
            CustomLabelPort.Text = "Port Number:";
            // 
            // CustomNumericUpDownPort
            // 
            CustomNumericUpDownPort.BackColor = Color.DimGray;
            CustomNumericUpDownPort.BorderColor = Color.Blue;
            CustomNumericUpDownPort.BorderStyle = BorderStyle.FixedSingle;
            CustomNumericUpDownPort.Location = new Point(99, 168);
            CustomNumericUpDownPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            CustomNumericUpDownPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            CustomNumericUpDownPort.Name = "CustomNumericUpDownPort";
            CustomNumericUpDownPort.Size = new Size(80, 23);
            CustomNumericUpDownPort.TabIndex = 8;
            CustomNumericUpDownPort.Value = new decimal(new int[] { 53, 0, 0, 0 });
            // 
            // CustomLabelPath
            // 
            CustomLabelPath.AutoSize = true;
            CustomLabelPath.BackColor = Color.DimGray;
            CustomLabelPath.Border = false;
            CustomLabelPath.BorderColor = Color.Blue;
            CustomLabelPath.FlatStyle = FlatStyle.Flat;
            CustomLabelPath.ForeColor = Color.White;
            CustomLabelPath.Location = new Point(12, 210);
            CustomLabelPath.Name = "CustomLabelPath";
            CustomLabelPath.RoundedCorners = 0;
            CustomLabelPath.Size = new Size(121, 17);
            CustomLabelPath.TabIndex = 9;
            CustomLabelPath.Text = "Path (Can be empty):";
            // 
            // CustomTextBoxPath
            // 
            CustomTextBoxPath.AcceptsReturn = false;
            CustomTextBoxPath.AcceptsTab = false;
            CustomTextBoxPath.BackColor = Color.DimGray;
            CustomTextBoxPath.Border = true;
            CustomTextBoxPath.BorderColor = Color.Blue;
            CustomTextBoxPath.BorderSize = 1;
            CustomTextBoxPath.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxPath.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxPath.ForeColor = Color.White;
            CustomTextBoxPath.HideSelection = true;
            CustomTextBoxPath.Location = new Point(139, 208);
            CustomTextBoxPath.MaxLength = 32767;
            CustomTextBoxPath.Multiline = false;
            CustomTextBoxPath.Name = "CustomTextBoxPath";
            CustomTextBoxPath.ReadOnly = false;
            CustomTextBoxPath.ScrollBars = ScrollBars.None;
            CustomTextBoxPath.ShortcutsEnabled = true;
            CustomTextBoxPath.Size = new Size(223, 23);
            CustomTextBoxPath.TabIndex = 0;
            CustomTextBoxPath.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxPath.Texts = "";
            CustomTextBoxPath.UnderlinedStyle = true;
            CustomTextBoxPath.UsePasswordChar = false;
            CustomTextBoxPath.WordWrap = true;
            // 
            // CustomLabelProviderName
            // 
            CustomLabelProviderName.AutoSize = true;
            CustomLabelProviderName.BackColor = Color.DimGray;
            CustomLabelProviderName.Border = false;
            CustomLabelProviderName.BorderColor = Color.Blue;
            CustomLabelProviderName.FlatStyle = FlatStyle.Flat;
            CustomLabelProviderName.ForeColor = Color.White;
            CustomLabelProviderName.Location = new Point(407, 50);
            CustomLabelProviderName.Name = "CustomLabelProviderName";
            CustomLabelProviderName.RoundedCorners = 0;
            CustomLabelProviderName.Size = new Size(91, 17);
            CustomLabelProviderName.TabIndex = 12;
            CustomLabelProviderName.Text = "Provider Name:";
            // 
            // CustomTextBoxProviderName
            // 
            CustomTextBoxProviderName.AcceptsReturn = false;
            CustomTextBoxProviderName.AcceptsTab = false;
            CustomTextBoxProviderName.BackColor = Color.DimGray;
            CustomTextBoxProviderName.Border = true;
            CustomTextBoxProviderName.BorderColor = Color.Blue;
            CustomTextBoxProviderName.BorderSize = 1;
            CustomTextBoxProviderName.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxProviderName.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxProviderName.ForeColor = Color.White;
            CustomTextBoxProviderName.HideSelection = true;
            CustomTextBoxProviderName.Location = new Point(407, 70);
            CustomTextBoxProviderName.MaxLength = 32767;
            CustomTextBoxProviderName.Multiline = false;
            CustomTextBoxProviderName.Name = "CustomTextBoxProviderName";
            CustomTextBoxProviderName.ReadOnly = false;
            CustomTextBoxProviderName.ScrollBars = ScrollBars.None;
            CustomTextBoxProviderName.ShortcutsEnabled = true;
            CustomTextBoxProviderName.Size = new Size(350, 23);
            CustomTextBoxProviderName.TabIndex = 0;
            CustomTextBoxProviderName.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxProviderName.Texts = "";
            CustomTextBoxProviderName.UnderlinedStyle = true;
            CustomTextBoxProviderName.UsePasswordChar = false;
            CustomTextBoxProviderName.WordWrap = true;
            // 
            // CustomLabelPublicKey
            // 
            CustomLabelPublicKey.AutoSize = true;
            CustomLabelPublicKey.BackColor = Color.DimGray;
            CustomLabelPublicKey.Border = false;
            CustomLabelPublicKey.BorderColor = Color.Blue;
            CustomLabelPublicKey.FlatStyle = FlatStyle.Flat;
            CustomLabelPublicKey.ForeColor = Color.White;
            CustomLabelPublicKey.Location = new Point(407, 110);
            CustomLabelPublicKey.Name = "CustomLabelPublicKey";
            CustomLabelPublicKey.RoundedCorners = 0;
            CustomLabelPublicKey.Size = new Size(355, 17);
            CustomLabelPublicKey.TabIndex = 15;
            CustomLabelPublicKey.Text = "Public Key (DNSCrypt provider’s Ed25519 public key) (HEX String):";
            // 
            // CustomTextBoxPublicKey
            // 
            CustomTextBoxPublicKey.AcceptsReturn = false;
            CustomTextBoxPublicKey.AcceptsTab = false;
            CustomTextBoxPublicKey.BackColor = Color.DimGray;
            CustomTextBoxPublicKey.Border = true;
            CustomTextBoxPublicKey.BorderColor = Color.Blue;
            CustomTextBoxPublicKey.BorderSize = 1;
            CustomTextBoxPublicKey.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxPublicKey.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxPublicKey.ForeColor = Color.White;
            CustomTextBoxPublicKey.HideSelection = true;
            CustomTextBoxPublicKey.Location = new Point(407, 130);
            CustomTextBoxPublicKey.MaxLength = 32767;
            CustomTextBoxPublicKey.Multiline = false;
            CustomTextBoxPublicKey.Name = "CustomTextBoxPublicKey";
            CustomTextBoxPublicKey.ReadOnly = false;
            CustomTextBoxPublicKey.ScrollBars = ScrollBars.None;
            CustomTextBoxPublicKey.ShortcutsEnabled = true;
            CustomTextBoxPublicKey.Size = new Size(350, 23);
            CustomTextBoxPublicKey.TabIndex = 0;
            CustomTextBoxPublicKey.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxPublicKey.Texts = "";
            CustomTextBoxPublicKey.UnderlinedStyle = true;
            CustomTextBoxPublicKey.UsePasswordChar = false;
            CustomTextBoxPublicKey.WordWrap = true;
            // 
            // CustomLabelHash
            // 
            CustomLabelHash.AutoSize = true;
            CustomLabelHash.BackColor = Color.DimGray;
            CustomLabelHash.Border = false;
            CustomLabelHash.BorderColor = Color.Blue;
            CustomLabelHash.FlatStyle = FlatStyle.Flat;
            CustomLabelHash.ForeColor = Color.White;
            CustomLabelHash.Location = new Point(407, 170);
            CustomLabelHash.Name = "CustomLabelHash";
            CustomLabelHash.RoundedCorners = 0;
            CustomLabelHash.Size = new Size(356, 17);
            CustomLabelHash.TabIndex = 17;
            CustomLabelHash.Text = "Hashes (Comma-separated) (SHA256 HEX String) (Can be empty):";
            // 
            // CustomTextBoxHash
            // 
            CustomTextBoxHash.AcceptsReturn = false;
            CustomTextBoxHash.AcceptsTab = false;
            CustomTextBoxHash.BackColor = Color.DimGray;
            CustomTextBoxHash.Border = true;
            CustomTextBoxHash.BorderColor = Color.Blue;
            CustomTextBoxHash.BorderSize = 1;
            CustomTextBoxHash.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxHash.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxHash.ForeColor = Color.White;
            CustomTextBoxHash.HideSelection = true;
            CustomTextBoxHash.Location = new Point(407, 190);
            CustomTextBoxHash.MaxLength = 32767;
            CustomTextBoxHash.Multiline = false;
            CustomTextBoxHash.Name = "CustomTextBoxHash";
            CustomTextBoxHash.ReadOnly = false;
            CustomTextBoxHash.ScrollBars = ScrollBars.None;
            CustomTextBoxHash.ShortcutsEnabled = true;
            CustomTextBoxHash.Size = new Size(350, 23);
            CustomTextBoxHash.TabIndex = 0;
            CustomTextBoxHash.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxHash.Texts = "";
            CustomTextBoxHash.UnderlinedStyle = true;
            CustomTextBoxHash.UsePasswordChar = false;
            CustomTextBoxHash.WordWrap = true;
            // 
            // CustomCheckBoxIsDnsSec
            // 
            CustomCheckBoxIsDnsSec.BackColor = Color.DimGray;
            CustomCheckBoxIsDnsSec.BorderColor = Color.Blue;
            CustomCheckBoxIsDnsSec.CheckColor = Color.Blue;
            CustomCheckBoxIsDnsSec.ForeColor = Color.White;
            CustomCheckBoxIsDnsSec.Location = new Point(12, 250);
            CustomCheckBoxIsDnsSec.Name = "CustomCheckBoxIsDnsSec";
            CustomCheckBoxIsDnsSec.SelectionColor = Color.LightBlue;
            CustomCheckBoxIsDnsSec.Size = new Size(76, 17);
            CustomCheckBoxIsDnsSec.TabIndex = 20;
            CustomCheckBoxIsDnsSec.Text = "Is DNSSec";
            CustomCheckBoxIsDnsSec.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxIsNoFilter
            // 
            CustomCheckBoxIsNoFilter.BackColor = Color.DimGray;
            CustomCheckBoxIsNoFilter.BorderColor = Color.Blue;
            CustomCheckBoxIsNoFilter.CheckColor = Color.Blue;
            CustomCheckBoxIsNoFilter.ForeColor = Color.White;
            CustomCheckBoxIsNoFilter.Location = new Point(122, 250);
            CustomCheckBoxIsNoFilter.Name = "CustomCheckBoxIsNoFilter";
            CustomCheckBoxIsNoFilter.SelectionColor = Color.LightBlue;
            CustomCheckBoxIsNoFilter.Size = new Size(79, 17);
            CustomCheckBoxIsNoFilter.TabIndex = 21;
            CustomCheckBoxIsNoFilter.Text = "Is No Filter";
            CustomCheckBoxIsNoFilter.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxIsNoLog
            // 
            CustomCheckBoxIsNoLog.BackColor = Color.DimGray;
            CustomCheckBoxIsNoLog.BorderColor = Color.Blue;
            CustomCheckBoxIsNoLog.CheckColor = Color.Blue;
            CustomCheckBoxIsNoLog.ForeColor = Color.White;
            CustomCheckBoxIsNoLog.Location = new Point(232, 250);
            CustomCheckBoxIsNoLog.Name = "CustomCheckBoxIsNoLog";
            CustomCheckBoxIsNoLog.SelectionColor = Color.LightBlue;
            CustomCheckBoxIsNoLog.Size = new Size(73, 17);
            CustomCheckBoxIsNoLog.TabIndex = 22;
            CustomCheckBoxIsNoLog.Text = "Is No Log";
            CustomCheckBoxIsNoLog.UseVisualStyleBackColor = false;
            // 
            // CustomTextBoxStamp
            // 
            CustomTextBoxStamp.AcceptsReturn = false;
            CustomTextBoxStamp.AcceptsTab = false;
            CustomTextBoxStamp.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CustomTextBoxStamp.BackColor = Color.DimGray;
            CustomTextBoxStamp.Border = true;
            CustomTextBoxStamp.BorderColor = Color.Blue;
            CustomTextBoxStamp.BorderSize = 1;
            CustomTextBoxStamp.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxStamp.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxStamp.ForeColor = Color.White;
            CustomTextBoxStamp.HideSelection = true;
            CustomTextBoxStamp.Location = new Point(12, 280);
            CustomTextBoxStamp.MaxLength = 32767;
            CustomTextBoxStamp.MinimumSize = new Size(0, 23);
            CustomTextBoxStamp.Multiline = true;
            CustomTextBoxStamp.Name = "CustomTextBoxStamp";
            CustomTextBoxStamp.ReadOnly = false;
            CustomTextBoxStamp.ScrollBars = ScrollBars.Vertical;
            CustomTextBoxStamp.ShortcutsEnabled = true;
            CustomTextBoxStamp.Size = new Size(602, 102);
            CustomTextBoxStamp.TabIndex = 0;
            CustomTextBoxStamp.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxStamp.Texts = "sdns://";
            CustomTextBoxStamp.UnderlinedStyle = false;
            CustomTextBoxStamp.UsePasswordChar = false;
            CustomTextBoxStamp.WordWrap = true;
            // 
            // CustomButtonClear
            // 
            CustomButtonClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CustomButtonClear.AutoSize = true;
            CustomButtonClear.BorderColor = Color.Blue;
            CustomButtonClear.FlatStyle = FlatStyle.Flat;
            CustomButtonClear.Location = new Point(655, 280);
            CustomButtonClear.Name = "CustomButtonClear";
            CustomButtonClear.RoundedCorners = 5;
            CustomButtonClear.SelectionColor = Color.LightBlue;
            CustomButtonClear.Size = new Size(75, 27);
            CustomButtonClear.TabIndex = 24;
            CustomButtonClear.Text = "Clear";
            CustomButtonClear.UseVisualStyleBackColor = true;
            CustomButtonClear.Click += CustomButtonClear_Click;
            // 
            // CustomButtonDecode
            // 
            CustomButtonDecode.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CustomButtonDecode.AutoSize = true;
            CustomButtonDecode.BorderColor = Color.Blue;
            CustomButtonDecode.FlatStyle = FlatStyle.Flat;
            CustomButtonDecode.Location = new Point(655, 317);
            CustomButtonDecode.Name = "CustomButtonDecode";
            CustomButtonDecode.RoundedCorners = 5;
            CustomButtonDecode.SelectionColor = Color.LightBlue;
            CustomButtonDecode.Size = new Size(75, 27);
            CustomButtonDecode.TabIndex = 33;
            CustomButtonDecode.Text = "Decode";
            CustomButtonDecode.UseVisualStyleBackColor = true;
            CustomButtonDecode.Click += CustomButtonDecode_Click;
            // 
            // CustomLabelStatus
            // 
            CustomLabelStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CustomLabelStatus.AutoSize = true;
            CustomLabelStatus.BackColor = Color.DimGray;
            CustomLabelStatus.Border = false;
            CustomLabelStatus.BorderColor = Color.Blue;
            CustomLabelStatus.FlatStyle = FlatStyle.Flat;
            CustomLabelStatus.ForeColor = Color.White;
            CustomLabelStatus.Location = new Point(12, 388);
            CustomLabelStatus.Name = "CustomLabelStatus";
            CustomLabelStatus.RoundedCorners = 0;
            CustomLabelStatus.Size = new Size(79, 17);
            CustomLabelStatus.TabIndex = 34;
            CustomLabelStatus.Text = "Status: Ready";
            // 
            // CustomButtonEncode
            // 
            CustomButtonEncode.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CustomButtonEncode.AutoSize = true;
            CustomButtonEncode.BorderColor = Color.Blue;
            CustomButtonEncode.FlatStyle = FlatStyle.Flat;
            CustomButtonEncode.Location = new Point(655, 355);
            CustomButtonEncode.Name = "CustomButtonEncode";
            CustomButtonEncode.RoundedCorners = 5;
            CustomButtonEncode.SelectionColor = Color.LightBlue;
            CustomButtonEncode.Size = new Size(75, 27);
            CustomButtonEncode.TabIndex = 35;
            CustomButtonEncode.Text = "Encode";
            CustomButtonEncode.UseVisualStyleBackColor = true;
            CustomButtonEncode.Click += CustomButtonEncode_Click;
            // 
            // FormStampGenerator
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.DimGray;
            ClientSize = new Size(769, 411);
            Controls.Add(CustomButtonEncode);
            Controls.Add(CustomLabelStatus);
            Controls.Add(CustomButtonDecode);
            Controls.Add(CustomButtonClear);
            Controls.Add(CustomTextBoxStamp);
            Controls.Add(CustomCheckBoxIsNoLog);
            Controls.Add(CustomCheckBoxIsNoFilter);
            Controls.Add(CustomCheckBoxIsDnsSec);
            Controls.Add(CustomTextBoxHash);
            Controls.Add(CustomLabelHash);
            Controls.Add(CustomTextBoxPublicKey);
            Controls.Add(CustomLabelPublicKey);
            Controls.Add(CustomTextBoxProviderName);
            Controls.Add(CustomLabelProviderName);
            Controls.Add(CustomTextBoxPath);
            Controls.Add(CustomLabelPath);
            Controls.Add(CustomNumericUpDownPort);
            Controls.Add(CustomLabelPort);
            Controls.Add(CustomTextBoxHost);
            Controls.Add(CustomLabelHost);
            Controls.Add(CustomTextBoxIP);
            Controls.Add(CustomLabelIP);
            Controls.Add(CustomComboBoxProtocol);
            Controls.Add(CustomLabelProtocol);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MaximumSize = new Size(785, 450);
            MinimumSize = new Size(785, 450);
            Name = "FormStampGenerator";
            Text = "DNSCrypt Stamp Generator";
            FormClosing += FormStampGenerator_FormClosing;
            ((System.ComponentModel.ISupportInitialize)CustomNumericUpDownPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CustomControls.CustomLabel CustomLabelProtocol;
        private CustomControls.CustomComboBox CustomComboBoxProtocol;
        private CustomControls.CustomLabel CustomLabelIP;
        private CustomControls.CustomTextBox CustomTextBoxIP;
        private CustomControls.CustomLabel CustomLabelHost;
        private CustomControls.CustomTextBox CustomTextBoxHost;
        private CustomControls.CustomLabel CustomLabelPort;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownPort;
        private CustomControls.CustomLabel CustomLabelPath;
        private CustomControls.CustomTextBox CustomTextBoxPath;
        private CustomControls.CustomLabel CustomLabelProviderName;
        private CustomControls.CustomTextBox CustomTextBoxProviderName;
        private CustomControls.CustomLabel CustomLabelPublicKey;
        private CustomControls.CustomTextBox CustomTextBoxPublicKey;
        private CustomControls.CustomLabel CustomLabelHash;
        private CustomControls.CustomTextBox CustomTextBoxHash;
        private CustomControls.CustomCheckBox CustomCheckBoxIsDnsSec;
        private CustomControls.CustomCheckBox CustomCheckBoxIsNoFilter;
        private CustomControls.CustomCheckBox CustomCheckBoxIsNoLog;
        private CustomControls.CustomTextBox CustomTextBoxStamp;
        private CustomControls.CustomButton CustomButtonClear;
        private CustomControls.CustomButton CustomButtonDecode;
        private CustomControls.CustomLabel CustomLabelStatus;
        private CustomControls.CustomButton CustomButtonEncode;
    }
}