using MsmhTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private void SetProxy()
        {
            if (!IsProxySet)
            {
                // Set Proxy
                // Write Enable Proxy first to log
                if (!IsSharing)
                {
                    string msg = "Enable Proxy first." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    return;
                }

                // Get IP:Port
                string ip = IPAddress.Loopback.ToString();
                int port = ProxyPort;

                // Start Set Proxy
                Network.SetHttpProxy(ip, port);

                Task.Delay(300).Wait(); // Wait a moment

                bool isProxySet = Network.IsProxySet(out string _, out string _, out string _, out string _);
                if (isProxySet)
                {
                    // Update bool
                    IsProxySet = true;

                    // Write Set Proxy message to log
                    string msg1 = "HTTP Proxy ";
                    string msg2 = $"{ip}:{port}";
                    string msg3 = " set to system.";
                    CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                    CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
                    CustomRichTextBoxLog.AppendText(msg3 + NL, Color.LightGray);

                    // Check DPI Works
                    if (CustomCheckBoxPDpiEnableDpiBypass.Checked)
                    {
                        // Get blocked domain
                        string blockedDomain = GetBlockedDomainSetting(out string _);
                        if (!string.IsNullOrEmpty(blockedDomain))
                            CheckDPIWorks(blockedDomain);
                    }
                }
                else
                {
                    // Write Set Proxy error to log
                    string msg = "Couldn't set HTTP Proxy to system.";
                    CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
                }
            }
            else
            {
                // Unset Proxy
                Network.UnsetProxy(false, true);

                Task.Delay(300).Wait(); // Wait a moment

                bool isProxySet = Network.IsProxySet(out string _, out string _, out string _, out string _);
                if (!isProxySet)
                {
                    // Update bool
                    IsProxySet = false;

                    // Write Unset Proxy message to log
                    string msg1 = "HTTP Proxy removed from system.";
                    CustomRichTextBoxLog.AppendText(msg1 + NL, Color.LightGray);
                }
                else
                {
                    // Write Unset Proxy error to log
                    string msg = "Couldn't unset HTTP Proxy from system.";
                    CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
                }
            }
        }
    }
}
