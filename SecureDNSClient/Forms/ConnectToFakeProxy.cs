using MsmhToolsClass;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task<bool> BypassFakeProxyDohStart(ConnectMode connectMode, int camouflagePort)
    {
        // Just in case something left running
        BypassFakeProxyDohStop(true, true, true, false);

        int getNextPort(int currentPort)
        {
            currentPort = NetworkTool.GetNextPort(currentPort);
            if (currentPort == GetProxyPortSetting() || currentPort == GetCamouflageDnsPortSetting())
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
        NetworkTool.GetUrlDetails(dohUrl, 443, out _, out string dohHost, out _, out int _, out string _, out bool _);

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
            return await TryConnectToFakeProxyDohNormally(checkDns, dohUrl, dohHost, timeoutMS);
        }
        else
        {
            if (IsDisconnecting) return false;

            // It's blocked, try to bypass
            if (connectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI)
                return await TryToBypassFakeProxyDohUsingProxyDPIAsync(cleanIP, camouflagePort, timeoutMS);
            else
                return await TryToBypassFakeProxyDohUsingGoodbyeDPIAsync(cleanIP, camouflagePort, timeoutMS);
        }
    }

    private bool CheckBypassWorks(int timeoutMS, int attempts, int pid)
    {
        if (!IsConnected || IsDisconnecting) return false;

        // Get loopback
        string loopback = IPAddress.Loopback.ToString();

        // Get blocked domain
        string blockedDomain = GetBlockedDomainSetting(out string blockedDomainNoWww);
        if (string.IsNullOrEmpty(blockedDomain)) return false;

        // Message
        string msg1 = "Bypassing";
        string msg2 = "...";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg1, Color.MediumSeaGreen));

        // New Check
        CheckDns checkDns = new(false, GetCPUPriority());

        for (int n = 0; n < attempts; n++)
        {
            if (!IsConnected || IsDisconnecting) return false;
            if (!ProcessManager.FindProcessByPID(pid)) return false;

            // Message before
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2, Color.MediumSeaGreen));

            // Delay
            checkDns.CheckDNS(blockedDomainNoWww, loopback, timeoutMS);

            Task.Delay(500).Wait(); // Wait a moment
            if (checkDns.IsDnsOnline)
            {
                // Update bool
                IsConnected = true;
                IsDNSConnected = true;

                // Message add NL on success
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2 + NL, Color.MediumSeaGreen));

                // Write delay to log
                string msgDelay1 = "Server delay: ";
                string msgDelay2 = $" ms.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay1, Color.Orange));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(checkDns.DnsLatency.ToString(), Color.DodgerBlue));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDelay2, Color.Orange));
                return true;
            }

            Task.Delay(500).Wait();
        }

        // Message add NL on failure
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg2 + NL, Color.MediumSeaGreen));

        return false;
    }

    private void BypassFakeProxyDohStop(bool stopCamouflageServer, bool stopDNSProxyOrDNSCrypt, bool stopDPI, bool writeToLog)
    {
        if (stopCamouflageServer && PIDCamouflageProxy != -1)
        {
            ProcessManager.KillProcessByPID(PIDCamouflageProxy);
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