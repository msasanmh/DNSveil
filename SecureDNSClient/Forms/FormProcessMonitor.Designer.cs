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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProcessMonitor));
            SplitContainerMain = new SplitContainer();
            CustomDataGridViewStatusTop = new CustomControls.CustomDataGridView();
            ColumnStatusName = new DataGridViewTextBoxColumn();
            ColumnStatusText = new DataGridViewTextBoxColumn();
            CustomRichTextBoxDown = new CustomControls.CustomRichTextBox();
            ((System.ComponentModel.ISupportInitialize)SplitContainerMain).BeginInit();
            SplitContainerMain.Panel1.SuspendLayout();
            SplitContainerMain.Panel2.SuspendLayout();
            SplitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CustomDataGridViewStatusTop).BeginInit();
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
            SplitContainerMain.Panel1.Controls.Add(CustomDataGridViewStatusTop);
            SplitContainerMain.Panel1MinSize = 120;
            // 
            // SplitContainerMain.Panel2
            // 
            SplitContainerMain.Panel2.Controls.Add(CustomRichTextBoxDown);
            SplitContainerMain.Panel2MinSize = 100;
            SplitContainerMain.Size = new Size(284, 411);
            SplitContainerMain.SplitterDistance = 170;
            SplitContainerMain.TabIndex = 1;
            // 
            // CustomDataGridViewStatusTop
            // 
            CustomDataGridViewStatusTop.AllowUserToAddRows = false;
            CustomDataGridViewStatusTop.AllowUserToDeleteRows = false;
            CustomDataGridViewStatusTop.AllowUserToResizeColumns = false;
            CustomDataGridViewStatusTop.AllowUserToResizeRows = false;
            CustomDataGridViewStatusTop.BorderColor = Color.Blue;
            CustomDataGridViewStatusTop.CheckColor = Color.Blue;
            CustomDataGridViewStatusTop.ColumnHeadersBorder = true;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(73, 73, 73);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(73, 73, 73);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            CustomDataGridViewStatusTop.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            CustomDataGridViewStatusTop.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            CustomDataGridViewStatusTop.ColumnHeadersVisible = false;
            CustomDataGridViewStatusTop.Columns.AddRange(new DataGridViewColumn[] { ColumnStatusName, ColumnStatusText });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.DimGray;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(97, 177, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            CustomDataGridViewStatusTop.DefaultCellStyle = dataGridViewCellStyle2;
            CustomDataGridViewStatusTop.Dock = DockStyle.Fill;
            CustomDataGridViewStatusTop.GridColor = Color.LightBlue;
            CustomDataGridViewStatusTop.Location = new Point(0, 0);
            CustomDataGridViewStatusTop.MultiSelect = false;
            CustomDataGridViewStatusTop.Name = "CustomDataGridViewStatusTop";
            CustomDataGridViewStatusTop.ReadOnly = true;
            CustomDataGridViewStatusTop.RowHeadersVisible = false;
            CustomDataGridViewStatusTop.RowTemplate.Height = 25;
            CustomDataGridViewStatusTop.ScrollBars = ScrollBars.None;
            CustomDataGridViewStatusTop.SelectionColor = Color.DodgerBlue;
            CustomDataGridViewStatusTop.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            CustomDataGridViewStatusTop.SelectionModeFocus = false;
            CustomDataGridViewStatusTop.ShowCellErrors = false;
            CustomDataGridViewStatusTop.ShowCellToolTips = false;
            CustomDataGridViewStatusTop.ShowEditingIcon = false;
            CustomDataGridViewStatusTop.ShowRowErrors = false;
            CustomDataGridViewStatusTop.Size = new Size(284, 170);
            CustomDataGridViewStatusTop.TabIndex = 19;
            // 
            // ColumnStatusName
            // 
            ColumnStatusName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            ColumnStatusName.HeaderText = "Status Name";
            ColumnStatusName.Name = "ColumnStatusName";
            ColumnStatusName.ReadOnly = true;
            ColumnStatusName.Resizable = DataGridViewTriState.False;
            ColumnStatusName.SortMode = DataGridViewColumnSortMode.NotSortable;
            ColumnStatusName.Width = 5;
            // 
            // ColumnStatusText
            // 
            ColumnStatusText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ColumnStatusText.HeaderText = "Status Text";
            ColumnStatusText.Name = "ColumnStatusText";
            ColumnStatusText.ReadOnly = true;
            ColumnStatusText.Resizable = DataGridViewTriState.False;
            ColumnStatusText.SortMode = DataGridViewColumnSortMode.NotSortable;
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
            CustomRichTextBoxDown.Font = new Font("Segoe UI", 9F);
            CustomRichTextBoxDown.ForeColor = Color.White;
            CustomRichTextBoxDown.HideSelection = false;
            CustomRichTextBoxDown.Location = new Point(0, 0);
            CustomRichTextBoxDown.MaxLength = int.MaxValue;
            CustomRichTextBoxDown.MinimumSize = new Size(0, 23);
            CustomRichTextBoxDown.Multiline = true;
            CustomRichTextBoxDown.Name = "CustomRichTextBoxDown";
            CustomRichTextBoxDown.ReadOnly = true;
            CustomRichTextBoxDown.RightMargin = 0;
            CustomRichTextBoxDown.RoundedCorners = 0;
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
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.DimGray;
            ClientSize = new Size(284, 411);
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
            ((System.ComponentModel.ISupportInitialize)CustomDataGridViewStatusTop).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private SplitContainer SplitContainerMain;
        private CustomControls.CustomRichTextBox CustomRichTextBoxDown;
        private CustomControls.CustomDataGridView CustomDataGridViewStatusTop;
        private DataGridViewTextBoxColumn ColumnStatusName;
        private DataGridViewTextBoxColumn ColumnStatusText;
    }
}