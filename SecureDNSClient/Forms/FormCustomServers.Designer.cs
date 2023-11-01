namespace SecureDNSClient
{
    partial class FormCustomServers
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
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCustomServers));
            SplitContainerMain = new SplitContainer();
            CustomGroupBoxGroups = new CustomControls.CustomGroupBox();
            CustomButtonExport = new CustomControls.CustomButton();
            CustomButtonImport = new CustomControls.CustomButton();
            CustomButtonNewGroup = new CustomControls.CustomButton();
            CustomDataGridViewGroups = new CustomControls.CustomDataGridView();
            dataGridViewCheckBoxColumn1 = new DataGridViewCheckBoxColumn();
            ColumnName = new DataGridViewTextBoxColumn();
            CustomGroupBoxDNSs = new CustomControls.CustomGroupBox();
            CustomButtonAddServers = new CustomControls.CustomButton();
            CustomButtonModifyDNS = new CustomControls.CustomButton();
            CustomTextBoxDescription = new CustomControls.CustomTextBox();
            CustomTextBoxDNS = new CustomControls.CustomTextBox();
            CustomLabelDescription = new CustomControls.CustomLabel();
            CustomLabelDNS = new CustomControls.CustomLabel();
            CustomDataGridViewDNSs = new CustomControls.CustomDataGridView();
            ColumnEnabled = new DataGridViewCheckBoxColumn();
            ColumnDNS = new DataGridViewTextBoxColumn();
            ColumnDescription = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)SplitContainerMain).BeginInit();
            SplitContainerMain.Panel1.SuspendLayout();
            SplitContainerMain.Panel2.SuspendLayout();
            SplitContainerMain.SuspendLayout();
            CustomGroupBoxGroups.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CustomDataGridViewGroups).BeginInit();
            CustomGroupBoxDNSs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CustomDataGridViewDNSs).BeginInit();
            SuspendLayout();
            // 
            // SplitContainerMain
            // 
            SplitContainerMain.Cursor = Cursors.VSplit;
            SplitContainerMain.Dock = DockStyle.Fill;
            SplitContainerMain.Location = new Point(0, 0);
            SplitContainerMain.Name = "SplitContainerMain";
            // 
            // SplitContainerMain.Panel1
            // 
            SplitContainerMain.Panel1.Controls.Add(CustomGroupBoxGroups);
            SplitContainerMain.Panel1.Cursor = Cursors.Default;
            SplitContainerMain.Panel1MinSize = 240;
            // 
            // SplitContainerMain.Panel2
            // 
            SplitContainerMain.Panel2.Controls.Add(CustomGroupBoxDNSs);
            SplitContainerMain.Panel2.Cursor = Cursors.Default;
            SplitContainerMain.Panel2MinSize = 500;
            SplitContainerMain.Size = new Size(974, 461);
            SplitContainerMain.SplitterDistance = 240;
            SplitContainerMain.TabIndex = 0;
            // 
            // CustomGroupBoxGroups
            // 
            CustomGroupBoxGroups.BorderColor = Color.Blue;
            CustomGroupBoxGroups.Controls.Add(CustomButtonExport);
            CustomGroupBoxGroups.Controls.Add(CustomButtonImport);
            CustomGroupBoxGroups.Controls.Add(CustomButtonNewGroup);
            CustomGroupBoxGroups.Controls.Add(CustomDataGridViewGroups);
            CustomGroupBoxGroups.Dock = DockStyle.Fill;
            CustomGroupBoxGroups.Location = new Point(0, 0);
            CustomGroupBoxGroups.Name = "CustomGroupBoxGroups";
            CustomGroupBoxGroups.RoundedCorners = 5;
            CustomGroupBoxGroups.Size = new Size(240, 461);
            CustomGroupBoxGroups.TabIndex = 0;
            CustomGroupBoxGroups.TabStop = false;
            CustomGroupBoxGroups.Text = "Groups";
            // 
            // CustomButtonExport
            // 
            CustomButtonExport.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CustomButtonExport.BorderColor = Color.Blue;
            CustomButtonExport.FlatStyle = FlatStyle.Flat;
            CustomButtonExport.Location = new Point(170, 423);
            CustomButtonExport.Name = "CustomButtonExport";
            CustomButtonExport.RoundedCorners = 5;
            CustomButtonExport.SelectionColor = Color.LightBlue;
            CustomButtonExport.Size = new Size(64, 27);
            CustomButtonExport.TabIndex = 3;
            CustomButtonExport.Text = "Export";
            CustomButtonExport.UseVisualStyleBackColor = true;
            CustomButtonExport.Click += CustomButtonExport_Click;
            // 
            // CustomButtonImport
            // 
            CustomButtonImport.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CustomButtonImport.BorderColor = Color.Blue;
            CustomButtonImport.FlatStyle = FlatStyle.Flat;
            CustomButtonImport.Location = new Point(100, 423);
            CustomButtonImport.Name = "CustomButtonImport";
            CustomButtonImport.RoundedCorners = 5;
            CustomButtonImport.SelectionColor = Color.LightBlue;
            CustomButtonImport.Size = new Size(64, 27);
            CustomButtonImport.TabIndex = 2;
            CustomButtonImport.Text = "Import";
            CustomButtonImport.UseVisualStyleBackColor = true;
            CustomButtonImport.Click += CustomButtonImport_Click;
            // 
            // CustomButtonNewGroup
            // 
            CustomButtonNewGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CustomButtonNewGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CustomButtonNewGroup.BorderColor = Color.Blue;
            CustomButtonNewGroup.FlatStyle = FlatStyle.Flat;
            CustomButtonNewGroup.Location = new Point(6, 423);
            CustomButtonNewGroup.Name = "CustomButtonNewGroup";
            CustomButtonNewGroup.RoundedCorners = 5;
            CustomButtonNewGroup.SelectionColor = Color.LightBlue;
            CustomButtonNewGroup.Size = new Size(87, 27);
            CustomButtonNewGroup.TabIndex = 1;
            CustomButtonNewGroup.Text = "New group...";
            CustomButtonNewGroup.UseVisualStyleBackColor = true;
            CustomButtonNewGroup.Click += CustomButtonNewGroup_Click;
            // 
            // CustomDataGridViewGroups
            // 
            CustomDataGridViewGroups.AllowUserToAddRows = false;
            CustomDataGridViewGroups.AllowUserToDeleteRows = false;
            CustomDataGridViewGroups.AllowUserToResizeColumns = false;
            CustomDataGridViewGroups.AllowUserToResizeRows = false;
            CustomDataGridViewGroups.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CustomDataGridViewGroups.BorderColor = Color.Blue;
            CustomDataGridViewGroups.BorderStyle = BorderStyle.None;
            CustomDataGridViewGroups.CellBorderStyle = DataGridViewCellBorderStyle.None;
            CustomDataGridViewGroups.CheckColor = Color.Blue;
            CustomDataGridViewGroups.ColumnHeadersBorder = false;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(73, 73, 73);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(73, 73, 73);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            CustomDataGridViewGroups.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            CustomDataGridViewGroups.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            CustomDataGridViewGroups.ColumnHeadersVisible = false;
            CustomDataGridViewGroups.Columns.AddRange(new DataGridViewColumn[] { dataGridViewCheckBoxColumn1, ColumnName });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.DimGray;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(97, 177, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            CustomDataGridViewGroups.DefaultCellStyle = dataGridViewCellStyle2;
            CustomDataGridViewGroups.GridColor = Color.LightBlue;
            CustomDataGridViewGroups.Location = new Point(6, 22);
            CustomDataGridViewGroups.MultiSelect = false;
            CustomDataGridViewGroups.Name = "CustomDataGridViewGroups";
            CustomDataGridViewGroups.ReadOnly = true;
            CustomDataGridViewGroups.RowHeadersVisible = false;
            CustomDataGridViewGroups.RowTemplate.Height = 25;
            CustomDataGridViewGroups.SelectionColor = Color.DodgerBlue;
            CustomDataGridViewGroups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            CustomDataGridViewGroups.SelectionModeFocus = true;
            CustomDataGridViewGroups.ShowCellToolTips = false;
            CustomDataGridViewGroups.ShowEditingIcon = false;
            CustomDataGridViewGroups.Size = new Size(228, 395);
            CustomDataGridViewGroups.TabIndex = 0;
            CustomDataGridViewGroups.CellClick += CustomDataGridViewGroups_CellClick;
            CustomDataGridViewGroups.SelectionChanged += CustomDataGridViewGroups_SelectionChanged;
            CustomDataGridViewGroups.KeyDown += CustomDataGridViewGroups_KeyDown;
            CustomDataGridViewGroups.MouseDown += CustomDataGridViewGroups_MouseDown;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            dataGridViewCheckBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            dataGridViewCheckBoxColumn1.FalseValue = "false";
            dataGridViewCheckBoxColumn1.HeaderText = "Enabled";
            dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            dataGridViewCheckBoxColumn1.ReadOnly = true;
            dataGridViewCheckBoxColumn1.Resizable = DataGridViewTriState.False;
            dataGridViewCheckBoxColumn1.TrueValue = "true";
            dataGridViewCheckBoxColumn1.Width = 5;
            // 
            // ColumnName
            // 
            ColumnName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ColumnName.HeaderText = "Name";
            ColumnName.Name = "ColumnName";
            ColumnName.ReadOnly = true;
            ColumnName.Resizable = DataGridViewTriState.False;
            ColumnName.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // CustomGroupBoxDNSs
            // 
            CustomGroupBoxDNSs.BorderColor = Color.Blue;
            CustomGroupBoxDNSs.Controls.Add(CustomButtonAddServers);
            CustomGroupBoxDNSs.Controls.Add(CustomButtonModifyDNS);
            CustomGroupBoxDNSs.Controls.Add(CustomTextBoxDescription);
            CustomGroupBoxDNSs.Controls.Add(CustomTextBoxDNS);
            CustomGroupBoxDNSs.Controls.Add(CustomLabelDescription);
            CustomGroupBoxDNSs.Controls.Add(CustomLabelDNS);
            CustomGroupBoxDNSs.Controls.Add(CustomDataGridViewDNSs);
            CustomGroupBoxDNSs.Dock = DockStyle.Fill;
            CustomGroupBoxDNSs.Location = new Point(0, 0);
            CustomGroupBoxDNSs.Name = "CustomGroupBoxDNSs";
            CustomGroupBoxDNSs.RoundedCorners = 5;
            CustomGroupBoxDNSs.Size = new Size(730, 461);
            CustomGroupBoxDNSs.TabIndex = 0;
            CustomGroupBoxDNSs.TabStop = false;
            CustomGroupBoxDNSs.Text = "DNS Addresses";
            // 
            // CustomButtonAddServers
            // 
            CustomButtonAddServers.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CustomButtonAddServers.BorderColor = Color.Blue;
            CustomButtonAddServers.FlatStyle = FlatStyle.Flat;
            CustomButtonAddServers.Location = new Point(641, 390);
            CustomButtonAddServers.Name = "CustomButtonAddServers";
            CustomButtonAddServers.RoundedCorners = 5;
            CustomButtonAddServers.SelectionColor = Color.LightBlue;
            CustomButtonAddServers.Size = new Size(83, 27);
            CustomButtonAddServers.TabIndex = 7;
            CustomButtonAddServers.Text = "Add Servers";
            CustomButtonAddServers.UseVisualStyleBackColor = true;
            CustomButtonAddServers.Click += CustomButtonAddServers_Click;
            // 
            // CustomButtonModifyDNS
            // 
            CustomButtonModifyDNS.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CustomButtonModifyDNS.BorderColor = Color.Blue;
            CustomButtonModifyDNS.FlatStyle = FlatStyle.Flat;
            CustomButtonModifyDNS.Location = new Point(641, 423);
            CustomButtonModifyDNS.Name = "CustomButtonModifyDNS";
            CustomButtonModifyDNS.RoundedCorners = 5;
            CustomButtonModifyDNS.SelectionColor = Color.LightBlue;
            CustomButtonModifyDNS.Size = new Size(83, 27);
            CustomButtonModifyDNS.TabIndex = 6;
            CustomButtonModifyDNS.Text = "Modify DNS";
            CustomButtonModifyDNS.UseVisualStyleBackColor = true;
            CustomButtonModifyDNS.Click += CustomButtonModifyDNS_Click;
            // 
            // CustomTextBoxDescription
            // 
            CustomTextBoxDescription.AcceptsReturn = false;
            CustomTextBoxDescription.AcceptsTab = false;
            CustomTextBoxDescription.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CustomTextBoxDescription.BackColor = Color.DimGray;
            CustomTextBoxDescription.Border = true;
            CustomTextBoxDescription.BorderColor = Color.Blue;
            CustomTextBoxDescription.BorderSize = 1;
            CustomTextBoxDescription.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxDescription.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxDescription.ForeColor = Color.White;
            CustomTextBoxDescription.HideSelection = true;
            CustomTextBoxDescription.Location = new Point(92, 424);
            CustomTextBoxDescription.MaxLength = 32767;
            CustomTextBoxDescription.Multiline = false;
            CustomTextBoxDescription.Name = "CustomTextBoxDescription";
            CustomTextBoxDescription.ReadOnly = false;
            CustomTextBoxDescription.RoundedCorners = 0;
            CustomTextBoxDescription.ScrollBars = ScrollBars.None;
            CustomTextBoxDescription.ShortcutsEnabled = true;
            CustomTextBoxDescription.Size = new Size(543, 23);
            CustomTextBoxDescription.TabIndex = 0;
            CustomTextBoxDescription.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxDescription.Texts = "";
            CustomTextBoxDescription.UnderlinedStyle = true;
            CustomTextBoxDescription.UsePasswordChar = false;
            CustomTextBoxDescription.WordWrap = true;
            // 
            // CustomTextBoxDNS
            // 
            CustomTextBoxDNS.AcceptsReturn = false;
            CustomTextBoxDNS.AcceptsTab = false;
            CustomTextBoxDNS.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CustomTextBoxDNS.BackColor = Color.DimGray;
            CustomTextBoxDNS.Border = true;
            CustomTextBoxDNS.BorderColor = Color.Blue;
            CustomTextBoxDNS.BorderSize = 1;
            CustomTextBoxDNS.CharacterCasing = CharacterCasing.Normal;
            CustomTextBoxDNS.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            CustomTextBoxDNS.ForeColor = Color.White;
            CustomTextBoxDNS.HideSelection = true;
            CustomTextBoxDNS.Location = new Point(92, 394);
            CustomTextBoxDNS.MaxLength = 32767;
            CustomTextBoxDNS.Multiline = false;
            CustomTextBoxDNS.Name = "CustomTextBoxDNS";
            CustomTextBoxDNS.ReadOnly = false;
            CustomTextBoxDNS.RoundedCorners = 0;
            CustomTextBoxDNS.ScrollBars = ScrollBars.None;
            CustomTextBoxDNS.ShortcutsEnabled = true;
            CustomTextBoxDNS.Size = new Size(543, 23);
            CustomTextBoxDNS.TabIndex = 0;
            CustomTextBoxDNS.TextAlign = HorizontalAlignment.Left;
            CustomTextBoxDNS.Texts = "";
            CustomTextBoxDNS.UnderlinedStyle = true;
            CustomTextBoxDNS.UsePasswordChar = false;
            CustomTextBoxDNS.WordWrap = true;
            // 
            // CustomLabelDescription
            // 
            CustomLabelDescription.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CustomLabelDescription.AutoSize = true;
            CustomLabelDescription.BackColor = Color.DimGray;
            CustomLabelDescription.Border = false;
            CustomLabelDescription.BorderColor = Color.Blue;
            CustomLabelDescription.FlatStyle = FlatStyle.Flat;
            CustomLabelDescription.ForeColor = Color.White;
            CustomLabelDescription.Location = new Point(6, 426);
            CustomLabelDescription.Name = "CustomLabelDescription";
            CustomLabelDescription.RoundedCorners = 0;
            CustomLabelDescription.Size = new Size(72, 17);
            CustomLabelDescription.TabIndex = 2;
            CustomLabelDescription.Text = "Description:";
            // 
            // CustomLabelDNS
            // 
            CustomLabelDNS.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CustomLabelDNS.AutoSize = true;
            CustomLabelDNS.BackColor = Color.DimGray;
            CustomLabelDNS.Border = false;
            CustomLabelDNS.BorderColor = Color.Blue;
            CustomLabelDNS.FlatStyle = FlatStyle.Flat;
            CustomLabelDNS.ForeColor = Color.White;
            CustomLabelDNS.Location = new Point(6, 396);
            CustomLabelDNS.Name = "CustomLabelDNS";
            CustomLabelDNS.RoundedCorners = 0;
            CustomLabelDNS.Size = new Size(80, 17);
            CustomLabelDNS.TabIndex = 1;
            CustomLabelDNS.Text = "DNS Address:";
            // 
            // CustomDataGridViewDNSs
            // 
            CustomDataGridViewDNSs.AllowUserToAddRows = false;
            CustomDataGridViewDNSs.AllowUserToDeleteRows = false;
            CustomDataGridViewDNSs.AllowUserToResizeRows = false;
            CustomDataGridViewDNSs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CustomDataGridViewDNSs.BorderColor = Color.Blue;
            CustomDataGridViewDNSs.CheckColor = Color.Blue;
            CustomDataGridViewDNSs.ColumnHeadersBorder = true;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(73, 73, 73);
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = Color.White;
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(73, 73, 73);
            dataGridViewCellStyle3.SelectionForeColor = Color.White;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            CustomDataGridViewDNSs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            CustomDataGridViewDNSs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            CustomDataGridViewDNSs.Columns.AddRange(new DataGridViewColumn[] { ColumnEnabled, ColumnDNS, ColumnDescription });
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.DimGray;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle4.ForeColor = Color.White;
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(97, 177, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            CustomDataGridViewDNSs.DefaultCellStyle = dataGridViewCellStyle4;
            CustomDataGridViewDNSs.GridColor = Color.LightBlue;
            CustomDataGridViewDNSs.Location = new Point(6, 22);
            CustomDataGridViewDNSs.Name = "CustomDataGridViewDNSs";
            CustomDataGridViewDNSs.ReadOnly = true;
            CustomDataGridViewDNSs.RowHeadersVisible = false;
            CustomDataGridViewDNSs.RowTemplate.Height = 25;
            CustomDataGridViewDNSs.SelectionColor = Color.DodgerBlue;
            CustomDataGridViewDNSs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            CustomDataGridViewDNSs.SelectionModeFocus = true;
            CustomDataGridViewDNSs.ShowCellToolTips = false;
            CustomDataGridViewDNSs.ShowEditingIcon = false;
            CustomDataGridViewDNSs.Size = new Size(718, 362);
            CustomDataGridViewDNSs.TabIndex = 0;
            CustomDataGridViewDNSs.CellClick += CustomDataGridViewDNSs_CellClick;
            CustomDataGridViewDNSs.CellDoubleClick += CustomDataGridViewDNSs_CellDoubleClick;
            CustomDataGridViewDNSs.SelectionChanged += CustomDataGridViewDNSs_SelectionChanged;
            CustomDataGridViewDNSs.KeyDown += CustomDataGridViewDNSs_KeyDown;
            CustomDataGridViewDNSs.MouseDown += CustomDataGridViewDNSs_MouseDown;
            // 
            // ColumnEnabled
            // 
            ColumnEnabled.FalseValue = "false";
            ColumnEnabled.FlatStyle = FlatStyle.Flat;
            ColumnEnabled.HeaderText = "Enabled";
            ColumnEnabled.IndeterminateValue = "null";
            ColumnEnabled.Name = "ColumnEnabled";
            ColumnEnabled.ReadOnly = true;
            ColumnEnabled.Resizable = DataGridViewTriState.False;
            ColumnEnabled.TrueValue = "true";
            ColumnEnabled.Width = 52;
            // 
            // ColumnDNS
            // 
            ColumnDNS.HeaderText = "DNS Address";
            ColumnDNS.Name = "ColumnDNS";
            ColumnDNS.ReadOnly = true;
            ColumnDNS.SortMode = DataGridViewColumnSortMode.NotSortable;
            ColumnDNS.Width = 500;
            // 
            // ColumnDescription
            // 
            ColumnDescription.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ColumnDescription.HeaderText = "Description";
            ColumnDescription.Name = "ColumnDescription";
            ColumnDescription.ReadOnly = true;
            ColumnDescription.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // FormCustomServers
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.DimGray;
            ClientSize = new Size(974, 461);
            Controls.Add(SplitContainerMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(990, 495);
            Name = "FormCustomServers";
            Text = "Manage Custom Servers";
            FormClosing += FormCustomServers_FormClosing;
            SplitContainerMain.Panel1.ResumeLayout(false);
            SplitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SplitContainerMain).EndInit();
            SplitContainerMain.ResumeLayout(false);
            CustomGroupBoxGroups.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)CustomDataGridViewGroups).EndInit();
            CustomGroupBoxDNSs.ResumeLayout(false);
            CustomGroupBoxDNSs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)CustomDataGridViewDNSs).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer SplitContainerMain;
        private CustomControls.CustomGroupBox CustomGroupBoxGroups;
        private CustomControls.CustomGroupBox CustomGroupBoxDNSs;
        private CustomControls.CustomDataGridView CustomDataGridViewGroups;
        private CustomControls.CustomButton CustomButtonNewGroup;
        private CustomControls.CustomButton CustomButtonExport;
        private CustomControls.CustomButton CustomButtonImport;
        private CustomControls.CustomDataGridView CustomDataGridViewDNSs;
        private CustomControls.CustomLabel CustomLabelDescription;
        private CustomControls.CustomLabel CustomLabelDNS;
        private DataGridViewCheckBoxColumn ColumnEnabled;
        private DataGridViewTextBoxColumn ColumnDNS;
        private DataGridViewTextBoxColumn ColumnDescription;
        private CustomControls.CustomTextBox CustomTextBoxDescription;
        private CustomControls.CustomTextBox CustomTextBoxDNS;
        private CustomControls.CustomButton CustomButtonAddServers;
        private CustomControls.CustomButton CustomButtonModifyDNS;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private DataGridViewTextBoxColumn ColumnName;
    }
}