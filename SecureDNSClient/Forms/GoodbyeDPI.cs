using MsmhToolsClass;
using SecureDNSClient.DPIBasic;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    public DPIBasicBypassMode GetGoodbyeDpiModeBasic()
    {
        if (CustomRadioButtonDPIMode1.Checked) return DPIBasicBypassMode.Mode1;
        else if (CustomRadioButtonDPIMode2.Checked) return DPIBasicBypassMode.Mode2;
        else if (CustomRadioButtonDPIMode3.Checked) return DPIBasicBypassMode.Mode3;
        else if (CustomRadioButtonDPIMode4.Checked) return DPIBasicBypassMode.Mode4;
        else if (CustomRadioButtonDPIMode5.Checked) return DPIBasicBypassMode.Mode5;
        else if (CustomRadioButtonDPIMode6.Checked) return DPIBasicBypassMode.Mode6;
        else if (CustomRadioButtonDPIModeLight.Checked) return DPIBasicBypassMode.Light;
        else if (CustomRadioButtonDPIModeMedium.Checked) return DPIBasicBypassMode.Medium;
        else if (CustomRadioButtonDPIModeHigh.Checked) return DPIBasicBypassMode.High;
        else if (CustomRadioButtonDPIModeExtreme.Checked) return DPIBasicBypassMode.Extreme;
        else return DPIBasicBypassMode.Light;
    }

    private async void GoodbyeDPIBasic(DPIBasicBypassMode? mode = null, bool limitLog = false)
    {
        if (IsExiting) return;

        // Return if binary files are missing
        if (!CheckNecessaryFiles()) return;

        // Check Internet Connectivity
        if (!IsInternetOnline) return;

        // Get blocked domain
        string blockedDomain = GetBlockedDomainSetting(out string _);
        if (string.IsNullOrEmpty(blockedDomain)) return;

        // Kill GoodbyeDPI
        await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIBasic);
        await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIAdvanced);

        string args = string.Empty;
        string modeStr = string.Empty;
        string fallbackDNS = SecureDNS.BootstrapDnsIPv4.ToString();
        int fallbackDnsPort = SecureDNS.BootstrapDnsPort;
        bool isfallBackDNS = NetworkTool.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? fallBackDNSIP);
        if (isfallBackDNS && fallBackDNSIP != null)
        {
            fallbackDNS = fallBackDNSIP.ToString();
            fallbackDnsPort = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
        }

        // Get User Mode
        mode ??= GetGoodbyeDpiModeBasic();

        DPIBasicBypass dpiBypass = new(mode, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
        args = dpiBypass.Args;
        modeStr = dpiBypass.Text;

        // Execute GoodByeDPI
        PIDGoodbyeDPIBasic = ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, null, args, true, true, SecureDNS.BinaryDirPath, GetCPUPriority());

        // Wait for GoodbyeDPI
        Task wait1 = Task.Run(async () =>
        {
            while (true)
            {
                if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic)) break;
                await Task.Delay(100);
            }
        });
        try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

        if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic))
        {
            // Write DPI Mode to log
            if (!limitLog)
            {
                string msg = "GoodbyeDPI is active, mode: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(modeStr + NL, Color.DodgerBlue));
            }
            else
            {
                string msg = $"GoodbyeDPI is active, mode: {modeStr}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray));
            }

            // Set IsGoodbyeDPIActive true
            IsGoodbyeDPIBasicActive = true;
            IsGoodbyeDPIAdvancedActive = false;
            if (Visible) await UpdateStatusShortOnBoolsChangedAsync();

            // To See Status Immediately
            IsDPIActive = UpdateBoolIsDpiActive();
            if (Visible) await UpdateStatusLongAsync();
        }
        else
        {
            // Write DPI Error to log
            string msg = "GoodbyeDPI couldn't start, try again.";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
        }
    }

    private async void GoodbyeDPIAdvanced(bool limitLog = false)
    {
        if (IsExiting) return;

        // Return if binary files are missing
        if (!CheckNecessaryFiles()) return;

        // Check Internet Connectivity
        if (!IsInternetOnline) return;

        // Get blocked domain
        string blockedDomain = GetBlockedDomainSetting(out string _);
        if (string.IsNullOrEmpty(blockedDomain)) return;

        // Write IP Error to log
        if (CustomCheckBoxDPIAdvIpId.Checked)
        {
            bool isIpValid = NetworkTool.IsIPv4Valid(CustomTextBoxDPIAdvIpId.Text, out IPAddress? tempIP);
            if (!isIpValid)
            {
                string msgIp = "IP Address is not valid." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgIp, Color.IndianRed));
                return;
            }
        }

        // Write Blacklist file Error to log
        if (CustomCheckBoxDPIAdvBlacklist.Checked)
        {
            if (!File.Exists(SecureDNS.DPIBlacklistPath))
            {
                string msgError = "Blacklist file not exist." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgError, Color.IndianRed));
                return;
            }
            else
            {
                string content = File.ReadAllText(SecureDNS.DPIBlacklistPath);
                if (content.Length < 1 || string.IsNullOrWhiteSpace(content))
                {
                    string msgError = "Blacklist file is empty." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgError, Color.IndianRed));
                    return;
                }
            }
        }

        // Get args
        int checkCount = 0;
        string args = string.Empty;

        if (CustomCheckBoxDPIAdvP.Checked)
        {
            args += "-p "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvR.Checked)
        {
            args += "-r "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvS.Checked)
        {
            args += "-s "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvM.Checked)
        {
            args += "-m "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvF.Checked)
        {
            args += $"-f {CustomNumericUpDownDPIAdvF.Value} "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvK.Checked)
        {
            args += $"-k {CustomNumericUpDownDPIAdvK.Value} "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvN.Checked)
        {
            args += "-n "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvE.Checked)
        {
            args += $"-e {CustomNumericUpDownDPIAdvE.Value} "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvA.Checked)
        {
            args += "-a "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvW.Checked)
        {
            args += "-w "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvPort.Checked)
        {
            args += $"--port {CustomNumericUpDownDPIAdvPort.Value} "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvIpId.Checked)
        {
            IPAddress ip = IPAddress.Parse(CustomTextBoxDPIAdvIpId.Text);
            args += $"--ip-id {ip} "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvAllowNoSNI.Checked)
        {
            args += "--allow-no-sni "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvSetTTL.Checked)
        {
            args += $"--set-ttl {CustomNumericUpDownDPIAdvSetTTL.Value} "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvAutoTTL.Checked)
        {
            args += "--auto-ttl "; checkCount++;
            if (CustomTextBoxDPIAdvAutoTTL.Text.Length > 0 && !string.IsNullOrWhiteSpace(CustomTextBoxDPIAdvAutoTTL.Text))
                args += CustomTextBoxDPIAdvAutoTTL.Text + " ";
        }
        if (CustomCheckBoxDPIAdvMinTTL.Checked)
        {
            args += $"--min-ttl {CustomNumericUpDownDPIAdvMinTTL.Value} "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvWrongChksum.Checked)
        {
            args += "--wrong-chksum "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvWrongSeq.Checked)
        {
            args += "--wrong-seq "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvNativeFrag.Checked)
        {
            args += "--native-frag "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvReverseFrag.Checked)
        {
            args += "--reverse-frag "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvMaxPayload.Checked)
        {
            args += $"--max-payload {CustomNumericUpDownDPIAdvMaxPayload.Value} "; checkCount++;
        }
        if (CustomCheckBoxDPIAdvBlacklist.Checked)
        {
            args += $"--blacklist \"{SecureDNS.DPIBlacklistPath}\" "; checkCount++;
        }

        string fallbackDNS = SecureDNS.BootstrapDnsIPv4.ToString();
        int fallbackDnsPort = SecureDNS.BootstrapDnsPort;
        bool isfallBackDNS = NetworkTool.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? fallBackDNSIP);
        if (isfallBackDNS && fallBackDNSIP != null)
        {
            fallbackDNS = fallBackDNSIP.ToString();
            fallbackDnsPort = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
        }

        if (checkCount > 0)
        {
            args += $"--dns-addr {fallbackDNS} --dns-port {fallbackDnsPort} --dnsv6-addr {SecureDNS.BootstrapDnsIPv6} --dnsv6-port {SecureDNS.BootstrapDnsPort}";
        }

        // Write Args Error to log
        if (args.Length < 1 && string.IsNullOrWhiteSpace(args))
        {
            string msgError = "Error occurred: Arguments." + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgError, Color.IndianRed));
            return;
        }

        // Kill GoodbyeDPI
        await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIBasic);
        await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIAdvanced);
        await Task.Delay(100);

        string modeStr = "Advanced";

        // Execute GoodByeDPI
        PIDGoodbyeDPIAdvanced = ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, null, args, true, true, SecureDNS.BinaryDirPath, GetCPUPriority());
        await Task.Delay(100);

        if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced))
        {
            // Write DPI Mode to log
            if (!limitLog)
            {
                string msg = "GoodbyeDPI is active, mode: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(modeStr + NL, Color.DodgerBlue));
            }
            else
            {
                string msg = $"GoodbyeDPI is active, mode: {modeStr}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray));
            }

            // Set IsGoodbyeDPIActive true
            IsGoodbyeDPIAdvancedActive = true;
            IsGoodbyeDPIBasicActive = false;
            if (Visible) await UpdateStatusShortOnBoolsChangedAsync();

            // To See Status Immediately
            IsDPIActive = UpdateBoolIsDpiActive();
            if (Visible) await UpdateStatusLongAsync();
        }
        else
        {
            // Write DPI Error to log
            string msg = "GoodbyeDPI couldn't start, try again.";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
        }
    }

    private static async Task DeleteGoodbyeDpiAndWinDivertServices_Async()
    {
        await Task.Delay(100);
        string service1 = "GoodbyeDPI", service2 = "WinDivert";
        await ServiceTool.DeleteWhereAsync(service1);
        await DriverTool.DeleteWhereAsync(service1);
        await ServiceTool.DeleteWhereAsync(service2);
        await DriverTool.DeleteWhereAsync(service2);
    }

    private async void GoodbyeDPIDeactive(bool deactiveBasic, bool deactiveAdvanced)
    {
        if (!deactiveBasic && !deactiveAdvanced) return;

        deactiveBasic = deactiveBasic && IsGoodbyeDPIBasicActive;
        deactiveAdvanced = deactiveAdvanced && IsGoodbyeDPIAdvancedActive;

        // Kill GoodbyeDPI Basic
        if (deactiveBasic)
        {
            await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIBasic);
            await DeleteGoodbyeDpiAndWinDivertServices_Async();
        }

        // Kill GoodbyeDPI Advanced
        if (deactiveAdvanced)
        {
            await ProcessManager.KillProcessByPidAsync(PIDGoodbyeDPIAdvanced);
            await DeleteGoodbyeDpiAndWinDivertServices_Async();
        }

        Task wait1 = Task.Run(async () =>
        {
            while (true)
            {
                if (deactiveBasic && deactiveAdvanced)
                {
                    if (!ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic) &&
                        !ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced)) break;
                }
                else
                {
                    if (deactiveBasic && !ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic)) break;
                    if (deactiveAdvanced && !ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced)) break;
                }
                await Task.Delay(100);
            }
        });
        try { await wait1.WaitAsync(TimeSpan.FromSeconds(10)); } catch (Exception) { }

        // Write to log Basic
        if (deactiveBasic)
        {
            if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic))
            {
                string msgDC = "Couldn't deactivate GoodbyeDPI (Basic). Try again." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDC, Color.IndianRed));
            }
            else
            {
                // Set IsGoodbyeDPIBasicActive to False
                IsGoodbyeDPIBasicActive = false;
                if (Visible) await UpdateStatusShortOnBoolsChangedAsync();

                string msgDC = "GoodbyeDPI (Basic) deactivated." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDC, Color.LightGray));

                PIDGoodbyeDPIBasic = -1;

                // To See Status Immediately
                IsDPIActive = UpdateBoolIsDpiActive();
                if (Visible) await UpdateStatusLongAsync();
            }
        }

        // Write to log Advanced
        if (deactiveAdvanced)
        {
            if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced))
            {
                string msgDC = "Couldn't deactivate GoodbyeDPI (Advanced). Try again." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDC, Color.IndianRed));
            }
            else
            {
                // Set IsGoodbyeDPIAdvancedActive to False
                IsGoodbyeDPIAdvancedActive = false;
                if (Visible) await UpdateStatusShortOnBoolsChangedAsync();

                string msgDC = "GoodbyeDPI (Advanced) deactivated." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDC, Color.LightGray));

                PIDGoodbyeDPIAdvanced = -1;

                // To See Status Immediately
                IsDPIActive = UpdateBoolIsDpiActive();
                if (Visible) await UpdateStatusLongAsync();
            }
        }

    }
}