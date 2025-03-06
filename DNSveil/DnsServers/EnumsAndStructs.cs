using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Net;
using static MsmhToolsClass.MsmhAgnosticServer.AgnosticProgram;

namespace DNSveil.DnsServers;

public static class EnumsAndStructs
{
    public enum GroupMode
    {
        Subscription,
        AnonymizedDNSCrypt,
        FragmentDoH,
        Custom,
        None
    }

    public static GroupMode Get_GroupMode(string groupModeStr)
    {
        return groupModeStr.Trim() switch
        {
            nameof(GroupMode.Subscription) => GroupMode.Subscription,
            nameof(GroupMode.AnonymizedDNSCrypt) => GroupMode.AnonymizedDNSCrypt,
            nameof(GroupMode.FragmentDoH) => GroupMode.FragmentDoH,
            nameof(GroupMode.Custom) => GroupMode.Custom,
            _ => GroupMode.None
        };
    }

    public static string Get_GroupModeName(GroupMode groupMode)
    {
        return groupMode switch
        {
            GroupMode.Subscription => "Subscription",
            GroupMode.AnonymizedDNSCrypt => "Anonymized DNSCrypt",
            GroupMode.FragmentDoH => "Fragment DoH",
            GroupMode.Custom => "Custom",
            _ => "None"
        };
    }

    public struct GroupItem
    {
        public bool Enabled { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public GroupMode Mode { get; set; } = GroupMode.None;

        public GroupItem() { }

        public GroupItem(bool enabled, string name, GroupMode mode)
        {
            Enabled = enabled;
            Name = name;
            Mode = mode;
        }
    }

    public struct DnsItem
    {
        public string IDUnique { get; set; }
        public bool Enabled { get; set; } = false;
        public string DNS_URL { get; set; } = string.Empty;
        /// <summary>
        /// For FragmentDoH
        /// </summary>
        public IPAddress DNS_IP { get; set; } = IPAddress.None;
        public readonly DnsReader DnsReader => new(DNS_URL);
        public string Protocol { get; set; } = string.Empty;
        public DnsStatus Status { get; set; } = DnsStatus.Unknown;
        public int Latency { get; set; } = -1;
        public DnsFilter IsGoogleSafeSearchEnabled { get; set; } = DnsFilter.Unknown;
        public DnsFilter IsBingSafeSearchEnabled { get; set; } = DnsFilter.Unknown;
        public DnsFilter IsYoutubeRestricted { get; set; } = DnsFilter.Unknown;
        public DnsFilter IsAdultBlocked { get; set; } = DnsFilter.Unknown;
        public string Description { get; set; } = string.Empty;

        public DnsItem()
        {
            IDUnique = Info.GetUniqueIdString(true);
        }
    }

    public static DnsItem Clone_DnsItem(DnsItem empty, DnsItem full)
    {
        empty.DNS_URL = full.DNS_URL;
        if (!full.DNS_IP.Equals(IPAddress.None) && !full.DNS_IP.Equals(IPAddress.IPv6None)) empty.DNS_IP = full.DNS_IP;
        empty.Protocol = full.Protocol;
        empty.Status = full.Status;
        empty.Latency = full.Latency;
        empty.IsGoogleSafeSearchEnabled = full.IsGoogleSafeSearchEnabled;
        empty.IsBingSafeSearchEnabled = full.IsBingSafeSearchEnabled;
        empty.IsYoutubeRestricted = full.IsYoutubeRestricted;
        empty.IsAdultBlocked = full.IsAdultBlocked;
        if (!string.IsNullOrWhiteSpace(full.Description)) empty.Description = full.Description;
        return empty;
    }

    public enum DnsStatus
    {
        Online,
        Offline,
        Skipped,
        Unknown
    }

    public static DnsStatus GetDnsStatusByName(string str)
    {
        return str switch
        {
            nameof(DnsStatus.Online) => DnsStatus.Online,
            nameof(DnsStatus.Offline) => DnsStatus.Offline,
            nameof(DnsStatus.Skipped) => DnsStatus.Skipped,
            _ => DnsStatus.Unknown
        };
    }

    public enum DnsFilter
    {
        Yes,
        No,
        Unknown
    }

    public static DnsFilter Get_DnsFilter_ByName(string str)
    {
        return str switch
        {
            nameof(DnsFilter.Yes) => DnsFilter.Yes,
            nameof(DnsFilter.No) => DnsFilter.No,
            _ => DnsFilter.Unknown
        };
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

    public struct GroupSettings
    {
        public string LookupDomain { get; set; } = "google.com";
        public double TimeoutSec { get; set; } = 4;
        public int ParallelSize { get; set; } = 5;
        public IPAddress BootstrapIP { get; set; } = IPAddress.Parse("8.8.8.8");
        public int BootstrapPort { get; set; } = 53;
        public int MaxServersToConnect { get; set; } = 5;
        public bool AllowInsecure { get; set; } = false;

        public GroupSettings() { }

        public GroupSettings(string lookupDomain, double timeoutSec, int parallelSize, IPAddress bootstrapIP, int bootstrapPort, int maxNumberOfServersToConnect, bool allowInsecure)
        {
            LookupDomain = lookupDomain;
            TimeoutSec = timeoutSec;
            ParallelSize = parallelSize;
            BootstrapIP = bootstrapIP;
            BootstrapPort = bootstrapPort;
            MaxServersToConnect = maxNumberOfServersToConnect;
            AllowInsecure = allowInsecure;
        }
    }

    public struct AutoUpdate
    {
        public int UpdateSource { get; set; } = 24;
        public int ScanServers { get; set; } = 6;

        public AutoUpdate() { }

        public AutoUpdate(int updateSource, int scanServers)
        {
            UpdateSource = updateSource;
            ScanServers = scanServers;
        }
    }

    public struct LastAutoUpdate
    {
        public DateTime LastUpdateSource { get; set; } = DateTime.MinValue;
        public DateTime LastScanServers { get; set; } = DateTime.MinValue;

        public LastAutoUpdate() { }

        public LastAutoUpdate(DateTime lastUpdateSource, DateTime lastScanServers)
        {
            LastUpdateSource = lastUpdateSource;
            LastScanServers = lastScanServers;
        }
    }

    public struct FilterByProtocols
    {
        public bool UDP { get; set; } = true;
        public bool TCP { get; set; } = true;
        public bool DnsCrypt { get; set; } = true;
        public bool DoT { get; set; } = true;
        public bool DoH { get; set; } = true;
        public bool DoQ { get; set; } = false;
        public bool AnonymizedDNSCrypt { get; set; } = true;
        public bool ObliviousDoH { get; set; } = false;

        public FilterByProtocols() { }

        public FilterByProtocols(bool udp, bool tcp, bool dnsCrypt, bool doT, bool doH, bool doQ, bool anonymizedDNSCrypt, bool obliviousDoH)
        {
            UDP = udp;
            TCP = tcp;
            DnsCrypt = dnsCrypt;
            DoT = doT;
            DoH = doH;
            DoQ = doQ;
            AnonymizedDNSCrypt = anonymizedDNSCrypt;
            ObliviousDoH = obliviousDoH;
        }
    }

    public struct FilterByProperties
    {
        public DnsFilter GoogleSafeSearch { get; set; } = DnsFilter.No;
        public DnsFilter BingSafeSearch { get; set; } = DnsFilter.No;
        public DnsFilter YoutubeRestricted { get; set; } = DnsFilter.No;
        public DnsFilter AdultBlocked { get; set; } = DnsFilter.No;

        public FilterByProperties() { }

        public FilterByProperties(DnsFilter googleSafeSearch, DnsFilter bingSafeSearch, DnsFilter youtubeRestricted, DnsFilter adultBlocked)
        {
            GoogleSafeSearch = googleSafeSearch;
            BingSafeSearch = bingSafeSearch;
            YoutubeRestricted = youtubeRestricted;
            AdultBlocked = adultBlocked;
        }
    }

    public struct FragmentSettings
    {
        public int ChunksBeforeSNI { get; set; } = 50;
        public Fragment.ChunkMode SniChunkMode { get; set; } = Fragment.ChunkMode.SNI;
        public int ChunksSNI { get; set; } = 5;
        public int AntiPatternOffset { get; set; } = 2;
        public int FragmentDelayMS { get; set; } = 1;

        public FragmentSettings() { }

        public FragmentSettings(int chunksBeforeSNI, Fragment.ChunkMode sniChunkMode, int chunksSNI, int antiPatternOffset, int fragmentDelayMS)
        {
            ChunksBeforeSNI = chunksBeforeSNI;
            SniChunkMode = sniChunkMode;
            ChunksSNI = chunksSNI;
            AntiPatternOffset = antiPatternOffset;
            FragmentDelayMS = fragmentDelayMS;
        }
    }

    public struct SubscriptionOptions
    {
        public AutoUpdate AutoUpdate { get; set; } = new();
        public FilterByProtocols FilterByProtocols { get; set; } = new();
        public FilterByProperties FilterByProperties { get; set; } = new();

        public SubscriptionOptions() { }

        public SubscriptionOptions(AutoUpdate autoUpdate, FilterByProtocols filterByProtocols, FilterByProperties filterByProperties)
        {
            AutoUpdate = autoUpdate;
            FilterByProtocols = filterByProtocols;
            FilterByProperties = filterByProperties;
        }
    }

    public struct AnonDNSCryptOptions
    {
        public AutoUpdate AutoUpdate { get; set; } = new();
        public FilterByProperties FilterByProperties { get; set; } = new();

        public AnonDNSCryptOptions() { }

        public AnonDNSCryptOptions(AutoUpdate autoUpdate, FilterByProperties filterByProperties)
        {
            AutoUpdate = autoUpdate;
            FilterByProperties = filterByProperties;
        }
    }

    public struct FragmentDoHOptions
    {
        public AutoUpdate AutoUpdate { get; set; } = new();
        public FilterByProperties FilterByProperties { get; set; } = new();
        public FragmentSettings FragmentSettings { get; set; } = new();

        public FragmentDoHOptions() { }

        public FragmentDoHOptions(AutoUpdate autoUpdate, FilterByProperties filterByProperties, FragmentSettings fragmentSettings)
        {
            AutoUpdate = autoUpdate;
            FilterByProperties = filterByProperties;
            FragmentSettings = fragmentSettings;
        }
    }

    public struct CustomOptions
    {
        public AutoUpdate AutoUpdate { get; set; } = new();
        public FilterByProtocols FilterByProtocols { get; set; } = new();
        public FilterByProperties FilterByProperties { get; set; } = new();

        public CustomOptions() { }

        public CustomOptions(AutoUpdate autoUpdate, FilterByProtocols filterByProtocols, FilterByProperties filterByProperties)
        {
            AutoUpdate = autoUpdate;
            FilterByProtocols = filterByProtocols;
            FilterByProperties = filterByProperties;
        }
    }

    public struct DnsItemsInfo
    {
        public int TotalServers { get; set; } = 0;
        public int OnlineServers { get; set; } = 0;
        public int SelectedServers { get; set; } = 0;
        public int AverageLatency { get; set; } = -1;

        public DnsItemsInfo() { }

        public DnsItemsInfo(int totalServers, int onlineServers, int selectedServers, int averageLatency)
        {
            TotalServers = totalServers;
            OnlineServers = onlineServers;
            SelectedServers = selectedServers;
            AverageLatency = averageLatency;
        }
    }

}