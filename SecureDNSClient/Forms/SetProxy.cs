using MsmhToolsClass;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    private async void SetProxy(bool unset = false)
    {
        if (!IsProxySet)
        {
            // Set Proxy
            if (unset) return;

            // Write Let Proxy Start to log
            if (IsProxyActivating)
            {
                string msg = "Let Proxy Start." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Orange));
                return;
            }

            // Write Enable Proxy first to log
            if (!IsProxyRunning)
            {
                string msg = "Enable Proxy first." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return;
            }

            // Get IP:Port
            string ip = IPAddress.Loopback.ToString();
            int port = ProxyPort != -1 ? ProxyPort : GetProxyPortSetting();

            // Start Set Proxy
            NetworkTool.SetProxy(null, null, null, $"{ip}:{port}", false);

            Task.Delay(300).Wait(); // Wait a moment

            bool isProxySet = NetworkTool.IsProxySet(out string _, out string _, out string _, out string _);
            if (isProxySet)
            {
                // Update bool
                IsProxySet = true;

                // Write Set Proxy message to log
                string msg1 = "Proxy Server ";
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
                        await CheckDPIWorks(blockedDomain);
                }
            }
            else
            {
                // Write Set Proxy error to log
                string msg = "Couldn't set Proxy Server to system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }
        else
        {
            // Unset Proxy
            NetworkTool.UnsetProxy(false, true);

            Task.Delay(300).Wait(); // Wait a moment

            bool isProxySet = NetworkTool.IsProxySet(out string _, out string _, out string _, out string _);
            if (!isProxySet)
            {
                // Update bool
                IsProxySet = false;

                // Write Unset Proxy message to log
                string msg1 = "Proxy Server removed from system.";
                CustomRichTextBoxLog.AppendText(msg1 + NL, Color.LightGray);
            }
            else
            {
                // Write Unset Proxy error to log
                string msg = "Couldn't unset Proxy Server from system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }
    }
}