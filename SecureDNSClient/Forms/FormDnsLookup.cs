using CustomControls;
using MsmhTools;
using MsmhTools.Themes;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient
{
    public partial class FormDnsLookup : Form
    {
        // Settings XML path
        private static readonly string SettingsXmlPath = Path.GetFullPath(SecureDNS.CurrentPath + "DnsLookupSettings.xml");
        private readonly Settings AppSettings;
        private List<string> ResultList = new();
        private int PID = -1;

        public FormDnsLookup()
        {
            InitializeComponent();

            // Load Theme
            Theme.LoadTheme(this, Theme.Themes.Dark);
            Controllers.SetDarkControl(CustomTextBoxResult);

            // Set Tooltips
            CustomRadioButtonSourceSDC.SetToolTip("Info", "SDC must be Online.");
            CustomRadioButtonSourceCustom.SetToolTip("Supported DNS", "Plain DNS, DoH (https://), DoT (tls://), DoQ (quic://), DNSCrypt (sdns://).");
            CustomLabelSUBNET.SetToolTip("Info", "e.g. 1.2.3.4/24");
            string ednsopt = $"Specify EDNS option with code point code and optionally payload of value as a hexadecimal string:";
            ednsopt += $"{Environment.NewLine}code:value";
            ednsopt += $"{Environment.NewLine}65074:3132333435363738";
            CustomLabelEDNSOPT.SetToolTip("Info", ednsopt);

            // Initialize and load Settings
            if (File.Exists(SettingsXmlPath) && Xml.IsValidXMLFile(SettingsXmlPath))
                AppSettings = new(this, SettingsXmlPath);
            else
                AppSettings = new(this);

            CustomTextBoxResult.Text = string.Empty;
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
            string remove1 = "dnslookup result ";
            ResultList = result.SplitToLines();
            for (int n = 0; n < ResultList.Count; n++)
            {
                if (n == 0) continue;
                string line = ResultList[n];
                if (n == 1 && string.IsNullOrEmpty(line)) continue;
                if (line.Contains(remove1)) line = line.Replace(remove1, string.Empty);
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
}
