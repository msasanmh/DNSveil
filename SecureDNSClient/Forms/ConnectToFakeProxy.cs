using MsmhTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
                currentPort = Network.GetNextPort(currentPort);
                if (currentPort == GetHTTPProxyPortSetting() || currentPort == GetCamouflageDnsPortSetting())
                    currentPort = Network.GetNextPort(currentPort);
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
            bool isValid = Network.IsIPv4Valid(cleanIP, out IPAddress _);
            if (!isValid)
            {
                string msg = $"Fake Proxy DoH clean IP is not valid, check Settings.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            // Get Fake Proxy DoH Address
            string dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text;
            Network.GetUrlDetails(dohUrl, 443, out string dohHost, out int _, out string _, out bool _);

            if (IsDisconnecting) return false;

            // Check Cloudflare message
            string msgCheckCF = $"Checking {dohHost}...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCheckCF, Color.Orange));

            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
            if (string.IsNullOrEmpty(blockedDomain)) return false;

            // Set timeout (ms)
            int timeoutMS = 5000;

            // Check Fake Proxy DoH
            int latency = SecureDNS.CheckDns(blockedDomainNoWww, dohUrl, timeoutMS, GetCPUPriority());
            bool isCfOpen = latency != -1;
            if (isCfOpen)
            {
                // Not blocked, connect normally
                return connectToFakeProxyDohNormally();
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

            bool connectToFakeProxyDohNormally()
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
                Task.Delay(500).Wait();

                if (ProcessManager.FindProcessByPID(PIDDNSProxyBypass))
                {
                    // Set domain to check
                    string domainToCheck = "google.com";

                    // Delay
                    int latency = SecureDNS.CheckDns(domainToCheck, dohUrl, timeoutMS * 10, GetCPUPriority());
                    bool result = latency != -1;
                    if (result)
                    {
                        if (IsDisconnecting) return false;

                        // Connected
                        string msgConnected = $"Connected to {dohHost}.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnected, Color.MediumSeaGreen));

                        // Write delay to log
                        string msgDelay1 = "Server delay: ";
                        string msgDelay2 = $" ms.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay1, Color.Orange));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(latency.ToString(), Color.DodgerBlue));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay2, Color.Orange));

                        return true;
                    }
                    else
                    {
                        if (IsDisconnecting) return false;

                        // Couldn't connect normally!
                        string connectNormallyFailed = $"Couldn't connect. It's really weird!{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(connectNormallyFailed, Color.IndianRed));

                        // Kill DNSProxy
                        ProcessManager.KillProcessByPID(PIDDNSProxyBypass);
                        return false;
                    }
                }
                else
                {
                    if (IsDisconnecting) return false;

                    // DNSProxy failed to execute
                    string msgDNSProxyFailed = $"DNSProxy failed to execute. Try again.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDNSProxyFailed, Color.IndianRed));
                    return false;
                }
            }
        }

        private void BypassFakeProxyDohStop(bool stopCamouflageServer, bool stopDNSProxyOrDNSCrypt, bool stopDPI, bool writeToLog)
        {
            if (stopCamouflageServer && CamouflageProxyServer != null && CamouflageProxyServer.IsRunning)
            {
                CamouflageProxyServer.Stop();
                IsBypassProxyActive = false;
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
