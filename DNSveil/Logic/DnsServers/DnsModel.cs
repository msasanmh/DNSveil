using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text.Json.Serialization;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using static MsmhToolsClass.MsmhAgnosticServer.AgnosticProgram;

namespace DNSveil.Logic.DnsServers;

[Serializable, DebuggerDisplay("Count = {Count}")]
public class DnsModel
{
    [JsonPropertyName("Settings")]
    public DnsSettings Settings { get; set; } = new();

    [JsonPropertyName("Groups")]
    public ObservableCollection<DnsGroup> Groups { get; set; } = new();

    public class DnsSettings
    {
        public MaliciousDomains MaliciousDomains { get; set; } = new();
    }

    public class MaliciousDomains
    {
        public bool IsEnable { get; set; } = true;
        public List<string> Source_URLs { get; set; } = new();
        public int UpdateSource { get; set; } = 72;
        public DateTime LastUpdateSource { get; set; } = DateTime.MinValue;
        public List<string> Items_Server { get; set; } = new();
        public List<string> Items_User { get; set; } = new();
        public List<string> Items_UserException { get; set; } = new();

        [JsonIgnore]
        public List<string> Get_Malicious_Domains
        {
            get
            {
                List<string> result = new();

                try
                {
                    // Merge Server And User Domains
                    List<string> items = new(Items_Server);
                    items.AddRange(Items_User);
                    items = items.Distinct().ToList();

                    // Create List Without User Exceptions
                    for (int n = 0; n < items.Count; n++)
                    {
                        string domain = items[n];
                        if (domain.StartsWith("//")) continue;
                        if (Items_UserException.IsContain(domain)) continue;
                        result.Add(domain);
                    }

                    // Dispose
                    items.Clear();
                }
                catch (Exception) { }

                return result;
            }
        }

        public static bool IsMalicious(List<string> maliciousDomains, string dns)
        {
            DnsReader dr = new(dns);
            if (maliciousDomains.IsContain(dr.Host)) return true;
            NetworkTool.URL url = NetworkTool.GetUrlOrDomainDetails(dr.Host, dr.Port);
            if (maliciousDomains.IsContain(url.BaseHost)) return true;
            return false;
        }
    }

    public class DnsGroup : INotifyPropertyChanged
    {
        // When Implemented, All Binded Properties Must Have PropertyChanged Event, Otherwise Binded UI Will Face Issues.
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsBuiltIn { get; set; } = false;
        public bool IsSelected { get; set; } = false;

        [JsonIgnore]
        private string NameValue = string.Empty;
        public string Name
        {
            get => NameValue;
            set
            {
                if (NameValue == value) return;
                NameValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        [JsonIgnore]
        private GroupMode ModeValue = GroupMode.None;
        public GroupMode Mode
        {
            get => ModeValue;
            set
            {
                if (ModeValue == value) return;
                ModeValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Mode)));
            }
        }

        public string ModeStr => Get_GroupModeName(Mode);
        public GroupSettings Settings { get; set; } = new();
        public Subscription? Subscription { get; set; }
        public AnonymizedDNSCrypt? AnonymizedDNSCrypt { get; set; }
        public FragmentDoH? FragmentDoH { get; set; }
        public Custom? Custom { get; set; }
        public ObservableCollection<DnsItem> Items { get; set; } = new();

        public DnsGroup() { }

        public DnsGroup(string name, GroupMode mode)
        {
            Name = name;
            Mode = mode;
            if (Mode == GroupMode.Subscription) Subscription = new();
            else if (Mode == GroupMode.AnonymizedDNSCrypt) AnonymizedDNSCrypt = new();
            else if (Mode == GroupMode.FragmentDoH) FragmentDoH = new();
            else if (Mode == GroupMode.Custom) Custom = new();
        }
    }

    public enum GroupMode
    {
        None = 0,
        Subscription = 1,
        AnonymizedDNSCrypt = 2,
        FragmentDoH = 3,
        Custom = 4
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

    public enum DnsFilter
    {
        Unknown = 0,
        Yes = 1,
        No = 2
    }

    public enum DnsStatus
    {
        Unknown = 0,
        Online = 1,
        Offline = 2,
        Skipped = 3
    }

    public class GroupSettings
    {
        public bool IsEnabled { get; set; } = true;
        public string LookupDomain { get; set; } = "google.com";
        public double ScanTimeoutSec { get; set; } = 10;
        public int ScanParallelSize { get; set; } = 5;

        [JsonIgnore] // The IPAddress Class Is Not Friendly To Serialization
        public IPAddress BootstrapIP { get; set; } = IPAddress.Parse("8.8.8.8");
        public string BootstrapIpStr
        {
            get => BootstrapIP.ToStringNoScopeId();
            set
            {
                bool isIP = NetworkTool.IsIP(value, out IPAddress? ip);
                if (isIP && ip != null)
                {
                    if (BootstrapIP == ip) return;
                    BootstrapIP = ip;
                }
            }
        }

        public int BootstrapPort { get; set; } = 53;
        public int MaxServersToConnect { get; set; } = 5;
        public bool AllowInsecure { get; set; } = false;

        public GroupSettings() { }

        public GroupSettings(bool isEnabled, string lookupDomain, double scanTimeoutSec, int scanParallelSize, string bootstrapIpStr, int bootstrapPort, int maxServersToConnect, bool allowInsecure)
        {
            IsEnabled = isEnabled;
            LookupDomain = lookupDomain;
            ScanTimeoutSec = scanTimeoutSec;
            ScanParallelSize = scanParallelSize;
            BootstrapIpStr = bootstrapIpStr;
            BootstrapPort = bootstrapPort;
            MaxServersToConnect = maxServersToConnect;
            AllowInsecure = allowInsecure;
        }
    }

    public class Subscription
    {
        public Source Source { get; set; } = new();
        public SubscriptionOptions Options { get; set; } = new();
    }

    public class SubscriptionOptions
    {
        public AutoUpdate AutoUpdate { get; set; } = new();
        public FilterByProtocols FilterByProtocols { get; set; } = new();
        public FilterByProperties FilterByProperties { get; set; } = new();
    }

    public class AnonymizedDNSCrypt
    {
        public Source_AnonymizedDNSCrypt Source { get; set; } = new();
        public AnonymizedDNSCryptOptions Options { get; set; } = new();
        public List<string> Relays { get; set; } = new();
        public List<string> Targets { get; set; } = new();
    }

    public class AnonymizedDNSCryptOptions
    {
        public AutoUpdate AutoUpdate { get; set; } = new();
        public FilterByProperties FilterByProperties { get; set; } = new();
    }

    public class FragmentDoH
    {
        public Source Source { get; set; } = new();
        public FragmentDoHOptions Options { get; set; } = new();
    }

    public class FragmentDoHOptions
    {
        public AutoUpdate AutoUpdate { get; set; } = new();
        public FilterByProperties FilterByProperties { get; set; } = new();
        public FragmentSettings FragmentSettings { get; set; } = new();
    }

    public class Custom
    {
        public Source Source { get; set; } = new();
        public CustomOptions Options { get; set; } = new();
    }

    public class CustomOptions
    {
        public AutoUpdate_Custom AutoUpdate { get; set; } = new();
        public FilterByProtocols FilterByProtocols { get; set; } = new();
        public FilterByProperties FilterByProperties { get; set; } = new();
    }

    public class Source
    {
        public bool IsEnabled { get; set; } = true;
        public List<string> URLs { get; set; } = new();
    }

    public class Source_AnonymizedDNSCrypt
    {
        public bool IsEnabled { get; set; } = true;
        public List<string> Relay_URLs { get; set; } = new();
        public List<string> Target_URLs { get; set; } = new();
    }

    public class AutoUpdate
    {
        public int UpdateSource { get; set; } = 24;
        public DateTime LastUpdateSource { get; set; } = DateTime.MinValue;
        public int ScanServers { get; set; } = 6;
        public DateTime LastScanServers { get; set; } = DateTime.MinValue;

        public AutoUpdate() { }

        public AutoUpdate(int updateSource, DateTime lastUpdateSource, int scanServers, DateTime lastScanServers)
        {
            UpdateSource = updateSource;
            LastUpdateSource = lastUpdateSource;
            ScanServers = scanServers;
            LastScanServers = lastScanServers;
        }
    }

    public class AutoUpdate_Custom
    {
        public int ScanServers { get; set; } = 6;
        public DateTime LastScanServers { get; set; } = DateTime.MinValue;

        public AutoUpdate_Custom() { }

        public AutoUpdate_Custom(int scanServers, DateTime lastScanServers)
        {
            ScanServers = scanServers;
            LastScanServers = lastScanServers;
        }
    }

    public class FilterByProtocols
    {
        public bool UDP { get; set; } = true;
        public bool TCP { get; set; } = true;
        public bool TcpOverUdp { get; set; } = true;
        public bool DnsCrypt { get; set; } = true;
        public bool DoT { get; set; } = true;
        public bool DoH { get; set; } = true;
        public bool DoQ { get; set; } = false;
        public bool AnonymizedDNSCrypt { get; set; } = true;
        public bool ObliviousDoH { get; set; } = false;

        public FilterByProtocols() { }

        public FilterByProtocols(bool udp, bool tcp, bool tcpOverUdp, bool dnsCrypt, bool doT, bool doH, bool doQ, bool anonymizedDNSCrypt, bool obliviousDoH)
        {
            UDP = udp;
            TCP = tcp;
            TcpOverUdp = tcpOverUdp;
            DnsCrypt = dnsCrypt;
            DoT = doT;
            DoH = doH;
            DoQ = doQ;
            AnonymizedDNSCrypt = anonymizedDNSCrypt;
            ObliviousDoH = obliviousDoH;
        }
    }

    public class FilterByProperties
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

    public class FragmentSettings
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

    public class DnsItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DNS_URL)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DNS_IP)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DNS_IP_Str)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Protocol)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProtocolName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Latency)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGoogleSafeSearchEnabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBingSafeSearchEnabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsYoutubeRestricted)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAdultBlocked)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
        }

        public string IDUnique { get; set; }
        public bool IsSelected { get; set; } = false;
        public string DNS_URL { get; set; } = string.Empty;

        [JsonIgnore]
        public DnsReader DnsReader => new(DNS_URL);

        /// <summary>
        /// For FragmentDoH
        /// </summary>
        [JsonIgnore] // The IPAddress Class Is Not Friendly To Serialization
        public IPAddress DNS_IP { get; set; } = IPAddress.None;
        public string DNS_IP_Str
        {
            get => DNS_IP.ToStringNoScopeId();
            set
            {
                bool isIP = NetworkTool.IsIP(value, out IPAddress? ip);
                if (isIP && ip != null)
                {
                    if (DNS_IP == ip) return;
                    DNS_IP = ip;
                }
            }
        }

        [JsonIgnore]
        public DnsEnums.DnsProtocol Protocol { get; set; } = DnsEnums.DnsProtocol.Unknown;
        public string ProtocolName
        {
            get => DnsEnums.GetDnsProtocolName(Protocol);
            set
            {
                if (ProtocolName == value) return;
                Protocol = DnsEnums.GetDnsProtocolByName(value);
            }
        }

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

        public DnsItem(string dns)
        {
            IDUnique = Info.GetUniqueIdString(true);
            DNS_URL = dns;
            try
            {
                DnsReader dr = new(dns);
                DNS_URL = dr.DnsWithRelay;
                Protocol = dr.Protocol;
                if (dr.IsDnsCryptStamp)
                {
                    IPAddress dns_IP = dr.StampReader.IP;
                    if (!NetworkTool.IsLocalIP(dns_IP)) DNS_IP = dns_IP;
                }
            }
            catch (Exception) { }
        }
    }

    //public class DnsItem : INotifyPropertyChanged
    //{
    //    public event PropertyChangedEventHandler? PropertyChanged;

    //    public string IDUnique { get; set; }

    //    [JsonIgnore]
    //    private bool IsSelectedValue = false;
    //    public bool IsSelected
    //    {
    //        get => IsSelectedValue;
    //        set
    //        {
    //            if (IsSelectedValue == value) return;
    //            IsSelectedValue = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
    //        }
    //    }

    //    [JsonIgnore]
    //    private string DNS_URL_Value = string.Empty;
    //    public string DNS_URL
    //    {
    //        get => DNS_URL_Value;
    //        set
    //        {
    //            if (DNS_URL_Value == value) return;
    //            DNS_URL_Value = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DNS_URL)));
    //        }
    //    }

    //    [JsonIgnore]
    //    public DnsReader DnsReader => new(DNS_URL);

    //    /// <summary>
    //    /// For FragmentDoH
    //    /// </summary>
    //    [JsonIgnore] // The IPAddress Class Is Not Friendly To Serialization
    //    public IPAddress DNS_IP { get; set; } = IPAddress.None;
    //    public string DNS_IP_Str
    //    {
    //        get => DNS_IP.ToStringNoScopeId();
    //        set
    //        {
    //            bool isIP = NetworkTool.IsIP(value, out IPAddress? ip);
    //            if (isIP && ip != null)
    //            {
    //                if (DNS_IP == ip) return;
    //                DNS_IP = ip;
    //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DNS_IP)));
    //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DNS_IP_Str)));
    //            }
    //        }
    //    }

    //    [JsonIgnore]
    //    public DnsEnums.DnsProtocol Protocol { get; set; } = DnsEnums.DnsProtocol.Unknown;
    //    public string ProtocolName
    //    {
    //        get => DnsEnums.GetDnsProtocolName(Protocol);
    //        set
    //        {
    //            if (ProtocolName == value) return;
    //            Protocol = DnsEnums.GetDnsProtocolByName(value);
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Protocol)));
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProtocolName)));
    //        }
    //    }

    //    [JsonIgnore]
    //    private DnsStatus StatusValue = DnsStatus.Unknown;
    //    public DnsStatus Status
    //    {
    //        get => StatusValue;
    //        set
    //        {
    //            if (StatusValue == value) return;
    //            StatusValue = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
    //        }
    //    }

    //    [JsonIgnore]
    //    private int LatencyValue = -1;
    //    public int Latency
    //    {
    //        get => LatencyValue;
    //        set
    //        {
    //            if (LatencyValue == value) return;
    //            LatencyValue = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Latency)));
    //        }
    //    }

    //    [JsonIgnore]
    //    private DnsFilter IsGoogleSafeSearchEnabledValue = DnsFilter.Unknown;
    //    public DnsFilter IsGoogleSafeSearchEnabled
    //    {
    //        get => IsGoogleSafeSearchEnabledValue;
    //        set
    //        {
    //            if (IsGoogleSafeSearchEnabledValue == value) return;
    //            IsGoogleSafeSearchEnabledValue = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGoogleSafeSearchEnabled)));
    //        }
    //    }

    //    [JsonIgnore]
    //    private DnsFilter IsBingSafeSearchEnabledValue = DnsFilter.Unknown;
    //    public DnsFilter IsBingSafeSearchEnabled
    //    {
    //        get => IsBingSafeSearchEnabledValue;
    //        set
    //        {
    //            if (IsBingSafeSearchEnabledValue == value) return;
    //            IsBingSafeSearchEnabledValue = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBingSafeSearchEnabled)));
    //        }
    //    }

    //    [JsonIgnore]
    //    private DnsFilter IsYoutubeRestrictedValue = DnsFilter.Unknown;
    //    public DnsFilter IsYoutubeRestricted
    //    {
    //        get => IsYoutubeRestrictedValue;
    //        set
    //        {
    //            if (IsYoutubeRestrictedValue == value) return;
    //            IsYoutubeRestrictedValue = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsYoutubeRestricted)));
    //        }
    //    }

    //    [JsonIgnore]
    //    private DnsFilter IsAdultBlockedValue = DnsFilter.Unknown;
    //    public DnsFilter IsAdultBlocked
    //    {
    //        get => IsAdultBlockedValue;
    //        set
    //        {
    //            if (IsAdultBlockedValue == value) return;
    //            IsAdultBlockedValue = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAdultBlocked)));
    //        }
    //    }

    //    [JsonIgnore]
    //    private string DescriptionValue = string.Empty;
    //    public string Description
    //    {
    //        get => DescriptionValue;
    //        set
    //        {
    //            if (DescriptionValue == value) return;
    //            DescriptionValue = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
    //        }
    //    }

    //    public DnsItem()
    //    {
    //        IDUnique = Info.GetUniqueIdString(true);
    //    }

    //    public DnsItem(string dns)
    //    {
    //        IDUnique = Info.GetUniqueIdString(true);
    //        DNS_URL = dns;
    //        try
    //        {
    //            DnsReader dr = new(dns);
    //            DNS_URL = dr.DnsWithRelay;
    //            Protocol = dr.Protocol;
    //            if (dr.IsDnsCryptStamp)
    //            {
    //                IPAddress dns_IP = dr.StampReader.IP;
    //                if (!NetworkTool.IsLocalIP(dns_IP)) DNS_IP = dns_IP;
    //            }
    //        }
    //        catch (Exception) { }
    //    }
    //}

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

    public class DnsExportImport
    {
        public DnsSettings? Settings { get; set; }
        public List<DnsGroup> Groups { get; set; } = new();

        public void Dispose()
        {
            try
            {
                Groups.Clear();
            }
            catch (Exception) { }
        }
    }

}