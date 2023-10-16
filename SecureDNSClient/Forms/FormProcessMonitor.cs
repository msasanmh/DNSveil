using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass.Themes;
using System.Net;

namespace SecureDNSClient;

public partial class FormProcessMonitor : Form
{
    private readonly string NL = Environment.NewLine;
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

        Shown -= FormProcessMonitor_Shown;
        Shown += FormProcessMonitor_Shown;
    }

    private void FormProcessMonitor_Shown(object? sender, EventArgs e)
    {
        CustomRichTextBox logUP = CustomRichTextBoxUp;
        CustomRichTextBox logDown = CustomRichTextBoxDown;

        ProcessMonitor.ProcessStatistics ps = new();

        Task.Run(async () =>
        {
            while (!Exit)
            {
                this.InvokeIt(() => logUP.ResetText());

                // Update PIDS
                //FormMain.MonitorProcess.SetPID(FormMain.GetPids(false));
                FormMain.MonitorProcess.SetPID(); // Measure Whole System
                ps = FormMain.MonitorProcess.GetProcessStatistics();

                string m01 = $"{NL} Data Sent: ";
                string m02 = $"{ConvertTool.ConvertByteToHumanRead(ps.BytesSent)}{NL}";

                string m03 = $" Data Received: ";
                string m04 = $"{ConvertTool.ConvertByteToHumanRead(ps.BytesReceived)}{NL}";

                string m05 = $"{NL} Upload Speed: ";
                string m06 = $"{ConvertTool.ConvertByteToHumanRead(ps.UploadSpeed)}/Sec{NL}";

                string m07 = $" Download Speed: ";
                string m08 = $"{ConvertTool.ConvertByteToHumanRead(ps.DownloadSpeed)}/Sec{NL}";

                this.InvokeIt(() => logUP.AppendText(m01, Color.LightGray));
                this.InvokeIt(() => logUP.AppendText(m02, Color.DodgerBlue));

                this.InvokeIt(() => logUP.AppendText(m03, Color.LightGray));
                this.InvokeIt(() => logUP.AppendText(m04, Color.DodgerBlue));

                this.InvokeIt(() => logUP.AppendText(m05, Color.LightGray));
                this.InvokeIt(() => logUP.AppendText(m06, Color.DodgerBlue));

                this.InvokeIt(() => logUP.AppendText(m07, Color.LightGray));
                this.InvokeIt(() => logUP.AppendText(m08, Color.DodgerBlue));

                await Task.Delay(1000);
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

    private void FormProcessMonitor_FormClosing(object sender, FormClosingEventArgs e)
    {
        Exit = true;
    }
}