using MsmhToolsClass;
using System;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private async Task<bool> BypassFakeProxyDohStart(int camouflagePort)
        {
            // Just in case something left running
            BypassFakeProxyDohStop(true, true, true, false);

            int getNextPort(int currentPort)
            {
                currentPort = NetworkTool.GetNextPort(currentPort);
                if (currentPort == GetHTTPProxyPortSetting() || currentPort == GetCamouflageDnsPortSetting())
                    currentPort = NetworkTool.GetNextPort(currentPort);
                return currentPort;
            }

            // Check port
            bool isPortOk = GetListeningPort(camouflagePort, string.Empty, Color.Orange);
            if (!isPortOk)
            {
                camouflagePort = getNextPort(camouflagePort);
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Trying Port {camouflagePort}...{NL}", Color.MediumSeaGreen));
                bool isPort2Ok = GetListeningPort(camouflagePort, string.Empty, Color.Orange);
                if (!isPort2Ok)
                {
                    camouflagePort = getNextPort(camouflagePort);
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Trying Port {camouflagePort}...{NL}", Color.MediumSeaGreen));
                    bool isPort3Ok = GetListeningPort(camouflagePort, "Change Camouflage port from settings.", Color.IndianRed);
                    if (!isPort3Ok)
                    {
                        return false;
                    }
                }
            }

            // Check Clean Ip is Valid
            string cleanIP = CustomTextBoxSettingFakeProxyDohCleanIP.Text;
            bool isValid = NetworkTool.IsIPv4Valid(cleanIP, out IPAddress? _);
            if (!isValid)
            {
                string msg = $"Fake Proxy DoH clean IP is not valid, check Settings.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            // Get Fake Proxy DoH Address
            string dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text;
            NetworkTool.GetUrlDetails(dohUrl, 443, out _, out string dohHost, out int _, out string _, out bool _);

            if (IsDisconnecting) return false;

            // Check Cloudflare message
            string msgCheckCF = $"Checking {dohHost}...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCheckCF, Color.Orange));

            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
            if (string.IsNullOrEmpty(blockedDomain)) return false;

            // Set timeout (ms)
            int timeoutMS = 10000;

            // Check Fake Proxy DoH
            CheckDns checkDns = new(false, GetCPUPriority());
            checkDns.CheckDNS(blockedDomainNoWww, dohUrl, timeoutMS / 2);
            
            if (checkDns.IsDnsOnline)
            {
                // Not blocked, connect normally
                return await connectToFakeProxyDohNormally();
            }
            else
            {
                if (IsDisconnecting) return false;

                // It's blocked, tryn to bypass
                ConnectMode connectMode = GetConnectMode();
                if (connectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI)
                    return await TryToBypassFakeProxyDohUsingProxyDPIAsync(cleanIP, camouflagePort, timeoutMS);
                else
                    return await TryToBypassFakeProxyDohUsingGoodbyeDPIAsync(cleanIP, camouflagePort, timeoutMS);
            }

            async Task<bool> connectToFakeProxyDohNormally()
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

        private void BypassFakeProxyDohStop(bool stopCamouflageServer, bool stopDNSProxyOrDNSCrypt, bool stopDPI, bool writeToLog)
        {
            if (stopCamouflageServer && CamouflageHttpProxyServer != null && CamouflageHttpProxyServer.IsRunning)
            {
                CamouflageHttpProxyServer.Stop();
                IsBypassHttpProxyActive = false;
            }

            if (stopCamouflageServer && CamouflageDNSServer != null && CamouflageDNSServer.IsRunning)
            {
                CamouflageDNSServer.Stop();
                IsBypassDNSActive = false;
            }

            if (stopDNSProxyOrDNSCrypt)
                ProcessManager.KillProcessByPID(PIDDNSCryptBypass);

            if (stopDNSProxyOrDNSCrypt)
                ProcessManager.KillProcessByPID(PIDDNSProxyBypass);

            if (stopDPI)
                ProcessManager.KillProcessByPID(PIDGoodbyeDPIBypass);

            if (writeToLog)
            {
                string msg = $"{NL}Camouflage mode deactivated.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Orange));
            }
        }

    }
}
