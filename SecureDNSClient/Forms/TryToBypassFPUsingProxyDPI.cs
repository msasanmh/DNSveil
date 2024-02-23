using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using MsmhToolsClass.ProxyServerPrograms;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    //---------------------------- Connect: Bypass Cloudflare, via Proxy DPI Bypass
    public async Task<bool> TryToBypassFakeProxyDohUsingProxyDPIAsync(string cleanIP, int fakeProxyPort, int timeoutMS)
    {
        if (IsDisconnecting) return false;

        // Get Fake Proxy DoH Address
        string dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text;
        NetworkTool.GetUrlDetails(dohUrl, 443, out _, out string dohHost, out _, out _, out int _, out string _, out bool _);

        // It's Blocked Message
        string msgBlocked = $"It's blocked.{NL}";
        msgBlocked += $"Trying to bypass {dohHost}{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBlocked, Color.Orange));

        // Get Fragment Settings
        int beforSniChunks = Convert.ToInt32(CustomNumericUpDownPDpiBeforeSniChunks.Value);
        int chunkModeInt = -1;
        this.InvokeIt(() => chunkModeInt = CustomComboBoxPDpiSniChunkMode.SelectedIndex);
        ProxyProgram.Fragment.ChunkMode chunkMode = chunkModeInt switch
        {
            0 => ProxyProgram.Fragment.ChunkMode.SNI,
            1 => ProxyProgram.Fragment.ChunkMode.SniExtension,
            2 => ProxyProgram.Fragment.ChunkMode.AllExtensions,
            _ => ProxyProgram.Fragment.ChunkMode.AllExtensions,
        };

        int sniChunks = Convert.ToInt32(CustomNumericUpDownPDpiSniChunks.Value);
        int antiPatternOffset = Convert.ToInt32(CustomNumericUpDownPDpiAntiPatternOffset.Value);
        int fragmentDelay = Convert.ToInt32(CustomNumericUpDownPDpiFragDelay.Value);

        // Get KillOnCpuUsage Setting
        int killOnCpuUsage = GetKillOnCpuUsageSetting();

        string command;
        bool isCmdSent = false;

        // Kill If it's Already Running
        ProcessManager.KillProcessByPID(PIDCamouflageProxy);

        PIDCamouflageProxy = CamouflageProxyConsole.Execute(SecureDNS.ProxyServerPath, null, true, true, SecureDNS.CurrentPath, GetCPUPriority());
        
        // Wait for Camouflage Proxy
        Task camouflageWait = Task.Run(async () =>
        {
            while (true)
            {
                if (ProcessManager.FindProcessByPID(PIDCamouflageProxy)) break;
                await Task.Delay(50);
            }
        });
        try { await camouflageWait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

        if (!ProcessManager.FindProcessByPID(PIDCamouflageProxy)) return false;
        
        // Apply DPI Bypass Program
        command = $"Programs Fragment -Mode=Program -BeforeSniChunks={beforSniChunks} -ChunkMode={chunkMode} -SniChunks={sniChunks} -AntiPatternOffset={antiPatternOffset} -FragmentDelay={fragmentDelay}";
        isCmdSent = await CamouflageProxyConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;
        
        // Apply Fake DNS and White List Program
        bool isOk = await ApplyFakeProxy(CamouflageProxyConsole);
        if (!isOk) return false;

        // Send Parent Process Command
        string parentCommand = $"ParentProcess -PID={Environment.ProcessId}";
        Debug.WriteLine(parentCommand);
        await CamouflageProxyConsole.SendCommandAsync(parentCommand);

        // Send Setting Command
        command = $"Setting -Port={fakeProxyPort} -MaxRequests=1000 -RequestTimeoutSec=0 -KillOnCpuUsage={killOnCpuUsage} -BlockPort80=True";
        isCmdSent = await CamouflageProxyConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        if (IsDisconnecting) return false;
        
        // Start
        command = "Start";
        isCmdSent = await CamouflageProxyConsole.SendCommandAsync(command);
        if (!isCmdSent) return false;

        // Check for successfull comunication with console
        isCmdSent = await CamouflageProxyConsole.SendCommandAsync("out");
        string confirmMsg = "details|true";
        
        // Wait For Confirm Message
        Task result = Task.Run(async () =>
        {
            while (true)
            {
                if (!isCmdSent) break;
                if (CamouflageProxyConsole.GetStdout.ToLower().StartsWith(confirmMsg)) break;
                await Task.Delay(100);
            }
        });
        try { await result.WaitAsync(TimeSpan.FromSeconds(10)); } catch (Exception) { }

        if (!isCmdSent || !CamouflageProxyConsole.GetStdout.ToLower().StartsWith(confirmMsg)) return false;

        IsBypassProxyActive = true;

        if (IsBypassProxyActive)
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

                ProcessManager.KillProcessByPID(PIDCamouflageProxy);
                return false;
            }

            // Get Proxy Scheme
            string proxyScheme = $"http://{IPAddress.Loopback}:{fakeProxyPort}";

            // Get Bootstrap IP & Port
            IPAddress bootstrap = GetBootstrapSetting(out int bootstrapPort);

            // Edit DNSCrypt Config File
            DNSCryptConfigEditor dnsCryptConfig = new(SecureDNS.DNSCryptConfigFakeProxyPath);
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
            NetworkTool.GetUrlDetails(dohUrl, 443, out _, out string host, out _, out _, out int port, out string path, out bool _);
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
            await Task.Delay(200);

            // Args
            string args = $"-config \"{SecureDNS.DNSCryptConfigFakeProxyPath}\"";

            if (IsDisconnecting) return false;

            // Execute DNSCrypt
            PIDDNSCryptBypass = ProcessManager.ExecuteOnly(SecureDNS.DNSCrypt, args, true, true);

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

                bool success = await CheckBypassWorks(timeoutMS, 15, PIDDNSCryptBypass);
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
                        string msgFailure2 = $"Change Fragment settings on Share tab and try again.{NL}";
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