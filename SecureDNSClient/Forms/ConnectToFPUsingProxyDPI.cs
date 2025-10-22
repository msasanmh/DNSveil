using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    //---------------------------- Connect: Bypass The DoH Via Proxy Fragment
    public async Task<bool> ConnectToFakeProxyDohUsingProxyDPIAsync()
    {
        if (IsDisconnecting) return false;

        // Get Fake Proxy DoH Address
        string dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text.Trim();
        NetworkTool.URL urid = NetworkTool.GetUrlOrDomainDetails(dohUrl, 443);
        string dohHost = urid.Host;

        // Get DoH Clean IP
        string dohCleanIP = CustomTextBoxSettingFakeProxyDohCleanIP.Text.Trim();
        bool isValid = NetworkTool.IsIP(dohCleanIP, out IPAddress? _);
        if (!isValid)
        {
            string msg = $"Fake Proxy Clean IP Is Not Valid, Check Settings.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            return false;
        }

        // Connect Message
        string msgBlocked = $"Connecting To {dohHost.CapitalizeFirstLetter()}{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBlocked, Color.Orange));

        // Local DNS Port
        int dnsPort = 53;

        // Get Insecure
        bool insecure = CustomCheckBoxInsecure.Checked;

        // Get Cloudflare Clean IP
        string cfCleanIP = GetCfCleanIpSetting();

        // Get Fragment Settings
        int beforSniChunks = Convert.ToInt32(CustomNumericUpDownPDpiBeforeSniChunks.Value);
        int chunkModeInt = -1;
        this.InvokeIt(() => chunkModeInt = CustomComboBoxPDpiSniChunkMode.SelectedIndex);
        AgnosticProgram.Fragment.ChunkMode chunkMode = chunkModeInt switch
        {
            0 => AgnosticProgram.Fragment.ChunkMode.SNI,
            1 => AgnosticProgram.Fragment.ChunkMode.SniExtension,
            2 => AgnosticProgram.Fragment.ChunkMode.AllExtensions,
            _ => AgnosticProgram.Fragment.ChunkMode.AllExtensions,
        };

        int sniChunks = Convert.ToInt32(CustomNumericUpDownPDpiSniChunks.Value);
        int antiPatternOffset = Convert.ToInt32(CustomNumericUpDownPDpiAntiPatternOffset.Value);
        int fragmentDelay = Convert.ToInt32(CustomNumericUpDownPDpiFragDelay.Value);

        // Set Rules Content
        string rulesContent = $"{dohHost}|{dohCleanIP};";

        string host1 = "dns.cloudflare.com";
        string host2 = "cloudflare-dns.com";
        string host3 = "every1dns.com";

        if (dohHost.Equals(host1) || dohHost.Equals(host2) || dohHost.Equals(host3))
        {
            rulesContent = $"{host1}|{dohCleanIP};";
            rulesContent += $"{NL}{host2}|{dohCleanIP};";
            rulesContent += $"{NL}{host3}|{dohCleanIP};";
        }

        rulesContent += $"{NL}*|DnsProxy:socks5://{IPAddress.Loopback}:{dnsPort};";

        // Kill If It's Already Running
        await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
        bool isCmdSent = false;
        int consoleDelayMs = 50, consoleTimeoutSec = 15;
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
        string command = "Profile BypassWithProxy_DNS";
        isCmdSent = await DnsConsole.SendCommandAsync(command, consoleDelayMs, consoleTimeoutSec, "Confirmed: Profile");
        if (!isCmdSent) return false;

        // Send DNS Settings
        command = $"Setting -Port={dnsPort} -WorkingMode=DnsAndProxy -MaxRequests=1000000 -DnsTimeoutSec=10 -ProxyTimeoutSec=40 -KillOnCpuUsage=40 -BlockPort80=True";
        command += $" -AllowInsecure={insecure} -DNSs=\"{dohUrl}\" -CfCleanIP={cfCleanIP}";
        command += $" -BootstrapIp={IPAddress.None} -BootstrapPort=53";
        isCmdSent = await DnsConsole.SendCommandAsync(command, consoleDelayMs, consoleTimeoutSec, "Confirmed: Setting");
        if (!isCmdSent) return false;

        // Send Fragment Command
        command = $"Programs Fragment -Mode=Program -BeforeSniChunks={beforSniChunks} -ChunkMode={chunkMode} -SniChunks={sniChunks} -AntiPatternOffset={antiPatternOffset} -FragmentDelay={fragmentDelay}";
        isCmdSent = await DnsConsole.SendCommandAsync(command, consoleDelayMs, consoleTimeoutSec, "Confirmed: Fragment");
        if (!isCmdSent) return false;

        // Send Rules Command
        isCmdSent = await ApplyRulesToDnsAsync(rulesContent);
        if (!isCmdSent) return false;

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
                command = "Profile BypassWithProxy_DoH";
                isCmdSent = await DnsConsole.SendCommandAsync(command, consoleDelayMs, consoleTimeoutSec, "Confirmed: Profile");
                if (!isCmdSent) return false;

                // Send DoH Settings
                command = $"Setting -Port={dohPort} -WorkingMode=Dns -MaxRequests=1000000 -DnsTimeoutSec=10 -KillOnCpuUsage=40";
                command += $" -AllowInsecure={insecure} -DNSs=\"udp://{IPAddress.Loopback}:{dnsPort}\"";
                command += $" -BootstrapIp={IPAddress.Any} -BootstrapPort=53";
                isCmdSent = await DnsConsole.SendCommandAsync(command, consoleDelayMs, consoleTimeoutSec, "Confirmed: Setting");
                if (!isCmdSent) return false;

                // Send SSL Settings
                command = $"SSLSetting -Enable=True -RootCA_Path=\"{SecureDNS.IssuerCertPath}\" -RootCA_KeyPath=\"{SecureDNS.IssuerKeyPath}\" -Cert_Path=\"{SecureDNS.CertPath}\" -Cert_KeyPath=\"{SecureDNS.KeyPath}\"";
                isCmdSent = await DnsConsole.SendCommandAsync(command, consoleDelayMs, consoleTimeoutSec, "Confirmed: SSLSetting");
                if (!isCmdSent) return false;
            }
        }

        // Send Write Requests To Log
        isCmdSent = await DnsConsole.SendCommandAsync("Requests True", consoleDelayMs, consoleTimeoutSec, "Confirmed: Requests True");
        if (!isCmdSent) return false;

        // Send Parent Process Command
        command = $"ParentProcess -PID={Environment.ProcessId}";
        isCmdSent = await DnsConsole.SendCommandAsync(command, consoleDelayMs, consoleTimeoutSec, "Confirmed: ParentProcess");
        if (!isCmdSent) return false;

        // Send Start Command
        isCmdSent = await DnsConsole.SendCommandAsync("Start", consoleDelayMs, 30, "Confirmed: Start");
        if (!isCmdSent) return false;

        GetBlockedDomainSetting(out string blockedDomainNoWww);
        if (IsConnected)
        {
            string msgSuccess = $"DNS Server Executed.{NL}Bypassing...{NL}";
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

                    List<int> pids = await ProcessManager.GetProcessPidsByUsingPortAsync(53);
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
                msgSuccess = $"Successfully Bypassed In {Math.Round(sw.Elapsed.TotalSeconds, 2)} Sec.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgSuccess, Color.MediumSeaGreen));
                await LogToDebugFileAsync(msgSuccess);

                return true;
            }
        }

        string msgFailed = "Couldn't Get Confirm Message From Console.";
        if (!ProcessManager.FindProcessByPID(PIDDnsServer)) msgFailed = "Error: Couldn't Start DNS Server!";
        else if (!IsDNSConnected && ProcessManager.FindProcessByPID(PIDDnsServer))
            msgFailed = $"DNS Can't Get Online (Check Domain: {blockedDomainNoWww}). Bypass Failed.";
        if (IsDisconnecting) msgFailed = "Task Canceled.";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailed + NL, Color.IndianRed));
        await LogToDebugFileAsync(msgFailed);

        await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
        return false;
    }
}