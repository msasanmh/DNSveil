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
            this.CustomLabelProtocol = new CustomControls.CustomLabel();
            this.CustomComboBoxProtocol = new CustomControls.CustomComboBox();
            this.CustomLabelIP = new CustomControls.CustomLabel();
            this.CustomTextBoxIP = new CustomControls.CustomTextBox();
            this.CustomLabelHost = new CustomControls.CustomLabel();
            this.CustomTextBoxHost = new CustomControls.CustomTextBox();
            this.CustomLabelPort = new CustomControls.CustomLabel();
            this.CustomNumericUpDownPort = new CustomControls.CustomNumericUpDown();
            this.CustomLabelPath = new CustomControls.CustomLabel();
            this.CustomTextBoxPath = new CustomControls.CustomTextBox();
            this.CustomLabelProviderName = new CustomControls.CustomLabel();
            this.CustomTextBoxProviderName = new CustomControls.CustomTextBox();
            this.CustomLabelPublicKey = new CustomControls.CustomLabel();
            this.CustomTextBoxPublicKey = new CustomControls.CustomTextBox();
            this.CustomLabelHash = new CustomControls.CustomLabel();
            this.CustomTextBoxHash = new CustomControls.CustomTextBox();
            this.CustomCheckBoxIsDnsSec = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxIsNoFilter = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxIsNoLog = new CustomControls.CustomCheckBox();
            this.CustomTextBoxStamp = new CustomControls.CustomTextBox();
            this.CustomButtonClear = new CustomControls.CustomButton();
            this.CustomButtonDecode = new CustomControls.CustomButton();
            this.CustomLabelStatus = new CustomControls.CustomLabel();
            this.CustomButtonEncode = new CustomControls.CustomButton();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPort)).BeginInit();
            this.SuspendLayout();
            // 
            // CustomLabelProtocol
            // 
            this.CustomLabelProtocol.AutoSize = true;
            this.CustomLabelProtocol.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelProtocol.Border = false;
            this.CustomLabelProtocol.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelProtocol.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelProtocol.ForeColor = System.Drawing.Color.White;
            this.CustomLabelProtocol.Location = new System.Drawing.Point(12, 10);
            this.CustomLabelProtocol.Name = "CustomLabelProtocol";
            this.CustomLabelProtocol.RoundedCorners = 0;
            this.CustomLabelProtocol.Size = new System.Drawing.Size(57, 17);
            this.CustomLabelProtocol.TabIndex = 0;
            this.CustomLabelProtocol.Text = "Protocol:";
            // 
            // CustomComboBoxProtocol
            // 
            this.CustomComboBoxProtocol.BackColor = System.Drawing.Color.DimGray;
            this.CustomComboBoxProtocol.BorderColor = System.Drawing.Color.Blue;
            this.CustomComboBoxProtocol.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CustomComboBoxProtocol.ForeColor = System.Drawing.Color.White;
            this.CustomComboBoxProtocol.FormattingEnabled = true;
            this.CustomComboBoxProtocol.ItemHeight = 17;
            this.CustomComboBoxProtocol.Items.AddRange(new object[] {
            "Plain DNS",
            "DNSCrypt",
            "DNS-Over-HTTPS",
            "DNS-Over-TLS",
            "DNS-Over-Quic",
            "Oblivious DoH Target",
            "Anonymized DNSCrypt Relay",
            "Oblivious DoH Relay"});
            this.CustomComboBoxProtocol.Location = new System.Drawing.Point(75, 8);
            this.CustomComboBoxProtocol.Name = "CustomComboBoxProtocol";
            this.CustomComboBoxProtocol.SelectionColor = System.Drawing.Color.DodgerBlue;
            this.CustomComboBoxProtocol.Size = new System.Drawing.Size(287, 23);
            this.CustomComboBoxProtocol.TabIndex = 1;
            this.CustomComboBoxProtocol.SelectedIndexChanged += new System.EventHandler(this.CustomComboBoxProtocol_SelectedIndexChanged);
            // 
            // CustomLabelIP
            // 
            this.CustomLabelIP.AutoSize = true;
            this.CustomLabelIP.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelIP.Border = false;
            this.CustomLabelIP.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelIP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelIP.ForeColor = System.Drawing.Color.White;
            this.CustomLabelIP.Location = new System.Drawing.Point(12, 50);
            this.CustomLabelIP.Name = "CustomLabelIP";
            this.CustomLabelIP.RoundedCorners = 0;
            this.CustomLabelIP.Size = new System.Drawing.Size(274, 17);
            this.CustomLabelIP.TabIndex = 2;
            this.CustomLabelIP.Text = "IP Address (IPv6 addresses must be in [ ] brackets):";
            // 
            // CustomTextBoxIP
            // 
            this.CustomTextBoxIP.AcceptsReturn = false;
            this.CustomTextBoxIP.AcceptsTab = false;
            this.CustomTextBoxIP.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxIP.Border = true;
            this.CustomTextBoxIP.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxIP.BorderSize = 1;
            this.CustomTextBoxIP.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxIP.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxIP.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxIP.HideSelection = true;
            this.CustomTextBoxIP.Location = new System.Drawing.Point(12, 70);
            this.CustomTextBoxIP.MaxLength = 32767;
            this.CustomTextBoxIP.Multiline = false;
            this.CustomTextBoxIP.Name = "CustomTextBoxIP";
            this.CustomTextBoxIP.ReadOnly = false;
            this.CustomTextBoxIP.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxIP.ShortcutsEnabled = true;
            this.CustomTextBoxIP.Size = new System.Drawing.Size(350, 23);
            this.CustomTextBoxIP.TabIndex = 0;
            this.CustomTextBoxIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxIP.Texts = "";
            this.CustomTextBoxIP.UnderlinedStyle = true;
            this.CustomTextBoxIP.UsePasswordChar = false;
            this.CustomTextBoxIP.WordWrap = true;
            // 
            // CustomLabelHost
            // 
            this.CustomLabelHost.AutoSize = true;
            this.CustomLabelHost.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelHost.Border = false;
            this.CustomLabelHost.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelHost.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelHost.ForeColor = System.Drawing.Color.White;
            this.CustomLabelHost.Location = new System.Drawing.Point(12, 110);
            this.CustomLabelHost.Name = "CustomLabelHost";
            this.CustomLabelHost.RoundedCorners = 0;
            this.CustomLabelHost.Size = new System.Drawing.Size(140, 17);
            this.CustomLabelHost.TabIndex = 4;
            this.CustomLabelHost.Text = "Host Name (vHost+SNI):";
            // 
            // CustomTextBoxHost
            // 
            this.CustomTextBoxHost.AcceptsReturn = false;
            this.CustomTextBoxHost.AcceptsTab = false;
            this.CustomTextBoxHost.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxHost.Border = true;
            this.CustomTextBoxHost.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxHost.BorderSize = 1;
            this.CustomTextBoxHost.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxHost.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxHost.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxHost.HideSelection = true;
            this.CustomTextBoxHost.Location = new System.Drawing.Point(12, 130);
            this.CustomTextBoxHost.MaxLength = 32767;
            this.CustomTextBoxHost.Multiline = false;
            this.CustomTextBoxHost.Name = "CustomTextBoxHost";
            this.CustomTextBoxHost.ReadOnly = false;
            this.CustomTextBoxHost.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxHost.ShortcutsEnabled = true;
            this.CustomTextBoxHost.Size = new System.Drawing.Size(350, 23);
            this.CustomTextBoxHost.TabIndex = 0;
            this.CustomTextBoxHost.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxHost.Texts = "";
            this.CustomTextBoxHost.UnderlinedStyle = true;
            this.CustomTextBoxHost.UsePasswordChar = false;
            this.CustomTextBoxHost.WordWrap = true;
            // 
            // CustomLabelPort
            // 
            this.CustomLabelPort.AutoSize = true;
            this.CustomLabelPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelPort.Border = false;
            this.CustomLabelPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelPort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelPort.ForeColor = System.Drawing.Color.White;
            this.CustomLabelPort.Location = new System.Drawing.Point(12, 170);
            this.CustomLabelPort.Name = "CustomLabelPort";
            this.CustomLabelPort.RoundedCorners = 0;
            this.CustomLabelPort.Size = new System.Drawing.Size(81, 17);
            this.CustomLabelPort.TabIndex = 7;
            this.CustomLabelPort.Text = "Port Number:";
            // 
            // CustomNumericUpDownPort
            // 
            this.CustomNumericUpDownPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownPort.Location = new System.Drawing.Point(99, 168);
            this.CustomNumericUpDownPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.CustomNumericUpDownPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownPort.Name = "CustomNumericUpDownPort";
            this.CustomNumericUpDownPort.Size = new System.Drawing.Size(80, 23);
            this.CustomNumericUpDownPort.TabIndex = 8;
            this.CustomNumericUpDownPort.Value = new decimal(new int[] {
            53,
            0,
            0,
            0});
            // 
            // CustomLabelPath
            // 
            this.CustomLabelPath.AutoSize = true;
            this.CustomLabelPath.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelPath.Border = false;
            this.CustomLabelPath.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelPath.ForeColor = System.Drawing.Color.White;
            this.CustomLabelPath.Location = new System.Drawing.Point(12, 210);
            this.CustomLabelPath.Name = "CustomLabelPath";
            this.CustomLabelPath.RoundedCorners = 0;
            this.CustomLabelPath.Size = new System.Drawing.Size(121, 17);
            this.CustomLabelPath.TabIndex = 9;
            this.CustomLabelPath.Text = "Path (Can be empty):";
            // 
            // CustomTextBoxPath
            // 
            this.CustomTextBoxPath.AcceptsReturn = false;
            this.CustomTextBoxPath.AcceptsTab = false;
            this.CustomTextBoxPath.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxPath.Border = true;
            this.CustomTextBoxPath.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxPath.BorderSize = 1;
            this.CustomTextBoxPath.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxPath.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxPath.HideSelection = true;
            this.CustomTextBoxPath.Location = new System.Drawing.Point(139, 208);
            this.CustomTextBoxPath.MaxLength = 32767;
            this.CustomTextBoxPath.Multiline = false;
            this.CustomTextBoxPath.Name = "CustomTextBoxPath";
            this.CustomTextBoxPath.ReadOnly = false;
            this.CustomTextBoxPath.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxPath.ShortcutsEnabled = true;
            this.CustomTextBoxPath.Size = new System.Drawing.Size(223, 23);
            this.CustomTextBoxPath.TabIndex = 0;
            this.CustomTextBoxPath.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxPath.Texts = "";
            this.CustomTextBoxPath.UnderlinedStyle = true;
            this.CustomTextBoxPath.UsePasswordChar = false;
            this.CustomTextBoxPath.WordWrap = true;
            // 
            // CustomLabelProviderName
            // 
            this.CustomLabelProviderName.AutoSize = true;
            this.CustomLabelProviderName.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelProviderName.Border = false;
            this.CustomLabelProviderName.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelProviderName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelProviderName.ForeColor = System.Drawing.Color.White;
            this.CustomLabelProviderName.Location = new System.Drawing.Point(407, 50);
            this.CustomLabelProviderName.Name = "CustomLabelProviderName";
            this.CustomLabelProviderName.RoundedCorners = 0;
            this.CustomLabelProviderName.Size = new System.Drawing.Size(91, 17);
            this.CustomLabelProviderName.TabIndex = 12;
            this.CustomLabelProviderName.Text = "Provider Name:";
            // 
            // CustomTextBoxProviderName
            // 
            this.CustomTextBoxProviderName.AcceptsReturn = false;
            this.CustomTextBoxProviderName.AcceptsTab = false;
            this.CustomTextBoxProviderName.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxProviderName.Border = true;
            this.CustomTextBoxProviderName.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxProviderName.BorderSize = 1;
            this.CustomTextBoxProviderName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxProviderName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxProviderName.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxProviderName.HideSelection = true;
            this.CustomTextBoxProviderName.Location = new System.Drawing.Point(407, 70);
            this.CustomTextBoxProviderName.MaxLength = 32767;
            this.CustomTextBoxProviderName.Multiline = false;
            this.CustomTextBoxProviderName.Name = "CustomTextBoxProviderName";
            this.CustomTextBoxProviderName.ReadOnly = false;
            this.CustomTextBoxProviderName.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxProviderName.ShortcutsEnabled = true;
            this.CustomTextBoxProviderName.Size = new System.Drawing.Size(350, 23);
            this.CustomTextBoxProviderName.TabIndex = 0;
            this.CustomTextBoxProviderName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxProviderName.Texts = "";
            this.CustomTextBoxProviderName.UnderlinedStyle = true;
            this.CustomTextBoxProviderName.UsePasswordChar = false;
            this.CustomTextBoxProviderName.WordWrap = true;
            // 
            // CustomLabelPublicKey
            // 
            this.CustomLabelPublicKey.AutoSize = true;
            this.CustomLabelPublicKey.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelPublicKey.Border = false;
            this.CustomLabelPublicKey.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelPublicKey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelPublicKey.ForeColor = System.Drawing.Color.White;
            this.CustomLabelPublicKey.Location = new System.Drawing.Point(407, 110);
            this.CustomLabelPublicKey.Name = "CustomLabelPublicKey";
            this.CustomLabelPublicKey.RoundedCorners = 0;
            this.CustomLabelPublicKey.Size = new System.Drawing.Size(355, 17);
            this.CustomLabelPublicKey.TabIndex = 15;
            this.CustomLabelPublicKey.Text = "Public Key (DNSCrypt provider’s Ed25519 public key) (HEX String):";
            // 
            // CustomTextBoxPublicKey
            // 
            this.CustomTextBoxPublicKey.AcceptsReturn = false;
            this.CustomTextBoxPublicKey.AcceptsTab = false;
            this.CustomTextBoxPublicKey.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxPublicKey.Border = true;
            this.CustomTextBoxPublicKey.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxPublicKey.BorderSize = 1;
            this.CustomTextBoxPublicKey.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxPublicKey.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxPublicKey.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxPublicKey.HideSelection = true;
            this.CustomTextBoxPublicKey.Location = new System.Drawing.Point(407, 130);
            this.CustomTextBoxPublicKey.MaxLength = 32767;
            this.CustomTextBoxPublicKey.Multiline = false;
            this.CustomTextBoxPublicKey.Name = "CustomTextBoxPublicKey";
            this.CustomTextBoxPublicKey.ReadOnly = false;
            this.CustomTextBoxPublicKey.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxPublicKey.ShortcutsEnabled = true;
            this.CustomTextBoxPublicKey.Size = new System.Drawing.Size(350, 23);
            this.CustomTextBoxPublicKey.TabIndex = 0;
            this.CustomTextBoxPublicKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxPublicKey.Texts = "";
            this.CustomTextBoxPublicKey.UnderlinedStyle = true;
            this.CustomTextBoxPublicKey.UsePasswordChar = false;
            this.CustomTextBoxPublicKey.WordWrap = true;
            // 
            // CustomLabelHash
            // 
            this.CustomLabelHash.AutoSize = true;
            this.CustomLabelHash.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelHash.Border = false;
            this.CustomLabelHash.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelHash.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelHash.ForeColor = System.Drawing.Color.White;
            this.CustomLabelHash.Location = new System.Drawing.Point(407, 170);
            this.CustomLabelHash.Name = "CustomLabelHash";
            this.CustomLabelHash.RoundedCorners = 0;
            this.CustomLabelHash.Size = new System.Drawing.Size(356, 17);
            this.CustomLabelHash.TabIndex = 17;
            this.CustomLabelHash.Text = "Hashes (Comma-separated) (SHA256 HEX String) (Can be empty):";
            // 
            // CustomTextBoxHash
            // 
            this.CustomTextBoxHash.AcceptsReturn = false;
            this.CustomTextBoxHash.AcceptsTab = false;
            this.CustomTextBoxHash.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxHash.Border = true;
            this.CustomTextBoxHash.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxHash.BorderSize = 1;
            this.CustomTextBoxHash.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxHash.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxHash.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxHash.HideSelection = true;
            this.CustomTextBoxHash.Location = new System.Drawing.Point(407, 190);
            this.CustomTextBoxHash.MaxLength = 32767;
            this.CustomTextBoxHash.Multiline = false;
            this.CustomTextBoxHash.Name = "CustomTextBoxHash";
            this.CustomTextBoxHash.ReadOnly = false;
            this.CustomTextBoxHash.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxHash.ShortcutsEnabled = true;
            this.CustomTextBoxHash.Size = new System.Drawing.Size(350, 23);
            this.CustomTextBoxHash.TabIndex = 0;
            this.CustomTextBoxHash.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxHash.Texts = "";
            this.CustomTextBoxHash.UnderlinedStyle = true;
            this.CustomTextBoxHash.UsePasswordChar = false;
            this.CustomTextBoxHash.WordWrap = true;
            // 
            // CustomCheckBoxIsDnsSec
            // 
            this.CustomCheckBoxIsDnsSec.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxIsDnsSec.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxIsDnsSec.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxIsDnsSec.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxIsDnsSec.Location = new System.Drawing.Point(12, 250);
            this.CustomCheckBoxIsDnsSec.Name = "CustomCheckBoxIsDnsSec";
            this.CustomCheckBoxIsDnsSec.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxIsDnsSec.Size = new System.Drawing.Size(76, 17);
            this.CustomCheckBoxIsDnsSec.TabIndex = 20;
            this.CustomCheckBoxIsDnsSec.Text = "Is DNSSec";
            this.CustomCheckBoxIsDnsSec.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxIsNoFilter
            // 
            this.CustomCheckBoxIsNoFilter.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxIsNoFilter.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxIsNoFilter.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxIsNoFilter.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxIsNoFilter.Location = new System.Drawing.Point(122, 250);
            this.CustomCheckBoxIsNoFilter.Name = "CustomCheckBoxIsNoFilter";
            this.CustomCheckBoxIsNoFilter.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxIsNoFilter.Size = new System.Drawing.Size(79, 17);
            this.CustomCheckBoxIsNoFilter.TabIndex = 21;
            this.CustomCheckBoxIsNoFilter.Text = "Is No Filter";
            this.CustomCheckBoxIsNoFilter.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxIsNoLog
            // 
            this.CustomCheckBoxIsNoLog.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxIsNoLog.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxIsNoLog.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxIsNoLog.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxIsNoLog.Location = new System.Drawing.Point(232, 250);
            this.CustomCheckBoxIsNoLog.Name = "CustomCheckBoxIsNoLog";
            this.CustomCheckBoxIsNoLog.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxIsNoLog.Size = new System.Drawing.Size(73, 17);
            this.CustomCheckBoxIsNoLog.TabIndex = 22;
            this.CustomCheckBoxIsNoLog.Text = "Is No Log";
            this.CustomCheckBoxIsNoLog.UseVisualStyleBackColor = false;
            // 
            // CustomTextBoxStamp
            // 
            this.CustomTextBoxStamp.AcceptsReturn = false;
            this.CustomTextBoxStamp.AcceptsTab = false;
            this.CustomTextBoxStamp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomTextBoxStamp.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxStamp.Border = true;
            this.CustomTextBoxStamp.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxStamp.BorderSize = 1;
            this.CustomTextBoxStamp.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxStamp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxStamp.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxStamp.HideSelection = true;
            this.CustomTextBoxStamp.Location = new System.Drawing.Point(12, 280);
            this.CustomTextBoxStamp.MaxLength = 32767;
            this.CustomTextBoxStamp.MinimumSize = new System.Drawing.Size(0, 23);
            this.CustomTextBoxStamp.Multiline = true;
            this.CustomTextBoxStamp.Name = "CustomTextBoxStamp";
            this.CustomTextBoxStamp.ReadOnly = false;
            this.CustomTextBoxStamp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CustomTextBoxStamp.ShortcutsEnabled = true;
            this.CustomTextBoxStamp.Size = new System.Drawing.Size(602, 102);
            this.CustomTextBoxStamp.TabIndex = 0;
            this.CustomTextBoxStamp.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxStamp.Texts = "sdns://";
            this.CustomTextBoxStamp.UnderlinedStyle = false;
            this.CustomTextBoxStamp.UsePasswordChar = false;
            this.CustomTextBoxStamp.WordWrap = true;
            // 
            // CustomButtonClear
            // 
            this.CustomButtonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomButtonClear.AutoSize = true;
            this.CustomButtonClear.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonClear.Location = new System.Drawing.Point(655, 280);
            this.CustomButtonClear.Name = "CustomButtonClear";
            this.CustomButtonClear.RoundedCorners = 5;
            this.CustomButtonClear.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonClear.Size = new System.Drawing.Size(75, 27);
            this.CustomButtonClear.TabIndex = 24;
            this.CustomButtonClear.Text = "Clear";
            this.CustomButtonClear.UseVisualStyleBackColor = true;
            this.CustomButtonClear.Click += new System.EventHandler(this.CustomButtonClear_Click);
            // 
            // CustomButtonDecode
            // 
            this.CustomButtonDecode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomButtonDecode.AutoSize = true;
            this.CustomButtonDecode.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonDecode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonDecode.Location = new System.Drawing.Point(655, 317);
            this.CustomButtonDecode.Name = "CustomButtonDecode";
            this.CustomButtonDecode.RoundedCorners = 5;
            this.CustomButtonDecode.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonDecode.Size = new System.Drawing.Size(75, 27);
            this.CustomButtonDecode.TabIndex = 33;
            this.CustomButtonDecode.Text = "Decode";
            this.CustomButtonDecode.UseVisualStyleBackColor = true;
            this.CustomButtonDecode.Click += new System.EventHandler(this.CustomButtonDecode_Click);
            // 
            // CustomLabelStatus
            // 
            this.CustomLabelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomLabelStatus.AutoSize = true;
            this.CustomLabelStatus.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelStatus.Border = false;
            this.CustomLabelStatus.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelStatus.ForeColor = System.Drawing.Color.White;
            this.CustomLabelStatus.Location = new System.Drawing.Point(12, 385);
            this.CustomLabelStatus.Name = "CustomLabelStatus";
            this.CustomLabelStatus.RoundedCorners = 0;
            this.CustomLabelStatus.Size = new System.Drawing.Size(79, 17);
            this.CustomLabelStatus.TabIndex = 34;
            this.CustomLabelStatus.Text = "Status: Ready";
            // 
            // CustomButtonEncode
            // 
            this.CustomButtonEncode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomButtonEncode.AutoSize = true;
            this.CustomButtonEncode.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonEncode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonEncode.Location = new System.Drawing.Point(655, 355);
            this.CustomButtonEncode.Name = "CustomButtonEncode";
            this.CustomButtonEncode.RoundedCorners = 5;
            this.CustomButtonEncode.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonEncode.Size = new System.Drawing.Size(75, 27);
            this.CustomButtonEncode.TabIndex = 35;
            this.CustomButtonEncode.Text = "Encode";
            this.CustomButtonEncode.UseVisualStyleBackColor = true;
            this.CustomButtonEncode.Click += new System.EventHandler(this.CustomButtonEncode_Click);
            // 
            // FormStampGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(769, 411);
            this.Controls.Add(this.CustomButtonEncode);
            this.Controls.Add(this.CustomLabelStatus);
            this.Controls.Add(this.CustomButtonDecode);
            this.Controls.Add(this.CustomButtonClear);
            this.Controls.Add(this.CustomTextBoxStamp);
            this.Controls.Add(this.CustomCheckBoxIsNoLog);
            this.Controls.Add(this.CustomCheckBoxIsNoFilter);
            this.Controls.Add(this.CustomCheckBoxIsDnsSec);
            this.Controls.Add(this.CustomTextBoxHash);
            this.Controls.Add(this.CustomLabelHash);
            this.Controls.Add(this.CustomTextBoxPublicKey);
            this.Controls.Add(this.CustomLabelPublicKey);
            this.Controls.Add(this.CustomTextBoxProviderName);
            this.Controls.Add(this.CustomLabelProviderName);
            this.Controls.Add(this.CustomTextBoxPath);
            this.Controls.Add(this.CustomLabelPath);
            this.Controls.Add(this.CustomNumericUpDownPort);
            this.Controls.Add(this.CustomLabelPort);
            this.Controls.Add(this.CustomTextBoxHost);
            this.Controls.Add(this.CustomLabelHost);
            this.Controls.Add(this.CustomTextBoxIP);
            this.Controls.Add(this.CustomLabelIP);
            this.Controls.Add(this.CustomComboBoxProtocol);
            this.Controls.Add(this.CustomLabelProtocol);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(785, 450);
            this.MinimumSize = new System.Drawing.Size(785, 450);
            this.Name = "FormStampGenerator";
            this.Text = "DNSCrypt Stamp Generator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormStampGenerator_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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