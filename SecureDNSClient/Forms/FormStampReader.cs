using MsmhToolsClass.DnsTool;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using System;

namespace SecureDNSClient
{
    public partial class FormStampReader : Form
    {
        private readonly string NL = Environment.NewLine;
        public FormStampReader()
        {
            // Fix Screed DPI
            ScreenDPI.FixDpiBeforeInitializeComponent(this);
            InitializeComponent();
            ScreenDPI.FixDpiAfterInitializeComponent(this);

            // Load Theme
            Theme.LoadTheme(this, Theme.Themes.Dark);
            Controllers.SetDarkControl(CustomTextBoxResult);

            CustomTextBoxResult.Text = string.Empty;
        }

        private void CustomButtonDecode_Click(object sender, EventArgs e)
        {
            string stamp = CustomTextBoxStampUrl.Text;
            stamp = stamp.Trim();
            if (string.IsNullOrEmpty(stamp)) return;
            if (string.IsNullOrWhiteSpace(stamp)) return;
            CustomTextBoxResult.Text = string.Empty;

            CustomTextBoxResult.Font = new Font(FontFamily.GenericMonospace, 10);

            if (!stamp.ToLower().StartsWith("sdns://"))
            {
                CustomTextBoxResult.Text = "Stamp URL must start with sdns://";
                return;
            }

            CustomTextBoxResult.Text = "Decoding...";
            string result = $"Output:{NL}{NL}";

            try
            {
                DNSCryptStampReader sr = new(stamp);

                if (!sr.IsDecryptionSuccess)
                {
                    CustomTextBoxResult.Text = "Invalid Stamp.";
                    return;
                }

                result += $"Protocol: {sr.ProtocolName}{NL}{NL}"; // Protocol

                result += $"Is DNSSec: {sr.IsDnsSec}{NL}"; // DNSSec
                result += $"Is No Filter: {sr.IsNoFilter}{NL}"; // Filter
                result += $"Is No Log: {sr.IsNoLog}{NL}{NL}"; // Log

                if (!string.IsNullOrEmpty(sr.IP)) // IP
                    result += $"IP: {sr.IP}{NL}";
                if (!string.IsNullOrEmpty(sr.Host)) // Host
                    result += $"Host: {sr.Host}{NL}";
                result += $"Port: {sr.Port}{NL}"; // Port
                if (!string.IsNullOrEmpty(sr.Path)) // Path
                    result += $"Path: {sr.Path}{NL}";
                if (!string.IsNullOrEmpty(sr.ProviderName)) // Provider Name
                    result += $"Provider Name: {sr.ProviderName}{NL}";

                if (!string.IsNullOrEmpty(sr.PublicKey)) // Public Key
                {
                    result += $"{NL}Public Key:{NL}";
                    result += $"{sr.PublicKey}{NL}";
                }

                if (sr.Hashi.Any()) // Hash Codes
                {
                    result += $"{NL}Hash Codes:{NL}";
                    for (int n = 0; n < sr.Hashi.Count; n++)
                        result += $"{sr.Hashi[n]}{NL}";
                    result += NL;
                }

                if (sr.Bootstraps.Any()) // Bootstraps
                {
                    result += $"{NL}Bootstraps:{NL}";
                    for (int n = 0; n < sr.Bootstraps.Count; n++)
                        result += $"{sr.Bootstraps[n]}{NL}";
                    result += NL;
                }

                CustomTextBoxResult.Text = result;
            }
            catch (Exception)
            {
                CustomTextBoxResult.Text = "Invalid Stamp.";
                return;
            }
        }
    }
}
