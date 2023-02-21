using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CustomControls;
using MsmhTools;

namespace SecureDNSClient
{
    public static class SettingsWindow
    {
        //private static readonly FormMain MainForm = new();
        private static readonly IPAddress BootstrapDNS = IPAddress.Parse("8.8.8.8");
        public static int GetCheckTimeout(CustomNumericUpDown customNumericUpDown)
        {
            return Convert.ToInt32(customNumericUpDown.Value);
        }

        public static IPAddress GetBootstrapDNS(CustomTextBox customTextBox)
        {
            bool isIP = Network.IsIPv4Valid(customTextBox.Text, out IPAddress? iPAddress);
            return isIP ? iPAddress ?? BootstrapDNS : BootstrapDNS;
        }

        public static int GetHTTPProxyPort(CustomNumericUpDown customNumericUpDown)
        {
            return Convert.ToInt32(Math.Round(customNumericUpDown.Value));
        }

        public static bool GetDontAskAboutCertificate(CustomCheckBox customCheckBox)
        {
            return customCheckBox.Checked;
        }
    }
}
