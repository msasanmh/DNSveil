using CustomControls;
using Microsoft.Win32;
using MsmhToolsClass;
using System.Net;
using System.Net.NetworkInformation;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task SetDNS(List<string> nicNameList, bool unset = false, bool limitLog = false)
    {
        string loopbackIPv4 = IPAddress.Loopback.ToString();
        string loopbackIPv6 = IPAddress.IPv6Loopback.ToString();

        bool isDnsSetOn = SetDnsOnNic_.IsDnsSet(nicNameList);
        if (!isDnsSetOn)
        {
            if (unset) return;
            if (IsDNSSetting || IsDNSUnsetting) return;

            // Set DNS
            IsDNSSetting = true;
            await UpdateStatusShortOnBoolsChanged();

            // Write Connect first to log
            string msgConnect = string.Empty;
            if (!IsConnected)
            {
                msgConnect = "Connect First." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                IsDNSSetting = false;
                await UpdateStatusShortOnBoolsChanged();
                return;
            }
            else if (!IsDNSConnected)
            {
                msgConnect = "Wait Until DNS Gets Online." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnect, Color.IndianRed));
                IsDNSSetting = false;
                await UpdateStatusShortOnBoolsChanged();
                return;
            }

            // Check Internet Connectivity
            if (!IsInternetOnline)
            {
                IsDNSSetting = false;
                await UpdateStatusShortOnBoolsChanged();
                return;
            }

            // Show warning while connected using upstream proxy
            if (LastConnectMode == ConnectMode.ConnectToPopularServersWithProxy && !Program.IsStartup)
            {
                string msg = "Set DNS while connected via proxy is not a good idea.\nYou may break the connection.\nContinue?";
                DialogResult dr = CustomMessageBox.Show(this, msg, "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.No)
                {
                    IsDNSSetting = false;
                    await UpdateStatusShortOnBoolsChanged();
                    return;
                }
            }

            // Write Setting DNS to Log
            CustomRichTextBoxLog.AppendText($"Setting DNS...{NL}", Color.MediumSeaGreen);

            bool setSuccess = false;
            LastNicNameList.Clear();
            for (int n = 0; n < nicNameList.Count; n++)
            {
                string nicName = nicNameList[n];

                // Check if NIC is Ok
                bool isNicOk = IsNicOk(nicName, out NetworkInterface? nic);
                if (!isNicOk || nic == null) continue;

                // Set DNS
                await SetDnsOnNic_.SetDns(nic);
                
                isDnsSetOn = SetDnsOnNic_.IsDnsSet(nic);
                if (isDnsSetOn)
                {
                    setSuccess = true;
                    IsDNSSet = true;
                    DoesDNSSetOnce = true;
                    IsDnsFlushed = false;
                    IsDnsFullFlushed = false;

                    // Write Set DNS message to log
                    if (!limitLog)
                    {
                        string msg1 = "Local DNS ";
                        string msg2 = loopbackIPv4;
                        if (nic.Supports(NetworkInterfaceComponent.IPv6))
                            msg2 += $" And {loopbackIPv6}";
                        string msg3 = " Set To ";
                        string msg4 = nicName + " (" + nic.Description + ")";
                        if (!IsDisconnecting && !IsDisconnectingAll)
                        {
                            CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                            CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
                            CustomRichTextBoxLog.AppendText(msg3, Color.LightGray);
                            CustomRichTextBoxLog.AppendText(msg4 + NL, Color.DodgerBlue);
                        }
                    }
                }
                else
                {
                    // Write Couldn't Set DNS to log
                    string msg1 = "Couldn't Set DNS ";
                    string msg2 = $"{nicName} ({nic.Description})";
                    CustomRichTextBoxLog.AppendText(msg1, Color.IndianRed);
                    CustomRichTextBoxLog.AppendText(msg2 + NL, Color.DodgerBlue);
                }
            }

            if (setSuccess)
            {
                // Flush DNS
                if (!Program.IsStartup)
                    if (!IsDisconnecting && !IsDisconnectingAll) await FlushDNS(true, false, false, false, true);

                // To See Status Immediately
                await UpdateStatusLong();
            }
            
            IsDNSSetting = false;
            await UpdateStatusShortOnBoolsChanged();
            await UpdateStatusNic();
        }
        else
        {
            // Unset DNS
            if (IsDNSSetting || IsDNSUnsetting) return;

            // Write Unsetting DNS to Log
            CustomRichTextBoxLog.AppendText($"Unsetting DNS...{NL}", Color.MediumSeaGreen);

            IsDNSUnsetting = true;
            await UpdateStatusShortOnBoolsChanged();

            bool unsetSuccess = false;
            for (int n = 0; n < nicNameList.Count; n++)
            {
                string nicName = nicNameList[n];

                NetworkInterface? nic = NetworkTool.GetNICByName(nicName);
                if (nic == null)
                {
                    string msgNicNotExist = $"Network Interface \"{nicName}\" Does Not Exist Or Disabled.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNicNotExist, Color.IndianRed));
                    continue;
                }

                // Unset DNS
                await UnsetDNS(nic);

                isDnsSetOn = SetDnsOnNic_.IsDnsSet(nic);
                if (!isDnsSetOn)
                {
                    unsetSuccess = true;

                    // Write Unset DNS message to log
                    if (!limitLog)
                    {
                        string msg1 = "Local DNS ";
                        string msg2 = loopbackIPv4;
                        if (nic.Supports(NetworkInterfaceComponent.IPv6))
                            msg2 += $" And {loopbackIPv6}";
                        string msg3 = " Removed From ";
                        string msg4 = $"{nicName} ({nic.Description})";

                        CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
                        CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
                        CustomRichTextBoxLog.AppendText(msg3, Color.LightGray);
                        CustomRichTextBoxLog.AppendText(msg4 + NL, Color.DodgerBlue);
                    }
                }
                else
                {
                    // Write Couldn't Unset DNS to log
                    string msg1 = "Couldn't Unset DNS ";
                    string msg2 = $"{nicName} ({nic.Description})";
                    CustomRichTextBoxLog.AppendText(msg1, Color.IndianRed);
                    CustomRichTextBoxLog.AppendText(msg2 + NL, Color.DodgerBlue);
                }
            }

            if (unsetSuccess)
            {
                // Flush DNS
                await FlushDNS(true, false, false, false, true);
                IsDnsFlushed = true;

                // To See Status Immediately
                await UpdateStatusLong();
            }
            
            IsDNSUnsetting = false;
            IsDNSSet = false;
            await UpdateStatusShortOnBoolsChanged();
            await UpdateStatusNic();
        }
    }

    private bool IsNicOk(string? nicName, out NetworkInterface? nic, bool writeToLog = true)
    {
        nic = null;

        if (string.IsNullOrEmpty(nicName))
        {
            string msg = $"Select A Network Interface.{NL}";
            if (writeToLog) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
            return false;
        }

        NetworkInterface? nicOut = NetworkTool.GetNICByName(nicName);
        if (nicOut == null)
        {
            string msgNicNotExist = $"Network Interface \"{nicName}\" Does Not Exist Or Disabled.{NL}";
            if (writeToLog) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNicNotExist, Color.IndianRed));
            return false;
        }

        if (nicOut.OperationalStatus != OperationalStatus.Up)
        {
            string msgNotConnected = $"Network Adapter \"{nicOut.Name}\" Is Not Up And Running.{NL}";
            if (writeToLog) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNotConnected, Color.IndianRed));
            return false;
        }

        // Set Last Nicname
        LastNicNameList.Add(nicName);

        nic = nicOut;
        return true;
    }

    private async Task UnsetDNS(NetworkInterface? nic)
    {
        if (nic == null) return;
        await UnsetDNS(nic.Name);
    }

    private async Task UnsetDNS(string nicName)
    {
        if (string.IsNullOrEmpty(nicName)) return;
        bool unsetToDHCP = CustomRadioButtonSettingUnsetDnsToDhcp.Checked;
        if (unsetToDHCP)
        {
            // Unset to DHCP
            await SetDnsOnNic_.UnsetDnsToDHCP(nicName);
        }
        else
        {
            // Unset to Static
            string dns1 = CustomTextBoxSettingUnsetDns1.Text;
            string dns2 = CustomTextBoxSettingUnsetDns2.Text;
            await SetDnsOnNic_.UnsetDnsToStatic(dns1, dns2, nicName);
        }
    }

    private async Task UnsetDNS(CustomComboBox ccb)
    {
        string? nicName = string.Empty;
        this.InvokeIt(() => nicName = ccb.SelectedItem as string);
        if (string.IsNullOrEmpty(nicName)) return;
        await UnsetDNS(nicName);
    }

    private async Task UnsetAllDNSs(bool writeToLog = false)
    {
        List<NetworkTool.NICResult> dnsList = SetDnsOnNic_.GetNicsList;
        for (int n = 0; n < dnsList.Count; n++)
        {
            NetworkTool.NICResult nicR = dnsList[n];
            if (nicR.IsDnsSetToLoopback)
            {
                if (writeToLog)
                {
                    string description = nicR.NIC != null ? nicR.NIC.Description : string.Empty;
                    if (!string.IsNullOrEmpty(description)) description = $"({description}) ";
                    string msg = $"Unsetting \"{nicR.NIC_Name}\" {description}...{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Gray));
                }
                await UnsetDNS(nicR.NIC_Name);
            }
        }
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

    private void UnsetDnsOnShutdown(SessionEndingEventArgs e)
    {
        e.Cancel = true; // We Have Zero Time To Unset DNS On Shutdown
        UnsetDnsOnShutdown();
        string msg = $"{NL}Alert System {e.Reason}{NL}Trying To Unset DNS...{NL}{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.OrangeRed));
    }

    private void UnsetDnsOnShutdown(FormClosingEventArgs e)
    {
        e.Cancel = true; // We Have Zero Time To Unset DNS On Shutdown
        UnsetDnsOnShutdown();
        string msg = $"{NL}Alert System {e.CloseReason}{NL}Trying To Unset DNS...{NL}{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.OrangeRed));
    }

    private void UnsetDnsOnShutdown()
    {
        foreach (string nicName in LastNicNameList)
        {
            string processName = "netsh";
            string processArgs1 = $"interface ipv4 delete dnsservers \"{nicName}\" all";
            ProcessManager.ExecuteOnly(processName, null, processArgs1, true, true);
            bool unsetToDHCP = CustomRadioButtonSettingUnsetDnsToDhcp.Checked;
            if (unsetToDHCP)
            {
                // DHCP
                string processArgs2 = $"interface ipv4 set dnsservers \"{nicName}\" source=dhcp";
                ProcessManager.ExecuteOnly(processName, null, processArgs2, true, true);
            }
            else
            {
                // STATIC
                string dns1 = CustomTextBoxSettingUnsetDns1.Text;
                string processArgs2 = $"interface ipv4 set dnsservers \"{nicName}\" static {dns1} primary";
                ProcessManager.ExecuteOnly(processName, null, processArgs2, true, true);
            }
        }
    }
}