using CustomControls;
using MsmhToolsClass;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task SetDNS(string? nicName = null, bool unset = false)
    {
        // Get NIC Name
        if (string.IsNullOrEmpty(nicName))
            this.InvokeIt(() => nicName = CustomComboBoxNICs.SelectedItem as string);

        // Check if NIC is Ok
        bool isNicOk = IsNicOk(nicName, out NetworkInterface? nic);
        if (!isNicOk || nic == null) return;

        string loopbackIP = IPAddress.Loopback.ToString();
        string dnss = loopbackIP;
        //if (LocalIP != null)
        //    dnss += "," + LocalIP;

        bool isDnsSetOn = SetDnsOnNic_.IsDnsSet(LastNicName);
        if (!isDnsSetOn)
        {
            if (unset) return;
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
            string blockedDomain = GetBlockedDomainSetting(out _);
            if (string.IsNullOrEmpty(blockedDomain))
            {
                IsDNSSetting = false;
                return;
            }

            // Show warning while connected using dnscrypt + proxy
            if (ProcessManager.FindProcessByPID(PIDDNSCrypt) && CustomRadioButtonConnectDNSCrypt.Checked)
            {
                string msg = "Set DNS while connected via proxy is not a good idea.\nYou may break the connection.\nContinue?";
                DialogResult dr = CustomMessageBox.Show(this, msg, "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.No)
                {
                    IsDNSSetting = false;
                    return;
                }
            }

            // Write Setting DNS to Log
            CustomRichTextBoxLog.AppendText($"Setting DNS...{NL}", Color.MediumSeaGreen);

            // Set DNS
            await SetDnsOnNic_.SetDns(nic, dnss);
            LastNicName = nic.Name;
            IsDNSSet = true;
            DoesDNSSetOnce = true;

            // Flush DNS
            if (!IsDisconnecting && !IsDisconnectingAll) await FlushDNS();

            // Update Groupbox Status
            UpdateStatusLong();

            // Write Set DNS message to log
            string msg1 = "Local DNS ";
            string msg2 = loopbackIP;
            string msg3 = " set to ";
            string msg4 = nicName + " (" + nic.Description + ")";
            if (!IsDisconnecting && !IsDisconnectingAll)
            {
                CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
                CustomRichTextBoxLog.AppendText(msg3, Color.LightGray);
                CustomRichTextBoxLog.AppendText(msg4 + NL, Color.DodgerBlue);
            }
            
            IsDNSSetting = false;

            // Check DPI works if DPI is Active
            if (IsDPIActive && !IsDisconnecting && !IsDisconnectingAll)
                await CheckDPIWorks(blockedDomain);
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
                await SetDnsOnNic_.UnsetDnsToDHCP(nic);
            }
            else
            {
                // Unset to Static
                string dns1 = CustomTextBoxSettingUnsetDns1.Text;
                string dns2 = CustomTextBoxSettingUnsetDns2.Text;

                await SetDnsOnNic_.UnsetDnsToStatic(dns1, dns2, nic);
            }

            LastNicName = nic.Name;
            IsDNSUnsetting = false;
            IsDNSSet = false;

            // Flush DNS
            await FlushDNS();

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

    private bool IsNicOk(string? nicName, out NetworkInterface? nic)
    {
        nic = null;

        if (string.IsNullOrEmpty(nicName))
        {
            string msg = $"Select a Network Interface first.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            return false;
        }

        NetworkInterfaces nis = new(nicName);
        Task.Delay(300).Wait();
        if (nis.NetConnectionStatus != 2)
        {
            string msgNotConnected = $"Network Adapter \"{nis.NetConnectionID}\" Status: {nis.NetConnectionStatusMessage}{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNotConnected, Color.IndianRed));
            return false;
        }

        NetworkInterface? nicOut = NetworkTool.GetNICByName(nicName);
        if (nicOut == null)
        {
            string msgNicNotExist = $"Network Interface \"{nicName}\" does not exist.{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNicNotExist, Color.IndianRed));
            return false;
        }

        // Set Last Nicname
        LastNicName = nicName;

        nic = nicOut;
        return true;
    }

    private async Task UnsetDNS(NetworkInterface? nic)
    {
        bool unsetToDHCP = CustomRadioButtonSettingUnsetDnsToDhcp.Checked;
        if (unsetToDHCP)
        {
            // Unset to DHCP
            await SetDnsOnNic_.UnsetDnsToDHCP(nic);
        }
        else
        {
            // Unset to Static
            string dns1 = CustomTextBoxSettingUnsetDns1.Text;
            string dns2 = CustomTextBoxSettingUnsetDns2.Text;
            await SetDnsOnNic_.UnsetDnsToStatic(dns1, dns2, nic);
        }
    }

    private async Task UnsetDNS(string nicName)
    {
        NetworkInterface? nic = NetworkTool.GetNICByName(nicName);
        if (nic == null) return;
        await UnsetDNS(nic);
    }

    private async Task UnsetDNS(CustomComboBox ccb)
    {
        string? nicName = string.Empty;
        this.InvokeIt(() => nicName = ccb.SelectedItem as string);
        if (string.IsNullOrEmpty(nicName)) return;
        await UnsetDNS(nicName);
    }

    private async Task UnsetAllDNSs()
    {
        List<DnsOnNic> dnsList = SetDnsOnNic_.GetDnsList;
        for (int n = 0; n < dnsList.Count; n++)
            if (dnsList[n].IsSet)
                await UnsetDNS(dnsList[n].NIC);
    }

    private async Task UnsetSavedDNS()
    {
        bool unsetToDHCP = CustomRadioButtonSettingUnsetDnsToDhcp.Checked;
        if (unsetToDHCP)
        {
            // Unset to DHCP
            await SetDnsOnNic_.UnsetSavedDnssToDHCP();
        }
        else
        {
            // Unset to Static
            string dns1 = CustomTextBoxSettingUnsetDns1.Text;
            string dns2 = CustomTextBoxSettingUnsetDns2.Text;
            await SetDnsOnNic_.UnsetSavedDnssToStatic(dns1, dns2);
        }
    }

}