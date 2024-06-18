using MsmhToolsClass;
using System.Diagnostics;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task<bool> ConnectToServersUsingProxy()
    {
        string proxyScheme = CustomTextBoxHTTPProxy.Text.ToLower().Trim();

        // Check if proxy scheme is correct
        if (string.IsNullOrEmpty(proxyScheme) || (!proxyScheme.StartsWith("http://") && !proxyScheme.StartsWith("socks5://")))
        {
            string msgWrongProxy = "Proxy scheme must be like: \"http://myproxy.net:8080\" or \"socks5://myproxy.net:8080\"";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgWrongProxy + NL, Color.IndianRed));
            return false;
        }

        // Get Host and Port of Proxy
        NetworkTool.GetUrlDetails(proxyScheme, 0, out _, out string host, out _, out _, out int port, out string _, out bool _);

        // Check if proxy works
        string sampleRequest = "https://dns.google/dns-query?dns=AAABAAABAAAAAAABCGdvb2dsZQNjb20AAAEAAQ";
        string headers = await NetworkTool.GetHeadersAsync(sampleRequest, null, 5000, false, proxyScheme);
        string[] header = headers.Split(NL, StringSplitOptions.RemoveEmptyEntries);
        if (header.Length > 0) headers = header[0];
        Color statusColor = headers.Contains("OK") ? Color.MediumSeaGreen : Color.IndianRed;
        if (!string.IsNullOrEmpty(headers))
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Proxy Status: {headers}{NL}", statusColor));
        if (statusColor.Equals(Color.IndianRed))
        {
            string msgProxy = $"Your Upstream Proxy Doesn't Work.";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy + NL, Color.IndianRed));
            return false;
        }

        // DNSs
        string dnss = "https://dns.google/dns-query,https://dns.cloudflare.com/dns-query";

        // Local DNS Port
        int dnsPort = 53;

        // Get Insecure
        bool insecure = CustomCheckBoxInsecure.Checked;

        // Get Cloudflare Clean IPv4
        string cfCleanIPv4 = GetCfCleanIpSetting();

        // Get Bootstrap IP & Port
        string bootstrapIP = GetBootstrapSetting(out int bootstrapPort).ToString();

        // Kill DnsServer If It's Already Running
        await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
        bool isCmdSent = false;
        PIDDnsServer = DnsConsole.Execute(SecureDNS.AgnosticServerPath, null, true, true, SecureDNS.CurrentPath, GetCPUPriority());
        await Task.Delay(50);

        // Wait For DNS Server
        Task wait1 = Task.Run(async () =>
        {
            while (true)
            {
                if (IsDisconnecting) break;
                if (ProcessManager.FindProcessByPID(PIDDnsServer)) break;
                await Task.Delay(50);
            }
        });
        try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

        if (!ProcessManager.FindProcessByPID(PIDDnsServer))
        {
            string msg = $"Couldn't Start DNS Server. Try Again.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
            return false;
        }

        // Update IsConnected Bool
        IsConnected = true;

        // Send DNS Profile
        string command = "Profile UpstreamProxy_DNS";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Send DNS Settings
        command = $"Setting -Port={dnsPort} -WorkingMode=Dns -MaxRequests=1000000 -DnsTimeoutSec=10 -KillOnCpuUsage=40";
        command += $" -AllowInsecure={insecure} -DNSs={dnss} -CfCleanIP={cfCleanIPv4}";
        command += $" -BootstrapIp={bootstrapIP} -BootstrapPort={bootstrapPort}";
        command += $" -ProxyScheme={proxyScheme}";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Send DNS Rules
        if (CustomCheckBoxSettingDnsEnableRules.Checked)
        {
            string dnsRulesCmd = $"Programs DnsRules -Mode=File -PathOrText=\"{SecureDNS.DnsRulesPath}\"";
            isCmdSent = await DnsConsole.SendCommandAsync(dnsRulesCmd);
            if (!isCmdSent) return false;
        }

        // DoH Server
        if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
        {
            if (File.Exists(SecureDNS.IssuerCertPath) && File.Exists(SecureDNS.IssuerKeyPath) && File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
            {
                // Get DoH Port
                int dohPort = GetDohPortSetting();

                // Set Connected DoH Port
                ConnectedDohPort = dohPort;

                // Send DoH Profile
                command = "Profile UpstreamProxy_DoH";
                isCmdSent = await DnsConsole.SendCommandAsync(command);
                if (!isCmdSent) return false;

                // Send DoH Settings
                command = $"Setting -Port={dohPort} -WorkingMode=Dns -MaxRequests=1000000 -DnsTimeoutSec=10 -KillOnCpuUsage=40";
                command += $" -AllowInsecure={insecure} -DNSs={dnss} -CfCleanIP={cfCleanIPv4}";
                command += $" -BootstrapIp={bootstrapIP} -BootstrapPort={bootstrapPort}";
                command += $" -ProxyScheme={proxyScheme}";
                isCmdSent = await DnsConsole.SendCommandAsync(command);
                if (!isCmdSent) return false;

                // Send SSL Settings
                command = $"SSLSetting -Enable=True -RootCA_Path=\"{SecureDNS.IssuerCertPath}\" -RootCA_KeyPath=\"{SecureDNS.IssuerKeyPath}\" -Cert_Path=\"{SecureDNS.CertPath}\" -Cert_KeyPath=\"{SecureDNS.KeyPath}\"";
                isCmdSent = await DnsConsole.SendCommandAsync(command);
                if (!isCmdSent) return false;

                // Send DNS Rules
                if (CustomCheckBoxSettingDnsEnableRules.Checked)
                {
                    string dnsRulesCmd = $"Programs DnsRules -Mode=File -PathOrText=\"{SecureDNS.DnsRulesPath}\"";
                    isCmdSent = await DnsConsole.SendCommandAsync(dnsRulesCmd);
                    if (!isCmdSent) return false;
                }
            }
        }

        // Send Write Requests To Log
        isCmdSent = await DnsConsole.SendCommandAsync("Requests True");
        if (!isCmdSent) return false;

        // Send Parent Process Command
        command = $"ParentProcess -PID={Environment.ProcessId}";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Send Start Command
        isCmdSent = await DnsConsole.SendCommandAsync("Start");
        if (!isCmdSent) return false;

        // Wait For Confirm Msg
        bool confirmed = false;
        Task wait2 = Task.Run(async () =>
        {
            while (true)
            {
                if (IsDisconnecting) break;
                if (!ProcessManager.FindProcessByPID(PIDDnsServer)) break;
                if (DnsConsole.GetStdoutBag.Contains("Confirmed"))
                {
                    confirmed = true;
                    break;
                }
                await Task.Delay(25);
            }
        });
        try { await wait2.WaitAsync(TimeSpan.FromSeconds(30)); } catch (Exception) { }

        GetBlockedDomainSetting(out string blockedDomainNoWww);
        if (IsConnected && confirmed)
        {
            string msgSuccess = $"DNS Server Executed.{NL}Waiting For DNS To Get Online...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgSuccess, Color.MediumSeaGreen));

            // Wait Until DNS Gets Online
            int timeoutMS = 1000;
            Stopwatch sw = Stopwatch.StartNew();
            Task wait3 = Task.Run(async () =>
            {
                while (!IsDNSConnected)
                {
                    if (IsDisconnecting) break;
                    if (IsDNSConnected) break;
                    if (!ProcessManager.FindProcessByPID(PIDDnsServer)) break;
                    if (!IsInternetOnline) break;

                    List<int> pids = ProcessManager.GetProcessPidsByUsingPort(53);
                    foreach (int pid in pids) if (pid != PIDDnsServer) await ProcessManager.KillProcessByPidAsync(pid);

                    await UpdateBoolDnsOnceAsync(timeoutMS, blockedDomainNoWww);
                    await Task.Delay(25);
                    timeoutMS += 500;
                    if (timeoutMS > 10000) timeoutMS = 10000;
                }
            });
            try { await wait3.WaitAsync(TimeSpan.FromSeconds(30)); } catch (Exception) { }
            sw.Stop();

            if (IsDNSConnected)
            {
                msgSuccess = $"Successfully Got Online In {Math.Round(sw.Elapsed.TotalSeconds, 2)} Sec.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgSuccess, Color.MediumSeaGreen));
                await LogToDebugFileAsync(msgSuccess);

                return true;
            }
        }

        string msgFailed = "Error: Couldn't Start DNS Server!";
        if (!confirmed) msgFailed = "Couldn't Get Confirm Message From Console.";
        else if (ProcessManager.FindProcessByPID(PIDDnsServer) && !IsDNSConnected)
            msgFailed = $"DNS Can't Get Online (Check Domain: {blockedDomainNoWww}).";
        if (IsDisconnecting) msgFailed = "Task Canceled.";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailed + NL, Color.IndianRed));
        await LogToDebugFileAsync(msgFailed);

        await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
        return false;
    }

}