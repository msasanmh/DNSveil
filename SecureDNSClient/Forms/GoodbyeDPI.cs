using MsmhTools;
using SecureDNSClient.DPIBasic;
using System;
using System.Net;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private async void DPIBasic()
        {
            //// Write Connect first to log
            //if (!IsDNSConnected && !IsDoHConnected)
            //{
            //    string msgConnect = "Connect first." + NL;
            //    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
            //    return;
            //}

            // Check Internet Connectivity
            if (!IsInternetAlive()) return;

            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string _);
            if (string.IsNullOrEmpty(blockedDomain)) return;

            // Kill GoodbyeDPI
            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
                ProcessManager.KillProcessByID(PIDGoodbyeDPI);

            string args = string.Empty;
            string text = string.Empty;
            string fallbackDNS = SecureDNS.BootstrapDnsIPv4.ToString();
            int fallbackDnsPort = SecureDNS.BootstrapDnsPort;
            bool isfallBackDNS = Network.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? fallBackDNSIP);
            if (isfallBackDNS && fallBackDNSIP != null)
            {
                fallbackDNS = fallBackDNSIP.ToString();
                fallbackDnsPort = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
            }

            if (CustomRadioButtonDPIMode1.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode1, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode2.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode2, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode3.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode3, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode4.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode4, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode5.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode5, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIMode6.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Mode6, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeLight.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Light, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeMedium.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Medium, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeHigh.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.High, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }
            else if (CustomRadioButtonDPIModeExtreme.Checked)
            {
                DPIBasicBypass dpiBypass = new(DPIBasicBypassMode.Extreme, CustomNumericUpDownSSLFragmentSize.Value, fallbackDNS, fallbackDnsPort);
                args = dpiBypass.Args;
                text = dpiBypass.Text;
            }

            // Execute GoodByeDPI
            PIDGoodbyeDPI = ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, args, true, true, SecureDNS.BinaryDirPath, GetCPUPriority());

            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
            {
                // Write DPI Mode to log
                string msg = "DPI bypass is active, mode: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(text + NL, Color.DodgerBlue));

                // Update Groupbox Status
                UpdateStatusLong();

                // Set IsGoodbyeDPIActive true
                IsGoodbyeDPIActive = true;
                IsDPIActive = true;

                // Go to SetDNS Tab if it's not already set
                if (ConnectAllClicked && !IsDNSSet)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 3);
                }

                // Check DPI works
                await CheckDPIWorks(blockedDomain);
            }
            else
            {
                // Write DPI Error to log
                string msg = "DPI bypass couldn't connect, try again.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
            }
        }

        private async void DPIAdvanced()
        {
            //// Write Connect first to log
            //if (!IsDNSConnected && !IsDoHConnected)
            //{
            //    string msgConnect = "Connect first." + NL;
            //    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
            //    return;
            //}

            // Check Internet Connectivity
            if (!IsInternetAlive()) return;

            // Get blocked domain
            string blockedDomain = GetBlockedDomainSetting(out string _);
            if (string.IsNullOrEmpty(blockedDomain)) return;

            // Write IP Error to log
            if (CustomCheckBoxDPIAdvIpId.Checked)
            {
                bool isIpValid = Network.IsIPv4Valid(CustomTextBoxDPIAdvIpId.Text, out IPAddress? tempIP);
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
                args += $"--blacklist {SecureDNS.DPIBlacklistPath} "; checkCount++;
            }

            string fallbackDNS = SecureDNS.BootstrapDnsIPv4.ToString();
            int fallbackDnsPort = SecureDNS.BootstrapDnsPort;
            bool isfallBackDNS = Network.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? fallBackDNSIP);
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
            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
            {
                ProcessManager.KillProcessByID(PIDGoodbyeDPI);
                Task.Delay(100).Wait();
            }

            string text = "Advanced";

            // Execute GoodByeDPI
            PIDGoodbyeDPI = ProcessManager.ExecuteOnly(SecureDNS.GoodbyeDpi, args, true, true, SecureDNS.BinaryDirPath, GetCPUPriority());

            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
            {
                // Write DPI Mode to log
                string msg = "DPI bypass is active, mode: ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(text + NL, Color.DodgerBlue));

                // Update Groupbox Status
                UpdateStatusLong();

                // Set IsGoodbyeDPIActive true
                IsGoodbyeDPIActive = true;
                IsDPIActive = true;

                // Go to SetDNS Tab if it's not already set
                if (ConnectAllClicked && !IsDNSSet)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 3);
                }

                // Check DPI works
                await CheckDPIWorks(blockedDomain);
            }
            else
            {
                // Write DPI Error to log
                string msg = "DPI bypass couldn't connect, try again.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
            }
        }

        private void DPIDeactive()
        {
            if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
            {
                // Kill GoodbyeDPI
                ProcessManager.KillProcessByID(PIDGoodbyeDPI);

                // Update Groupbox Status
                UpdateStatusLong();

                // Write to log
                if (ProcessManager.FindProcessByID(PIDGoodbyeDPI))
                {
                    string msgDC = "Couldn't deactivate DPI Bypass. Try again." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDC, Color.IndianRed));
                }
                else
                {
                    // Set IsGoodbyeDPIActive False
                    IsGoodbyeDPIActive = false;

                    string msgDC = "DPI bypass deactivated." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDC, Color.LightGray));
                }
            }
        }
    }
}
