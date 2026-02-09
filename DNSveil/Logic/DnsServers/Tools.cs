using System.Collections.ObjectModel;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using static DNSveil.Logic.DnsServers.DnsModel;

namespace DNSveil.Logic.DnsServers;

public static class Tools
{
    public static void NotifyPropertyChanged(this ObservableCollection<DnsItem> items)
    {
        foreach (DnsItem item in items) item.NotifyPropertyChanged();
    }

    public static GroupSettings Clone_GroupSettings(this GroupSettings obj)
    {
        return new GroupSettings()
        {
            IsEnabled = obj.IsEnabled,
            LookupDomain = obj.LookupDomain,
            ScanTimeoutSec = obj.ScanTimeoutSec,
            ScanParallelSize = obj.ScanParallelSize,
            BootstrapIP = obj.BootstrapIP,
            BootstrapPort = obj.BootstrapPort,
            MaxServersToConnect = obj.MaxServersToConnect,
            AllowInsecure = obj.AllowInsecure
        };
    }

    /// <summary>
    /// Clone DnsItem. Except IDUnique Property.
    /// </summary>
    public static DnsItem Clone_DnsItem(this DnsItem full, DnsItem input)
    {
        try
        {
            input.DNS_URL = full.DNS_URL;
            if (!NetworkTool.IsLocalIP(full.DNS_IP)) input.DNS_IP = full.DNS_IP;
            input.Protocol = full.Protocol;
            input.Status = full.Status;
            input.Latency = full.Latency;
            input.IsGoogleSafeSearchEnabled = full.IsGoogleSafeSearchEnabled;
            input.IsBingSafeSearchEnabled = full.IsBingSafeSearchEnabled;
            input.IsYoutubeRestricted = full.IsYoutubeRestricted;
            input.IsAdultBlocked = full.IsAdultBlocked;
            if (!string.IsNullOrWhiteSpace(full.Description)) input.Description = full.Description;
        }
        catch (Exception) { }

        return input;
    }

    public static ObservableCollection<DnsItem> RemoveRelays(this ObservableCollection<DnsItem> items)
    {
        ObservableCollection<DnsItem> result = new();

        try
        {
            for (int n = 0; n < items.Count; n++)
            {
                DnsItem item = items[n];
                string dnsWithRelay = item.DNS_URL;
                string[] split = dnsWithRelay.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 1) dnsWithRelay = split[0]; // Without Relay
                item.DNS_URL = dnsWithRelay;
                result.Add(item);
            }
        }
        catch (Exception) { }

        return result;
    }

    public static bool? DnsFilterToBool(DnsFilter dnsFilter)
    {
        return dnsFilter == DnsFilter.Yes ? true : dnsFilter == DnsFilter.No ? false : null;
    }

    public static DnsFilter BoolToDnsFilter(bool? isChecked)
    {
        try
        {
            return isChecked == null ? DnsFilter.Unknown : Convert.ToBoolean(isChecked) ? DnsFilter.Yes : DnsFilter.No;
        }
        catch (Exception)
        {
            return DnsFilter.Unknown;
        }
    }

    public static ObservableCollection<DnsItem> Convert_DNSs_To_DnsItem(ObservableCollection<string> dnss)
    {
        ObservableCollection<DnsItem> result = new();

        try
        {
            dnss = dnss.Distinct().ToObservableCollection();
            for (int n = 0; n < dnss.Count; n++)
            {
                string dns = dnss[n];
                result.Add(new DnsItem(dns));
            }
        }
        catch (Exception) { }

        return result;
    }

    public static ObservableCollection<DnsItem> Convert_DNSs_To_DnsItem_ForFragmentDoH(List<string> dnss, List<string> maliciousDomains)
    {
        ObservableCollection<DnsItem> result = new();

        try
        {
            dnss = dnss.Distinct().ToList();
            for (int n = 0; n < dnss.Count; n++)
            {
                string dns = dnss[n];
                DnsReader dr = new(dns);
                if (dr.IsDnsCryptStamp && dr.Protocol == DnsEnums.DnsProtocol.DoH)
                {
                    if (!dr.IsHostIP && !NetworkTool.IsLocalIP(dr.IP))
                    {
                        if (MaliciousDomains.IsMalicious(maliciousDomains, dns)) continue; // Ignore Malicious Domains
                        
                        string dns_URL = $"{dr.Scheme}{dr.Host}{dr.Path}";
                        if (dr.Port != 443) dns_URL = $"{dr.Scheme}{dr.Host}:{dr.Port}{dr.Path}";
                        DnsItem di = new() { DNS_URL = dns_URL, DNS_IP = dr.IP, Protocol = dr.Protocol };
                        result.Add(di);
                    }
                }
            }

            // DeDup By DNS And IP
            result = result.DistinctByProperties(_ => new { _.DNS_URL, _.DNS_IP_Str }).ToObservableCollection();
        }
        catch (Exception) { }

        return result;
    }

    public static ObservableCollection<DnsItem> Create_DnsItem_ForAnonDNSCrypt(List<string> targets, List<string> relays)
    {
        ObservableCollection<DnsItem> result = new();

        try
        {
            for (int i = 0; i < targets.Count; i++)
            {
                string anonDNSCrypt = targets[i];
                string? relay = relays.GetRandomValue();
                if (!string.IsNullOrEmpty(relay)) anonDNSCrypt += $" {relay}";
                result.Add(new DnsItem(anonDNSCrypt));
            }
        }
        catch (Exception) { }

        return result;
    }

    public static List<string> Find_Relays_ForAnonDNSCrypt(List<string> dnss, List<string> maliciousDomains)
    {
        List<string> relays = new();

        try
        {
            for (int i = 0; i < dnss.Count; i++)
            {
                string dns = dnss[i];
                DnsReader dr = new(dns);
                if (dr.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCryptRelay ||
                    dr.Protocol == DnsEnums.DnsProtocol.UDP ||
                    dr.Protocol == DnsEnums.DnsProtocol.TCP ||
                    dr.Protocol == DnsEnums.DnsProtocol.TcpOverUdp)
                {
                    if (MaliciousDomains.IsMalicious(maliciousDomains, dns)) continue; // Ignore Malicious Domains
                    relays.Add(dr.Dns);
                }
            }
        }
        catch (Exception) { }

        return relays;
    }

    public static List<string> Find_Targets_ForAnonDNSCrypt(List<string> dnss, List<string> maliciousDomains)
    {
        List<string> targets = new();

        try
        {
            for (int i = 0; i < dnss.Count; i++)
            {
                string dns = dnss[i];
                DnsReader dr = new(dns);
                if (dr.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                {
                    if (MaliciousDomains.IsMalicious(maliciousDomains, dns)) continue; // Ignore Malicious Domains
                    targets.Add(dr.Dns);
                }
            }
        }
        catch (Exception) { }

        return targets;
    }

}