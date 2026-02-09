using System.Net;
using System.Text.Json.Serialization;
using MsmhToolsClass.V2RayConfigTool;
using MsmhToolsClass;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace DNSveil.Logic.UpstreamServers;

[Serializable, DebuggerDisplay("Count = {Count}")]
public class UpstreamModel
{
    [JsonPropertyName("Settings")]
    public UpstreamSettings Settings { get; set; } = new();

    [JsonPropertyName("Groups")]
    public ObservableCollection<UpstreamGroup> Groups { get; set; } = new();

    public class UpstreamSettings // Placeholder
    {
        //
    }

    public class UpstreamGroup : INotifyPropertyChanged
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

        public GroupSettings Settings { get; set; } = new();
        public Subscription? Subscription { get; set; }
        public Custom? Custom { get; set; }
        public ObservableCollection<UpstreamItem> Items { get; set; } = new();

        public UpstreamGroup() { }

        public UpstreamGroup(string name, GroupMode mode)
        {
            Name = name;
            Mode = mode;
            if (Mode == GroupMode.Subscription) Subscription = new();
            else if (Mode == GroupMode.Custom) Custom = new();
        }
    }

    public enum GroupMode
    {
        None = 0,
        Subscription = 1,
        Custom = 2
    }

    public class GroupSettings
    {
        public bool IsEnabled { get; set; } = true;
        public string TestURL { get; set; } = "https://captive.apple.com/hotspot-detect.html";
        public double ScanTimeoutSec { get; set; } = 10;
        public int ScanParallelSize { get; set; } = 5;

        [JsonIgnore] // The IPAddress Class Is Not Friendly To Serialization
        public IPAddress BootstrapIP { get; set; } = IPAddress.Parse("8.8.8.8");
        public string BootstrapIpStr
        {
            get => BootstrapIP.ToStringNoScopeId();
            set
            {
                if (BootstrapIpStr == value) return;
                bool isIP = NetworkTool.IsIP(value, out IPAddress? ip);
                if (isIP && ip != null) BootstrapIP = ip;
            }
        }

        public int BootstrapPort { get; set; } = 53;
        public XrayFragment Fragment { get; set; } = new();
        public bool GetRegionInfo { get; set; } = true;
        public bool TestSpeed { get; set; } = false;
        public bool DontUseServersWithNoSecurity { get; set; } = false;
        public bool AllowInsecure { get; set; } = false;

        public GroupSettings() { }

        public GroupSettings(bool isEnabled, string testURL, double scanTimeoutSec, int scanParallelSize, string bootstrapIpStr, int bootstrapPort, XrayFragment fragment, bool getRegionInfo, bool getSpeedTest, bool dontUseServersWithNoSecurity, bool allowInsecure)
        {
            IsEnabled = isEnabled;
            TestURL = testURL;
            ScanTimeoutSec = scanTimeoutSec;
            ScanParallelSize = scanParallelSize;
            BootstrapIpStr = bootstrapIpStr;
            BootstrapPort = bootstrapPort;
            Fragment = fragment;
            GetRegionInfo = getRegionInfo;
            TestSpeed = getSpeedTest;
            DontUseServersWithNoSecurity = dontUseServersWithNoSecurity;
            AllowInsecure = allowInsecure;
        }
    }

    public class XrayFragment
    {
        public bool IsEnabled { get; set; } = true;
        public string Size { get; set; } = "2-4";
        public string Delay { get; set; } = "3-5";
    }

    public class Subscription
    {
        public Source Source { get; set; } = new();
        public SubscriptionOptions Options { get; set; } = new();
    }

    public class Custom
    {
        public Source Source { get; set; } = new();
        public CustomOptions Options { get; set; } = new();
    }

    public class Source
    {
        public bool IsEnabled { get; set; } = true;
        public List<string> URLs { get; set; } = new();
    }

    public class SubscriptionOptions
    {
        public SubscriptionAutoUpdate AutoUpdate { get; set; } = new();
    }

    public class SubscriptionAutoUpdate
    {
        public int UpdateSource { get; set; } = 24;
        public DateTime LastUpdateSource { get; set; } = DateTime.MinValue;
        public bool AutoScanSelect { get; set; } = true;
        public int ScanServers { get; set; } = 6;
        public DateTime LastScanServers { get; set; } = DateTime.MinValue;
    }

    public class CustomOptions
    {
        public CustomAutoUpdate AutoUpdate { get; set; } = new();
    }

    public class CustomAutoUpdate
    {
        public bool AutoScanSelect { get; set; } = true;
        public int ScanServers { get; set; } = 6;
        public DateTime LastScanServers { get; set; } = DateTime.MinValue;
    }

    public class UpstreamItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemarksStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProtocolStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecurityStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TransportStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusCode)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusCodeNumber)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Latency)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StabilityPercent)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StabilityStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusShortDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Country)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DLSpeed)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DLSpeedStr)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ULSpeed)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ULSpeedStr)));
        }

        public ConfigBuilder.GetConfigInfo ConfigInfo { get; set; } = new();
        public string IDUniqueWithRemarks { get; set; } = "0123456789";
        public bool IsSelected { get; set; } = false;

        [JsonIgnore]
        public string RemarksStr
        {
            get
            {
                string remarks = string.Empty;
                string cir = ConfigInfo.Remarks.Trim();
                if (!string.IsNullOrEmpty(cir)) remarks += $"{cir}      ";
                if (!string.IsNullOrEmpty(ConfigInfo.Address)) remarks += $"{ConfigInfo.Address}:{ConfigInfo.Port}";
                return remarks;
            }
        }

        [JsonIgnore]
        public string ProtocolStr => ConfigInfo.Protocol.ToString();

        [JsonIgnore]
        public string SecurityStr => ConfigInfo.Security.ToString();

        [JsonIgnore]
        public string TransportStr => ConfigInfo.Transport.ToString();

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.RequestTimeout; // 408
        public int StatusCodeNumber { get; set; } = 408;
        public int Latency { get; set; } = -1;
        public int StabilityPercent { get; set; } = 0;
        public string StabilityStr => $"{StabilityPercent}%";
        public string StatusShortDescription { get; set; } = "Unknown";
        public string StatusDescription { get; set; } = string.Empty;
        public CultureTool.RegionResult Region { get; set; } = new(CultureTool.GetDefaultRegion);
        public string Country => $"{Region.TwoLetterISORegionName} {Region.EnglishName}";

        public double DLSpeed { get; set; } = 0;
        [JsonIgnore]
        public string DLSpeedStr => $"{ConvertTool.ConvertByteToHumanRead(DLSpeed)}/Sec";

        public double ULSpeed { get; set; } = 0;
        [JsonIgnore]
        public string ULSpeedStr => $"{ConvertTool.ConvertByteToHumanRead(ULSpeed)}/Sec";

        public string Description { get; set; } = string.Empty;

        public UpstreamItem() { }

        public UpstreamItem(string urlOrJson)
        {
            ConfigInfo = new(urlOrJson);
            bool isSuccess = EncodingTool.TryGetSHA512($"{ConfigInfo.IDUnique}_{ConfigInfo.Remarks}", out string value);
            if (isSuccess) IDUniqueWithRemarks = value;
        }
    }

    public struct UpstreamItemsInfo
    {
        public int TotalServers { get; set; } = 0;
        public int OnlineServers { get; set; } = 0;
        public int SelectedServers { get; set; } = 0;
        public int AverageLatency { get; set; } = -1;

        public UpstreamItemsInfo() { }

        public UpstreamItemsInfo(int totalServers, int onlineServers, int selectedServers, int averageLatency)
        {
            TotalServers = totalServers;
            OnlineServers = onlineServers;
            SelectedServers = selectedServers;
            AverageLatency = averageLatency;
        }
    }

    public class UpstreamExportImport
    {
        public UpstreamSettings? Settings { get; set; }
        public List<UpstreamGroup> Groups { get; set; } = new();

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