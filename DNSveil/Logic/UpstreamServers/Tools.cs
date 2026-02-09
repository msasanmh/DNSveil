using System.Collections.ObjectModel;
using static DNSveil.Logic.UpstreamServers.UpstreamModel;

namespace DNSveil.Logic.UpstreamServers;

public static class Tools
{
    public static void NotifyPropertyChanged(this ObservableCollection<UpstreamItem> items)
    {
        foreach (UpstreamItem item in items) item.NotifyPropertyChanged();
    }

    public static GroupSettings Clone_GroupSettings(this GroupSettings obj)
    {
        return new GroupSettings()
        {
            IsEnabled = obj.IsEnabled,
            TestURL = obj.TestURL,
            ScanTimeoutSec = obj.ScanTimeoutSec,
            ScanParallelSize = obj.ScanParallelSize,
            BootstrapIP = obj.BootstrapIP,
            BootstrapPort = obj.BootstrapPort,
            Fragment = obj.Fragment,
            GetRegionInfo = obj.GetRegionInfo,
            TestSpeed = obj.TestSpeed,
            DontUseServersWithNoSecurity = obj.DontUseServersWithNoSecurity,
            AllowInsecure = obj.AllowInsecure
        };
    }

}