using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    // ========================================== Start Proxy
    private async Task StartProxy(bool stop = false, bool limitLog = false)
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
                bool isPortOk = GetListeningPort(proxyPort, "Change Proxy Server Port From Settings.", Color.IndianRed);
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
                bool isIPv6SupportedByOS = NetworkTool.IsIPv6Supported();
                string dnss = isIPv6SupportedByOS ? $"udp://[{IPAddress.IPv6Loopback}]:53" : $"udp://{IPAddress.Loopback}:53";
                dnss += ",system";

                // Get Cloudflare Clean IPv4
                string cfCleanIPv4 = GetCfCleanIpSetting();

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
                bool isCmdSent = false;
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
                isCmdSent = await ProxyConsole.SendCommandAsync("Profile Proxy");
                if (!isCmdSent)
                {
                    await FaildSendCommandMessageAsync();
                    return;
                }

                // Send Settings
                string settingsCmd = $"Setting -Port={proxyPort} -WorkingMode=DnsAndProxy -MaxRequests={maxRequests} -DnsTimeoutSec=5 -ProxyTimeoutSec={proxyTimeoutSec}";
                settingsCmd += $" -KillOnCpuUsage={killOnCpuUsage} -BlockPort80={blockPort80} -AllowInsecure=True -DNSs={dnss} -CfCleanIP={cfCleanIPv4}";
                settingsCmd += $" -BootstrapIp={bootstrapIp} -BootstrapPort={bootstrapPort}";
                settingsCmd += $" -ProxyScheme={upstream.ProxyScheme} -ProxyUser={upstream.ProxyUser} -ProxyPass={upstream.ProxyPass} -OnlyBlockedIPs={upstream.OnlyBlockedIPs}";
                isCmdSent = await ProxyConsole.SendCommandAsync(settingsCmd);
                if (!isCmdSent)
                {
                    await FaildSendCommandMessageAsync();
                    return;
                }

                // Send SSL Settings (HTTPS) / SSL Decryption / Fragment
                bool isDpiBypassApplied = await ApplyProxyDpiChanges();
                if (!isDpiBypassApplied)
                {
                    await FaildSendCommandMessageAsync();
                    return;
                }

                // Send ProxyRules
                bool isRulesOk = await ApplyProxyRules();
                if (!isRulesOk)
                {
                    await FaildSendCommandMessageAsync();
                    return;
                }

                // Send Write Requests To Log
                if (CustomCheckBoxProxyEventShowRequest.Checked)
                {
                    isCmdSent = await ProxyConsole.SendCommandAsync("Requests True");
                    if (!isCmdSent)
                    {
                        await FaildSendCommandMessageAsync();
                        return;
                    }
                }
                else
                {
                    isCmdSent = await ProxyConsole.SendCommandAsync("Requests False");
                    if (!isCmdSent)
                    {
                        await FaildSendCommandMessageAsync();
                        return;
                    }
                }

                // Send Write Fragment Details To Log
                if (CustomCheckBoxProxyEventShowChunkDetails.Checked)
                {
                    isCmdSent = await ProxyConsole.SendCommandAsync("FragmentDetails True");
                    if (!isCmdSent)
                    {
                        await FaildSendCommandMessageAsync();
                        return;
                    }
                }
                else
                {
                    isCmdSent = await ProxyConsole.SendCommandAsync("FragmentDetails False");
                    if (!isCmdSent)
                    {
                        await FaildSendCommandMessageAsync();
                        return;
                    }
                }

                // Send Parent Process Command
                string parentCommand = $"ParentProcess -PID={Environment.ProcessId}";
                Debug.WriteLine(parentCommand);
                isCmdSent = await ProxyConsole.SendCommandAsync(parentCommand);
                if (!isCmdSent)
                {
                    await FaildSendCommandMessageAsync();
                    return;
                }

                // Send Start Command
                await ProxyConsole.SendCommandAsync("Start");
                await Task.Delay(200);

                // Check For Successfull Communication With Console
                isCmdSent = await ProxyConsole.SendCommandAsync("Out Proxy"); // Out <ProfileName>
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
                    await FaildSendCommandMessageAsync();
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

                    string msgProxy1 = $"{IPAddress.Loopback}:{proxyPort}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy1 + NL, Color.DodgerBlue));

                    if (LocalIP != null)
                    {
                        string msgProxy2 = $"{LocalIP}:{proxyPort}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy2 + NL, Color.DodgerBlue));
                    }
                    
                    if (isIPv6SupportedByOS)
                    {
                        string msgProxy3 = $"[{IPAddress.IPv6Loopback}]:{proxyPort}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy3 + NL, Color.DodgerBlue));

                        IPAddress? localIPv6 = NetworkTool.GetLocalIPv6();
                        if (localIPv6 != null)
                        {
                            string msgProxy4 = $"[{localIPv6}]:{proxyPort}";
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
                        LastProxyRulesPath = string.Empty;

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

    public async Task FaildSendCommandMessageAsync()
    {
        if (!IsDisconnectingAll && !IsQuickConnecting)
        {
            string msg = $"Couldn't Communicate With Proxy Console.{NL}";
            msg += $"Please Make Sure You Have .NET Desktop v6 And ASP.NET Core v6 Installed On Your OS.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
        }

        await ProcessManager.KillProcessByPidAsync(PIDProxyServer);
        UpdateProxyBools = true;
        IsProxyActivating = false;
        await UpdateStatusShortOnBoolsChangedAsync();
    }

    private async Task<bool> ApplyProxyDpiChanges()
    {
        // Return if DNS is setting or unsetting
        if (IsDNSSetting || IsDNSUnsetting) return false;

        UpdateProxyBools = false;
        string command = GetFragmentProgramCommand();
        Debug.WriteLine(command);
        bool isCommandSent = await ProxyConsole.SendCommandAsync(command);
        
        // Save LastFragmentProgramCommand
        if (isCommandSent) LastFragmentProgramCommand = command;

        // Apply SSL Decryption
        bool isSSLDecryptionOk = await ApplyProxySSLDecryption();
        
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
            _ => AgnosticProgram.Fragment.ChunkMode.AllExtensions,
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

    public async Task<bool> ApplyProxySSLDecryption()
    {
        bool changeSni = false;
        this.InvokeIt(() => changeSni = CustomCheckBoxProxySSLChangeSni.Checked);

        if (IsSSLDecryptionEnable())
        {
            // Get Default SNI
            string defaultSni = GetDefaultSniSetting();

            string command = $"SSLSetting -Enable=True -RootCA_Path=\"{SecureDNS.IssuerCertPath}\" -RootCA_KeyPath=\"{SecureDNS.IssuerKeyPath}\" -Cert_Path=\"{SecureDNS.CertPath}\" -Cert_KeyPath=\"{SecureDNS.KeyPath}\" -ChangeSni={changeSni} -DefaultSni={defaultSni}";

            Debug.WriteLine(command);
            bool isSuccess = await ProxyConsole.SendCommandAsync(command);

            if (isSuccess) LastDefaultSni = defaultSni;
            return isSuccess;
        }
        else
        {
            string command = $"SSLSetting -Enable=False";
            Debug.WriteLine(command);
            return await ProxyConsole.SendCommandAsync(command);
        }
    }

    public async Task<bool> ApplyProxyRules()
    {
        if (CustomCheckBoxSettingProxyEnableRules.Checked)
        {
            string command = $"Programs ProxyRules -Mode=File -PathOrText=\"{SecureDNS.ProxyRulesPath}\"";
            Debug.WriteLine(command);
            bool success = await ProxyConsole.SendCommandAsync(command);
            if (success) LastProxyRulesPath = SecureDNS.ProxyRulesPath;
            return success;
        }

        return true;
    }

}