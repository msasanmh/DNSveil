using MsmhToolsClass;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    private async void SetProxy(bool unset = false, bool limitLog = false)
    {
        if (IsAnotherProxySet)
        {
            // Unset Another Proxy
            NetworkTool.UnsetProxy(false, true);
            await Task.Delay(300); // Wait a moment

            IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
            IsAnotherProxySet = isAnotherProxySet;
            CurrentSystemProxy = currentSystemProxy;
            if (!IsAnotherProxySet)
            {
                // Write Unset Proxy message to log
                string msg = $"Proxy Server ({CurrentSystemProxy}) removed from system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray);
            }
            else
            {
                // Write Unset Proxy error to log
                string msg = $"Couldn't unset Proxy Server ({CurrentSystemProxy}) from system.";
                CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
            }
        }
        else
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
                if (!IsProxyActivated)
                {
                    string msg = "Enable Proxy first." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    return;
                }

                // Get IP:Port
                string ip = IPAddress.Loopback.ToString();
                int port = ProxyPort != -1 ? ProxyPort : GetProxyPortSetting();

                // Start Set Proxy
                await SetProxyInternalAsync();
                await Task.Delay(100); // Wait a moment

                IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
                IsAnotherProxySet = isAnotherProxySet;
                CurrentSystemProxy = currentSystemProxy;
                if (IsProxySet)
                {
                    // Write Set Proxy message to log
                    if (!limitLog)
                    {
                        string msg1 = "Proxy Server ";
                        string msg2 = " set to system.";
                        CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                        CustomRichTextBoxLog.AppendText(CurrentSystemProxy, Color.DodgerBlue);
                        CustomRichTextBoxLog.AppendText(msg2 + NL, Color.LightGray);
                    }
                    else
                    {
                        string msg = $"Proxy Server ({CurrentSystemProxy}) set to system.";
                        CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray);
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
                await Task.Delay(100); // Wait a moment

                IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
                IsAnotherProxySet = isAnotherProxySet;
                CurrentSystemProxy = currentSystemProxy;
                if (!IsProxySet)
                {
                    // Write Unset Proxy message to log
                    string msg = $"Proxy Server ({CurrentSystemProxy}) removed from system.";
                    CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray);
                }
                else
                {
                    // Write Unset Proxy error to log
                    string msg = $"Couldn't unset Proxy Server ({CurrentSystemProxy}) from system.";
                    CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
                }
            }
        }

        // To See Status Immediately
        await UpdateStatusLong();
    }

    private async Task SetProxyInternalAsync()
    {
        // Update Proxy Bools
        await UpdateBoolProxy();

        // Get IP:Port
        string ip = IPAddress.Loopback.ToString();
        int port = ProxyPort != -1 ? ProxyPort : GetProxyPortSetting();

        // Start Set Proxy
        if (IsProxySSLDecryptionActive)
            NetworkTool.SetProxy($"{ip}:{port}", null, null, null, true);
        else
            NetworkTool.SetProxy(null, null, null, $"{ip}:{port}", false);
    }
}