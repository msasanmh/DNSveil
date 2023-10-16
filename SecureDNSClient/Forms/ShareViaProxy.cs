using MsmhToolsClass;
using MsmhToolsClass.ProxyServerPrograms;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace SecureDNSClient;

public partial class FormMain
{
    /// <summary>
    /// Apply Fake Proxy (Fake DNS and White List)
    /// </summary>
    /// <param name="fakeProcess">Process</param>
    /// <returns>Returns True if everything's ok.</returns>
    public async Task<bool> ApplyFakeProxy(ProcessConsole processConsole)
    {
        bool isOk = ApplyFakeProxyOut(out ProxyProgram.BlackWhiteList? wListProgram, out ProxyProgram.FakeDns? fakeDNSProgram);
        if (!isOk) return false;
        if (wListProgram == null) return false;
        if (fakeDNSProgram == null) return false;

        // Add Programs to Proxy
        string wlCommand = $"Programs BwList -Mode={wListProgram.ListMode} -PathOrText=\"{wListProgram.TextContent.ReplaceLineEndings("\\n")}\"";
        Debug.WriteLine(wlCommand);
        bool isWlSent = await processConsole.SendCommandAsync(wlCommand);

        string fdCommand = $"Programs FakeDns -Mode={fakeDNSProgram.FakeDnsMode} -PathOrText=\"{fakeDNSProgram.TextContent.ReplaceLineEndings("\\n")}\"";
        Debug.WriteLine(fdCommand);
        bool isFdSent = await processConsole.SendCommandAsync(fdCommand);

        return isWlSent && isFdSent;
    }

    public bool ApplyFakeProxyOut(out ProxyProgram.BlackWhiteList? blackWhiteList, out ProxyProgram.FakeDns? fakeDns)
    {
        // Get DoH Clean Ip
        string dohCleanIP = CustomTextBoxSettingFakeProxyDohCleanIP.Text;
        bool isValid = NetworkTool.IsIPv4Valid(dohCleanIP, out IPAddress? _);
        if (!isValid)
        {
            string msg = $"Fake Proxy clean IP is not valid, check Settings.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            blackWhiteList = null;
            fakeDns = null;
            return false;
        }

        // Check IP is Clean
        bool isDohIpClean1 = NetworkTool.CanPing(dohCleanIP, 5000).Result;
        bool isDohIpClean2 = NetworkTool.CanTcpConnect(dohCleanIP, 443, 5000).Result;
        if (!isDohIpClean1 || !isDohIpClean2)
        {
            // CF Clean IP is not valid
            string msg = $"Fake Proxy clean IP is not clean.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            blackWhiteList = null;
            fakeDns = null;
            return false;
        }

        // Get Fake Proxy DoH Address
        string url = CustomTextBoxSettingFakeProxyDohAddress.Text;
        NetworkTool.GetUrlDetails(url, 443, out _, out string host, out _, out int _, out string _, out bool _);
        string host1 = "dns.cloudflare.com";
        string host2 = "cloudflare-dns.com";
        string host3 = "every1dns.com";

        // Set White List and Fake DNS Content (Rules)
        string wlContent = $"{host}\n{dohCleanIP}";
        string fdContent = $"{host}|{dohCleanIP}";

        if (host.Equals(host1) || host.Equals(host2) || host.Equals(host3))
        {
            wlContent = $"{host1}\n{host2}\n{host3}\n{dohCleanIP}";
            fdContent = $"{host1}|{dohCleanIP}\n{host2}|{dohCleanIP}\n{host3}|{dohCleanIP}";
        }

        // White List Program
        ProxyProgram.BlackWhiteList wListProgram = new();
        wListProgram.Set(ProxyProgram.BlackWhiteList.Mode.WhiteListText, wlContent);

        // Fake DNS Program
        ProxyProgram.FakeDns fakeDNSProgram = new();
        fakeDNSProgram.Set(ProxyProgram.FakeDns.Mode.Text, fdContent);

        blackWhiteList = wListProgram;
        fakeDns = fakeDNSProgram;
        return true;
    }

    private async Task StartProxy(bool stop = false)
    {
        await Task.Run(async () =>
        {
            if (!IsProxyActivated && !IsProxyRunning)
            {
                // Start Proxy
                if (stop) return;
                if (IsProxyActivating || IsProxyDeactivating) return;

                if (IsDNSSetting)
                {
                    string msg = $"Let DNS Set.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Orange));
                    return;
                }

                if (IsDNSUnsetting)
                {
                    string msg = $"Let DNS Unset.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Orange));
                    return;
                }

                UpdateProxyBools = false;
                IsProxyActivating = true;

                // Start Share
                string msgStart = $"Starting Proxy Server...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStart, Color.MediumSeaGreen));

                // Delete request log on > 50KB
                try
                {
                    if (File.Exists(SecureDNS.ProxyServerRequestLogPath))
                    {
                        long lenth = new FileInfo(SecureDNS.ProxyServerRequestLogPath).Length;
                        if (ConvertTool.ConvertSize(lenth, ConvertTool.SizeUnits.Byte, ConvertTool.SizeUnits.KB, out _) > 50)
                            File.Delete(SecureDNS.ProxyServerRequestLogPath);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Delete Proxy Server Request log file: {ex.Message}");
                }

                // Check port
                int proxyPort = GetProxyPortSetting();
                bool isPortOk = GetListeningPort(proxyPort, "Change Proxy Server port from settings.", Color.IndianRed);
                if (!isPortOk)
                {
                    IsProxyActivating = false;
                    return;
                }

                // Kill if it's already running
                ProcessManager.KillProcessByPID(PIDProxy);
                bool isCmdSent = false;
                PIDProxy = ProxyConsole.Execute(SecureDNS.ProxyServerPath, null, true, true, SecureDNS.CurrentPath, GetCPUPriority());
                
                // Wait for Proxy
                Task wait1 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (ProcessManager.FindProcessByPID(PIDProxy)) break;
                        await Task.Delay(100);
                    }
                });
                try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                if (!ProcessManager.FindProcessByPID(PIDProxy))
                {
                    string msg = $"Couldn't start Proxy Server. Try again.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    ProcessManager.KillProcessByPID(PIDProxy);
                    IsProxyActivating = false;
                    return;
                }

                // Write Requests to Log
                if (CustomCheckBoxProxyEventShowRequest.Checked)
                {
                    isCmdSent = await ProxyConsole.SendCommandAsync("Requests True");
                    if (!isCmdSent)
                    {
                        FaildSendCommandMessage();
                        IsProxyActivating = false;
                        return;
                    }
                }
                else
                {
                    isCmdSent = await ProxyConsole.SendCommandAsync("Requests False");
                    if (!isCmdSent)
                    {
                        FaildSendCommandMessage();
                        IsProxyActivating = false;
                        return;
                    }
                }

                // Write Chunk Details to Log
                if (CustomCheckBoxProxyEventShowChunkDetails.Checked)
                {
                    isCmdSent = await ProxyConsole.SendCommandAsync("ChunkDetails True");
                    if (!isCmdSent)
                    {
                        FaildSendCommandMessage();
                        IsProxyActivating = false;
                        return;
                    }
                }
                else
                {
                    isCmdSent = await ProxyConsole.SendCommandAsync("ChunkDetails False");
                    if (!isCmdSent)
                    {
                        FaildSendCommandMessage();
                        IsProxyActivating = false;
                        return;
                    }
                }

                // Write Proxy Requests And Chunk Details To Log
                ProxyConsole.ErrorDataReceived -= ProxyProcess_ErrorDataReceived;
                ProxyConsole.ErrorDataReceived += ProxyProcess_ErrorDataReceived;
                void ProxyProcess_ErrorDataReceived(object? sender, DataReceivedEventArgs e)
                {
                    string? msg = e.Data;
                    if (!string.IsNullOrEmpty(msg))
                    {
                        // Write to log
                        if (!IsCheckingStarted && !IsConnecting && !IsExiting && IsProxyActivated && IsProxyRunning)
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.Gray));

                        // Write to file
                        try
                        {
                            FileDirectory.AppendTextLine(SecureDNS.ProxyServerRequestLogPath, msg, new UTF8Encoding(false));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Write Proxy Request log file: {ex.Message}");
                        }
                    }
                }

                // Apply DPI Bypass Support
                bool isDpiBypassApplied = await ApplyPDpiChanges();
                if (!isDpiBypassApplied)
                {
                    ProcessManager.KillProcessByPID(PIDProxy);
                    IsProxyActivating = false;
                    return;
                }

                // Apply UpStream Proxy Support
                bool isUpstreamOK = await ApplyPUpStreamProxy();
                if (!isUpstreamOK)
                {
                    ProcessManager.KillProcessByPID(PIDProxy);
                    IsProxyActivating = false;
                    return;
                }

                // Apply DNS Support
                bool isDnsOk = await ApplyPDNS();
                if (!isDnsOk)
                {
                    ProcessManager.KillProcessByPID(PIDFakeProxy);
                    ProcessManager.KillProcessByPID(PIDProxy);
                    IsProxyActivating = false;
                    return;
                }

                // Apply Fake DNS Support
                bool isFakeDnsOk = await ApplyPFakeDNS();
                if (!isFakeDnsOk)
                {
                    ProcessManager.KillProcessByPID(PIDFakeProxy);
                    ProcessManager.KillProcessByPID(PIDProxy);
                    IsProxyActivating = false;
                    return;
                }

                // Apply Black White List Support
                bool isBlackWhiteListOk = await ApplyPBlackWhiteList();
                if (!isBlackWhiteListOk)
                {
                    ProcessManager.KillProcessByPID(PIDFakeProxy);
                    ProcessManager.KillProcessByPID(PIDProxy);
                    IsProxyActivating = false;
                    return;
                }

                // Apply DontBypass Support
                bool isDontBypassOk = await ApplyPDontBypass();
                if (!isDontBypassOk)
                {
                    ProcessManager.KillProcessByPID(PIDFakeProxy);
                    ProcessManager.KillProcessByPID(PIDProxy);
                    IsProxyActivating = false;
                    return;
                }

                // Get number of handle requests
                int handleReq = 250;
                this.InvokeIt(() => handleReq = Convert.ToInt32(CustomNumericUpDownSettingProxyHandleRequests.Value));

                // Get block port 80 setting
                bool blockPort80 = false;
                this.InvokeIt(() => blockPort80 = CustomCheckBoxSettingProxyBlockPort80.Checked);

                // Get killOnCpuUsage Setting
                int killOnCpuUsage = GetKillOnCpuUsageSetting();

                // Kill Request on Timeout (Sec)
                int reqTimeoutSec = 0;
                this.InvokeIt(() => reqTimeoutSec = Convert.ToInt32(CustomNumericUpDownSettingProxyKillRequestTimeout.Value));

                // Send Setting Command
                string settingCommand = $"Setting -Port={proxyPort} -MaxThreads={handleReq} -RequestTimeoutSec={reqTimeoutSec} -KillOnCpuUsage={killOnCpuUsage} -BlockPort80={blockPort80}";
                Debug.WriteLine(settingCommand);
                await ProxyConsole.SendCommandAsync(settingCommand);

                // Start Proxy
                string startCommand = "Start";
                Debug.WriteLine(startCommand);
                await ProxyConsole.SendCommandAsync(startCommand);
                await Task.Delay(500);

                // Check for successfull comunication with console
                isCmdSent = await ProxyConsole.SendCommandAsync("out");
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
                    ProcessManager.KillProcessByPID(PIDFakeProxy);
                    ProcessManager.KillProcessByPID(PIDProxy);
                    FaildSendCommandMessage();
                    IsProxyActivating = false;
                    return;
                }

                // Update Proxy Port
                ProxyPort = GetProxyPortSetting();

                // Write Sharing Address to log
                LocalIP = NetworkTool.GetLocalIPv4(); // Update Local IP
                IPAddress localIP = LocalIP ?? IPAddress.Loopback;

                string msgProxy1 = "Local Proxy Server (HTTP, SOCKS4, SOCKS4A, SOCKS5):" + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy1, Color.LightGray));

                string msgProxy2 = $"{IPAddress.Loopback}:{proxyPort}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy2 + NL, Color.DodgerBlue));

                string msgProxy3 = $"{localIP}:{proxyPort}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgProxy3 + NL, Color.DodgerBlue));

                // Update Bools
                IsProxyRunning = true;
                UpdateProxyBools = true;
                IsProxyActivating = false;

                // Warm up Proxy (Sync)
                string blockedDomain = GetBlockedDomainSetting(out string _);
                string proxyScheme = $"socks5://{IPAddress.Loopback}:{ProxyPort}";
                if (IsDNSSet || ProcessManager.FindProcessByPID(PIDFakeProxy))
                    _ = Task.Run(() => WarmUpProxy(blockedDomain, proxyScheme, 30, CancellationToken.None));
            }
            else
            {
                // Stop Proxy
                if (IsProxyActivating || IsProxyDeactivating) return;
                IsProxyDeactivating = true;

                // Stop Fake Proxy
                ProcessManager.KillProcessByPID(PIDFakeProxy);

                // Stop Proxy Server
                if (ProcessManager.FindProcessByPID(PIDProxy))
                {
                    // Unset Proxy First
                    if (IsProxySet) SetProxy();
                    await Task.Delay(100);
                    if (IsProxySet) NetworkTool.UnsetProxy(false, true);

                    ProcessManager.KillProcessByPID(PIDProxy);

                    // Wait for Proxy Server to Exit
                    Task wait1 = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (!ProcessManager.FindProcessByPID(PIDProxy)) break;
                            await Task.Delay(100);
                        }
                    });
                    try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                    if (!ProcessManager.FindProcessByPID(PIDProxy))
                    {
                        // Update Bool
                        IsProxyRunning = false;

                        // Write deactivated message to log
                        string msgDiactivated = $"Proxy Server deactivated.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDiactivated, Color.MediumSeaGreen));
                    }
                    else
                    {
                        // Couldn't stop
                        string msg = $"Couldn't stop Proxy Server.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    }
                }
                else
                {
                    // Already deactivated
                    string msg = $"It's already deactivated.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                }

                IsProxyDeactivating = false;
            }
        });
    }

    public void FaildSendCommandMessage()
    {
        string msg = $"Couldn't communicate with Proxy Console.{NL}";
        msg += $"Please make sure you have .NET Desktop v6 x86 and x64 installed on your OS.{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
    }

    public string ConvertNewLineToN(string text)
    {
        return text.Replace(NL, "\n").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", @"\n").Replace(@"\n\n", @"\n");
    }

    private async Task<bool> ApplyPDpiChanges()
    {
        // Return if DNS is setting or unsetting
        if (IsDNSSetting || IsDNSUnsetting) return false;

        ApplyPDpiChangesOut(out ProxyProgram.DPIBypass.Mode bypassMode, out int beforSniChunks, out ProxyProgram.DPIBypass.ChunkMode chunkMode, out int sniChunks, out int antiPatternOffset, out int fragmentDelay);

        UpdateProxyBools = false;
        string command = $"Programs DpiBypass -Mode={bypassMode} -BeforeSniChunks={beforSniChunks} -ChunkMode={chunkMode} -SniChunks={sniChunks} -AntiPatternOffset={antiPatternOffset} -FragmentDelay={fragmentDelay}";
        Debug.WriteLine(command);
        bool isCommandSent = await ProxyConsole.SendCommandAsync(command);
        UpdateProxyBools = true;
        if (!isCommandSent) return false;

        // Check DPI Works
        bool isDpiBypassEnabled = false;
        this.InvokeIt(() => isDpiBypassEnabled = CustomCheckBoxPDpiEnableDpiBypass.Checked);
        if (isDpiBypassEnabled && IsProxyActivated && !IsProxyActivating)
        {
            if (bypassMode != ProxyProgram.DPIBypass.Mode.Disable)
            {
                IsProxyDpiBypassActive = true;
                IsDPIActive = true;
            }

            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string _);
            if (!string.IsNullOrEmpty(blockedDomain))
                await CheckDPIWorks(blockedDomain);
        }

        return true;
    }

    private async Task ApplyPDpiChangesFakeProxy()
    {
        ApplyPDpiChangesOut(out ProxyProgram.DPIBypass.Mode bypassMode, out int beforSniChunks, out ProxyProgram.DPIBypass.ChunkMode chunkMode, out int sniChunks, out int antiPatternOffset, out int fragmentDelay);

        UpdateProxyBools = false;
        string command = $"Programs DpiBypass -Mode={bypassMode} -BeforeSniChunks={beforSniChunks} -ChunkMode={chunkMode} -SniChunks={sniChunks} -AntiPatternOffset={antiPatternOffset} -FragmentDelay={fragmentDelay}";
        Debug.WriteLine(command);
        await FakeProxyConsole.SendCommandAsync(command);
        UpdateProxyBools = true;
    }

    private void ApplyPDpiChangesOut(out ProxyProgram.DPIBypass.Mode bypassMode, out int beforSniChunks, out ProxyProgram.DPIBypass.ChunkMode chunkMode, out int sniChunks, out int antiPatternOffset, out int fragmentDelay)
    {
        // Get fragment settings
        bool enableDpiBypass = false;
        int beforSniChunks0 = -1, chunkModeInt = -1;
        this.InvokeIt(() =>
        {
            enableDpiBypass = CustomCheckBoxPDpiEnableDpiBypass.Checked;
            beforSniChunks0 = Convert.ToInt32(CustomNumericUpDownPDpiBeforeSniChunks.Value);
            chunkModeInt = CustomComboBoxPDpiSniChunkMode.SelectedIndex;
        });
        beforSniChunks = beforSniChunks0;

        chunkMode = chunkModeInt switch
        {
            0 => ProxyProgram.DPIBypass.ChunkMode.SNI,
            1 => ProxyProgram.DPIBypass.ChunkMode.SniExtension,
            2 => ProxyProgram.DPIBypass.ChunkMode.AllExtensions,
            _ => ProxyProgram.DPIBypass.ChunkMode.AllExtensions,
        };

        int sniChunks0 = -1, antiPatternOffset0 = -1, fragmentDelay0 = -1;
        this.InvokeIt(() =>
        {
            sniChunks0 = Convert.ToInt32(CustomNumericUpDownPDpiSniChunks.Value);
            antiPatternOffset0 = Convert.ToInt32(CustomNumericUpDownPDpiAntiPatternOffset.Value);
            fragmentDelay0 = Convert.ToInt32(CustomNumericUpDownPDpiFragDelay.Value);
        });
        sniChunks = sniChunks0; antiPatternOffset = antiPatternOffset0; fragmentDelay = fragmentDelay0;

        bypassMode = enableDpiBypass ? ProxyProgram.DPIBypass.Mode.Program : ProxyProgram.DPIBypass.Mode.Disable;
    }

    public async Task<bool> ApplyPUpStreamProxy()
    {
        if (!CustomCheckBoxSettingProxyUpstream.Checked) return true;

        // Get Upstream Settings
        // Upstream Mode
        string? upstreamModeString = null;
        this.InvokeIt(() => upstreamModeString = CustomComboBoxSettingProxyUpstreamMode.SelectedItem as string);

        // Check if Mode is empty
        if (string.IsNullOrEmpty(upstreamModeString))
        {
            string msg = "Select the mode of upstream proxy." + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            return false;
        }

        ProxyProgram.UpStreamProxy.Mode upstreamMode = ProxyProgram.UpStreamProxy.Mode.Disable;
        if (upstreamModeString.Equals("HTTP"))
            upstreamMode = ProxyProgram.UpStreamProxy.Mode.HTTP;
        else if (upstreamModeString.Equals("SOCKS5"))
            upstreamMode = ProxyProgram.UpStreamProxy.Mode.SOCKS5;

        // Upstream Host
        string upstreamHost = string.Empty;
        this.InvokeIt(() => upstreamHost = CustomTextBoxSettingProxyUpstreamHost.Text);

        // Check if Host is empty
        if (string.IsNullOrWhiteSpace(upstreamHost))
        {
            string msg = "Upstream proxy host cannot be empty." + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            return false;
        }

        // Upstream Port
        int upstreamPort = -1;
        this.InvokeIt(() => upstreamPort = Convert.ToInt32(CustomNumericUpDownSettingProxyUpstreamPort.Value));

        // Get blocked domain
        string blockedDomain = GetBlockedDomainSetting(out string _);
        if (string.IsNullOrEmpty(blockedDomain)) return false;

        // Get Upstream Proxy Scheme
        string upstreamProxyScheme = string.Empty;
        if (upstreamMode == ProxyProgram.UpStreamProxy.Mode.HTTP)
            upstreamProxyScheme += "http";
        if (upstreamMode == ProxyProgram.UpStreamProxy.Mode.SOCKS5)
            upstreamProxyScheme += "socks5";
        upstreamProxyScheme += $"://{upstreamHost}:{upstreamPort}";

        // Check Upstream Proxy Works
        bool isUpstreamProxyOk = await NetworkTool.CheckProxyWorks($"https://{blockedDomain}", upstreamProxyScheme, 15);
        if (!isUpstreamProxyOk)
        {
            string msg = $"Upstream proxy cannot open {blockedDomain}." + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            return false;
        }

        // Only apply to blocked IPs
        bool onlyBlockedIPs = false;
        this.InvokeIt(() => onlyBlockedIPs = CustomCheckBoxSettingProxyUpstreamOnlyBlockedIPs.Checked);

        // Apply UpStream Proxy Program to Proxy Server
        string command = $"Programs UpStreamProxy -Mode={upstreamMode} -Host={upstreamHost} -Port={upstreamPort} -OnlyApplyToBlockedIPs={onlyBlockedIPs}";
        Debug.WriteLine(command);
        return await ProxyConsole.SendCommandAsync(command);
    }

    public async Task<bool> ApplyPDNS()
    {
        // Set timeout
        int timeoutSec = 5;

        // Get CF Clean IP
        bool changeCfIp = false;
        this.InvokeIt(() => changeCfIp = CustomCheckBoxSettingProxyCfCleanIP.Checked);

        string cfCleanIP = string.Empty;
        this.InvokeIt(() => cfCleanIP = CustomTextBoxSettingProxyCfCleanIP.Text.Trim());

        if (CustomCheckBoxSettingProxyEnableFakeProxy.Checked)
        {
            // Use Fake Proxy DoH
            // Start msg
            string msgStart = $"Starting Fake Proxy server...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStart, Color.Orange));

            // Get Fake Proxy DoH Address
            string dohUrl = string.Empty;
            this.InvokeIt(() => dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text);

            // Get Clean Ip
            string dohCleanIP = string.Empty;
            this.InvokeIt(() => dohCleanIP = CustomTextBoxSettingFakeProxyDohCleanIP.Text);

            // Get loopback
            string loopback = IPAddress.Loopback.ToString();

            // Check port
            int fakeProxyPort = GetFakeProxyPortSetting();
            bool isPortOk = GetListeningPort(fakeProxyPort, string.Empty, Color.Orange);
            if (!isPortOk)
            {
                fakeProxyPort = NetworkTool.GetNextPort(fakeProxyPort);
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Trying Port {fakeProxyPort}...{NL}", Color.MediumSeaGreen));
                bool isPort2Ok = GetListeningPort(fakeProxyPort, string.Empty, Color.Orange);
                if (!isPort2Ok)
                {
                    fakeProxyPort = NetworkTool.GetNextPort(fakeProxyPort);
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Trying Port {fakeProxyPort}...{NL}", Color.MediumSeaGreen));
                    bool isPort3Ok = GetListeningPort(fakeProxyPort, "Change Fake Proxy port from settings.", Color.IndianRed);
                    if (!isPort3Ok)
                    {
                        return false;
                    }
                }
            }

            // Get Fake Proxy Scheme
            string fakeProxyScheme = $"http://{loopback}:{fakeProxyPort}";

            //============================== Start FakeProxy
            // Kill if it's already running
            ProcessManager.KillProcessByPID(PIDFakeProxy);

            // Execute Fake Proxy
            PIDFakeProxy = FakeProxyConsole.Execute(SecureDNS.ProxyServerPath, null, true, true, SecureDNS.CurrentPath, GetCPUPriority());
            
            // Wait for Fake Proxy
            Task fakeWait = Task.Run(async () =>
            {
                while (true)
                {
                    if (ProcessManager.FindProcessByPID(PIDFakeProxy)) break;
                    await Task.Delay(100);
                }
            });
            try { await fakeWait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

            if (!ProcessManager.FindProcessByPID(PIDFakeProxy)) return false;

            // Apply Fake DNS and White List Support to Fake Proxy
            bool isFpOk = await ApplyFakeProxy(FakeProxyConsole);
            if (!isFpOk) return false;

            // Apply DPI Bypass Support to Fake Proxy
            await ApplyPDpiChangesFakeProxy();

            // Get killOnCpuUsage Setting
            int killOnCpuUsage = GetKillOnCpuUsageSetting();

            // Send Setting Command
            string settingCommand = $"Setting -Port={fakeProxyPort} -MaxThreads=1000 -RequestTimeoutSec=0 -KillOnCpuUsage={killOnCpuUsage} -BlockPort80=True";
            Debug.WriteLine(settingCommand);
            bool isSettingSent = await FakeProxyConsole.SendCommandAsync(settingCommand);
            if (!isSettingSent) return false;

            // Start Fake Proxy
            string startCommand = $"Start";
            Debug.WriteLine(startCommand);
            bool isStartSent = await FakeProxyConsole.SendCommandAsync(startCommand);
            if (!isStartSent) return false;
            //============================== End FakeProxy

            bool isCfIpOk = await isCloudflareIpOk();
            if (!isCfIpOk) return false;

            // Apply DNS Program to Proxy Server
            string dnsCommand = $"Programs Dns -Mode=DoH -TimeoutSec={timeoutSec} -DnsAddr={dohUrl} -DnsCleanIp={dohCleanIP} -ProxyScheme={fakeProxyScheme}";
            if (changeCfIp && !string.IsNullOrEmpty(cfCleanIP))
                dnsCommand += $" -CfCleanIp={cfCleanIP}";
            Debug.WriteLine(dnsCommand);
            bool isCmdSent = await ProxyConsole.SendCommandAsync(dnsCommand);
            if (!isCmdSent) return false;

            // End msg
            string msgEnd = $"Fake Proxy Server activated. Port: {fakeProxyPort}{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgEnd, Color.MediumSeaGreen));

            return true;
        }
        else
        {
            // Use System DNS
            bool isCfIpOk = await isCloudflareIpOk();
            if (!isCfIpOk) return false;

            // Apply DNS Program to Proxy Server
            string dnsCommand = $"Programs Dns -Mode=System -TimeoutSec={timeoutSec}";
            if (changeCfIp && !string.IsNullOrEmpty(cfCleanIP))
                dnsCommand += $" -CfCleanIp={cfCleanIP}";
            Debug.WriteLine(dnsCommand);
            return await ProxyConsole.SendCommandAsync(dnsCommand);
        }

        async Task<bool> isCloudflareIpOk()
        {
            // Add redirect all Cloudflare IPs to a clean IP
            bool changeIp = false;
            this.InvokeIt(() => changeIp = CustomCheckBoxSettingProxyCfCleanIP.Checked);

            if (changeIp)
            {
                // Get CF Clean IP
                string cleanIP = string.Empty;
                this.InvokeIt(() => cleanIP = CustomTextBoxSettingProxyCfCleanIP.Text.Trim());

                // Check CF IP is valid
                bool isCleanIpValid = NetworkTool.IsIPv4Valid(cleanIP, out IPAddress? _);
                if (!isCleanIpValid)
                {
                    // CF Clean IP is not valid
                    string msg = $"Cloudflare clean IP is not valid.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    return false;
                }

                // Check IP is Clean
                bool isClean1 = await NetworkTool.CanPing(cleanIP, timeoutSec * 1000);
                bool isClean2 = await NetworkTool.CanTcpConnect(cleanIP, 443, timeoutSec * 1000);
                if (!isClean1 || !isClean2)
                {
                    // CF Clean IP is not valid
                    string msg = $"Cloudflare clean IP is not clean.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    return false;
                }
            }

            return true;
        }
    }

    public async Task<bool> ApplyPFakeDNS()
    {
        if (CustomCheckBoxSettingProxyEnableFakeDNS.Checked)
        {
            string command = $"Programs FakeDns -Mode=File -PathOrText=\"{SecureDNS.FakeDnsRulesPath}\"";
            Debug.WriteLine(command);
            return await ProxyConsole.SendCommandAsync(command);
        }

        return true;
    }

    public async Task<bool> ApplyPBlackWhiteList()
    {
        if (CustomCheckBoxSettingProxyEnableBlackWhiteList.Checked)
        {
            string command = $"Programs BwList -Mode=BlackListFile -PathOrText=\"{SecureDNS.BlackWhiteListPath}\"";
            Debug.WriteLine(command);
            return await ProxyConsole.SendCommandAsync(command);
        }

        return true;
    }

    public async Task<bool> ApplyPDontBypass()
    {
        if (CustomCheckBoxSettingProxyEnableDontBypass.Checked)
        {
            string command = $"Programs DontBypass -Mode=File -PathOrText=\"{SecureDNS.DontBypassListPath}\"";
            Debug.WriteLine(command);
            return await ProxyConsole.SendCommandAsync(command);
        }

        return true;
    }

}