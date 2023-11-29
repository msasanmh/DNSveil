using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace SecureDNSClient;

public partial class FormDnsLookup : Form
{
    // Settings XML path
    private static readonly string SettingsXmlPath = SecureDNS.SettingsXmlDnsLookup;
    private readonly Settings AppSettings;
    private List<string> ResultList = new();
    private int PID = -1;

    public FormDnsLookup()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        InitializeComponent();

        // Invariant Culture
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        // Load Theme
        Theme.LoadTheme(this, Theme.Themes.Dark);
        Controllers.SetDarkControl(CustomTextBoxResult);

        // Set Tooltips
        CustomRadioButtonSourceSDC.SetToolTip(FormMain.MainToolTip, "Info", "SDC must be Online.");
        CustomRadioButtonSourceCustom.SetToolTip(FormMain.MainToolTip, "Supported DNS", "Plain DNS, DoH (https://), DoT (tls://), DoQ (quic://), DNSCrypt (sdns://).");
        CustomLabelSUBNET.SetToolTip(FormMain.MainToolTip, "Info", "e.g. 1.2.3.4/24");
        string ednsopt = $"Specify EDNS option with code point code and optionally payload of value as a hexadecimal string:";
        ednsopt += $"{Environment.NewLine}code:value";
        ednsopt += $"{Environment.NewLine}65074:3132333435363738";
        CustomLabelEDNSOPT.SetToolTip(FormMain.MainToolTip, "Info", ednsopt);

        // Initialize and load Settings
        if (File.Exists(SettingsXmlPath) && XmlTool.IsValidXMLFile(SettingsXmlPath))
            AppSettings = new(this, SettingsXmlPath);
        else
            AppSettings = new(this);

        CustomTextBoxResult.Text = string.Empty;

        Shown -= FormDnsLookup_Shown;
        Shown += FormDnsLookup_Shown;
    }

    private void FormDnsLookup_Shown(object? sender, EventArgs e)
    {
        // Fix Controls Location
        int shw = TextRenderer.MeasureText("I", Font).Width;
        int spaceBottom = 10, spaceRight = 12, spaceV = 6, spaceH = shw;
        CustomRadioButtonSourceSDC.Location = new Point(spaceRight, spaceBottom);

        CustomRadioButtonSourceCustom.Left = CustomRadioButtonSourceSDC.Right + spaceH;
        CustomRadioButtonSourceCustom.Top = CustomRadioButtonSourceSDC.Top;

        CustomTextBoxSourceCustom.Left = CustomRadioButtonSourceCustom.Right + spaceH;
        CustomTextBoxSourceCustom.Top = CustomRadioButtonSourceCustom.Top - 2;

        CustomButtonDefault.Top = CustomRadioButtonSourceCustom.Top;
        CustomButtonDefault.Left = ClientRectangle.Width - CustomButtonDefault.Width - spaceRight;

        CustomTextBoxSourceCustom.Width = CustomButtonDefault.Left - CustomTextBoxSourceCustom.Left - spaceH;

        CustomButtonLookup.Left = CustomButtonDefault.Left;
        CustomButtonLookup.Top = CustomButtonDefault.Bottom + spaceV;

        CustomTextBoxDomain.Left = CustomTextBoxSourceCustom.Left;
        CustomTextBoxDomain.Top = CustomTextBoxSourceCustom.Bottom + spaceV;
        CustomTextBoxDomain.Width = CustomTextBoxSourceCustom.Width;

        CustomLabelDomain.Left = CustomTextBoxDomain.Left - CustomLabelDomain.Width - spaceH;
        CustomLabelDomain.Top = CustomTextBoxDomain.Top + 2;

        CustomCheckBoxHTTP3.Left = spaceRight;
        CustomCheckBoxHTTP3.Top = CustomTextBoxDomain.Bottom + spaceV;

        CustomCheckBoxVERIFY.Left = CustomCheckBoxHTTP3.Right + spaceH;
        CustomCheckBoxVERIFY.Top = CustomCheckBoxHTTP3.Top;

        CustomCheckBoxDNSSEC.Left = spaceRight;
        CustomCheckBoxDNSSEC.Top = CustomCheckBoxVERIFY.Bottom + spaceV;

        CustomCheckBoxPAD.Left = CustomCheckBoxDNSSEC.Right + spaceH;
        CustomCheckBoxPAD.Top = CustomCheckBoxDNSSEC.Top;

        CustomCheckBoxVERBOSE.Left = CustomCheckBoxPAD.Right + spaceH;
        CustomCheckBoxVERBOSE.Top = CustomCheckBoxPAD.Top;

        CustomCheckBoxJSON.Left = CustomCheckBoxVERBOSE.Right + spaceH;
        CustomCheckBoxJSON.Top = CustomCheckBoxVERBOSE.Top;

        CustomLabelRRTYPE.Left = spaceRight;
        CustomLabelRRTYPE.Top = CustomCheckBoxJSON.Bottom + spaceV;

        CustomTextBoxRRTYPE.Left = CustomLabelRRTYPE.Right + spaceH;
        CustomTextBoxRRTYPE.Top = CustomLabelRRTYPE.Top - 2;

        CustomLabelCLASS.Left = CustomTextBoxRRTYPE.Right + spaceH;
        CustomLabelCLASS.Top = CustomLabelRRTYPE.Top;

        CustomTextBoxCLASS.Left = CustomLabelCLASS.Right + spaceH;
        CustomTextBoxCLASS.Top = CustomTextBoxRRTYPE.Top;

        CustomLabelSUBNET.Left = CustomTextBoxCLASS.Right + spaceH;
        CustomLabelSUBNET.Top = CustomLabelCLASS.Top;

        CustomTextBoxSUBNET.Left = CustomLabelSUBNET.Right + spaceH;
        CustomTextBoxSUBNET.Top = CustomTextBoxCLASS.Top;

        CustomLabelEDNSOPT.Left = spaceRight;
        CustomLabelEDNSOPT.Top = CustomTextBoxSUBNET.Bottom + spaceV;

        CustomTextBoxEDNSOPT.Left = CustomLabelEDNSOPT.Right + spaceH;
        CustomTextBoxEDNSOPT.Top = CustomLabelEDNSOPT.Top - 2;
        CustomTextBoxEDNSOPT.Width = ClientRectangle.Width - CustomTextBoxEDNSOPT.Left - CustomButtonLookup.Width - spaceH - spaceRight;

        CustomTextBoxResult.Left = spaceRight;
        CustomTextBoxResult.Top = CustomTextBoxEDNSOPT.Bottom + spaceV;
        CustomTextBoxResult.Width = ClientRectangle.Width - (spaceRight * 2);
        CustomTextBoxResult.Height = ClientRectangle.Height - CustomTextBoxResult.Top - spaceBottom;
    }

    private void CustomButtonDefault_Click(object sender, EventArgs e)
    {
        CustomRadioButtonSourceSDC.Checked = true;
        CustomRadioButtonSourceCustom.Checked = false;
        CustomCheckBoxHTTP3.Checked = false;
        CustomCheckBoxVERIFY.Checked = false;
        CustomCheckBoxDNSSEC.Checked = false;
        CustomCheckBoxPAD.Checked = false;
        CustomCheckBoxVERBOSE.Checked = false;
        CustomTextBoxRRTYPE.Text = "A";
        CustomTextBoxCLASS.Text = "IN";
        CustomTextBoxSUBNET.Text = string.Empty;
        CustomTextBoxEDNSOPT.Text = string.Empty;
    }

    private async void CustomButtonLookup_Click(object sender, EventArgs e)
    {
        // Kill previous task
        if (PID != -1) ProcessManager.KillProcessByPID(PID);

        CustomTextBoxResult.Text = string.Empty;

        if (!File.Exists(SecureDNS.DnsLookup))
        {
            CustomTextBoxResult.Text = "Binary is missing.";
            return;
        }

        string dns = $"{IPAddress.Loopback}:53";
        if (CustomRadioButtonSourceCustom.Checked)
            dns = CustomTextBoxSourceCustom.Text;

        if (!FormMain.IsDnsProtocolSupported(dns))
        {
            CustomTextBoxResult.Text = "DNS is not supported.";
            return;
        }

        if (CustomRadioButtonSourceSDC.Checked && !FormMain.IsDNSConnected)
        {
            CustomTextBoxResult.Text = "SDC is not Online.";
            return;
        }

        string domain = CustomTextBoxDomain.Text;

        if (string.IsNullOrEmpty(domain) || string.IsNullOrWhiteSpace(domain))
        {
            CustomTextBoxResult.Text = "Domain field is empty.";
            return;
        }

        string args = $"{domain} {dns}";

        CustomTextBoxResult.Text = $"Looking Up {domain}...";

        string result;

        using Process process = new();
        process.StartInfo.FileName = SecureDNS.DnsLookup;
        process.StartInfo.Arguments = args;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.WorkingDirectory = AppContext.BaseDirectory;

        if (CustomCheckBoxHTTP3.Checked)
            process.StartInfo.EnvironmentVariables["HTTP3"] = "1";
        else
            process.StartInfo.EnvironmentVariables["HTTP3"] = "";

        if (CustomCheckBoxVERIFY.Checked)
            process.StartInfo.EnvironmentVariables["VERIFY"] = "0";
        else
            process.StartInfo.EnvironmentVariables["VERIFY"] = "";

        if (CustomCheckBoxDNSSEC.Checked)
            process.StartInfo.EnvironmentVariables["DNSSEC"] = "1";
        else
            process.StartInfo.EnvironmentVariables["DNSSEC"] = "";

        if (CustomCheckBoxPAD.Checked)
            process.StartInfo.EnvironmentVariables["PAD"] = "1";
        else
            process.StartInfo.EnvironmentVariables["PAD"] = "";

        if (CustomCheckBoxVERBOSE.Checked)
            process.StartInfo.EnvironmentVariables["VERBOSE"] = "1";
        else
            process.StartInfo.EnvironmentVariables["VERBOSE"] = "";

        bool json = CustomCheckBoxJSON.Checked;
        if (json)
            process.StartInfo.EnvironmentVariables["JSON"] = "1";
        else
            process.StartInfo.EnvironmentVariables["JSON"] = "";

        string rrtype = CustomTextBoxRRTYPE.Text.Trim();
        if (string.IsNullOrWhiteSpace(rrtype)) rrtype = "A";
        process.StartInfo.EnvironmentVariables["RRTYPE"] = rrtype;

        string dnsClass = CustomTextBoxCLASS.Text.Trim();
        if (string.IsNullOrWhiteSpace(dnsClass)) dnsClass = "IN";
        process.StartInfo.EnvironmentVariables["CLASS"] = dnsClass;

        string dnsSubnet = CustomTextBoxSUBNET.Text.Trim();
        if (string.IsNullOrWhiteSpace(dnsSubnet)) dnsSubnet = string.Empty;
        process.StartInfo.EnvironmentVariables["SUBNET"] = dnsSubnet;

        string dnsEDNSOPT = CustomTextBoxEDNSOPT.Text.Trim();
        if (string.IsNullOrWhiteSpace(dnsEDNSOPT)) dnsEDNSOPT = string.Empty;
        process.StartInfo.EnvironmentVariables["EDNSOPT"] = dnsEDNSOPT;

        try
        {
            await Task.Run(() => process.Start());
            PID = process.Id;
        }
        catch (Exception ex)
        {
            result = Environment.NewLine + ex.Message;
            PID = -1;
        }

        string stdout = string.Empty;
        string errout = string.Empty;

        await Task.Run(() =>
        {
            stdout = process.StandardOutput.ReadToEnd().ReplaceLineEndings(Environment.NewLine);
            errout = process.StandardError.ReadToEnd().ReplaceLineEndings(Environment.NewLine);
        });

        result = stdout + Environment.NewLine + errout;

        string resultOut = string.Empty;
        string remove1 = "dnslookup";
        string remove2 = "dnslookup result ";
        ResultList = result.SplitToLines();
        for (int n = 0; n < ResultList.Count; n++)
        {
            string line = ResultList[n];
            if (n == 0 && line.Contains(remove1)) continue;
            if (n == 1 && string.IsNullOrEmpty(line)) continue;
            if (line.Contains(remove2)) line = line.Replace(remove2, string.Empty);
            resultOut += line + Environment.NewLine;
        }

        if (resultOut.Length > 2)
            CustomTextBoxResult.Text = resultOut;

        try
        {
            process.Kill();
            PID = -1;
        }
        catch (Exception)
        {
            // do nothing
        }
    }

    private async void FormDnsLookup_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown)
        {
            CustomTextBoxResult.Text = string.Empty;

            // Select Control type and properties to save
            AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Text");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Texts");
            AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");

            // Add Settings to save
            AppSettings.AddSelectedSettings(this);

            // Save Application Settings
            await AppSettings.SaveAsync(SettingsXmlPath);
        }
    }

}