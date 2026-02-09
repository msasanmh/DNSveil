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
            CustomLabelStampUrl = new CustomControls.CustomLabel();
            CustomTextBoxStampUrl = new CustomControls.CustomTextBox();
            CustomButtonDecode = new CustomControls.CustomButton();
            CustomTextBoxResult = new CustomControls.CustomTextBox();
            SuspendLayout();
            // 
            // CustomLabelStampUrl
            // 
            CustomLabelStampUrl.AutoSize = true;
            CustomLabelStampUrl.BackColor = Color.DimGray;
            CustomLabelStampUrl.Border = false;
            CustomLabelStampUrl.BorderColor = Color.Blue;
            CustomLabelStampUrl.FlatStyle = FlatStyle.Flat;
            CustomLabelStampUrl.ForeColor = Color.White;
            CustomLabelStampUrl.Location = new Point(12, 10);
            CustomLabelStampUrl.Name = "CustomLabelStampUrl";
            CustomLabelStampUrl.RoundedCorners = 0;
            CustomLabelStampUrl.Size = new Size(64, 17);
            CustomLabelStampUrl.TabIndex = 0;
            CustomLabelStampUrl.Text = "Stamp Url:";
            // 
            // CustomTextBoxStampUrl
            // 
            CustomTextBoxStampUrl.AcceptsReturn = false;
            CustomTextBoxStampUrl.AcceptsTab = false;
            CustomTextBoxStampUrl.BackColor = Color.DimGray;
            CustomTextBoxStampUrl.Border = true;
            CustomTextBoxStampUrl.BorderColor = Color.Blue;
            CustomTextBoxStampUrl.BorderSize = 1;
            CustomTextBoxStampUrl.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxStampUrl.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxStampUrl.ForeColor = Color.White;
            CustomTextBoxStampUrl.HideSelection = true;
            CustomTextBoxStampUrl.Location = new Point(82, 8);
            CustomTextBoxStampUrl.MaxLength = 32767;
            CustomTextBoxStampUrl.Multiline = false;
            CustomTextBoxStampUrl.Name = "CustomTextBoxStampUrl";
            CustomTextBoxStampUrl.ReadOnly = false;
            CustomTextBoxStampUrl.RoundedCorners = 0;
            CustomTextBoxStampUrl.ScrollBars = ScrollBars.None;
            CustomTextBoxStampUrl.ShortcutsEnabled = true;
            CustomTextBoxStampUrl.Size = new Size(500, 23);
            CustomTextBoxStampUrl.TabIndex = 0;
            CustomTextBoxStampUrl.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxStampUrl.Texts = "";
            CustomTextBoxStampUrl.UnderlinedStyle = true;
            CustomTextBoxStampUrl.UsePasswordChar = false;
            CustomTextBoxStampUrl.WordWrap = true;
            // 
            // CustomButtonDecode
            // 
            CustomButtonDecode.AutoSize = true;
            CustomButtonDecode.BorderColor = Color.Blue;
            CustomButtonDecode.FlatStyle = FlatStyle.Flat;
            CustomButtonDecode.Location = new Point(610, 10);
            CustomButtonDecode.Name = "CustomButtonDecode";
            CustomButtonDecode.RoundedCorners = 5;
            CustomButtonDecode.SelectionColor = Color.LightBlue;
            CustomButtonDecode.Size = new Size(75, 27);
            CustomButtonDecode.TabIndex = 2;
            CustomButtonDecode.Text = "Decode";
            CustomButtonDecode.UseVisualStyleBackColor = true;
            CustomButtonDecode.Click += CustomButtonDecode_Click;
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
            CustomTextBoxResult.Location = new Point(12, 43);
            CustomTextBoxResult.MaxLength = 32767;
            CustomTextBoxResult.MinimumSize = new Size(0, 23);
            CustomTextBoxResult.Multiline = true;
            CustomTextBoxResult.Name = "CustomTextBoxResult";
            CustomTextBoxResult.ReadOnly = true;
            CustomTextBoxResult.RoundedCorners = 5;
            CustomTextBoxResult.ScrollBars = ScrollBars.Vertical;
            CustomTextBoxResult.ShortcutsEnabled = true;
            CustomTextBoxResult.Size = new Size(673, 356);
            CustomTextBoxResult.TabIndex = 0;
            CustomTextBoxResult.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxResult.Texts = "";
            CustomTextBoxResult.UnderlinedStyle = false;
            CustomTextBoxResult.UsePasswordChar = false;
            CustomTextBoxResult.WordWrap = true;
            // 
            // FormStampReader
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.DimGray;
            ClientSize = new Size(699, 411);
            Controls.Add(CustomTextBoxResult);
            Controls.Add(CustomButtonDecode);
            Controls.Add(CustomTextBoxStampUrl);
            Controls.Add(CustomLabelStampUrl);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MaximumSize = new Size(715, 450);
            MinimumSize = new Size(715, 450);
            Name = "FormStampReader";
            Text = "DNSCrypt Stamp Reader";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CustomControls.CustomLabel CustomLabelStampUrl;
        private CustomControls.CustomTextBox CustomTextBoxStampUrl;
        private CustomControls.CustomButton CustomButtonDecode;
        private CustomControls.CustomTextBox CustomTextBoxResult;
    }
}