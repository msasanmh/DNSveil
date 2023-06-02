namespace SecureDNSClient
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.CustomRichTextBoxLog = new CustomControls.CustomRichTextBox();
            this.CustomButtonCheck = new CustomControls.CustomButton();
            this.CustomGroupBoxLog = new CustomControls.CustomGroupBox();
            this.CustomCheckBoxInsecure = new CustomControls.CustomCheckBox();
            this.CustomLabelCustomServersInfo = new CustomControls.CustomLabel();
            this.CustomButtonEditCustomServers = new CustomControls.CustomButton();
            this.CustomRadioButtonCustom = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonBuiltIn = new CustomControls.CustomRadioButton();
            this.CustomLabelSSLFragmentSize = new CustomControls.CustomLabel();
            this.CustomNumericUpDownSSLFragmentSize = new CustomControls.CustomNumericUpDown();
            this.CustomTextBoxHTTPProxy = new CustomControls.CustomTextBox();
            this.CustomRadioButtonDPIModeExtreme = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonDPIModeHigh = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonDPIModeMedium = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonDPIModeLight = new CustomControls.CustomRadioButton();
            this.CustomLabelDPIModes = new CustomControls.CustomLabel();
            this.CustomButtonConnect = new CustomControls.CustomButton();
            this.CustomTabControlMain = new CustomControls.CustomTabControl();
            this.TabPageSecureDNS = new System.Windows.Forms.TabPage();
            this.CustomTabControlSecureDNS = new CustomControls.CustomTabControl();
            this.TabPageCheck = new System.Windows.Forms.TabPage();
            this.CustomLabelCheckPercent = new CustomControls.CustomLabel();
            this.CustomButtonConnectAll = new CustomControls.CustomButton();
            this.CustomButtonViewWorkingServers = new CustomControls.CustomButton();
            this.TabPageConnect = new System.Windows.Forms.TabPage();
            this.CustomRadioButtonConnectCloudflare = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonConnectCheckedServers = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonConnectDNSCrypt = new CustomControls.CustomRadioButton();
            this.TabPageSetDNS = new System.Windows.Forms.TabPage();
            this.CustomButtonSetDNS = new CustomControls.CustomButton();
            this.CustomLabelSelectNIC = new CustomControls.CustomLabel();
            this.CustomLabelSetDNSInfo = new CustomControls.CustomLabel();
            this.CustomComboBoxNICs = new CustomControls.CustomComboBox();
            this.TabPageShare = new System.Windows.Forms.TabPage();
            this.CustomNumericUpDownHTTPProxyHandleRequests = new CustomControls.CustomNumericUpDown();
            this.CustomLabelHTTPProxyHandleRequests = new CustomControls.CustomLabel();
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails = new CustomControls.CustomCheckBox();
            this.CustomButtonPDpiApplyChanges = new CustomControls.CustomButton();
            this.CustomNumericUpDownPDpiFragDelay = new CustomControls.CustomNumericUpDown();
            this.CustomLabelPDpiFragDelay = new CustomControls.CustomLabel();
            this.CustomCheckBoxPDpiDontChunkBigData = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxPDpiFragModeRandom = new CustomControls.CustomCheckBox();
            this.CustomButtonSetProxy = new CustomControls.CustomButton();
            this.CustomNumericUpDownPDpiDataLength = new CustomControls.CustomNumericUpDown();
            this.CustomLabelPDpiDpiInfo1 = new CustomControls.CustomLabel();
            this.CustomNumericUpDownPDpiFragmentSize = new CustomControls.CustomNumericUpDown();
            this.CustomLabelPDpiDpiInfo2 = new CustomControls.CustomLabel();
            this.CustomLabelShareSeparator1 = new CustomControls.CustomLabel();
            this.CustomCheckBoxHTTPProxyEventShowRequest = new CustomControls.CustomCheckBox();
            this.CustomNumericUpDownPDpiFragmentChunks = new CustomControls.CustomNumericUpDown();
            this.CustomLabelPDpiDpiInfo3 = new CustomControls.CustomLabel();
            this.CustomCheckBoxPDpiEnableDpiBypass = new CustomControls.CustomCheckBox();
            this.CustomButtonShare = new CustomControls.CustomButton();
            this.CustomLabelHTTPProxyPort = new CustomControls.CustomLabel();
            this.CustomLabelShareInfo = new CustomControls.CustomLabel();
            this.CustomNumericUpDownHTTPProxyPort = new CustomControls.CustomNumericUpDown();
            this.TabPageDPI = new System.Windows.Forms.TabPage();
            this.CustomTabControlDPIBasicAdvanced = new CustomControls.CustomTabControl();
            this.TabPageDPIBasic = new System.Windows.Forms.TabPage();
            this.CustomLabelInfoDPIModes = new CustomControls.CustomLabel();
            this.CustomRadioButtonDPIMode6 = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonDPIMode5 = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonDPIMode4 = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonDPIMode3 = new CustomControls.CustomRadioButton();
            this.CustomLabelDPIModesGoodbyeDPI = new CustomControls.CustomLabel();
            this.CustomRadioButtonDPIMode2 = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonDPIMode1 = new CustomControls.CustomRadioButton();
            this.CustomButtonDPIBasicDeactivate = new CustomControls.CustomButton();
            this.CustomButtonDPIBasicActivate = new CustomControls.CustomButton();
            this.TabPageDPIAdvanced = new System.Windows.Forms.TabPage();
            this.CustomTextBoxDPIAdvAutoTTL = new CustomControls.CustomTextBox();
            this.CustomNumericUpDownDPIAdvMaxPayload = new CustomControls.CustomNumericUpDown();
            this.CustomNumericUpDownDPIAdvMinTTL = new CustomControls.CustomNumericUpDown();
            this.CustomNumericUpDownDPIAdvSetTTL = new CustomControls.CustomNumericUpDown();
            this.CustomNumericUpDownDPIAdvPort = new CustomControls.CustomNumericUpDown();
            this.CustomButtonDPIAdvDeactivate = new CustomControls.CustomButton();
            this.CustomButtonDPIAdvActivate = new CustomControls.CustomButton();
            this.CustomButtonDPIAdvBlacklist = new CustomControls.CustomButton();
            this.CustomCheckBoxDPIAdvBlacklist = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvMaxPayload = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvReverseFrag = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvNativeFrag = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvWrongSeq = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvWrongChksum = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvMinTTL = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvAutoTTL = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvSetTTL = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvAllowNoSNI = new CustomControls.CustomCheckBox();
            this.CustomTextBoxDPIAdvIpId = new CustomControls.CustomTextBox();
            this.CustomCheckBoxDPIAdvIpId = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvPort = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvW = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvA = new CustomControls.CustomCheckBox();
            this.CustomNumericUpDownDPIAdvE = new CustomControls.CustomNumericUpDown();
            this.CustomCheckBoxDPIAdvE = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvN = new CustomControls.CustomCheckBox();
            this.CustomNumericUpDownDPIAdvK = new CustomControls.CustomNumericUpDown();
            this.CustomCheckBoxDPIAdvK = new CustomControls.CustomCheckBox();
            this.CustomNumericUpDownDPIAdvF = new CustomControls.CustomNumericUpDown();
            this.CustomCheckBoxDPIAdvF = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvM = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvS = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvR = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxDPIAdvP = new CustomControls.CustomCheckBox();
            this.TabPageSettings = new System.Windows.Forms.TabPage();
            this.CustomTabControlSettings = new CustomControls.CustomTabControl();
            this.TabPageSettingsWorkingMode = new System.Windows.Forms.TabPage();
            this.CustomLabelSettingInfoWorkingMode2 = new CustomControls.CustomLabel();
            this.CustomRadioButtonSettingWorkingModeDNSandDoH = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonSettingWorkingModeDNS = new CustomControls.CustomRadioButton();
            this.CustomLabelSettingInfoWorkingMode1 = new CustomControls.CustomLabel();
            this.TabPageSettingsCheck = new System.Windows.Forms.TabPage();
            this.CustomGroupBoxSettingCheckSDNS = new CustomControls.CustomGroupBox();
            this.CustomCheckBoxSettingSdnsNoFilter = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxSettingSdnsNoLog = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxSettingSdnsDNSSec = new CustomControls.CustomCheckBox();
            this.CustomTextBoxSettingCheckDPIHost = new CustomControls.CustomTextBox();
            this.CustomLabelSettingCheckDPIInfo = new CustomControls.CustomLabel();
            this.CustomLabelSettingCheckTimeout = new CustomControls.CustomLabel();
            this.CustomNumericUpDownSettingCheckTimeout = new CustomControls.CustomNumericUpDown();
            this.TabPageSettingsConnect = new System.Windows.Forms.TabPage();
            this.CustomCheckBoxSettingEnableCache = new CustomControls.CustomCheckBox();
            this.CustomNumericUpDownSettingCamouflagePort = new CustomControls.CustomNumericUpDown();
            this.CustomLabelCheckSettingCamouflagePort = new CustomControls.CustomLabel();
            this.CustomNumericUpDownSettingMaxServers = new CustomControls.CustomNumericUpDown();
            this.CustomLabelSettingMaxServers = new CustomControls.CustomLabel();
            this.TabPageSettingsSetUnsetDNS = new System.Windows.Forms.TabPage();
            this.CustomTextBoxSettingUnsetDns2 = new CustomControls.CustomTextBox();
            this.CustomTextBoxSettingUnsetDns1 = new CustomControls.CustomTextBox();
            this.CustomLabelSettingUnsetDns2 = new CustomControls.CustomLabel();
            this.CustomLabelSettingUnsetDns1 = new CustomControls.CustomLabel();
            this.CustomRadioButtonSettingUnsetDnsToStatic = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonSettingUnsetDnsToDhcp = new CustomControls.CustomRadioButton();
            this.TabPageSettingsCPU = new System.Windows.Forms.TabPage();
            this.CustomRadioButtonSettingCPULow = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonSettingCPUBelowNormal = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonSettingCPUNormal = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonSettingCPUAboveNormal = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonSettingCPUHigh = new CustomControls.CustomRadioButton();
            this.CustomLabelSettingInfoCPU = new CustomControls.CustomLabel();
            this.TabPageSettingsOthers = new System.Windows.Forms.TabPage();
            this.CustomNumericUpDownSettingFallbackDnsPort = new CustomControls.CustomNumericUpDown();
            this.CustomLabelSettingFallbackDnsPort = new CustomControls.CustomLabel();
            this.CustomTextBoxSettingFallbackDnsIP = new CustomControls.CustomTextBox();
            this.CustomLabelSettingFallbackDnsIP = new CustomControls.CustomLabel();
            this.CustomNumericUpDownSettingBootstrapDnsPort = new CustomControls.CustomNumericUpDown();
            this.CustomLabelSettingBootstrapDnsPort = new CustomControls.CustomLabel();
            this.CustomButtonSettingRestoreDefault = new CustomControls.CustomButton();
            this.CustomCheckBoxSettingDisableAudioAlert = new CustomControls.CustomCheckBox();
            this.CustomLabelSettingBootstrapDnsIP = new CustomControls.CustomLabel();
            this.CustomCheckBoxSettingDontAskCertificate = new CustomControls.CustomCheckBox();
            this.CustomTextBoxSettingBootstrapDnsIP = new CustomControls.CustomTextBox();
            this.TabPageAbout = new System.Windows.Forms.TabPage();
            this.LinkLabelStAlidxdydz = new System.Windows.Forms.LinkLabel();
            this.CustomLabelAboutSpecialThanks = new CustomControls.CustomLabel();
            this.LinkLabelGoodbyeDPI = new System.Windows.Forms.LinkLabel();
            this.LinkLabelDNSCrypt = new System.Windows.Forms.LinkLabel();
            this.LinkLabelDNSProxy = new System.Windows.Forms.LinkLabel();
            this.LinkLabelDNSLookup = new System.Windows.Forms.LinkLabel();
            this.CustomLabelAboutUsing = new CustomControls.CustomLabel();
            this.CustomLabelAboutVersion = new CustomControls.CustomLabel();
            this.CustomLabelAboutThis2 = new CustomControls.CustomLabel();
            this.CustomLabelAboutThis = new CustomControls.CustomLabel();
            this.PictureBoxAbout = new System.Windows.Forms.PictureBox();
            this.CustomButtonToggleLogView = new CustomControls.CustomButton();
            this.NotifyIconMain = new System.Windows.Forms.NotifyIcon(this.components);
            this.CustomContextMenuStripIcon = new CustomControls.CustomContextMenuStrip();
            this.CustomGroupBoxStatus = new CustomControls.CustomGroupBox();
            this.CustomRichTextBoxStatusGoodbyeDPI = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusProxyRequests = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusLocalDoHLatency = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusLocalDoH = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusLocalDnsLatency = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusLocalDNS = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusIsProxySet = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusIsSharing = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusIsDNSSet = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusProxyDpiBypass = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusIsConnected = new CustomControls.CustomRichTextBox();
            this.CustomRichTextBoxStatusWorkingServers = new CustomControls.CustomRichTextBox();
            this.SplitContainerMain = new System.Windows.Forms.SplitContainer();
            this.SplitContainerTop = new System.Windows.Forms.SplitContainer();
            this.CustomGroupBoxLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSSLFragmentSize)).BeginInit();
            this.CustomTabControlMain.SuspendLayout();
            this.TabPageSecureDNS.SuspendLayout();
            this.CustomTabControlSecureDNS.SuspendLayout();
            this.TabPageCheck.SuspendLayout();
            this.TabPageConnect.SuspendLayout();
            this.TabPageSetDNS.SuspendLayout();
            this.TabPageShare.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownHTTPProxyHandleRequests)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPDpiFragDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPDpiDataLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPDpiFragmentSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPDpiFragmentChunks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownHTTPProxyPort)).BeginInit();
            this.TabPageDPI.SuspendLayout();
            this.CustomTabControlDPIBasicAdvanced.SuspendLayout();
            this.TabPageDPIBasic.SuspendLayout();
            this.TabPageDPIAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvMaxPayload)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvMinTTL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvSetTTL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvE)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvF)).BeginInit();
            this.TabPageSettings.SuspendLayout();
            this.CustomTabControlSettings.SuspendLayout();
            this.TabPageSettingsWorkingMode.SuspendLayout();
            this.TabPageSettingsCheck.SuspendLayout();
            this.CustomGroupBoxSettingCheckSDNS.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingCheckTimeout)).BeginInit();
            this.TabPageSettingsConnect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingCamouflagePort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingMaxServers)).BeginInit();
            this.TabPageSettingsSetUnsetDNS.SuspendLayout();
            this.TabPageSettingsCPU.SuspendLayout();
            this.TabPageSettingsOthers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingFallbackDnsPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingBootstrapDnsPort)).BeginInit();
            this.TabPageAbout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxAbout)).BeginInit();
            this.CustomGroupBoxStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainerMain)).BeginInit();
            this.SplitContainerMain.Panel1.SuspendLayout();
            this.SplitContainerMain.Panel2.SuspendLayout();
            this.SplitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainerTop)).BeginInit();
            this.SplitContainerTop.Panel1.SuspendLayout();
            this.SplitContainerTop.Panel2.SuspendLayout();
            this.SplitContainerTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // CustomRichTextBoxLog
            // 
            this.CustomRichTextBoxLog.AcceptsTab = false;
            this.CustomRichTextBoxLog.AutoWordSelection = false;
            this.CustomRichTextBoxLog.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxLog.Border = false;
            this.CustomRichTextBoxLog.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxLog.BorderSize = 1;
            this.CustomRichTextBoxLog.BulletIndent = 0;
            this.CustomRichTextBoxLog.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.CustomRichTextBoxLog.DetectUrls = false;
            this.CustomRichTextBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomRichTextBoxLog.EnableAutoDragDrop = false;
            this.CustomRichTextBoxLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxLog.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxLog.HideSelection = false;
            this.CustomRichTextBoxLog.Location = new System.Drawing.Point(3, 19);
            this.CustomRichTextBoxLog.Margin = new System.Windows.Forms.Padding(1);
            this.CustomRichTextBoxLog.MaxLength = 2147483647;
            this.CustomRichTextBoxLog.MinimumSize = new System.Drawing.Size(0, 23);
            this.CustomRichTextBoxLog.Multiline = true;
            this.CustomRichTextBoxLog.Name = "CustomRichTextBoxLog";
            this.CustomRichTextBoxLog.ReadOnly = true;
            this.CustomRichTextBoxLog.RightMargin = 0;
            this.CustomRichTextBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.CustomRichTextBoxLog.ScrollToBottom = true;
            this.CustomRichTextBoxLog.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxLog.SelectionLength = 0;
            this.CustomRichTextBoxLog.SelectionStart = 0;
            this.CustomRichTextBoxLog.ShortcutsEnabled = true;
            this.CustomRichTextBoxLog.Size = new System.Drawing.Size(878, 155);
            this.CustomRichTextBoxLog.TabIndex = 0;
            this.CustomRichTextBoxLog.Texts = "";
            this.CustomRichTextBoxLog.UnderlinedStyle = false;
            this.CustomRichTextBoxLog.WordWrap = true;
            this.CustomRichTextBoxLog.ZoomFactor = 1F;
            // 
            // CustomButtonCheck
            // 
            this.CustomButtonCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonCheck.AutoSize = true;
            this.CustomButtonCheck.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonCheck.Location = new System.Drawing.Point(302, 303);
            this.CustomButtonCheck.Name = "CustomButtonCheck";
            this.CustomButtonCheck.RoundedCorners = 0;
            this.CustomButtonCheck.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonCheck.Size = new System.Drawing.Size(93, 27);
            this.CustomButtonCheck.TabIndex = 2;
            this.CustomButtonCheck.Text = "Check/Cancel";
            this.CustomButtonCheck.UseVisualStyleBackColor = true;
            this.CustomButtonCheck.Click += new System.EventHandler(this.CustomButtonCheck_Click);
            // 
            // CustomGroupBoxLog
            // 
            this.CustomGroupBoxLog.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CustomGroupBoxLog.BorderColor = System.Drawing.Color.Blue;
            this.CustomGroupBoxLog.Controls.Add(this.CustomRichTextBoxLog);
            this.CustomGroupBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomGroupBoxLog.Location = new System.Drawing.Point(0, 0);
            this.CustomGroupBoxLog.Margin = new System.Windows.Forms.Padding(1);
            this.CustomGroupBoxLog.Name = "CustomGroupBoxLog";
            this.CustomGroupBoxLog.Size = new System.Drawing.Size(884, 177);
            this.CustomGroupBoxLog.TabIndex = 3;
            this.CustomGroupBoxLog.TabStop = false;
            this.CustomGroupBoxLog.Text = "Log";
            // 
            // CustomCheckBoxInsecure
            // 
            this.CustomCheckBoxInsecure.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxInsecure.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxInsecure.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxInsecure.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxInsecure.Location = new System.Drawing.Point(25, 190);
            this.CustomCheckBoxInsecure.Name = "CustomCheckBoxInsecure";
            this.CustomCheckBoxInsecure.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxInsecure.Size = new System.Drawing.Size(211, 17);
            this.CustomCheckBoxInsecure.TabIndex = 7;
            this.CustomCheckBoxInsecure.Text = "Allow insecure (not recommended)";
            this.CustomCheckBoxInsecure.UseVisualStyleBackColor = false;
            this.CustomCheckBoxInsecure.CheckedChanged += new System.EventHandler(this.SecureDNSClient_CheckedChanged);
            // 
            // CustomLabelCustomServersInfo
            // 
            this.CustomLabelCustomServersInfo.AutoSize = true;
            this.CustomLabelCustomServersInfo.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelCustomServersInfo.Border = false;
            this.CustomLabelCustomServersInfo.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelCustomServersInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelCustomServersInfo.ForeColor = System.Drawing.Color.White;
            this.CustomLabelCustomServersInfo.Location = new System.Drawing.Point(41, 75);
            this.CustomLabelCustomServersInfo.Name = "CustomLabelCustomServersInfo";
            this.CustomLabelCustomServersInfo.RoundedCorners = 0;
            this.CustomLabelCustomServersInfo.Size = new System.Drawing.Size(218, 92);
            this.CustomLabelCustomServersInfo.TabIndex = 6;
            this.CustomLabelCustomServersInfo.Text = "Supported: DoH, DoT, DoQ, DNSCrypt.\r\nEach line one server. e.g:\r\n  https://cloudf" +
    "lare-dns.com/dns-query\r\n  tls://dns.google\r\n  quic://dns.adguard.com\r\n  sdns://";
            // 
            // CustomButtonEditCustomServers
            // 
            this.CustomButtonEditCustomServers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonEditCustomServers.AutoSize = true;
            this.CustomButtonEditCustomServers.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonEditCustomServers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonEditCustomServers.Location = new System.Drawing.Point(40, 303);
            this.CustomButtonEditCustomServers.Name = "CustomButtonEditCustomServers";
            this.CustomButtonEditCustomServers.RoundedCorners = 0;
            this.CustomButtonEditCustomServers.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonEditCustomServers.Size = new System.Drawing.Size(121, 27);
            this.CustomButtonEditCustomServers.TabIndex = 5;
            this.CustomButtonEditCustomServers.Text = "Edit custom servers";
            this.CustomButtonEditCustomServers.UseVisualStyleBackColor = true;
            this.CustomButtonEditCustomServers.Click += new System.EventHandler(this.CustomButtonEditCustomServers_Click);
            // 
            // CustomRadioButtonCustom
            // 
            this.CustomRadioButtonCustom.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonCustom.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonCustom.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonCustom.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonCustom.Location = new System.Drawing.Point(25, 50);
            this.CustomRadioButtonCustom.Name = "CustomRadioButtonCustom";
            this.CustomRadioButtonCustom.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonCustom.Size = new System.Drawing.Size(125, 17);
            this.CustomRadioButtonCustom.TabIndex = 4;
            this.CustomRadioButtonCustom.TabStop = true;
            this.CustomRadioButtonCustom.Text = "Use custom servers";
            this.CustomRadioButtonCustom.UseVisualStyleBackColor = false;
            this.CustomRadioButtonCustom.CheckedChanged += new System.EventHandler(this.SecureDNSClient_CheckedChanged);
            // 
            // CustomRadioButtonBuiltIn
            // 
            this.CustomRadioButtonBuiltIn.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonBuiltIn.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonBuiltIn.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonBuiltIn.Checked = true;
            this.CustomRadioButtonBuiltIn.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonBuiltIn.Location = new System.Drawing.Point(25, 20);
            this.CustomRadioButtonBuiltIn.Name = "CustomRadioButtonBuiltIn";
            this.CustomRadioButtonBuiltIn.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonBuiltIn.Size = new System.Drawing.Size(125, 17);
            this.CustomRadioButtonBuiltIn.TabIndex = 3;
            this.CustomRadioButtonBuiltIn.TabStop = true;
            this.CustomRadioButtonBuiltIn.Text = "Use built-in servers";
            this.CustomRadioButtonBuiltIn.UseVisualStyleBackColor = false;
            this.CustomRadioButtonBuiltIn.CheckedChanged += new System.EventHandler(this.SecureDNSClient_CheckedChanged);
            // 
            // CustomLabelSSLFragmentSize
            // 
            this.CustomLabelSSLFragmentSize.AutoSize = true;
            this.CustomLabelSSLFragmentSize.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSSLFragmentSize.Border = false;
            this.CustomLabelSSLFragmentSize.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSSLFragmentSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSSLFragmentSize.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSSLFragmentSize.Location = new System.Drawing.Point(296, 95);
            this.CustomLabelSSLFragmentSize.Name = "CustomLabelSSLFragmentSize";
            this.CustomLabelSSLFragmentSize.RoundedCorners = 0;
            this.CustomLabelSSLFragmentSize.Size = new System.Drawing.Size(102, 15);
            this.CustomLabelSSLFragmentSize.TabIndex = 10;
            this.CustomLabelSSLFragmentSize.Text = "SSL fragment size:";
            // 
            // CustomNumericUpDownSSLFragmentSize
            // 
            this.CustomNumericUpDownSSLFragmentSize.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownSSLFragmentSize.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownSSLFragmentSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownSSLFragmentSize.Location = new System.Drawing.Point(404, 93);
            this.CustomNumericUpDownSSLFragmentSize.Margin = new System.Windows.Forms.Padding(1);
            this.CustomNumericUpDownSSLFragmentSize.Maximum = new decimal(new int[] {
            70000,
            0,
            0,
            0});
            this.CustomNumericUpDownSSLFragmentSize.Name = "CustomNumericUpDownSSLFragmentSize";
            this.CustomNumericUpDownSSLFragmentSize.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownSSLFragmentSize.TabIndex = 9;
            this.CustomNumericUpDownSSLFragmentSize.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            // 
            // CustomTextBoxHTTPProxy
            // 
            this.CustomTextBoxHTTPProxy.AcceptsReturn = false;
            this.CustomTextBoxHTTPProxy.AcceptsTab = false;
            this.CustomTextBoxHTTPProxy.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxHTTPProxy.Border = true;
            this.CustomTextBoxHTTPProxy.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxHTTPProxy.BorderSize = 1;
            this.CustomTextBoxHTTPProxy.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxHTTPProxy.Enabled = false;
            this.CustomTextBoxHTTPProxy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxHTTPProxy.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxHTTPProxy.HideSelection = true;
            this.CustomTextBoxHTTPProxy.Location = new System.Drawing.Point(312, 123);
            this.CustomTextBoxHTTPProxy.Margin = new System.Windows.Forms.Padding(1);
            this.CustomTextBoxHTTPProxy.MaxLength = 32767;
            this.CustomTextBoxHTTPProxy.Multiline = false;
            this.CustomTextBoxHTTPProxy.Name = "CustomTextBoxHTTPProxy";
            this.CustomTextBoxHTTPProxy.ReadOnly = false;
            this.CustomTextBoxHTTPProxy.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxHTTPProxy.ShortcutsEnabled = true;
            this.CustomTextBoxHTTPProxy.Size = new System.Drawing.Size(205, 23);
            this.CustomTextBoxHTTPProxy.TabIndex = 0;
            this.CustomTextBoxHTTPProxy.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxHTTPProxy.Texts = "";
            this.CustomTextBoxHTTPProxy.UnderlinedStyle = false;
            this.CustomTextBoxHTTPProxy.UsePasswordChar = false;
            this.CustomTextBoxHTTPProxy.WordWrap = true;
            // 
            // CustomRadioButtonDPIModeExtreme
            // 
            this.CustomRadioButtonDPIModeExtreme.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIModeExtreme.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIModeExtreme.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIModeExtreme.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIModeExtreme.Location = new System.Drawing.Point(196, 95);
            this.CustomRadioButtonDPIModeExtreme.Margin = new System.Windows.Forms.Padding(1);
            this.CustomRadioButtonDPIModeExtreme.Name = "CustomRadioButtonDPIModeExtreme";
            this.CustomRadioButtonDPIModeExtreme.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIModeExtreme.Size = new System.Drawing.Size(64, 17);
            this.CustomRadioButtonDPIModeExtreme.TabIndex = 7;
            this.CustomRadioButtonDPIModeExtreme.Text = "Extreme";
            this.CustomRadioButtonDPIModeExtreme.UseVisualStyleBackColor = false;
            this.CustomRadioButtonDPIModeExtreme.CheckedChanged += new System.EventHandler(this.SecureDNSClient_CheckedChanged);
            // 
            // CustomRadioButtonDPIModeHigh
            // 
            this.CustomRadioButtonDPIModeHigh.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIModeHigh.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIModeHigh.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIModeHigh.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIModeHigh.Location = new System.Drawing.Point(147, 95);
            this.CustomRadioButtonDPIModeHigh.Margin = new System.Windows.Forms.Padding(1);
            this.CustomRadioButtonDPIModeHigh.Name = "CustomRadioButtonDPIModeHigh";
            this.CustomRadioButtonDPIModeHigh.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIModeHigh.Size = new System.Drawing.Size(47, 17);
            this.CustomRadioButtonDPIModeHigh.TabIndex = 6;
            this.CustomRadioButtonDPIModeHigh.Text = "High";
            this.CustomRadioButtonDPIModeHigh.UseVisualStyleBackColor = false;
            this.CustomRadioButtonDPIModeHigh.CheckedChanged += new System.EventHandler(this.SecureDNSClient_CheckedChanged);
            // 
            // CustomRadioButtonDPIModeMedium
            // 
            this.CustomRadioButtonDPIModeMedium.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIModeMedium.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIModeMedium.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIModeMedium.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIModeMedium.Location = new System.Drawing.Point(79, 95);
            this.CustomRadioButtonDPIModeMedium.Margin = new System.Windows.Forms.Padding(1);
            this.CustomRadioButtonDPIModeMedium.Name = "CustomRadioButtonDPIModeMedium";
            this.CustomRadioButtonDPIModeMedium.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIModeMedium.Size = new System.Drawing.Size(66, 17);
            this.CustomRadioButtonDPIModeMedium.TabIndex = 5;
            this.CustomRadioButtonDPIModeMedium.Text = "Medium";
            this.CustomRadioButtonDPIModeMedium.UseVisualStyleBackColor = false;
            this.CustomRadioButtonDPIModeMedium.CheckedChanged += new System.EventHandler(this.SecureDNSClient_CheckedChanged);
            // 
            // CustomRadioButtonDPIModeLight
            // 
            this.CustomRadioButtonDPIModeLight.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIModeLight.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIModeLight.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIModeLight.Checked = true;
            this.CustomRadioButtonDPIModeLight.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIModeLight.Location = new System.Drawing.Point(25, 95);
            this.CustomRadioButtonDPIModeLight.Margin = new System.Windows.Forms.Padding(1);
            this.CustomRadioButtonDPIModeLight.Name = "CustomRadioButtonDPIModeLight";
            this.CustomRadioButtonDPIModeLight.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIModeLight.Size = new System.Drawing.Size(48, 17);
            this.CustomRadioButtonDPIModeLight.TabIndex = 4;
            this.CustomRadioButtonDPIModeLight.TabStop = true;
            this.CustomRadioButtonDPIModeLight.Text = "Light";
            this.CustomRadioButtonDPIModeLight.UseVisualStyleBackColor = false;
            this.CustomRadioButtonDPIModeLight.CheckedChanged += new System.EventHandler(this.SecureDNSClient_CheckedChanged);
            // 
            // CustomLabelDPIModes
            // 
            this.CustomLabelDPIModes.AutoSize = true;
            this.CustomLabelDPIModes.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelDPIModes.Border = false;
            this.CustomLabelDPIModes.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelDPIModes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelDPIModes.ForeColor = System.Drawing.Color.White;
            this.CustomLabelDPIModes.Location = new System.Drawing.Point(25, 70);
            this.CustomLabelDPIModes.Name = "CustomLabelDPIModes";
            this.CustomLabelDPIModes.RoundedCorners = 0;
            this.CustomLabelDPIModes.Size = new System.Drawing.Size(75, 15);
            this.CustomLabelDPIModes.TabIndex = 3;
            this.CustomLabelDPIModes.Text = "Select mode:";
            // 
            // CustomButtonConnect
            // 
            this.CustomButtonConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonConnect.AutoSize = true;
            this.CustomButtonConnect.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonConnect.Location = new System.Drawing.Point(40, 303);
            this.CustomButtonConnect.Name = "CustomButtonConnect";
            this.CustomButtonConnect.RoundedCorners = 0;
            this.CustomButtonConnect.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonConnect.Size = new System.Drawing.Size(128, 27);
            this.CustomButtonConnect.TabIndex = 1;
            this.CustomButtonConnect.Text = "Connect/Disconnect";
            this.CustomButtonConnect.UseVisualStyleBackColor = true;
            this.CustomButtonConnect.Click += new System.EventHandler(this.CustomButtonConnect_Click);
            // 
            // CustomTabControlMain
            // 
            this.CustomTabControlMain.BorderColor = System.Drawing.Color.Blue;
            this.CustomTabControlMain.Controls.Add(this.TabPageSecureDNS);
            this.CustomTabControlMain.Controls.Add(this.TabPageSettings);
            this.CustomTabControlMain.Controls.Add(this.TabPageAbout);
            this.CustomTabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomTabControlMain.HideTabHeader = false;
            this.CustomTabControlMain.ItemSize = new System.Drawing.Size(90, 21);
            this.CustomTabControlMain.Location = new System.Drawing.Point(0, 0);
            this.CustomTabControlMain.Margin = new System.Windows.Forms.Padding(0);
            this.CustomTabControlMain.Name = "CustomTabControlMain";
            this.CustomTabControlMain.SelectedIndex = 0;
            this.CustomTabControlMain.Size = new System.Drawing.Size(700, 400);
            this.CustomTabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.CustomTabControlMain.TabIndex = 6;
            this.CustomTabControlMain.Tag = 0;
            // 
            // TabPageSecureDNS
            // 
            this.TabPageSecureDNS.BackColor = System.Drawing.Color.Transparent;
            this.TabPageSecureDNS.Controls.Add(this.CustomTabControlSecureDNS);
            this.TabPageSecureDNS.Location = new System.Drawing.Point(4, 25);
            this.TabPageSecureDNS.Name = "TabPageSecureDNS";
            this.TabPageSecureDNS.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSecureDNS.Size = new System.Drawing.Size(692, 371);
            this.TabPageSecureDNS.TabIndex = 0;
            this.TabPageSecureDNS.Tag = 0;
            this.TabPageSecureDNS.Text = "Secure DNS";
            // 
            // CustomTabControlSecureDNS
            // 
            this.CustomTabControlSecureDNS.BorderColor = System.Drawing.Color.Blue;
            this.CustomTabControlSecureDNS.Controls.Add(this.TabPageCheck);
            this.CustomTabControlSecureDNS.Controls.Add(this.TabPageConnect);
            this.CustomTabControlSecureDNS.Controls.Add(this.TabPageSetDNS);
            this.CustomTabControlSecureDNS.Controls.Add(this.TabPageShare);
            this.CustomTabControlSecureDNS.Controls.Add(this.TabPageDPI);
            this.CustomTabControlSecureDNS.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomTabControlSecureDNS.HideTabHeader = false;
            this.CustomTabControlSecureDNS.ItemSize = new System.Drawing.Size(120, 21);
            this.CustomTabControlSecureDNS.Location = new System.Drawing.Point(3, 3);
            this.CustomTabControlSecureDNS.Name = "CustomTabControlSecureDNS";
            this.CustomTabControlSecureDNS.SelectedIndex = 0;
            this.CustomTabControlSecureDNS.Size = new System.Drawing.Size(686, 365);
            this.CustomTabControlSecureDNS.TabIndex = 0;
            this.CustomTabControlSecureDNS.Tag = 0;
            // 
            // TabPageCheck
            // 
            this.TabPageCheck.BackColor = System.Drawing.Color.Transparent;
            this.TabPageCheck.Controls.Add(this.CustomLabelCheckPercent);
            this.TabPageCheck.Controls.Add(this.CustomButtonConnectAll);
            this.TabPageCheck.Controls.Add(this.CustomButtonViewWorkingServers);
            this.TabPageCheck.Controls.Add(this.CustomButtonCheck);
            this.TabPageCheck.Controls.Add(this.CustomButtonEditCustomServers);
            this.TabPageCheck.Controls.Add(this.CustomCheckBoxInsecure);
            this.TabPageCheck.Controls.Add(this.CustomRadioButtonBuiltIn);
            this.TabPageCheck.Controls.Add(this.CustomLabelCustomServersInfo);
            this.TabPageCheck.Controls.Add(this.CustomRadioButtonCustom);
            this.TabPageCheck.Location = new System.Drawing.Point(4, 25);
            this.TabPageCheck.Name = "TabPageCheck";
            this.TabPageCheck.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageCheck.Size = new System.Drawing.Size(678, 336);
            this.TabPageCheck.TabIndex = 0;
            this.TabPageCheck.Tag = 0;
            this.TabPageCheck.Text = "1. Check";
            // 
            // CustomLabelCheckPercent
            // 
            this.CustomLabelCheckPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomLabelCheckPercent.AutoSize = true;
            this.CustomLabelCheckPercent.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelCheckPercent.Border = false;
            this.CustomLabelCheckPercent.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelCheckPercent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelCheckPercent.ForeColor = System.Drawing.Color.White;
            this.CustomLabelCheckPercent.Location = new System.Drawing.Point(415, 309);
            this.CustomLabelCheckPercent.Name = "CustomLabelCheckPercent";
            this.CustomLabelCheckPercent.RoundedCorners = 0;
            this.CustomLabelCheckPercent.Size = new System.Drawing.Size(25, 17);
            this.CustomLabelCheckPercent.TabIndex = 11;
            this.CustomLabelCheckPercent.Text = "0%";
            // 
            // CustomButtonConnectAll
            // 
            this.CustomButtonConnectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomButtonConnectAll.AutoSize = true;
            this.CustomButtonConnectAll.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonConnectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonConnectAll.Location = new System.Drawing.Point(517, 303);
            this.CustomButtonConnectAll.Name = "CustomButtonConnectAll";
            this.CustomButtonConnectAll.RoundedCorners = 0;
            this.CustomButtonConnectAll.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonConnectAll.Size = new System.Drawing.Size(158, 27);
            this.CustomButtonConnectAll.TabIndex = 9;
            this.CustomButtonConnectAll.Text = "Connect all/Disconnect all";
            this.CustomButtonConnectAll.UseVisualStyleBackColor = true;
            this.CustomButtonConnectAll.Visible = false;
            this.CustomButtonConnectAll.Click += new System.EventHandler(this.CustomButtonConnectAll_Click);
            // 
            // CustomButtonViewWorkingServers
            // 
            this.CustomButtonViewWorkingServers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonViewWorkingServers.AutoSize = true;
            this.CustomButtonViewWorkingServers.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonViewWorkingServers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonViewWorkingServers.Location = new System.Drawing.Point(167, 303);
            this.CustomButtonViewWorkingServers.Name = "CustomButtonViewWorkingServers";
            this.CustomButtonViewWorkingServers.RoundedCorners = 0;
            this.CustomButtonViewWorkingServers.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonViewWorkingServers.Size = new System.Drawing.Size(129, 27);
            this.CustomButtonViewWorkingServers.TabIndex = 8;
            this.CustomButtonViewWorkingServers.Text = "View working servers";
            this.CustomButtonViewWorkingServers.UseVisualStyleBackColor = true;
            this.CustomButtonViewWorkingServers.Click += new System.EventHandler(this.CustomButtonViewWorkingServers_Click);
            // 
            // TabPageConnect
            // 
            this.TabPageConnect.BackColor = System.Drawing.Color.Transparent;
            this.TabPageConnect.Controls.Add(this.CustomRadioButtonConnectCloudflare);
            this.TabPageConnect.Controls.Add(this.CustomRadioButtonConnectCheckedServers);
            this.TabPageConnect.Controls.Add(this.CustomRadioButtonConnectDNSCrypt);
            this.TabPageConnect.Controls.Add(this.CustomTextBoxHTTPProxy);
            this.TabPageConnect.Controls.Add(this.CustomButtonConnect);
            this.TabPageConnect.Location = new System.Drawing.Point(4, 25);
            this.TabPageConnect.Name = "TabPageConnect";
            this.TabPageConnect.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageConnect.Size = new System.Drawing.Size(678, 336);
            this.TabPageConnect.TabIndex = 1;
            this.TabPageConnect.Tag = 1;
            this.TabPageConnect.Text = "2. Connect";
            // 
            // CustomRadioButtonConnectCloudflare
            // 
            this.CustomRadioButtonConnectCloudflare.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonConnectCloudflare.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonConnectCloudflare.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonConnectCloudflare.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonConnectCloudflare.Location = new System.Drawing.Point(25, 80);
            this.CustomRadioButtonConnectCloudflare.Name = "CustomRadioButtonConnectCloudflare";
            this.CustomRadioButtonConnectCloudflare.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonConnectCloudflare.Size = new System.Drawing.Size(168, 17);
            this.CustomRadioButtonConnectCloudflare.TabIndex = 15;
            this.CustomRadioButtonConnectCloudflare.Text = "Connect to Cloudflare DoH";
            this.CustomRadioButtonConnectCloudflare.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonConnectCheckedServers
            // 
            this.CustomRadioButtonConnectCheckedServers.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonConnectCheckedServers.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonConnectCheckedServers.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonConnectCheckedServers.Checked = true;
            this.CustomRadioButtonConnectCheckedServers.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonConnectCheckedServers.Location = new System.Drawing.Point(25, 35);
            this.CustomRadioButtonConnectCheckedServers.Name = "CustomRadioButtonConnectCheckedServers";
            this.CustomRadioButtonConnectCheckedServers.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonConnectCheckedServers.Size = new System.Drawing.Size(169, 17);
            this.CustomRadioButtonConnectCheckedServers.TabIndex = 14;
            this.CustomRadioButtonConnectCheckedServers.TabStop = true;
            this.CustomRadioButtonConnectCheckedServers.Text = "Connect to working servers";
            this.CustomRadioButtonConnectCheckedServers.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonConnectDNSCrypt
            // 
            this.CustomRadioButtonConnectDNSCrypt.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonConnectDNSCrypt.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonConnectDNSCrypt.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonConnectDNSCrypt.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonConnectDNSCrypt.Location = new System.Drawing.Point(25, 125);
            this.CustomRadioButtonConnectDNSCrypt.Name = "CustomRadioButtonConnectDNSCrypt";
            this.CustomRadioButtonConnectDNSCrypt.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonConnectDNSCrypt.Size = new System.Drawing.Size(283, 17);
            this.CustomRadioButtonConnectDNSCrypt.TabIndex = 13;
            this.CustomRadioButtonConnectDNSCrypt.Text = "Connect to popular servers using HTTP(S) proxy:";
            this.CustomRadioButtonConnectDNSCrypt.UseVisualStyleBackColor = false;
            this.CustomRadioButtonConnectDNSCrypt.CheckedChanged += new System.EventHandler(this.SecureDNSClient_CheckedChanged);
            // 
            // TabPageSetDNS
            // 
            this.TabPageSetDNS.BackColor = System.Drawing.Color.Transparent;
            this.TabPageSetDNS.Controls.Add(this.CustomButtonSetDNS);
            this.TabPageSetDNS.Controls.Add(this.CustomLabelSelectNIC);
            this.TabPageSetDNS.Controls.Add(this.CustomLabelSetDNSInfo);
            this.TabPageSetDNS.Controls.Add(this.CustomComboBoxNICs);
            this.TabPageSetDNS.Location = new System.Drawing.Point(4, 25);
            this.TabPageSetDNS.Name = "TabPageSetDNS";
            this.TabPageSetDNS.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSetDNS.Size = new System.Drawing.Size(678, 336);
            this.TabPageSetDNS.TabIndex = 3;
            this.TabPageSetDNS.Tag = 2;
            this.TabPageSetDNS.Text = "3. Set DNS";
            // 
            // CustomButtonSetDNS
            // 
            this.CustomButtonSetDNS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonSetDNS.AutoSize = true;
            this.CustomButtonSetDNS.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonSetDNS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonSetDNS.Location = new System.Drawing.Point(40, 303);
            this.CustomButtonSetDNS.Name = "CustomButtonSetDNS";
            this.CustomButtonSetDNS.RoundedCorners = 0;
            this.CustomButtonSetDNS.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonSetDNS.Size = new System.Drawing.Size(71, 27);
            this.CustomButtonSetDNS.TabIndex = 3;
            this.CustomButtonSetDNS.Text = "Set/Unset";
            this.CustomButtonSetDNS.UseVisualStyleBackColor = true;
            this.CustomButtonSetDNS.Click += new System.EventHandler(this.CustomButtonSetDNS_Click);
            // 
            // CustomLabelSelectNIC
            // 
            this.CustomLabelSelectNIC.AutoSize = true;
            this.CustomLabelSelectNIC.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSelectNIC.Border = false;
            this.CustomLabelSelectNIC.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSelectNIC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSelectNIC.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSelectNIC.Location = new System.Drawing.Point(25, 25);
            this.CustomLabelSelectNIC.Name = "CustomLabelSelectNIC";
            this.CustomLabelSelectNIC.RoundedCorners = 0;
            this.CustomLabelSelectNIC.Size = new System.Drawing.Size(135, 15);
            this.CustomLabelSelectNIC.TabIndex = 0;
            this.CustomLabelSelectNIC.Text = "Select Network Interface";
            // 
            // CustomLabelSetDNSInfo
            // 
            this.CustomLabelSetDNSInfo.AutoSize = true;
            this.CustomLabelSetDNSInfo.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSetDNSInfo.Border = false;
            this.CustomLabelSetDNSInfo.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSetDNSInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSetDNSInfo.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSetDNSInfo.Location = new System.Drawing.Point(25, 100);
            this.CustomLabelSetDNSInfo.Name = "CustomLabelSetDNSInfo";
            this.CustomLabelSetDNSInfo.RoundedCorners = 0;
            this.CustomLabelSetDNSInfo.Size = new System.Drawing.Size(220, 45);
            this.CustomLabelSetDNSInfo.TabIndex = 2;
            this.CustomLabelSetDNSInfo.Text = "You have two options:\r\n1. Set DNS to Windows by below button.\r\n2. Set DoH to Fire" +
    "fox manually.";
            // 
            // CustomComboBoxNICs
            // 
            this.CustomComboBoxNICs.BackColor = System.Drawing.Color.DimGray;
            this.CustomComboBoxNICs.BorderColor = System.Drawing.Color.Blue;
            this.CustomComboBoxNICs.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CustomComboBoxNICs.ForeColor = System.Drawing.Color.White;
            this.CustomComboBoxNICs.FormattingEnabled = true;
            this.CustomComboBoxNICs.ItemHeight = 17;
            this.CustomComboBoxNICs.Location = new System.Drawing.Point(25, 55);
            this.CustomComboBoxNICs.Name = "CustomComboBoxNICs";
            this.CustomComboBoxNICs.SelectionColor = System.Drawing.Color.DodgerBlue;
            this.CustomComboBoxNICs.Size = new System.Drawing.Size(171, 23);
            this.CustomComboBoxNICs.TabIndex = 1;
            // 
            // TabPageShare
            // 
            this.TabPageShare.BackColor = System.Drawing.Color.Transparent;
            this.TabPageShare.Controls.Add(this.CustomNumericUpDownHTTPProxyHandleRequests);
            this.TabPageShare.Controls.Add(this.CustomLabelHTTPProxyHandleRequests);
            this.TabPageShare.Controls.Add(this.CustomCheckBoxHTTPProxyEventShowChunkDetails);
            this.TabPageShare.Controls.Add(this.CustomButtonPDpiApplyChanges);
            this.TabPageShare.Controls.Add(this.CustomNumericUpDownPDpiFragDelay);
            this.TabPageShare.Controls.Add(this.CustomLabelPDpiFragDelay);
            this.TabPageShare.Controls.Add(this.CustomCheckBoxPDpiDontChunkBigData);
            this.TabPageShare.Controls.Add(this.CustomCheckBoxPDpiFragModeRandom);
            this.TabPageShare.Controls.Add(this.CustomButtonSetProxy);
            this.TabPageShare.Controls.Add(this.CustomNumericUpDownPDpiDataLength);
            this.TabPageShare.Controls.Add(this.CustomLabelPDpiDpiInfo1);
            this.TabPageShare.Controls.Add(this.CustomNumericUpDownPDpiFragmentSize);
            this.TabPageShare.Controls.Add(this.CustomLabelPDpiDpiInfo2);
            this.TabPageShare.Controls.Add(this.CustomLabelShareSeparator1);
            this.TabPageShare.Controls.Add(this.CustomCheckBoxHTTPProxyEventShowRequest);
            this.TabPageShare.Controls.Add(this.CustomNumericUpDownPDpiFragmentChunks);
            this.TabPageShare.Controls.Add(this.CustomLabelPDpiDpiInfo3);
            this.TabPageShare.Controls.Add(this.CustomCheckBoxPDpiEnableDpiBypass);
            this.TabPageShare.Controls.Add(this.CustomButtonShare);
            this.TabPageShare.Controls.Add(this.CustomLabelHTTPProxyPort);
            this.TabPageShare.Controls.Add(this.CustomLabelShareInfo);
            this.TabPageShare.Controls.Add(this.CustomNumericUpDownHTTPProxyPort);
            this.TabPageShare.Location = new System.Drawing.Point(4, 25);
            this.TabPageShare.Name = "TabPageShare";
            this.TabPageShare.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageShare.Size = new System.Drawing.Size(678, 336);
            this.TabPageShare.TabIndex = 4;
            this.TabPageShare.Tag = 3;
            this.TabPageShare.Text = "4. Share + Bypass DPI";
            // 
            // CustomNumericUpDownHTTPProxyHandleRequests
            // 
            this.CustomNumericUpDownHTTPProxyHandleRequests.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownHTTPProxyHandleRequests.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownHTTPProxyHandleRequests.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownHTTPProxyHandleRequests.Location = new System.Drawing.Point(300, 53);
            this.CustomNumericUpDownHTTPProxyHandleRequests.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.CustomNumericUpDownHTTPProxyHandleRequests.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.CustomNumericUpDownHTTPProxyHandleRequests.Name = "CustomNumericUpDownHTTPProxyHandleRequests";
            this.CustomNumericUpDownHTTPProxyHandleRequests.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownHTTPProxyHandleRequests.TabIndex = 40;
            this.CustomNumericUpDownHTTPProxyHandleRequests.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // CustomLabelHTTPProxyHandleRequests
            // 
            this.CustomLabelHTTPProxyHandleRequests.AutoSize = true;
            this.CustomLabelHTTPProxyHandleRequests.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelHTTPProxyHandleRequests.Border = false;
            this.CustomLabelHTTPProxyHandleRequests.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelHTTPProxyHandleRequests.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelHTTPProxyHandleRequests.ForeColor = System.Drawing.Color.White;
            this.CustomLabelHTTPProxyHandleRequests.Location = new System.Drawing.Point(200, 55);
            this.CustomLabelHTTPProxyHandleRequests.Name = "CustomLabelHTTPProxyHandleRequests";
            this.CustomLabelHTTPProxyHandleRequests.RoundedCorners = 0;
            this.CustomLabelHTTPProxyHandleRequests.Size = new System.Drawing.Size(95, 15);
            this.CustomLabelHTTPProxyHandleRequests.TabIndex = 39;
            this.CustomLabelHTTPProxyHandleRequests.Text = "Handle requests:";
            // 
            // CustomCheckBoxHTTPProxyEventShowChunkDetails
            // 
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.Location = new System.Drawing.Point(455, 55);
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.Name = "CustomCheckBoxHTTPProxyEventShowChunkDetails";
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.Size = new System.Drawing.Size(161, 17);
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.TabIndex = 38;
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.Text = "Write chunk details to log";
            this.CustomCheckBoxHTTPProxyEventShowChunkDetails.UseVisualStyleBackColor = false;
            // 
            // CustomButtonPDpiApplyChanges
            // 
            this.CustomButtonPDpiApplyChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonPDpiApplyChanges.AutoSize = true;
            this.CustomButtonPDpiApplyChanges.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonPDpiApplyChanges.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonPDpiApplyChanges.Location = new System.Drawing.Point(249, 303);
            this.CustomButtonPDpiApplyChanges.Name = "CustomButtonPDpiApplyChanges";
            this.CustomButtonPDpiApplyChanges.RoundedCorners = 0;
            this.CustomButtonPDpiApplyChanges.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonPDpiApplyChanges.Size = new System.Drawing.Size(157, 27);
            this.CustomButtonPDpiApplyChanges.TabIndex = 37;
            this.CustomButtonPDpiApplyChanges.Text = "Apply DPI bypass changes";
            this.CustomButtonPDpiApplyChanges.UseVisualStyleBackColor = true;
            this.CustomButtonPDpiApplyChanges.Click += new System.EventHandler(this.CustomButtonPDpiApplyChanges_Click);
            // 
            // CustomNumericUpDownPDpiFragDelay
            // 
            this.CustomNumericUpDownPDpiFragDelay.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownPDpiFragDelay.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownPDpiFragDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownPDpiFragDelay.Location = new System.Drawing.Point(363, 218);
            this.CustomNumericUpDownPDpiFragDelay.Name = "CustomNumericUpDownPDpiFragDelay";
            this.CustomNumericUpDownPDpiFragDelay.Size = new System.Drawing.Size(43, 23);
            this.CustomNumericUpDownPDpiFragDelay.TabIndex = 36;
            // 
            // CustomLabelPDpiFragDelay
            // 
            this.CustomLabelPDpiFragDelay.AutoSize = true;
            this.CustomLabelPDpiFragDelay.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelPDpiFragDelay.Border = false;
            this.CustomLabelPDpiFragDelay.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelPDpiFragDelay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelPDpiFragDelay.ForeColor = System.Drawing.Color.White;
            this.CustomLabelPDpiFragDelay.Location = new System.Drawing.Point(240, 220);
            this.CustomLabelPDpiFragDelay.Name = "CustomLabelPDpiFragDelay";
            this.CustomLabelPDpiFragDelay.RoundedCorners = 0;
            this.CustomLabelPDpiFragDelay.Size = new System.Drawing.Size(119, 15);
            this.CustomLabelPDpiFragDelay.TabIndex = 35;
            this.CustomLabelPDpiFragDelay.Text = "Fragment delay (ms):";
            // 
            // CustomCheckBoxPDpiDontChunkBigData
            // 
            this.CustomCheckBoxPDpiDontChunkBigData.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxPDpiDontChunkBigData.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxPDpiDontChunkBigData.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxPDpiDontChunkBigData.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxPDpiDontChunkBigData.Location = new System.Drawing.Point(50, 220);
            this.CustomCheckBoxPDpiDontChunkBigData.Name = "CustomCheckBoxPDpiDontChunkBigData";
            this.CustomCheckBoxPDpiDontChunkBigData.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxPDpiDontChunkBigData.Size = new System.Drawing.Size(182, 17);
            this.CustomCheckBoxPDpiDontChunkBigData.TabIndex = 34;
            this.CustomCheckBoxPDpiDontChunkBigData.Text = "Don\'t chunk the biggest data.";
            this.CustomCheckBoxPDpiDontChunkBigData.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxPDpiFragModeRandom
            // 
            this.CustomCheckBoxPDpiFragModeRandom.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxPDpiFragModeRandom.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxPDpiFragModeRandom.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxPDpiFragModeRandom.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxPDpiFragModeRandom.Location = new System.Drawing.Point(285, 180);
            this.CustomCheckBoxPDpiFragModeRandom.Name = "CustomCheckBoxPDpiFragModeRandom";
            this.CustomCheckBoxPDpiFragModeRandom.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxPDpiFragModeRandom.Size = new System.Drawing.Size(101, 17);
            this.CustomCheckBoxPDpiFragModeRandom.TabIndex = 33;
            this.CustomCheckBoxPDpiFragModeRandom.Text = "Random mode";
            this.CustomCheckBoxPDpiFragModeRandom.UseVisualStyleBackColor = false;
            // 
            // CustomButtonSetProxy
            // 
            this.CustomButtonSetProxy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonSetProxy.AutoSize = true;
            this.CustomButtonSetProxy.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonSetProxy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonSetProxy.Location = new System.Drawing.Point(143, 303);
            this.CustomButtonSetProxy.Name = "CustomButtonSetProxy";
            this.CustomButtonSetProxy.RoundedCorners = 0;
            this.CustomButtonSetProxy.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonSetProxy.Size = new System.Drawing.Size(71, 27);
            this.CustomButtonSetProxy.TabIndex = 32;
            this.CustomButtonSetProxy.Text = "Set/Unset";
            this.CustomButtonSetProxy.UseVisualStyleBackColor = true;
            this.CustomButtonSetProxy.Click += new System.EventHandler(this.CustomButtonSetProxy_Click);
            // 
            // CustomNumericUpDownPDpiDataLength
            // 
            this.CustomNumericUpDownPDpiDataLength.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownPDpiDataLength.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownPDpiDataLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownPDpiDataLength.Location = new System.Drawing.Point(159, 138);
            this.CustomNumericUpDownPDpiDataLength.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.CustomNumericUpDownPDpiDataLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownPDpiDataLength.Name = "CustomNumericUpDownPDpiDataLength";
            this.CustomNumericUpDownPDpiDataLength.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownPDpiDataLength.TabIndex = 31;
            this.CustomNumericUpDownPDpiDataLength.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // CustomLabelPDpiDpiInfo1
            // 
            this.CustomLabelPDpiDpiInfo1.AutoSize = true;
            this.CustomLabelPDpiDpiInfo1.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelPDpiDpiInfo1.Border = false;
            this.CustomLabelPDpiDpiInfo1.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelPDpiDpiInfo1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelPDpiDpiInfo1.ForeColor = System.Drawing.Color.White;
            this.CustomLabelPDpiDpiInfo1.Location = new System.Drawing.Point(50, 140);
            this.CustomLabelPDpiDpiInfo1.Name = "CustomLabelPDpiDpiInfo1";
            this.CustomLabelPDpiDpiInfo1.RoundedCorners = 0;
            this.CustomLabelPDpiDpiInfo1.Size = new System.Drawing.Size(107, 15);
            this.CustomLabelPDpiDpiInfo1.TabIndex = 30;
            this.CustomLabelPDpiDpiInfo1.Text = "If data length is <=";
            // 
            // CustomNumericUpDownPDpiFragmentSize
            // 
            this.CustomNumericUpDownPDpiFragmentSize.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownPDpiFragmentSize.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownPDpiFragmentSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownPDpiFragmentSize.Location = new System.Drawing.Point(333, 138);
            this.CustomNumericUpDownPDpiFragmentSize.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.CustomNumericUpDownPDpiFragmentSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownPDpiFragmentSize.Name = "CustomNumericUpDownPDpiFragmentSize";
            this.CustomNumericUpDownPDpiFragmentSize.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownPDpiFragmentSize.TabIndex = 28;
            this.CustomNumericUpDownPDpiFragmentSize.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // CustomLabelPDpiDpiInfo2
            // 
            this.CustomLabelPDpiDpiInfo2.AutoSize = true;
            this.CustomLabelPDpiDpiInfo2.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelPDpiDpiInfo2.Border = false;
            this.CustomLabelPDpiDpiInfo2.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelPDpiDpiInfo2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelPDpiDpiInfo2.ForeColor = System.Drawing.Color.White;
            this.CustomLabelPDpiDpiInfo2.Location = new System.Drawing.Point(218, 140);
            this.CustomLabelPDpiDpiInfo2.Name = "CustomLabelPDpiDpiInfo2";
            this.CustomLabelPDpiDpiInfo2.RoundedCorners = 0;
            this.CustomLabelPDpiDpiInfo2.Size = new System.Drawing.Size(110, 15);
            this.CustomLabelPDpiDpiInfo2.TabIndex = 27;
            this.CustomLabelPDpiDpiInfo2.Text = "set fragment size to";
            // 
            // CustomLabelShareSeparator1
            // 
            this.CustomLabelShareSeparator1.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelShareSeparator1.Border = true;
            this.CustomLabelShareSeparator1.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelShareSeparator1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelShareSeparator1.ForeColor = System.Drawing.Color.White;
            this.CustomLabelShareSeparator1.Location = new System.Drawing.Point(20, 89);
            this.CustomLabelShareSeparator1.Name = "CustomLabelShareSeparator1";
            this.CustomLabelShareSeparator1.RoundedCorners = 0;
            this.CustomLabelShareSeparator1.Size = new System.Drawing.Size(609, 1);
            this.CustomLabelShareSeparator1.TabIndex = 24;
            // 
            // CustomCheckBoxHTTPProxyEventShowRequest
            // 
            this.CustomCheckBoxHTTPProxyEventShowRequest.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxHTTPProxyEventShowRequest.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxHTTPProxyEventShowRequest.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxHTTPProxyEventShowRequest.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxHTTPProxyEventShowRequest.Location = new System.Drawing.Point(455, 30);
            this.CustomCheckBoxHTTPProxyEventShowRequest.Name = "CustomCheckBoxHTTPProxyEventShowRequest";
            this.CustomCheckBoxHTTPProxyEventShowRequest.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxHTTPProxyEventShowRequest.Size = new System.Drawing.Size(135, 17);
            this.CustomCheckBoxHTTPProxyEventShowRequest.TabIndex = 23;
            this.CustomCheckBoxHTTPProxyEventShowRequest.Text = "Write requests to log";
            this.CustomCheckBoxHTTPProxyEventShowRequest.UseVisualStyleBackColor = false;
            // 
            // CustomNumericUpDownPDpiFragmentChunks
            // 
            this.CustomNumericUpDownPDpiFragmentChunks.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownPDpiFragmentChunks.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownPDpiFragmentChunks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownPDpiFragmentChunks.Location = new System.Drawing.Point(218, 178);
            this.CustomNumericUpDownPDpiFragmentChunks.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.CustomNumericUpDownPDpiFragmentChunks.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownPDpiFragmentChunks.Name = "CustomNumericUpDownPDpiFragmentChunks";
            this.CustomNumericUpDownPDpiFragmentChunks.Size = new System.Drawing.Size(47, 23);
            this.CustomNumericUpDownPDpiFragmentChunks.TabIndex = 19;
            this.CustomNumericUpDownPDpiFragmentChunks.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // CustomLabelPDpiDpiInfo3
            // 
            this.CustomLabelPDpiDpiInfo3.AutoSize = true;
            this.CustomLabelPDpiDpiInfo3.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelPDpiDpiInfo3.Border = false;
            this.CustomLabelPDpiDpiInfo3.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelPDpiDpiInfo3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelPDpiDpiInfo3.ForeColor = System.Drawing.Color.White;
            this.CustomLabelPDpiDpiInfo3.Location = new System.Drawing.Point(50, 180);
            this.CustomLabelPDpiDpiInfo3.Name = "CustomLabelPDpiDpiInfo3";
            this.CustomLabelPDpiDpiInfo3.RoundedCorners = 0;
            this.CustomLabelPDpiDpiInfo3.Size = new System.Drawing.Size(162, 15);
            this.CustomLabelPDpiDpiInfo3.TabIndex = 18;
            this.CustomLabelPDpiDpiInfo3.Text = "else chunk data into n pieces:";
            // 
            // CustomCheckBoxPDpiEnableDpiBypass
            // 
            this.CustomCheckBoxPDpiEnableDpiBypass.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxPDpiEnableDpiBypass.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxPDpiEnableDpiBypass.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxPDpiEnableDpiBypass.Checked = true;
            this.CustomCheckBoxPDpiEnableDpiBypass.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxPDpiEnableDpiBypass.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxPDpiEnableDpiBypass.Location = new System.Drawing.Point(25, 110);
            this.CustomCheckBoxPDpiEnableDpiBypass.Name = "CustomCheckBoxPDpiEnableDpiBypass";
            this.CustomCheckBoxPDpiEnableDpiBypass.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxPDpiEnableDpiBypass.Size = new System.Drawing.Size(120, 17);
            this.CustomCheckBoxPDpiEnableDpiBypass.TabIndex = 17;
            this.CustomCheckBoxPDpiEnableDpiBypass.Text = "Enable DPI bypass";
            this.CustomCheckBoxPDpiEnableDpiBypass.UseVisualStyleBackColor = false;
            // 
            // CustomButtonShare
            // 
            this.CustomButtonShare.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonShare.AutoSize = true;
            this.CustomButtonShare.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonShare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonShare.Location = new System.Drawing.Point(40, 303);
            this.CustomButtonShare.Name = "CustomButtonShare";
            this.CustomButtonShare.RoundedCorners = 0;
            this.CustomButtonShare.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonShare.Size = new System.Drawing.Size(97, 27);
            this.CustomButtonShare.TabIndex = 16;
            this.CustomButtonShare.Text = "Enable/Disable";
            this.CustomButtonShare.UseVisualStyleBackColor = true;
            this.CustomButtonShare.Click += new System.EventHandler(this.CustomButtonShare_Click);
            // 
            // CustomLabelHTTPProxyPort
            // 
            this.CustomLabelHTTPProxyPort.AutoSize = true;
            this.CustomLabelHTTPProxyPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelHTTPProxyPort.Border = false;
            this.CustomLabelHTTPProxyPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelHTTPProxyPort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelHTTPProxyPort.ForeColor = System.Drawing.Color.White;
            this.CustomLabelHTTPProxyPort.Location = new System.Drawing.Point(25, 55);
            this.CustomLabelHTTPProxyPort.Name = "CustomLabelHTTPProxyPort";
            this.CustomLabelHTTPProxyPort.RoundedCorners = 0;
            this.CustomLabelHTTPProxyPort.Size = new System.Drawing.Size(99, 15);
            this.CustomLabelHTTPProxyPort.TabIndex = 15;
            this.CustomLabelHTTPProxyPort.Text = "HTTP Proxy. Port:";
            // 
            // CustomLabelShareInfo
            // 
            this.CustomLabelShareInfo.AutoSize = true;
            this.CustomLabelShareInfo.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelShareInfo.Border = false;
            this.CustomLabelShareInfo.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelShareInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelShareInfo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomLabelShareInfo.ForeColor = System.Drawing.Color.White;
            this.CustomLabelShareInfo.Location = new System.Drawing.Point(25, 10);
            this.CustomLabelShareInfo.Name = "CustomLabelShareInfo";
            this.CustomLabelShareInfo.RoundedCorners = 0;
            this.CustomLabelShareInfo.Size = new System.Drawing.Size(421, 21);
            this.CustomLabelShareInfo.TabIndex = 14;
            this.CustomLabelShareInfo.Text = "Share to other devices on the same network. (Experimental)";
            // 
            // CustomNumericUpDownHTTPProxyPort
            // 
            this.CustomNumericUpDownHTTPProxyPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownHTTPProxyPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownHTTPProxyPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownHTTPProxyPort.Location = new System.Drawing.Point(130, 53);
            this.CustomNumericUpDownHTTPProxyPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.CustomNumericUpDownHTTPProxyPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownHTTPProxyPort.Name = "CustomNumericUpDownHTTPProxyPort";
            this.CustomNumericUpDownHTTPProxyPort.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownHTTPProxyPort.TabIndex = 12;
            this.CustomNumericUpDownHTTPProxyPort.Value = new decimal(new int[] {
            8080,
            0,
            0,
            0});
            // 
            // TabPageDPI
            // 
            this.TabPageDPI.BackColor = System.Drawing.Color.Transparent;
            this.TabPageDPI.Controls.Add(this.CustomTabControlDPIBasicAdvanced);
            this.TabPageDPI.Location = new System.Drawing.Point(4, 25);
            this.TabPageDPI.Name = "TabPageDPI";
            this.TabPageDPI.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageDPI.Size = new System.Drawing.Size(678, 336);
            this.TabPageDPI.TabIndex = 2;
            this.TabPageDPI.Tag = 4;
            this.TabPageDPI.Text = "5. GoodbyeDPI";
            // 
            // CustomTabControlDPIBasicAdvanced
            // 
            this.CustomTabControlDPIBasicAdvanced.BorderColor = System.Drawing.Color.Blue;
            this.CustomTabControlDPIBasicAdvanced.Controls.Add(this.TabPageDPIBasic);
            this.CustomTabControlDPIBasicAdvanced.Controls.Add(this.TabPageDPIAdvanced);
            this.CustomTabControlDPIBasicAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomTabControlDPIBasicAdvanced.HideTabHeader = false;
            this.CustomTabControlDPIBasicAdvanced.ItemSize = new System.Drawing.Size(90, 21);
            this.CustomTabControlDPIBasicAdvanced.Location = new System.Drawing.Point(3, 3);
            this.CustomTabControlDPIBasicAdvanced.Name = "CustomTabControlDPIBasicAdvanced";
            this.CustomTabControlDPIBasicAdvanced.SelectedIndex = 0;
            this.CustomTabControlDPIBasicAdvanced.Size = new System.Drawing.Size(672, 330);
            this.CustomTabControlDPIBasicAdvanced.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.CustomTabControlDPIBasicAdvanced.TabIndex = 13;
            this.CustomTabControlDPIBasicAdvanced.Tag = 1;
            // 
            // TabPageDPIBasic
            // 
            this.TabPageDPIBasic.BackColor = System.Drawing.Color.Transparent;
            this.TabPageDPIBasic.Controls.Add(this.CustomLabelInfoDPIModes);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIMode6);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIMode5);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIMode4);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIMode3);
            this.TabPageDPIBasic.Controls.Add(this.CustomLabelDPIModesGoodbyeDPI);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIMode2);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIMode1);
            this.TabPageDPIBasic.Controls.Add(this.CustomButtonDPIBasicDeactivate);
            this.TabPageDPIBasic.Controls.Add(this.CustomButtonDPIBasicActivate);
            this.TabPageDPIBasic.Controls.Add(this.CustomNumericUpDownSSLFragmentSize);
            this.TabPageDPIBasic.Controls.Add(this.CustomLabelDPIModes);
            this.TabPageDPIBasic.Controls.Add(this.CustomLabelSSLFragmentSize);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIModeLight);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIModeMedium);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIModeExtreme);
            this.TabPageDPIBasic.Controls.Add(this.CustomRadioButtonDPIModeHigh);
            this.TabPageDPIBasic.Location = new System.Drawing.Point(4, 25);
            this.TabPageDPIBasic.Name = "TabPageDPIBasic";
            this.TabPageDPIBasic.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageDPIBasic.Size = new System.Drawing.Size(664, 301);
            this.TabPageDPIBasic.TabIndex = 0;
            this.TabPageDPIBasic.Tag = 0;
            this.TabPageDPIBasic.Text = "Basic";
            // 
            // CustomLabelInfoDPIModes
            // 
            this.CustomLabelInfoDPIModes.AutoSize = true;
            this.CustomLabelInfoDPIModes.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelInfoDPIModes.Border = false;
            this.CustomLabelInfoDPIModes.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelInfoDPIModes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelInfoDPIModes.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CustomLabelInfoDPIModes.ForeColor = System.Drawing.Color.White;
            this.CustomLabelInfoDPIModes.Location = new System.Drawing.Point(25, 15);
            this.CustomLabelInfoDPIModes.Name = "CustomLabelInfoDPIModes";
            this.CustomLabelInfoDPIModes.RoundedCorners = 0;
            this.CustomLabelInfoDPIModes.Size = new System.Drawing.Size(147, 38);
            this.CustomLabelInfoDPIModes.TabIndex = 20;
            this.CustomLabelInfoDPIModes.Text = "Light: MTN-AST-ASK\r\nMedium: MCI-SHT";
            // 
            // CustomRadioButtonDPIMode6
            // 
            this.CustomRadioButtonDPIMode6.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIMode6.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode6.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode6.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIMode6.Location = new System.Drawing.Point(370, 155);
            this.CustomRadioButtonDPIMode6.Name = "CustomRadioButtonDPIMode6";
            this.CustomRadioButtonDPIMode6.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIMode6.Size = new System.Drawing.Size(63, 17);
            this.CustomRadioButtonDPIMode6.TabIndex = 19;
            this.CustomRadioButtonDPIMode6.Text = "Mode 6";
            this.CustomRadioButtonDPIMode6.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonDPIMode5
            // 
            this.CustomRadioButtonDPIMode5.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIMode5.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode5.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode5.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIMode5.Location = new System.Drawing.Point(301, 155);
            this.CustomRadioButtonDPIMode5.Name = "CustomRadioButtonDPIMode5";
            this.CustomRadioButtonDPIMode5.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIMode5.Size = new System.Drawing.Size(63, 17);
            this.CustomRadioButtonDPIMode5.TabIndex = 18;
            this.CustomRadioButtonDPIMode5.Text = "Mode 5";
            this.CustomRadioButtonDPIMode5.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonDPIMode4
            // 
            this.CustomRadioButtonDPIMode4.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIMode4.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode4.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode4.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIMode4.Location = new System.Drawing.Point(232, 155);
            this.CustomRadioButtonDPIMode4.Name = "CustomRadioButtonDPIMode4";
            this.CustomRadioButtonDPIMode4.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIMode4.Size = new System.Drawing.Size(63, 17);
            this.CustomRadioButtonDPIMode4.TabIndex = 17;
            this.CustomRadioButtonDPIMode4.Text = "Mode 4";
            this.CustomRadioButtonDPIMode4.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonDPIMode3
            // 
            this.CustomRadioButtonDPIMode3.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIMode3.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode3.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode3.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIMode3.Location = new System.Drawing.Point(163, 155);
            this.CustomRadioButtonDPIMode3.Name = "CustomRadioButtonDPIMode3";
            this.CustomRadioButtonDPIMode3.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIMode3.Size = new System.Drawing.Size(63, 17);
            this.CustomRadioButtonDPIMode3.TabIndex = 16;
            this.CustomRadioButtonDPIMode3.Text = "Mode 3";
            this.CustomRadioButtonDPIMode3.UseVisualStyleBackColor = false;
            // 
            // CustomLabelDPIModesGoodbyeDPI
            // 
            this.CustomLabelDPIModesGoodbyeDPI.AutoSize = true;
            this.CustomLabelDPIModesGoodbyeDPI.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelDPIModesGoodbyeDPI.Border = false;
            this.CustomLabelDPIModesGoodbyeDPI.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelDPIModesGoodbyeDPI.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelDPIModesGoodbyeDPI.ForeColor = System.Drawing.Color.White;
            this.CustomLabelDPIModesGoodbyeDPI.Location = new System.Drawing.Point(25, 130);
            this.CustomLabelDPIModesGoodbyeDPI.Name = "CustomLabelDPIModesGoodbyeDPI";
            this.CustomLabelDPIModesGoodbyeDPI.RoundedCorners = 0;
            this.CustomLabelDPIModesGoodbyeDPI.Size = new System.Drawing.Size(118, 15);
            this.CustomLabelDPIModesGoodbyeDPI.TabIndex = 15;
            this.CustomLabelDPIModesGoodbyeDPI.Text = "Goodbye DPI modes:";
            // 
            // CustomRadioButtonDPIMode2
            // 
            this.CustomRadioButtonDPIMode2.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIMode2.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode2.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode2.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIMode2.Location = new System.Drawing.Point(94, 155);
            this.CustomRadioButtonDPIMode2.Name = "CustomRadioButtonDPIMode2";
            this.CustomRadioButtonDPIMode2.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIMode2.Size = new System.Drawing.Size(63, 17);
            this.CustomRadioButtonDPIMode2.TabIndex = 14;
            this.CustomRadioButtonDPIMode2.Text = "Mode 2";
            this.CustomRadioButtonDPIMode2.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonDPIMode1
            // 
            this.CustomRadioButtonDPIMode1.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonDPIMode1.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode1.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonDPIMode1.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonDPIMode1.Location = new System.Drawing.Point(25, 155);
            this.CustomRadioButtonDPIMode1.Name = "CustomRadioButtonDPIMode1";
            this.CustomRadioButtonDPIMode1.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonDPIMode1.Size = new System.Drawing.Size(63, 17);
            this.CustomRadioButtonDPIMode1.TabIndex = 13;
            this.CustomRadioButtonDPIMode1.Text = "Mode 1";
            this.CustomRadioButtonDPIMode1.UseVisualStyleBackColor = false;
            // 
            // CustomButtonDPIBasicDeactivate
            // 
            this.CustomButtonDPIBasicDeactivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonDPIBasicDeactivate.AutoSize = true;
            this.CustomButtonDPIBasicDeactivate.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonDPIBasicDeactivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonDPIBasicDeactivate.Location = new System.Drawing.Point(172, 268);
            this.CustomButtonDPIBasicDeactivate.Name = "CustomButtonDPIBasicDeactivate";
            this.CustomButtonDPIBasicDeactivate.RoundedCorners = 0;
            this.CustomButtonDPIBasicDeactivate.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonDPIBasicDeactivate.Size = new System.Drawing.Size(74, 27);
            this.CustomButtonDPIBasicDeactivate.TabIndex = 12;
            this.CustomButtonDPIBasicDeactivate.Text = "Deactivate";
            this.CustomButtonDPIBasicDeactivate.UseVisualStyleBackColor = true;
            this.CustomButtonDPIBasicDeactivate.Click += new System.EventHandler(this.CustomButtonDPIBasicDeactivate_Click);
            // 
            // CustomButtonDPIBasicActivate
            // 
            this.CustomButtonDPIBasicActivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonDPIBasicActivate.AutoSize = true;
            this.CustomButtonDPIBasicActivate.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonDPIBasicActivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonDPIBasicActivate.Location = new System.Drawing.Point(45, 268);
            this.CustomButtonDPIBasicActivate.Name = "CustomButtonDPIBasicActivate";
            this.CustomButtonDPIBasicActivate.RoundedCorners = 0;
            this.CustomButtonDPIBasicActivate.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonDPIBasicActivate.Size = new System.Drawing.Size(121, 27);
            this.CustomButtonDPIBasicActivate.TabIndex = 11;
            this.CustomButtonDPIBasicActivate.Text = "Activate/Reactivate";
            this.CustomButtonDPIBasicActivate.UseVisualStyleBackColor = true;
            this.CustomButtonDPIBasicActivate.Click += new System.EventHandler(this.CustomButtonDPIBasic_Click);
            // 
            // TabPageDPIAdvanced
            // 
            this.TabPageDPIAdvanced.BackColor = System.Drawing.Color.Transparent;
            this.TabPageDPIAdvanced.Controls.Add(this.CustomTextBoxDPIAdvAutoTTL);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomNumericUpDownDPIAdvMaxPayload);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomNumericUpDownDPIAdvMinTTL);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomNumericUpDownDPIAdvSetTTL);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomNumericUpDownDPIAdvPort);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomButtonDPIAdvDeactivate);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomButtonDPIAdvActivate);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomButtonDPIAdvBlacklist);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvBlacklist);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvMaxPayload);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvReverseFrag);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvNativeFrag);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvWrongSeq);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvWrongChksum);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvMinTTL);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvAutoTTL);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvSetTTL);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvAllowNoSNI);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomTextBoxDPIAdvIpId);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvIpId);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvPort);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvW);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvA);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomNumericUpDownDPIAdvE);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvE);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvN);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomNumericUpDownDPIAdvK);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvK);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomNumericUpDownDPIAdvF);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvF);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvM);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvS);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvR);
            this.TabPageDPIAdvanced.Controls.Add(this.CustomCheckBoxDPIAdvP);
            this.TabPageDPIAdvanced.Location = new System.Drawing.Point(4, 25);
            this.TabPageDPIAdvanced.Name = "TabPageDPIAdvanced";
            this.TabPageDPIAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageDPIAdvanced.Size = new System.Drawing.Size(664, 301);
            this.TabPageDPIAdvanced.TabIndex = 1;
            this.TabPageDPIAdvanced.Tag = 1;
            this.TabPageDPIAdvanced.Text = "Advanced";
            // 
            // CustomTextBoxDPIAdvAutoTTL
            // 
            this.CustomTextBoxDPIAdvAutoTTL.AcceptsReturn = false;
            this.CustomTextBoxDPIAdvAutoTTL.AcceptsTab = false;
            this.CustomTextBoxDPIAdvAutoTTL.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxDPIAdvAutoTTL.Border = true;
            this.CustomTextBoxDPIAdvAutoTTL.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxDPIAdvAutoTTL.BorderSize = 1;
            this.CustomTextBoxDPIAdvAutoTTL.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxDPIAdvAutoTTL.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxDPIAdvAutoTTL.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxDPIAdvAutoTTL.HideSelection = true;
            this.CustomTextBoxDPIAdvAutoTTL.Location = new System.Drawing.Point(393, 93);
            this.CustomTextBoxDPIAdvAutoTTL.MaxLength = 32767;
            this.CustomTextBoxDPIAdvAutoTTL.Multiline = false;
            this.CustomTextBoxDPIAdvAutoTTL.Name = "CustomTextBoxDPIAdvAutoTTL";
            this.CustomTextBoxDPIAdvAutoTTL.ReadOnly = false;
            this.CustomTextBoxDPIAdvAutoTTL.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxDPIAdvAutoTTL.ShortcutsEnabled = true;
            this.CustomTextBoxDPIAdvAutoTTL.Size = new System.Drawing.Size(53, 23);
            this.CustomTextBoxDPIAdvAutoTTL.TabIndex = 0;
            this.CustomTextBoxDPIAdvAutoTTL.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxDPIAdvAutoTTL.Texts = "1-4-10";
            this.CustomTextBoxDPIAdvAutoTTL.UnderlinedStyle = false;
            this.CustomTextBoxDPIAdvAutoTTL.UsePasswordChar = false;
            this.CustomTextBoxDPIAdvAutoTTL.WordWrap = true;
            // 
            // CustomNumericUpDownDPIAdvMaxPayload
            // 
            this.CustomNumericUpDownDPIAdvMaxPayload.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownDPIAdvMaxPayload.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownDPIAdvMaxPayload.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownDPIAdvMaxPayload.Location = new System.Drawing.Point(106, 153);
            this.CustomNumericUpDownDPIAdvMaxPayload.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvMaxPayload.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvMaxPayload.Name = "CustomNumericUpDownDPIAdvMaxPayload";
            this.CustomNumericUpDownDPIAdvMaxPayload.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownDPIAdvMaxPayload.TabIndex = 36;
            this.CustomNumericUpDownDPIAdvMaxPayload.Value = new decimal(new int[] {
            1200,
            0,
            0,
            0});
            // 
            // CustomNumericUpDownDPIAdvMinTTL
            // 
            this.CustomNumericUpDownDPIAdvMinTTL.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownDPIAdvMinTTL.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownDPIAdvMinTTL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownDPIAdvMinTTL.Location = new System.Drawing.Point(539, 93);
            this.CustomNumericUpDownDPIAdvMinTTL.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvMinTTL.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvMinTTL.Name = "CustomNumericUpDownDPIAdvMinTTL";
            this.CustomNumericUpDownDPIAdvMinTTL.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownDPIAdvMinTTL.TabIndex = 35;
            this.CustomNumericUpDownDPIAdvMinTTL.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // CustomNumericUpDownDPIAdvSetTTL
            // 
            this.CustomNumericUpDownDPIAdvSetTTL.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownDPIAdvSetTTL.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownDPIAdvSetTTL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownDPIAdvSetTTL.Location = new System.Drawing.Point(230, 93);
            this.CustomNumericUpDownDPIAdvSetTTL.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvSetTTL.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvSetTTL.Name = "CustomNumericUpDownDPIAdvSetTTL";
            this.CustomNumericUpDownDPIAdvSetTTL.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownDPIAdvSetTTL.TabIndex = 34;
            this.CustomNumericUpDownDPIAdvSetTTL.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // CustomNumericUpDownDPIAdvPort
            // 
            this.CustomNumericUpDownDPIAdvPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownDPIAdvPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownDPIAdvPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownDPIAdvPort.Location = new System.Drawing.Point(376, 63);
            this.CustomNumericUpDownDPIAdvPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvPort.Name = "CustomNumericUpDownDPIAdvPort";
            this.CustomNumericUpDownDPIAdvPort.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownDPIAdvPort.TabIndex = 33;
            this.CustomNumericUpDownDPIAdvPort.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // CustomButtonDPIAdvDeactivate
            // 
            this.CustomButtonDPIAdvDeactivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonDPIAdvDeactivate.AutoSize = true;
            this.CustomButtonDPIAdvDeactivate.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonDPIAdvDeactivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonDPIAdvDeactivate.Location = new System.Drawing.Point(172, 268);
            this.CustomButtonDPIAdvDeactivate.Name = "CustomButtonDPIAdvDeactivate";
            this.CustomButtonDPIAdvDeactivate.RoundedCorners = 0;
            this.CustomButtonDPIAdvDeactivate.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonDPIAdvDeactivate.Size = new System.Drawing.Size(74, 27);
            this.CustomButtonDPIAdvDeactivate.TabIndex = 32;
            this.CustomButtonDPIAdvDeactivate.Text = "Deactivate";
            this.CustomButtonDPIAdvDeactivate.UseVisualStyleBackColor = true;
            this.CustomButtonDPIAdvDeactivate.Click += new System.EventHandler(this.CustomButtonDPIAdvDeactivate_Click);
            // 
            // CustomButtonDPIAdvActivate
            // 
            this.CustomButtonDPIAdvActivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonDPIAdvActivate.AutoSize = true;
            this.CustomButtonDPIAdvActivate.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonDPIAdvActivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonDPIAdvActivate.Location = new System.Drawing.Point(45, 268);
            this.CustomButtonDPIAdvActivate.Name = "CustomButtonDPIAdvActivate";
            this.CustomButtonDPIAdvActivate.RoundedCorners = 0;
            this.CustomButtonDPIAdvActivate.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonDPIAdvActivate.Size = new System.Drawing.Size(121, 27);
            this.CustomButtonDPIAdvActivate.TabIndex = 31;
            this.CustomButtonDPIAdvActivate.Text = "Activate/Reactivate";
            this.CustomButtonDPIAdvActivate.UseVisualStyleBackColor = true;
            this.CustomButtonDPIAdvActivate.Click += new System.EventHandler(this.CustomButtonDPIAdvActivate_Click);
            // 
            // CustomButtonDPIAdvBlacklist
            // 
            this.CustomButtonDPIAdvBlacklist.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonDPIAdvBlacklist.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonDPIAdvBlacklist.Location = new System.Drawing.Point(241, 153);
            this.CustomButtonDPIAdvBlacklist.Name = "CustomButtonDPIAdvBlacklist";
            this.CustomButtonDPIAdvBlacklist.RoundedCorners = 0;
            this.CustomButtonDPIAdvBlacklist.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonDPIAdvBlacklist.Size = new System.Drawing.Size(57, 25);
            this.CustomButtonDPIAdvBlacklist.TabIndex = 30;
            this.CustomButtonDPIAdvBlacklist.Text = "Edit";
            this.CustomButtonDPIAdvBlacklist.UseVisualStyleBackColor = true;
            this.CustomButtonDPIAdvBlacklist.Click += new System.EventHandler(this.CustomButtonDPIAdvBlacklist_Click);
            // 
            // CustomCheckBoxDPIAdvBlacklist
            // 
            this.CustomCheckBoxDPIAdvBlacklist.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvBlacklist.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvBlacklist.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvBlacklist.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvBlacklist.Location = new System.Drawing.Point(165, 155);
            this.CustomCheckBoxDPIAdvBlacklist.Name = "CustomCheckBoxDPIAdvBlacklist";
            this.CustomCheckBoxDPIAdvBlacklist.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvBlacklist.Size = new System.Drawing.Size(74, 17);
            this.CustomCheckBoxDPIAdvBlacklist.TabIndex = 29;
            this.CustomCheckBoxDPIAdvBlacklist.Text = "--blacklist";
            this.CustomCheckBoxDPIAdvBlacklist.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvMaxPayload
            // 
            this.CustomCheckBoxDPIAdvMaxPayload.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvMaxPayload.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvMaxPayload.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvMaxPayload.Checked = true;
            this.CustomCheckBoxDPIAdvMaxPayload.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxDPIAdvMaxPayload.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvMaxPayload.Location = new System.Drawing.Point(5, 155);
            this.CustomCheckBoxDPIAdvMaxPayload.Name = "CustomCheckBoxDPIAdvMaxPayload";
            this.CustomCheckBoxDPIAdvMaxPayload.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvMaxPayload.Size = new System.Drawing.Size(101, 17);
            this.CustomCheckBoxDPIAdvMaxPayload.TabIndex = 27;
            this.CustomCheckBoxDPIAdvMaxPayload.Text = "--max-payload";
            this.CustomCheckBoxDPIAdvMaxPayload.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvReverseFrag
            // 
            this.CustomCheckBoxDPIAdvReverseFrag.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvReverseFrag.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvReverseFrag.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvReverseFrag.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvReverseFrag.Location = new System.Drawing.Point(470, 125);
            this.CustomCheckBoxDPIAdvReverseFrag.Name = "CustomCheckBoxDPIAdvReverseFrag";
            this.CustomCheckBoxDPIAdvReverseFrag.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvReverseFrag.Size = new System.Drawing.Size(96, 17);
            this.CustomCheckBoxDPIAdvReverseFrag.TabIndex = 26;
            this.CustomCheckBoxDPIAdvReverseFrag.Text = "--reverse-frag";
            this.CustomCheckBoxDPIAdvReverseFrag.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvNativeFrag
            // 
            this.CustomCheckBoxDPIAdvNativeFrag.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvNativeFrag.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvNativeFrag.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvNativeFrag.Checked = true;
            this.CustomCheckBoxDPIAdvNativeFrag.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxDPIAdvNativeFrag.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvNativeFrag.Location = new System.Drawing.Point(320, 125);
            this.CustomCheckBoxDPIAdvNativeFrag.Name = "CustomCheckBoxDPIAdvNativeFrag";
            this.CustomCheckBoxDPIAdvNativeFrag.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvNativeFrag.Size = new System.Drawing.Size(90, 17);
            this.CustomCheckBoxDPIAdvNativeFrag.TabIndex = 25;
            this.CustomCheckBoxDPIAdvNativeFrag.Text = "--native-frag";
            this.CustomCheckBoxDPIAdvNativeFrag.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvWrongSeq
            // 
            this.CustomCheckBoxDPIAdvWrongSeq.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvWrongSeq.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvWrongSeq.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvWrongSeq.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvWrongSeq.Location = new System.Drawing.Point(165, 125);
            this.CustomCheckBoxDPIAdvWrongSeq.Name = "CustomCheckBoxDPIAdvWrongSeq";
            this.CustomCheckBoxDPIAdvWrongSeq.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvWrongSeq.Size = new System.Drawing.Size(89, 17);
            this.CustomCheckBoxDPIAdvWrongSeq.TabIndex = 24;
            this.CustomCheckBoxDPIAdvWrongSeq.Text = "--wrong-seq";
            this.CustomCheckBoxDPIAdvWrongSeq.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvWrongChksum
            // 
            this.CustomCheckBoxDPIAdvWrongChksum.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvWrongChksum.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvWrongChksum.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvWrongChksum.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvWrongChksum.Location = new System.Drawing.Point(5, 125);
            this.CustomCheckBoxDPIAdvWrongChksum.Name = "CustomCheckBoxDPIAdvWrongChksum";
            this.CustomCheckBoxDPIAdvWrongChksum.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvWrongChksum.Size = new System.Drawing.Size(112, 17);
            this.CustomCheckBoxDPIAdvWrongChksum.TabIndex = 23;
            this.CustomCheckBoxDPIAdvWrongChksum.Text = "--wrong-chksum";
            this.CustomCheckBoxDPIAdvWrongChksum.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvMinTTL
            // 
            this.CustomCheckBoxDPIAdvMinTTL.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvMinTTL.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvMinTTL.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvMinTTL.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvMinTTL.Location = new System.Drawing.Point(470, 95);
            this.CustomCheckBoxDPIAdvMinTTL.Name = "CustomCheckBoxDPIAdvMinTTL";
            this.CustomCheckBoxDPIAdvMinTTL.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvMinTTL.Size = new System.Drawing.Size(67, 17);
            this.CustomCheckBoxDPIAdvMinTTL.TabIndex = 21;
            this.CustomCheckBoxDPIAdvMinTTL.Text = "--min-ttl";
            this.CustomCheckBoxDPIAdvMinTTL.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvAutoTTL
            // 
            this.CustomCheckBoxDPIAdvAutoTTL.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvAutoTTL.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvAutoTTL.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvAutoTTL.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvAutoTTL.Location = new System.Drawing.Point(320, 95);
            this.CustomCheckBoxDPIAdvAutoTTL.Name = "CustomCheckBoxDPIAdvAutoTTL";
            this.CustomCheckBoxDPIAdvAutoTTL.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvAutoTTL.Size = new System.Drawing.Size(71, 17);
            this.CustomCheckBoxDPIAdvAutoTTL.TabIndex = 20;
            this.CustomCheckBoxDPIAdvAutoTTL.Text = "--auto-ttl";
            this.CustomCheckBoxDPIAdvAutoTTL.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvSetTTL
            // 
            this.CustomCheckBoxDPIAdvSetTTL.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvSetTTL.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvSetTTL.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvSetTTL.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvSetTTL.Location = new System.Drawing.Point(165, 95);
            this.CustomCheckBoxDPIAdvSetTTL.Name = "CustomCheckBoxDPIAdvSetTTL";
            this.CustomCheckBoxDPIAdvSetTTL.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvSetTTL.Size = new System.Drawing.Size(63, 17);
            this.CustomCheckBoxDPIAdvSetTTL.TabIndex = 18;
            this.CustomCheckBoxDPIAdvSetTTL.Text = "--set-ttl";
            this.CustomCheckBoxDPIAdvSetTTL.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvAllowNoSNI
            // 
            this.CustomCheckBoxDPIAdvAllowNoSNI.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvAllowNoSNI.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvAllowNoSNI.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvAllowNoSNI.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvAllowNoSNI.Location = new System.Drawing.Point(5, 95);
            this.CustomCheckBoxDPIAdvAllowNoSNI.Name = "CustomCheckBoxDPIAdvAllowNoSNI";
            this.CustomCheckBoxDPIAdvAllowNoSNI.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvAllowNoSNI.Size = new System.Drawing.Size(98, 17);
            this.CustomCheckBoxDPIAdvAllowNoSNI.TabIndex = 17;
            this.CustomCheckBoxDPIAdvAllowNoSNI.Text = "--allow-no-sni";
            this.CustomCheckBoxDPIAdvAllowNoSNI.UseVisualStyleBackColor = false;
            // 
            // CustomTextBoxDPIAdvIpId
            // 
            this.CustomTextBoxDPIAdvIpId.AcceptsReturn = false;
            this.CustomTextBoxDPIAdvIpId.AcceptsTab = false;
            this.CustomTextBoxDPIAdvIpId.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxDPIAdvIpId.Border = true;
            this.CustomTextBoxDPIAdvIpId.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxDPIAdvIpId.BorderSize = 1;
            this.CustomTextBoxDPIAdvIpId.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxDPIAdvIpId.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxDPIAdvIpId.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxDPIAdvIpId.HideSelection = true;
            this.CustomTextBoxDPIAdvIpId.Location = new System.Drawing.Point(529, 63);
            this.CustomTextBoxDPIAdvIpId.MaxLength = 32767;
            this.CustomTextBoxDPIAdvIpId.Multiline = false;
            this.CustomTextBoxDPIAdvIpId.Name = "CustomTextBoxDPIAdvIpId";
            this.CustomTextBoxDPIAdvIpId.ReadOnly = false;
            this.CustomTextBoxDPIAdvIpId.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxDPIAdvIpId.ShortcutsEnabled = true;
            this.CustomTextBoxDPIAdvIpId.Size = new System.Drawing.Size(70, 23);
            this.CustomTextBoxDPIAdvIpId.TabIndex = 0;
            this.CustomTextBoxDPIAdvIpId.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxDPIAdvIpId.Texts = "";
            this.CustomTextBoxDPIAdvIpId.UnderlinedStyle = false;
            this.CustomTextBoxDPIAdvIpId.UsePasswordChar = false;
            this.CustomTextBoxDPIAdvIpId.WordWrap = true;
            // 
            // CustomCheckBoxDPIAdvIpId
            // 
            this.CustomCheckBoxDPIAdvIpId.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvIpId.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvIpId.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvIpId.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvIpId.Location = new System.Drawing.Point(470, 65);
            this.CustomCheckBoxDPIAdvIpId.Name = "CustomCheckBoxDPIAdvIpId";
            this.CustomCheckBoxDPIAdvIpId.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvIpId.Size = new System.Drawing.Size(56, 17);
            this.CustomCheckBoxDPIAdvIpId.TabIndex = 15;
            this.CustomCheckBoxDPIAdvIpId.Text = "--ip-id";
            this.CustomCheckBoxDPIAdvIpId.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvPort
            // 
            this.CustomCheckBoxDPIAdvPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvPort.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvPort.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvPort.Location = new System.Drawing.Point(320, 65);
            this.CustomCheckBoxDPIAdvPort.Name = "CustomCheckBoxDPIAdvPort";
            this.CustomCheckBoxDPIAdvPort.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvPort.Size = new System.Drawing.Size(53, 17);
            this.CustomCheckBoxDPIAdvPort.TabIndex = 13;
            this.CustomCheckBoxDPIAdvPort.Text = "--port";
            this.CustomCheckBoxDPIAdvPort.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvW
            // 
            this.CustomCheckBoxDPIAdvW.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvW.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvW.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvW.Checked = true;
            this.CustomCheckBoxDPIAdvW.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxDPIAdvW.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvW.Location = new System.Drawing.Point(165, 65);
            this.CustomCheckBoxDPIAdvW.Name = "CustomCheckBoxDPIAdvW";
            this.CustomCheckBoxDPIAdvW.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvW.Size = new System.Drawing.Size(34, 17);
            this.CustomCheckBoxDPIAdvW.TabIndex = 12;
            this.CustomCheckBoxDPIAdvW.Text = "-w";
            this.CustomCheckBoxDPIAdvW.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvA
            // 
            this.CustomCheckBoxDPIAdvA.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvA.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvA.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvA.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvA.Location = new System.Drawing.Point(5, 65);
            this.CustomCheckBoxDPIAdvA.Name = "CustomCheckBoxDPIAdvA";
            this.CustomCheckBoxDPIAdvA.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvA.Size = new System.Drawing.Size(32, 17);
            this.CustomCheckBoxDPIAdvA.TabIndex = 11;
            this.CustomCheckBoxDPIAdvA.Text = "-a";
            this.CustomCheckBoxDPIAdvA.UseVisualStyleBackColor = false;
            // 
            // CustomNumericUpDownDPIAdvE
            // 
            this.CustomNumericUpDownDPIAdvE.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownDPIAdvE.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownDPIAdvE.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownDPIAdvE.Location = new System.Drawing.Point(506, 33);
            this.CustomNumericUpDownDPIAdvE.Maximum = new decimal(new int[] {
            70000,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvE.Name = "CustomNumericUpDownDPIAdvE";
            this.CustomNumericUpDownDPIAdvE.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownDPIAdvE.TabIndex = 10;
            this.CustomNumericUpDownDPIAdvE.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            // 
            // CustomCheckBoxDPIAdvE
            // 
            this.CustomCheckBoxDPIAdvE.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvE.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvE.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvE.Checked = true;
            this.CustomCheckBoxDPIAdvE.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxDPIAdvE.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvE.Location = new System.Drawing.Point(470, 35);
            this.CustomCheckBoxDPIAdvE.Name = "CustomCheckBoxDPIAdvE";
            this.CustomCheckBoxDPIAdvE.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvE.Size = new System.Drawing.Size(32, 17);
            this.CustomCheckBoxDPIAdvE.TabIndex = 9;
            this.CustomCheckBoxDPIAdvE.Text = "-e";
            this.CustomCheckBoxDPIAdvE.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvN
            // 
            this.CustomCheckBoxDPIAdvN.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvN.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvN.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvN.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvN.Location = new System.Drawing.Point(320, 35);
            this.CustomCheckBoxDPIAdvN.Name = "CustomCheckBoxDPIAdvN";
            this.CustomCheckBoxDPIAdvN.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvN.Size = new System.Drawing.Size(32, 17);
            this.CustomCheckBoxDPIAdvN.TabIndex = 8;
            this.CustomCheckBoxDPIAdvN.Text = "-n";
            this.CustomCheckBoxDPIAdvN.UseVisualStyleBackColor = false;
            // 
            // CustomNumericUpDownDPIAdvK
            // 
            this.CustomNumericUpDownDPIAdvK.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownDPIAdvK.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownDPIAdvK.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownDPIAdvK.Location = new System.Drawing.Point(201, 33);
            this.CustomNumericUpDownDPIAdvK.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvK.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvK.Name = "CustomNumericUpDownDPIAdvK";
            this.CustomNumericUpDownDPIAdvK.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownDPIAdvK.TabIndex = 7;
            this.CustomNumericUpDownDPIAdvK.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // CustomCheckBoxDPIAdvK
            // 
            this.CustomCheckBoxDPIAdvK.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvK.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvK.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvK.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvK.Location = new System.Drawing.Point(165, 35);
            this.CustomCheckBoxDPIAdvK.Name = "CustomCheckBoxDPIAdvK";
            this.CustomCheckBoxDPIAdvK.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvK.Size = new System.Drawing.Size(32, 17);
            this.CustomCheckBoxDPIAdvK.TabIndex = 6;
            this.CustomCheckBoxDPIAdvK.Text = "-k";
            this.CustomCheckBoxDPIAdvK.UseVisualStyleBackColor = false;
            // 
            // CustomNumericUpDownDPIAdvF
            // 
            this.CustomNumericUpDownDPIAdvF.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownDPIAdvF.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownDPIAdvF.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownDPIAdvF.Location = new System.Drawing.Point(38, 33);
            this.CustomNumericUpDownDPIAdvF.Maximum = new decimal(new int[] {
            70000,
            0,
            0,
            0});
            this.CustomNumericUpDownDPIAdvF.Name = "CustomNumericUpDownDPIAdvF";
            this.CustomNumericUpDownDPIAdvF.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownDPIAdvF.TabIndex = 5;
            this.CustomNumericUpDownDPIAdvF.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // CustomCheckBoxDPIAdvF
            // 
            this.CustomCheckBoxDPIAdvF.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvF.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvF.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvF.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvF.Location = new System.Drawing.Point(5, 35);
            this.CustomCheckBoxDPIAdvF.Name = "CustomCheckBoxDPIAdvF";
            this.CustomCheckBoxDPIAdvF.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvF.Size = new System.Drawing.Size(29, 17);
            this.CustomCheckBoxDPIAdvF.TabIndex = 4;
            this.CustomCheckBoxDPIAdvF.Text = "-f";
            this.CustomCheckBoxDPIAdvF.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvM
            // 
            this.CustomCheckBoxDPIAdvM.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvM.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvM.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvM.Checked = true;
            this.CustomCheckBoxDPIAdvM.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxDPIAdvM.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvM.Location = new System.Drawing.Point(470, 5);
            this.CustomCheckBoxDPIAdvM.Name = "CustomCheckBoxDPIAdvM";
            this.CustomCheckBoxDPIAdvM.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvM.Size = new System.Drawing.Size(36, 17);
            this.CustomCheckBoxDPIAdvM.TabIndex = 3;
            this.CustomCheckBoxDPIAdvM.Text = "-m";
            this.CustomCheckBoxDPIAdvM.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvS
            // 
            this.CustomCheckBoxDPIAdvS.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvS.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvS.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvS.Checked = true;
            this.CustomCheckBoxDPIAdvS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxDPIAdvS.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvS.Location = new System.Drawing.Point(320, 5);
            this.CustomCheckBoxDPIAdvS.Name = "CustomCheckBoxDPIAdvS";
            this.CustomCheckBoxDPIAdvS.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvS.Size = new System.Drawing.Size(31, 17);
            this.CustomCheckBoxDPIAdvS.TabIndex = 2;
            this.CustomCheckBoxDPIAdvS.Text = "-s";
            this.CustomCheckBoxDPIAdvS.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvR
            // 
            this.CustomCheckBoxDPIAdvR.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvR.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvR.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvR.Checked = true;
            this.CustomCheckBoxDPIAdvR.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxDPIAdvR.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvR.Location = new System.Drawing.Point(165, 5);
            this.CustomCheckBoxDPIAdvR.Name = "CustomCheckBoxDPIAdvR";
            this.CustomCheckBoxDPIAdvR.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvR.Size = new System.Drawing.Size(30, 17);
            this.CustomCheckBoxDPIAdvR.TabIndex = 1;
            this.CustomCheckBoxDPIAdvR.Text = "-r";
            this.CustomCheckBoxDPIAdvR.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxDPIAdvP
            // 
            this.CustomCheckBoxDPIAdvP.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDPIAdvP.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvP.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDPIAdvP.Checked = true;
            this.CustomCheckBoxDPIAdvP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxDPIAdvP.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDPIAdvP.Location = new System.Drawing.Point(5, 5);
            this.CustomCheckBoxDPIAdvP.Name = "CustomCheckBoxDPIAdvP";
            this.CustomCheckBoxDPIAdvP.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDPIAdvP.Size = new System.Drawing.Size(33, 17);
            this.CustomCheckBoxDPIAdvP.TabIndex = 0;
            this.CustomCheckBoxDPIAdvP.Text = "-p";
            this.CustomCheckBoxDPIAdvP.UseVisualStyleBackColor = false;
            // 
            // TabPageSettings
            // 
            this.TabPageSettings.BackColor = System.Drawing.Color.Transparent;
            this.TabPageSettings.Controls.Add(this.CustomTabControlSettings);
            this.TabPageSettings.Location = new System.Drawing.Point(4, 25);
            this.TabPageSettings.Name = "TabPageSettings";
            this.TabPageSettings.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSettings.Size = new System.Drawing.Size(692, 371);
            this.TabPageSettings.TabIndex = 1;
            this.TabPageSettings.Tag = 1;
            this.TabPageSettings.Text = "Settings";
            // 
            // CustomTabControlSettings
            // 
            this.CustomTabControlSettings.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.CustomTabControlSettings.BorderColor = System.Drawing.Color.Blue;
            this.CustomTabControlSettings.Controls.Add(this.TabPageSettingsWorkingMode);
            this.CustomTabControlSettings.Controls.Add(this.TabPageSettingsCheck);
            this.CustomTabControlSettings.Controls.Add(this.TabPageSettingsConnect);
            this.CustomTabControlSettings.Controls.Add(this.TabPageSettingsSetUnsetDNS);
            this.CustomTabControlSettings.Controls.Add(this.TabPageSettingsCPU);
            this.CustomTabControlSettings.Controls.Add(this.TabPageSettingsOthers);
            this.CustomTabControlSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomTabControlSettings.HideTabHeader = false;
            this.CustomTabControlSettings.ItemSize = new System.Drawing.Size(30, 90);
            this.CustomTabControlSettings.Location = new System.Drawing.Point(3, 3);
            this.CustomTabControlSettings.Margin = new System.Windows.Forms.Padding(0);
            this.CustomTabControlSettings.Multiline = true;
            this.CustomTabControlSettings.Name = "CustomTabControlSettings";
            this.CustomTabControlSettings.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.CustomTabControlSettings.SelectedIndex = 0;
            this.CustomTabControlSettings.Size = new System.Drawing.Size(686, 365);
            this.CustomTabControlSettings.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.CustomTabControlSettings.TabIndex = 10;
            this.CustomTabControlSettings.Tag = 0;
            // 
            // TabPageSettingsWorkingMode
            // 
            this.TabPageSettingsWorkingMode.BackColor = System.Drawing.Color.Transparent;
            this.TabPageSettingsWorkingMode.Controls.Add(this.CustomLabelSettingInfoWorkingMode2);
            this.TabPageSettingsWorkingMode.Controls.Add(this.CustomRadioButtonSettingWorkingModeDNSandDoH);
            this.TabPageSettingsWorkingMode.Controls.Add(this.CustomRadioButtonSettingWorkingModeDNS);
            this.TabPageSettingsWorkingMode.Controls.Add(this.CustomLabelSettingInfoWorkingMode1);
            this.TabPageSettingsWorkingMode.Location = new System.Drawing.Point(94, 4);
            this.TabPageSettingsWorkingMode.Name = "TabPageSettingsWorkingMode";
            this.TabPageSettingsWorkingMode.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSettingsWorkingMode.Size = new System.Drawing.Size(588, 357);
            this.TabPageSettingsWorkingMode.TabIndex = 0;
            this.TabPageSettingsWorkingMode.Tag = 0;
            this.TabPageSettingsWorkingMode.Text = "Working mode";
            // 
            // CustomLabelSettingInfoWorkingMode2
            // 
            this.CustomLabelSettingInfoWorkingMode2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomLabelSettingInfoWorkingMode2.AutoSize = true;
            this.CustomLabelSettingInfoWorkingMode2.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingInfoWorkingMode2.Border = false;
            this.CustomLabelSettingInfoWorkingMode2.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingInfoWorkingMode2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingInfoWorkingMode2.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingInfoWorkingMode2.Location = new System.Drawing.Point(50, 334);
            this.CustomLabelSettingInfoWorkingMode2.Name = "CustomLabelSettingInfoWorkingMode2";
            this.CustomLabelSettingInfoWorkingMode2.RoundedCorners = 0;
            this.CustomLabelSettingInfoWorkingMode2.Size = new System.Drawing.Size(262, 15);
            this.CustomLabelSettingInfoWorkingMode2.TabIndex = 3;
            this.CustomLabelSettingInfoWorkingMode2.Text = "* Reconnect is require for changes to take effect.";
            // 
            // CustomRadioButtonSettingWorkingModeDNSandDoH
            // 
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.Location = new System.Drawing.Point(50, 145);
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.Name = "CustomRadioButtonSettingWorkingModeDNSandDoH";
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.Size = new System.Drawing.Size(234, 17);
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.TabIndex = 2;
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.Text = "Legacy DNS + DNS-Over-HTTPS Server";
            this.CustomRadioButtonSettingWorkingModeDNSandDoH.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSettingWorkingModeDNS
            // 
            this.CustomRadioButtonSettingWorkingModeDNS.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSettingWorkingModeDNS.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingWorkingModeDNS.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingWorkingModeDNS.Checked = true;
            this.CustomRadioButtonSettingWorkingModeDNS.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSettingWorkingModeDNS.Location = new System.Drawing.Point(50, 100);
            this.CustomRadioButtonSettingWorkingModeDNS.Name = "CustomRadioButtonSettingWorkingModeDNS";
            this.CustomRadioButtonSettingWorkingModeDNS.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSettingWorkingModeDNS.Size = new System.Drawing.Size(123, 17);
            this.CustomRadioButtonSettingWorkingModeDNS.TabIndex = 1;
            this.CustomRadioButtonSettingWorkingModeDNS.TabStop = true;
            this.CustomRadioButtonSettingWorkingModeDNS.Text = "Legacy DNS Server";
            this.CustomRadioButtonSettingWorkingModeDNS.UseVisualStyleBackColor = false;
            this.CustomRadioButtonSettingWorkingModeDNS.CheckedChanged += new System.EventHandler(this.SecureDNSClient_CheckedChanged);
            // 
            // CustomLabelSettingInfoWorkingMode1
            // 
            this.CustomLabelSettingInfoWorkingMode1.AutoSize = true;
            this.CustomLabelSettingInfoWorkingMode1.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingInfoWorkingMode1.Border = false;
            this.CustomLabelSettingInfoWorkingMode1.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingInfoWorkingMode1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingInfoWorkingMode1.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingInfoWorkingMode1.Location = new System.Drawing.Point(50, 35);
            this.CustomLabelSettingInfoWorkingMode1.Name = "CustomLabelSettingInfoWorkingMode1";
            this.CustomLabelSettingInfoWorkingMode1.RoundedCorners = 0;
            this.CustomLabelSettingInfoWorkingMode1.Size = new System.Drawing.Size(394, 30);
            this.CustomLabelSettingInfoWorkingMode1.TabIndex = 0;
            this.CustomLabelSettingInfoWorkingMode1.Text = "Legacy DNS Server: You can set and unset DNS easily.\r\nDNS Over HTTPS Server: You " +
    "need to install certificate and set it manually.";
            // 
            // TabPageSettingsCheck
            // 
            this.TabPageSettingsCheck.BackColor = System.Drawing.Color.Transparent;
            this.TabPageSettingsCheck.Controls.Add(this.CustomGroupBoxSettingCheckSDNS);
            this.TabPageSettingsCheck.Controls.Add(this.CustomTextBoxSettingCheckDPIHost);
            this.TabPageSettingsCheck.Controls.Add(this.CustomLabelSettingCheckDPIInfo);
            this.TabPageSettingsCheck.Controls.Add(this.CustomLabelSettingCheckTimeout);
            this.TabPageSettingsCheck.Controls.Add(this.CustomNumericUpDownSettingCheckTimeout);
            this.TabPageSettingsCheck.Location = new System.Drawing.Point(94, 4);
            this.TabPageSettingsCheck.Name = "TabPageSettingsCheck";
            this.TabPageSettingsCheck.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSettingsCheck.Size = new System.Drawing.Size(588, 357);
            this.TabPageSettingsCheck.TabIndex = 3;
            this.TabPageSettingsCheck.Tag = 1;
            this.TabPageSettingsCheck.Text = "Check";
            // 
            // CustomGroupBoxSettingCheckSDNS
            // 
            this.CustomGroupBoxSettingCheckSDNS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomGroupBoxSettingCheckSDNS.BorderColor = System.Drawing.Color.Blue;
            this.CustomGroupBoxSettingCheckSDNS.Controls.Add(this.CustomCheckBoxSettingSdnsNoFilter);
            this.CustomGroupBoxSettingCheckSDNS.Controls.Add(this.CustomCheckBoxSettingSdnsNoLog);
            this.CustomGroupBoxSettingCheckSDNS.Controls.Add(this.CustomCheckBoxSettingSdnsDNSSec);
            this.CustomGroupBoxSettingCheckSDNS.Location = new System.Drawing.Point(4, 111);
            this.CustomGroupBoxSettingCheckSDNS.Margin = new System.Windows.Forms.Padding(1);
            this.CustomGroupBoxSettingCheckSDNS.Name = "CustomGroupBoxSettingCheckSDNS";
            this.CustomGroupBoxSettingCheckSDNS.Size = new System.Drawing.Size(580, 65);
            this.CustomGroupBoxSettingCheckSDNS.TabIndex = 11;
            this.CustomGroupBoxSettingCheckSDNS.TabStop = false;
            this.CustomGroupBoxSettingCheckSDNS.Text = "sdns:// servers must have";
            // 
            // CustomCheckBoxSettingSdnsNoFilter
            // 
            this.CustomCheckBoxSettingSdnsNoFilter.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxSettingSdnsNoFilter.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingSdnsNoFilter.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingSdnsNoFilter.Checked = true;
            this.CustomCheckBoxSettingSdnsNoFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxSettingSdnsNoFilter.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxSettingSdnsNoFilter.Location = new System.Drawing.Point(420, 28);
            this.CustomCheckBoxSettingSdnsNoFilter.Name = "CustomCheckBoxSettingSdnsNoFilter";
            this.CustomCheckBoxSettingSdnsNoFilter.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxSettingSdnsNoFilter.Size = new System.Drawing.Size(67, 17);
            this.CustomCheckBoxSettingSdnsNoFilter.TabIndex = 2;
            this.CustomCheckBoxSettingSdnsNoFilter.Text = "No Filter";
            this.CustomCheckBoxSettingSdnsNoFilter.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxSettingSdnsNoLog
            // 
            this.CustomCheckBoxSettingSdnsNoLog.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxSettingSdnsNoLog.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingSdnsNoLog.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingSdnsNoLog.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxSettingSdnsNoLog.Location = new System.Drawing.Point(230, 28);
            this.CustomCheckBoxSettingSdnsNoLog.Name = "CustomCheckBoxSettingSdnsNoLog";
            this.CustomCheckBoxSettingSdnsNoLog.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxSettingSdnsNoLog.Size = new System.Drawing.Size(61, 17);
            this.CustomCheckBoxSettingSdnsNoLog.TabIndex = 1;
            this.CustomCheckBoxSettingSdnsNoLog.Text = "No Log";
            this.CustomCheckBoxSettingSdnsNoLog.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxSettingSdnsDNSSec
            // 
            this.CustomCheckBoxSettingSdnsDNSSec.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxSettingSdnsDNSSec.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingSdnsDNSSec.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingSdnsDNSSec.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxSettingSdnsDNSSec.Location = new System.Drawing.Point(15, 28);
            this.CustomCheckBoxSettingSdnsDNSSec.Name = "CustomCheckBoxSettingSdnsDNSSec";
            this.CustomCheckBoxSettingSdnsDNSSec.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxSettingSdnsDNSSec.Size = new System.Drawing.Size(111, 17);
            this.CustomCheckBoxSettingSdnsDNSSec.TabIndex = 0;
            this.CustomCheckBoxSettingSdnsDNSSec.Text = "DNSSec Enabled";
            this.CustomCheckBoxSettingSdnsDNSSec.UseVisualStyleBackColor = false;
            // 
            // CustomTextBoxSettingCheckDPIHost
            // 
            this.CustomTextBoxSettingCheckDPIHost.AcceptsReturn = false;
            this.CustomTextBoxSettingCheckDPIHost.AcceptsTab = false;
            this.CustomTextBoxSettingCheckDPIHost.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxSettingCheckDPIHost.Border = true;
            this.CustomTextBoxSettingCheckDPIHost.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxSettingCheckDPIHost.BorderSize = 1;
            this.CustomTextBoxSettingCheckDPIHost.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxSettingCheckDPIHost.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxSettingCheckDPIHost.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxSettingCheckDPIHost.HideSelection = true;
            this.CustomTextBoxSettingCheckDPIHost.Location = new System.Drawing.Point(274, 63);
            this.CustomTextBoxSettingCheckDPIHost.MaxLength = 32767;
            this.CustomTextBoxSettingCheckDPIHost.Multiline = false;
            this.CustomTextBoxSettingCheckDPIHost.Name = "CustomTextBoxSettingCheckDPIHost";
            this.CustomTextBoxSettingCheckDPIHost.ReadOnly = false;
            this.CustomTextBoxSettingCheckDPIHost.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxSettingCheckDPIHost.ShortcutsEnabled = true;
            this.CustomTextBoxSettingCheckDPIHost.Size = new System.Drawing.Size(143, 23);
            this.CustomTextBoxSettingCheckDPIHost.TabIndex = 0;
            this.CustomTextBoxSettingCheckDPIHost.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxSettingCheckDPIHost.Texts = "www.youtube.com";
            this.CustomTextBoxSettingCheckDPIHost.UnderlinedStyle = false;
            this.CustomTextBoxSettingCheckDPIHost.UsePasswordChar = false;
            this.CustomTextBoxSettingCheckDPIHost.WordWrap = true;
            // 
            // CustomLabelSettingCheckDPIInfo
            // 
            this.CustomLabelSettingCheckDPIInfo.AutoSize = true;
            this.CustomLabelSettingCheckDPIInfo.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingCheckDPIInfo.Border = false;
            this.CustomLabelSettingCheckDPIInfo.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingCheckDPIInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingCheckDPIInfo.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingCheckDPIInfo.Location = new System.Drawing.Point(15, 65);
            this.CustomLabelSettingCheckDPIInfo.Name = "CustomLabelSettingCheckDPIInfo";
            this.CustomLabelSettingCheckDPIInfo.RoundedCorners = 0;
            this.CustomLabelSettingCheckDPIInfo.Size = new System.Drawing.Size(257, 15);
            this.CustomLabelSettingCheckDPIInfo.TabIndex = 10;
            this.CustomLabelSettingCheckDPIInfo.Text = "A DNS based blocked website to check. https://";
            // 
            // CustomLabelSettingCheckTimeout
            // 
            this.CustomLabelSettingCheckTimeout.AutoSize = true;
            this.CustomLabelSettingCheckTimeout.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingCheckTimeout.Border = false;
            this.CustomLabelSettingCheckTimeout.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingCheckTimeout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingCheckTimeout.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingCheckTimeout.Location = new System.Drawing.Point(15, 25);
            this.CustomLabelSettingCheckTimeout.Name = "CustomLabelSettingCheckTimeout";
            this.CustomLabelSettingCheckTimeout.RoundedCorners = 0;
            this.CustomLabelSettingCheckTimeout.Size = new System.Drawing.Size(142, 15);
            this.CustomLabelSettingCheckTimeout.TabIndex = 2;
            this.CustomLabelSettingCheckTimeout.Text = "Check timeout (seconds):";
            // 
            // CustomNumericUpDownSettingCheckTimeout
            // 
            this.CustomNumericUpDownSettingCheckTimeout.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownSettingCheckTimeout.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownSettingCheckTimeout.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownSettingCheckTimeout.DecimalPlaces = 1;
            this.CustomNumericUpDownSettingCheckTimeout.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.CustomNumericUpDownSettingCheckTimeout.Location = new System.Drawing.Point(160, 23);
            this.CustomNumericUpDownSettingCheckTimeout.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.CustomNumericUpDownSettingCheckTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.CustomNumericUpDownSettingCheckTimeout.Name = "CustomNumericUpDownSettingCheckTimeout";
            this.CustomNumericUpDownSettingCheckTimeout.Size = new System.Drawing.Size(47, 23);
            this.CustomNumericUpDownSettingCheckTimeout.TabIndex = 3;
            this.CustomNumericUpDownSettingCheckTimeout.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // TabPageSettingsConnect
            // 
            this.TabPageSettingsConnect.BackColor = System.Drawing.Color.Transparent;
            this.TabPageSettingsConnect.Controls.Add(this.CustomCheckBoxSettingEnableCache);
            this.TabPageSettingsConnect.Controls.Add(this.CustomNumericUpDownSettingCamouflagePort);
            this.TabPageSettingsConnect.Controls.Add(this.CustomLabelCheckSettingCamouflagePort);
            this.TabPageSettingsConnect.Controls.Add(this.CustomNumericUpDownSettingMaxServers);
            this.TabPageSettingsConnect.Controls.Add(this.CustomLabelSettingMaxServers);
            this.TabPageSettingsConnect.Location = new System.Drawing.Point(94, 4);
            this.TabPageSettingsConnect.Name = "TabPageSettingsConnect";
            this.TabPageSettingsConnect.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSettingsConnect.Size = new System.Drawing.Size(588, 357);
            this.TabPageSettingsConnect.TabIndex = 4;
            this.TabPageSettingsConnect.Tag = 2;
            this.TabPageSettingsConnect.Text = "Connect";
            // 
            // CustomCheckBoxSettingEnableCache
            // 
            this.CustomCheckBoxSettingEnableCache.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxSettingEnableCache.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingEnableCache.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingEnableCache.Checked = true;
            this.CustomCheckBoxSettingEnableCache.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CustomCheckBoxSettingEnableCache.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxSettingEnableCache.Location = new System.Drawing.Point(50, 55);
            this.CustomCheckBoxSettingEnableCache.Name = "CustomCheckBoxSettingEnableCache";
            this.CustomCheckBoxSettingEnableCache.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxSettingEnableCache.Size = new System.Drawing.Size(119, 17);
            this.CustomCheckBoxSettingEnableCache.TabIndex = 17;
            this.CustomCheckBoxSettingEnableCache.Text = "Enable DNS cache";
            this.CustomCheckBoxSettingEnableCache.UseVisualStyleBackColor = false;
            // 
            // CustomNumericUpDownSettingCamouflagePort
            // 
            this.CustomNumericUpDownSettingCamouflagePort.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownSettingCamouflagePort.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownSettingCamouflagePort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownSettingCamouflagePort.Location = new System.Drawing.Point(215, 163);
            this.CustomNumericUpDownSettingCamouflagePort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.CustomNumericUpDownSettingCamouflagePort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownSettingCamouflagePort.Name = "CustomNumericUpDownSettingCamouflagePort";
            this.CustomNumericUpDownSettingCamouflagePort.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownSettingCamouflagePort.TabIndex = 16;
            this.CustomNumericUpDownSettingCamouflagePort.Value = new decimal(new int[] {
            5380,
            0,
            0,
            0});
            // 
            // CustomLabelCheckSettingCamouflagePort
            // 
            this.CustomLabelCheckSettingCamouflagePort.AutoSize = true;
            this.CustomLabelCheckSettingCamouflagePort.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelCheckSettingCamouflagePort.Border = false;
            this.CustomLabelCheckSettingCamouflagePort.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelCheckSettingCamouflagePort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelCheckSettingCamouflagePort.ForeColor = System.Drawing.Color.White;
            this.CustomLabelCheckSettingCamouflagePort.Location = new System.Drawing.Point(50, 165);
            this.CustomLabelCheckSettingCamouflagePort.Name = "CustomLabelCheckSettingCamouflagePort";
            this.CustomLabelCheckSettingCamouflagePort.RoundedCorners = 0;
            this.CustomLabelCheckSettingCamouflagePort.Size = new System.Drawing.Size(160, 15);
            this.CustomLabelCheckSettingCamouflagePort.TabIndex = 15;
            this.CustomLabelCheckSettingCamouflagePort.Text = "Camouflage DNS server port:";
            // 
            // CustomNumericUpDownSettingMaxServers
            // 
            this.CustomNumericUpDownSettingMaxServers.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownSettingMaxServers.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownSettingMaxServers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownSettingMaxServers.Location = new System.Drawing.Point(275, 108);
            this.CustomNumericUpDownSettingMaxServers.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.CustomNumericUpDownSettingMaxServers.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownSettingMaxServers.Name = "CustomNumericUpDownSettingMaxServers";
            this.CustomNumericUpDownSettingMaxServers.Size = new System.Drawing.Size(47, 23);
            this.CustomNumericUpDownSettingMaxServers.TabIndex = 7;
            this.CustomNumericUpDownSettingMaxServers.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // CustomLabelSettingMaxServers
            // 
            this.CustomLabelSettingMaxServers.AutoSize = true;
            this.CustomLabelSettingMaxServers.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingMaxServers.Border = false;
            this.CustomLabelSettingMaxServers.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingMaxServers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingMaxServers.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingMaxServers.Location = new System.Drawing.Point(50, 110);
            this.CustomLabelSettingMaxServers.Name = "CustomLabelSettingMaxServers";
            this.CustomLabelSettingMaxServers.RoundedCorners = 0;
            this.CustomLabelSettingMaxServers.Size = new System.Drawing.Size(223, 15);
            this.CustomLabelSettingMaxServers.TabIndex = 6;
            this.CustomLabelSettingMaxServers.Text = "Maximum number of servers to connect:";
            // 
            // TabPageSettingsSetUnsetDNS
            // 
            this.TabPageSettingsSetUnsetDNS.BackColor = System.Drawing.Color.Transparent;
            this.TabPageSettingsSetUnsetDNS.Controls.Add(this.CustomTextBoxSettingUnsetDns2);
            this.TabPageSettingsSetUnsetDNS.Controls.Add(this.CustomTextBoxSettingUnsetDns1);
            this.TabPageSettingsSetUnsetDNS.Controls.Add(this.CustomLabelSettingUnsetDns2);
            this.TabPageSettingsSetUnsetDNS.Controls.Add(this.CustomLabelSettingUnsetDns1);
            this.TabPageSettingsSetUnsetDNS.Controls.Add(this.CustomRadioButtonSettingUnsetDnsToStatic);
            this.TabPageSettingsSetUnsetDNS.Controls.Add(this.CustomRadioButtonSettingUnsetDnsToDhcp);
            this.TabPageSettingsSetUnsetDNS.Location = new System.Drawing.Point(94, 4);
            this.TabPageSettingsSetUnsetDNS.Name = "TabPageSettingsSetUnsetDNS";
            this.TabPageSettingsSetUnsetDNS.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSettingsSetUnsetDNS.Size = new System.Drawing.Size(588, 357);
            this.TabPageSettingsSetUnsetDNS.TabIndex = 5;
            this.TabPageSettingsSetUnsetDNS.Tag = 3;
            this.TabPageSettingsSetUnsetDNS.Text = "Set/Unset DNS";
            // 
            // CustomTextBoxSettingUnsetDns2
            // 
            this.CustomTextBoxSettingUnsetDns2.AcceptsReturn = false;
            this.CustomTextBoxSettingUnsetDns2.AcceptsTab = false;
            this.CustomTextBoxSettingUnsetDns2.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxSettingUnsetDns2.Border = true;
            this.CustomTextBoxSettingUnsetDns2.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxSettingUnsetDns2.BorderSize = 1;
            this.CustomTextBoxSettingUnsetDns2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxSettingUnsetDns2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxSettingUnsetDns2.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxSettingUnsetDns2.HideSelection = true;
            this.CustomTextBoxSettingUnsetDns2.Location = new System.Drawing.Point(180, 138);
            this.CustomTextBoxSettingUnsetDns2.MaxLength = 32767;
            this.CustomTextBoxSettingUnsetDns2.Multiline = false;
            this.CustomTextBoxSettingUnsetDns2.Name = "CustomTextBoxSettingUnsetDns2";
            this.CustomTextBoxSettingUnsetDns2.ReadOnly = false;
            this.CustomTextBoxSettingUnsetDns2.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxSettingUnsetDns2.ShortcutsEnabled = true;
            this.CustomTextBoxSettingUnsetDns2.Size = new System.Drawing.Size(95, 23);
            this.CustomTextBoxSettingUnsetDns2.TabIndex = 0;
            this.CustomTextBoxSettingUnsetDns2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxSettingUnsetDns2.Texts = "8.8.4.4";
            this.CustomTextBoxSettingUnsetDns2.UnderlinedStyle = false;
            this.CustomTextBoxSettingUnsetDns2.UsePasswordChar = false;
            this.CustomTextBoxSettingUnsetDns2.WordWrap = true;
            // 
            // CustomTextBoxSettingUnsetDns1
            // 
            this.CustomTextBoxSettingUnsetDns1.AcceptsReturn = false;
            this.CustomTextBoxSettingUnsetDns1.AcceptsTab = false;
            this.CustomTextBoxSettingUnsetDns1.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxSettingUnsetDns1.Border = true;
            this.CustomTextBoxSettingUnsetDns1.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxSettingUnsetDns1.BorderSize = 1;
            this.CustomTextBoxSettingUnsetDns1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxSettingUnsetDns1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxSettingUnsetDns1.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxSettingUnsetDns1.HideSelection = true;
            this.CustomTextBoxSettingUnsetDns1.Location = new System.Drawing.Point(180, 103);
            this.CustomTextBoxSettingUnsetDns1.MaxLength = 32767;
            this.CustomTextBoxSettingUnsetDns1.Multiline = false;
            this.CustomTextBoxSettingUnsetDns1.Name = "CustomTextBoxSettingUnsetDns1";
            this.CustomTextBoxSettingUnsetDns1.ReadOnly = false;
            this.CustomTextBoxSettingUnsetDns1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxSettingUnsetDns1.ShortcutsEnabled = true;
            this.CustomTextBoxSettingUnsetDns1.Size = new System.Drawing.Size(95, 23);
            this.CustomTextBoxSettingUnsetDns1.TabIndex = 0;
            this.CustomTextBoxSettingUnsetDns1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxSettingUnsetDns1.Texts = "8.8.8.8";
            this.CustomTextBoxSettingUnsetDns1.UnderlinedStyle = false;
            this.CustomTextBoxSettingUnsetDns1.UsePasswordChar = false;
            this.CustomTextBoxSettingUnsetDns1.WordWrap = true;
            // 
            // CustomLabelSettingUnsetDns2
            // 
            this.CustomLabelSettingUnsetDns2.AutoSize = true;
            this.CustomLabelSettingUnsetDns2.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingUnsetDns2.Border = false;
            this.CustomLabelSettingUnsetDns2.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingUnsetDns2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingUnsetDns2.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingUnsetDns2.Location = new System.Drawing.Point(110, 140);
            this.CustomLabelSettingUnsetDns2.Name = "CustomLabelSettingUnsetDns2";
            this.CustomLabelSettingUnsetDns2.RoundedCorners = 0;
            this.CustomLabelSettingUnsetDns2.Size = new System.Drawing.Size(65, 15);
            this.CustomLabelSettingUnsetDns2.TabIndex = 3;
            this.CustomLabelSettingUnsetDns2.Text = "Secondary:";
            // 
            // CustomLabelSettingUnsetDns1
            // 
            this.CustomLabelSettingUnsetDns1.AutoSize = true;
            this.CustomLabelSettingUnsetDns1.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingUnsetDns1.Border = false;
            this.CustomLabelSettingUnsetDns1.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingUnsetDns1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingUnsetDns1.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingUnsetDns1.Location = new System.Drawing.Point(110, 105);
            this.CustomLabelSettingUnsetDns1.Name = "CustomLabelSettingUnsetDns1";
            this.CustomLabelSettingUnsetDns1.RoundedCorners = 0;
            this.CustomLabelSettingUnsetDns1.Size = new System.Drawing.Size(51, 15);
            this.CustomLabelSettingUnsetDns1.TabIndex = 2;
            this.CustomLabelSettingUnsetDns1.Text = "Primary:";
            // 
            // CustomRadioButtonSettingUnsetDnsToStatic
            // 
            this.CustomRadioButtonSettingUnsetDnsToStatic.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSettingUnsetDnsToStatic.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingUnsetDnsToStatic.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingUnsetDnsToStatic.Checked = true;
            this.CustomRadioButtonSettingUnsetDnsToStatic.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSettingUnsetDnsToStatic.Location = new System.Drawing.Point(50, 70);
            this.CustomRadioButtonSettingUnsetDnsToStatic.Name = "CustomRadioButtonSettingUnsetDnsToStatic";
            this.CustomRadioButtonSettingUnsetDnsToStatic.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSettingUnsetDnsToStatic.Size = new System.Drawing.Size(131, 17);
            this.CustomRadioButtonSettingUnsetDnsToStatic.TabIndex = 1;
            this.CustomRadioButtonSettingUnsetDnsToStatic.TabStop = true;
            this.CustomRadioButtonSettingUnsetDnsToStatic.Text = "Unset DNS to Static.";
            this.CustomRadioButtonSettingUnsetDnsToStatic.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSettingUnsetDnsToDhcp
            // 
            this.CustomRadioButtonSettingUnsetDnsToDhcp.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSettingUnsetDnsToDhcp.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingUnsetDnsToDhcp.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingUnsetDnsToDhcp.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSettingUnsetDnsToDhcp.Location = new System.Drawing.Point(50, 35);
            this.CustomRadioButtonSettingUnsetDnsToDhcp.Name = "CustomRadioButtonSettingUnsetDnsToDhcp";
            this.CustomRadioButtonSettingUnsetDnsToDhcp.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSettingUnsetDnsToDhcp.Size = new System.Drawing.Size(130, 17);
            this.CustomRadioButtonSettingUnsetDnsToDhcp.TabIndex = 0;
            this.CustomRadioButtonSettingUnsetDnsToDhcp.Text = "Unset DNS to DHCP";
            this.CustomRadioButtonSettingUnsetDnsToDhcp.UseVisualStyleBackColor = false;
            // 
            // TabPageSettingsCPU
            // 
            this.TabPageSettingsCPU.BackColor = System.Drawing.Color.Transparent;
            this.TabPageSettingsCPU.Controls.Add(this.CustomRadioButtonSettingCPULow);
            this.TabPageSettingsCPU.Controls.Add(this.CustomRadioButtonSettingCPUBelowNormal);
            this.TabPageSettingsCPU.Controls.Add(this.CustomRadioButtonSettingCPUNormal);
            this.TabPageSettingsCPU.Controls.Add(this.CustomRadioButtonSettingCPUAboveNormal);
            this.TabPageSettingsCPU.Controls.Add(this.CustomRadioButtonSettingCPUHigh);
            this.TabPageSettingsCPU.Controls.Add(this.CustomLabelSettingInfoCPU);
            this.TabPageSettingsCPU.Location = new System.Drawing.Point(94, 4);
            this.TabPageSettingsCPU.Name = "TabPageSettingsCPU";
            this.TabPageSettingsCPU.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSettingsCPU.Size = new System.Drawing.Size(588, 357);
            this.TabPageSettingsCPU.TabIndex = 1;
            this.TabPageSettingsCPU.Tag = 4;
            this.TabPageSettingsCPU.Text = "CPU";
            // 
            // CustomRadioButtonSettingCPULow
            // 
            this.CustomRadioButtonSettingCPULow.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSettingCPULow.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPULow.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPULow.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSettingCPULow.Location = new System.Drawing.Point(50, 180);
            this.CustomRadioButtonSettingCPULow.Name = "CustomRadioButtonSettingCPULow";
            this.CustomRadioButtonSettingCPULow.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSettingCPULow.Size = new System.Drawing.Size(42, 17);
            this.CustomRadioButtonSettingCPULow.TabIndex = 5;
            this.CustomRadioButtonSettingCPULow.Text = "Low";
            this.CustomRadioButtonSettingCPULow.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSettingCPUBelowNormal
            // 
            this.CustomRadioButtonSettingCPUBelowNormal.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSettingCPUBelowNormal.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPUBelowNormal.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPUBelowNormal.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSettingCPUBelowNormal.Location = new System.Drawing.Point(50, 155);
            this.CustomRadioButtonSettingCPUBelowNormal.Name = "CustomRadioButtonSettingCPUBelowNormal";
            this.CustomRadioButtonSettingCPUBelowNormal.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSettingCPUBelowNormal.Size = new System.Drawing.Size(95, 17);
            this.CustomRadioButtonSettingCPUBelowNormal.TabIndex = 4;
            this.CustomRadioButtonSettingCPUBelowNormal.Text = "Below normal";
            this.CustomRadioButtonSettingCPUBelowNormal.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSettingCPUNormal
            // 
            this.CustomRadioButtonSettingCPUNormal.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSettingCPUNormal.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPUNormal.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPUNormal.Checked = true;
            this.CustomRadioButtonSettingCPUNormal.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSettingCPUNormal.Location = new System.Drawing.Point(50, 130);
            this.CustomRadioButtonSettingCPUNormal.Name = "CustomRadioButtonSettingCPUNormal";
            this.CustomRadioButtonSettingCPUNormal.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSettingCPUNormal.Size = new System.Drawing.Size(61, 17);
            this.CustomRadioButtonSettingCPUNormal.TabIndex = 3;
            this.CustomRadioButtonSettingCPUNormal.TabStop = true;
            this.CustomRadioButtonSettingCPUNormal.Text = "Normal";
            this.CustomRadioButtonSettingCPUNormal.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSettingCPUAboveNormal
            // 
            this.CustomRadioButtonSettingCPUAboveNormal.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSettingCPUAboveNormal.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPUAboveNormal.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPUAboveNormal.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSettingCPUAboveNormal.Location = new System.Drawing.Point(50, 105);
            this.CustomRadioButtonSettingCPUAboveNormal.Name = "CustomRadioButtonSettingCPUAboveNormal";
            this.CustomRadioButtonSettingCPUAboveNormal.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSettingCPUAboveNormal.Size = new System.Drawing.Size(97, 17);
            this.CustomRadioButtonSettingCPUAboveNormal.TabIndex = 2;
            this.CustomRadioButtonSettingCPUAboveNormal.Text = "Above normal";
            this.CustomRadioButtonSettingCPUAboveNormal.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSettingCPUHigh
            // 
            this.CustomRadioButtonSettingCPUHigh.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSettingCPUHigh.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPUHigh.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSettingCPUHigh.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSettingCPUHigh.Location = new System.Drawing.Point(50, 80);
            this.CustomRadioButtonSettingCPUHigh.Name = "CustomRadioButtonSettingCPUHigh";
            this.CustomRadioButtonSettingCPUHigh.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSettingCPUHigh.Size = new System.Drawing.Size(47, 17);
            this.CustomRadioButtonSettingCPUHigh.TabIndex = 1;
            this.CustomRadioButtonSettingCPUHigh.Text = "High";
            this.CustomRadioButtonSettingCPUHigh.UseVisualStyleBackColor = false;
            // 
            // CustomLabelSettingInfoCPU
            // 
            this.CustomLabelSettingInfoCPU.AutoSize = true;
            this.CustomLabelSettingInfoCPU.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingInfoCPU.Border = false;
            this.CustomLabelSettingInfoCPU.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingInfoCPU.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingInfoCPU.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingInfoCPU.Location = new System.Drawing.Point(50, 35);
            this.CustomLabelSettingInfoCPU.Name = "CustomLabelSettingInfoCPU";
            this.CustomLabelSettingInfoCPU.RoundedCorners = 0;
            this.CustomLabelSettingInfoCPU.Size = new System.Drawing.Size(132, 15);
            this.CustomLabelSettingInfoCPU.TabIndex = 0;
            this.CustomLabelSettingInfoCPU.Text = "Set processing priorities";
            // 
            // TabPageSettingsOthers
            // 
            this.TabPageSettingsOthers.BackColor = System.Drawing.Color.Transparent;
            this.TabPageSettingsOthers.Controls.Add(this.CustomNumericUpDownSettingFallbackDnsPort);
            this.TabPageSettingsOthers.Controls.Add(this.CustomLabelSettingFallbackDnsPort);
            this.TabPageSettingsOthers.Controls.Add(this.CustomTextBoxSettingFallbackDnsIP);
            this.TabPageSettingsOthers.Controls.Add(this.CustomLabelSettingFallbackDnsIP);
            this.TabPageSettingsOthers.Controls.Add(this.CustomNumericUpDownSettingBootstrapDnsPort);
            this.TabPageSettingsOthers.Controls.Add(this.CustomLabelSettingBootstrapDnsPort);
            this.TabPageSettingsOthers.Controls.Add(this.CustomButtonSettingRestoreDefault);
            this.TabPageSettingsOthers.Controls.Add(this.CustomCheckBoxSettingDisableAudioAlert);
            this.TabPageSettingsOthers.Controls.Add(this.CustomLabelSettingBootstrapDnsIP);
            this.TabPageSettingsOthers.Controls.Add(this.CustomCheckBoxSettingDontAskCertificate);
            this.TabPageSettingsOthers.Controls.Add(this.CustomTextBoxSettingBootstrapDnsIP);
            this.TabPageSettingsOthers.Location = new System.Drawing.Point(94, 4);
            this.TabPageSettingsOthers.Name = "TabPageSettingsOthers";
            this.TabPageSettingsOthers.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageSettingsOthers.Size = new System.Drawing.Size(588, 357);
            this.TabPageSettingsOthers.TabIndex = 2;
            this.TabPageSettingsOthers.Tag = 5;
            this.TabPageSettingsOthers.Text = "Others";
            // 
            // CustomNumericUpDownSettingFallbackDnsPort
            // 
            this.CustomNumericUpDownSettingFallbackDnsPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownSettingFallbackDnsPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownSettingFallbackDnsPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownSettingFallbackDnsPort.Enabled = false;
            this.CustomNumericUpDownSettingFallbackDnsPort.Location = new System.Drawing.Point(395, 68);
            this.CustomNumericUpDownSettingFallbackDnsPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.CustomNumericUpDownSettingFallbackDnsPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownSettingFallbackDnsPort.Name = "CustomNumericUpDownSettingFallbackDnsPort";
            this.CustomNumericUpDownSettingFallbackDnsPort.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownSettingFallbackDnsPort.TabIndex = 13;
            this.CustomNumericUpDownSettingFallbackDnsPort.Value = new decimal(new int[] {
            53,
            0,
            0,
            0});
            // 
            // CustomLabelSettingFallbackDnsPort
            // 
            this.CustomLabelSettingFallbackDnsPort.AutoSize = true;
            this.CustomLabelSettingFallbackDnsPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingFallbackDnsPort.Border = false;
            this.CustomLabelSettingFallbackDnsPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingFallbackDnsPort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingFallbackDnsPort.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingFallbackDnsPort.Location = new System.Drawing.Point(275, 70);
            this.CustomLabelSettingFallbackDnsPort.Name = "CustomLabelSettingFallbackDnsPort";
            this.CustomLabelSettingFallbackDnsPort.RoundedCorners = 0;
            this.CustomLabelSettingFallbackDnsPort.Size = new System.Drawing.Size(104, 15);
            this.CustomLabelSettingFallbackDnsPort.TabIndex = 12;
            this.CustomLabelSettingFallbackDnsPort.Text = "Fallback DNS Port:";
            // 
            // CustomTextBoxSettingFallbackDnsIP
            // 
            this.CustomTextBoxSettingFallbackDnsIP.AcceptsReturn = false;
            this.CustomTextBoxSettingFallbackDnsIP.AcceptsTab = false;
            this.CustomTextBoxSettingFallbackDnsIP.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxSettingFallbackDnsIP.Border = true;
            this.CustomTextBoxSettingFallbackDnsIP.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxSettingFallbackDnsIP.BorderSize = 1;
            this.CustomTextBoxSettingFallbackDnsIP.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxSettingFallbackDnsIP.Enabled = false;
            this.CustomTextBoxSettingFallbackDnsIP.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxSettingFallbackDnsIP.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxSettingFallbackDnsIP.HideSelection = true;
            this.CustomTextBoxSettingFallbackDnsIP.Location = new System.Drawing.Point(120, 68);
            this.CustomTextBoxSettingFallbackDnsIP.MaxLength = 32767;
            this.CustomTextBoxSettingFallbackDnsIP.Multiline = false;
            this.CustomTextBoxSettingFallbackDnsIP.Name = "CustomTextBoxSettingFallbackDnsIP";
            this.CustomTextBoxSettingFallbackDnsIP.ReadOnly = false;
            this.CustomTextBoxSettingFallbackDnsIP.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxSettingFallbackDnsIP.ShortcutsEnabled = true;
            this.CustomTextBoxSettingFallbackDnsIP.Size = new System.Drawing.Size(95, 23);
            this.CustomTextBoxSettingFallbackDnsIP.TabIndex = 0;
            this.CustomTextBoxSettingFallbackDnsIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxSettingFallbackDnsIP.Texts = "8.8.8.8";
            this.CustomTextBoxSettingFallbackDnsIP.UnderlinedStyle = false;
            this.CustomTextBoxSettingFallbackDnsIP.UsePasswordChar = false;
            this.CustomTextBoxSettingFallbackDnsIP.WordWrap = true;
            // 
            // CustomLabelSettingFallbackDnsIP
            // 
            this.CustomLabelSettingFallbackDnsIP.AutoSize = true;
            this.CustomLabelSettingFallbackDnsIP.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingFallbackDnsIP.Border = false;
            this.CustomLabelSettingFallbackDnsIP.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingFallbackDnsIP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingFallbackDnsIP.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingFallbackDnsIP.Location = new System.Drawing.Point(15, 70);
            this.CustomLabelSettingFallbackDnsIP.Name = "CustomLabelSettingFallbackDnsIP";
            this.CustomLabelSettingFallbackDnsIP.RoundedCorners = 0;
            this.CustomLabelSettingFallbackDnsIP.Size = new System.Drawing.Size(92, 15);
            this.CustomLabelSettingFallbackDnsIP.TabIndex = 10;
            this.CustomLabelSettingFallbackDnsIP.Text = "Fallback DNS IP:";
            // 
            // CustomNumericUpDownSettingBootstrapDnsPort
            // 
            this.CustomNumericUpDownSettingBootstrapDnsPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomNumericUpDownSettingBootstrapDnsPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomNumericUpDownSettingBootstrapDnsPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CustomNumericUpDownSettingBootstrapDnsPort.Location = new System.Drawing.Point(395, 18);
            this.CustomNumericUpDownSettingBootstrapDnsPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.CustomNumericUpDownSettingBootstrapDnsPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CustomNumericUpDownSettingBootstrapDnsPort.Name = "CustomNumericUpDownSettingBootstrapDnsPort";
            this.CustomNumericUpDownSettingBootstrapDnsPort.Size = new System.Drawing.Size(53, 23);
            this.CustomNumericUpDownSettingBootstrapDnsPort.TabIndex = 9;
            this.CustomNumericUpDownSettingBootstrapDnsPort.Value = new decimal(new int[] {
            53,
            0,
            0,
            0});
            // 
            // CustomLabelSettingBootstrapDnsPort
            // 
            this.CustomLabelSettingBootstrapDnsPort.AutoSize = true;
            this.CustomLabelSettingBootstrapDnsPort.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingBootstrapDnsPort.Border = false;
            this.CustomLabelSettingBootstrapDnsPort.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingBootstrapDnsPort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingBootstrapDnsPort.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingBootstrapDnsPort.Location = new System.Drawing.Point(275, 20);
            this.CustomLabelSettingBootstrapDnsPort.Name = "CustomLabelSettingBootstrapDnsPort";
            this.CustomLabelSettingBootstrapDnsPort.RoundedCorners = 0;
            this.CustomLabelSettingBootstrapDnsPort.Size = new System.Drawing.Size(112, 15);
            this.CustomLabelSettingBootstrapDnsPort.TabIndex = 8;
            this.CustomLabelSettingBootstrapDnsPort.Text = "Bootstrap DNS Port:";
            // 
            // CustomButtonSettingRestoreDefault
            // 
            this.CustomButtonSettingRestoreDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CustomButtonSettingRestoreDefault.AutoSize = true;
            this.CustomButtonSettingRestoreDefault.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonSettingRestoreDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonSettingRestoreDefault.Location = new System.Drawing.Point(40, 324);
            this.CustomButtonSettingRestoreDefault.Name = "CustomButtonSettingRestoreDefault";
            this.CustomButtonSettingRestoreDefault.RoundedCorners = 0;
            this.CustomButtonSettingRestoreDefault.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonSettingRestoreDefault.Size = new System.Drawing.Size(171, 27);
            this.CustomButtonSettingRestoreDefault.TabIndex = 2;
            this.CustomButtonSettingRestoreDefault.Text = "Restore all settings to default";
            this.CustomButtonSettingRestoreDefault.UseVisualStyleBackColor = true;
            this.CustomButtonSettingRestoreDefault.Click += new System.EventHandler(this.CustomButtonRestoreDefault_Click);
            // 
            // CustomCheckBoxSettingDisableAudioAlert
            // 
            this.CustomCheckBoxSettingDisableAudioAlert.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxSettingDisableAudioAlert.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingDisableAudioAlert.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingDisableAudioAlert.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxSettingDisableAudioAlert.Location = new System.Drawing.Point(15, 170);
            this.CustomCheckBoxSettingDisableAudioAlert.Name = "CustomCheckBoxSettingDisableAudioAlert";
            this.CustomCheckBoxSettingDisableAudioAlert.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxSettingDisableAudioAlert.Size = new System.Drawing.Size(125, 17);
            this.CustomCheckBoxSettingDisableAudioAlert.TabIndex = 6;
            this.CustomCheckBoxSettingDisableAudioAlert.Text = "Disable audio alert.";
            this.CustomCheckBoxSettingDisableAudioAlert.UseVisualStyleBackColor = false;
            // 
            // CustomLabelSettingBootstrapDnsIP
            // 
            this.CustomLabelSettingBootstrapDnsIP.AutoSize = true;
            this.CustomLabelSettingBootstrapDnsIP.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSettingBootstrapDnsIP.Border = false;
            this.CustomLabelSettingBootstrapDnsIP.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSettingBootstrapDnsIP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSettingBootstrapDnsIP.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSettingBootstrapDnsIP.Location = new System.Drawing.Point(15, 20);
            this.CustomLabelSettingBootstrapDnsIP.Name = "CustomLabelSettingBootstrapDnsIP";
            this.CustomLabelSettingBootstrapDnsIP.RoundedCorners = 0;
            this.CustomLabelSettingBootstrapDnsIP.Size = new System.Drawing.Size(100, 15);
            this.CustomLabelSettingBootstrapDnsIP.TabIndex = 3;
            this.CustomLabelSettingBootstrapDnsIP.Text = "Bootstrap DNS IP:";
            // 
            // CustomCheckBoxSettingDontAskCertificate
            // 
            this.CustomCheckBoxSettingDontAskCertificate.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxSettingDontAskCertificate.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingDontAskCertificate.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxSettingDontAskCertificate.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxSettingDontAskCertificate.Location = new System.Drawing.Point(15, 120);
            this.CustomCheckBoxSettingDontAskCertificate.Name = "CustomCheckBoxSettingDontAskCertificate";
            this.CustomCheckBoxSettingDontAskCertificate.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxSettingDontAskCertificate.Size = new System.Drawing.Size(210, 17);
            this.CustomCheckBoxSettingDontAskCertificate.TabIndex = 4;
            this.CustomCheckBoxSettingDontAskCertificate.Text = "Don\'t ask for certificate every time.";
            this.CustomCheckBoxSettingDontAskCertificate.UseVisualStyleBackColor = false;
            // 
            // CustomTextBoxSettingBootstrapDnsIP
            // 
            this.CustomTextBoxSettingBootstrapDnsIP.AcceptsReturn = false;
            this.CustomTextBoxSettingBootstrapDnsIP.AcceptsTab = false;
            this.CustomTextBoxSettingBootstrapDnsIP.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxSettingBootstrapDnsIP.Border = true;
            this.CustomTextBoxSettingBootstrapDnsIP.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxSettingBootstrapDnsIP.BorderSize = 1;
            this.CustomTextBoxSettingBootstrapDnsIP.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxSettingBootstrapDnsIP.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxSettingBootstrapDnsIP.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxSettingBootstrapDnsIP.HideSelection = true;
            this.CustomTextBoxSettingBootstrapDnsIP.Location = new System.Drawing.Point(120, 18);
            this.CustomTextBoxSettingBootstrapDnsIP.MaxLength = 32767;
            this.CustomTextBoxSettingBootstrapDnsIP.Multiline = false;
            this.CustomTextBoxSettingBootstrapDnsIP.Name = "CustomTextBoxSettingBootstrapDnsIP";
            this.CustomTextBoxSettingBootstrapDnsIP.ReadOnly = false;
            this.CustomTextBoxSettingBootstrapDnsIP.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxSettingBootstrapDnsIP.ShortcutsEnabled = true;
            this.CustomTextBoxSettingBootstrapDnsIP.Size = new System.Drawing.Size(95, 23);
            this.CustomTextBoxSettingBootstrapDnsIP.TabIndex = 0;
            this.CustomTextBoxSettingBootstrapDnsIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxSettingBootstrapDnsIP.Texts = "8.8.8.8";
            this.CustomTextBoxSettingBootstrapDnsIP.UnderlinedStyle = false;
            this.CustomTextBoxSettingBootstrapDnsIP.UsePasswordChar = false;
            this.CustomTextBoxSettingBootstrapDnsIP.WordWrap = true;
            // 
            // TabPageAbout
            // 
            this.TabPageAbout.BackColor = System.Drawing.Color.Transparent;
            this.TabPageAbout.Controls.Add(this.LinkLabelStAlidxdydz);
            this.TabPageAbout.Controls.Add(this.CustomLabelAboutSpecialThanks);
            this.TabPageAbout.Controls.Add(this.LinkLabelGoodbyeDPI);
            this.TabPageAbout.Controls.Add(this.LinkLabelDNSCrypt);
            this.TabPageAbout.Controls.Add(this.LinkLabelDNSProxy);
            this.TabPageAbout.Controls.Add(this.LinkLabelDNSLookup);
            this.TabPageAbout.Controls.Add(this.CustomLabelAboutUsing);
            this.TabPageAbout.Controls.Add(this.CustomLabelAboutVersion);
            this.TabPageAbout.Controls.Add(this.CustomLabelAboutThis2);
            this.TabPageAbout.Controls.Add(this.CustomLabelAboutThis);
            this.TabPageAbout.Controls.Add(this.PictureBoxAbout);
            this.TabPageAbout.Location = new System.Drawing.Point(4, 25);
            this.TabPageAbout.Name = "TabPageAbout";
            this.TabPageAbout.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageAbout.Size = new System.Drawing.Size(692, 371);
            this.TabPageAbout.TabIndex = 2;
            this.TabPageAbout.Tag = 2;
            this.TabPageAbout.Text = "About";
            // 
            // LinkLabelStAlidxdydz
            // 
            this.LinkLabelStAlidxdydz.AutoSize = true;
            this.LinkLabelStAlidxdydz.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.LinkLabelStAlidxdydz.Location = new System.Drawing.Point(461, 174);
            this.LinkLabelStAlidxdydz.Name = "LinkLabelStAlidxdydz";
            this.LinkLabelStAlidxdydz.Size = new System.Drawing.Size(59, 15);
            this.LinkLabelStAlidxdydz.TabIndex = 10;
            this.LinkLabelStAlidxdydz.TabStop = true;
            this.LinkLabelStAlidxdydz.Text = "Alidxdydz";
            this.LinkLabelStAlidxdydz.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelStAlidxdydz_LinkClicked);
            // 
            // CustomLabelAboutSpecialThanks
            // 
            this.CustomLabelAboutSpecialThanks.AutoSize = true;
            this.CustomLabelAboutSpecialThanks.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelAboutSpecialThanks.Border = false;
            this.CustomLabelAboutSpecialThanks.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelAboutSpecialThanks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelAboutSpecialThanks.ForeColor = System.Drawing.Color.White;
            this.CustomLabelAboutSpecialThanks.Location = new System.Drawing.Point(446, 135);
            this.CustomLabelAboutSpecialThanks.Name = "CustomLabelAboutSpecialThanks";
            this.CustomLabelAboutSpecialThanks.RoundedCorners = 0;
            this.CustomLabelAboutSpecialThanks.Size = new System.Drawing.Size(81, 75);
            this.CustomLabelAboutSpecialThanks.TabIndex = 9;
            this.CustomLabelAboutSpecialThanks.Text = "special thanks\r\n{\r\n\r\n\r\n}";
            // 
            // LinkLabelGoodbyeDPI
            // 
            this.LinkLabelGoodbyeDPI.AutoSize = true;
            this.LinkLabelGoodbyeDPI.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.LinkLabelGoodbyeDPI.Location = new System.Drawing.Point(285, 230);
            this.LinkLabelGoodbyeDPI.Name = "LinkLabelGoodbyeDPI";
            this.LinkLabelGoodbyeDPI.Size = new System.Drawing.Size(76, 15);
            this.LinkLabelGoodbyeDPI.TabIndex = 6;
            this.LinkLabelGoodbyeDPI.TabStop = true;
            this.LinkLabelGoodbyeDPI.Text = "GoodbyeDPI;";
            this.LinkLabelGoodbyeDPI.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelGoodbyeDPI_LinkClicked);
            // 
            // LinkLabelDNSCrypt
            // 
            this.LinkLabelDNSCrypt.AutoSize = true;
            this.LinkLabelDNSCrypt.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.LinkLabelDNSCrypt.Location = new System.Drawing.Point(285, 208);
            this.LinkLabelDNSCrypt.Name = "LinkLabelDNSCrypt";
            this.LinkLabelDNSCrypt.Size = new System.Drawing.Size(62, 15);
            this.LinkLabelDNSCrypt.TabIndex = 5;
            this.LinkLabelDNSCrypt.TabStop = true;
            this.LinkLabelDNSCrypt.Text = "DNSCrypt;";
            this.LinkLabelDNSCrypt.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelDNSCrypt_LinkClicked);
            // 
            // LinkLabelDNSProxy
            // 
            this.LinkLabelDNSProxy.AutoSize = true;
            this.LinkLabelDNSProxy.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.LinkLabelDNSProxy.Location = new System.Drawing.Point(285, 185);
            this.LinkLabelDNSProxy.Name = "LinkLabelDNSProxy";
            this.LinkLabelDNSProxy.Size = new System.Drawing.Size(160, 15);
            this.LinkLabelDNSProxy.TabIndex = 4;
            this.LinkLabelDNSProxy.TabStop = true;
            this.LinkLabelDNSProxy.Text = "DNSProxy by AdGuard Team;";
            this.LinkLabelDNSProxy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelDNSProxy_LinkClicked);
            // 
            // LinkLabelDNSLookup
            // 
            this.LinkLabelDNSLookup.AutoSize = true;
            this.LinkLabelDNSLookup.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.LinkLabelDNSLookup.Location = new System.Drawing.Point(285, 163);
            this.LinkLabelDNSLookup.Name = "LinkLabelDNSLookup";
            this.LinkLabelDNSLookup.Size = new System.Drawing.Size(73, 15);
            this.LinkLabelDNSLookup.TabIndex = 3;
            this.LinkLabelDNSLookup.TabStop = true;
            this.LinkLabelDNSLookup.Text = "DNSLookup;";
            this.LinkLabelDNSLookup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelDNSLookup_LinkClicked);
            // 
            // CustomLabelAboutUsing
            // 
            this.CustomLabelAboutUsing.AutoSize = true;
            this.CustomLabelAboutUsing.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelAboutUsing.Border = false;
            this.CustomLabelAboutUsing.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelAboutUsing.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelAboutUsing.ForeColor = System.Drawing.Color.White;
            this.CustomLabelAboutUsing.Location = new System.Drawing.Point(270, 135);
            this.CustomLabelAboutUsing.Name = "CustomLabelAboutUsing";
            this.CustomLabelAboutUsing.RoundedCorners = 0;
            this.CustomLabelAboutUsing.Size = new System.Drawing.Size(36, 120);
            this.CustomLabelAboutUsing.TabIndex = 8;
            this.CustomLabelAboutUsing.Text = "using\r\n{\r\n\r\n\r\n\r\n\r\n\r\n}";
            // 
            // CustomLabelAboutVersion
            // 
            this.CustomLabelAboutVersion.AutoSize = true;
            this.CustomLabelAboutVersion.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelAboutVersion.Border = false;
            this.CustomLabelAboutVersion.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelAboutVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelAboutVersion.ForeColor = System.Drawing.Color.White;
            this.CustomLabelAboutVersion.Location = new System.Drawing.Point(600, 51);
            this.CustomLabelAboutVersion.Name = "CustomLabelAboutVersion";
            this.CustomLabelAboutVersion.RoundedCorners = 0;
            this.CustomLabelAboutVersion.Size = new System.Drawing.Size(45, 15);
            this.CustomLabelAboutVersion.TabIndex = 7;
            this.CustomLabelAboutVersion.Text = "Version";
            // 
            // CustomLabelAboutThis2
            // 
            this.CustomLabelAboutThis2.AutoSize = true;
            this.CustomLabelAboutThis2.BackColor = System.Drawing.Color.Transparent;
            this.CustomLabelAboutThis2.Border = false;
            this.CustomLabelAboutThis2.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelAboutThis2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelAboutThis2.ForeColor = System.Drawing.Color.IndianRed;
            this.CustomLabelAboutThis2.Location = new System.Drawing.Point(267, 75);
            this.CustomLabelAboutThis2.Name = "CustomLabelAboutThis2";
            this.CustomLabelAboutThis2.RoundedCorners = 0;
            this.CustomLabelAboutThis2.Size = new System.Drawing.Size(333, 15);
            this.CustomLabelAboutThis2.TabIndex = 2;
            this.CustomLabelAboutThis2.Text = "A GUI for DNSLookup, DNSProxy, DNSCrypt and GoodbyeDPI.";
            // 
            // CustomLabelAboutThis
            // 
            this.CustomLabelAboutThis.AutoSize = true;
            this.CustomLabelAboutThis.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelAboutThis.Border = false;
            this.CustomLabelAboutThis.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelAboutThis.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CustomLabelAboutThis.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelAboutThis.Font = new System.Drawing.Font("Verdana", 19F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CustomLabelAboutThis.ForeColor = System.Drawing.Color.DodgerBlue;
            this.CustomLabelAboutThis.Location = new System.Drawing.Point(235, 33);
            this.CustomLabelAboutThis.Name = "CustomLabelAboutThis";
            this.CustomLabelAboutThis.RoundedCorners = 0;
            this.CustomLabelAboutThis.Size = new System.Drawing.Size(367, 32);
            this.CustomLabelAboutThis.TabIndex = 1;
            this.CustomLabelAboutThis.Text = "SDC - Secure DNS Client";
            this.CustomLabelAboutThis.Click += new System.EventHandler(this.CustomLabelAboutThis_Click);
            // 
            // PictureBoxAbout
            // 
            this.PictureBoxAbout.Image = global::SecureDNSClient.Properties.Resources.SecureDNSClient;
            this.PictureBoxAbout.Location = new System.Drawing.Point(55, 35);
            this.PictureBoxAbout.Name = "PictureBoxAbout";
            this.PictureBoxAbout.Size = new System.Drawing.Size(128, 128);
            this.PictureBoxAbout.TabIndex = 0;
            this.PictureBoxAbout.TabStop = false;
            // 
            // CustomButtonToggleLogView
            // 
            this.CustomButtonToggleLogView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomButtonToggleLogView.AutoSize = true;
            this.CustomButtonToggleLogView.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonToggleLogView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonToggleLogView.Location = new System.Drawing.Point(144, 362);
            this.CustomButtonToggleLogView.Name = "CustomButtonToggleLogView";
            this.CustomButtonToggleLogView.RoundedCorners = 5;
            this.CustomButtonToggleLogView.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonToggleLogView.Size = new System.Drawing.Size(25, 27);
            this.CustomButtonToggleLogView.TabIndex = 10;
            this.CustomButtonToggleLogView.Text = "T";
            this.CustomButtonToggleLogView.UseVisualStyleBackColor = true;
            this.CustomButtonToggleLogView.Click += new System.EventHandler(this.CustomButtonToggleLogView_Click);
            // 
            // NotifyIconMain
            // 
            this.NotifyIconMain.ContextMenuStrip = this.CustomContextMenuStripIcon;
            this.NotifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIconMain.Icon")));
            this.NotifyIconMain.Visible = true;
            this.NotifyIconMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIconMain_MouseClick);
            // 
            // CustomContextMenuStripIcon
            // 
            this.CustomContextMenuStripIcon.BackColor = System.Drawing.Color.DimGray;
            this.CustomContextMenuStripIcon.BorderColor = System.Drawing.Color.Blue;
            this.CustomContextMenuStripIcon.ForeColor = System.Drawing.Color.White;
            this.CustomContextMenuStripIcon.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.CustomContextMenuStripIcon.Name = "CustomContextMenuStripIcon";
            this.CustomContextMenuStripIcon.SameColorForSubItems = true;
            this.CustomContextMenuStripIcon.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomContextMenuStripIcon.Size = new System.Drawing.Size(61, 4);
            // 
            // CustomGroupBoxStatus
            // 
            this.CustomGroupBoxStatus.BorderColor = System.Drawing.Color.Blue;
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusGoodbyeDPI);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusProxyRequests);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusLocalDoHLatency);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomButtonToggleLogView);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusLocalDoH);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusLocalDnsLatency);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusLocalDNS);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusIsProxySet);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusIsSharing);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusIsDNSSet);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusProxyDpiBypass);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusIsConnected);
            this.CustomGroupBoxStatus.Controls.Add(this.CustomRichTextBoxStatusWorkingServers);
            this.CustomGroupBoxStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomGroupBoxStatus.Location = new System.Drawing.Point(0, 0);
            this.CustomGroupBoxStatus.Margin = new System.Windows.Forms.Padding(1);
            this.CustomGroupBoxStatus.Name = "CustomGroupBoxStatus";
            this.CustomGroupBoxStatus.Size = new System.Drawing.Size(180, 400);
            this.CustomGroupBoxStatus.TabIndex = 8;
            this.CustomGroupBoxStatus.TabStop = false;
            this.CustomGroupBoxStatus.Text = "Status";
            // 
            // CustomRichTextBoxStatusGoodbyeDPI
            // 
            this.CustomRichTextBoxStatusGoodbyeDPI.AcceptsTab = false;
            this.CustomRichTextBoxStatusGoodbyeDPI.AutoWordSelection = false;
            this.CustomRichTextBoxStatusGoodbyeDPI.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusGoodbyeDPI.Border = false;
            this.CustomRichTextBoxStatusGoodbyeDPI.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusGoodbyeDPI.BorderSize = 1;
            this.CustomRichTextBoxStatusGoodbyeDPI.BulletIndent = 0;
            this.CustomRichTextBoxStatusGoodbyeDPI.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusGoodbyeDPI.DetectUrls = true;
            this.CustomRichTextBoxStatusGoodbyeDPI.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusGoodbyeDPI.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusGoodbyeDPI.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusGoodbyeDPI.HideSelection = true;
            this.CustomRichTextBoxStatusGoodbyeDPI.Location = new System.Drawing.Point(5, 290);
            this.CustomRichTextBoxStatusGoodbyeDPI.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusGoodbyeDPI.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusGoodbyeDPI.Multiline = false;
            this.CustomRichTextBoxStatusGoodbyeDPI.Name = "CustomRichTextBoxStatusGoodbyeDPI";
            this.CustomRichTextBoxStatusGoodbyeDPI.ReadOnly = true;
            this.CustomRichTextBoxStatusGoodbyeDPI.RightMargin = 0;
            this.CustomRichTextBoxStatusGoodbyeDPI.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusGoodbyeDPI.ScrollToBottom = false;
            this.CustomRichTextBoxStatusGoodbyeDPI.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusGoodbyeDPI.SelectionLength = 0;
            this.CustomRichTextBoxStatusGoodbyeDPI.SelectionStart = 0;
            this.CustomRichTextBoxStatusGoodbyeDPI.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusGoodbyeDPI.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusGoodbyeDPI.TabIndex = 0;
            this.CustomRichTextBoxStatusGoodbyeDPI.Texts = "GoodbyeDPI: Inactive";
            this.CustomRichTextBoxStatusGoodbyeDPI.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusGoodbyeDPI.WordWrap = false;
            this.CustomRichTextBoxStatusGoodbyeDPI.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusProxyRequests
            // 
            this.CustomRichTextBoxStatusProxyRequests.AcceptsTab = false;
            this.CustomRichTextBoxStatusProxyRequests.AutoWordSelection = false;
            this.CustomRichTextBoxStatusProxyRequests.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusProxyRequests.Border = false;
            this.CustomRichTextBoxStatusProxyRequests.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusProxyRequests.BorderSize = 1;
            this.CustomRichTextBoxStatusProxyRequests.BulletIndent = 0;
            this.CustomRichTextBoxStatusProxyRequests.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusProxyRequests.DetectUrls = true;
            this.CustomRichTextBoxStatusProxyRequests.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusProxyRequests.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusProxyRequests.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusProxyRequests.HideSelection = true;
            this.CustomRichTextBoxStatusProxyRequests.Location = new System.Drawing.Point(5, 215);
            this.CustomRichTextBoxStatusProxyRequests.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusProxyRequests.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusProxyRequests.Multiline = false;
            this.CustomRichTextBoxStatusProxyRequests.Name = "CustomRichTextBoxStatusProxyRequests";
            this.CustomRichTextBoxStatusProxyRequests.ReadOnly = true;
            this.CustomRichTextBoxStatusProxyRequests.RightMargin = 0;
            this.CustomRichTextBoxStatusProxyRequests.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusProxyRequests.ScrollToBottom = false;
            this.CustomRichTextBoxStatusProxyRequests.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusProxyRequests.SelectionLength = 0;
            this.CustomRichTextBoxStatusProxyRequests.SelectionStart = 0;
            this.CustomRichTextBoxStatusProxyRequests.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusProxyRequests.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusProxyRequests.TabIndex = 0;
            this.CustomRichTextBoxStatusProxyRequests.Texts = "Proxy Requests 20000 of 20000";
            this.CustomRichTextBoxStatusProxyRequests.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusProxyRequests.WordWrap = false;
            this.CustomRichTextBoxStatusProxyRequests.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusLocalDoHLatency
            // 
            this.CustomRichTextBoxStatusLocalDoHLatency.AcceptsTab = false;
            this.CustomRichTextBoxStatusLocalDoHLatency.AutoWordSelection = false;
            this.CustomRichTextBoxStatusLocalDoHLatency.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusLocalDoHLatency.Border = false;
            this.CustomRichTextBoxStatusLocalDoHLatency.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusLocalDoHLatency.BorderSize = 1;
            this.CustomRichTextBoxStatusLocalDoHLatency.BulletIndent = 0;
            this.CustomRichTextBoxStatusLocalDoHLatency.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusLocalDoHLatency.DetectUrls = true;
            this.CustomRichTextBoxStatusLocalDoHLatency.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusLocalDoHLatency.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusLocalDoHLatency.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusLocalDoHLatency.HideSelection = true;
            this.CustomRichTextBoxStatusLocalDoHLatency.Location = new System.Drawing.Point(5, 140);
            this.CustomRichTextBoxStatusLocalDoHLatency.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusLocalDoHLatency.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusLocalDoHLatency.Multiline = false;
            this.CustomRichTextBoxStatusLocalDoHLatency.Name = "CustomRichTextBoxStatusLocalDoHLatency";
            this.CustomRichTextBoxStatusLocalDoHLatency.ReadOnly = true;
            this.CustomRichTextBoxStatusLocalDoHLatency.RightMargin = 0;
            this.CustomRichTextBoxStatusLocalDoHLatency.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusLocalDoHLatency.ScrollToBottom = false;
            this.CustomRichTextBoxStatusLocalDoHLatency.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusLocalDoHLatency.SelectionLength = 0;
            this.CustomRichTextBoxStatusLocalDoHLatency.SelectionStart = 0;
            this.CustomRichTextBoxStatusLocalDoHLatency.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusLocalDoHLatency.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusLocalDoHLatency.TabIndex = 0;
            this.CustomRichTextBoxStatusLocalDoHLatency.Texts = "Local DoH Latency: -1";
            this.CustomRichTextBoxStatusLocalDoHLatency.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusLocalDoHLatency.WordWrap = false;
            this.CustomRichTextBoxStatusLocalDoHLatency.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusLocalDoH
            // 
            this.CustomRichTextBoxStatusLocalDoH.AcceptsTab = false;
            this.CustomRichTextBoxStatusLocalDoH.AutoWordSelection = false;
            this.CustomRichTextBoxStatusLocalDoH.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusLocalDoH.Border = false;
            this.CustomRichTextBoxStatusLocalDoH.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusLocalDoH.BorderSize = 1;
            this.CustomRichTextBoxStatusLocalDoH.BulletIndent = 0;
            this.CustomRichTextBoxStatusLocalDoH.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusLocalDoH.DetectUrls = true;
            this.CustomRichTextBoxStatusLocalDoH.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusLocalDoH.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusLocalDoH.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusLocalDoH.HideSelection = true;
            this.CustomRichTextBoxStatusLocalDoH.Location = new System.Drawing.Point(5, 115);
            this.CustomRichTextBoxStatusLocalDoH.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusLocalDoH.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusLocalDoH.Multiline = false;
            this.CustomRichTextBoxStatusLocalDoH.Name = "CustomRichTextBoxStatusLocalDoH";
            this.CustomRichTextBoxStatusLocalDoH.ReadOnly = true;
            this.CustomRichTextBoxStatusLocalDoH.RightMargin = 0;
            this.CustomRichTextBoxStatusLocalDoH.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusLocalDoH.ScrollToBottom = false;
            this.CustomRichTextBoxStatusLocalDoH.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusLocalDoH.SelectionLength = 0;
            this.CustomRichTextBoxStatusLocalDoH.SelectionStart = 0;
            this.CustomRichTextBoxStatusLocalDoH.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusLocalDoH.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusLocalDoH.TabIndex = 0;
            this.CustomRichTextBoxStatusLocalDoH.Texts = "Local DoH: Offline";
            this.CustomRichTextBoxStatusLocalDoH.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusLocalDoH.WordWrap = false;
            this.CustomRichTextBoxStatusLocalDoH.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusLocalDnsLatency
            // 
            this.CustomRichTextBoxStatusLocalDnsLatency.AcceptsTab = false;
            this.CustomRichTextBoxStatusLocalDnsLatency.AutoWordSelection = false;
            this.CustomRichTextBoxStatusLocalDnsLatency.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusLocalDnsLatency.Border = false;
            this.CustomRichTextBoxStatusLocalDnsLatency.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusLocalDnsLatency.BorderSize = 1;
            this.CustomRichTextBoxStatusLocalDnsLatency.BulletIndent = 0;
            this.CustomRichTextBoxStatusLocalDnsLatency.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusLocalDnsLatency.DetectUrls = true;
            this.CustomRichTextBoxStatusLocalDnsLatency.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusLocalDnsLatency.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusLocalDnsLatency.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusLocalDnsLatency.HideSelection = true;
            this.CustomRichTextBoxStatusLocalDnsLatency.Location = new System.Drawing.Point(5, 90);
            this.CustomRichTextBoxStatusLocalDnsLatency.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusLocalDnsLatency.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusLocalDnsLatency.Multiline = false;
            this.CustomRichTextBoxStatusLocalDnsLatency.Name = "CustomRichTextBoxStatusLocalDnsLatency";
            this.CustomRichTextBoxStatusLocalDnsLatency.ReadOnly = true;
            this.CustomRichTextBoxStatusLocalDnsLatency.RightMargin = 0;
            this.CustomRichTextBoxStatusLocalDnsLatency.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusLocalDnsLatency.ScrollToBottom = false;
            this.CustomRichTextBoxStatusLocalDnsLatency.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusLocalDnsLatency.SelectionLength = 0;
            this.CustomRichTextBoxStatusLocalDnsLatency.SelectionStart = 0;
            this.CustomRichTextBoxStatusLocalDnsLatency.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusLocalDnsLatency.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusLocalDnsLatency.TabIndex = 0;
            this.CustomRichTextBoxStatusLocalDnsLatency.Texts = "Local DNS Latency: -1";
            this.CustomRichTextBoxStatusLocalDnsLatency.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusLocalDnsLatency.WordWrap = false;
            this.CustomRichTextBoxStatusLocalDnsLatency.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusLocalDNS
            // 
            this.CustomRichTextBoxStatusLocalDNS.AcceptsTab = false;
            this.CustomRichTextBoxStatusLocalDNS.AutoWordSelection = false;
            this.CustomRichTextBoxStatusLocalDNS.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusLocalDNS.Border = false;
            this.CustomRichTextBoxStatusLocalDNS.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusLocalDNS.BorderSize = 1;
            this.CustomRichTextBoxStatusLocalDNS.BulletIndent = 0;
            this.CustomRichTextBoxStatusLocalDNS.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusLocalDNS.DetectUrls = true;
            this.CustomRichTextBoxStatusLocalDNS.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusLocalDNS.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusLocalDNS.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusLocalDNS.HideSelection = true;
            this.CustomRichTextBoxStatusLocalDNS.Location = new System.Drawing.Point(5, 65);
            this.CustomRichTextBoxStatusLocalDNS.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusLocalDNS.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusLocalDNS.Multiline = false;
            this.CustomRichTextBoxStatusLocalDNS.Name = "CustomRichTextBoxStatusLocalDNS";
            this.CustomRichTextBoxStatusLocalDNS.ReadOnly = true;
            this.CustomRichTextBoxStatusLocalDNS.RightMargin = 0;
            this.CustomRichTextBoxStatusLocalDNS.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusLocalDNS.ScrollToBottom = false;
            this.CustomRichTextBoxStatusLocalDNS.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusLocalDNS.SelectionLength = 0;
            this.CustomRichTextBoxStatusLocalDNS.SelectionStart = 0;
            this.CustomRichTextBoxStatusLocalDNS.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusLocalDNS.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusLocalDNS.TabIndex = 0;
            this.CustomRichTextBoxStatusLocalDNS.Texts = "Local DNS: Offline";
            this.CustomRichTextBoxStatusLocalDNS.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusLocalDNS.WordWrap = false;
            this.CustomRichTextBoxStatusLocalDNS.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusIsProxySet
            // 
            this.CustomRichTextBoxStatusIsProxySet.AcceptsTab = false;
            this.CustomRichTextBoxStatusIsProxySet.AutoWordSelection = false;
            this.CustomRichTextBoxStatusIsProxySet.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusIsProxySet.Border = false;
            this.CustomRichTextBoxStatusIsProxySet.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusIsProxySet.BorderSize = 1;
            this.CustomRichTextBoxStatusIsProxySet.BulletIndent = 0;
            this.CustomRichTextBoxStatusIsProxySet.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusIsProxySet.DetectUrls = true;
            this.CustomRichTextBoxStatusIsProxySet.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusIsProxySet.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusIsProxySet.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusIsProxySet.HideSelection = true;
            this.CustomRichTextBoxStatusIsProxySet.Location = new System.Drawing.Point(5, 240);
            this.CustomRichTextBoxStatusIsProxySet.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusIsProxySet.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusIsProxySet.Multiline = false;
            this.CustomRichTextBoxStatusIsProxySet.Name = "CustomRichTextBoxStatusIsProxySet";
            this.CustomRichTextBoxStatusIsProxySet.ReadOnly = true;
            this.CustomRichTextBoxStatusIsProxySet.RightMargin = 0;
            this.CustomRichTextBoxStatusIsProxySet.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusIsProxySet.ScrollToBottom = false;
            this.CustomRichTextBoxStatusIsProxySet.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusIsProxySet.SelectionLength = 0;
            this.CustomRichTextBoxStatusIsProxySet.SelectionStart = 0;
            this.CustomRichTextBoxStatusIsProxySet.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusIsProxySet.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusIsProxySet.TabIndex = 0;
            this.CustomRichTextBoxStatusIsProxySet.Texts = "Is Proxy Set: Yes";
            this.CustomRichTextBoxStatusIsProxySet.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusIsProxySet.WordWrap = false;
            this.CustomRichTextBoxStatusIsProxySet.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusIsSharing
            // 
            this.CustomRichTextBoxStatusIsSharing.AcceptsTab = false;
            this.CustomRichTextBoxStatusIsSharing.AutoWordSelection = false;
            this.CustomRichTextBoxStatusIsSharing.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusIsSharing.Border = false;
            this.CustomRichTextBoxStatusIsSharing.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusIsSharing.BorderSize = 1;
            this.CustomRichTextBoxStatusIsSharing.BulletIndent = 0;
            this.CustomRichTextBoxStatusIsSharing.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusIsSharing.DetectUrls = true;
            this.CustomRichTextBoxStatusIsSharing.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusIsSharing.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusIsSharing.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusIsSharing.HideSelection = true;
            this.CustomRichTextBoxStatusIsSharing.Location = new System.Drawing.Point(5, 190);
            this.CustomRichTextBoxStatusIsSharing.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusIsSharing.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusIsSharing.Multiline = false;
            this.CustomRichTextBoxStatusIsSharing.Name = "CustomRichTextBoxStatusIsSharing";
            this.CustomRichTextBoxStatusIsSharing.ReadOnly = true;
            this.CustomRichTextBoxStatusIsSharing.RightMargin = 0;
            this.CustomRichTextBoxStatusIsSharing.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusIsSharing.ScrollToBottom = false;
            this.CustomRichTextBoxStatusIsSharing.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusIsSharing.SelectionLength = 0;
            this.CustomRichTextBoxStatusIsSharing.SelectionStart = 0;
            this.CustomRichTextBoxStatusIsSharing.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusIsSharing.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusIsSharing.TabIndex = 0;
            this.CustomRichTextBoxStatusIsSharing.Texts = "Is Sharing: Yes";
            this.CustomRichTextBoxStatusIsSharing.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusIsSharing.WordWrap = false;
            this.CustomRichTextBoxStatusIsSharing.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusIsDNSSet
            // 
            this.CustomRichTextBoxStatusIsDNSSet.AcceptsTab = false;
            this.CustomRichTextBoxStatusIsDNSSet.AutoWordSelection = false;
            this.CustomRichTextBoxStatusIsDNSSet.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusIsDNSSet.Border = false;
            this.CustomRichTextBoxStatusIsDNSSet.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusIsDNSSet.BorderSize = 1;
            this.CustomRichTextBoxStatusIsDNSSet.BulletIndent = 0;
            this.CustomRichTextBoxStatusIsDNSSet.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusIsDNSSet.DetectUrls = true;
            this.CustomRichTextBoxStatusIsDNSSet.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusIsDNSSet.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusIsDNSSet.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusIsDNSSet.HideSelection = true;
            this.CustomRichTextBoxStatusIsDNSSet.Location = new System.Drawing.Point(5, 165);
            this.CustomRichTextBoxStatusIsDNSSet.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusIsDNSSet.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusIsDNSSet.Multiline = false;
            this.CustomRichTextBoxStatusIsDNSSet.Name = "CustomRichTextBoxStatusIsDNSSet";
            this.CustomRichTextBoxStatusIsDNSSet.ReadOnly = true;
            this.CustomRichTextBoxStatusIsDNSSet.RightMargin = 0;
            this.CustomRichTextBoxStatusIsDNSSet.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusIsDNSSet.ScrollToBottom = false;
            this.CustomRichTextBoxStatusIsDNSSet.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusIsDNSSet.SelectionLength = 0;
            this.CustomRichTextBoxStatusIsDNSSet.SelectionStart = 0;
            this.CustomRichTextBoxStatusIsDNSSet.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusIsDNSSet.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusIsDNSSet.TabIndex = 0;
            this.CustomRichTextBoxStatusIsDNSSet.Texts = "Is DNS Set: Yes";
            this.CustomRichTextBoxStatusIsDNSSet.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusIsDNSSet.WordWrap = false;
            this.CustomRichTextBoxStatusIsDNSSet.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusProxyDpiBypass
            // 
            this.CustomRichTextBoxStatusProxyDpiBypass.AcceptsTab = false;
            this.CustomRichTextBoxStatusProxyDpiBypass.AutoWordSelection = false;
            this.CustomRichTextBoxStatusProxyDpiBypass.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusProxyDpiBypass.Border = false;
            this.CustomRichTextBoxStatusProxyDpiBypass.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusProxyDpiBypass.BorderSize = 1;
            this.CustomRichTextBoxStatusProxyDpiBypass.BulletIndent = 0;
            this.CustomRichTextBoxStatusProxyDpiBypass.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusProxyDpiBypass.DetectUrls = true;
            this.CustomRichTextBoxStatusProxyDpiBypass.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusProxyDpiBypass.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusProxyDpiBypass.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusProxyDpiBypass.HideSelection = true;
            this.CustomRichTextBoxStatusProxyDpiBypass.Location = new System.Drawing.Point(5, 265);
            this.CustomRichTextBoxStatusProxyDpiBypass.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusProxyDpiBypass.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusProxyDpiBypass.Multiline = false;
            this.CustomRichTextBoxStatusProxyDpiBypass.Name = "CustomRichTextBoxStatusProxyDpiBypass";
            this.CustomRichTextBoxStatusProxyDpiBypass.ReadOnly = true;
            this.CustomRichTextBoxStatusProxyDpiBypass.RightMargin = 0;
            this.CustomRichTextBoxStatusProxyDpiBypass.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusProxyDpiBypass.ScrollToBottom = false;
            this.CustomRichTextBoxStatusProxyDpiBypass.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusProxyDpiBypass.SelectionLength = 0;
            this.CustomRichTextBoxStatusProxyDpiBypass.SelectionStart = 0;
            this.CustomRichTextBoxStatusProxyDpiBypass.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusProxyDpiBypass.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusProxyDpiBypass.TabIndex = 0;
            this.CustomRichTextBoxStatusProxyDpiBypass.Texts = "Proxy DPI Bypass: Inactive";
            this.CustomRichTextBoxStatusProxyDpiBypass.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusProxyDpiBypass.WordWrap = false;
            this.CustomRichTextBoxStatusProxyDpiBypass.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusIsConnected
            // 
            this.CustomRichTextBoxStatusIsConnected.AcceptsTab = false;
            this.CustomRichTextBoxStatusIsConnected.AutoWordSelection = false;
            this.CustomRichTextBoxStatusIsConnected.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusIsConnected.Border = false;
            this.CustomRichTextBoxStatusIsConnected.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusIsConnected.BorderSize = 1;
            this.CustomRichTextBoxStatusIsConnected.BulletIndent = 0;
            this.CustomRichTextBoxStatusIsConnected.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusIsConnected.DetectUrls = true;
            this.CustomRichTextBoxStatusIsConnected.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusIsConnected.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusIsConnected.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusIsConnected.HideSelection = true;
            this.CustomRichTextBoxStatusIsConnected.Location = new System.Drawing.Point(5, 40);
            this.CustomRichTextBoxStatusIsConnected.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusIsConnected.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusIsConnected.Multiline = false;
            this.CustomRichTextBoxStatusIsConnected.Name = "CustomRichTextBoxStatusIsConnected";
            this.CustomRichTextBoxStatusIsConnected.ReadOnly = true;
            this.CustomRichTextBoxStatusIsConnected.RightMargin = 0;
            this.CustomRichTextBoxStatusIsConnected.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusIsConnected.ScrollToBottom = false;
            this.CustomRichTextBoxStatusIsConnected.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusIsConnected.SelectionLength = 0;
            this.CustomRichTextBoxStatusIsConnected.SelectionStart = 0;
            this.CustomRichTextBoxStatusIsConnected.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusIsConnected.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusIsConnected.TabIndex = 0;
            this.CustomRichTextBoxStatusIsConnected.Texts = "Is Connected: Yes";
            this.CustomRichTextBoxStatusIsConnected.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusIsConnected.WordWrap = false;
            this.CustomRichTextBoxStatusIsConnected.ZoomFactor = 1F;
            // 
            // CustomRichTextBoxStatusWorkingServers
            // 
            this.CustomRichTextBoxStatusWorkingServers.AcceptsTab = false;
            this.CustomRichTextBoxStatusWorkingServers.AutoWordSelection = false;
            this.CustomRichTextBoxStatusWorkingServers.BackColor = System.Drawing.Color.DimGray;
            this.CustomRichTextBoxStatusWorkingServers.Border = false;
            this.CustomRichTextBoxStatusWorkingServers.BorderColor = System.Drawing.Color.Blue;
            this.CustomRichTextBoxStatusWorkingServers.BorderSize = 1;
            this.CustomRichTextBoxStatusWorkingServers.BulletIndent = 0;
            this.CustomRichTextBoxStatusWorkingServers.Cursor = System.Windows.Forms.Cursors.Default;
            this.CustomRichTextBoxStatusWorkingServers.DetectUrls = true;
            this.CustomRichTextBoxStatusWorkingServers.EnableAutoDragDrop = false;
            this.CustomRichTextBoxStatusWorkingServers.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomRichTextBoxStatusWorkingServers.ForeColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusWorkingServers.HideSelection = true;
            this.CustomRichTextBoxStatusWorkingServers.Location = new System.Drawing.Point(5, 15);
            this.CustomRichTextBoxStatusWorkingServers.MaxLength = 2147483647;
            this.CustomRichTextBoxStatusWorkingServers.MinimumSize = new System.Drawing.Size(0, 15);
            this.CustomRichTextBoxStatusWorkingServers.Multiline = false;
            this.CustomRichTextBoxStatusWorkingServers.Name = "CustomRichTextBoxStatusWorkingServers";
            this.CustomRichTextBoxStatusWorkingServers.ReadOnly = true;
            this.CustomRichTextBoxStatusWorkingServers.RightMargin = 0;
            this.CustomRichTextBoxStatusWorkingServers.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomRichTextBoxStatusWorkingServers.ScrollToBottom = false;
            this.CustomRichTextBoxStatusWorkingServers.SelectionColor = System.Drawing.Color.White;
            this.CustomRichTextBoxStatusWorkingServers.SelectionLength = 0;
            this.CustomRichTextBoxStatusWorkingServers.SelectionStart = 0;
            this.CustomRichTextBoxStatusWorkingServers.ShortcutsEnabled = true;
            this.CustomRichTextBoxStatusWorkingServers.Size = new System.Drawing.Size(170, 23);
            this.CustomRichTextBoxStatusWorkingServers.TabIndex = 0;
            this.CustomRichTextBoxStatusWorkingServers.Texts = "Working Servers: 0000";
            this.CustomRichTextBoxStatusWorkingServers.UnderlinedStyle = false;
            this.CustomRichTextBoxStatusWorkingServers.WordWrap = false;
            this.CustomRichTextBoxStatusWorkingServers.ZoomFactor = 1F;
            // 
            // SplitContainerMain
            // 
            this.SplitContainerMain.Cursor = System.Windows.Forms.Cursors.Default;
            this.SplitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainerMain.IsSplitterFixed = true;
            this.SplitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.SplitContainerMain.Name = "SplitContainerMain";
            this.SplitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainerMain.Panel1
            // 
            this.SplitContainerMain.Panel1.Controls.Add(this.SplitContainerTop);
            // 
            // SplitContainerMain.Panel2
            // 
            this.SplitContainerMain.Panel2.Controls.Add(this.CustomGroupBoxLog);
            this.SplitContainerMain.Size = new System.Drawing.Size(884, 581);
            this.SplitContainerMain.SplitterDistance = 400;
            this.SplitContainerMain.TabIndex = 9;
            // 
            // SplitContainerTop
            // 
            this.SplitContainerTop.Cursor = System.Windows.Forms.Cursors.Default;
            this.SplitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainerTop.IsSplitterFixed = true;
            this.SplitContainerTop.Location = new System.Drawing.Point(0, 0);
            this.SplitContainerTop.Name = "SplitContainerTop";
            // 
            // SplitContainerTop.Panel1
            // 
            this.SplitContainerTop.Panel1.Controls.Add(this.CustomTabControlMain);
            // 
            // SplitContainerTop.Panel2
            // 
            this.SplitContainerTop.Panel2.Controls.Add(this.CustomGroupBoxStatus);
            this.SplitContainerTop.Size = new System.Drawing.Size(884, 400);
            this.SplitContainerTop.SplitterDistance = 700;
            this.SplitContainerTop.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AcceptButton = this.CustomButtonCheck;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 581);
            this.Controls.Add(this.SplitContainerMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(900, 620);
            this.MinimumSize = new System.Drawing.Size(900, 435);
            this.Name = "FormMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "SecureDNSClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.CustomGroupBoxLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSSLFragmentSize)).EndInit();
            this.CustomTabControlMain.ResumeLayout(false);
            this.TabPageSecureDNS.ResumeLayout(false);
            this.CustomTabControlSecureDNS.ResumeLayout(false);
            this.TabPageCheck.ResumeLayout(false);
            this.TabPageCheck.PerformLayout();
            this.TabPageConnect.ResumeLayout(false);
            this.TabPageConnect.PerformLayout();
            this.TabPageSetDNS.ResumeLayout(false);
            this.TabPageSetDNS.PerformLayout();
            this.TabPageShare.ResumeLayout(false);
            this.TabPageShare.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownHTTPProxyHandleRequests)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPDpiFragDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPDpiDataLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPDpiFragmentSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownPDpiFragmentChunks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownHTTPProxyPort)).EndInit();
            this.TabPageDPI.ResumeLayout(false);
            this.CustomTabControlDPIBasicAdvanced.ResumeLayout(false);
            this.TabPageDPIBasic.ResumeLayout(false);
            this.TabPageDPIBasic.PerformLayout();
            this.TabPageDPIAdvanced.ResumeLayout(false);
            this.TabPageDPIAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvMaxPayload)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvMinTTL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvSetTTL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvE)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownDPIAdvF)).EndInit();
            this.TabPageSettings.ResumeLayout(false);
            this.CustomTabControlSettings.ResumeLayout(false);
            this.TabPageSettingsWorkingMode.ResumeLayout(false);
            this.TabPageSettingsWorkingMode.PerformLayout();
            this.TabPageSettingsCheck.ResumeLayout(false);
            this.TabPageSettingsCheck.PerformLayout();
            this.CustomGroupBoxSettingCheckSDNS.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingCheckTimeout)).EndInit();
            this.TabPageSettingsConnect.ResumeLayout(false);
            this.TabPageSettingsConnect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingCamouflagePort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingMaxServers)).EndInit();
            this.TabPageSettingsSetUnsetDNS.ResumeLayout(false);
            this.TabPageSettingsSetUnsetDNS.PerformLayout();
            this.TabPageSettingsCPU.ResumeLayout(false);
            this.TabPageSettingsCPU.PerformLayout();
            this.TabPageSettingsOthers.ResumeLayout(false);
            this.TabPageSettingsOthers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingFallbackDnsPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomNumericUpDownSettingBootstrapDnsPort)).EndInit();
            this.TabPageAbout.ResumeLayout(false);
            this.TabPageAbout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxAbout)).EndInit();
            this.CustomGroupBoxStatus.ResumeLayout(false);
            this.CustomGroupBoxStatus.PerformLayout();
            this.SplitContainerMain.Panel1.ResumeLayout(false);
            this.SplitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainerMain)).EndInit();
            this.SplitContainerMain.ResumeLayout(false);
            this.SplitContainerTop.Panel1.ResumeLayout(false);
            this.SplitContainerTop.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainerTop)).EndInit();
            this.SplitContainerTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private CustomControls.CustomRichTextBox CustomRichTextBoxLog;
        private CustomControls.CustomButton CustomButtonCheck;
        private CustomControls.CustomGroupBox CustomGroupBoxLog;
        private CustomControls.CustomRadioButton CustomRadioButtonBuiltIn;
        private CustomControls.CustomRadioButton CustomRadioButtonCustom;
        private CustomControls.CustomButton CustomButtonEditCustomServers;
        private CustomControls.CustomLabel CustomLabelCustomServersInfo;
        private CustomControls.CustomButton CustomButtonConnect;
        private CustomControls.CustomTabControl CustomTabControlMain;
        private TabPage TabPageSecureDNS;
        private TabPage TabPageSettings;
        private CustomControls.CustomLabel CustomLabelDPIModes;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIModeLight;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIModeMedium;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIModeHigh;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIModeExtreme;
        private CustomControls.CustomLabel CustomLabelSelectNIC;
        private CustomControls.CustomComboBox CustomComboBoxNICs;
        private CustomControls.CustomLabel CustomLabelSetDNSInfo;
        private CustomControls.CustomButton CustomButtonSetDNS;
        private CustomControls.CustomCheckBox CustomCheckBoxInsecure;
        private CustomControls.CustomTextBox CustomTextBoxHTTPProxy;
        private CustomControls.CustomLabel CustomLabelSSLFragmentSize;
        private TabPage TabPageAbout;
        private CustomControls.CustomButton CustomButtonSettingRestoreDefault;
        private CustomControls.CustomTabControl CustomTabControlSecureDNS;
        private TabPage TabPageCheck;
        private TabPage TabPageConnect;
        private TabPage TabPageDPI;
        private TabPage TabPageSetDNS;
        private CustomControls.CustomTabControl CustomTabControlDPIBasicAdvanced;
        private TabPage TabPageDPIBasic;
        private TabPage TabPageDPIAdvanced;
        private CustomControls.CustomButton CustomButtonDPIBasicActivate;
        private CustomControls.CustomButton CustomButtonDPIBasicDeactivate;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIMode2;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIMode1;
        private CustomControls.CustomLabel CustomLabelDPIModesGoodbyeDPI;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIMode3;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIMode6;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIMode5;
        private CustomControls.CustomRadioButton CustomRadioButtonDPIMode4;
        private CustomControls.CustomButton CustomButtonDPIAdvDeactivate;
        private CustomControls.CustomButton CustomButtonDPIAdvActivate;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownSSLFragmentSize;
        private CustomControls.CustomTextBox CustomTextBoxDPIAdvAutoTTL;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvR;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvP;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvS;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvM;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownDPIAdvK;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvK;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownDPIAdvF;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvF;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvN;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvE;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownDPIAdvE;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvA;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvW;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvPort;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvIpId;
        private CustomControls.CustomTextBox CustomTextBoxDPIAdvIpId;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvAllowNoSNI;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvSetTTL;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvAutoTTL;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvMinTTL;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvWrongChksum;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvWrongSeq;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvNativeFrag;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvReverseFrag;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvMaxPayload;
        private CustomControls.CustomCheckBox CustomCheckBoxDPIAdvBlacklist;
        private CustomControls.CustomButton CustomButtonDPIAdvBlacklist;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownDPIAdvPort;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownDPIAdvSetTTL;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownDPIAdvMinTTL;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownDPIAdvMaxPayload;
        private CustomControls.CustomLabel CustomLabelSettingBootstrapDnsIP;
        private CustomControls.CustomTextBox CustomTextBoxSettingBootstrapDnsIP;
        private CustomControls.CustomButton CustomButtonViewWorkingServers;
        private NotifyIcon NotifyIconMain;
        private CustomControls.CustomContextMenuStrip CustomContextMenuStripIcon;
        private CustomControls.CustomLabel CustomLabelAboutUsing;
        private CustomControls.CustomLabel CustomLabelAboutVersion;
        private LinkLabel LinkLabelGoodbyeDPI;
        private LinkLabel LinkLabelDNSCrypt;
        private LinkLabel LinkLabelDNSProxy;
        private LinkLabel LinkLabelDNSLookup;
        private CustomControls.CustomLabel CustomLabelAboutThis2;
        private CustomControls.CustomLabel CustomLabelAboutThis;
        private PictureBox PictureBoxAbout;
        private CustomControls.CustomCheckBox CustomCheckBoxSettingDontAskCertificate;
        private CustomControls.CustomGroupBox CustomGroupBoxStatus;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusIsConnected;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusWorkingServers;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusProxyDpiBypass;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusIsDNSSet;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusIsSharing;
        private CustomControls.CustomButton CustomButtonConnectAll;
        private CustomControls.CustomCheckBox CustomCheckBoxSettingDisableAudioAlert;
        private CustomControls.CustomTabControl CustomTabControlSettings;
        private TabPage TabPageSettingsWorkingMode;
        private TabPage TabPageSettingsCPU;
        private TabPage TabPageSettingsOthers;
        private CustomControls.CustomLabel CustomLabelSettingInfoWorkingMode1;
        private CustomControls.CustomRadioButton CustomRadioButtonSettingWorkingModeDNSandDoH;
        private CustomControls.CustomRadioButton CustomRadioButtonSettingWorkingModeDNS;
        private CustomControls.CustomRadioButton CustomRadioButtonSettingCPUAboveNormal;
        private CustomControls.CustomRadioButton CustomRadioButtonSettingCPUHigh;
        private CustomControls.CustomLabel CustomLabelSettingInfoCPU;
        private CustomControls.CustomRadioButton CustomRadioButtonSettingCPULow;
        private CustomControls.CustomRadioButton CustomRadioButtonSettingCPUBelowNormal;
        private CustomControls.CustomRadioButton CustomRadioButtonSettingCPUNormal;
        private CustomControls.CustomLabel CustomLabelSettingInfoWorkingMode2;
        private TabPage TabPageSettingsCheck;
        private CustomControls.CustomLabel CustomLabelSettingCheckTimeout;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownSettingCheckTimeout;
        private CustomControls.CustomTextBox CustomTextBoxSettingCheckDPIHost;
        private CustomControls.CustomLabel CustomLabelSettingCheckDPIInfo;
        private CustomControls.CustomGroupBox CustomGroupBoxSettingCheckSDNS;
        private CustomControls.CustomCheckBox CustomCheckBoxSettingSdnsNoFilter;
        private CustomControls.CustomCheckBox CustomCheckBoxSettingSdnsNoLog;
        private CustomControls.CustomCheckBox CustomCheckBoxSettingSdnsDNSSec;
        private CustomControls.CustomRadioButton CustomRadioButtonConnectDNSCrypt;
        private CustomControls.CustomRadioButton CustomRadioButtonConnectCheckedServers;
        private CustomControls.CustomRadioButton CustomRadioButtonConnectCloudflare;
        private TabPage TabPageSettingsConnect;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownSettingCamouflagePort;
        private CustomControls.CustomLabel CustomLabelCheckSettingCamouflagePort;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownSettingMaxServers;
        private CustomControls.CustomLabel CustomLabelSettingMaxServers;
        private CustomControls.CustomCheckBox CustomCheckBoxSettingEnableCache;
        private TabPage TabPageShare;
        public CustomControls.CustomNumericUpDown CustomNumericUpDownHTTPProxyPort;
        private CustomControls.CustomLabel CustomLabelHTTPProxyPort;
        private CustomControls.CustomLabel CustomLabelShareInfo;
        private CustomControls.CustomButton CustomButtonShare;
        private CustomControls.CustomCheckBox CustomCheckBoxPDpiEnableDpiBypass;
        private CustomControls.CustomLabel CustomLabelPDpiDpiInfo3;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownPDpiFragmentChunks;
        private CustomControls.CustomLabel CustomLabelSettingBootstrapDnsPort;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownSettingBootstrapDnsPort;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownSettingFallbackDnsPort;
        private CustomControls.CustomLabel CustomLabelSettingFallbackDnsPort;
        private CustomControls.CustomTextBox CustomTextBoxSettingFallbackDnsIP;
        private CustomControls.CustomLabel CustomLabelSettingFallbackDnsIP;
        private CustomControls.CustomLabel CustomLabelInfoDPIModes;
        private LinkLabel LinkLabelStAlidxdydz;
        private CustomControls.CustomLabel CustomLabelAboutSpecialThanks;
        private CustomControls.CustomCheckBox CustomCheckBoxHTTPProxyEventShowRequest;
        private CustomControls.CustomLabel CustomLabelShareSeparator1;
        private CustomControls.CustomButton CustomButtonToggleLogView;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownPDpiFragmentSize;
        private CustomControls.CustomLabel CustomLabelPDpiDpiInfo2;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownPDpiDataLength;
        private CustomControls.CustomLabel CustomLabelPDpiDpiInfo1;
        private CustomControls.CustomButton CustomButtonSetProxy;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusIsProxySet;
        private CustomControls.CustomCheckBox CustomCheckBoxPDpiFragModeRandom;
        private CustomControls.CustomCheckBox CustomCheckBoxPDpiDontChunkBigData;
        private CustomControls.CustomLabel CustomLabelPDpiFragDelay;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownPDpiFragDelay;
        private TabPage TabPageSettingsSetUnsetDNS;
        private CustomControls.CustomRadioButton CustomRadioButtonSettingUnsetDnsToDhcp;
        private CustomControls.CustomRadioButton CustomRadioButtonSettingUnsetDnsToStatic;
        private CustomControls.CustomLabel CustomLabelSettingUnsetDns2;
        private CustomControls.CustomLabel CustomLabelSettingUnsetDns1;
        private CustomControls.CustomTextBox CustomTextBoxSettingUnsetDns2;
        private CustomControls.CustomTextBox CustomTextBoxSettingUnsetDns1;
        private CustomControls.CustomButton CustomButtonPDpiApplyChanges;
        private CustomControls.CustomCheckBox CustomCheckBoxHTTPProxyEventShowChunkDetails;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusLocalDoHLatency;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusLocalDoH;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusLocalDnsLatency;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusLocalDNS;
        private CustomControls.CustomLabel CustomLabelCheckPercent;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusProxyRequests;
        private CustomControls.CustomNumericUpDown CustomNumericUpDownHTTPProxyHandleRequests;
        private CustomControls.CustomLabel CustomLabelHTTPProxyHandleRequests;
        private SplitContainer SplitContainerMain;
        private SplitContainer SplitContainerTop;
        private CustomControls.CustomRichTextBox CustomRichTextBoxStatusGoodbyeDPI;
    }
}