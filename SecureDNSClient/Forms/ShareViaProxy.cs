using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    // ========================================== Start Proxy
    private async Task StartProxyAsync(bool stop = false, bool limitLog = false)
    {
        await Task.Run(async () =>
        {
            if (!IsProxyActivated && !IsProxyRunning)
            {
                // Start Proxy
                if (stop) return;

                this.InvokeIt(() => CustomButtonShare.Enabled = false);

                // Return if binary files are missing
                if (!CheckNecessaryFiles())
                {
                    this.InvokeIt(() => CustomButtonShare.Enabled = true);
                    return;
                }

                if (IsProxyActivating || IsProxyDeactivating) return;

                UpdateProxyBools = false;
                IsProxyActivating = true;
                await UpdateStatusShortOnBoolsChangedAsync();

                // Start Proxy Server
                string msgStart = $"Starting Proxy Server...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStart, Color.MediumSeaGreen));

                // Check Port
                int proxyPort = GetProxyPortSetting();
                bool isPortOk = await GetListeningPortAsync(proxyPort, "Change Proxy Server Port From Settings.", Color.IndianRed);
                if (!isPortOk)
                {
                    UpdateProxyBools = true;
                    IsProxyActivating = false;
                    await UpdateStatusShortOnBoolsChangedAsync();
                    return;
                }

                // Get MaxRequests
                int maxRequests = GetMaxRequestsSetting();

                // Get Proxy Timeout (Sec)
                int proxyTimeoutSec = GetProxyTimeoutSetting();

                // Get KillOnCpuUsage Setting
                int killOnCpuUsage = GetKillOnCpuUsageSetting();

                // Get Block Port 80 Setting
                bool blockPort80 = GetBlockPort80Setting();

                // Get DNSs
                bool isIPv6SupportedByOS = NetworkTool.IsIPv6SupportedByOS();
                string dnss = isIPv6SupportedByOS ? $"udp://[{IPAddress.IPv6Loopback}]:53" : $"udp://{IPAddress.Loopback}:53";
                dnss += ",system";

                // Get Cloudflare Clean IPv4
                string cfCleanIP = GetCfCleanIpSetting();

                // Get Bootstrap IP And Port
                string bootstrapIp = GetBootstrapSetting(out int bootstrapPort).ToString();

                // Get UpStream Proxy
                var upstream = await GetUpStreamProxySettingAsync();
                if (!upstream.IsSuccess)
                {
                    UpdateProxyBools = true;
                    IsProxyActivating = false;
                    await UpdateStatusShortOnBoolsChangedAsync();
                    return;
                }

                // Kill If It's Already Running
                await ProcessManager.KillProcessByPidAsync(PIDProxyServer);
                await Task.Delay(50);
                bool isCmdSent = false;
                int consoleDelayMs = 50, consoleTimeoutSec = 15;
                PIDProxyServer = ProxyConsole.Execute(SecureDNS.AgnosticServerPath, null, true, true, SecureDNS.CurrentPath, GetCPUPriority());
                await Task.Delay(100);

                // Wait For Proxy Server
                Task wait1 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (ProcessManager.FindProcessByPID(PIDProxyServer)) break;
                        await Task.Delay(100);
                    }
                });
                try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                if (!ProcessManager.FindProcessByPID(PIDProxyServer))
                {
                    string msg = $"Couldn't Start Proxy Server. Try Again.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    await ProcessManager.KillProcessByPidAsync(PIDProxyServer);
                    UpdateProxyBools = true;
                    IsProxyActivating = false;
                    await UpdateStatusShortOnBoolsChangedAsync();
                    return;
                }
                
                // Send Set Profile
                string setProfileCommand = "Profile Proxy";
                isCmdSent = await ProxyConsole.SendCommandAsync(setProfileCommand, consoleDelayMs, consoleTimeoutSec, "Confirmed: Profile");
                if (!isCmdSent)
                {
                    await FaildSendCommandMessageAsync(setProfileCommand);
                    return;
                }

                // Send Settings
                string settingsCmd = $"Setting -Port={proxyPort} -WorkingMode=Proxy -MaxRequests={maxRequests} -DnsTimeoutSec=5 -ProxyTimeoutSec={proxyTimeoutSec}";
                settingsCmd += $" -KillOnCpuUsage={killOnCpuUsage} -BlockPort80={blockPort80} -AllowInsecure=True -DNSs=\"{dnss}\" -CfCleanIP={cfCleanIP}";
                settingsCmd += $" -BootstrapIp={bootstrapIp} -BootstrapPort={bootstrapPort}";
                settingsCmd += $" -ProxyScheme={upstream.ProxyScheme} -ProxyUser={upstream.ProxyUser} -ProxyPass={upstream.ProxyPass} -OnlyBlockedIPs={upstream.OnlyBlockedIPs}";
                isCmdSent = await ProxyConsole.SendCommandAsync(settingsCmd, consoleDelayMs, consoleTimeoutSec, "Confirmed: Setting");
                if (!isCmdSent)
                {
                    await FaildSendCommandMessageAsync("Send Settings");
                    return;
                }

                // Send SSL Settings (HTTPS) / SSL Decryption / Fragment
                bool isDpiBypassApplied = await ApplyProxyDpiChangesAsync();
                if (!isDpiBypassApplied)
                {
                    await FaildSendCommandMessageAsync("ApplyProxyDpiChanges");
                    return;
                }

                // Send Rules
                bool isRulesOk = await ApplyRulesToProxyAsync();
                if (!isRulesOk)
                {
                    await FaildSendCommandMessageAsync("ApplyRulesToProxy");
                    return;
                }

                // Send Write Requests To Log
                if (CustomCheckBoxProxyEventShowRequest.Checked)
                {
                    string showRequestsCommand = "Requests True";
                    isCmdSent = await ProxyConsole.SendCommandAsync(showRequestsCommand, consoleDelayMs, consoleTimeoutSec, "Confirmed: Requests True");
                    if (!isCmdSent)
                    {
                        await FaildSendCommandMessageAsync(showRequestsCommand);
                        return;
                    }
                }
                else
                {
                    string showRequestsCommand = "Requests False";
                    isCmdSent = await ProxyConsole.SendCommandAsync(showRequestsCommand, consoleDelayMs, consoleTimeoutSec, "Confirmed: Requests False");
                    if (!isCmdSent)
                    {
                        await FaildSendCommandMessageAsync(showRequestsCommand);
                        return;
                    }
                }

                // Send Write Fragment Details To Log
                if (CustomCheckBoxProxyEventShowChunkDetails.Checked)
                {
                    string fragmentDetailsCommand = "FragmentDetails True";
                    isCmdSent = await ProxyConsole.SendCommandAsync(fragmentDetailsCommand, consoleDelayMs, consoleTimeoutSec, "Confirmed: FragmentDetails True");
                    if (!isCmdSent)
                    {
                        await FaildSendCommandMessageAsync(fragmentDetailsCommand);
                        return;
                    }
                }
                else
                {
                    string fragmentDetailsCommand = "FragmentDetails False";
                    isCmdSent = await ProxyConsole.SendCommandAsync(fragmentDetailsCommand, consoleDelayMs, consoleTimeoutSec, "Confirmed: FragmentDetails False");
                    if (!isCmdSent)
                    {
                        await FaildSendCommandMessageAsync(fragmentDetailsCommand);
                        return;
                    }
                }

                // Send Parent Process Command
                string parentCommand = $"ParentProcess -PID={Environment.ProcessId}";
                Debug.WriteLine(parentCommand);
                isCmdSent = await ProxyConsole.SendCommandAsync(parentCommand, consoleDelayMs, consoleTimeoutSec, "Confirmed: ParentProcess");
                if (!isCmdSent)
                {
                    await FaildSendCommandMessageAsync(parentCommand);
                    return;
                }

                // Send Start Command
                await ProxyConsole.SendCommandAsync("Start", consoleDelayMs, 30, "Confirmed: Start");
                await Task.Delay(200);

                // Check For Successfull Communication With Console
                string outCommand = "Out Proxy";
                isCmdSent = await ProxyConsole.SendCommandAsync(outCommand); // Out <ProfileName>
                string confirmMsg = "details|true";
                
                // Wait For Confirm Message
                Task result = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (!isCmdSent) break;
                        if (ProxyConsole.GetStdout.ToLower().StartsWith(confirmMsg)) break;
                        await Task.Delay(500);
                    }
                });
                try { await result.WaitAsync(TimeSpan.FromSeconds(10)); } catch (Exception) { }
                
                if (!isCmdSent || !ProxyConsole.GetStdout.ToLower().StartsWith(confirmMsg))
                {
                    await FaildSendCommandMessageAsync(outCommand);
                    return;
                }

                // Update Proxy Port
                ProxyPort = GetProxyPortSetting();

                // Write Sharing Addresses To Log
                if (!limitLog)
                {
                    LocalIP = NetworkTool.GetLocalIPv4(); // Update Local IP

                    string msgProxy0 = $"Local Proxy Server (HTTP, HTTPS, SOCKS4, SOCKS4A, SOCKS5):{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy0, Color.LightGray));

                    string msgProxy1 = NetworkTool.IpToUrl(string.Empty, IPAddress.Loopback, proxyPort, string.Empty);
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy1 + NL, Color.DodgerBlue));

                    if (LocalIP != null)
                    {
                        string msgProxy2 = NetworkTool.IpToUrl(string.Empty, LocalIP, proxyPort, string.Empty);
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy2 + NL, Color.DodgerBlue));
                    }
                    
                    if (isIPv6SupportedByOS)
                    {
                        string msgProxy3 = NetworkTool.IpToUrl(string.Empty, IPAddress.IPv6Loopback, proxyPort, string.Empty);
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy3 + NL, Color.DodgerBlue));

                        IPAddress? localIPv6 = NetworkTool.GetLocalIPv6();
                        if (localIPv6 != null)
                        {
                            string msgProxy4 = NetworkTool.IpToUrl(string.Empty, localIPv6, proxyPort, string.Empty);
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy4 + NL, Color.DodgerBlue));
                        }
                    }
                }
                
                // Update Bools
                IsProxyRunning = true;
                UpdateProxyBools = true;
                IsProxyActivating = false;
                await UpdateStatusShortOnBoolsChangedAsync();

                // To See Status Immediately
                await UpdateBoolProxyAsync();
                IsDPIActive = UpdateBoolIsDpiActive();
                await UpdateStatusLongAsync();

                // Warm Up Proxy (Sync)
                string blockedDomain = GetBlockedDomainSetting(out string _);
                string proxyScheme = $"socks5://{IPAddress.Loopback}:{ProxyPort}";
                if (isIPv6SupportedByOS) proxyScheme = $"socks5://[{IPAddress.IPv6Loopback}]:{ProxyPort}";
                if (IsDNSSet)
                {
                    _ = Task.Run(() => WarmUpProxyAsync(blockedDomain, proxyScheme, 30, CancellationToken.None));
                    _ = Task.Run(() => WarmUpProxyAsync("www.twitch.tv", proxyScheme, 30, CancellationToken.None));
                }
            }
            else
            {
                // Stop Proxy
                if (IsProxyActivating || IsProxyDeactivating) return;
                IsProxyDeactivating = true;
                await UpdateStatusShortOnBoolsChangedAsync();

                // Stop Proxy Server
                if (ProcessManager.FindProcessByPID(PIDProxyServer))
                {
                    // Unset Proxy First
                    if (IsProxySet) await SetProxyAsync(true);
                    await Task.Delay(100);
                    if (IsProxySet) NetworkTool.UnsetProxy(false, true);

                    await ProcessManager.KillProcessByPidAsync(PIDProxyServer);

                    // Wait for Proxy Server to Exit
                    Task wait1 = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (!ProcessManager.FindProcessByPID(PIDProxyServer)) break;
                            await Task.Delay(100);
                        }
                    });
                    try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                    if (!ProcessManager.FindProcessByPID(PIDProxyServer))
                    {
                        // Update Bool
                        IsProxyRunning = false;
                        await UpdateStatusShortOnBoolsChangedAsync();

                        // Clear LastProxyRulesPath
                        LastRulesPath = string.Empty;

                        // Write deactivated message to log
                        string msgDiactivated = $"Proxy Server Deactivated.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDiactivated, Color.MediumSeaGreen));

                        // Stop Delay Timer
                        StopWatchWriteProxyOutputDelay.Reset();

                        PIDProxyServer = -1;
                    }
                    else
                    {
                        // Couldn't Stop
                        string msg = $"Couldn't Stop Proxy Server.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    }

                    // To See Status Immediately
                    await UpdateBoolProxyAsync();
                    IsDPIActive = UpdateBoolIsDpiActive();
                    await UpdateStatusLongAsync();
                }
                else
                {
                    // Already Deactivated
                    string msg = $"It's Already Deactivated.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                }

                IsProxyDeactivating = false;
                await UpdateStatusShortOnBoolsChangedAsync();
            }
        });
    }

    // ========================================== Other Proxy Methods

    public async Task FaildSendCommandMessageAsync(string command)
    {
        if (!IsDisconnectingAll && !IsQuickConnecting)
        {
            string msg = $"Couldn't Communicate With Proxy Console.{NL}";
            msg += $"Command: {command}{NL}";
            msg += $"1. Please Make Sure You Have .NET Desktop v6 And ASP.NET Core v6 Installed On Your OS.{NL}";
            msg += $"2. White List \"{SecureDNS.AgnosticServerPath}\" In Your Anti-Virus Or Any Security Software You Have.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
        }

        await ProcessManager.KillProcessByPidAsync(PIDProxyServer);
        UpdateProxyBools = true;
        IsProxyActivating = false;
        await UpdateStatusShortOnBoolsChangedAsync();
    }

    private async Task<bool> ApplyProxyDpiChangesAsync()
    {
        // Return if DNS is setting or unsetting
        if (IsDNSSetting || IsDNSUnsetting) return false;

        UpdateProxyBools = false;
        string command = GetFragmentProgramCommand();
        Debug.WriteLine(command);
        bool isCommandSent = await ProxyConsole.SendCommandAsync(command, 50, 15, "Confirmed: Fragment");
        
        // Save LastFragmentProgramCommand
        if (isCommandSent) LastFragmentProgramCommand = command;

        // Apply SSL Decryption
        bool isSSLDecryptionOk = await ApplyProxySSLDecryptionAsync();
        
        UpdateProxyBools = true;
        if (!isCommandSent || !isSSLDecryptionOk) return false;

        return true;
    }

    private string GetFragmentProgramCommand()
    {
        ApplyProxyDpiFragmentChangesOut(out AgnosticProgram.Fragment.Mode fragmentMode, out int beforSniChunks, out AgnosticProgram.Fragment.ChunkMode chunkMode, out int sniChunks, out int antiPatternOffset, out int fragmentDelay);
        string command = $"Programs Fragment -Mode={fragmentMode} -BeforeSniChunks={beforSniChunks} -ChunkMode={chunkMode} -SniChunks={sniChunks} -AntiPatternOffset={antiPatternOffset} -FragmentDelay={fragmentDelay}";
        return command;
    }

    private void ApplyProxyDpiFragmentChangesOut(out AgnosticProgram.Fragment.Mode fragmentMode, out int beforSniChunks, out AgnosticProgram.Fragment.ChunkMode chunkMode, out int sniChunks, out int antiPatternOffset, out int fragmentDelay)
    {
        // Get Fragment AgnosticSettings
        bool enableFragment = false;
        int beforSniChunks0 = -1, chunkModeInt = -1;
        this.InvokeIt(() =>
        {
            enableFragment = CustomCheckBoxPDpiEnableFragment.Checked;
            beforSniChunks0 = Convert.ToInt32(CustomNumericUpDownPDpiBeforeSniChunks.Value);
            chunkModeInt = CustomComboBoxPDpiSniChunkMode.SelectedIndex;
        });
        beforSniChunks = beforSniChunks0;

        chunkMode = chunkModeInt switch
        {
            0 => AgnosticProgram.Fragment.ChunkMode.SNI,
            1 => AgnosticProgram.Fragment.ChunkMode.SniExtension,
            2 => AgnosticProgram.Fragment.ChunkMode.AllExtensions,
            _ => AgnosticProgram.Fragment.ChunkMode.SNI,
        };

        int sniChunks0 = -1, antiPatternOffset0 = -1, fragmentDelay0 = -1;
        this.InvokeIt(() =>
        {
            sniChunks0 = Convert.ToInt32(CustomNumericUpDownPDpiSniChunks.Value);
            antiPatternOffset0 = Convert.ToInt32(CustomNumericUpDownPDpiAntiPatternOffset.Value);
            fragmentDelay0 = Convert.ToInt32(CustomNumericUpDownPDpiFragDelay.Value);
        });
        sniChunks = sniChunks0; antiPatternOffset = antiPatternOffset0; fragmentDelay = fragmentDelay0;

        fragmentMode = enableFragment ? AgnosticProgram.Fragment.Mode.Program : AgnosticProgram.Fragment.Mode.Disable;
    }

    public async Task<bool> ApplyProxySSLDecryptionAsync()
    {
        bool changeSni = false;
        this.InvokeIt(() => changeSni = CustomCheckBoxProxySSLChangeSni.Checked);

        if (IsSSLDecryptionEnable())
        {
            // Get Default SNI
            string defaultSni = GetDefaultSniSetting();

            string command = $"SSLSetting -Enable=True -RootCA_Path=\"{SecureDNS.IssuerCertPath}\" -RootCA_KeyPath=\"{SecureDNS.IssuerKeyPath}\" -Cert_Path=\"{SecureDNS.CertPath}\" -Cert_KeyPath=\"{SecureDNS.KeyPath}\" -ChangeSni={changeSni} -DefaultSni={defaultSni}";

            Debug.WriteLine(command);
            bool isSuccess = await ProxyConsole.SendCommandAsync(command, 50, 15, "Confirmed: SSLSetting");

            if (isSuccess) LastDefaultSni = defaultSni;
            return isSuccess;
        }
        else
        {
            string command = $"SSLSetting -Enable=False";
            Debug.WriteLine(command);
            return await ProxyConsole.SendCommandAsync(command, 50, 15, "Confirmed: SSLSetting");
        }
    }

    public async Task<bool> ApplyRulesToProxyAsync()
    {
        try
        {
            List<string> rulesList = new();

            bool customRules = false;
            bool ir_ADS = false, ir_Domains = false, ir_CIDRs = false;

            this.InvokeIt(() =>
            {
                customRules = CustomCheckBoxSettingEnableRules.Checked;
                ir_ADS = CustomCheckBoxGeoAsset_IR_ADS.Checked;
                ir_Domains = CustomCheckBoxGeoAsset_IR_Domains.Checked;
                ir_CIDRs = CustomCheckBoxGeoAsset_IR_CIDRs.Checked;
            });

            if (customRules && File.Exists(SecureDNS.RulesPath))
            {
                List<string> customRulesList = new();
                await customRulesList.LoadFromFileAsync(SecureDNS.RulesPath, true, true);
                if (customRulesList.Count > 0) rulesList.AddRange(customRulesList);
            }

            // IR ADS Domains: Block
            if (ir_ADS && File.Exists(SecureDNS.Asset_IR_ADS_Domains))
            {
                string rule = $"{SecureDNS.Asset_IR_ADS_Domains}|-;";
                rulesList.Add(rule);
            }

            // IR Domains: Direct
            if (ir_Domains && File.Exists(SecureDNS.Asset_IR_Domains))
            {
                string rule = $"{SecureDNS.Asset_IR_Domains}|--;";
                rulesList.Add(rule);
                rulesList.Add("*.ir|--;");
            }

            // IR CIDRs: Direct
            if (ir_CIDRs && File.Exists(SecureDNS.Asset_IR_CIDRs))
            {
                string rule = $"{SecureDNS.Asset_IR_CIDRs}|--;";
                rulesList.Add(rule);
            }

            if (rulesList.Count > 0)
            {
                // Save To Temp File
                await rulesList.SaveToFileAsync(SecureDNS.Rules_Assets_Proxy);

                // Send Command
                string command = $"Programs Rules -Mode=File -PathOrText=\"{SecureDNS.Rules_Assets_Proxy}\"";
                Debug.WriteLine(command);
                bool success = await ProxyConsole.SendCommandAsync(command, 50, 30, "Confirmed: Rules");
                if (success) LastRulesPath = SecureDNS.RulesPath;
                return success;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ShareViaProxy ApplyRulesToProxyAsync: " + ex.Message);
            return false;
        }
    }

}