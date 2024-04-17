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
        NetworkTool.GetUrlDetails(dohUrl, 443, out _, out string dohHost, out _, out _, out int _, out string _, out bool _);

        // Get DoH Clean IP
        string dohCleanIP = CustomTextBoxSettingFakeProxyDohCleanIP.Text.Trim();
        bool isValid = NetworkTool.IsIPv4Valid(dohCleanIP, out IPAddress? _);
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

        // Get Cloudflare Clean IPv4
        string cfCleanIPv4 = GetCfCleanIpSetting();

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

        // Set DnsRules Content
        string dnsRulesContent = $"{dohHost}|{dohCleanIP};";
        if (CustomCheckBoxSettingDnsEnableRules.Checked)
        {
            try
            {
                string userDnsRules = await File.ReadAllTextAsync(SecureDNS.DnsRulesPath);
                userDnsRules = userDnsRules.ReplaceLineEndings("\\n");
                dnsRulesContent += $"\\n{userDnsRules}";
            }
            catch (Exception) { }
        }

        // Set ProxyRules Content (Apply Fake DNS and White List Program)
        string proxyRulesContent = $"{dohHost}|{dohCleanIP};";
        proxyRulesContent += $"\\n{dohCleanIP}|+;";
        proxyRulesContent += $"\\n{IPAddress.Loopback}|-;"; // Block Loopback IPv4
        proxyRulesContent += $"\\n{IPAddress.IPv6Loopback}|-;"; // Block Loopback IPv6

        string host1 = "dns.cloudflare.com";
        string host2 = "cloudflare-dns.com";
        string host3 = "every1dns.com";

        if (dohHost.Equals(host1) || dohHost.Equals(host2) || dohHost.Equals(host3))
        {
            proxyRulesContent = $"{host1}|{dohCleanIP};";
            proxyRulesContent += $"\\n{host2}|{dohCleanIP};";
            proxyRulesContent += $"\\n{host3}|{dohCleanIP};";
            proxyRulesContent += $"\\n{dohCleanIP}|+;";
            proxyRulesContent += $"\\n{IPAddress.Loopback}|-;"; // Block Loopback IPv4
            proxyRulesContent += $"\\n{IPAddress.IPv6Loopback}|-;"; // Block Loopback IPv6
        }
        
        // Kill If It's Already Running
        ProcessManager.KillProcessByPID(PIDDnsServer);
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
            ProcessManager.KillProcessByPID(PIDDnsServer);
            return false;
        }

        // Update IsConnected Bool
        IsConnected = true;

        // Send DNS Profile
        string command = "Profile BypassWithProxy_DNS";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Send DNS Settings
        command = $"Setting -Port={dnsPort} -WorkingMode=DnsAndProxy -MaxRequests=1000000 -DnsTimeoutSec=10 -ProxyTimeoutSec=40 -KillOnCpuUsage=40 -BlockPort80=True";
        command += $" -AllowInsecure={insecure} -DNSs={dohUrl} -CfCleanIP={cfCleanIPv4}";
        command += $" -BootstrapIp={IPAddress.Loopback} -BootstrapPort={dnsPort}";
        command += $" -ProxyScheme=socks5://{IPAddress.Loopback}:{dnsPort}";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Send Fragment Command
        command = $"Programs Fragment -Mode=Program -BeforeSniChunks={beforSniChunks} -ChunkMode={chunkMode} -SniChunks={sniChunks} -AntiPatternOffset={antiPatternOffset} -FragmentDelay={fragmentDelay}";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Send DNS Rules Command
        command = $"Programs DnsRules -Mode=Text -PathOrText=\"{dnsRulesContent}\"";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Send Proxy Rules Command
        command = $"Programs ProxyRules -Mode=Text -PathOrText=\"{proxyRulesContent}\"";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
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
                isCmdSent = await DnsConsole.SendCommandAsync(command);
                if (!isCmdSent) return false;

                // Send DoH Settings
                command = $"Setting -Port={dohPort} -WorkingMode=Dns -MaxRequests=1000000 -DnsTimeoutSec=10 -KillOnCpuUsage=40";
                command += $" -AllowInsecure={insecure} -DNSs=udp://{IPAddress.Loopback}:{dnsPort}";
                command += $" -BootstrapIp={IPAddress.Loopback} -BootstrapPort={dnsPort}";
                isCmdSent = await DnsConsole.SendCommandAsync(command);
                if (!isCmdSent) return false;

                // Send SSL Settings
                command = $"SSLSetting -Enable=True -RootCA_Path=\"{SecureDNS.IssuerCertPath}\" -RootCA_KeyPath=\"{SecureDNS.IssuerKeyPath}\" -Cert_Path=\"{SecureDNS.CertPath}\" -Cert_KeyPath=\"{SecureDNS.KeyPath}\"";
                isCmdSent = await DnsConsole.SendCommandAsync(command);
                if (!isCmdSent) return false;
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

        if (IsConnected)
        {
            string msgSuccess = $"DNS Server Executed.{NL}Bypassing...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgSuccess, Color.MediumSeaGreen));

            // Wait Until DNS Gets Online
            Stopwatch sw = Stopwatch.StartNew();
            Task wait2 = Task.Run(async () =>
            {
                while (!IsDNSConnected)
                {
                    if (IsDisconnecting) break;
                    if (IsDNSConnected) break;
                    if (!ProcessManager.FindProcessByPID(PIDDnsServer)) break;
                    if (!IsInternetOnline) break;
                    await UpdateBoolDnsOnce(1000, GetBlockedDomainSetting(out string _));
                    await Task.Delay(325);
                }
            });
            try { await wait2.WaitAsync(TimeSpan.FromSeconds(30)); } catch (Exception) { }
            sw.Stop();

            if (IsDNSConnected)
            {
                msgSuccess = $"Successfully Bypassed In {Math.Round(sw.Elapsed.TotalSeconds, 2)} Sec.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgSuccess, Color.MediumSeaGreen));

                return true;
            }
        }

        string msgFailed = "Error: Couldn't Start DNS Server!";
        if (ProcessManager.FindProcessByPID(PIDDnsServer) && !IsDNSConnected)
            msgFailed = "DNS Can't Get Online. Bypass Failed.";
        if (IsDisconnecting) msgFailed = "Task Canceled.";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailed + NL, Color.IndianRed));

        ProcessManager.KillProcessByPID(PIDDnsServer);
        return false;
    }
}