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
        public static int GetCheckTimeout(CustomNumericUpDown customNumericUpDown)
        {
            return Convert.ToInt32(customNumericUpDown.Value);
        }

        public static int GetHTTPProxyPort(CustomNumericUpDown customNumericUpDown)
        {
            return Convert.ToInt32(Math.Round(customNumericUpDown.Value));
        }
    }
}
