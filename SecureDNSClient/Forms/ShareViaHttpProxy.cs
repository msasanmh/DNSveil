using MsmhToolsClass;
using MsmhToolsClass.HTTPProxyServer;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        /// <summary>
        /// Apply Fake Proxy (Fake DNS and White List)
        /// </summary>
        /// <param name="proxy">HTTP Proxy</param>
        /// <returns>Returns True if everything's ok.</returns>
        public bool ApplyFakeProxy(HTTPProxyServer proxy)
        {
            bool isOk = ApplyFakeProxyOut(out HTTPProxyServer.Program.BlackWhiteList? wListProgram, out HTTPProxyServer.Program.FakeDns? fakeDNSProgram);
            if (!isOk) return false;
            if (wListProgram == null) return false;
            if (fakeDNSProgram == null) return false;

            // Add Programs to Proxy
            proxy.BlockPort80 = true;
            proxy.RequestTimeoutSec = 0; // 0 = Disable
            proxy.EnableBlackWhiteList(wListProgram);
            proxy.EnableFakeDNS(fakeDNSProgram);

            return true;
        }

        /// <summary>
        /// Apply Fake Proxy (Fake DNS and White List)
        /// </summary>
        /// <param name="fakeProcess">Process</param>
        /// <returns>Returns True if everything's ok.</returns>
        public async Task<bool> ApplyFakeProxy(Process fakeProcess)
        {
            bool isOk = ApplyFakeProxyOut(out HTTPProxyServer.Program.BlackWhiteList? wListProgram, out HTTPProxyServer.Program.FakeDns? fakeDNSProgram);
            if (!isOk) return false;
            if (wListProgram == null) return false;
            if (fakeDNSProgram == null) return false;

            // Add Programs to Proxy
            // Get and set block port 80 setting
            string blockPort80Command = $"blockport80 -true";
            Debug.WriteLine(blockPort80Command);
            await ProcessManager.SendCommandAsync(fakeProcess, blockPort80Command);

            // Kill Request on Timeout (Sec)
            string killOnRequestTimeoutCommand = $"requesttimeout -0";
            Debug.WriteLine(killOnRequestTimeoutCommand);
            await ProcessManager.SendCommandAsync(fakeProcess, killOnRequestTimeoutCommand);

            string wlCommand = $"bwlistprogram -{wListProgram.ListMode} -{ConvertNewLineToN(wListProgram.TextContent)}";
            Debug.WriteLine(wlCommand);
            await ProcessManager.SendCommandAsync(fakeProcess, wlCommand);

            string fdCommand = $"fakednsprogram -{fakeDNSProgram.FakeDnsMode} -{ConvertNewLineToN(fakeDNSProgram.TextContent)}";
            Debug.WriteLine(fdCommand);
            await ProcessManager.SendCommandAsync(fakeProcess, fdCommand);

            return true;
        }

        public bool ApplyFakeProxyOut(out HTTPProxyServer.Program.BlackWhiteList? blackWhiteList, out HTTPProxyServer.Program.FakeDns? fakeDns)
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
            bool isDohIpClean1 = NetworkTool.CanPing(dohCleanIP, 5000);
            bool isDohIpClean2 = NetworkTool.CanTcpConnect(dohCleanIP, 443, 5000);
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
            NetworkTool.GetUrlDetails(url, 443, out _, out string host, out int _, out string _, out bool _);
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
            HTTPProxyServer.Program.BlackWhiteList wListProgram = new();
            wListProgram.Set(HTTPProxyServer.Program.BlackWhiteList.Mode.WhiteListText, wlContent);

            // Fake DNS Program
            HTTPProxyServer.Program.FakeDns fakeDNSProgram = new();
            fakeDNSProgram.Set(HTTPProxyServer.Program.FakeDns.Mode.Text, fdContent);

            blackWhiteList = wListProgram;
            fakeDns = fakeDNSProgram;
            return true;
        }

        private async Task StartHttpProxy()
        {
            await Task.Run(async () =>
            {
                if (!IsHttpProxyActivated && !IsHttpProxyRunning)
                {
                    // Start Proxy
                    if (IsHttpProxyActivating || IsHttpProxyDeactivating) return;

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

                    UpdateHttpProxyBools = false;
                    IsHttpProxyActivating = true;

                    // Start Share
                    string msgStart = $"Starting HTTP Proxy...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStart, Color.MediumSeaGreen));

                    // Delete request log on > 50KB
                    try
                    {
                        if (File.Exists(SecureDNS.HTTPProxyServerRequestLogPath))
                        {
                            long lenth = new FileInfo(SecureDNS.HTTPProxyServerRequestLogPath).Length;
                            if (ConvertTool.ConvertSize(lenth, ConvertTool.SizeUnits.Byte, ConvertTool.SizeUnits.KB, out _) > 50)
                                File.Delete(SecureDNS.HTTPProxyServerRequestLogPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Delete HTTP Proxy Request log file: {ex.Message}");
                    }

                    // Check port
                    int httpProxyPort = GetHTTPProxyPortSetting();
                    bool isPortOk = GetListeningPort(httpProxyPort, "Change HTTP Proxy port from settings.", Color.IndianRed);
                    if (!isPortOk)
                    {
                        IsHttpProxyActivating = false;
                        return;
                    }

                    // Kill if it's already running
                    ProcessManager.KillProcessByPID(PIDHttpProxy);

                    // Execute Http Proxy
                    PIDHttpProxy = ProcessManager.ExecuteOnly(out Process process, SecureDNS.HttpProxyPath, null, true, true, SecureDNS.CurrentPath, GetCPUPriority());
                    HttpProxyProcess = process;

                    // Wait for HTTP Proxy
                    Task wait1 = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (ProcessManager.FindProcessByPID(PIDHttpProxy)) break;
                            await Task.Delay(100);
                        }
                    });
                    try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                    if (!ProcessManager.FindProcessByPID(PIDHttpProxy))
                    {
                        string msg = $"Couldn't start Http Proxy Server. Try again.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                        ProcessManager.KillProcessByPID(PIDHttpProxy);
                        IsHttpProxyActivating = false;
                        return;
                    }

                    // Write Requests to Log
                    if (CustomCheckBoxHTTPProxyEventShowRequest.Checked)
                    {
                        string done = await ProcessManager.SendCommandAndGetAnswerAsync(HttpProxyProcess, "writerequests -true", false);
                        if (string.IsNullOrEmpty(done) && !done.Contains("Done"))
                        {
                            FaildSendCommandMessage();
                            IsHttpProxyActivating = false;
                            return;
                        }
                    }
                    else
                    {
                        string done = await ProcessManager.SendCommandAndGetAnswerAsync(HttpProxyProcess, "writerequests -false", false);
                        if (string.IsNullOrEmpty(done) && !done.Contains("Done"))
                        {
                            FaildSendCommandMessage();
                            IsHttpProxyActivating = false;
                            return;
                        }
                    }

                    // Write Chunk Details to Log
                    if (CustomCheckBoxHTTPProxyEventShowChunkDetails.Checked)
                    {
                        string done = await ProcessManager.SendCommandAndGetAnswerAsync(HttpProxyProcess, "writechunkdetails -true", false);
                        if (string.IsNullOrEmpty(done) && !done.Contains("Done"))
                        {
                            FaildSendCommandMessage();
                            IsHttpProxyActivating = false;
                            return;
                        }
                    }
                    else
                    {
                        string done = await ProcessManager.SendCommandAndGetAnswerAsync(HttpProxyProcess, "writechunkdetails -false", false);
                        if (string.IsNullOrEmpty(done) && !done.Contains("Done"))
                        {
                            FaildSendCommandMessage();
                            IsHttpProxyActivating = false;
                            return;
                        }
                    }

                    // Write Proxy Requests And Chunk Details To Log
                    HttpProxyProcess.ErrorDataReceived -= ProxyProcess_ErrorDataReceived;
                    HttpProxyProcess.ErrorDataReceived += ProxyProcess_ErrorDataReceived;
                    HttpProxyProcess.BeginErrorReadLine(); // Redirects Error Output to Event
                    void ProxyProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
                    {
                        string? msg = e.Data;
                        if (!string.IsNullOrEmpty(msg))
                        {
                            // Write to log
                            if (!IsCheckingStarted && !IsConnecting && !IsExiting && IsHttpProxyActivated && IsHttpProxyRunning)
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.Gray));

                            // Write to file
                            try
                            {
                                FileDirectory.AppendTextLine(SecureDNS.HTTPProxyServerRequestLogPath, msg, new UTF8Encoding(false));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Write HTTP Proxy Request log file: {ex.Message}");
                            }
                        }
                    }

                    //Apply DPI Bypass Support
                    bool isDpiBypassApplied = ApplyPDpiChanges(HttpProxyProcess);
                    if (!isDpiBypassApplied)
                    {
                        ProcessManager.KillProcessByPID(PIDHttpProxy);
                        IsHttpProxyActivating = false;
                        return;
                    }

                    // Apply UpStream Proxy Support
                    bool isUpstreamOK = await ApplyPUpStreamProxy(HttpProxyProcess);
                    if (!isUpstreamOK)
                    {
                        ProcessManager.KillProcessByPID(PIDHttpProxy);
                        IsHttpProxyActivating = false;
                        return;
                    }

                    // Apply DNS Support
                    bool isDnsOk = await ApplyPDNS(HttpProxyProcess);
                    if (!isDnsOk)
                    {
                        ProcessManager.KillProcessByPID(PIDFakeHttpProxy);
                        ProcessManager.KillProcessByPID(PIDHttpProxy);
                        IsHttpProxyActivating = false;
                        return;
                    }

                    // Apply Fake DNS Support
                    bool isFakeDnsOk = await ApplyPFakeDNS(HttpProxyProcess);
                    if (!isFakeDnsOk)
                    {
                        ProcessManager.KillProcessByPID(PIDFakeHttpProxy);
                        ProcessManager.KillProcessByPID(PIDHttpProxy);
                        IsHttpProxyActivating = false;
                        return;
                    }

                    // Apply Black White List Support
                    bool isBlackWhiteListOk = await ApplyPBlackWhiteList(HttpProxyProcess);
                    if (!isBlackWhiteListOk)
                    {
                        ProcessManager.KillProcessByPID(PIDFakeHttpProxy);
                        ProcessManager.KillProcessByPID(PIDHttpProxy);
                        IsHttpProxyActivating = false;
                        return;
                    }

                    // Apply DontBypass Support
                    bool isDontBypassOk = await ApplyPDontBypass(HttpProxyProcess);
                    if (!isDontBypassOk)
                    {
                        ProcessManager.KillProcessByPID(PIDFakeHttpProxy);
                        ProcessManager.KillProcessByPID(PIDHttpProxy);
                        IsHttpProxyActivating = false;
                        return;
                    }

                    // Get number of handle requests
                    int handleReq = 250;
                    this.InvokeIt(() => handleReq = Convert.ToInt32(CustomNumericUpDownSettingHTTPProxyHandleRequests.Value));

                    // Get and set block port 80 setting
                    bool blockPort80 = false;
                    this.InvokeIt(() => blockPort80 = CustomCheckBoxSettingProxyBlockPort80.Checked);
                    string blockPort80Command = $"blockport80 -{blockPort80}";
                    Debug.WriteLine(blockPort80Command);
                    await ProcessManager.SendCommandAsync(HttpProxyProcess, blockPort80Command);

                    // Kill Requests on overload CPU usage
                    int killOnCpuUsage = 40;
                    this.InvokeIt(() => killOnCpuUsage = Convert.ToInt32(CustomNumericUpDownSettingCpuKillProxyRequests.Value));
                    string killOnCpuUsageCommand = $"killoncpuusage -{killOnCpuUsage}";
                    Debug.WriteLine(killOnCpuUsageCommand);
                    await ProcessManager.SendCommandAsync(HttpProxyProcess, killOnCpuUsageCommand);

                    // Kill Request on Timeout (Sec)
                    int reqTimeoutSec = 0;
                    this.InvokeIt(() => reqTimeoutSec = Convert.ToInt32(CustomNumericUpDownSettingHTTPProxyKillRequestTimeout.Value));
                    string killOnRequestTimeoutCommand = $"requesttimeout -{reqTimeoutSec}";
                    Debug.WriteLine(killOnRequestTimeoutCommand);
                    await ProcessManager.SendCommandAsync(HttpProxyProcess, killOnRequestTimeoutCommand);

                    // Start Http Proxy
                    string startCommand = $"start -any -{httpProxyPort} -{handleReq}";
                    Debug.WriteLine(startCommand);
                    await ProcessManager.SendCommandAsync(HttpProxyProcess, startCommand);
                    await Task.Delay(500);

                    // Check for successfull comunication with console
                    HttpProxyProcess.StandardOutput.DiscardBufferedData();
                    string outMsg = await ProcessManager.SendCommandAndGetAnswerAsync(HttpProxyProcess, "out", false);
                    if (string.IsNullOrEmpty(outMsg) && !outMsg.StartsWith("Details"))
                    {
                        ProcessManager.KillProcessByPID(PIDFakeHttpProxy);
                        ProcessManager.KillProcessByPID(PIDHttpProxy);
                        FaildSendCommandMessage();
                        IsHttpProxyActivating = false;
                        return;
                    }

                    // Update Proxy Port
                    HttpProxyPort = GetHTTPProxyPortSetting();

                    // Write Sharing Address to log
                    LocalIP = NetworkTool.GetLocalIPv4(); // Update Local IP
                    IPAddress localIP = LocalIP ?? IPAddress.Loopback;

                    string msgHTTPProxy1 = "Local HTTP Proxy:" + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxy1, Color.LightGray));

                    string msgHTTPProxy2 = $"http://{IPAddress.Loopback}:{httpProxyPort}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxy2 + NL, Color.DodgerBlue));

                    string msgHTTPProxy3 = $"http://{localIP}:{httpProxyPort}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxy3 + NL, Color.DodgerBlue));

                    // Update Bools
                    IsHttpProxyRunning = true;
                    UpdateHttpProxyBools = true;
                    IsHttpProxyActivating = false;
                    HttpProxyProcess.StandardOutput.DiscardBufferedData();
                }
                else
                {
                    // Stop Proxy
                    if (IsHttpProxyActivating || IsHttpProxyDeactivating) return;
                    IsHttpProxyDeactivating = true;

                    // Stop Fake Proxy
                    ProcessManager.KillProcessByPID(PIDFakeHttpProxy);

                    // Stop HTTP Proxy
                    if (ProcessManager.FindProcessByPID(PIDHttpProxy))
                    {
                        // Unset Proxy First
                        if (IsHttpProxySet) SetProxy();
                        await Task.Delay(100);
                        if (IsHttpProxySet) NetworkTool.UnsetProxy(false, true);

                        ProcessManager.KillProcessByPID(PIDHttpProxy);

                        // Wait for HTTP Proxy to Exit
                        Task wait1 = Task.Run(async () =>
                        {
                            while (true)
                            {
                                if (!ProcessManager.FindProcessByPID(PIDHttpProxy)) break;
                                await Task.Delay(100);
                            }
                        });
                        try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                        if (!ProcessManager.FindProcessByPID(PIDHttpProxy))
                        {
                            // Update Bool
                            IsHttpProxyRunning = false;

                            // Write deactivated message to log
                            string msgDiactivated = $"HTTP Proxy Server deactivated.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDiactivated, Color.MediumSeaGreen));
                        }
                        else
                        {
                            // Couldn't stop
                            string msg = $"Couldn't stop HTTP Proxy Server.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                        }
                    }
                    else
                    {
                        // Already deactivated
                        string msg = $"It's already deactivated.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                    }

                    IsHttpProxyDeactivating = false;
                }
            });
        }

        public void FaildSendCommandMessage()
        {
            string msg = $"Couldn't communicate with Proxy Console.{NL}";
            msg += "Please make sure you have .NET Desktop v6 installed on your OS.";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
        }

        public string ConvertNewLineToN(string text)
        {
            return text.Replace(NL, "\n").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", @"\n").Replace(@"\n\n", @"\n");
        }

        private bool ApplyPDpiChanges(Process process)
        {
            // Return if DNS is setting or unsetting
            if (IsDNSSetting || IsDNSUnsetting) return false;

            ApplyPDpiChangesOut(out HTTPProxyServer.Program.DPIBypass.Mode bypassMode, out int beforSniChunks, out HTTPProxyServer.Program.DPIBypass.ChunkMode chunkMode, out int sniChunks, out int antiPatternOffset, out int fragmentDelay);

            UpdateHttpProxyBools = false;
            string command = $"staticdpibypassprogram -{bypassMode} -{beforSniChunks} -{chunkMode} -{sniChunks} -{antiPatternOffset} -{fragmentDelay}";
            Debug.WriteLine(command);
            bool isCommandSent = ProcessManager.SendCommand(process, command);
            if (!isCommandSent) return false;
            UpdateHttpProxyBools = true;

            // Check DPI Works
            bool isDpiBypassEnabled = false;
            this.InvokeIt(() => isDpiBypassEnabled = CustomCheckBoxPDpiEnableDpiBypass.Checked);
            if (isDpiBypassEnabled && IsHttpProxyActivated && !IsHttpProxyActivating)
            {
                if (bypassMode != HTTPProxyServer.Program.DPIBypass.Mode.Disable)
                {
                    IsHttpProxyDpiBypassActive = true;
                    IsDPIActive = true;
                }

                // Get blocked domain
                string blockedDomain = GetBlockedDomainSetting(out string _);
                if (!string.IsNullOrEmpty(blockedDomain))
                    Task.Run(() => CheckDPIWorks(blockedDomain));
            }

            return true;
        }

        private void ApplyPDpiChangesFakeProxy(Process process)
        {
            ApplyPDpiChangesOut(out HTTPProxyServer.Program.DPIBypass.Mode bypassMode, out int beforSniChunks, out HTTPProxyServer.Program.DPIBypass.ChunkMode chunkMode, out int sniChunks, out int antiPatternOffset, out int fragmentDelay);

            string command = $"staticdpibypassprogram -{bypassMode} -{beforSniChunks} -{chunkMode} -{sniChunks} -{antiPatternOffset} -{fragmentDelay}";
            Debug.WriteLine(command);
            ProcessManager.SendCommand(process, command);
        }

        private void ApplyPDpiChangesOut(out HTTPProxyServer.Program.DPIBypass.Mode bypassMode, out int beforSniChunks, out HTTPProxyServer.Program.DPIBypass.ChunkMode chunkMode, out int sniChunks, out int antiPatternOffset, out int fragmentDelay)
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
                0 => HTTPProxyServer.Program.DPIBypass.ChunkMode.SNI,
                1 => HTTPProxyServer.Program.DPIBypass.ChunkMode.SniExtension,
                2 => HTTPProxyServer.Program.DPIBypass.ChunkMode.AllExtensions,
                _ => HTTPProxyServer.Program.DPIBypass.ChunkMode.AllExtensions,
            };

            int sniChunks0 = -1, antiPatternOffset0 = -1, fragmentDelay0 = -1;
            this.InvokeIt(() =>
            {
                sniChunks0 = Convert.ToInt32(CustomNumericUpDownPDpiSniChunks.Value);
                antiPatternOffset0 = Convert.ToInt32(CustomNumericUpDownPDpiAntiPatternOffset.Value);
                fragmentDelay0 = Convert.ToInt32(CustomNumericUpDownPDpiFragDelay.Value);
            });
            sniChunks = sniChunks0; antiPatternOffset = antiPatternOffset0; fragmentDelay = fragmentDelay0;

            bypassMode = enableDpiBypass ? HTTPProxyServer.Program.DPIBypass.Mode.Program : HTTPProxyServer.Program.DPIBypass.Mode.Disable;
        }

        public async Task<bool> ApplyPUpStreamProxy(Process process)
        {
            if (!CustomCheckBoxSettingHTTPProxyUpstream.Checked) return true;

            // Get Upstream Settings
            // Upstream Mode
            string? upstreamModeString = null;
            this.InvokeIt(() => upstreamModeString = CustomComboBoxSettingHttpProxyUpstreamMode.SelectedItem as string);

            // Check if Mode is empty
            if (string.IsNullOrEmpty(upstreamModeString))
            {
                string msg = "Select the mode of upstream proxy." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            HTTPProxyServer.Program.UpStreamProxy.Mode upstreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.Disable;
            if (upstreamModeString.Equals("HTTP"))
                upstreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.HTTP;
            else if (upstreamModeString.Equals("SOCKS5"))
                upstreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.SOCKS5;

            // Upstream Host
            string upstreamHost = string.Empty;
            this.InvokeIt(() => upstreamHost = CustomTextBoxSettingHTTPProxyUpstreamHost.Text);

            // Check if Host is empty
            if (string.IsNullOrWhiteSpace(upstreamHost))
            {
                string msg = "Upstream proxy host cannot be empty." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            // Upstream Port
            int upstreamPort = -1;
            this.InvokeIt(() => upstreamPort = Convert.ToInt32(CustomNumericUpDownSettingHTTPProxyUpstreamPort.Value));

            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string _);
            if (string.IsNullOrEmpty(blockedDomain)) return false;

            // Get Upstream Proxy Scheme
            string upstreamProxyScheme = string.Empty;
            if (upstreamMode == HTTPProxyServer.Program.UpStreamProxy.Mode.HTTP)
                upstreamProxyScheme += "http";
            if (upstreamMode == HTTPProxyServer.Program.UpStreamProxy.Mode.SOCKS5)
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
            this.InvokeIt(() => onlyBlockedIPs = CustomCheckBoxSettingHTTPProxyUpstreamOnlyBlockedIPs.Checked);

            // Apply UpStream Proxy Program to HTTP Proxy
            string command = $"upstreamproxyprogram -{upstreamMode} -{upstreamHost} -{upstreamPort} -{onlyBlockedIPs}";
            Debug.WriteLine(command);
            await ProcessManager.SendCommandAsync(process, command);

            return true;
        }

        public async Task<bool> ApplyPDNS(Process process)
        {
            // Set timeout
            int timeoutSec = 5;

            if (CustomCheckBoxSettingHTTPProxyEnableFakeProxy.Checked)
            {
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
                ProcessManager.KillProcessByPID(PIDFakeHttpProxy);

                // Execute Fake Proxy
                PIDFakeHttpProxy = ProcessManager.ExecuteOnly(out Process fakeProcess, SecureDNS.HttpProxyPath, null, true, true, SecureDNS.CurrentPath, GetCPUPriority());
                FakeHttpProxyProcess = fakeProcess;

                // Wait for Fake Proxy
                Task fakeWait = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (ProcessManager.FindProcessByPID(PIDFakeHttpProxy)) break;
                        await Task.Delay(100);
                    }
                });
                try { await fakeWait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                if (!ProcessManager.FindProcessByPID(PIDFakeHttpProxy)) return false;

                // Apply Fake DNS and White List Support to Fake Proxy
                bool isFpOk = await ApplyFakeProxy(FakeHttpProxyProcess);
                if (!isFpOk) return false;

                // Apply DPI Bypass Support to Fake Proxy
                ApplyPDpiChangesFakeProxy(FakeHttpProxyProcess);

                // Start Fake Proxy
                string startCommand = $"start -loopback -{fakeProxyPort} -50";
                Debug.WriteLine(startCommand);
                await ProcessManager.SendCommandAsync(FakeHttpProxyProcess, startCommand);

                await Task.Delay(500);
                FakeHttpProxyProcess.StandardOutput.DiscardBufferedData();
                //============================== End FakeProxy

                bool isSetCfOk = await setCloudflareIPs();
                if (!isSetCfOk) return false;

                // Apply DNS Program to HTTP Proxy
                string dnsCommand = $"dnsprogram -doh -{dohUrl} -{dohCleanIP} -{timeoutSec} -{fakeProxyScheme}";
                Debug.WriteLine(dnsCommand);
                await ProcessManager.SendCommandAsync(process, dnsCommand);

                // End msg
                string msgEnd = $"Fake Proxy Server activated. Port: {fakeProxyPort}{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgEnd, Color.MediumSeaGreen));

                return true;
            }
            else
            {
                bool isSetCfOk = await setCloudflareIPs();
                if (!isSetCfOk) return false;

                // Apply DNS Program to HTTP Proxy
                string dnsCommand = $"dnsprogram -system -null -null -{timeoutSec} -null";
                Debug.WriteLine(dnsCommand);
                await ProcessManager.SendCommandAsync(process, dnsCommand);

                return true;
            }

            async Task<bool> setCloudflareIPs()
            {
                // Add redirect all Cloudflare IPs to a clean IP
                if (CustomCheckBoxSettingHTTPProxyCfCleanIP.Checked)
                {
                    // Get CF Clean IP
                    string cleanIP = string.Empty;
                    this.InvokeIt(() => cleanIP = CustomTextBoxSettingHTTPProxyCfCleanIP.Text);

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
                    bool isClean1 = NetworkTool.CanPing(cleanIP, timeoutSec * 1000);
                    bool isClean2 = NetworkTool.CanTcpConnect(cleanIP, 443, timeoutSec * 1000);
                    if (!isClean1 || !isClean2)
                    {
                        // CF Clean IP is not valid
                        string msg = $"Cloudflare clean IP is not clean.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                        return false;
                    }

                    // Add to program
                    string command = $"dnsprogram -setcloudflareips -true -{cleanIP} -null";
                    Debug.WriteLine(command);
                    await ProcessManager.SendCommandAsync(process, command);

                    return true;
                }
                else
                    return true;
            }
        }

        public async Task<bool> ApplyPFakeDNS(Process process)
        {
            if (CustomCheckBoxSettingHTTPProxyEnableFakeDNS.Checked)
            {
                string command = $"fakednsprogram -file -{SecureDNS.FakeDnsRulesPath}";
                Debug.WriteLine(command);
                await ProcessManager.SendCommandAsync(process, command);
            }

            return true;
        }

        public async Task<bool> ApplyPBlackWhiteList(Process process)
        {
            if (CustomCheckBoxSettingHTTPProxyEnableBlackWhiteList.Checked)
            {
                string command = $"bwlistprogram -BlackListFile -{SecureDNS.BlackWhiteListPath}";
                Debug.WriteLine(command);
                await ProcessManager.SendCommandAsync(process, command);
            }

            return true;
        }

        public async Task<bool> ApplyPDontBypass(Process process)
        {
            if (CustomCheckBoxSettingHTTPProxyEnableDontBypass.Checked)
            {
                string command = $"dontbypassprogram -file -{SecureDNS.DontBypassListPath}";
                Debug.WriteLine(command);
                await ProcessManager.SendCommandAsync(process, command);
            }

            return true;
        }

        private void WriteProxyRequestsAndChunkDetailsToLogAuto()
        {
            // Another way of reading Error output (I'm using Event instead)
            System.Timers.Timer timer = new();
            timer.Interval = 50;
            timer.Elapsed += Timer_Elapsed;

            void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
            {
                if (!IsCheckingStarted && !IsConnecting && IsHttpProxyActivated && IsHttpProxyRunning)
                {
                    if (HttpProxyProcess != null)
                    {
                        string? msg = HttpProxyProcess.StandardError.ReadLine();
                        if (!string.IsNullOrEmpty(msg))
                        {
                            // Write to log
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.Gray));
                        }
                        HttpProxyProcess.StandardError.DiscardBufferedData();
                    }
                }
            }

            timer.Start();
        }

    }
}
