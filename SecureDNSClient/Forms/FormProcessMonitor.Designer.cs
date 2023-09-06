namespace SecureDNSClient
{
    partial class FormProcessMonitor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProcessMonitor));
            CustomRichTextBoxUp = new CustomControls.CustomRichTextBox();
            SplitContainerMain = new SplitContainer();
            CustomRichTextBoxDown = new CustomControls.CustomRichTextBox();
            ((System.ComponentModel.ISupportInitialize)SplitContainerMain).BeginInit();
            SplitContainerMain.Panel1.SuspendLayout();
            SplitContainerMain.Panel2.SuspendLayout();
            SplitContainerMain.SuspendLayout();
            SuspendLayout();
            // 
            // CustomRichTextBoxUp
            // 
            CustomRichTextBoxUp.AcceptsTab = false;
            CustomRichTextBoxUp.AutoWordSelection = false;
            CustomRichTextBoxUp.BackColor = Color.DimGray;
            CustomRichTextBoxUp.Border = true;
            CustomRichTextBoxUp.BorderColor = Color.Blue;
            CustomRichTextBoxUp.BorderSize = 1;
            CustomRichTextBoxUp.BulletIndent = 0;
            CustomRichTextBoxUp.DetectUrls = false;
            CustomRichTextBoxUp.Dock = DockStyle.Fill;
            CustomRichTextBoxUp.EnableAutoDragDrop = false;
            CustomRichTextBoxUp.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomRichTextBoxUp.ForeColor = Color.White;
            CustomRichTextBoxUp.HideSelection = true;
            CustomRichTextBoxUp.Location = new Point(0, 0);
            CustomRichTextBoxUp.MaxLength = int.MaxValue;
            CustomRichTextBoxUp.MinimumSize = new Size(0, 23);
            CustomRichTextBoxUp.Multiline = true;
            CustomRichTextBoxUp.Name = "CustomRichTextBoxUp";
            CustomRichTextBoxUp.ReadOnly = true;
            CustomRichTextBoxUp.RightMargin = 0;
            CustomRichTextBoxUp.ScrollBars = ScrollBars.Vertical;
            CustomRichTextBoxUp.ScrollToBottom = false;
            CustomRichTextBoxUp.SelectionColor = Color.White;
            CustomRichTextBoxUp.SelectionLength = 0;
            CustomRichTextBoxUp.SelectionStart = 0;
            CustomRichTextBoxUp.ShortcutsEnabled = true;
            CustomRichTextBoxUp.Size = new Size(284, 120);
            CustomRichTextBoxUp.TabIndex = 0;
            CustomRichTextBoxUp.Texts = "";
            CustomRichTextBoxUp.UnderlinedStyle = false;
            CustomRichTextBoxUp.WordWrap = true;
            CustomRichTextBoxUp.ZoomFactor = 1F;
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
            SplitContainerMain.Panel1.Controls.Add(CustomRichTextBoxUp);
            SplitContainerMain.Panel1MinSize = 120;
            // 
            // SplitContainerMain.Panel2
            // 
            SplitContainerMain.Panel2.Controls.Add(CustomRichTextBoxDown);
            SplitContainerMain.Panel2MinSize = 100;
            SplitContainerMain.Size = new Size(284, 361);
            SplitContainerMain.SplitterDistance = 120;
            SplitContainerMain.TabIndex = 1;
            // 
            // CustomRichTextBoxDown
            // 
            CustomRichTextBoxDown.AcceptsTab = false;
            CustomRichTextBoxDown.AutoWordSelection = false;
            CustomRichTextBoxDown.BackColor = Color.DimGray;
            CustomRichTextBoxDown.Border = true;
            CustomRichTextBoxDown.BorderColor = Color.Blue;
            CustomRichTextBoxDown.BorderSize = 1;
            CustomRichTextBoxDown.BulletIndent = 0;
            CustomRichTextBoxDown.DetectUrls = false;
            CustomRichTextBoxDown.Dock = DockStyle.Fill;
            CustomRichTextBoxDown.EnableAutoDragDrop = false;
            CustomRichTextBoxDown.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomRichTextBoxDown.ForeColor = Color.White;
            CustomRichTextBoxDown.HideSelection = false;
            CustomRichTextBoxDown.Location = new Point(0, 0);
            CustomRichTextBoxDown.MaxLength = int.MaxValue;
            CustomRichTextBoxDown.MinimumSize = new Size(0, 23);
            CustomRichTextBoxDown.Multiline = true;
            CustomRichTextBoxDown.Name = "CustomRichTextBoxDown";
            CustomRichTextBoxDown.ReadOnly = true;
            CustomRichTextBoxDown.RightMargin = 0;
            CustomRichTextBoxDown.ScrollBars = ScrollBars.Vertical;
            CustomRichTextBoxDown.ScrollToBottom = false;
            CustomRichTextBoxDown.SelectionColor = Color.White;
            CustomRichTextBoxDown.SelectionLength = 0;
            CustomRichTextBoxDown.SelectionStart = 0;
            CustomRichTextBoxDown.ShortcutsEnabled = true;
            CustomRichTextBoxDown.Size = new Size(284, 237);
            CustomRichTextBoxDown.TabIndex = 0;
            CustomRichTextBoxDown.Texts = "";
            CustomRichTextBoxDown.UnderlinedStyle = false;
            CustomRichTextBoxDown.WordWrap = true;
            CustomRichTextBoxDown.ZoomFactor = 1F;
            // 
            // FormProcessMonitor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(284, 361);
            Controls.Add(SplitContainerMain);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormProcessMonitor";
            Text = "Process Monitor";
            FormClosing += FormProcessMonitor_FormClosing;
            SplitContainerMain.Panel1.ResumeLayout(false);
            SplitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SplitContainerMain).EndInit();
            SplitContainerMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private CustomControls.CustomRichTextBox CustomRichTextBoxUp;
        private SplitContainer SplitContainerMain;
        private CustomControls.CustomRichTextBox CustomRichTextBoxDown;
    }
}