using CustomControls;
using MsmhTools;
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private async void SetDNS()
        {
            // Get NIC Name
            string? nicName = CustomComboBoxNICs.SelectedItem as string;

            // Check if NIC Name is empty
            if (string.IsNullOrEmpty(nicName))
            {
                string msg = "Select a Network Interface first." + NL;
                CustomRichTextBoxLog.AppendText(msg, Color.IndianRed);
                return;
            }

            // Check if NIC is null
            NetworkInterface? nic = Network.GetNICByName(nicName);
            if (nic == null) return;

            string loopbackIP = IPAddress.Loopback.ToString();
            string dnss = loopbackIP;
            if (LocalIP != null)
                dnss += "," + LocalIP;

            if (!IsDNSSet)
            {
                if (IsDNSSetting || IsDNSUnsetting) return;
                // Set DNS
                IsDNSSetting = true;
                // Write Connect first to log
                string msgConnect = string.Empty;
                if (!IsConnected)
                {
                    msgConnect = "Connect first." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                    IsDNSSetting = false;
                    return;
                }
                else if (!IsDNSConnected)
                {
                    msgConnect = "Wait until DNS gets online." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                    IsDNSSetting = false;
                    return;
                }

                // Check Internet Connectivity
                if (!IsInternetAlive())
                {
                    IsDNSSetting = false;
                    return;
                }

                // Get blocked domain
                string blockedDomain = GetBlockedDomainSetting(out string _);
                if (string.IsNullOrEmpty(blockedDomain))
                {
                    IsDNSSetting = false;
                    return;
                }

                // Show warning while connected using dnscrypt + proxy
                if (ProcessManager.FindProcessByPID(PIDDNSCrypt) && CustomRadioButtonConnectDNSCrypt.Checked)
                {
                    string msg = "Set DNS while connected via proxy is not a good idea.\nYou may break the connection.\nContinue?";
                    DialogResult dr = CustomMessageBox.Show(msg, "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dr == DialogResult.No)
                    {
                        IsDNSSetting = false;
                        return;
                    }
                }

                // Write Setting DNS to Log
                CustomRichTextBoxLog.AppendText($"Setting DNS...{NL}", Color.MediumSeaGreen);

                // Set DNS
                await Task.Run(() => Network.SetDNS(nic, dnss));
                IsDNSSet = true;

                // Flush DNS
                FlushDNS();

                // Save NIC name to file
                FileDirectory.CreateEmptyFile(SecureDNS.NicNamePath);
                File.WriteAllText(SecureDNS.NicNamePath, nicName);

                // Update Groupbox Status
                UpdateStatusLong();

                // Write Set DNS message to log
                string msg1 = "Local DNS ";
                string msg2 = loopbackIP;
                string msg3 = " set to ";
                string msg4 = nicName + " (" + nic.Description + ")";
                CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
                CustomRichTextBoxLog.AppendText(msg3, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg4 + NL, Color.DodgerBlue);

                // Go to Check Tab
                if (ConnectAllClicked && IsConnected)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 0);
                    ConnectAllClicked = false;
                }

                IsDNSSetting = false;

                // Check DPI works if DPI is Active
                if (IsDPIActive)
                    CheckDPIWorks(blockedDomain);
            }
            else
            {
                // Unset DNS
                if (IsDNSSetting || IsDNSUnsetting) return;

                // Write Unsetting DNS to Log
                CustomRichTextBoxLog.AppendText($"Unsetting DNS...{NL}", Color.MediumSeaGreen);

                IsDNSUnsetting = true;

                bool unsetToDHCP = CustomRadioButtonSettingUnsetDnsToDhcp.Checked;
                if (unsetToDHCP)
                {
                    // Unset to DHCP
                    await Task.Run(() => Network.UnsetDNS(nic));
                    Task.Delay(200).Wait();
                    await UnsetSavedDnsDHCP();
                }
                else
                {
                    // Unset to Static
                    string dns1 = CustomTextBoxSettingUnsetDns1.Text;
                    string dns2 = CustomTextBoxSettingUnsetDns2.Text;
                    await Task.Run(() => Network.UnsetDNS(nic, dns1, dns2));
                    Task.Delay(200).Wait();
                    await UnsetSavedDnsStatic(dns1, dns2);
                }

                IsDNSUnsetting = false;
                IsDNSSet = false;

                // Flush DNS
                FlushDNS();

                // Update Groupbox Status
                UpdateStatusLong();

                // Write Unset DNS message to log
                string msg1 = "Local DNS ";
                string msg2 = loopbackIP;
                string msg3 = " removed from ";
                string msg4 = $"{nicName} ({nic.Description})";

                CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
                CustomRichTextBoxLog.AppendText(msg3, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg4 + NL, Color.DodgerBlue);
            }
        }

        private async Task UnsetSavedDNS()
        {
            bool unsetToDHCP = CustomRadioButtonSettingUnsetDnsToDhcp.Checked;
            if (unsetToDHCP)
            {
                // Unset to DHCP
                await UnsetSavedDnsDHCP();
            }
            else
            {
                // Unset to Static
                string dns1 = CustomTextBoxSettingUnsetDns1.Text;
                string dns2 = CustomTextBoxSettingUnsetDns2.Text;
                await UnsetSavedDnsStatic(dns1, dns2);
            }
        }

        // Unset to DHCP
        private async Task UnsetSavedDnsDHCP()
        {
            if (File.Exists(SecureDNS.NicNamePath))
            {
                string nicName = File.ReadAllText(SecureDNS.NicNamePath).Replace(NL, string.Empty);
                if (nicName.Length > 0)
                {
                    NetworkInterface? nic = Network.GetNICByName(nicName);
                    if (nic != null)
                    {
                        await Task.Run(() => Network.UnsetDNS(nic));
                        IsDNSSet = false;
                    }
                }
            }
        }

        // Unset to Static
        private async Task UnsetSavedDnsStatic(string dns1, string dns2)
        {
            if (File.Exists(SecureDNS.NicNamePath))
            {
                string nicName = File.ReadAllText(SecureDNS.NicNamePath).Replace(NL, string.Empty);
                if (nicName.Length > 0)
                {
                    NetworkInterface? nic = Network.GetNICByName(nicName);
                    if (nic != null)
                    {
                        dns1 = dns1.Trim();
                        dns2 = dns2.Trim();
                        if (Network.IsIPv4Valid(dns1, out IPAddress _) && Network.IsIPv4Valid(dns2, out IPAddress _))
                        {
                            await Task.Run(() => Network.UnsetDNS(nic, dns1, dns2));
                            IsDNSSet = false;
                        }
                    }
                }
            }
        }

    }
}
