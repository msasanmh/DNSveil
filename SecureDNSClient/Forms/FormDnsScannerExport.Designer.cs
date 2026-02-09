namespace SecureDNSClient
{
    partial class FormDnsScannerExport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDnsScannerExport));
            CustomLabelFilterExport = new CustomControls.CustomLabel();
            CustomCheckBoxSecureOnline = new CustomControls.CustomCheckBox();
            CustomCheckBoxInsecureOnline = new CustomControls.CustomCheckBox();
            CustomCheckBoxGoogleSafeSearchActive = new CustomControls.CustomCheckBox();
            CustomCheckBoxAdultContentFilter = new CustomControls.CustomCheckBox();
            CustomRadioButtonSortBySecure = new CustomControls.CustomRadioButton();
            CustomRadioButtonSortByInsecure = new CustomControls.CustomRadioButton();
            CustomButtonExport = new CustomControls.CustomButton();
            CustomCheckBoxBingSafeSearchActive = new CustomControls.CustomCheckBox();
            CustomCheckBoxYoutubeRestrictActive = new CustomControls.CustomCheckBox();
            SuspendLayout();
            // 
            // CustomLabelFilterExport
            // 
            CustomLabelFilterExport.AutoSize = true;
            CustomLabelFilterExport.BackColor = Color.DimGray;
            CustomLabelFilterExport.Border = false;
            CustomLabelFilterExport.BorderColor = Color.Blue;
            CustomLabelFilterExport.FlatStyle = FlatStyle.Flat;
            CustomLabelFilterExport.ForeColor = Color.White;
            CustomLabelFilterExport.Location = new Point(12, 9);
            CustomLabelFilterExport.Name = "CustomLabelFilterExport";
            CustomLabelFilterExport.RoundedCorners = 0;
            CustomLabelFilterExport.Size = new Size(119, 17);
            CustomLabelFilterExport.TabIndex = 0;
            CustomLabelFilterExport.Text = "Filter Export, where...";
            // 
            // CustomCheckBoxSecureOnline
            // 
            CustomCheckBoxSecureOnline.BackColor = Color.DimGray;
            CustomCheckBoxSecureOnline.BorderColor = Color.Blue;
            CustomCheckBoxSecureOnline.CheckColor = Color.Blue;
            CustomCheckBoxSecureOnline.Checked = true;
            CustomCheckBoxSecureOnline.CheckState = CheckState.Checked;
            CustomCheckBoxSecureOnline.ForeColor = Color.White;
            CustomCheckBoxSecureOnline.Location = new Point(12, 40);
            CustomCheckBoxSecureOnline.Name = "CustomCheckBoxSecureOnline";
            CustomCheckBoxSecureOnline.SelectionColor = Color.LightBlue;
            CustomCheckBoxSecureOnline.Size = new Size(145, 17);
            CustomCheckBoxSecureOnline.TabIndex = 1;
            CustomCheckBoxSecureOnline.Text = "Secure status is online";
            CustomCheckBoxSecureOnline.ThreeState = true;
            CustomCheckBoxSecureOnline.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxInsecureOnline
            // 
            CustomCheckBoxInsecureOnline.BackColor = Color.DimGray;
            CustomCheckBoxInsecureOnline.BorderColor = Color.Blue;
            CustomCheckBoxInsecureOnline.CheckColor = Color.Blue;
            CustomCheckBoxInsecureOnline.Checked = true;
            CustomCheckBoxInsecureOnline.CheckState = CheckState.Indeterminate;
            CustomCheckBoxInsecureOnline.ForeColor = Color.White;
            CustomCheckBoxInsecureOnline.Location = new Point(225, 40);
            CustomCheckBoxInsecureOnline.Name = "CustomCheckBoxInsecureOnline";
            CustomCheckBoxInsecureOnline.SelectionColor = Color.LightBlue;
            CustomCheckBoxInsecureOnline.Size = new Size(154, 17);
            CustomCheckBoxInsecureOnline.TabIndex = 2;
            CustomCheckBoxInsecureOnline.Text = "Insecure status is online";
            CustomCheckBoxInsecureOnline.ThreeState = true;
            CustomCheckBoxInsecureOnline.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxGoogleSafeSearchActive
            // 
            CustomCheckBoxGoogleSafeSearchActive.BackColor = Color.DimGray;
            CustomCheckBoxGoogleSafeSearchActive.BorderColor = Color.Blue;
            CustomCheckBoxGoogleSafeSearchActive.CheckColor = Color.Blue;
            CustomCheckBoxGoogleSafeSearchActive.ForeColor = Color.White;
            CustomCheckBoxGoogleSafeSearchActive.Location = new Point(12, 63);
            CustomCheckBoxGoogleSafeSearchActive.Name = "CustomCheckBoxGoogleSafeSearchActive";
            CustomCheckBoxGoogleSafeSearchActive.SelectionColor = Color.LightBlue;
            CustomCheckBoxGoogleSafeSearchActive.Size = new Size(174, 17);
            CustomCheckBoxGoogleSafeSearchActive.TabIndex = 3;
            CustomCheckBoxGoogleSafeSearchActive.Text = "Google safe search is active";
            CustomCheckBoxGoogleSafeSearchActive.ThreeState = true;
            CustomCheckBoxGoogleSafeSearchActive.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxAdultContentFilter
            // 
            CustomCheckBoxAdultContentFilter.BackColor = Color.DimGray;
            CustomCheckBoxAdultContentFilter.BorderColor = Color.Blue;
            CustomCheckBoxAdultContentFilter.CheckColor = Color.Blue;
            CustomCheckBoxAdultContentFilter.ForeColor = Color.White;
            CustomCheckBoxAdultContentFilter.Location = new Point(225, 86);
            CustomCheckBoxAdultContentFilter.Name = "CustomCheckBoxAdultContentFilter";
            CustomCheckBoxAdultContentFilter.SelectionColor = Color.LightBlue;
            CustomCheckBoxAdultContentFilter.Size = new Size(138, 17);
            CustomCheckBoxAdultContentFilter.TabIndex = 4;
            CustomCheckBoxAdultContentFilter.Text = "Adult content is filter";
            CustomCheckBoxAdultContentFilter.ThreeState = true;
            CustomCheckBoxAdultContentFilter.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSortBySecure
            // 
            CustomRadioButtonSortBySecure.BackColor = Color.DimGray;
            CustomRadioButtonSortBySecure.BorderColor = Color.Blue;
            CustomRadioButtonSortBySecure.CheckColor = Color.Blue;
            CustomRadioButtonSortBySecure.Checked = true;
            CustomRadioButtonSortBySecure.ForeColor = Color.White;
            CustomRadioButtonSortBySecure.Location = new Point(12, 128);
            CustomRadioButtonSortBySecure.Name = "CustomRadioButtonSortBySecure";
            CustomRadioButtonSortBySecure.SelectionColor = Color.LightBlue;
            CustomRadioButtonSortBySecure.Size = new Size(143, 17);
            CustomRadioButtonSortBySecure.TabIndex = 5;
            CustomRadioButtonSortBySecure.TabStop = true;
            CustomRadioButtonSortBySecure.Text = "Sort by secure latency";
            CustomRadioButtonSortBySecure.UseVisualStyleBackColor = false;
            // 
            // CustomRadioButtonSortByInsecure
            // 
            CustomRadioButtonSortByInsecure.BackColor = Color.DimGray;
            CustomRadioButtonSortByInsecure.BorderColor = Color.Blue;
            CustomRadioButtonSortByInsecure.CheckColor = Color.Blue;
            CustomRadioButtonSortByInsecure.ForeColor = Color.White;
            CustomRadioButtonSortByInsecure.Location = new Point(225, 128);
            CustomRadioButtonSortByInsecure.Name = "CustomRadioButtonSortByInsecure";
            CustomRadioButtonSortByInsecure.SelectionColor = Color.LightBlue;
            CustomRadioButtonSortByInsecure.Size = new Size(153, 17);
            CustomRadioButtonSortByInsecure.TabIndex = 6;
            CustomRadioButtonSortByInsecure.Text = "Sort by insecure latency";
            CustomRadioButtonSortByInsecure.UseVisualStyleBackColor = false;
            // 
            // CustomButtonExport
            // 
            CustomButtonExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CustomButtonExport.BorderColor = Color.Blue;
            CustomButtonExport.FlatStyle = FlatStyle.Flat;
            CustomButtonExport.Location = new Point(316, 170);
            CustomButtonExport.Name = "CustomButtonExport";
            CustomButtonExport.RoundedCorners = 5;
            CustomButtonExport.SelectionColor = Color.LightBlue;
            CustomButtonExport.Size = new Size(75, 27);
            CustomButtonExport.TabIndex = 7;
            CustomButtonExport.Text = "Export";
            CustomButtonExport.UseVisualStyleBackColor = true;
            CustomButtonExport.Click += CustomButtonExport_Click;
            // 
            // CustomCheckBoxBingSafeSearchActive
            // 
            CustomCheckBoxBingSafeSearchActive.BackColor = Color.DimGray;
            CustomCheckBoxBingSafeSearchActive.BorderColor = Color.Blue;
            CustomCheckBoxBingSafeSearchActive.CheckColor = Color.Blue;
            CustomCheckBoxBingSafeSearchActive.ForeColor = Color.White;
            CustomCheckBoxBingSafeSearchActive.Location = new Point(225, 63);
            CustomCheckBoxBingSafeSearchActive.Name = "CustomCheckBoxBingSafeSearchActive";
            CustomCheckBoxBingSafeSearchActive.SelectionColor = Color.LightBlue;
            CustomCheckBoxBingSafeSearchActive.Size = new Size(159, 17);
            CustomCheckBoxBingSafeSearchActive.TabIndex = 8;
            CustomCheckBoxBingSafeSearchActive.Text = "Bing safe search is active";
            CustomCheckBoxBingSafeSearchActive.ThreeState = true;
            CustomCheckBoxBingSafeSearchActive.UseVisualStyleBackColor = false;
            // 
            // CustomCheckBoxYoutubeRestrictActive
            // 
            CustomCheckBoxYoutubeRestrictActive.BackColor = Color.DimGray;
            CustomCheckBoxYoutubeRestrictActive.BorderColor = Color.Blue;
            CustomCheckBoxYoutubeRestrictActive.CheckColor = Color.Blue;
            CustomCheckBoxYoutubeRestrictActive.ForeColor = Color.White;
            CustomCheckBoxYoutubeRestrictActive.Location = new Point(12, 86);
            CustomCheckBoxYoutubeRestrictActive.Name = "CustomCheckBoxYoutubeRestrictActive";
            CustomCheckBoxYoutubeRestrictActive.SelectionColor = Color.LightBlue;
            CustomCheckBoxYoutubeRestrictActive.Size = new Size(175, 17);
            CustomCheckBoxYoutubeRestrictActive.TabIndex = 9;
            CustomCheckBoxYoutubeRestrictActive.Text = "Youtube restriction is active";
            CustomCheckBoxYoutubeRestrictActive.ThreeState = true;
            CustomCheckBoxYoutubeRestrictActive.UseVisualStyleBackColor = false;
            // 
            // FormDnsScannerExport
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.DimGray;
            ClientSize = new Size(419, 220);
            Controls.Add(CustomCheckBoxYoutubeRestrictActive);
            Controls.Add(CustomCheckBoxBingSafeSearchActive);
            Controls.Add(CustomButtonExport);
            Controls.Add(CustomRadioButtonSortByInsecure);
            Controls.Add(CustomRadioButtonSortBySecure);
            Controls.Add(CustomCheckBoxAdultContentFilter);
            Controls.Add(CustomCheckBoxGoogleSafeSearchActive);
            Controls.Add(CustomCheckBoxInsecureOnline);
            Controls.Add(CustomCheckBoxSecureOnline);
            Controls.Add(CustomLabelFilterExport);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormDnsScannerExport";
            Text = "Export Dns Scanner Data";
            FormClosing += FormDnsScannerExport_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CustomControls.CustomLabel CustomLabelFilterExport;
        private CustomControls.CustomCheckBox CustomCheckBoxSecureOnline;
        private CustomControls.CustomCheckBox CustomCheckBoxInsecureOnline;
        private CustomControls.CustomCheckBox CustomCheckBoxGoogleSafeSearchActive;
        private CustomControls.CustomCheckBox CustomCheckBoxAdultContentFilter;
        private CustomControls.CustomRadioButton CustomRadioButtonSortBySecure;
        private CustomControls.CustomRadioButton CustomRadioButtonSortByInsecure;
        private CustomControls.CustomButton CustomButtonExport;
        private CustomControls.CustomCheckBox CustomCheckBoxBingSafeSearchActive;
        private CustomControls.CustomCheckBox CustomCheckBoxYoutubeRestrictActive;
    }
}