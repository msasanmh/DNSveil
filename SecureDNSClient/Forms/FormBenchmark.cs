using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using System.Globalization;
using System.Net;

namespace SecureDNSClient;

public partial class FormBenchmark : Form
{
    private readonly CheckDns ScanDns = new(false, false);
    private string BootstrapIp { get; set; }
    private int BootstrapPort { get; set; }
    private bool IsExiting { get; set; } = false;
    private bool IsExitDone { get; set; } = false;
    private bool Exit { get; set; } = false;

    public FormBenchmark(string bootstrapIp, int bootstrapPort)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        // Invariant Culture
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);

        BootstrapIp = bootstrapIp;
        BootstrapPort = bootstrapPort;

        FixScreenDpi();

        StartBenchmarkAuto();

        FormClosing -= FormBenchmark_FormClosing;
        FormClosing += FormBenchmark_FormClosing;
    }

    private async void FixScreenDpi()
    {
        // Setting Width Of Controls
        await ScreenDPI.SettingWidthOfControls(this);

        // Fix Controls Location
        int spaceBottom = 20, spaceRight = 20, spaceV = 10;

        CustomLabelBenchmarkInfo.Left = (ClientRectangle.Width - CustomLabelBenchmarkInfo.Width) / 2;
        CustomLabelBenchmarkInfo.Top = spaceBottom;

        CustomLabelNoSDC.Left = spaceRight;
        CustomLabelNoSDC.Top = (ClientRectangle.Height / 2) - (CustomLabelNoSDC.Height * 3);

        CustomLabelNoSdcLatencyUdp.Left = CustomLabelNoSDC.Left;
        CustomLabelNoSdcLatencyUdp.Top = CustomLabelNoSDC.Bottom + spaceV;
        CustomLabelNoSdcLatencyUdp.Width = CustomLabelNoSDC.Width;

        CustomLabelNoSdcLatencyTcp.Left = CustomLabelNoSdcLatencyUdp.Left;
        CustomLabelNoSdcLatencyTcp.Top = CustomLabelNoSdcLatencyUdp.Bottom + spaceV;
        CustomLabelNoSdcLatencyTcp.Width = CustomLabelNoSdcLatencyUdp.Width;

        CustomLabelSDC.Left = ClientRectangle.Width - CustomLabelSDC.Width - spaceRight;
        CustomLabelSDC.Top = CustomLabelNoSDC.Top;

        CustomLabelSdcLatencyUdp.Left = CustomLabelSDC.Left;
        CustomLabelSdcLatencyUdp.Top = CustomLabelNoSdcLatencyUdp.Top;
        CustomLabelSdcLatencyUdp.Width = CustomLabelSDC.Width;

        CustomLabelSdcLatencyTcp.Left = CustomLabelSdcLatencyUdp.Left;
        CustomLabelSdcLatencyTcp.Top = CustomLabelSdcLatencyUdp.Bottom + spaceV;
        CustomLabelSdcLatencyTcp.Width = CustomLabelSdcLatencyUdp.Width;

        CustomLabelBoostTcp.Width = ClientRectangle.Width * 2 / 3;
        CustomLabelBoostTcp.Left = (ClientRectangle.Width - CustomLabelBoostTcp.Width) / 2;
        CustomLabelBoostTcp.Top = ClientRectangle.Height - CustomLabelBoostTcp.Height - spaceBottom;

        CustomLabelBoostUdp.Width = CustomLabelBoostTcp.Width;
        CustomLabelBoostUdp.Left = CustomLabelBoostTcp.Left;
        CustomLabelBoostUdp.Top = CustomLabelBoostTcp.Top - CustomLabelBoostTcp.Height - spaceV;
    }

    private async void StartBenchmarkAuto()
    {
        this.InvokeIt(() =>
        {
            CustomLabelNoSdcLatencyUdp.Text = "UDP: -1 ms";
            CustomLabelSdcLatencyUdp.Text = "UDP: -1 ms";
            CustomLabelBoostUdp.Text = "UDP Boost: Calculating...";

            CustomLabelNoSdcLatencyTcp.Text = "TCP: -1 ms";
            CustomLabelSdcLatencyTcp.Text = "TCP: -1 ms";
            CustomLabelBoostTcp.Text = "TCP Boost: Calculating...";
        });
        int n = 0;
        await Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await Task.Delay(1000);

                    n++;
                    if (n > int.MaxValue / 2) n = 0;
                    string host = n % 2 == 0 ? "yahoo.com" : "microsoft.com";

                    await StartBenchmark(host);
                    if (IsExiting) break;
                }
                catch (Exception) { }
            }
        });
        Exit = true;
    }

    private async Task StartBenchmark(string host)
    {
        int timeoutMS = 5000;
        float noSdcLatencyUdp = -1, noSdcLatencyTcp = -1, sdcLatencyUdp = -1, sdcLatencyTcp = -1;
        string boostUdp = "UDP Boost: 0%", boostTcp = "TCP Boost: 0%";
        Color udpColor = Color.Gray, tcpColor = Color.Gray;

        if (FormMain.IsDNSConnected)
        {
            // UDP
            if (IsExiting) return;
            CheckDns.CheckDnsResult cdr = await ScanDns.CheckDnsExternalAsync(host, $"udp://{BootstrapIp}:{BootstrapPort}", timeoutMS);
            noSdcLatencyUdp = cdr.DnsLatency;

            if (IsExiting) return;
            cdr = await ScanDns.CheckDnsExternalAsync(host, $"udp://{IPAddress.Loopback}", timeoutMS);
            sdcLatencyUdp = cdr.DnsLatency;

            if (noSdcLatencyUdp != -1 && sdcLatencyUdp != -1)
            {
                try
                {
                    float boostPercent = noSdcLatencyUdp / sdcLatencyUdp * 100f;
                    udpColor = boostPercent < 100 ? Color.DarkOrange : boostPercent >= 100 ? Color.MediumSeaGreen : Color.Gray;
                    boostUdp = $"UDP Boost: {Math.Round(boostPercent)}%";
                }
                catch (Exception)
                {
                    boostUdp = $"UDP Boost: Error";
                    udpColor = Color.IndianRed;
                }
            }
            else
            {
                boostUdp = $"UDP Boost: Offline";
                udpColor = Color.DodgerBlue;
            }

            // TCP
            if (IsExiting) return;
            cdr = await ScanDns.CheckDnsExternalAsync(host, $"tcp://{BootstrapIp}:{BootstrapPort}", timeoutMS);
            noSdcLatencyTcp = cdr.DnsLatency;

            if (IsExiting) return;
            cdr = await ScanDns.CheckDnsExternalAsync(host, $"tcp://{IPAddress.Loopback}", timeoutMS);
            sdcLatencyTcp = cdr.DnsLatency;

            if (noSdcLatencyTcp != -1 && sdcLatencyTcp != -1)
            {
                try
                {
                    float boostPercent = noSdcLatencyTcp / sdcLatencyTcp * 100f;
                    tcpColor = boostPercent < 100 ? Color.DarkOrange : boostPercent >= 100 ? Color.MediumSeaGreen : Color.Gray;
                    boostTcp = $"TCP Boost: {Math.Round(boostPercent)}%";
                }
                catch (Exception)
                {
                    boostTcp = $"TCP Boost: Error";
                    tcpColor = Color.IndianRed;
                }
            }
            else
            {
                boostTcp = $"TCP Boost: Offline";
                tcpColor = Color.DodgerBlue;
            }
        }
        else
        {
            boostUdp = $"UDP Boost: SDC Must Be Online.";
            udpColor = Color.Orange;
            boostTcp = $"TCP Boost: SDC Must Be Online.";
            tcpColor = Color.Orange;
        }

        if (!IsExiting)
        {
            this.InvokeIt(() =>
            {
                // UDP
                CustomLabelNoSdcLatencyUdp.ForeColor = Color.DodgerBlue;
                CustomLabelNoSdcLatencyUdp.Text = $"UDP: {noSdcLatencyUdp} ms";

                CustomLabelSdcLatencyUdp.ForeColor = Color.DodgerBlue;
                CustomLabelSdcLatencyUdp.Text = $"UDP: {sdcLatencyUdp} ms";

                CustomLabelBoostUdp.ForeColor = udpColor;
                CustomLabelBoostUdp.Text = boostUdp;
                
                // TCP
                CustomLabelNoSdcLatencyTcp.ForeColor = Color.DodgerBlue;
                CustomLabelNoSdcLatencyTcp.Text = $"TCP: {noSdcLatencyTcp} ms";

                CustomLabelSdcLatencyTcp.ForeColor = Color.DodgerBlue;
                CustomLabelSdcLatencyTcp.Text = $"TCP: {sdcLatencyTcp} ms";

                CustomLabelBoostTcp.ForeColor = tcpColor;
                CustomLabelBoostTcp.Text = boostTcp;
            });
        }
    }

    private async void FormBenchmark_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!IsExiting)
        {
            e.Cancel = true;
            IsExiting = true;

            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                    if (Exit) break;
                }
            });

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
