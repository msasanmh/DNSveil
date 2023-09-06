using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using MsmhToolsClass.HTTPProxyServer;
using System;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        //---------------------------- Connect: Bypass Cloudflare, via Proxy DPI Bypass
        public async Task<bool> TryToBypassFakeProxyDohUsingProxyDPIAsync(string cleanIP, int fakeProxyPort, int timeoutMS)
        {
            if (IsDisconnecting) return false;

            // Get Fake Proxy DoH Address
            string dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text;
            NetworkTool.GetUrlDetails(dohUrl, 443, out _, out string dohHost, out int _, out string _, out bool _);

            // It's blocked message
            string msgBlocked = $"It's blocked.{NL}";
            msgBlocked += $"Trying to bypass {dohHost}{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBlocked, Color.Orange));

            // Get fragment settings
            int beforSniChunks = Convert.ToInt32(CustomNumericUpDownPDpiBeforeSniChunks.Value);
            int chunkModeInt = -1;
            this.InvokeIt(() => chunkModeInt = CustomComboBoxPDpiSniChunkMode.SelectedIndex);
            HTTPProxyServer.Program.DPIBypass.ChunkMode chunkMode = chunkModeInt switch
            {
                0 => HTTPProxyServer.Program.DPIBypass.ChunkMode.SNI,
                1 => HTTPProxyServer.Program.DPIBypass.ChunkMode.SniExtension,
                2 => HTTPProxyServer.Program.DPIBypass.ChunkMode.AllExtensions,
                _ => HTTPProxyServer.Program.DPIBypass.ChunkMode.AllExtensions,
            };

            int sniChunks = Convert.ToInt32(CustomNumericUpDownPDpiSniChunks.Value);
            int antiPatternOffset = Convert.ToInt32(CustomNumericUpDownPDpiAntiPatternOffset.Value);
            int fragmentDelay = Convert.ToInt32(CustomNumericUpDownPDpiFragDelay.Value);

            // DPI Bypass Program
            HTTPProxyServer.Program.DPIBypass dpiBypassProgram = new();
            dpiBypassProgram.Set(HTTPProxyServer.Program.DPIBypass.Mode.Program, beforSniChunks, chunkMode, sniChunks, antiPatternOffset, fragmentDelay);

            //// Test Only
            //dpiBypassProgram.OnChunkDetailsReceived += DpiBypassProgram_OnChunkDetailsReceived;
            //void DpiBypassProgram_OnChunkDetailsReceived(object? sender, EventArgs e)
            //{
            //    if (sender is string msg)
            //        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.RebeccaPurple));
            //}

            // Add Programs to Proxy
            if (CamouflageHttpProxyServer.IsRunning)
                CamouflageHttpProxyServer.Stop();

            CamouflageHttpProxyServer = new();
            CamouflageHttpProxyServer.EnableDPIBypass(dpiBypassProgram);
            bool isOk = ApplyFakeProxy(CamouflageHttpProxyServer);
            if (!isOk) return false;

            //// Test Only
            //CamouflageProxyServer.OnRequestReceived += CamouflageProxyServer_OnRequestReceived;
            //void CamouflageProxyServer_OnRequestReceived(object? sender, EventArgs e)
            //{
            //    if (sender is string msg)
            //        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.RebeccaPurple));
            //}

            if (IsDisconnecting) return false;

            // Start
            CamouflageHttpProxyServer.Start(IPAddress.Loopback, fakeProxyPort, 20000);

            // Wait for CamouflageProxyServer
            Task wait1 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsDisconnecting) break;
                    if (CamouflageHttpProxyServer.IsRunning) break;
                    await Task.Delay(100);
                }
            });
            try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

            IsBypassHttpProxyActive = CamouflageHttpProxyServer.IsRunning;

            if (IsBypassHttpProxyActive)
            {
                if (IsDisconnecting) return false;

                string msgCfServer1 = $"Fake Proxy Server activated. Port: ";
                string msgCfServer2 = $"{fakeProxyPort}{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCfServer1, Color.Orange));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCfServer2, Color.DodgerBlue));

                // Check if config file exist
                if (!File.Exists(SecureDNS.DNSCryptConfigPath))
                {
                    string msg = "Error: Configuration file doesn't exist";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));

                    CamouflageHttpProxyServer.Stop();
                    return false;
                }

                // Get Proxy Scheme
                string proxyScheme = $"http://{IPAddress.Loopback}:{fakeProxyPort}";

                // Get Bootstrap IP & Port
                IPAddress bootstrap = GetBootstrapSetting(out int bootstrapPort);

                // Edit DNSCrypt Config File
                DNSCryptConfigEditor dnsCryptConfig = new(SecureDNS.DNSCryptConfigCloudflarePath);
                dnsCryptConfig.EditHTTPProxy(proxyScheme);
                dnsCryptConfig.EditBootstrapDNS(bootstrap, bootstrapPort);
                dnsCryptConfig.EditDnsCache(CustomCheckBoxSettingEnableCache.Checked);

                // Edit DNSCrypt Config File: Enable DoH
                if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
                {
                    if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
                    {
                        // Get DoH Port
                        int dohPort = GetDohPortSetting();

                        // Set Connected DoH Port
                        ConnectedDohPort = dohPort;

                        dnsCryptConfig.EnableDoH(dohPort);
                        dnsCryptConfig.EditCertKeyPath(SecureDNS.KeyPath);
                        dnsCryptConfig.EditCertPath(SecureDNS.CertPath);
                    }
                    else
                        dnsCryptConfig.DisableDoH();
                }
                else
                    dnsCryptConfig.DisableDoH();

                // Generate SDNS
                bool isSdnsGenerateSuccess = false;
                NetworkTool.GetUrlDetails(dohUrl, 443, out _, out string host, out int port, out string path, out bool _);
                DNSCryptStampGenerator stampGenerator = new();
                string host1 = "dns.cloudflare.com";
                string host2 = "cloudflare-dns.com";
                string host3 = "every1dns.com";

                if (host.Equals(host1) || host.Equals(host2) || host.Equals(host3))
                {
                    string sdns1 = stampGenerator.GenerateDoH(cleanIP, null, $"{host1}:{port}", path, null, true, true, true);
                    string sdns2 = stampGenerator.GenerateDoH(cleanIP, null, $"{host2}:{port}", path, null, true, true, true);
                    string sdns3 = stampGenerator.GenerateDoH(cleanIP, null, $"{host3}:{port}", path, null, true, true, true);

                    string[] sdnss = Array.Empty<string>();
                    if (!string.IsNullOrEmpty(sdns1))
                    {
                        Array.Resize(ref sdnss, sdnss.Length + 1);
                        sdnss[0] = sdns1;
                    }
                    if (!string.IsNullOrEmpty(sdns2))
                    {
                        Array.Resize(ref sdnss, sdnss.Length + 1);
                        sdnss[1] = sdns2;
                    }
                    if (!string.IsNullOrEmpty(sdns3))
                    {
                        Array.Resize(ref sdnss, sdnss.Length + 1);
                        sdnss[2] = sdns3;
                    }

                    if (sdnss.Length > 0)
                    {
                        isSdnsGenerateSuccess = true;
                        dnsCryptConfig.ChangePersonalServer(sdnss);
                    }
                }
                else
                {
                    string sdns = stampGenerator.GenerateDoH(cleanIP, null, $"{host}:{port}", path, null, true, true, true);
                    if (!string.IsNullOrEmpty(sdns))
                    {
                        isSdnsGenerateSuccess = true;
                        string[] sdnss = { sdns };
                        dnsCryptConfig.ChangePersonalServer(sdnss);
                    }
                }

                // Check for success sdns
                if (!isSdnsGenerateSuccess)
                {
                    string msg = $"Coudn't generate SDNS!{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                    return false;
                }
                
                // Save DNSCrypt Config File
                await dnsCryptConfig.WriteAsync();
                await Task.Delay(500);

                // Args
                string args = $"-config \"{SecureDNS.DNSCryptConfigCloudflarePath}\"";

                if (IsDisconnecting) return false;

                // Execute DNSCrypt
                PIDDNSCryptBypass = ProcessManager.ExecuteOnly(out Process _, SecureDNS.DNSCrypt, args, true, true);
                
                // Wait for DNSCrypt
                Task wait2 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (IsDisconnecting) break;
                        if (ProcessManager.FindProcessByPID(PIDDNSCryptBypass)) break;
                        await Task.Delay(100);
                    }
                });
                try { await wait2.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                if (ProcessManager.FindProcessByPID(PIDDNSCryptBypass))
                {
                    IsConnected = true;

                    bool success = CheckBypassWorks(timeoutMS, 15, PIDDNSCryptBypass);
                    if (success)
                    {
                        // Success message
                        string msgBypassed1 = $"Successfully bypassed.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBypassed1, Color.MediumSeaGreen));

                        return true;
                    }
                    else
                    {
                        // Not seccess
                        if (IsConnected)
                        {
                            string msgFailure1 = $"{NL}Failure: ";
                            string msgFailure2 = $"Change DPI bypass settings on Share tab and try again.{NL}";
                            if (IsDisconnecting) msgFailure2 = $"Task Canceled.{NL}";
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailure1, Color.IndianRed));
                            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailure2, Color.LightGray));
                        }
                        else
                        {
                            if (!ProcessManager.FindProcessByPID(PIDDNSCryptBypass))
                            {
                                string msgFailure1 = $"{NL}Failure: ";
                                string msgFailure2 = $"DNSCrypt crashed or closed by another application.{NL}";
                                if (IsDisconnecting) msgFailure2 = $"Task Canceled.{NL}";
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailure1, Color.IndianRed));
                                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailure2, Color.LightGray));
                            }
                        }

                        BypassFakeProxyDohStop(true, true, true, false);

                        return false;
                    }
                }
                else
                {
                    // DNSCrypt-Proxy failed to execute
                    string msgDNSProxyFailed = $"DNSCrypt failed to execute. Try again.{NL}";
                    if (IsDisconnecting) msgDNSProxyFailed = $"Task Canceled.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDNSProxyFailed, Color.IndianRed));

                    // Kill
                    BypassFakeProxyDohStop(true, true, true, false);
                    return false;
                }
            }
            else
            {
                if (IsDisconnecting) return false;

                // Camouflage Proxy Server couldn't start
                string msg = $"Couldn't start Fake Proxy server, please try again.{NL}";
                if (IsDisconnecting) msg = $"Task Canceled.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));

                return false;
            }
        }
    }
}
