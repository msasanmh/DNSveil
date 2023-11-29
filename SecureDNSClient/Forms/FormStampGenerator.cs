using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using System.Diagnostics;
using System.Globalization;

namespace SecureDNSClient;

public partial class FormStampGenerator : Form
{
    private readonly Stopwatch ClearStatus = new();
    private static bool IsExiting = false;
    public FormStampGenerator()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        // Invariant Culture
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);
        Controllers.SetDarkControl(CustomTextBoxStamp);

        CustomComboBoxProtocol.SelectedIndex = 2;
        IsExiting = false;
        ClearStatusAuto();

        Shown -= FormStampGenerator_Shown;
        Shown += FormStampGenerator_Shown;
    }

    private void FormStampGenerator_Shown(object? sender, EventArgs e)
    {
        // Fix Controls Location
        int shw = TextRenderer.MeasureText("I", Font).Width;
        int spaceBottom = 10, spaceRight = 10, spaceV = 10, spaceH = shw, spaceHH = (spaceH * 3);
        CustomLabelProtocol.Location = new Point(spaceRight, spaceBottom);

        CustomComboBoxProtocol.Left = CustomLabelProtocol.Right + spaceH;
        CustomComboBoxProtocol.Top = CustomLabelProtocol.Top - 2;

        CustomLabelIP.Left = spaceRight;
        CustomLabelIP.Top = CustomComboBoxProtocol.Bottom + spaceV;

        CustomTextBoxIP.Left = spaceRight;
        CustomTextBoxIP.Top = CustomLabelIP.Bottom + spaceV;

        CustomLabelHost.Left = spaceRight;
        CustomLabelHost.Top = CustomTextBoxIP.Bottom + spaceV;

        CustomTextBoxHost.Left = spaceRight;
        CustomTextBoxHost.Top = CustomLabelHost.Bottom + spaceV;

        CustomLabelPort.Left = spaceRight;
        CustomLabelPort.Top = CustomTextBoxHost.Bottom + spaceV;

        CustomNumericUpDownPort.Left = CustomLabelPort.Right + spaceH;
        CustomNumericUpDownPort.Top = CustomLabelPort.Top - 2;

        CustomLabelPath.Left = spaceRight;
        CustomLabelPath.Top = CustomNumericUpDownPort.Bottom + spaceV;

        CustomTextBoxPath.Left = CustomLabelPath.Right + spaceH;
        CustomTextBoxPath.Top = CustomLabelPath.Top - 2;

        CustomCheckBoxIsDnsSec.Left = spaceRight;
        CustomCheckBoxIsDnsSec.Top = CustomTextBoxPath.Bottom + spaceV;

        CustomCheckBoxIsNoFilter.Left = CustomCheckBoxIsDnsSec.Right + spaceHH;
        CustomCheckBoxIsNoFilter.Top = CustomCheckBoxIsDnsSec.Top;

        CustomCheckBoxIsNoLog.Left = CustomCheckBoxIsNoFilter.Right + spaceHH;
        CustomCheckBoxIsNoLog.Top = CustomCheckBoxIsNoFilter.Top;

        CustomLabelStatus.Left = spaceRight;
        CustomLabelStatus.Top = ClientRectangle.Height - CustomLabelStatus.Height - spaceBottom;

        // Buttons
        CustomButtonClear.Left = ClientRectangle.Width - CustomButtonClear.Width - spaceH;
        CustomButtonClear.Top = CustomCheckBoxIsNoLog.Bottom + spaceV;

        CustomButtonDecode.Left = CustomButtonClear.Left;
        CustomButtonDecode.Top = CustomButtonClear.Bottom + spaceV;

        CustomButtonEncode.Left = CustomButtonDecode.Left;
        CustomButtonEncode.Top = CustomButtonDecode.Bottom + spaceV;

        // The Box
        CustomTextBoxStamp.Left = spaceRight;
        CustomTextBoxStamp.Top = CustomCheckBoxIsNoLog.Bottom + spaceV;
        CustomTextBoxStamp.Width = CustomButtonEncode.Left - CustomTextBoxStamp.Left - spaceH;
        CustomTextBoxStamp.Height = CustomLabelStatus.Top - CustomTextBoxStamp.Top - spaceV;

        // Top-Right Side
        CustomLabelProviderName.Left = CustomComboBoxProtocol.Right + spaceHH;
        CustomLabelProviderName.Top = CustomLabelIP.Top;

        CustomTextBoxProviderName.Left = CustomLabelProviderName.Left;
        CustomTextBoxProviderName.Top = CustomTextBoxIP.Top;

        CustomLabelPublicKey.Left = CustomTextBoxProviderName.Left;
        CustomLabelPublicKey.Top = CustomLabelHost.Top;

        CustomTextBoxPublicKey.Left = CustomLabelPublicKey.Left;
        CustomTextBoxPublicKey.Top = CustomTextBoxHost.Top;

        CustomLabelHash.Left = CustomTextBoxPublicKey.Left;
        CustomLabelHash.Top = CustomLabelPort.Top;

        CustomTextBoxHash.Left = CustomLabelHash.Left;
        CustomTextBoxHash.Top = CustomLabelHash.Bottom + spaceV;
    }

    private void CustomButtonClear_Click(object? sender, EventArgs? e)
    {
        CustomTextBoxIP.Text = string.Empty;
        CustomTextBoxHost.Text = string.Empty;
        CustomTextBoxPath.Text = string.Empty;
        CustomTextBoxProviderName.Text = string.Empty;
        CustomTextBoxPublicKey.Text = string.Empty;
        CustomTextBoxHash.Text = string.Empty;
        CustomCheckBoxIsDnsSec.Checked = false;
        CustomCheckBoxIsNoFilter.Checked = false;
        CustomCheckBoxIsNoLog.Checked = false;
        CustomTextBoxStamp.Text = string.Empty;
    }

    private void CustomButtonDecode_Click(object sender, EventArgs e)
    {
        string stamp = CustomTextBoxStamp.Text;
        stamp = stamp.Trim();

        if (string.IsNullOrEmpty(stamp) || string.IsNullOrWhiteSpace(stamp))
        {
            Status("Stamp is Empty.", Color.IndianRed);
            return;
        }

        if (!stamp.ToLower().StartsWith("sdns://"))
        {
            Status("Stamp URL must start with sdns://", Color.IndianRed);
            return;
        }

        if (stamp.ToLower().Equals("sdns://"))
        {
            Status("Invalid Stamp.", Color.IndianRed);
            return;
        }

        try
        {
            DNSCryptStampReader sr = new(stamp);

            if (!sr.IsDecryptionSuccess)
            {
                Status("Invalid Stamp.", Color.IndianRed);
                return;
            }

            SetProtocol(sr.Protocol);

            CustomCheckBoxIsDnsSec.Checked = sr.IsDnsSec;
            CustomCheckBoxIsNoFilter.Checked = sr.IsNoFilter;
            CustomCheckBoxIsNoLog.Checked = sr.IsNoLog;

            CustomTextBoxIP.Text = sr.IP; // IP
            CustomTextBoxHost.Text = sr.Host; // Host
            CustomNumericUpDownPort.Value = Convert.ToDecimal(sr.Port); // Port
            CustomTextBoxPath.Text = sr.Path; // Path
            CustomTextBoxProviderName.Text = sr.ProviderName; // Provider Name
            CustomTextBoxPublicKey.Text = sr.PublicKey; // Public Key

            // Hashes
            CustomTextBoxHash.Text = string.Empty;
            string hashes = string.Empty;
            for (int n = 0; n < sr.Hashi.Count; n++)
            {
                string hash = sr.Hashi[n];
                if (n == 0) hashes += hash;
                else hashes += $",{hash}";
            }
            CustomTextBoxHash.Text = hashes;

            Status("Stamp Decrypted Successfully.", Color.MediumSeaGreen);
        }
        catch (Exception)
        {
            Status("Invalid Stamp.", Color.IndianRed);
            return;
        }

    }

    private void CustomButtonEncode_Click(object sender, EventArgs e)
    {
        DNSCryptStampReader.StampProtocol p = GetProtocol();
        string ip = CustomTextBoxIP.Text.Trim();
        string host = CustomTextBoxHost.Text.Trim();
        int port = Convert.ToInt32(CustomNumericUpDownPort.Value);
        string path = CustomTextBoxPath.Text.Trim();
        string provider = CustomTextBoxProviderName.Text.Trim();
        string publicKey = CustomTextBoxPublicKey.Text.Trim();
        string hash = CustomTextBoxHash.Text.Trim();
        bool isDNSSec = CustomCheckBoxIsDnsSec.Checked;
        bool isNoFilter = CustomCheckBoxIsNoFilter.Checked;
        bool isNoLog = CustomCheckBoxIsNoLog.Checked;

        // Empty Messages
        string msgIp = "IP Address is Empty.";
        string msgHost = "Host Name is Empty.";
        string msgProvider = "Provider Name is Empty.";
        string msgPublicKey = "Public Key is Empty.";
        string msgSuccess = "Stamp Encrypted Successfully.";

        string result = "sdns://";
        DNSCryptStampGenerator generator = new();

        try
        {
            if (p == DNSCryptStampReader.StampProtocol.PlainDNS)
            {
                if (string.IsNullOrEmpty(ip))
                    Status(msgIp, Color.IndianRed);
                else
                {
                    string ipPort = ip;
                    if (port != DNSCryptStampReader.DefaultPort.PlainDNS)
                        ipPort += $":{port}";
                    result = generator.GeneratePlainDns(ipPort, isDNSSec, isNoLog, isNoFilter);
                    Status(msgSuccess, Color.MediumSeaGreen);
                }
            }
            else if (p == DNSCryptStampReader.StampProtocol.DnsCrypt)
            {
                if (string.IsNullOrEmpty(ip))
                    Status(msgIp, Color.IndianRed);
                else if (string.IsNullOrEmpty(publicKey))
                    Status(msgPublicKey, Color.IndianRed);
                else if (string.IsNullOrEmpty(provider))
                    Status(msgProvider, Color.IndianRed);
                else
                {
                    string ipPort = ip;
                    if (port != DNSCryptStampReader.DefaultPort.DnsCrypt)
                        ipPort += $":{port}";
                    result = generator.GenerateDNSCrypt(ipPort, publicKey, provider, isDNSSec, isNoLog, isNoFilter);
                    Status(msgSuccess, Color.MediumSeaGreen);
                }
            }
            else if (p == DNSCryptStampReader.StampProtocol.DoH)
            {
                if (string.IsNullOrEmpty(ip))
                    Status(msgIp, Color.IndianRed);
                else if (string.IsNullOrEmpty(host))
                    Status(msgHost, Color.IndianRed);
                else
                {
                    string hostPort = host;
                    if (port != DNSCryptStampReader.DefaultPort.DoH)
                        hostPort += $":{port}";
                    result = generator.GenerateDoH(ip, hash, hostPort, path, null, isDNSSec, isNoLog, isNoFilter);
                    Status(msgSuccess, Color.MediumSeaGreen);
                }
            }
            else if (p == DNSCryptStampReader.StampProtocol.DoT)
            {
                if (string.IsNullOrEmpty(ip))
                    Status(msgIp, Color.IndianRed);
                else if (string.IsNullOrEmpty(host))
                    Status(msgHost, Color.IndianRed);
                else
                {
                    string hostPort = host;
                    if (port != DNSCryptStampReader.DefaultPort.DoT)
                        hostPort += $":{port}";
                    result = generator.GenerateDoT(ip, hash, hostPort, null, isDNSSec, isNoLog, isNoFilter);
                    Status(msgSuccess, Color.MediumSeaGreen);
                }
            }
            else if (p == DNSCryptStampReader.StampProtocol.DoQ)
            {
                if (string.IsNullOrEmpty(ip))
                    Status(msgIp, Color.IndianRed);
                else if (string.IsNullOrEmpty(host))
                    Status(msgHost, Color.IndianRed);
                else
                {
                    result = generator.GenerateDoQ(ip, hash, $"{host}:{port}", null, isDNSSec, isNoLog, isNoFilter);
                    Status(msgSuccess, Color.MediumSeaGreen);
                }
            }
            else if (p == DNSCryptStampReader.StampProtocol.ObliviousDohTarget)
            {
                if (string.IsNullOrEmpty(host))
                    Status(msgHost, Color.IndianRed);
                else
                {
                    result = generator.GenerateObliviousDohTarget($"{host}:{port}", path, isDNSSec, isNoLog, isNoFilter);
                    Status(msgSuccess, Color.MediumSeaGreen);
                }
            }
            else if (p == DNSCryptStampReader.StampProtocol.AnonymizedDNSCryptRelay)
            {
                if (string.IsNullOrEmpty(ip))
                    Status(msgIp, Color.IndianRed);
                else
                {
                    result = generator.GenerateAnonymizedDNSCryptRelay($"{ip}:{port}");
                    Status(msgSuccess, Color.MediumSeaGreen);
                }
            }
            else if (p == DNSCryptStampReader.StampProtocol.ObliviousDohRelay)
            {
                if (string.IsNullOrEmpty(ip))
                    Status(msgIp, Color.IndianRed);
                else if (string.IsNullOrEmpty(host))
                    Status(msgHost, Color.IndianRed);
                else
                {
                    result = generator.GenerateObliviousDohRelay(ip, hash, $"{host}:{port}", path, null, isDNSSec, isNoLog, isNoFilter);
                    Status(msgSuccess, Color.MediumSeaGreen);
                }
            }
            else
                Status("Select a protocol.", Color.IndianRed);
        }
        catch (Exception)
        {
            Status("Insufficient Data!", Color.IndianRed);
        }

        CustomTextBoxStamp.Text = result;
    }

    private DNSCryptStampReader.StampProtocol GetProtocol()
    {
        int index = CustomComboBoxProtocol.SelectedIndex;

        DNSCryptStampReader.StampProtocol protocol = index switch
        {
            -1 => DNSCryptStampReader.StampProtocol.Unknown,
            0 => DNSCryptStampReader.StampProtocol.PlainDNS,
            1 => DNSCryptStampReader.StampProtocol.DnsCrypt,
            2 => DNSCryptStampReader.StampProtocol.DoH,
            3 => DNSCryptStampReader.StampProtocol.DoT,
            4 => DNSCryptStampReader.StampProtocol.DoQ,
            5 => DNSCryptStampReader.StampProtocol.ObliviousDohTarget,
            6 => DNSCryptStampReader.StampProtocol.AnonymizedDNSCryptRelay,
            7 => DNSCryptStampReader.StampProtocol.ObliviousDohRelay,
            _ => DNSCryptStampReader.StampProtocol.Unknown,
        };

        return protocol;
    }

    private void SetProtocol(DNSCryptStampReader.StampProtocol protocol)
    {
        CustomComboBoxProtocol.SelectedIndex = protocol switch
        {
            DNSCryptStampReader.StampProtocol.Unknown => -1,
            DNSCryptStampReader.StampProtocol.PlainDNS => 0,
            DNSCryptStampReader.StampProtocol.DnsCrypt => 1,
            DNSCryptStampReader.StampProtocol.DoH => 2,
            DNSCryptStampReader.StampProtocol.DoT => 3,
            DNSCryptStampReader.StampProtocol.DoQ => 4,
            DNSCryptStampReader.StampProtocol.ObliviousDohTarget => 5,
            DNSCryptStampReader.StampProtocol.AnonymizedDNSCryptRelay => 6,
            DNSCryptStampReader.StampProtocol.ObliviousDohRelay => 7,
            _ => CustomComboBoxProtocol.SelectedIndex = -1,
        };
    }

    private void Status(string msg, Color color)
    {
        if (!ClearStatus.IsRunning) ClearStatus.Start();
        ClearStatus.Restart();
        CustomLabelStatus.ForeColor = color;
        CustomLabelStatus.Text = msg;
    }

    private async void ClearStatusAuto()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(200);
                if (IsExiting) break;

                try
                {
                    if (ClearStatus.ElapsedMilliseconds > 5000 && !IsExiting)
                        if (!string.IsNullOrEmpty(CustomLabelStatus.Text))
                            this.InvokeIt(() => CustomLabelStatus.Text = string.Empty);
                }
                catch (Exception) { }
            }
        });
    }

    private void CustomComboBoxProtocol_SelectedIndexChanged(object sender, EventArgs e)
    {
        var p = GetProtocol();
        var ip = CustomTextBoxIP;
        var host = CustomTextBoxHost;
        var port = CustomNumericUpDownPort;
        var path = CustomTextBoxPath;
        var provider = CustomTextBoxProviderName;
        var publicKey = CustomTextBoxPublicKey;
        var hash = CustomTextBoxHash;
        var isDNSSec = CustomCheckBoxIsDnsSec;
        var isNoFilter = CustomCheckBoxIsNoFilter;
        var isNoLog = CustomCheckBoxIsNoLog;

        if (p == DNSCryptStampReader.StampProtocol.PlainDNS)
        {
            ip.Enabled = true;
            host.Enabled = false;
            port.Enabled = true; port.Value = DNSCryptStampReader.DefaultPort.PlainDNS;
            path.Enabled = false;
            provider.Enabled = false;
            publicKey.Enabled = false;
            hash.Enabled = false;
            isDNSSec.Enabled = true;
            isNoFilter.Enabled = true;
            isNoLog.Enabled = true;
        }
        else if (p == DNSCryptStampReader.StampProtocol.DnsCrypt)
        {
            ip.Enabled = true;
            host.Enabled = false;
            port.Enabled = true; port.Value = DNSCryptStampReader.DefaultPort.DnsCrypt;
            path.Enabled = false;
            provider.Enabled = true; provider.Text = "2.dnscrypt-cert.";
            publicKey.Enabled = true;
            hash.Enabled = false;
            isDNSSec.Enabled = true;
            isNoFilter.Enabled = true;
            isNoLog.Enabled = true;
        }
        else if (p == DNSCryptStampReader.StampProtocol.DoH)
        {
            ip.Enabled = true;
            host.Enabled = true;
            port.Enabled = true; port.Value = DNSCryptStampReader.DefaultPort.DoH;
            path.Enabled = true; path.Text = "/dns-query";
            provider.Enabled = false;
            publicKey.Enabled = false;
            hash.Enabled = true;
            isDNSSec.Enabled = true;
            isNoFilter.Enabled = true;
            isNoLog.Enabled = true;
        }
        else if (p == DNSCryptStampReader.StampProtocol.DoT)
        {
            ip.Enabled = true;
            host.Enabled = true;
            port.Enabled = true; port.Value = DNSCryptStampReader.DefaultPort.DoT;
            path.Enabled = false;
            provider.Enabled = false;
            publicKey.Enabled = false;
            hash.Enabled = true;
            isDNSSec.Enabled = true;
            isNoFilter.Enabled = true;
            isNoLog.Enabled = true;
        }
        else if (p == DNSCryptStampReader.StampProtocol.DoQ)
        {
            ip.Enabled = true;
            host.Enabled = true;
            port.Enabled = true; port.Value = DNSCryptStampReader.DefaultPort.DoQ;
            path.Enabled = false;
            provider.Enabled = false;
            publicKey.Enabled = false;
            hash.Enabled = true;
            isDNSSec.Enabled = true;
            isNoFilter.Enabled = true;
            isNoLog.Enabled = true;
        }
        else if (p == DNSCryptStampReader.StampProtocol.ObliviousDohTarget)
        {
            ip.Enabled = false;
            host.Enabled = true;
            port.Enabled = true; port.Value = DNSCryptStampReader.DefaultPort.ObliviousDohTarget;
            path.Enabled = true; path.Text = "/dns-query";
            provider.Enabled = false;
            publicKey.Enabled = false;
            hash.Enabled = false;
            isDNSSec.Enabled = true;
            isNoFilter.Enabled = true;
            isNoLog.Enabled = true;
        }
        else if (p == DNSCryptStampReader.StampProtocol.AnonymizedDNSCryptRelay)
        {
            ip.Enabled = true;
            host.Enabled = false;
            port.Enabled = true; port.Value = DNSCryptStampReader.DefaultPort.AnonymizedDNSCryptRelay;
            path.Enabled = false;
            provider.Enabled = false;
            publicKey.Enabled = false;
            hash.Enabled = false;
            isDNSSec.Enabled = false;
            isNoFilter.Enabled = false;
            isNoLog.Enabled = false;
        }
        else if (p == DNSCryptStampReader.StampProtocol.ObliviousDohRelay)
        {
            ip.Enabled = true;
            host.Enabled = true;
            port.Enabled = true; port.Value = DNSCryptStampReader.DefaultPort.ObliviousDohRelay;
            path.Enabled = true; path.Text = "/dns-query";
            provider.Enabled = false;
            publicKey.Enabled = false;
            hash.Enabled = true;
            isDNSSec.Enabled = true;
            isNoFilter.Enabled = true;
            isNoLog.Enabled = true;
        }
        else
        {
            CustomButtonClear_Click(null, null);
            ip.Enabled = false;
            host.Enabled = false;
            port.Enabled = false;
            path.Enabled = false;
            provider.Enabled = false;
            publicKey.Enabled = false;
            hash.Enabled = false;
            isDNSSec.Enabled = false;
            isNoFilter.Enabled = false;
            isNoLog.Enabled = false;
        }
    }

    private void FormStampGenerator_FormClosing(object sender, FormClosingEventArgs e)
    {
        IsExiting = true;
    }
}