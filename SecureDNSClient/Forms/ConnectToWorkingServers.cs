using MsmhToolsClass;

namespace SecureDNSClient;

public partial class FormMain
{
    private static bool ProcessOutputFilter = false;

    private async Task<bool> ConnectToWorkingServersAsync()
    {
        // Write Check first to log
        if (!WorkingDnsList.Any())
        {
            string msgCheck = "Check Servers First." + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCheck, Color.IndianRed));
            return false;
        }

        string hosts = string.Empty;
        int countUsingServers = 0;

        // Sort by latency
        if (WorkingDnsList.Count > 1)
            WorkingDnsList = WorkingDnsList.OrderBy(x => x.Latency).ToList();

        // Get number of max servers
        int maxServers = GetMaxServersToConnectSetting();

        // New SavedDnsList and SavedEncodedDnsList
        List<string> savedDnsList = new();
        List<string> savedEncodedDnsList = new();

        // Clear CurrentUsingCustomServers
        CurrentUsingCustomServersList.Clear();

        // Find fastest servers, max 10
        for (int n = 0; n < WorkingDnsList.Count; n++)
        {
            if (IsDisconnecting) return false;

            DnsInfo dnsInfo = WorkingDnsList[n];
            string host = dnsInfo.DNS;

            // Add host to UsingDnsList
            savedDnsList.Add(host);

            // Add encoded host to SavedEncodedDnsList
            savedEncodedDnsList.Add(EncodingTool.GetSHA512(host));

            // Update Current Using DNS Servers
            CurrentUsingCustomServersList.Add(dnsInfo);

            hosts += $"{host},";

            countUsingServers = n + 1;
            if (n >= maxServers - 1) break;
        }

        if (hosts.EndsWith(',')) hosts = hosts.TrimEnd(',');

        // Update Saved Dns List
        SavedDnsList = new(savedDnsList);
        SavedEncodedDnsList = new(savedEncodedDnsList);

        // Save encoded hosts to file
        SavedEncodedDnsList.SaveToFile(SecureDNS.SavedEncodedDnsPath);

        return await NormalConnectAsync(hosts, countUsingServers);
    }

    private async Task<bool> NormalConnectAsync(string dnss, int numberOfUsingServers)
    {
        // Local DNS Port
        int dnsPort = 53;

        // Get Insecure
        bool insecure = CustomCheckBoxInsecure.Checked;

        // Get Cloudflare Clean IPv4
        string cfCleanIPv4 = GetCfCleanIpSetting();

        // Get Bootstrap IP & Port
        string bootstrapIP = GetBootstrapSetting(out int bootstrapPort).ToString();

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

        // Send Set Profile
        isCmdSent = await DnsConsole.SendCommandAsync("Profile DNS");
        if (!isCmdSent) return false;

        // Send DNS Settings
        string dnsSettingsCmd = $"Setting -Port={dnsPort} -WorkingMode=Dns -MaxRequests=1000000 -DnsTimeoutSec=10 -KillOnCpuUsage=40";
        dnsSettingsCmd += $" -AllowInsecure={insecure} -DNSs={dnss} -CfCleanIP={cfCleanIPv4}";
        dnsSettingsCmd += $" -BootstrapIp={bootstrapIP} -BootstrapPort={bootstrapPort}";
        isCmdSent = await DnsConsole.SendCommandAsync(dnsSettingsCmd);
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

                // Send Set Profile
                isCmdSent = await DnsConsole.SendCommandAsync("Profile DoH");
                if (!isCmdSent) return false;

                // Send DoH Settings
                string dohSettingsCmd = $"Setting -Port={dohPort} -WorkingMode=Dns -MaxRequests=1000000 -DnsTimeoutSec=10 -KillOnCpuUsage=40";
                dohSettingsCmd += $" -AllowInsecure={insecure} -DNSs={dnss} -CfCleanIP={cfCleanIPv4}";
                dohSettingsCmd += $" -BootstrapIp={bootstrapIP} -BootstrapPort={bootstrapPort}";
                isCmdSent = await DnsConsole.SendCommandAsync(dohSettingsCmd);
                if (!isCmdSent) return false;

                // Send SSL Settings
                string dohSslSettingsCmd = $"SSLSetting -Enable=True -RootCA_Path=\"{SecureDNS.IssuerCertPath}\" -RootCA_KeyPath=\"{SecureDNS.IssuerKeyPath}\" -Cert_Path=\"{SecureDNS.CertPath}\" -Cert_KeyPath=\"{SecureDNS.KeyPath}\"";
                isCmdSent = await DnsConsole.SendCommandAsync(dohSslSettingsCmd);
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
        string parentCommand = $"ParentProcess -PID={Environment.ProcessId}";
        isCmdSent = await DnsConsole.SendCommandAsync(parentCommand);
        if (!isCmdSent) return false;

        // Send Start Command
        isCmdSent = await DnsConsole.SendCommandAsync("Start");
        if (!isCmdSent) return false;

        if (IsConnected)
        {
            string msgSuccess = $"Connected.{NL}Waiting For DNS To Get Online...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgSuccess, Color.MediumSeaGreen));

            // Wait Until DNS Gets Online
            Task wait2 = Task.Run(async () =>
            {
                while (!IsDNSConnected)
                {
                    if (IsDisconnecting) break;
                    if (IsDNSConnected) break;
                    if (!ProcessManager.FindProcessByPID(PIDDnsServer)) break;
                    if (!IsInternetOnline) break;
                    await UpdateBoolDnsOnce(GetCheckTimeoutSetting() + 100, GetBlockedDomainSetting(out string _));
                    await Task.Delay(25);
                }
            });
            try { await wait2.WaitAsync(TimeSpan.FromSeconds(15)); } catch (Exception) { }

            if (IsDNSConnected)
            {
                if (numberOfUsingServers > 1)
                    msgSuccess = "Local DNS Server Started Using " + numberOfUsingServers + " Fastest Servers In Parallel." + NL;
                else
                    msgSuccess = "Local DNS Server Started Using " + numberOfUsingServers + " Server." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgSuccess, Color.MediumSeaGreen));

                return true;
            }
        }

        string msgFailed = "Error: Couldn't Start DNS Server!";
        if (ProcessManager.FindProcessByPID(PIDDnsServer) && !IsDNSConnected)
            msgFailed = "DNS Can't Get Online. Check Servers.";
        if (IsDisconnecting) msgFailed = "Task Canceled.";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailed + NL, Color.IndianRed));

        ProcessManager.KillProcessByPID(PIDDnsServer);
        return false;
    }

}