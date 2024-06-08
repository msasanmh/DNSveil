using System.Diagnostics;
using System.Net;
using MsmhToolsClass;
using SecureDNSClient.DPIBasic;

namespace SecureDNSClient;

public partial class FormMain
{
    //---------------------------- Connect: Bypass The DoH Via GoodbyeDPI
    public async Task<bool> ConnectToFakeProxyDohUsingGoodbyeDPIAsync()
    {
        if (IsDisconnecting) return false;

        // Get Fake Proxy DoH Address
        string dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text.Trim();
        if (string.IsNullOrEmpty(dohUrl)) return false;

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

        // Kill GoodbyeDPI If It's Already Running
        await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIBypass);

        // Create Blacklist File For GoodbyeDPI
        File.WriteAllText(SecureDNS.DPIBlacklistFPPath, dohHost);

        // Get GoodbyeDPI User Mode
        DPIBasicBypassMode mode = GetGoodbyeDpiModeBasic();

        // Start GoodbyeDPI
        DPIBasicBypass dpiBypass = new(mode, CustomNumericUpDownSSLFragmentSize.Value, IPAddress.Loopback.ToString(), dnsPort);
        string args = $"{dpiBypass.Args} --blacklist \"{SecureDNS.DPIBlacklistFPPath}\"";
        PIDGoodbyeDPIBypass = ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, null, args, true, true, SecureDNS.BinaryDirPath, GetCPUPriority());

        // Wait For GoodbyeDPIBypass
        Task wait0 = Task.Run(async () =>
        {
            while (true)
            {
                if (IsDisconnecting) break;
                if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass)) break;
                await Task.Delay(50);
            }
        });
        try { await wait0.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

        if (!ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass))
        {
            // GoodbyeDPI failed to execute
            string msgGoodbyeDPIFailed = $"GoodbyeDPI Failed To Execute. Try Again.{NL}";
            if (IsDisconnecting) msgGoodbyeDPIFailed = $"Task Canceled.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgGoodbyeDPIFailed, Color.IndianRed));
            return false;
        }

        string gDpiMsg = $"GoodbyeDPI Executed, Mode: {mode}{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(gDpiMsg, Color.Orange));

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
        string command = "Profile BypassWithGoodbyeDPI_DNS";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Send DNS Settings
        command = $"Setting -Port={dnsPort} -WorkingMode=Dns -MaxRequests=1000000 -DnsTimeoutSec=10 -KillOnCpuUsage=40";
        command += $" -AllowInsecure={insecure} -DNSs={dohUrl} -CfCleanIP={cfCleanIPv4}";
        command += $" -BootstrapIp={IPAddress.Loopback} -BootstrapPort={dnsPort}";
        isCmdSent = await DnsConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Send DNS Rules Command
        command = $"Programs DnsRules -Mode=Text -PathOrText=\"{dnsRulesContent}\"";
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
                command = "Profile BypassWithGoodbyeDPI_DoH";
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
                    if (!ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass)) break;
                    if (!IsInternetOnline) break;

                    List<int> pids = ProcessManager.GetProcessPidsByUsingPort(53);
                    foreach (int pid in pids) if (pid != PIDDnsServer) await ProcessManager.KillProcessByPidAsync(pid);

                    await UpdateBoolDnsOnce(timeoutMS, blockedDomainNoWww);
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

        string msgFailed = "Error: Couldn't Start DNS Server!";
        if (!confirmed) msgFailed = "Couldn't Get Confirm Message From Console.";
        else if (ProcessManager.FindProcessByPID(PIDDnsServer) && !IsDNSConnected)
            msgFailed = $"DNS Can't Get Online (Check Domain: {blockedDomainNoWww}). Bypass Failed.";
        if (IsDisconnecting) msgFailed = "Task Canceled.";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailed + NL, Color.IndianRed));
        await LogToDebugFileAsync(msgFailed);

        await ProcessManager.KillProcessByPidAsync(PIDDnsServer);
        await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIBypass);
        return false;
    }
}