using System.Diagnostics;
using System.Net;
using MsmhToolsClass;
using SecureDNSClient.DPIBasic;

namespace SecureDNSClient;

public partial class FormMain
{
    //---------------------------- Connect: Bypass Cloudflare, via GoodbyeDPI
    public async Task<bool> TryToBypassFakeProxyDohUsingGoodbyeDPIAsync(string cleanIP, int camouflageDnsPort, int timeoutMS)
    {
        if (IsDisconnecting) return false;

        // Get Fake Proxy DoH Address
        string dohUrl = CustomTextBoxSettingFakeProxyDohAddress.Text;
        NetworkTool.GetUrlDetails(dohUrl, 443, out _, out string dohHost, out _, out _, out int _, out string _, out bool _);

        // It's blocked message
        string msgBlocked = $"It's blocked.{NL}";
        msgBlocked += $"Trying to bypass {dohHost}{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBlocked, Color.Orange));

        if (IsDisconnecting) return false;

        // Start Camouflage DNS Server
        CamouflageDNSServer = new(camouflageDnsPort, dohUrl, cleanIP);
        CamouflageDNSServer.Start();
        await Task.Delay(500);
        IsBypassDNSActive = CamouflageDNSServer.IsRunning;

        // Wait for CamouflageDNSServer
        Task wait1 = Task.Run(async () =>
        {
            while (true)
            {
                if (IsDisconnecting) break;
                if (IsBypassDNSActive) break;
                await Task.Delay(100);
            }
        });
        try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

        if (IsBypassDNSActive)
        {
            if (IsDisconnecting) return false;

            string msgCfServer1 = $"Camouflage DNS Server activated. Port: ";
            string msgCfServer2 = $"{camouflageDnsPort}{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCfServer1, Color.Orange));
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCfServer2, Color.DodgerBlue));

            // Check if Camouflage DNS Server works
            CheckDns checkDns = new(true, false, GetCPUPriority());
            await checkDns.CheckDnsAsync("google.com", $"{IPAddress.Loopback}:{camouflageDnsPort}", 5000);

            if (!checkDns.IsDnsOnline)
            {
                // Restart if not responsive
                CamouflageDNSServer.Stop();
                CamouflageDNSServer.Start();
            }

            // Attempt 1
            // Write attempt 1 message to log
            string msgAttempt1 = $"Attempt 1 (Light), please wait...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAttempt1, Color.Orange));

            // Get loopback
            string loopback = IPAddress.Loopback.ToString();

            // Start dnsproxy
            string dnsproxyArgs = "-l 0.0.0.0";

            // Add Legacy DNS args
            dnsproxyArgs += " -p 53";

            // Add DoH args
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
            {
                if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
                {
                    // Get DoH Port
                    int dohPort = GetDohPortSetting();

                    // Set Connected DoH Port
                    ConnectedDohPort = dohPort;

                    dnsproxyArgs += " --https-port=" + dohPort + " --tls-crt=\"" + SecureDNS.CertPath + "\" --tls-key=\"" + SecureDNS.KeyPath + "\"";
                }
            }

            // Add Cache args
            if (CustomCheckBoxSettingEnableCache.Checked)
                dnsproxyArgs += " --cache";

            // Add upstream args
            dnsproxyArgs += $" -u {dohUrl}";

            // Add bootstrap args
            dnsproxyArgs += $" -b {loopback}:{camouflageDnsPort}";

            if (IsDisconnecting) return false;

            // Execute DNSProxy
            PIDDNSProxyBypass = ProcessManager.ExecuteOnly(SecureDNS.DnsProxy, dnsproxyArgs, true, true, SecureDNS.CurrentPath, GetCPUPriority());

            // Wait for DNSProxyBypass
            Task wait2 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsDisconnecting) break;
                    if (ProcessManager.FindProcessByPID(PIDDNSProxyBypass)) break;
                    await Task.Delay(50);
                }
            });
            try { await wait2.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

            // Create blacklist file for GoodbyeDPI
            File.WriteAllText(SecureDNS.DPIBlacklistFPPath, dohHost);

            if (ProcessManager.FindProcessByPID(PIDDNSProxyBypass))
            {
                IsConnected = true;

                // Start attempt 1
                bool success1 = await bypassCFAsync(DPIBasicBypassMode.Light);
                if (success1)
                {
                    // Success message
                    string msgBypassed1 = $"Successfully bypassed on first attempt.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBypassed1, Color.MediumSeaGreen));

                    return true;
                }
                else
                {
                    if (!IsConnected || IsDisconnecting) return false;

                    // Write attempt 1 failed message to log
                    string msgAttempt1Failed = $"{NL}Attempt 1 failed.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAttempt1Failed, Color.IndianRed));

                    // Get User Mode
                    DPIBasicBypassMode mode = GetGoodbyeDpiModeBasic();

                    // Return if it's the same mode
                    if (mode == DPIBasicBypassMode.Light) return false;

                    // Attempt 2
                    // Write attempt 2 message to log
                    string msgAttempt2 = $"Attempt 2 ({mode}), please wait...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgAttempt2, Color.Orange));

                    // Deactive GoodbyeDPI of attempt 1
                    BypassFakeProxyDohStop(false, false, true, false);

                    // Start attempt 2
                    bool success2 = await bypassCFAsync(mode);
                    if (success2)
                    {
                        // Success message
                        string msgBypassed2 = $"Successfully bypassed on second attempt.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgBypassed2, Color.MediumSeaGreen));

                        return true;
                    }
                    else
                    {
                        if (!IsConnected || IsDisconnecting) return false;

                        // Not seccess after 2 attempts
                        BypassFakeProxyDohStop(true, true, true, false);
                        string msgFailure1 = $"{NL}Failure: ";
                        string msgFailure2 = $"Camouflage mode is not compatible with your ISP.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailure1, Color.IndianRed));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgFailure2, Color.LightGray));

                        return false;
                    }
                }

                async Task<bool> bypassCFAsync(DPIBasicBypassMode bypassMode)
                {
                    // Get Bootsrap IP & Port
                    string bootstrap = GetBootstrapSetting(out int bootstrapPort).ToString();

                    if (IsDisconnecting) return false;

                    // Start GoodbyeDPI
                    DPIBasicBypass dpiBypass = new(bypassMode, CustomNumericUpDownSSLFragmentSize.Value, bootstrap, bootstrapPort);
                    string args = $"{dpiBypass.Args} --blacklist \"{SecureDNS.DPIBlacklistFPPath}\"";
                    PIDGoodbyeDPIBypass = ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, args, true, true, SecureDNS.BinaryDirPath, GetCPUPriority());

                    // Wait for DNSProxyBypass
                    Task wait3 = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (IsDisconnecting) break;
                            if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass)) break;
                            await Task.Delay(50);
                        }
                    });
                    try { await wait3.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

                    if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass))
                    {
                        return await CheckBypassWorks(timeoutMS, 10, PIDGoodbyeDPIBypass);
                    }
                    else
                    {
                        // GoodbyeDPI failed to execute
                        string msgGoodbyeDPIFailed = $"GoodbyeDPI failed to execute. Try again.{NL}";
                        if (IsDisconnecting) msgGoodbyeDPIFailed = $"Task Canceled.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgGoodbyeDPIFailed, Color.IndianRed));
                        return false;
                    }
                }
            }
            else
            {
                // DNSProxy failed to execute
                string msgDNSProxyFailed = $"DNSProxy failed to execute. Try again.{NL}";
                if (IsDisconnecting) msgDNSProxyFailed = $"Task Canceled.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDNSProxyFailed, Color.IndianRed));

                // Kill
                BypassFakeProxyDohStop(true, true, true, false);
                return false;
            }
        }
        else
        {
            // Camouflage DNS Server couldn't start
            string msg = $"Couldn't start camouflage DNS server, please try again.{NL}";
            if (IsDisconnecting) msg = $"Task Canceled.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));

            return false;
        }
    }
}