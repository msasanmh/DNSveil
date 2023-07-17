namespace SecureDNSClient
{
    partial class FormStampReader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormStampReader));
            this.CustomLabelStampUrl = new CustomControls.CustomLabel();
            this.CustomTextBoxStampUrl = new CustomControls.CustomTextBox();
            this.CustomButtonDecode = new CustomControls.CustomButton();
            this.CustomTextBoxResult = new CustomControls.CustomTextBox();
            this.SuspendLayout();
            // 
            // CustomLabelStampUrl
            // 
            this.CustomLabelStampUrl.AutoSize = true;
            this.CustomLabelStampUrl.BackColor = System.Drawing.Color.DimGray;
            this.CustomLabelStampUrl.Border = false;
            this.CustomLabelStampUrl.BorderColor = System.Drawing.Color.Blue;
            this.CustomLabelStampUrl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomLabelStampUrl.ForeColor = System.Drawing.Color.White;
            this.CustomLabelStampUrl.Location = new System.Drawing.Point(12, 10);
            this.CustomLabelStampUrl.Name = "CustomLabelStampUrl";
            this.CustomLabelStampUrl.RoundedCorners = 0;
            this.CustomLabelStampUrl.Size = new System.Drawing.Size(64, 17);
            this.CustomLabelStampUrl.TabIndex = 0;
            this.CustomLabelStampUrl.Text = "Stamp Url:";
            // 
            // CustomTextBoxStampUrl
            // 
            this.CustomTextBoxStampUrl.AcceptsReturn = false;
            this.CustomTextBoxStampUrl.AcceptsTab = false;
            this.CustomTextBoxStampUrl.BackColor = System.Drawing.Color.DimGray;
            this.CustomTextBoxStampUrl.Border = true;
            this.CustomTextBoxStampUrl.BorderColor = System.Drawing.Color.Blue;
            this.CustomTextBoxStampUrl.BorderSize = 1;
            this.CustomTextBoxStampUrl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CustomTextBoxStampUrl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CustomTextBoxStampUrl.ForeColor = System.Drawing.Color.White;
            this.CustomTextBoxStampUrl.HideSelection = true;
            this.CustomTextBoxStampUrl.Location = new System.Drawing.Point(82, 8);
            this.CustomTextBoxStampUrl.MaxLength = 32767;
            this.CustomTextBoxStampUrl.Multiline = false;
            this.CustomTextBoxStampUrl.Name = "CustomTextBoxStampUrl";
            this.CustomTextBoxStampUrl.ReadOnly = false;
            this.CustomTextBoxStampUrl.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.CustomTextBoxStampUrl.ShortcutsEnabled = true;
            this.CustomTextBoxStampUrl.Size = new System.Drawing.Size(500, 23);
            this.CustomTextBoxStampUrl.TabIndex = 0;
            this.CustomTextBoxStampUrl.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxStampUrl.Texts = "";
            this.CustomTextBoxStampUrl.UnderlinedStyle = true;
            this.CustomTextBoxStampUrl.UsePasswordChar = false;
            this.CustomTextBoxStampUrl.WordWrap = true;
            // 
            // CustomButtonDecode
            // 
            this.CustomButtonDecode.AutoSize = true;
            this.CustomButtonDecode.BorderColor = System.Drawing.Color.Blue;
            this.CustomButtonDecode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CustomButtonDecode.Location = new System.Drawing.Point(610, 10);
            this.CustomButtonDecode.Name = "CustomButtonDecode";
            this.CustomButtonDecode.RoundedCorners = 5;
            this.CustomButtonDecode.SelectionColor = System.Drawing.Color.LightBlue;
            this.CustomButtonDecode.Size = new System.Drawing.Size(75, 27);
            this.CustomButtonDecode.TabIndex = 2;
            this.CustomButtonDecode.Text = "Decode";
            this.CustomButtonDecode.UseVisualStyleBackColor = true;
            this.CustomButtonDecode.Click += new System.EventHandler(this.CustomButtonDecode_Click);
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
            this.CustomTextBoxResult.Location = new System.Drawing.Point(12, 43);
            this.CustomTextBoxResult.MaxLength = 32767;
            this.CustomTextBoxResult.MinimumSize = new System.Drawing.Size(0, 23);
            this.CustomTextBoxResult.Multiline = true;
            this.CustomTextBoxResult.Name = "CustomTextBoxResult";
            this.CustomTextBoxResult.ReadOnly = true;
            this.CustomTextBoxResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CustomTextBoxResult.ShortcutsEnabled = true;
            this.CustomTextBoxResult.Size = new System.Drawing.Size(673, 356);
            this.CustomTextBoxResult.TabIndex = 0;
            this.CustomTextBoxResult.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.CustomTextBoxResult.Texts = "";
            this.CustomTextBoxResult.UnderlinedStyle = false;
            this.CustomTextBoxResult.UsePasswordChar = false;
            this.CustomTextBoxResult.WordWrap = true;
            // 
            // FormStampReader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(699, 411);
            this.Controls.Add(this.CustomTextBoxResult);
            this.Controls.Add(this.CustomButtonDecode);
            this.Controls.Add(this.CustomTextBoxStampUrl);
            this.Controls.Add(this.CustomLabelStampUrl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(715, 450);
            this.MinimumSize = new System.Drawing.Size(715, 450);
            this.Name = "FormStampReader";
            this.Text = "DNSCrypt Stamp Reader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CustomControls.CustomLabel CustomLabelStampUrl;
        private CustomControls.CustomTextBox CustomTextBoxStampUrl;
        private CustomControls.CustomButton CustomButtonDecode;
        private CustomControls.CustomTextBox CustomTextBoxResult;
    }
}