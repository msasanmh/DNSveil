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
            CustomRadioButtonSourceSDC = new CustomControls.CustomRadioButton();
            CustomRadioButtonSourceCustom = new CustomControls.CustomRadioButton();
            CustomTextBoxSourceCustom = new CustomControls.CustomTextBox();
            CustomLabelDomain = new CustomControls.CustomLabel();
            CustomTextBoxDomain = new CustomControls.CustomTextBox();
            CustomButtonLookup = new CustomControls.CustomButton();
            CustomTextBoxResult = new CustomControls.CustomTextBox();
            CustomCheckBoxHTTP3 = new CustomControls.CustomCheckBox();
            CustomCheckBoxVERIFY = new CustomControls.CustomCheckBox();
            CustomLabelRRTYPE = new CustomControls.CustomLabel();
            CustomTextBoxRRTYPE = new CustomControls.CustomTextBox();
            CustomLabelCLASS = new CustomControls.CustomLabel();
            CustomTextBoxCLASS = new CustomControls.CustomTextBox();
            CustomCheckBoxDNSSEC = new CustomControls.CustomCheckBox();
            CustomLabelSUBNET = new CustomControls.CustomLabel();
            CustomTextBoxSUBNET = new CustomControls.CustomTextBox();
            CustomCheckBoxPAD = new CustomControls.CustomCheckBox();
            CustomCheckBoxVERBOSE = new CustomControls.CustomCheckBox();
            CustomLabelEDNSOPT = new CustomControls.CustomLabel();
            CustomTextBoxEDNSOPT = new CustomControls.CustomTextBox();
            CustomButtonDefault = new CustomControls.CustomButton();
            CustomCheckBoxJSON = new CustomControls.CustomCheckBox();
            SuspendLayout();
            // 
            // CustomRadioButtonSourceSDC
            // 
            CustomRadioButtonSourceSDC.BackColor = Color.DimGray;
            CustomRadioButtonSourceSDC.BorderColor = Color.Blue;
            CustomRadioButtonSourceSDC.CheckColor = Color.Blue;
            CustomRadioButtonSourceSDC.Checked = true;
            CustomRadioButtonSourceSDC.ForeColor = Color.White;
            CustomRadioButtonSourceSDC.Location = new Point(12, 10);
            CustomRadioButtonSourceSDC.Name = "CustomRadioButtonSourceSDC";
            CustomRadioButtonSourceSDC.SelectionColor = Color.LightBlue;
            CustomRadioButtonSourceSDC.Size = new Size(47, 17);
            CustomRadioButtonSourceSDC.TabIndex = 0;
            CustomRadioButtonSourceSDC.TabStop = true;
            CustomRadioButtonSourceSDC.Text = "SDC";
            CustomRadioButtonSourceSDC.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSourceCustom
            // 
            CustomRadioButtonSourceCustom.BackColor = Color.DimGray;
            CustomRadioButtonSourceCustom.BorderColor = Color.Blue;
            CustomRadioButtonSourceCustom.CheckColor = Color.Blue;
            CustomRadioButtonSourceCustom.ForeColor = Color.White;
            CustomRadioButtonSourceCustom.Location = new Point(61, 10);
            CustomRadioButtonSourceCustom.Name = "CustomRadioButtonSourceCustom";
            CustomRadioButtonSourceCustom.SelectionColor = Color.LightBlue;
            CustomRadioButtonSourceCustom.Size = new Size(69, 17);
            CustomRadioButtonSourceCustom.TabIndex = 1;
            CustomRadioButtonSourceCustom.Text = "Custom:";
            CustomRadioButtonSourceCustom.UseVisualStyleBackColor = false;
            // 
            // CustomTextBoxSourceCustom
            // 
            CustomTextBoxSourceCustom.AcceptsReturn = false;
            CustomTextBoxSourceCustom.AcceptsTab = false;
            CustomTextBoxSourceCustom.BackColor = Color.DimGray;
            CustomTextBoxSourceCustom.Border = true;
            CustomTextBoxSourceCustom.BorderColor = Color.Blue;
            CustomTextBoxSourceCustom.BorderSize = 1;
            CustomTextBoxSourceCustom.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxSourceCustom.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxSourceCustom.ForeColor = Color.White;
            CustomTextBoxSourceCustom.HideSelection = true;
            CustomTextBoxSourceCustom.Location = new Point(132, 8);
            CustomTextBoxSourceCustom.MaxLength = 32767;
            CustomTextBoxSourceCustom.Multiline = false;
            CustomTextBoxSourceCustom.Name = "CustomTextBoxSourceCustom";
            CustomTextBoxSourceCustom.ReadOnly = false;
            CustomTextBoxSourceCustom.RoundedCorners = 0;
            CustomTextBoxSourceCustom.ScrollBars = ScrollBars.None;
            CustomTextBoxSourceCustom.ShortcutsEnabled = true;
            CustomTextBoxSourceCustom.Size = new Size(200, 23);
            CustomTextBoxSourceCustom.TabIndex = 0;
            CustomTextBoxSourceCustom.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxSourceCustom.Texts = "";
            CustomTextBoxSourceCustom.UnderlinedStyle = true;
            CustomTextBoxSourceCustom.UsePasswordChar = false;
            CustomTextBoxSourceCustom.WordWrap = true;
            // 
            // CustomLabelDomain
            // 
            CustomLabelDomain.AutoSize = true;
            CustomLabelDomain.BackColor = Color.DimGray;
            CustomLabelDomain.Border = false;
            CustomLabelDomain.BorderColor = Color.Blue;
            CustomLabelDomain.FlatStyle = FlatStyle.Flat;
            CustomLabelDomain.ForeColor = Color.White;
            CustomLabelDomain.Location = new Point(74, 50);
            CustomLabelDomain.Name = "CustomLabelDomain";
            CustomLabelDomain.RoundedCorners = 0;
            CustomLabelDomain.Size = new Size(54, 17);
            CustomLabelDomain.TabIndex = 3;
            CustomLabelDomain.Text = "Domain:";
            // 
            // CustomTextBoxDomain
            // 
            CustomTextBoxDomain.AcceptsReturn = false;
            CustomTextBoxDomain.AcceptsTab = false;
            CustomTextBoxDomain.BackColor = Color.DimGray;
            CustomTextBoxDomain.Border = true;
            CustomTextBoxDomain.BorderColor = Color.Blue;
            CustomTextBoxDomain.BorderSize = 1;
            CustomTextBoxDomain.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxDomain.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxDomain.ForeColor = Color.White;
            CustomTextBoxDomain.HideSelection = true;
            CustomTextBoxDomain.Location = new Point(132, 48);
            CustomTextBoxDomain.MaxLength = 32767;
            CustomTextBoxDomain.Multiline = false;
            CustomTextBoxDomain.Name = "CustomTextBoxDomain";
            CustomTextBoxDomain.ReadOnly = false;
            CustomTextBoxDomain.RoundedCorners = 0;
            CustomTextBoxDomain.ScrollBars = ScrollBars.None;
            CustomTextBoxDomain.ShortcutsEnabled = true;
            CustomTextBoxDomain.Size = new Size(200, 23);
            CustomTextBoxDomain.TabIndex = 0;
            CustomTextBoxDomain.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxDomain.Texts = "";
            CustomTextBoxDomain.UnderlinedStyle = true;
            CustomTextBoxDomain.UsePasswordChar = false;
            CustomTextBoxDomain.WordWrap = true;
            // 
            // CustomButtonLookup
            // 
            CustomButtonLookup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CustomButtonLookup.AutoSize = true;
            CustomButtonLookup.BorderColor = Color.Blue;
            CustomButtonLookup.FlatStyle = FlatStyle.Flat;
            CustomButtonLookup.Location = new Point(370, 50);
            CustomButtonLookup.Name = "CustomButtonLookup";
            CustomButtonLookup.RoundedCorners = 5;
            CustomButtonLookup.SelectionColor = Color.LightBlue;
            CustomButtonLookup.Size = new Size(75, 27);
            CustomButtonLookup.TabIndex = 7;
            CustomButtonLookup.Text = "Lookup";
            CustomButtonLookup.UseVisualStyleBackColor = true;
            CustomButtonLookup.Click += CustomButtonLookup_Click;
            // 
            // CustomTextBoxResult
            // 
            CustomTextBoxResult.AcceptsReturn = false;
            CustomTextBoxResult.AcceptsTab = false;
            CustomTextBoxResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CustomTextBoxResult.BackColor = Color.DimGray;
            CustomTextBoxResult.Border = true;
            CustomTextBoxResult.BorderColor = Color.Blue;
            CustomTextBoxResult.BorderSize = 1;
            CustomTextBoxResult.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxResult.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxResult.ForeColor = Color.White;
            CustomTextBoxResult.HideSelection = true;
            CustomTextBoxResult.Location = new Point(12, 187);
            CustomTextBoxResult.MaxLength = 32767;
            CustomTextBoxResult.MinimumSize = new Size(0, 23);
            CustomTextBoxResult.Multiline = true;
            CustomTextBoxResult.Name = "CustomTextBoxResult";
            CustomTextBoxResult.ReadOnly = true;
            CustomTextBoxResult.RoundedCorners = 5;
            CustomTextBoxResult.ScrollBars = ScrollBars.Vertical;
            CustomTextBoxResult.ShortcutsEnabled = true;
            CustomTextBoxResult.Size = new Size(433, 262);
            CustomTextBoxResult.TabIndex = 0;
            CustomTextBoxResult.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxResult.Texts = "";
            CustomTextBoxResult.UnderlinedStyle = false;
            CustomTextBoxResult.UsePasswordChar = false;
            CustomTextBoxResult.WordWrap = true;
            // 
            // CustomCheckBoxHTTP3
            // 
            CustomCheckBoxHTTP3.BackColor = Color.DimGray;
            CustomCheckBoxHTTP3.BorderColor = Color.Blue;
            CustomCheckBoxHTTP3.CheckColor = Color.Blue;
            CustomCheckBoxHTTP3.ForeColor = Color.White;
            CustomCheckBoxHTTP3.Location = new Point(12, 80);
            CustomCheckBoxHTTP3.Name = "CustomCheckBoxHTTP3";
            CustomCheckBoxHTTP3.SelectionColor = Color.LightBlue;
            CustomCheckBoxHTTP3.Size = new Size(110, 17);
            CustomCheckBoxHTTP3.TabIndex = 10;
            CustomCheckBoxHTTP3.Text = "HTTP/3 support";
            CustomCheckBoxHTTP3.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxVERIFY
            // 
            CustomCheckBoxVERIFY.BackColor = Color.DimGray;
            CustomCheckBoxVERIFY.BorderColor = Color.Blue;
            CustomCheckBoxVERIFY.CheckColor = Color.Blue;
            CustomCheckBoxVERIFY.ForeColor = Color.White;
            CustomCheckBoxVERIFY.Location = new Point(132, 80);
            CustomCheckBoxVERIFY.Name = "CustomCheckBoxVERIFY";
            CustomCheckBoxVERIFY.SelectionColor = Color.LightBlue;
            CustomCheckBoxVERIFY.Size = new Size(193, 17);
            CustomCheckBoxVERIFY.TabIndex = 14;
            CustomCheckBoxVERIFY.Text = "Disable Certificates Verification";
            CustomCheckBoxVERIFY.UseVisualStyleBackColor = false;
            // 
            // CustomLabelRRTYPE
            // 
            CustomLabelRRTYPE.AutoSize = true;
            CustomLabelRRTYPE.BackColor = Color.DimGray;
            CustomLabelRRTYPE.Border = false;
            CustomLabelRRTYPE.BorderColor = Color.Blue;
            CustomLabelRRTYPE.FlatStyle = FlatStyle.Flat;
            CustomLabelRRTYPE.ForeColor = Color.White;
            CustomLabelRRTYPE.Location = new Point(12, 130);
            CustomLabelRRTYPE.Name = "CustomLabelRRTYPE";
            CustomLabelRRTYPE.RoundedCorners = 0;
            CustomLabelRRTYPE.Size = new Size(51, 17);
            CustomLabelRRTYPE.TabIndex = 15;
            CustomLabelRRTYPE.Text = "RRTYPE:";
            // 
            // CustomTextBoxRRTYPE
            // 
            CustomTextBoxRRTYPE.AcceptsReturn = false;
            CustomTextBoxRRTYPE.AcceptsTab = false;
            CustomTextBoxRRTYPE.BackColor = Color.DimGray;
            CustomTextBoxRRTYPE.Border = true;
            CustomTextBoxRRTYPE.BorderColor = Color.Blue;
            CustomTextBoxRRTYPE.BorderSize = 1;
            CustomTextBoxRRTYPE.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxRRTYPE.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxRRTYPE.ForeColor = Color.White;
            CustomTextBoxRRTYPE.HideSelection = true;
            CustomTextBoxRRTYPE.Location = new Point(69, 128);
            CustomTextBoxRRTYPE.MaxLength = 32767;
            CustomTextBoxRRTYPE.Multiline = false;
            CustomTextBoxRRTYPE.Name = "CustomTextBoxRRTYPE";
            CustomTextBoxRRTYPE.ReadOnly = false;
            CustomTextBoxRRTYPE.RoundedCorners = 0;
            CustomTextBoxRRTYPE.ScrollBars = ScrollBars.None;
            CustomTextBoxRRTYPE.ShortcutsEnabled = true;
            CustomTextBoxRRTYPE.Size = new Size(50, 23);
            CustomTextBoxRRTYPE.TabIndex = 0;
            CustomTextBoxRRTYPE.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxRRTYPE.Texts = "A";
            CustomTextBoxRRTYPE.UnderlinedStyle = true;
            CustomTextBoxRRTYPE.UsePasswordChar = false;
            CustomTextBoxRRTYPE.WordWrap = true;
            // 
            // CustomLabelCLASS
            // 
            CustomLabelCLASS.AutoSize = true;
            CustomLabelCLASS.BackColor = Color.DimGray;
            CustomLabelCLASS.Border = false;
            CustomLabelCLASS.BorderColor = Color.Blue;
            CustomLabelCLASS.FlatStyle = FlatStyle.Flat;
            CustomLabelCLASS.ForeColor = Color.White;
            CustomLabelCLASS.Location = new Point(130, 130);
            CustomLabelCLASS.Name = "CustomLabelCLASS";
            CustomLabelCLASS.RoundedCorners = 0;
            CustomLabelCLASS.Size = new Size(46, 17);
            CustomLabelCLASS.TabIndex = 17;
            CustomLabelCLASS.Text = "CLASS:";
            // 
            // CustomTextBoxCLASS
            // 
            CustomTextBoxCLASS.AcceptsReturn = false;
            CustomTextBoxCLASS.AcceptsTab = false;
            CustomTextBoxCLASS.BackColor = Color.DimGray;
            CustomTextBoxCLASS.Border = true;
            CustomTextBoxCLASS.BorderColor = Color.Blue;
            CustomTextBoxCLASS.BorderSize = 1;
            CustomTextBoxCLASS.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxCLASS.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxCLASS.ForeColor = Color.White;
            CustomTextBoxCLASS.HideSelection = true;
            CustomTextBoxCLASS.Location = new Point(182, 128);
            CustomTextBoxCLASS.MaxLength = 32767;
            CustomTextBoxCLASS.Multiline = false;
            CustomTextBoxCLASS.Name = "CustomTextBoxCLASS";
            CustomTextBoxCLASS.ReadOnly = false;
            CustomTextBoxCLASS.RoundedCorners = 0;
            CustomTextBoxCLASS.ScrollBars = ScrollBars.None;
            CustomTextBoxCLASS.ShortcutsEnabled = true;
            CustomTextBoxCLASS.Size = new Size(50, 23);
            CustomTextBoxCLASS.TabIndex = 0;
            CustomTextBoxCLASS.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxCLASS.Texts = "IN";
            CustomTextBoxCLASS.UnderlinedStyle = true;
            CustomTextBoxCLASS.UsePasswordChar = false;
            CustomTextBoxCLASS.WordWrap = true;
            // 
            // CustomCheckBoxDNSSEC
            // 
            CustomCheckBoxDNSSEC.BackColor = Color.DimGray;
            CustomCheckBoxDNSSEC.BorderColor = Color.Blue;
            CustomCheckBoxDNSSEC.CheckColor = Color.Blue;
            CustomCheckBoxDNSSEC.ForeColor = Color.White;
            CustomCheckBoxDNSSEC.Location = new Point(12, 100);
            CustomCheckBoxDNSSEC.Name = "CustomCheckBoxDNSSEC";
            CustomCheckBoxDNSSEC.SelectionColor = Color.LightBlue;
            CustomCheckBoxDNSSEC.Size = new Size(69, 17);
            CustomCheckBoxDNSSEC.TabIndex = 19;
            CustomCheckBoxDNSSEC.Text = "DNSSEC";
            CustomCheckBoxDNSSEC.UseVisualStyleBackColor = false;
            // 
            // CustomLabelSUBNET
            // 
            CustomLabelSUBNET.AutoSize = true;
            CustomLabelSUBNET.BackColor = Color.DimGray;
            CustomLabelSUBNET.Border = false;
            CustomLabelSUBNET.BorderColor = Color.Blue;
            CustomLabelSUBNET.FlatStyle = FlatStyle.Flat;
            CustomLabelSUBNET.ForeColor = Color.White;
            CustomLabelSUBNET.Location = new Point(249, 130);
            CustomLabelSUBNET.Name = "CustomLabelSUBNET";
            CustomLabelSUBNET.RoundedCorners = 0;
            CustomLabelSUBNET.Size = new Size(54, 17);
            CustomLabelSUBNET.TabIndex = 20;
            CustomLabelSUBNET.Text = "SUBNET:";
            // 
            // CustomTextBoxSUBNET
            // 
            CustomTextBoxSUBNET.AcceptsReturn = false;
            CustomTextBoxSUBNET.AcceptsTab = false;
            CustomTextBoxSUBNET.BackColor = Color.DimGray;
            CustomTextBoxSUBNET.Border = true;
            CustomTextBoxSUBNET.BorderColor = Color.Blue;
            CustomTextBoxSUBNET.BorderSize = 1;
            CustomTextBoxSUBNET.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxSUBNET.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxSUBNET.ForeColor = Color.White;
            CustomTextBoxSUBNET.HideSelection = true;
            CustomTextBoxSUBNET.Location = new Point(309, 128);
            CustomTextBoxSUBNET.MaxLength = 32767;
            CustomTextBoxSUBNET.Multiline = false;
            CustomTextBoxSUBNET.Name = "CustomTextBoxSUBNET";
            CustomTextBoxSUBNET.ReadOnly = false;
            CustomTextBoxSUBNET.RoundedCorners = 0;
            CustomTextBoxSUBNET.ScrollBars = ScrollBars.None;
            CustomTextBoxSUBNET.ShortcutsEnabled = true;
            CustomTextBoxSUBNET.Size = new Size(100, 23);
            CustomTextBoxSUBNET.TabIndex = 0;
            CustomTextBoxSUBNET.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxSUBNET.Texts = "";
            CustomTextBoxSUBNET.UnderlinedStyle = true;
            CustomTextBoxSUBNET.UsePasswordChar = false;
            CustomTextBoxSUBNET.WordWrap = true;
            // 
            // CustomCheckBoxPAD
            // 
            CustomCheckBoxPAD.BackColor = Color.DimGray;
            CustomCheckBoxPAD.BorderColor = Color.Blue;
            CustomCheckBoxPAD.CheckColor = Color.Blue;
            CustomCheckBoxPAD.ForeColor = Color.White;
            CustomCheckBoxPAD.Location = new Point(83, 100);
            CustomCheckBoxPAD.Name = "CustomCheckBoxPAD";
            CustomCheckBoxPAD.SelectionColor = Color.LightBlue;
            CustomCheckBoxPAD.Size = new Size(136, 17);
            CustomCheckBoxPAD.TabIndex = 22;
            CustomCheckBoxPAD.Text = "Add EDNS0 Padding";
            CustomCheckBoxPAD.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxVERBOSE
            // 
            CustomCheckBoxVERBOSE.BackColor = Color.DimGray;
            CustomCheckBoxVERBOSE.BorderColor = Color.Blue;
            CustomCheckBoxVERBOSE.CheckColor = Color.Blue;
            CustomCheckBoxVERBOSE.ForeColor = Color.White;
            CustomCheckBoxVERBOSE.Location = new Point(221, 100);
            CustomCheckBoxVERBOSE.Name = "CustomCheckBoxVERBOSE";
            CustomCheckBoxVERBOSE.SelectionColor = Color.LightBlue;
            CustomCheckBoxVERBOSE.Size = new Size(117, 17);
            CustomCheckBoxVERBOSE.TabIndex = 23;
            CustomCheckBoxVERBOSE.Text = "Verbose Logging";
            CustomCheckBoxVERBOSE.UseVisualStyleBackColor = false;
            // 
            // CustomLabelEDNSOPT
            // 
            CustomLabelEDNSOPT.AutoSize = true;
            CustomLabelEDNSOPT.BackColor = Color.DimGray;
            CustomLabelEDNSOPT.Border = false;
            CustomLabelEDNSOPT.BorderColor = Color.Blue;
            CustomLabelEDNSOPT.FlatStyle = FlatStyle.Flat;
            CustomLabelEDNSOPT.ForeColor = Color.White;
            CustomLabelEDNSOPT.Location = new Point(12, 160);
            CustomLabelEDNSOPT.Name = "CustomLabelEDNSOPT";
            CustomLabelEDNSOPT.RoundedCorners = 0;
            CustomLabelEDNSOPT.Size = new Size(82, 17);
            CustomLabelEDNSOPT.TabIndex = 24;
            CustomLabelEDNSOPT.Text = "Specify EDNS:";
            // 
            // CustomTextBoxEDNSOPT
            // 
            CustomTextBoxEDNSOPT.AcceptsReturn = false;
            CustomTextBoxEDNSOPT.AcceptsTab = false;
            CustomTextBoxEDNSOPT.BackColor = Color.DimGray;
            CustomTextBoxEDNSOPT.Border = true;
            CustomTextBoxEDNSOPT.BorderColor = Color.Blue;
            CustomTextBoxEDNSOPT.BorderSize = 1;
            CustomTextBoxEDNSOPT.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxEDNSOPT.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxEDNSOPT.ForeColor = Color.White;
            CustomTextBoxEDNSOPT.HideSelection = true;
            CustomTextBoxEDNSOPT.Location = new Point(100, 158);
            CustomTextBoxEDNSOPT.MaxLength = 32767;
            CustomTextBoxEDNSOPT.Multiline = false;
            CustomTextBoxEDNSOPT.Name = "CustomTextBoxEDNSOPT";
            CustomTextBoxEDNSOPT.ReadOnly = false;
            CustomTextBoxEDNSOPT.RoundedCorners = 0;
            CustomTextBoxEDNSOPT.ScrollBars = ScrollBars.None;
            CustomTextBoxEDNSOPT.ShortcutsEnabled = true;
            CustomTextBoxEDNSOPT.Size = new Size(200, 23);
            CustomTextBoxEDNSOPT.TabIndex = 0;
            CustomTextBoxEDNSOPT.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxEDNSOPT.Texts = "";
            CustomTextBoxEDNSOPT.UnderlinedStyle = true;
            CustomTextBoxEDNSOPT.UsePasswordChar = false;
            CustomTextBoxEDNSOPT.WordWrap = true;
            // 
            // CustomButtonDefault
            // 
            CustomButtonDefault.AutoSize = true;
            CustomButtonDefault.BorderColor = Color.Blue;
            CustomButtonDefault.FlatStyle = FlatStyle.Flat;
            CustomButtonDefault.Location = new Point(370, 12);
            CustomButtonDefault.Name = "CustomButtonDefault";
            CustomButtonDefault.RoundedCorners = 5;
            CustomButtonDefault.SelectionColor = Color.LightBlue;
            CustomButtonDefault.Size = new Size(75, 27);
            CustomButtonDefault.TabIndex = 26;
            CustomButtonDefault.Text = "Default";
            CustomButtonDefault.UseVisualStyleBackColor = true;
            CustomButtonDefault.Click += CustomButtonDefault_Click;
            // 
            // CustomCheckBoxJSON
            // 
            CustomCheckBoxJSON.BackColor = Color.DimGray;
            CustomCheckBoxJSON.BorderColor = Color.Blue;
            CustomCheckBoxJSON.CheckColor = Color.Blue;
            CustomCheckBoxJSON.ForeColor = Color.White;
            CustomCheckBoxJSON.Location = new Point(340, 100);
            CustomCheckBoxJSON.Name = "CustomCheckBoxJSON";
            CustomCheckBoxJSON.SelectionColor = Color.LightBlue;
            CustomCheckBoxJSON.Size = new Size(48, 17);
            CustomCheckBoxJSON.TabIndex = 34;
            CustomCheckBoxJSON.Text = "Json";
            CustomCheckBoxJSON.UseVisualStyleBackColor = false;
            // 
            // FormDnsLookup
            // 
            AcceptButton = CustomButtonLookup;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.DimGray;
            ClientSize = new Size(459, 461);
            Controls.Add(CustomCheckBoxJSON);
            Controls.Add(CustomButtonDefault);
            Controls.Add(CustomTextBoxEDNSOPT);
            Controls.Add(CustomLabelEDNSOPT);
            Controls.Add(CustomCheckBoxVERBOSE);
            Controls.Add(CustomCheckBoxPAD);
            Controls.Add(CustomTextBoxSUBNET);
            Controls.Add(CustomLabelSUBNET);
            Controls.Add(CustomCheckBoxDNSSEC);
            Controls.Add(CustomTextBoxCLASS);
            Controls.Add(CustomLabelCLASS);
            Controls.Add(CustomTextBoxRRTYPE);
            Controls.Add(CustomLabelRRTYPE);
            Controls.Add(CustomCheckBoxVERIFY);
            Controls.Add(CustomCheckBoxHTTP3);
            Controls.Add(CustomTextBoxResult);
            Controls.Add(CustomButtonLookup);
            Controls.Add(CustomTextBoxDomain);
            Controls.Add(CustomLabelDomain);
            Controls.Add(CustomTextBoxSourceCustom);
            Controls.Add(CustomRadioButtonSourceCustom);
            Controls.Add(CustomRadioButtonSourceSDC);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MaximumSize = new Size(475, 500);
            MinimumSize = new Size(475, 500);
            Name = "FormDnsLookup";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "DNS Lookup";
            FormClosing += FormDnsLookup_FormClosing;
            ResumeLayout(false);
            PerformLayout();
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