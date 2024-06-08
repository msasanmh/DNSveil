using CustomControls;
using MsmhToolsClass;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SecureDNSClient;

public class SetDnsOnNic
{
    private List<NetworkTool.NICResult> NICs { get; set; } = new();
    private string SavedDnssPath { get; set; } = SecureDNS.NicNamePath;

    public readonly struct DefaultNicName
    {
        public static readonly string Auto = "Automatic";
    }

    public class ActiveNICs
    {
        public List<string> NICs { get; set; } = new();
        public async Task<string> PrimaryNic(IPAddress bootstrapIP, int bootstrapPort)
        {
            List<string> nics = NICs.ToList();
            int count = nics.Count;
            if (count > 1)
            {
                for (int n = 0; n < count; n++)
                {
                    string nicName = nics[n];
                    if (!string.IsNullOrEmpty(nicName))
                    {
                        NetworkInterface? nic = NetworkTool.GetNICByName(nicName);
                        if (nic != null)
                        {
                            if (nic.OperationalStatus == OperationalStatus.Up)
                            {
                                IPInterfaceStatistics statistics = nic.GetIPStatistics();
                                long br1 = statistics.BytesReceived;
                                long bs1 = statistics.BytesSent;
                                if (br1 > 0 && bs1 > 0)
                                {
                                    try
                                    {
                                        using TcpClient client = new();
                                        client.ReceiveTimeout = 200;
                                        client.SendTimeout = 200;
                                        await client.ConnectAsync(bootstrapIP, bootstrapPort);
                                    }
                                    catch (Exception) { }

                                    statistics = nic.GetIPStatistics();
                                    long br2 = statistics.BytesReceived;
                                    long bs2 = statistics.BytesSent;
                                    if (br2 > br1 || bs2 > bs1)
                                    {
                                        return nicName;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return nics.Any() ? nics[0] : string.Empty;
        }
    }

    public SetDnsOnNic() { }

    private void SaveToFile()
    {
        string result = string.Empty;
        List<NetworkTool.NICResult> nics = NICs.ToList();
        for (int n = 0; n < nics.Count; n++)
        {
            if (nics[n].IsDnsSetToLoopback)
            {
                result += nics[n].NIC_Name + Environment.NewLine;
            }
        }

        try
        {
            // Save NIC names to file
            FileDirectory.CreateEmptyFile(SavedDnssPath);
            File.WriteAllText(SavedDnssPath, result);
        }
        catch (Exception) { }
    }

    private bool IsUpdatingNics = false;
    public async Task<List<string>> UpdateNICs(CustomComboBox ccb, IPAddress bootstrapIP, int bootstrapPort, bool selectActiveNic = false, bool selectAuto = false)
    {
        List<string> activeNicNameList = new();

        if (IsUpdatingNics) return activeNicNameList;
        IsUpdatingNics = true;

        await Task.Run(async () =>
        {
            try
            {
                ccb.InvokeIt(() => ccb.Text = "Loading Network Adapters...");
                ccb.InvokeIt(() => ccb.DropDownHeight = 1);
                string? item = null;
                ccb.InvokeIt(() => item = ccb.SelectedItem as string);

                ccb.InvokeIt(() => ccb.Items.Clear());
                List<NetworkTool.NICResult> nics = NetworkTool.GetAllNetworkInterfaces();

                if (nics.Count < 1)
                {
                    Debug.WriteLine("There is no Network Interface.");
                    ccb.InvokeIt(() => ccb.Text = "There Is Not Any Network Adapter");
                    ccb.InvokeIt(() => ccb.SelectedIndex = -1);
                    return;
                }

                // Add NICs To ComboBox
                ccb.InvokeIt(() => ccb.Items.Add(DefaultNicName.Auto));
                ccb.InvokeIt(() => ccb.Items.AddRange(nics.Select(x => x.NIC_Name).ToArray()));

                if (ccb.Items.Count > 0)
                {
                    bool exist = false;
                    for (int i = 0; i < ccb.Items.Count; i++)
                    {
                        string? selectedItem = null;
                        ccb.InvokeIt(() => selectedItem = ccb.Items[i] as string);
                        if (!string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(selectedItem) &&
                            item.Equals(selectedItem) && !item.Equals(DefaultNicName.Auto))
                        {
                            exist = true;
                            break;
                        }
                    }

                    string activeNicNamePrimary = string.Empty;
                    IsDnsSet(ccb, out _, out ActiveNICs activeNICs);
                    if (selectActiveNic && !selectAuto)
                    {
                        activeNicNamePrimary = await activeNICs.PrimaryNic(bootstrapIP, bootstrapPort);
                    }

                    ccb.InvokeIt(() =>
                    {
                        if (selectAuto)
                        {
                            activeNicNameList = activeNICs.NICs;
                            ccb.SelectedItem = DefaultNicName.Auto;
                        }
                        else if (selectActiveNic)
                        {
                            if (!string.IsNullOrEmpty(activeNicNamePrimary))
                            {
                                activeNicNameList.Add(activeNicNamePrimary);
                                try { ccb.SelectedItem = activeNicNamePrimary; } catch (Exception) { }
                            }
                            else
                            {
                                ccb.SelectedIndex = -1;
                            }
                        }
                        else
                        {
                            if (exist && !string.IsNullOrEmpty(item))
                            {
                                activeNicNameList.Add(item);
                                ccb.SelectedItem = item;
                            }
                            else
                            {
                                activeNicNameList = activeNICs.NICs;
                                ccb.SelectedItem = DefaultNicName.Auto;
                            }
                        }

                        // Set DropDown Height
                        ccb.DropDownHeight = 500;
                    });
                }
                else
                {
                    ccb.InvokeIt(() =>
                    {
                        ccb.SelectedIndex = -1;
                    });
                }

                ccb.InvokeIt(() =>
                {
                    if (ccb.SelectedIndex == -1)
                    {
                        ccb.Text = "Select A Network Adapter";
                        ccb.Refresh();
                    }
                });
            }
            catch (Exception) { }
        });

        IsUpdatingNics = false;
        return activeNicNameList;
    }

    public bool IsDnsSet(CustomComboBox ccb, out bool isDnsSetOn, out ActiveNICs activeNICs)
    {
        // We Need To Get The New NIC With New Properties
        NICs = NetworkTool.GetNetworkInterfaces();

        ActiveNICs activeNICsOut = new();

        isDnsSetOn = false;
        activeNICs = activeNICsOut;

        bool isAnyDnsSet = false;
        List<NetworkTool.NICResult> nics = NICs.ToList();
        for (int n = 0; n < nics.Count; n++)
        {
            NetworkTool.NICResult nicR = nics[n];
            if (nicR.IsDnsSetToLoopback) isAnyDnsSet = true;
            if (nicR.IsUpAndRunning) activeNICsOut.NICs.Add(nicR.NIC_Name);
        }
        
        bool isDnsSetOnOut = false;
        try
        {
            ccb.InvokeIt(() =>
            {
                if (ccb != null && ccb.SelectedItem != null) // Important
                {
                    string? nicName = ccb.SelectedItem as string;
                    if (!string.IsNullOrEmpty(nicName))
                    {
                        if (nicName.Equals(DefaultNicName.Auto))
                        {
                            isDnsSetOnOut = IsDnsSet(activeNICsOut.NICs);
                        }
                        else isDnsSetOnOut = IsDnsSet(nicName);
                    }
                }
            });
        }
        catch (Exception) { }

        isDnsSetOn = isDnsSetOnOut;
        activeNICs = activeNICsOut;
        return isAnyDnsSet;
    }

    public bool IsDnsSet(List<string> nicNameList)
    {
        try
        {
            // We Need To Get The New NIC With New Properties
            NICs = NetworkTool.GetNetworkInterfaces();

            int count = 0;
            for (int i = 0; i < nicNameList.Count; i++)
            {
                string nicName = nicNameList[i];
                List<NetworkTool.NICResult> nics = NICs.ToList();
                for (int n = 0; n < nics.Count; n++)
                    if (nics[n].NIC_Name.Equals(nicName) && nics[n].IsDnsSetToLoopback)
                    {
                        count++; break;
                    }
            }

            return nicNameList.Any() && nicNameList.Count == count;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SetDnsOnNic IsDnsSet 1: " + ex.Message);
            return false;
        }
    }

    public bool IsDnsSet(NetworkInterface nic)
    {
        try
        {
            // We Need To Get The New NIC With New Properties
            NICs = NetworkTool.GetNetworkInterfaces();
            List<NetworkTool.NICResult> nics = NICs.ToList();
            for (int n = 0; n < nics.Count; n++)
                if (nics[n].NIC_Name.Equals(nic.Name)) return nics[n].IsDnsSetToLoopback;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SetDnsOnNic IsDnsSet 2: " + ex.Message);
        }

        return false;
    }

    public bool IsDnsSet(string? nicName)
    {
        try
        {
            if (string.IsNullOrEmpty(nicName)) return false;
            // We Need To Get The New NIC With New Properties
            NICs = NetworkTool.GetNetworkInterfaces();
            List<NetworkTool.NICResult> nics = NICs.ToList();
            for (int n = 0; n < nics.Count; n++)
                if (nics[n].NIC_Name.Equals(nicName)) return nics[n].IsDnsSetToLoopback;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SetDnsOnNic IsDnsSet 3: " + ex.Message);
        }

        return false;
    }

    public async Task SetDns(string nicName)
    {
        await Task.Run(async () => await NetworkTool.SetDnsIPv4(nicName, IPAddress.Loopback.ToString()));
        await Task.Run(async () => await NetworkTool.SetDnsIPv6(nicName, IPAddress.IPv6Loopback.ToString()));
        SaveToFile();
    }

    public async Task SetDns(NetworkInterface nic)
    {
        await Task.Run(async () => await NetworkTool.SetDnsIPv4(nic, IPAddress.Loopback.ToString()));
        await Task.Run(async () => await NetworkTool.SetDnsIPv6(nic, IPAddress.IPv6Loopback.ToString()));
        SaveToFile();
    }

    public async Task SetDns(List<string> nicNameList)
    {
        for (int n = 0; n < nicNameList.Count; n++)
        {
            string nicName = nicNameList[n];
            await SetDns(nicName);
        }
    }

    public async Task UnsetDnsToDHCP(string nicName)
    {
        if (string.IsNullOrEmpty(nicName)) return;
        await Task.Run(async () => await NetworkTool.UnsetDnsIPv4(nicName));
        await Task.Run(async () => await NetworkTool.UnsetDnsIPv6(nicName));
        SaveToFile();
    }

    public async Task UnsetDnsToDHCP(List<string> nicNameList)
    {
        for (int n = 0; n < nicNameList.Count; n++)
        {
            string nicName = nicNameList[n];
            await UnsetDnsToDHCP(nicName);
        }
    }

    public async Task UnsetDnsToDHCP(NetworkInterface? nic)
    {
        if (nic == null) return;
        await UnsetDnsToDHCP(nic.Name);
    }

    public async Task UnsetDnsToDHCP(CustomComboBox ccb)
    {
        string? nicName = ccb.SelectedItem as string;
        if (string.IsNullOrEmpty(nicName)) return;
        await UnsetDnsToDHCP(nicName);
    }

    public async Task UnsetDnsToStatic(string dns1, string dns2, string nicName)
    {
        if (string.IsNullOrEmpty(nicName)) return;
        dns1 = dns1.Trim();
        dns2 = dns2.Trim();
        await Task.Run(async () => await NetworkTool.UnsetDnsIPv4(nicName, dns1, dns2));
        await Task.Run(async () => await NetworkTool.UnsetDnsIPv6(nicName));
        SaveToFile();
    }

    public async Task UnsetDnsToStatic(string dns1, string dns2, List<string> nicNameList)
    {
        for (int n = 0; n < nicNameList.Count; n++)
        {
            string nicName = nicNameList[n];
            await UnsetDnsToStatic(dns1, dns2, nicName);
        }
    }

    public async Task UnsetDnsToStatic(string dns1, string dns2, NetworkInterface? nic)
    {
        if (nic == null) return;
        await UnsetDnsToStatic(dns1, dns2, nic.Name);
    }

    public async Task UnsetDnsToStatic(string dns1, string dns2, CustomComboBox ccb)
    {
        string? nicName = ccb.SelectedItem as string;
        if (string.IsNullOrEmpty(nicName)) return;
        await UnsetDnsToStatic(dns1, dns2, nicName);
    }

    public async Task UnsetSavedDnssToDHCP()
    {
        if (File.Exists(SavedDnssPath))
        {
            string content = string.Empty;

            try
            {
                content = File.ReadAllText(SavedDnssPath);
            }
            catch (Exception) { }

            List<string> nicNames = content.SplitToLines();
            for (int n = 0; n < nicNames.Count; n++)
            {
                string nicName = nicNames[n].Trim();
                await Task.Run(async () => await NetworkTool.UnsetDnsIPv4(nicName));
                await Task.Run(async () => await NetworkTool.UnsetDnsIPv6(nicName));
            }
        }
    }

    public async Task UnsetSavedDnssToStatic(string dns1, string dns2)
    {
        if (File.Exists(SavedDnssPath))
        {
            dns1 = dns1.Trim();
            dns2 = dns2.Trim();

            string content = string.Empty;

            try
            {
                content = File.ReadAllText(SavedDnssPath);
            }
            catch (Exception) { }

            List<string> nicNames = content.SplitToLines();
            for (int n = 0; n < nicNames.Count; n++)
            {
                string nicName = nicNames[n].Trim();
                await Task.Run(async () => await NetworkTool.UnsetDnsIPv4(nicName, dns1, dns2));
                await Task.Run(async () => await NetworkTool.UnsetDnsIPv6(nicName));
            }
        }
    }

    public List<NetworkTool.NICResult> GetNicsList => NICs.ToList();
}