using CustomControls;
using MsmhToolsClass;

namespace SecureDNSClient;

public partial class FormMain
{
    public class DnsInfo
    {
        public string DNS { get; set; } = string.Empty;
        public long Latency { get; set; } = -1;
        public CheckMode CheckMode { get; set; }
        private string pGroupName = string.Empty;
        public string GroupName
        {
            get
            {
                if (!string.IsNullOrEmpty(pGroupName)) return pGroupName;
                return GetCheckModeNameByCheckMode(CheckMode);
            }

            set
            {
                if (pGroupName != value) pGroupName = value;
            }
        }
    }

    public enum CheckMode
    {
        BuiltIn, SavedServers, CustomServers, MixedServers, Unknown
    }

    public static string GetCheckModeNameByCheckMode(CheckMode checkMode)
    {
        return checkMode switch
        {
            CheckMode.BuiltIn => "Built-In",
            CheckMode.SavedServers => "Saved Servers",
            CheckMode.CustomServers => "Custom Servers",
            CheckMode.MixedServers => "Mixed Servers",
            CheckMode.Unknown => "Unknown",
            _ => "Unknown"
        };
    }

    public CheckMode GetCheckMode()
    {
        CheckMode checkMode;
        if (CustomRadioButtonBuiltIn.Checked) checkMode = CheckMode.BuiltIn;
        else checkMode = CheckMode.CustomServers;
        return checkMode;
    }

    public class CheckRequest
    {
        public CheckMode CheckMode { get; set; } = CheckMode.Unknown;
        public bool ClearWorkingServers { get; set; } = false;
        public bool HasUserGroupName { get; set; } = false;
        private string pGroupName = string.Empty;
        public string GroupName
        {
            get
            {
                if (!string.IsNullOrEmpty(pGroupName)) return pGroupName;
                return GetCheckModeNameByCheckMode(CheckMode);
            }

            set
            {
                if (pGroupName != value) pGroupName = value;
            }
        }
    }

    public enum ConnectMode
    {
        ConnectToWorkingServers,
        ConnectToFakeProxyDohViaProxyDPI,
        ConnectToFakeProxyDohViaGoodbyeDPI,
        ConnectToPopularServersWithProxy,
        Unknown
    }

    public struct ConnectModeName
    {
        public const string WorkingServers = "Working Servers";
        public const string FakeProxyViaProxyDPI = "Fake Proxy Via Proxy DPI Bypass";
        public const string FakeProxyViaGoodbyeDPI = "Fake Proxy Via GoodbyeDPI";
        public const string PopularServersWithProxy = "Popular Servers With Proxy";
        public const string Unknown = "Unknown";
    }

    public static ConnectMode GetConnectModeByName(string? name)
    {
        return name switch
        {
            ConnectModeName.WorkingServers => ConnectMode.ConnectToWorkingServers,
            ConnectModeName.FakeProxyViaProxyDPI => ConnectMode.ConnectToFakeProxyDohViaProxyDPI,
            ConnectModeName.FakeProxyViaGoodbyeDPI => ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI,
            ConnectModeName.PopularServersWithProxy => ConnectMode.ConnectToPopularServersWithProxy,
            ConnectModeName.Unknown => ConnectMode.Unknown,
            _ => ConnectMode.Unknown
        };
    }

    public static string GetConnectModeNameByConnectMode(ConnectMode mode)
    {
        return mode switch
        {
            ConnectMode.ConnectToWorkingServers => ConnectModeName.WorkingServers,
            ConnectMode.ConnectToFakeProxyDohViaProxyDPI => ConnectModeName.FakeProxyViaProxyDPI,
            ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI => ConnectModeName.FakeProxyViaGoodbyeDPI,
            ConnectMode.ConnectToPopularServersWithProxy => ConnectModeName.PopularServersWithProxy,
            ConnectMode.Unknown => ConnectModeName.Unknown,
            _ => ConnectModeName.Unknown
        };
    }

    public static void UpdateConnectModes(CustomComboBox ccb)
    {
        ccb.InvokeIt(() =>
        {
            ccb.Text = "Select a Connect Mode";
            object item = ccb.SelectedItem;
            ccb.Items.Clear();
            List<string> connectModeNames = new()
            {
                ConnectModeName.WorkingServers,
                ConnectModeName.FakeProxyViaProxyDPI,
                ConnectModeName.FakeProxyViaGoodbyeDPI,
                ConnectModeName.PopularServersWithProxy
            };
            for (int n = 0; n < connectModeNames.Count; n++)
            {
                string connectModeName = connectModeNames[n];
                ccb.Items.Add(connectModeName);
            }
            if (ccb.Items.Count > 0)
            {
                bool exist = false;
                for (int i = 0; i < ccb.Items.Count; i++)
                {
                    object selectedItem = ccb.Items[i];
                    if (item != null && item.Equals(selectedItem))
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist)
                    ccb.SelectedItem = item;
                else
                    ccb.SelectedIndex = 0;
                ccb.DropDownHeight = 500;
            }
            else ccb.SelectedIndex = -1;
        });
    }

    public ConnectMode GetConnectMode()
    {
        // Get Connect modes
        bool a = CustomRadioButtonConnectCheckedServers.Checked;
        bool b = CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Checked;
        bool c = CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Checked;
        bool d = CustomRadioButtonConnectDNSCrypt.Checked;

        ConnectMode connectMode = ConnectMode.ConnectToWorkingServers;
        if (a) connectMode = ConnectMode.ConnectToWorkingServers;
        else if (b) connectMode = ConnectMode.ConnectToFakeProxyDohViaProxyDPI;
        else if (c) connectMode = ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI;
        else if (d) connectMode = ConnectMode.ConnectToPopularServersWithProxy;
        return connectMode;
    }

    public class QuickConnectRequest
    {
        public CheckRequest CheckRequest { get; set; } = new();
        public bool CanUseSavedServers { get; set; } = false;
        public ConnectMode ConnectMode { get; set; } = ConnectMode.Unknown;
    }

    public ConnectMode GetConnectModeForQuickConnect()
    {
        ConnectMode connectMode = ConnectMode.ConnectToWorkingServers;
        if (CustomComboBoxSettingQcConnectMode.SelectedItem != null)
            connectMode = GetConnectModeByName(CustomComboBoxSettingQcConnectMode.SelectedItem.ToString());
        return connectMode;
    }

}