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
            // Get Clean Ip
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
            bool isDohIpClean2 = Network.CanPing(dohCleanIP, 443, 5000);
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

                //HTTPProxy = new();
                if (!HTTPProxy.IsRunning)
                {
                    HTTPProxy.OnRequestReceived -= HTTPProxy_OnRequestReceived;
                    HTTPProxy.OnRequestReceived += HTTPProxy_OnRequestReceived;
                    StaticDPIBypassProgram.OnChunkDetailsReceived -= StaticDPIBypassProgram_OnChunkDetailsReceived;
                    StaticDPIBypassProgram.OnChunkDetailsReceived += StaticDPIBypassProgram_OnChunkDetailsReceived;
                    HTTPProxy.OnErrorOccurred -= HTTPProxy_OnErrorOccurred;
                    HTTPProxy.OnErrorOccurred += HTTPProxy_OnErrorOccurred;

                    // Apply DPI Bypass Support
                    ApplyPDpiChanges(HTTPProxy);

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
                    int handleReq = int.Parse(CustomNumericUpDownSettingHTTPProxyHandleRequests.Value.ToString());

                    // Get and set block port 80 setting
                    HTTPProxy.BlockPort80 = CustomCheckBoxSettingProxyBlockPort80.Checked;

                    if (HTTPProxy.IsRunning)
                        HTTPProxy.Stop();

                    HTTPProxy.Start(IPAddress.Any, httpProxyPort, handleReq);
                    Task.Delay(500).Wait();

                    // Delete error log on > 1MB
                    if (File.Exists(SecureDNS.HTTPProxyServerErrorLogPath))
                    {
                        try
                        {
                            long lenth = new FileInfo(SecureDNS.HTTPProxyServerErrorLogPath).Length;
                            if (FileDirectory.ConvertSize(lenth, FileDirectory.SizeUnits.Byte, FileDirectory.SizeUnits.MB) > 1)
                                File.Delete(SecureDNS.HTTPProxyServerErrorLogPath);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Delete HTTP Proxy log file: {ex.Message}");
                        }
                    }

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
                                    if (StopWatchShowRequests.ElapsedMilliseconds > 20)
                                    {
                                        req += NL; // Adding an additional line break.
                                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(req, Color.Gray));

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
                                    if (StopWatchShowChunkDetails.ElapsedMilliseconds > 200)
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
                            error += NL; // Adding an additional line break.
                            FileDirectory.AppendTextLine(SecureDNS.HTTPProxyServerErrorLogPath, error, new UTF8Encoding(false));
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

        public bool ApplyPDNS(HTTPProxyServer httpProxy)
        {
            if (!CustomCheckBoxSettingHTTPProxyEnableFakeProxy.Checked) return true;

            // Start msg
            string msgStart = $"Starting Fake Proxy server...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStart, Color.Orange));

            // Get Fake Proxy DoH Address
            string dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text;

            // Set timeout
            int timeoutSec = 5;

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
            bool isFpOk = ApplyFakeProxy(FakeProxy);
            if (!isFpOk) return false;

            // Apply DPI Bypass Support to Fake Proxy
            ApplyPDpiChanges(FakeProxy);

            if (FakeProxy.IsRunning)
                FakeProxy.Stop();

            FakeProxy.Start(IPAddress.Loopback, fakeProxyPort, 20000);
            Task.Delay(500).Wait();
            //============================== End FakeProxy

            // Create DNS Program
            HTTPProxyServer.Program.Dns dnsProgram = new();
            dnsProgram.Set(HTTPProxyServer.Program.Dns.Mode.DoH, dohUrl, timeoutSec, fakeProxyScheme);

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
                bool isClean2 = Network.CanPing(cleanIP, 443, timeoutSec * 1000);
                if (!isClean1 || !isClean2)
                {
                    // CF Clean IP is not valid
                    string msg = $"Cloudflare clean IP is not clean.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    return false;
                }

                // Add to program
                dnsProgram.SetCloudflareIPs(cleanIP);
            }

            // Apply DNS Program to HTTP Proxy
            httpProxy.EnableDNS(dnsProgram);

            // End msg
            string msgEnd = $"Fake Proxy Server activated. Port: {fakeProxyPort}{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgEnd, Color.MediumSeaGreen));

            return true;
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
