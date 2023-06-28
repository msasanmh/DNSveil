using MsmhTools;
using MsmhTools.HTTPProxyServer;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
            // Get DoH Clean Ip
            string dohCleanIP = CustomTextBoxSettingFakeProxyDohCleanIP.Text;
            bool isValid = Network.IsIPv4Valid(dohCleanIP, out IPAddress _);
            if (!isValid)
            {
                string msg = $"Fake Proxy clean IP is not valid, check Settings.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            // Check IP is Clean
            bool isDohIpClean1 = Network.CanPing(dohCleanIP, 5000);
            bool isDohIpClean2 = Network.CanTcpConnect(dohCleanIP, 443, 5000);
            if (!isDohIpClean1 || !isDohIpClean2)
            {
                // CF Clean IP is not valid
                string msg = $"Fake Proxy clean IP is not clean.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return false;
            }

            // Get Fake Proxy DoH Address
            string url = CustomTextBoxSettingFakeProxyDohAddress.Text;
            string host = Network.UrlToHostAndPort(url, 443, out int _, out string _, out bool _);
            string host1 = "dns.cloudflare.com";
            string host2 = "cloudflare-dns.com";
            string host3 = "every1dns.com";

            // Set White List and Fake DNS Content (Rules)
            string wlContent = $"{host}";
            string fdContent = $"{host}|{dohCleanIP}";

            if (host.Equals(host1) || host.Equals(host2) || host.Equals(host3))
            {
                wlContent = $"{host1}\n{host2}\n{host3}";
                fdContent = $"{host1}|{dohCleanIP}\n{host2}|{dohCleanIP}\n{host3}|{dohCleanIP}";
            }
            
            // White List Program
            HTTPProxyServer.Program.BlackWhiteList wListProgram = new();
            wListProgram.Set(HTTPProxyServer.Program.BlackWhiteList.Mode.WhiteListText, wlContent);

            // Fake DNS Program
            HTTPProxyServer.Program.FakeDns fakeDNSProgram = new();
            fakeDNSProgram.Set(HTTPProxyServer.Program.FakeDns.Mode.Text, fdContent);

            // Add Programs to Proxy
            proxy.BlockPort80 = true;
            proxy.EnableBlackWhiteList(wListProgram);
            proxy.EnableFakeDNS(fakeDNSProgram);

            return true;
        }

        private async void Share()
        {
            if (!IsSharing)
            {
                // Start Share
                string msgStart = $"Starting HTTP Proxy...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStart, Color.MediumSeaGreen));

                //// Write Set DNS first to log
                //if (!IsDNSSet)
                //{
                //    string msg = "Set DNS first." + NL;
                //    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                //    return;
                //}

                // Check port
                int httpProxyPort = GetHTTPProxyPortSetting();
                bool isPortOk = GetListeningPort(httpProxyPort, "Change HTTP Proxy port from settings.", Color.IndianRed);
                if (!isPortOk) return;

                if (HTTPProxy.IsRunning)
                    HTTPProxy.Stop();

                HTTPProxy = new();
                if (!HTTPProxy.IsRunning)
                {
                    // Delete request log on > 100KB
                    try
                    {
                        if (File.Exists(SecureDNS.HTTPProxyServerRequestLogPath))
                        {
                            long lenth = new FileInfo(SecureDNS.HTTPProxyServerRequestLogPath).Length;
                            if (FileDirectory.ConvertSize(lenth, FileDirectory.SizeUnits.Byte, FileDirectory.SizeUnits.KB) > 100)
                                File.Delete(SecureDNS.HTTPProxyServerRequestLogPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Delete HTTP Proxy Request log file: {ex.Message}");
                    }

                    // Delete error log on > 100KB
                    try
                    {
                        if (File.Exists(SecureDNS.HTTPProxyServerErrorLogPath))
                        {
                            long lenth = new FileInfo(SecureDNS.HTTPProxyServerErrorLogPath).Length;
                            if (FileDirectory.ConvertSize(lenth, FileDirectory.SizeUnits.Byte, FileDirectory.SizeUnits.KB) > 100)
                                File.Delete(SecureDNS.HTTPProxyServerErrorLogPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Delete HTTP Proxy Error log file: {ex.Message}");
                    }

                    HTTPProxy.OnRequestReceived -= HTTPProxy_OnRequestReceived;
                    HTTPProxy.OnRequestReceived += HTTPProxy_OnRequestReceived;
                    StaticDPIBypassProgram.OnChunkDetailsReceived -= StaticDPIBypassProgram_OnChunkDetailsReceived;
                    StaticDPIBypassProgram.OnChunkDetailsReceived += StaticDPIBypassProgram_OnChunkDetailsReceived;
                    HTTPProxy.OnErrorOccurred -= HTTPProxy_OnErrorOccurred;
                    HTTPProxy.OnErrorOccurred += HTTPProxy_OnErrorOccurred;

                    // Apply DPI Bypass Support
                    ApplyPDpiChanges(HTTPProxy);

                    // Apply UpStream Proxy Support
                    bool isUpstreamOK = await ApplyPUpStreamProxy(HTTPProxy);
                    if (!isUpstreamOK) return;

                    // Apply DNS Support
                    bool isDnsOk = ApplyPDNS(HTTPProxy);
                    if (!isDnsOk)
                    {
                        if (FakeProxy.IsRunning)
                            FakeProxy.Stop();
                        return;
                    }

                    // Apply Fake DNS Support
                    bool isFakeDnsOk = await ApplyPFakeDNS(HTTPProxy);
                    if (!isFakeDnsOk)
                    {
                        if (FakeProxy.IsRunning)
                            FakeProxy.Stop();
                        return;
                    }

                    // Apply Black White List Support
                    bool isBlackWhiteListOk = await ApplyPBlackWhiteList(HTTPProxy);
                    if (!isBlackWhiteListOk)
                    {
                        if (FakeProxy.IsRunning)
                            FakeProxy.Stop();
                        return;
                    }

                    // Get number of handle requests
                    int handleReq = Convert.ToInt32(CustomNumericUpDownSettingHTTPProxyHandleRequests.Value);

                    // Get and set block port 80 setting
                    HTTPProxy.BlockPort80 = CustomCheckBoxSettingProxyBlockPort80.Checked;

                    HTTPProxy.Start(IPAddress.Any, httpProxyPort, handleReq);
                    await Task.Delay(500);

                    // Proxy Event Requests
                    void HTTPProxy_OnRequestReceived(object? sender, EventArgs e)
                    {
                        if (sender is string req)
                        {
                            if (CustomCheckBoxHTTPProxyEventShowRequest.Checked)
                            {
                                if (!IsCheckingStarted && !IsConnecting)
                                {
                                    if (!StopWatchShowRequests.IsRunning) StopWatchShowRequests.Start();
                                    if (StopWatchShowRequests.ElapsedMilliseconds > 5)
                                    {
                                        // Write to log
                                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(req + NL, Color.Gray));

                                        // Write to file
                                        try
                                        {
                                            FileDirectory.AppendTextLine(SecureDNS.HTTPProxyServerRequestLogPath, req, new UTF8Encoding(false));
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"Write HTTP Proxy Request log file: {ex.Message}");
                                        }

                                        StopWatchShowRequests.Stop();
                                        StopWatchShowRequests.Reset();
                                    }
                                }
                            }
                        }
                    }

                    // Proxy Event Chunk Details
                    void StaticDPIBypassProgram_OnChunkDetailsReceived(object? sender, EventArgs e)
                    {
                        if (sender is string msg)
                        {
                            if (CustomCheckBoxHTTPProxyEventShowChunkDetails.Checked)
                            {
                                if (!IsCheckingStarted && !IsConnecting)
                                {
                                    if (!StopWatchShowChunkDetails.IsRunning) StopWatchShowChunkDetails.Start();
                                    if (StopWatchShowChunkDetails.ElapsedMilliseconds > 500)
                                    {
                                        msg += NL; // Adding an additional line break.
                                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));

                                        StopWatchShowChunkDetails.Stop();
                                        StopWatchShowChunkDetails.Reset();
                                    }
                                }
                            }
                        }
                    }

                    // Proxy Event Errors
                    void HTTPProxy_OnErrorOccurred(object? sender, EventArgs e)
                    {
                        if (sender is string error)
                        {
                            // Write to file
                            try
                            {
                                FileDirectory.AppendTextLine(SecureDNS.HTTPProxyServerErrorLogPath, error, new UTF8Encoding(false));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Write HTTP Proxy Error log file: {ex.Message}");
                            }
                        }
                    }

                    if (HTTPProxy.IsRunning)
                    {
                        // Update bool
                        IsSharing = true;

                        // Set Last Proxy Port
                        LastProxyPort = HTTPProxy.ListeningPort;

                        // Write Sharing Address to log
                        LocalIP = Network.GetLocalIPv4(); // Update Local IP
                        IPAddress localIP = LocalIP ?? IPAddress.Loopback;
                        string msgHTTPProxy1 = "Local HTTP Proxy:" + NL;
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxy1, Color.LightGray));
                        string msgHTTPProxy2 = $"http://{localIP}:{httpProxyPort}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxy2 + NL, Color.DodgerBlue));
                    }
                    else
                    {
                        // Update bool
                        IsSharing = false;

                        // Write Sharing Error to log
                        string msgHTTPProxyError = $"HTTP Proxy Server couldn't run.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgHTTPProxyError, Color.IndianRed));
                    }
                }
            }
            else
            {
                // Stop Fake Proxy
                if (FakeProxy != null && FakeProxy.IsRunning)
                    FakeProxy.Stop();

                // Stop HTTP Proxy
                if (HTTPProxy != null)
                {
                    if (HTTPProxy.IsRunning)
                    {
                        // Unset Proxy First
                        if (IsProxySet) SetProxy();
                        Task.Delay(100).Wait();
                        if (IsProxySet) Network.UnsetProxy(false, true);

                        HTTPProxy.Stop();
                        Task.Delay(500).Wait();

                        if (!HTTPProxy.IsRunning)
                        {
                            // Update bool
                            IsSharing = false;
                            IsProxyDPIActive = false;

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
                }
                else
                {
                    // Already deactivated
                    string msg = $"It's already deactivated.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                }
            }
        }

        public async Task<bool> ApplyPUpStreamProxy(HTTPProxyServer httpProxy)
        {
            if (!CustomCheckBoxSettingHTTPProxyUpstream.Checked) return true;

            // Get Upstream Settings
            // Upstream Mode
            string? upstreamModeString = CustomComboBoxSettingHttpProxyUpstreamMode.SelectedItem as string;

            // Check if Mode is empty
            if (string.IsNullOrEmpty(upstreamModeString))
            {
                string msg = "Select the mode of upstream proxy." + NL;
                CustomRichTextBoxLog.AppendText(msg, Color.IndianRed);
                return false;
            }

            HTTPProxyServer.Program.UpStreamProxy.Mode upstreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.Disable;
            if (upstreamModeString.Equals("HTTP"))
                upstreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.HTTP;
            else if (upstreamModeString.Equals("SOCKS5"))
                upstreamMode = HTTPProxyServer.Program.UpStreamProxy.Mode.SOCKS5;

            // Upstream Host
            string upstreamHost = CustomTextBoxSettingHTTPProxyUpstreamHost.Text;

            // Check if Host is empty
            if (string.IsNullOrWhiteSpace(upstreamHost))
            {
                string msg = "Upstream proxy host cannot be empty." + NL;
                CustomRichTextBoxLog.AppendText(msg, Color.IndianRed);
                return false;
            }

            // Upstream Port
            int upstreamPort = Convert.ToInt32(CustomNumericUpDownSettingHTTPProxyUpstreamPort.Value);

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
            bool isUpstreamProxyOk = await Network.CheckProxyWorks($"https://{blockedDomain}", upstreamProxyScheme, 15);
            if (!isUpstreamProxyOk)
            {
                string msg = $"Upstream proxy cannot open {blockedDomain}." + NL;
                CustomRichTextBoxLog.AppendText(msg, Color.IndianRed);
                return false;
            }

            HTTPProxyServer.Program.UpStreamProxy upStreamProxyProgram = new();
            upStreamProxyProgram.Set(upstreamMode, upstreamHost, upstreamPort, true);

            // Apply UpStream Proxy Program to HTTP Proxy
            httpProxy.EnableUpStreamProxy(upStreamProxyProgram);

            return true;
        }

        public bool ApplyPDNS(HTTPProxyServer httpProxy)
        {
            // Create DNS Program
            HTTPProxyServer.Program.Dns dnsProgram = new();

            // Set timeout
            int timeoutSec = 5;

            if (CustomCheckBoxSettingHTTPProxyEnableFakeProxy.Checked)
            {
                // Start msg
                string msgStart = $"Starting Fake Proxy server...{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStart, Color.Orange));

                // Get Fake Proxy DoH Address
                string dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text;

                // Get Clean Ip
                string dohCleanIP = CustomTextBoxSettingFakeProxyDohCleanIP.Text;

                // Get loopback
                string loopback = IPAddress.Loopback.ToString();

                int getNextPort(int currentPort)
                {
                    currentPort = currentPort < 65535 ? currentPort + 1 : currentPort - 1;
                    if (currentPort == GetHTTPProxyPortSetting())
                        currentPort = currentPort < 65535 ? currentPort + 1 : currentPort - 1;
                    return currentPort;
                }

                // Check port
                int fakeProxyPort = GetFakeProxyPortSetting();
                bool isPortOk = GetListeningPort(fakeProxyPort, string.Empty, Color.Orange);
                if (!isPortOk)
                {
                    fakeProxyPort = getNextPort(fakeProxyPort);
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Trying Port {fakeProxyPort}...{NL}", Color.MediumSeaGreen));
                    bool isPort2Ok = GetListeningPort(fakeProxyPort, string.Empty, Color.Orange);
                    if (!isPort2Ok)
                    {
                        fakeProxyPort = getNextPort(fakeProxyPort);
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
                if (FakeProxy.IsRunning)
                    FakeProxy.Stop();

                FakeProxy = new();
                bool isFpOk = ApplyFakeProxy(FakeProxy);
                if (!isFpOk) return false;

                // Apply DPI Bypass Support to Fake Proxy
                ApplyPDpiChanges(FakeProxy);

                FakeProxy.Start(IPAddress.Loopback, fakeProxyPort, 20000);
                Task.Delay(500).Wait();
                //============================== End FakeProxy

                // Set Dns Program
                dnsProgram.Set(HTTPProxyServer.Program.Dns.Mode.DoH, dohUrl, dohCleanIP, timeoutSec, fakeProxyScheme);

                bool isSetCfOk = setCloudflareIPs();
                if (!isSetCfOk) return false;

                // Apply DNS Program to HTTP Proxy
                httpProxy.EnableDNS(dnsProgram);

                // End msg
                string msgEnd = $"Fake Proxy Server activated. Port: {fakeProxyPort}{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgEnd, Color.MediumSeaGreen));

                return true;
            }
            else
            {
                // Set Dns Program (System)
                dnsProgram.Set(HTTPProxyServer.Program.Dns.Mode.System, null, null, 0);

                bool isSetCfOk = setCloudflareIPs();
                if (!isSetCfOk) return false;

                // Apply DNS Program to HTTP Proxy
                httpProxy.EnableDNS(dnsProgram);

                return true;
            }

            bool setCloudflareIPs()
            {
                // Add redirect all Cloudflare IPs to a clean IP
                if (CustomCheckBoxSettingHTTPProxyCfCleanIP.Checked)
                {
                    // Get CF Clean IP
                    string cleanIP = CustomTextBoxSettingHTTPProxyCfCleanIP.Text;

                    // Check CF IP is valid
                    bool isCleanIpValid = Network.IsIPv4Valid(cleanIP, out IPAddress _);
                    if (!isCleanIpValid)
                    {
                        // CF Clean IP is not valid
                        string msg = $"Cloudflare clean IP is not valid.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                        return false;
                    }

                    // Check IP is Clean
                    bool isClean1 = Network.CanPing(cleanIP, timeoutSec * 1000);
                    bool isClean2 = Network.CanTcpConnect(cleanIP, 443, timeoutSec * 1000);
                    if (!isClean1 || !isClean2)
                    {
                        // CF Clean IP is not valid
                        string msg = $"Cloudflare clean IP is not clean.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                        return false;
                    }

                    // Add to program
                    dnsProgram.SetCloudflareIPs(cleanIP);

                    return true;
                }
                else
                    return true;
            }
        }

        public async Task<bool> ApplyPFakeDNS(HTTPProxyServer httpProxy)
        {
            if (CustomCheckBoxSettingHTTPProxyEnableFakeDNS.Checked)
            {
                // Get Rules
                string rules = string.Empty;

                try
                {
                    rules = await File.ReadAllTextAsync(SecureDNS.FakeDnsRulesPath);
                }
                catch (Exception ex)
                {
                    string msg = $"Fake DNS Error: {ex.Message}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    return false;
                }

                if (!string.IsNullOrEmpty(rules) && !string.IsNullOrWhiteSpace(rules) && rules.Contains('|'))
                {
                    // Create Fake DNS Program
                    HTTPProxyServer.Program.FakeDns fakeDnsProgram = new();
                    fakeDnsProgram.Set(HTTPProxyServer.Program.FakeDns.Mode.Text, rules);

                    // Apply Fake DNS Program to HTTP Proxy
                    httpProxy.EnableFakeDNS(fakeDnsProgram);
                }
            }

            return true;
        }

        public async Task<bool> ApplyPBlackWhiteList(HTTPProxyServer httpProxy)
        {
            if (CustomCheckBoxSettingHTTPProxyEnableBlackWhiteList.Checked)
            {
                // Get BW List
                string bwList = string.Empty;

                try
                {
                    bwList = await File.ReadAllTextAsync(SecureDNS.BlackWhiteListPath);
                }
                catch (Exception ex)
                {
                    string msg = $"Black White List Error: {ex.Message}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    return false;
                }

                if (!string.IsNullOrEmpty(bwList) && !string.IsNullOrWhiteSpace(bwList))
                {
                    // Create Black White List Program
                    HTTPProxyServer.Program.BlackWhiteList bwProgram = new();
                    bwProgram.Set(HTTPProxyServer.Program.BlackWhiteList.Mode.BlackListText, bwList);

                    // Apply Black White List Program to HTTP Proxy
                    httpProxy.EnableBlackWhiteList(bwProgram);
                }
            }

            return true;
        }


    }
}
