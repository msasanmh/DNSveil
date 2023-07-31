namespace SecureDNSClient
{
    partial class FormDnsLookup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDnsLookup));
            this.CustomRadioButtonSourceSDC = new CustomControls.CustomRadioButton();
            this.CustomRadioButtonSourceCustom = new CustomControls.CustomRadioButton();
            this.CustomTextBoxSourceCustom = new CustomControls.CustomTextBox();
            this.CustomLabelDomain = new CustomControls.CustomLabel();
            this.CustomTextBoxDomain = new CustomControls.CustomTextBox();
            this.CustomButtonLookup = new CustomControls.CustomButton();
            this.CustomTextBoxResult = new CustomControls.CustomTextBox();
            this.CustomCheckBoxHTTP3 = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxVERIFY = new CustomControls.CustomCheckBox();
            this.CustomLabelRRTYPE = new CustomControls.CustomLabel();
            this.CustomTextBoxRRTYPE = new CustomControls.CustomTextBox();
            this.CustomLabelCLASS = new CustomControls.CustomLabel();
            this.CustomTextBoxCLASS = new CustomControls.CustomTextBox();
            this.CustomCheckBoxDNSSEC = new CustomControls.CustomCheckBox();
            this.CustomLabelSUBNET = new CustomControls.CustomLabel();
            this.CustomTextBoxSUBNET = new CustomControls.CustomTextBox();
            this.CustomCheckBoxPAD = new CustomControls.CustomCheckBox();
            this.CustomCheckBoxVERBOSE = new CustomControls.CustomCheckBox();
            this.CustomLabelEDNSOPT = new CustomControls.CustomLabel();
            this.CustomTextBoxEDNSOPT = new CustomControls.CustomTextBox();
            this.CustomButtonDefault = new CustomControls.CustomButton();
            this.CustomCheckBoxJSON = new CustomControls.CustomCheckBox();
            this.SuspendLayout();
            // 
            // CustomRadioButtonSourceSDC
            // 
            this.CustomRadioButtonSourceSDC.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSourceSDC.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSourceSDC.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSourceSDC.Checked = true;
            this.CustomRadioButtonSourceSDC.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSourceSDC.Location = new System.Drawing.Point(12, 10);
            this.CustomRadioButtonSourceSDC.Name = "CustomRadioButtonSourceSDC";
            this.CustomRadioButtonSourceSDC.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSourceSDC.Size = new System.Drawing.Size(43, 17);
            this.CustomRadioButtonSourceSDC.TabIndex = 0;
            this.CustomRadioButtonSourceSDC.TabStop = true;
            this.CustomRadioButtonSourceSDC.Text = "SDC";
            this.CustomRadioButtonSourceSDC.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSourceCustom
            // 
            this.CustomRadioButtonSourceCustom.BackColor = System.Drawing.Color.DimGray;
            this.CustomRadioButtonSourceCustom.BorderColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSourceCustom.CheckColor = System.Drawing.Color.Blue;
            this.CustomRadioButtonSourceCustom.ForeColor = System.Drawing.Color.White;
            this.CustomRadioButtonSourceCustom.Location = new System.Drawing.Point(61, 10);
            this.CustomRadioButtonSourceCustom.Name = "CustomRadioButtonSourceCustom";
            this.CustomRadioButtonSourceCustom.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomRadioButtonSourceCustom.Size = new System.Drawing.Size(65, 17);
            this.CustomRadioButtonSourceCustom.TabIndex = 1;
            this.CustomRadioButtonSourceCustom.Text = "Custom:";
            this.CustomRadioButtonSourceCustom.UseVisualStyleBackColor = false;
            // 
            // CustomTextBoxSourceCustom
            // 
            this.CustomTextBoxSourceCustom.AcceptsReturn = false;
            this.CustomTextBoxSourceCustom.AcceptsTab = false;
            this.CustomTextBoxSourceCustom.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxSourceCustom.Border = true;
            this.CustomTextBoxSourceCustom.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxSourceCustom.BorderSize = 1;
            this.CustomTextBoxSourceCustom.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxSourceCustom.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxSourceCustom.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxSourceCustom.HideSelection = true;
            this.CustomTextBoxSourceCustom.Location = new System.Drawing.Point(132, 8);
            this.CustomTextBoxSourceCustom.MaxLength = 32767;
            this.CustomTextBoxSourceCustom.Multiline = false;
            this.CustomTextBoxSourceCustom.Name = "CustomTextBoxSourceCustom";
            this.CustomTextBoxSourceCustom.ReadOnly = false;
            this.CustomTextBoxSourceCustom.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxSourceCustom.ShortcutsEnabled = true;
            this.CustomTextBoxSourceCustom.Size = new System.Drawing.Size(200, 23);
            this.CustomTextBoxSourceCustom.TabIndex = 0;
            this.CustomTextBoxSourceCustom.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxSourceCustom.Texts = "";
            this.CustomTextBoxSourceCustom.UnderlinedStyle = true;
            this.CustomTextBoxSourceCustom.UsePasswordChar = false;
            this.CustomTextBoxSourceCustom.WordWrap = true;
            // 
            // CustomLabelDomain
            // 
            this.CustomLabelDomain.AutoSize = true;
            this.CustomLabelDomain.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelDomain.Border = false;
            this.CustomLabelDomain.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelDomain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelDomain.ForeColor = System.Drawing.Color.White;
            this.CustomLabelDomain.Location = new System.Drawing.Point(74, 50);
            this.CustomLabelDomain.Name = "CustomLabelDomain";
            this.CustomLabelDomain.RoundedCorners = 0;
            this.CustomLabelDomain.Size = new System.Drawing.Size(54, 17);
            this.CustomLabelDomain.TabIndex = 3;
            this.CustomLabelDomain.Text = "Domain:";
            // 
            // CustomTextBoxDomain
            // 
            this.CustomTextBoxDomain.AcceptsReturn = false;
            this.CustomTextBoxDomain.AcceptsTab = false;
            this.CustomTextBoxDomain.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxDomain.Border = true;
            this.CustomTextBoxDomain.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxDomain.BorderSize = 1;
            this.CustomTextBoxDomain.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxDomain.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxDomain.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxDomain.HideSelection = true;
            this.CustomTextBoxDomain.Location = new System.Drawing.Point(132, 48);
            this.CustomTextBoxDomain.MaxLength = 32767;
            this.CustomTextBoxDomain.Multiline = false;
            this.CustomTextBoxDomain.Name = "CustomTextBoxDomain";
            this.CustomTextBoxDomain.ReadOnly = false;
            this.CustomTextBoxDomain.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxDomain.ShortcutsEnabled = true;
            this.CustomTextBoxDomain.Size = new System.Drawing.Size(200, 23);
            this.CustomTextBoxDomain.TabIndex = 0;
            this.CustomTextBoxDomain.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxDomain.Texts = "";
            this.CustomTextBoxDomain.UnderlinedStyle = true;
            this.CustomTextBoxDomain.UsePasswordChar = false;
            this.CustomTextBoxDomain.WordWrap = true;
            // 
            // CustomButtonLookup
            // 
            this.CustomButtonLookup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomButtonLookup.AutoSize = true;
            this.CustomButtonLookup.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonLookup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonLookup.Location = new System.Drawing.Point(370, 50);
            this.CustomButtonLookup.Name = "CustomButtonLookup";
            this.CustomButtonLookup.RoundedCorners = 5;
            this.CustomButtonLookup.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonLookup.Size = new System.Drawing.Size(75, 27);
            this.CustomButtonLookup.TabIndex = 7;
            this.CustomButtonLookup.Text = "Lookup";
            this.CustomButtonLookup.UseVisualStyleBackColor = true;
            this.CustomButtonLookup.Click += new System.EventHandler(this.CustomButtonLookup_Click);
            // 
            // CustomTextBoxResult
            // 
            this.CustomTextBoxResult.AcceptsReturn = false;
            this.CustomTextBoxResult.AcceptsTab = false;
            this.CustomTextBoxResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CustomTextBoxResult.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxResult.Border = true;
            this.CustomTextBoxResult.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxResult.BorderSize = 1;
            this.CustomTextBoxResult.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxResult.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxResult.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxResult.HideSelection = true;
            this.CustomTextBoxResult.Location = new System.Drawing.Point(12, 187);
            this.CustomTextBoxResult.MaxLength = 32767;
            this.CustomTextBoxResult.MinimumSize = new System.Drawing.Size(0, 23);
            this.CustomTextBoxResult.Multiline = true;
            this.CustomTextBoxResult.Name = "CustomTextBoxResult";
            this.CustomTextBoxResult.ReadOnly = true;
            this.CustomTextBoxResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CustomTextBoxResult.ShortcutsEnabled = true;
            this.CustomTextBoxResult.Size = new System.Drawing.Size(433, 262);
            this.CustomTextBoxResult.TabIndex = 0;
            this.CustomTextBoxResult.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxResult.Texts = "";
            this.CustomTextBoxResult.UnderlinedStyle = false;
            this.CustomTextBoxResult.UsePasswordChar = false;
            this.CustomTextBoxResult.WordWrap = true;
            // 
            // CustomCheckBoxHTTP3
            // 
            this.CustomCheckBoxHTTP3.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxHTTP3.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxHTTP3.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxHTTP3.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxHTTP3.Location = new System.Drawing.Point(12, 80);
            this.CustomCheckBoxHTTP3.Name = "CustomCheckBoxHTTP3";
            this.CustomCheckBoxHTTP3.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxHTTP3.Size = new System.Drawing.Size(107, 17);
            this.CustomCheckBoxHTTP3.TabIndex = 10;
            this.CustomCheckBoxHTTP3.Text = "HTTP/3 support";
            this.CustomCheckBoxHTTP3.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxVERIFY
            // 
            this.CustomCheckBoxVERIFY.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxVERIFY.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxVERIFY.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxVERIFY.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxVERIFY.Location = new System.Drawing.Point(132, 80);
            this.CustomCheckBoxVERIFY.Name = "CustomCheckBoxVERIFY";
            this.CustomCheckBoxVERIFY.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxVERIFY.Size = new System.Drawing.Size(189, 17);
            this.CustomCheckBoxVERIFY.TabIndex = 14;
            this.CustomCheckBoxVERIFY.Text = "Disable Certificates Verification";
            this.CustomCheckBoxVERIFY.UseVisualStyleBackColor = false;
            // 
            // CustomLabelRRTYPE
            // 
            this.CustomLabelRRTYPE.AutoSize = true;
            this.CustomLabelRRTYPE.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelRRTYPE.Border = false;
            this.CustomLabelRRTYPE.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelRRTYPE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelRRTYPE.ForeColor = System.Drawing.Color.White;
            this.CustomLabelRRTYPE.Location = new System.Drawing.Point(12, 130);
            this.CustomLabelRRTYPE.Name = "CustomLabelRRTYPE";
            this.CustomLabelRRTYPE.RoundedCorners = 0;
            this.CustomLabelRRTYPE.Size = new System.Drawing.Size(51, 17);
            this.CustomLabelRRTYPE.TabIndex = 15;
            this.CustomLabelRRTYPE.Text = "RRTYPE:";
            // 
            // CustomTextBoxRRTYPE
            // 
            this.CustomTextBoxRRTYPE.AcceptsReturn = false;
            this.CustomTextBoxRRTYPE.AcceptsTab = false;
            this.CustomTextBoxRRTYPE.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxRRTYPE.Border = true;
            this.CustomTextBoxRRTYPE.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxRRTYPE.BorderSize = 1;
            this.CustomTextBoxRRTYPE.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxRRTYPE.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxRRTYPE.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxRRTYPE.HideSelection = true;
            this.CustomTextBoxRRTYPE.Location = new System.Drawing.Point(69, 128);
            this.CustomTextBoxRRTYPE.MaxLength = 32767;
            this.CustomTextBoxRRTYPE.Multiline = false;
            this.CustomTextBoxRRTYPE.Name = "CustomTextBoxRRTYPE";
            this.CustomTextBoxRRTYPE.ReadOnly = false;
            this.CustomTextBoxRRTYPE.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxRRTYPE.ShortcutsEnabled = true;
            this.CustomTextBoxRRTYPE.Size = new System.Drawing.Size(50, 23);
            this.CustomTextBoxRRTYPE.TabIndex = 0;
            this.CustomTextBoxRRTYPE.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxRRTYPE.Texts = "A";
            this.CustomTextBoxRRTYPE.UnderlinedStyle = true;
            this.CustomTextBoxRRTYPE.UsePasswordChar = false;
            this.CustomTextBoxRRTYPE.WordWrap = true;
            // 
            // CustomLabelCLASS
            // 
            this.CustomLabelCLASS.AutoSize = true;
            this.CustomLabelCLASS.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelCLASS.Border = false;
            this.CustomLabelCLASS.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelCLASS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelCLASS.ForeColor = System.Drawing.Color.White;
            this.CustomLabelCLASS.Location = new System.Drawing.Point(130, 130);
            this.CustomLabelCLASS.Name = "CustomLabelCLASS";
            this.CustomLabelCLASS.RoundedCorners = 0;
            this.CustomLabelCLASS.Size = new System.Drawing.Size(46, 17);
            this.CustomLabelCLASS.TabIndex = 17;
            this.CustomLabelCLASS.Text = "CLASS:";
            // 
            // CustomTextBoxCLASS
            // 
            this.CustomTextBoxCLASS.AcceptsReturn = false;
            this.CustomTextBoxCLASS.AcceptsTab = false;
            this.CustomTextBoxCLASS.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxCLASS.Border = true;
            this.CustomTextBoxCLASS.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxCLASS.BorderSize = 1;
            this.CustomTextBoxCLASS.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxCLASS.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxCLASS.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxCLASS.HideSelection = true;
            this.CustomTextBoxCLASS.Location = new System.Drawing.Point(182, 128);
            this.CustomTextBoxCLASS.MaxLength = 32767;
            this.CustomTextBoxCLASS.Multiline = false;
            this.CustomTextBoxCLASS.Name = "CustomTextBoxCLASS";
            this.CustomTextBoxCLASS.ReadOnly = false;
            this.CustomTextBoxCLASS.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxCLASS.ShortcutsEnabled = true;
            this.CustomTextBoxCLASS.Size = new System.Drawing.Size(50, 23);
            this.CustomTextBoxCLASS.TabIndex = 0;
            this.CustomTextBoxCLASS.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxCLASS.Texts = "IN";
            this.CustomTextBoxCLASS.UnderlinedStyle = true;
            this.CustomTextBoxCLASS.UsePasswordChar = false;
            this.CustomTextBoxCLASS.WordWrap = true;
            // 
            // CustomCheckBoxDNSSEC
            // 
            this.CustomCheckBoxDNSSEC.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxDNSSEC.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDNSSEC.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxDNSSEC.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxDNSSEC.Location = new System.Drawing.Point(12, 100);
            this.CustomCheckBoxDNSSEC.Name = "CustomCheckBoxDNSSEC";
            this.CustomCheckBoxDNSSEC.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxDNSSEC.Size = new System.Drawing.Size(65, 17);
            this.CustomCheckBoxDNSSEC.TabIndex = 19;
            this.CustomCheckBoxDNSSEC.Text = "DNSSEC";
            this.CustomCheckBoxDNSSEC.UseVisualStyleBackColor = false;
            // 
            // CustomLabelSUBNET
            // 
            this.CustomLabelSUBNET.AutoSize = true;
            this.CustomLabelSUBNET.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelSUBNET.Border = false;
            this.CustomLabelSUBNET.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelSUBNET.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelSUBNET.ForeColor = System.Drawing.Color.White;
            this.CustomLabelSUBNET.Location = new System.Drawing.Point(249, 130);
            this.CustomLabelSUBNET.Name = "CustomLabelSUBNET";
            this.CustomLabelSUBNET.RoundedCorners = 0;
            this.CustomLabelSUBNET.Size = new System.Drawing.Size(54, 17);
            this.CustomLabelSUBNET.TabIndex = 20;
            this.CustomLabelSUBNET.Text = "SUBNET:";
            // 
            // CustomTextBoxSUBNET
            // 
            this.CustomTextBoxSUBNET.AcceptsReturn = false;
            this.CustomTextBoxSUBNET.AcceptsTab = false;
            this.CustomTextBoxSUBNET.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxSUBNET.Border = true;
            this.CustomTextBoxSUBNET.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxSUBNET.BorderSize = 1;
            this.CustomTextBoxSUBNET.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxSUBNET.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxSUBNET.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxSUBNET.HideSelection = true;
            this.CustomTextBoxSUBNET.Location = new System.Drawing.Point(309, 128);
            this.CustomTextBoxSUBNET.MaxLength = 32767;
            this.CustomTextBoxSUBNET.Multiline = false;
            this.CustomTextBoxSUBNET.Name = "CustomTextBoxSUBNET";
            this.CustomTextBoxSUBNET.ReadOnly = false;
            this.CustomTextBoxSUBNET.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxSUBNET.ShortcutsEnabled = true;
            this.CustomTextBoxSUBNET.Size = new System.Drawing.Size(100, 23);
            this.CustomTextBoxSUBNET.TabIndex = 0;
            this.CustomTextBoxSUBNET.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxSUBNET.Texts = "";
            this.CustomTextBoxSUBNET.UnderlinedStyle = true;
            this.CustomTextBoxSUBNET.UsePasswordChar = false;
            this.CustomTextBoxSUBNET.WordWrap = true;
            // 
            // CustomCheckBoxPAD
            // 
            this.CustomCheckBoxPAD.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxPAD.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxPAD.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxPAD.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxPAD.Location = new System.Drawing.Point(83, 100);
            this.CustomCheckBoxPAD.Name = "CustomCheckBoxPAD";
            this.CustomCheckBoxPAD.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxPAD.Size = new System.Drawing.Size(132, 17);
            this.CustomCheckBoxPAD.TabIndex = 22;
            this.CustomCheckBoxPAD.Text = "Add EDNS0 Padding";
            this.CustomCheckBoxPAD.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxVERBOSE
            // 
            this.CustomCheckBoxVERBOSE.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxVERBOSE.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxVERBOSE.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxVERBOSE.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxVERBOSE.Location = new System.Drawing.Point(221, 100);
            this.CustomCheckBoxVERBOSE.Name = "CustomCheckBoxVERBOSE";
            this.CustomCheckBoxVERBOSE.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxVERBOSE.Size = new System.Drawing.Size(113, 17);
            this.CustomCheckBoxVERBOSE.TabIndex = 23;
            this.CustomCheckBoxVERBOSE.Text = "Verbose Logging";
            this.CustomCheckBoxVERBOSE.UseVisualStyleBackColor = false;
            // 
            // CustomLabelEDNSOPT
            // 
            this.CustomLabelEDNSOPT.AutoSize = true;
            this.CustomLabelEDNSOPT.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelEDNSOPT.Border = false;
            this.CustomLabelEDNSOPT.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelEDNSOPT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelEDNSOPT.ForeColor = System.Drawing.Color.White;
            this.CustomLabelEDNSOPT.Location = new System.Drawing.Point(12, 160);
            this.CustomLabelEDNSOPT.Name = "CustomLabelEDNSOPT";
            this.CustomLabelEDNSOPT.RoundedCorners = 0;
            this.CustomLabelEDNSOPT.Size = new System.Drawing.Size(82, 17);
            this.CustomLabelEDNSOPT.TabIndex = 24;
            this.CustomLabelEDNSOPT.Text = "Specify EDNS:";
            // 
            // CustomTextBoxEDNSOPT
            // 
            this.CustomTextBoxEDNSOPT.AcceptsReturn = false;
            this.CustomTextBoxEDNSOPT.AcceptsTab = false;
            this.CustomTextBoxEDNSOPT.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxEDNSOPT.Border = true;
            this.CustomTextBoxEDNSOPT.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxEDNSOPT.BorderSize = 1;
            this.CustomTextBoxEDNSOPT.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxEDNSOPT.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxEDNSOPT.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxEDNSOPT.HideSelection = true;
            this.CustomTextBoxEDNSOPT.Location = new System.Drawing.Point(100, 158);
            this.CustomTextBoxEDNSOPT.MaxLength = 32767;
            this.CustomTextBoxEDNSOPT.Multiline = false;
            this.CustomTextBoxEDNSOPT.Name = "CustomTextBoxEDNSOPT";
            this.CustomTextBoxEDNSOPT.ReadOnly = false;
            this.CustomTextBoxEDNSOPT.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxEDNSOPT.ShortcutsEnabled = true;
            this.CustomTextBoxEDNSOPT.Size = new System.Drawing.Size(200, 23);
            this.CustomTextBoxEDNSOPT.TabIndex = 0;
            this.CustomTextBoxEDNSOPT.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxEDNSOPT.Texts = "";
            this.CustomTextBoxEDNSOPT.UnderlinedStyle = true;
            this.CustomTextBoxEDNSOPT.UsePasswordChar = false;
            this.CustomTextBoxEDNSOPT.WordWrap = true;
            // 
            // CustomButtonDefault
            // 
            this.CustomButtonDefault.AutoSize = true;
            this.CustomButtonDefault.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonDefault.Location = new System.Drawing.Point(370, 12);
            this.CustomButtonDefault.Name = "CustomButtonDefault";
            this.CustomButtonDefault.RoundedCorners = 5;
            this.CustomButtonDefault.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonDefault.Size = new System.Drawing.Size(75, 27);
            this.CustomButtonDefault.TabIndex = 26;
            this.CustomButtonDefault.Text = "Default";
            this.CustomButtonDefault.UseVisualStyleBackColor = true;
            this.CustomButtonDefault.Click += new System.EventHandler(this.CustomButtonDefault_Click);
            // 
            // CustomCheckBoxJSON
            // 
            this.CustomCheckBoxJSON.BackColor = System.Drawing.Color.DimGray;
            this.CustomCheckBoxJSON.BorderColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxJSON.CheckColor = System.Drawing.Color.Blue;
            this.CustomCheckBoxJSON.ForeColor = System.Drawing.Color.White;
            this.CustomCheckBoxJSON.Location = new System.Drawing.Point(340, 100);
            this.CustomCheckBoxJSON.Name = "CustomCheckBoxJSON";
            this.CustomCheckBoxJSON.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomCheckBoxJSON.Size = new System.Drawing.Size(44, 17);
            this.CustomCheckBoxJSON.TabIndex = 34;
            this.CustomCheckBoxJSON.Text = "Json";
            this.CustomCheckBoxJSON.UseVisualStyleBackColor = false;
            // 
            // FormDnsLookup
            // 
            this.AcceptButton = this.CustomButtonLookup;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(459, 461);
            this.Controls.Add(this.CustomCheckBoxJSON);
            this.Controls.Add(this.CustomButtonDefault);
            this.Controls.Add(this.CustomTextBoxEDNSOPT);
            this.Controls.Add(this.CustomLabelEDNSOPT);
            this.Controls.Add(this.CustomCheckBoxVERBOSE);
            this.Controls.Add(this.CustomCheckBoxPAD);
            this.Controls.Add(this.CustomTextBoxSUBNET);
            this.Controls.Add(this.CustomLabelSUBNET);
            this.Controls.Add(this.CustomCheckBoxDNSSEC);
            this.Controls.Add(this.CustomTextBoxCLASS);
            this.Controls.Add(this.CustomLabelCLASS);
            this.Controls.Add(this.CustomTextBoxRRTYPE);
            this.Controls.Add(this.CustomLabelRRTYPE);
            this.Controls.Add(this.CustomCheckBoxVERIFY);
            this.Controls.Add(this.CustomCheckBoxHTTP3);
            this.Controls.Add(this.CustomTextBoxResult);
            this.Controls.Add(this.CustomButtonLookup);
            this.Controls.Add(this.CustomTextBoxDomain);
            this.Controls.Add(this.CustomLabelDomain);
            this.Controls.Add(this.CustomTextBoxSourceCustom);
            this.Controls.Add(this.CustomRadioButtonSourceCustom);
            this.Controls.Add(this.CustomRadioButtonSourceSDC);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(475, 500);
            this.MinimumSize = new System.Drawing.Size(475, 500);
            this.Name = "FormDnsLookup";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "DNS Lookup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormDnsLookup_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CustomControls.CustomRadioButton CustomRadioButtonSourceSDC;
        private CustomControls.CustomRadioButton CustomRadioButtonSourceCustom;
        private CustomControls.CustomTextBox CustomTextBoxSourceCustom;
        private CustomControls.CustomLabel CustomLabelDomain;
        private CustomControls.CustomTextBox CustomTextBoxDomain;
        private CustomControls.CustomButton CustomButtonLookup;
        private CustomControls.CustomTextBox CustomTextBoxResult;
        private CustomControls.CustomCheckBox CustomCheckBoxHTTP3;
        private CustomControls.CustomCheckBox CustomCheckBoxVERIFY;
        private CustomControls.CustomLabel CustomLabelRRTYPE;
        private CustomControls.CustomTextBox CustomTextBoxRRTYPE;
        private CustomControls.CustomLabel CustomLabelCLASS;
        private CustomControls.CustomTextBox CustomTextBoxCLASS;
        private CustomControls.CustomCheckBox CustomCheckBoxDNSSEC;
        private CustomControls.CustomLabel CustomLabelSUBNET;
        private CustomControls.CustomTextBox CustomTextBoxSUBNET;
        private CustomControls.CustomCheckBox CustomCheckBoxPAD;
        private CustomControls.CustomCheckBox CustomCheckBoxVERBOSE;
        private CustomControls.CustomLabel CustomLabelEDNSOPT;
        private CustomControls.CustomTextBox CustomTextBoxEDNSOPT;
        private CustomControls.CustomButton CustomButtonDefault;
        private CustomControls.CustomCheckBox CustomCheckBoxJSON;
    }
}