using CustomControls;
using MsmhToolsClass;
using System.Net;
using System.Net.NetworkInformation;

namespace SecureDNSClient;

public class DnsOnNic
{
    public string NicName { get; set; } = string.Empty;
    public NetworkInterface? NIC { get; set; }
    public bool IsSet { get; set; } = false;
}

public class SetDnsOnNic
{
    private List<DnsOnNic> DnsOnNics { get; set; } = new();
    private string SavedDnssPath { get; set; } = SecureDNS.NicNamePath;

    public SetDnsOnNic()
    {

    }

    private void AddOrUpdate(NetworkInterface nic, bool isSet)
    {
        bool exist = false;

        for (int n = 0; n < DnsOnNics.Count; n++)
        {
            if (DnsOnNics[n].NicName.Equals(nic.Name))
            {
                DnsOnNics[n].IsSet = isSet;
                exist = true;
                SaveToFile();
                break;
            }
        }

        if (!exist)
        {
            DnsOnNic dnsOnNic = new();
            dnsOnNic.NicName = nic.Name;
            dnsOnNic.NIC = nic;
            dnsOnNic.IsSet = isSet;
            if (dnsOnNic.IsSet)
                DnsOnNics.Add(dnsOnNic);
            SaveToFile();
        }
    }

    private void SaveToFile()
    {
        string result = string.Empty;
        for (int n = 0; n < DnsOnNics.Count; n++)
        {
            if (DnsOnNics[n].IsSet)
            {
                result += DnsOnNics[n].NicName + Environment.NewLine;
            }
        }

        try
        {
            // Save NIC names to file
            FileDirectory.CreateEmptyFile(SavedDnssPath);
            File.WriteAllText(SavedDnssPath, result);
        }
        catch (Exception)
        {
            // do nothing
        }
    }

    public async Task SetDns(NetworkInterface nic, string dnss)
    {
        await Task.Run(async () => await NetworkTool.SetDnsIPv4(nic, dnss));
        await Task.Run(async () => await NetworkTool.UnsetDnsIPv6(nic)); // Unset IPv6
        AddOrUpdate(nic, true);
    }

    public async Task UnsetDnsToDHCP(NetworkInterface? nic)
    {
        if (nic == null) return;
        for (int n = 0; n < DnsOnNics.Count; n++)
        {
            if (DnsOnNics[n].NicName.Equals(nic.Name))
            {
                await Task.Run(async () => await NetworkTool.UnsetDnsIPv4(nic));
                await Task.Run(async () => await NetworkTool.UnsetDnsIPv6(nic));
                AddOrUpdate(nic, false);
                break;
            }
        }
    }

    public async Task UnsetDnsToDHCP(string nicName)
    {
        NetworkInterface? nic = NetworkTool.GetNICByName(nicName);
        if (nic == null) return;
        await UnsetDnsToDHCP(nic);
    }

    public async Task UnsetDnsToDHCP(CustomComboBox ccb)
    {
        string? nicName = ccb.SelectedItem as string;
        if (string.IsNullOrEmpty(nicName)) return;
        await UnsetDnsToDHCP(nicName);
    }

    public async Task UnsetDnsToStatic(string dns1, string dns2, NetworkInterface? nic)
    {
        if (nic == null) return;
        for (int n = 0; n < DnsOnNics.Count; n++)
        {
            if (DnsOnNics[n].NicName.Equals(nic.Name))
            {
                dns1 = dns1.Trim();
                dns2 = dns2.Trim();
                await Task.Run(async () => await NetworkTool.UnsetDnsIPv4(nic, dns1, dns2));
                await Task.Run(async () => await NetworkTool.UnsetDnsIPv6(nic));
                AddOrUpdate(nic, false);
                break;
            }
        }
    }

    public async Task UnsetDnsToStatic(string dns1, string dns2, string nicName)
    {
        NetworkInterface? nic = NetworkTool.GetNICByName(nicName);
        if (nic == null) return;
        await UnsetDnsToStatic(dns1, dns2, nic);
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
            catch (Exception)
            {
                // do nothing
            }

            List<string> nicNames = content.SplitToLines();
            for (int n = 0; n < nicNames.Count; n++)
            {
                string nicName = nicNames[n].Trim();
                if (!string.IsNullOrEmpty(nicName))
                {
                    NetworkInterface? nic = NetworkTool.GetNICByName(nicName);
                    if (nic != null)
                    {
                        await Task.Run(async () => await NetworkTool.UnsetDnsIPv4(nic));
                        await Task.Run(async () => await NetworkTool.UnsetDnsIPv6(nic));
                    }
                }
            }
        }
    }

    public async Task UnsetSavedDnssToStatic(string dns1, string dns2)
    {
        if (File.Exists(SavedDnssPath))
        {
            string content = string.Empty;

            try
            {
                content = File.ReadAllText(SavedDnssPath);
            }
            catch (Exception)
            {
                // do nothing
            }

            List<string> nicNames = content.SplitToLines();
            for (int n = 0; n < nicNames.Count; n++)
            {
                string nicName = nicNames[n].Trim();
                if (!string.IsNullOrEmpty(nicName))
                {
                    NetworkInterface? nic = NetworkTool.GetNICByName(nicName);
                    if (nic != null && NetworkTool.IsIPv4Valid(dns1, out IPAddress? _) && NetworkTool.IsIPv4Valid(dns2, out IPAddress? _))
                    {
                        dns1 = dns1.Trim();
                        dns2 = dns2.Trim();
                        await Task.Run(async () => await NetworkTool.UnsetDnsIPv4(nic, dns1, dns2));
                        await Task.Run(async () => await NetworkTool.UnsetDnsIPv6(nic));
                    }
                }
            }
        }
    }

    public bool IsDnsSet()
    {
        for (int n = 0; n < DnsOnNics.Count; n++)
            if (DnsOnNics[n].IsSet) return true;
        return false;
    }

    public bool IsDnsSet(NetworkInterface nic)
    {
        for (int n = 0; n < DnsOnNics.Count; n++)
            if (DnsOnNics[n].NicName.Equals(nic.Name)) return DnsOnNics[n].IsSet;
        return false;
    }

    public bool IsDnsSet(string? nicName)
    {
        if (string.IsNullOrEmpty(nicName)) return false;
        for (int n = 0; n < DnsOnNics.Count; n++)
            if (DnsOnNics[n].NicName.Equals(nicName)) return DnsOnNics[n].IsSet;
        return false;
    }

    public bool IsDnsSet(CustomComboBox ccb)
    {
        bool isDnsSet = false;
        ccb.InvokeIt(() => isDnsSet = IsDnsSet(ccb.SelectedItem.ToString()));
        return isDnsSet;
    }

    public List<DnsOnNic> GetDnsList => DnsOnNics;
}