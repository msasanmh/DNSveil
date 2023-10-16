using MsmhToolsClass;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    public async Task<bool> TryConnectToFakeProxyDohNormally(CheckDns checkDns, string dohUrl, string dohHost, int timeoutMS)
    {
        if (IsDisconnecting) return false;

        // It's available message
        string msgAvailable = $"It's available. Connecting...{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAvailable, Color.MediumSeaGreen));

        // Get loopback
        string loopback = IPAddress.Loopback.ToString();

        // Start dnsproxy
        string dnsproxyArgs = "-l 0.0.0.0";

        // Add Legacy DNS args
        dnsproxyArgs += " -p 53";

        // Add DoH args
        if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
        {
            if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
            {
                // Get DoH Port
                int dohPort = GetDohPortSetting();

                // Set Connected DoH Port
                ConnectedDohPort = dohPort;

                dnsproxyArgs += " --https-port=" + dohPort + " --tls-crt=\"" + SecureDNS.CertPath + "\" --tls-key=\"" + SecureDNS.KeyPath + "\"";
            }
        }

        // Add Cache args
        if (CustomCheckBoxSettingEnableCache.Checked)
            dnsproxyArgs += " --cache";

        // Add upstream args
        dnsproxyArgs += $" -u {dohUrl}";

        // Get Bootstrap IP & Port
        string bootstrap = GetBootstrapSetting(out int bootstrapPort).ToString();

        // Add bootstrap args
        dnsproxyArgs += $" -b {bootstrap}:{bootstrapPort}";

        if (IsDisconnecting) return false;

        // Execute DNSProxy
        PIDDNSProxyBypass = ProcessManager.ExecuteOnly(out Process _, SecureDNS.DnsProxy, dnsproxyArgs, true, true, SecureDNS.CurrentPath, GetCPUPriority());

        // Wait for DNSProxy
        Task wait1 = Task.Run(async () =>
        {
            while (true)
            {
                if (IsDisconnecting) break;
                if (ProcessManager.FindProcessByPID(PIDDNSProxyBypass)) break;
                await Task.Delay(100);
            }
        });
        try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

        if (ProcessManager.FindProcessByPID(PIDDNSProxyBypass))
        {
            // Set domain to check
            string domainToCheck = "google.com";

            // Check DNS
            checkDns.CheckDNS(domainToCheck, dohUrl, timeoutMS * 10);

            if (checkDns.IsDnsOnline)
            {
                if (IsDisconnecting) return false;

                // Connected
                string msgConnected = $"Connected to {dohHost}.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnected, Color.MediumSeaGreen));

                // Write delay to log
                string msgDelay1 = "Server delay: ";
                string msgDelay2 = $" ms.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay1, Color.Orange));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(checkDns.DnsLatency.ToString(), Color.DodgerBlue));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay2, Color.Orange));

                return true;
            }
            else
            {
                if (IsDisconnecting) return false;

                // Couldn't connect normally!
                string connectNormallyFailed = $"Couldn't connect. It's really weird!{NL}";
                if (IsDisconnecting) connectNormallyFailed = $"Task Canceled.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(connectNormallyFailed, Color.IndianRed));

                // Kill DNSProxy
                ProcessManager.KillProcessByPID(PIDDNSProxyBypass);
                return false;
            }
        }
        else
        {
            // DNSProxy failed to execute
            string msgDNSProxyFailed = $"DNSProxy failed to execute. Try again.{NL}";
            if (IsDisconnecting) msgDNSProxyFailed = $"Task Canceled.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDNSProxyFailed, Color.IndianRed));
            return false;
        }
    }
}