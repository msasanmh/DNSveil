namespace SecureDNSClient
{
    partial class FormBenchmark
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBenchmark));
            CustomLabelBenchmarkInfo = new CustomControls.CustomLabel();
            CustomLabelNoSDC = new CustomControls.CustomLabel();
            CustomLabelSDC = new CustomControls.CustomLabel();
            CustomLabelNoSdcLatencyUdp = new CustomControls.CustomLabel();
            CustomLabelSdcLatencyUdp = new CustomControls.CustomLabel();
            CustomLabelBoostUdp = new CustomControls.CustomLabel();
            CustomLabelNoSdcLatencyTcp = new CustomControls.CustomLabel();
            CustomLabelSdcLatencyTcp = new CustomControls.CustomLabel();
            CustomLabelBoostTcp = new CustomControls.CustomLabel();
            SuspendLayout();
            // 
            // CustomLabelBenchmarkInfo
            // 
            CustomLabelBenchmarkInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            CustomLabelBenchmarkInfo.AutoSize = true;
            CustomLabelBenchmarkInfo.BackColor = Color.DimGray;
            CustomLabelBenchmarkInfo.Border = false;
            CustomLabelBenchmarkInfo.BorderColor = Color.Blue;
            CustomLabelBenchmarkInfo.FlatStyle = FlatStyle.Flat;
            CustomLabelBenchmarkInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            CustomLabelBenchmarkInfo.ForeColor = Color.White;
            CustomLabelBenchmarkInfo.Location = new Point(214, 21);
            CustomLabelBenchmarkInfo.Name = "CustomLabelBenchmarkInfo";
            CustomLabelBenchmarkInfo.RoundedCorners = 0;
            CustomLabelBenchmarkInfo.Size = new Size(165, 17);
            CustomLabelBenchmarkInfo.TabIndex = 0;
            CustomLabelBenchmarkInfo.Text = "Bootstrap DNS Vs. SDC DNS";
            // 
            // CustomLabelNoSDC
            // 
            CustomLabelNoSDC.AutoSize = true;
            CustomLabelNoSDC.BackColor = Color.DimGray;
            CustomLabelNoSDC.Border = false;
            CustomLabelNoSDC.BorderColor = Color.Blue;
            CustomLabelNoSDC.FlatStyle = FlatStyle.Flat;
            CustomLabelNoSDC.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            CustomLabelNoSDC.ForeColor = Color.White;
            CustomLabelNoSDC.Location = new Point(31, 79);
            CustomLabelNoSDC.Name = "CustomLabelNoSDC";
            CustomLabelNoSDC.RoundedCorners = 0;
            CustomLabelNoSDC.Size = new Size(219, 17);
            CustomLabelNoSDC.TabIndex = 1;
            CustomLabelNoSDC.Text = "DNS Latency Without SDC (Unsecure)";
            // 
            // CustomLabelSDC
            // 
            CustomLabelSDC.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CustomLabelSDC.AutoSize = true;
            CustomLabelSDC.BackColor = Color.DimGray;
            CustomLabelSDC.Border = false;
            CustomLabelSDC.BorderColor = Color.Blue;
            CustomLabelSDC.FlatStyle = FlatStyle.Flat;
            CustomLabelSDC.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            CustomLabelSDC.ForeColor = Color.White;
            CustomLabelSDC.Location = new Point(479, 79);
            CustomLabelSDC.Name = "CustomLabelSDC";
            CustomLabelSDC.RoundedCorners = 0;
            CustomLabelSDC.Size = new Size(186, 17);
            CustomLabelSDC.TabIndex = 2;
            CustomLabelSDC.Text = "DNS Latency With SDC (Secure)";
            // 
            // CustomLabelNoSdcLatencyUdp
            // 
            CustomLabelNoSdcLatencyUdp.BackColor = Color.DimGray;
            CustomLabelNoSdcLatencyUdp.Border = false;
            CustomLabelNoSdcLatencyUdp.BorderColor = Color.Blue;
            CustomLabelNoSdcLatencyUdp.FlatStyle = FlatStyle.Flat;
            CustomLabelNoSdcLatencyUdp.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            CustomLabelNoSdcLatencyUdp.ForeColor = Color.White;
            CustomLabelNoSdcLatencyUdp.Location = new Point(31, 107);
            CustomLabelNoSdcLatencyUdp.Name = "CustomLabelNoSdcLatencyUdp";
            CustomLabelNoSdcLatencyUdp.RoundedCorners = 0;
            CustomLabelNoSdcLatencyUdp.Size = new Size(156, 17);
            CustomLabelNoSdcLatencyUdp.TabIndex = 3;
            CustomLabelNoSdcLatencyUdp.Text = "UDP: 5000 ms";
            CustomLabelNoSdcLatencyUdp.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CustomLabelSdcLatencyUdp
            // 
            CustomLabelSdcLatencyUdp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CustomLabelSdcLatencyUdp.BackColor = Color.DimGray;
            CustomLabelSdcLatencyUdp.Border = false;
            CustomLabelSdcLatencyUdp.BorderColor = Color.Blue;
            CustomLabelSdcLatencyUdp.FlatStyle = FlatStyle.Flat;
            CustomLabelSdcLatencyUdp.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            CustomLabelSdcLatencyUdp.ForeColor = Color.White;
            CustomLabelSdcLatencyUdp.Location = new Point(501, 107);
            CustomLabelSdcLatencyUdp.Name = "CustomLabelSdcLatencyUdp";
            CustomLabelSdcLatencyUdp.RoundedCorners = 0;
            CustomLabelSdcLatencyUdp.Size = new Size(136, 17);
            CustomLabelSdcLatencyUdp.TabIndex = 4;
            CustomLabelSdcLatencyUdp.Text = "UDP: 5000 ms";
            CustomLabelSdcLatencyUdp.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CustomLabelBoostUdp
            // 
            CustomLabelBoostUdp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            CustomLabelBoostUdp.BackColor = Color.DimGray;
            CustomLabelBoostUdp.Border = false;
            CustomLabelBoostUdp.BorderColor = Color.Blue;
            CustomLabelBoostUdp.FlatStyle = FlatStyle.Flat;
            CustomLabelBoostUdp.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            CustomLabelBoostUdp.ForeColor = Color.White;
            CustomLabelBoostUdp.Location = new Point(123, 189);
            CustomLabelBoostUdp.Name = "CustomLabelBoostUdp";
            CustomLabelBoostUdp.RoundedCorners = 0;
            CustomLabelBoostUdp.Size = new Size(421, 17);
            CustomLabelBoostUdp.TabIndex = 5;
            CustomLabelBoostUdp.Text = "UDP Boost: 100%";
            CustomLabelBoostUdp.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CustomLabelNoSdcLatencyTcp
            // 
            CustomLabelNoSdcLatencyTcp.BackColor = Color.DimGray;
            CustomLabelNoSdcLatencyTcp.Border = false;
            CustomLabelNoSdcLatencyTcp.BorderColor = Color.Blue;
            CustomLabelNoSdcLatencyTcp.FlatStyle = FlatStyle.Flat;
            CustomLabelNoSdcLatencyTcp.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            CustomLabelNoSdcLatencyTcp.ForeColor = Color.White;
            CustomLabelNoSdcLatencyTcp.Location = new Point(31, 124);
            CustomLabelNoSdcLatencyTcp.Name = "CustomLabelNoSdcLatencyTcp";
            CustomLabelNoSdcLatencyTcp.RoundedCorners = 0;
            CustomLabelNoSdcLatencyTcp.Size = new Size(156, 17);
            CustomLabelNoSdcLatencyTcp.TabIndex = 6;
            CustomLabelNoSdcLatencyTcp.Text = "TCP: 5000 ms";
            CustomLabelNoSdcLatencyTcp.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CustomLabelSdcLatencyTcp
            // 
            CustomLabelSdcLatencyTcp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CustomLabelSdcLatencyTcp.BackColor = Color.DimGray;
            CustomLabelSdcLatencyTcp.Border = false;
            CustomLabelSdcLatencyTcp.BorderColor = Color.Blue;
            CustomLabelSdcLatencyTcp.FlatStyle = FlatStyle.Flat;
            CustomLabelSdcLatencyTcp.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            CustomLabelSdcLatencyTcp.ForeColor = Color.White;
            CustomLabelSdcLatencyTcp.Location = new Point(501, 124);
            CustomLabelSdcLatencyTcp.Name = "CustomLabelSdcLatencyTcp";
            CustomLabelSdcLatencyTcp.RoundedCorners = 0;
            CustomLabelSdcLatencyTcp.Size = new Size(136, 17);
            CustomLabelSdcLatencyTcp.TabIndex = 7;
            CustomLabelSdcLatencyTcp.Text = "TCP: 5000 ms";
            CustomLabelSdcLatencyTcp.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CustomLabelBoostTcp
            // 
            CustomLabelBoostTcp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            CustomLabelBoostTcp.BackColor = Color.DimGray;
            CustomLabelBoostTcp.Border = false;
            CustomLabelBoostTcp.BorderColor = Color.Blue;
            CustomLabelBoostTcp.FlatStyle = FlatStyle.Flat;
            CustomLabelBoostTcp.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            CustomLabelBoostTcp.ForeColor = Color.White;
            CustomLabelBoostTcp.Location = new Point(123, 217);
            CustomLabelBoostTcp.Name = "CustomLabelBoostTcp";
            CustomLabelBoostTcp.RoundedCorners = 0;
            CustomLabelBoostTcp.Size = new Size(421, 17);
            CustomLabelBoostTcp.TabIndex = 8;
            CustomLabelBoostTcp.Text = "TCP Boost: 100%";
            CustomLabelBoostTcp.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FormBenchmark
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.DimGray;
            ClientSize = new Size(684, 261);
            Controls.Add(CustomLabelBoostTcp);
            Controls.Add(CustomLabelSdcLatencyTcp);
            Controls.Add(CustomLabelNoSdcLatencyTcp);
            Controls.Add(CustomLabelBoostUdp);
            Controls.Add(CustomLabelSdcLatencyUdp);
            Controls.Add(CustomLabelNoSdcLatencyUdp);
            Controls.Add(CustomLabelSDC);
            Controls.Add(CustomLabelNoSDC);
            Controls.Add(CustomLabelBenchmarkInfo);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FormBenchmark";
            Text = "Benchmark";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CustomControls.CustomLabel CustomLabelBenchmarkInfo;
        private CustomControls.CustomLabel CustomLabelNoSDC;
        private CustomControls.CustomLabel CustomLabelSDC;
        private CustomControls.CustomLabel CustomLabelNoSdcLatencyUdp;
        private CustomControls.CustomLabel CustomLabelSdcLatencyUdp;
        private CustomControls.CustomLabel CustomLabelBoostUdp;
        private CustomControls.CustomLabel CustomLabelNoSdcLatencyTcp;
        private CustomControls.CustomLabel CustomLabelSdcLatencyTcp;
        private CustomControls.CustomLabel CustomLabelBoostTcp;
    }
}