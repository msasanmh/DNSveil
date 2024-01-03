using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass.Themes;
using System.Net;

namespace SecureDNSClient;

public partial class FormProcessMonitor : Form
{
    private readonly string NL = Environment.NewLine;
    private bool IsExiting { get; set; } = false;
    private bool IsExitDone { get; set; } = false;
    private bool Exit = false;
    public FormProcessMonitor()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);

        // Update PIDS
        //FormMain.MonitorProcess.SetPID(FormMain.GetPids(false));
        FormMain.MonitorProcess.SetPID(); // Measure Whole System

        // Initialize Status Top
        InitializeStatus(CustomDataGridViewStatusTop);

        Shown -= FormProcessMonitor_Shown;
        Shown += FormProcessMonitor_Shown;
    }

    private void FormProcessMonitor_Shown(object? sender, EventArgs e)
    {
        CustomRichTextBox logDown = CustomRichTextBoxDown;

        ProcessMonitor.ProcessStatistics ps = new();

        Task.Run(async () =>
        {
            while (!Exit)
            {
                // Update PIDS
                //FormMain.MonitorProcess.SetPID(FormMain.GetPids(false));
                FormMain.MonitorProcess.SetPID(); // Measure Whole System
                ps = FormMain.MonitorProcess.GetProcessStatistics();

                // Update Bar Color
                if (!FormMain.IsInActionState)
                    this.InvokeIt(() => SplitContainerMain.BackColor = FormMain.IsDNSConnected ? Color.MediumSeaGreen : Color.IndianRed);
                else
                    this.InvokeIt(() => SplitContainerMain.BackColor = Color.DodgerBlue);

                string dataSent = $"{ConvertTool.ConvertByteToHumanRead(ps.BytesSent)}";
                this.InvokeIt(() => CustomDataGridViewStatusTop.Rows[0].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewStatusTop.Rows[0].Cells[1].Value = dataSent);

                string dataReceived = $"{ConvertTool.ConvertByteToHumanRead(ps.BytesReceived)}";
                this.InvokeIt(() => CustomDataGridViewStatusTop.Rows[1].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewStatusTop.Rows[1].Cells[1].Value = dataReceived);

                string uploadSpeed = $"{ConvertTool.ConvertByteToHumanRead(ps.UploadSpeed)}/Sec";
                this.InvokeIt(() => CustomDataGridViewStatusTop.Rows[2].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewStatusTop.Rows[2].Cells[1].Value = uploadSpeed);

                string downloadSpeed = $"{ConvertTool.ConvertByteToHumanRead(ps.DownloadSpeed)}/Sec";
                this.InvokeIt(() => CustomDataGridViewStatusTop.Rows[3].Cells[1].Style.ForeColor = Color.DodgerBlue);
                this.InvokeIt(() => CustomDataGridViewStatusTop.Rows[3].Cells[1].Value = downloadSpeed);

                await Task.Delay(300);
            }
        });

        Task.Run(async () =>
        {
            while (!Exit)
            {
                this.InvokeIt(() => logDown.ResetText());

                string m01 = $"{NL} Local DNS Address: ";
                string m02;
                if (FormMain.IsDNSConnected)
                    m02 = $"{NL} {IPAddress.Loopback}, {FormMain.LocalIP}{NL}";
                else m02 = $"Offline{NL}";

                string m03 = $"{NL} Local DoH Address: ";
                string m04;
                if (FormMain.IsDoHConnected)
                {
                    if (FormMain.ConnectedDohPort == 443)
                    {
                        m04 = $"{NL} https://{IPAddress.Loopback}/dns-query{NL}";
                        m04 += $" https://{FormMain.LocalIP}/dns-query{NL}";
                    }
                    else
                    {
                        m04 = $"{NL} https://{IPAddress.Loopback}:{FormMain.ConnectedDohPort}/dns-query{NL}";
                        m04 += $" https://{FormMain.LocalIP}:{FormMain.ConnectedDohPort}/dns-query{NL}";
                    }
                }
                else m04 = $"Offline{NL}";

                string m05 = $"{NL} Local Proxy Address: ";
                string m06;
                if (FormMain.IsProxyRunning)
                {
                    m06 = $"{NL} {IPAddress.Loopback}:{FormMain.ProxyPort}{NL}";
                    m06 += $" {FormMain.LocalIP}:{FormMain.ProxyPort}{NL}";
                }
                else m06 = $"Offline{NL}";

                this.InvokeIt(() => logDown.AppendText(m01, Color.LightGray));
                this.InvokeIt(() => logDown.AppendText(m02, Color.DodgerBlue));

                this.InvokeIt(() => logDown.AppendText(m03, Color.LightGray));
                this.InvokeIt(() => logDown.AppendText(m04, Color.DodgerBlue));

                this.InvokeIt(() => logDown.AppendText(m05, Color.LightGray));
                this.InvokeIt(() => logDown.AppendText(m06, Color.DodgerBlue));

                // Show Connected Devices
                bool showConnectedDevices = false;
                List<ProcessMonitor.ConnectedDevices> cdl = ps.ConnectedDevices;
                if (cdl.Any() && showConnectedDevices)
                {
                    string cd0 = $"{NL} Connected Devices:{NL}";
                    this.InvokeIt(() => logDown.AppendText(cd0, Color.LightGray));

                    for (int n = 0; n < cdl.Count; n++)
                    {
                        ProcessMonitor.ConnectedDevices cd = cdl[n];

                        string cd1 = $" {cd.DeviceIP}";
                        string cd2 = " Connected to ";
                        string cd3 = $"{cd.ProcessName}{NL}";

                        this.InvokeIt(() => logDown.AppendText(cd1, Color.DodgerBlue));
                        this.InvokeIt(() => logDown.AppendText(cd2, Color.LightGray));
                        this.InvokeIt(() => logDown.AppendText(cd3, Color.DodgerBlue));
                    }
                }

                await Task.Delay(4000);
            }
        });
    }

    private void InitializeStatus(CustomDataGridView dgv)
    {
        dgv.SelectionChanged += (s, e) => dgv.ClearSelection();
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
        dgv.BorderStyle = BorderStyle.FixedSingle;
        List<DataGridViewRow> rList = new();
        for (int n = 0; n < 4; n++)
        {
            DataGridViewRow row = new();
            row.CreateCells(dgv, "cell0", "cell1");
            row.Height = TextRenderer.MeasureText("It doesn't matter what we write here!", dgv.Font).Height + 7;

            string cellName = n switch
            {
                0 => "Data Sent",
                1 => "Data Received",
                2 => "Upload Speed",
                3 => "Download Speed",
                _ => string.Empty
            };

            if (n % 2 == 0)
                row.DefaultCellStyle.BackColor = BackColor.ChangeBrightness(-0.2f);
            else
                row.DefaultCellStyle.BackColor = BackColor;

            row.Cells[0].Value = cellName;
            row.Cells[1].Value = string.Empty;
            rList.Add(row);
        }

        dgv.Rows.AddRange(rList.ToArray());
    }

    private async void FormProcessMonitor_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!IsExiting)
        {
            e.Cancel = true;
            IsExiting = true;

            Exit = true;
            await Task.Delay(200);
            IsExitDone = true;

            e.Cancel = false;
            Close();
        }
        else
        {
            if (!IsExitDone)
            {
                e.Cancel = true;
                return;
            }

            Dispose();
        }
    }
}