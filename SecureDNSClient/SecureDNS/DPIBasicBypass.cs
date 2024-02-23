using CustomControls;
using MsmhToolsClass;

namespace SecureDNSClient.DPIBasic;

public enum DPIBasicBypassMode
{
    Mode1 = 1,
    Mode2 = 2,
    Mode3 = 3,
    Mode4 = 4,
    Mode5 = 5,
    Mode6 = 6,
    Light = 11,
    Medium = 12,
    High = 13,
    Extreme = 14,
}

public class DPIBasicBypass
{
    public static List<DPIBasicBypassMode> GetAllModes()
    {
        List<DPIBasicBypassMode> list = new()
        {
            DPIBasicBypassMode.Light,
            DPIBasicBypassMode.Medium,
            DPIBasicBypassMode.High,
            DPIBasicBypassMode.Extreme,
            DPIBasicBypassMode.Mode1,
            DPIBasicBypassMode.Mode2,
            DPIBasicBypassMode.Mode3,
            DPIBasicBypassMode.Mode4,
            DPIBasicBypassMode.Mode5,
            DPIBasicBypassMode.Mode6
        };
        return list;
    }

    public static DPIBasicBypassMode GetGoodbyeDpiModeBasicByName(string? modeName)
    {
        return modeName switch
        {
            nameof(DPIBasicBypassMode.Light) => DPIBasicBypassMode.Light,
            nameof(DPIBasicBypassMode.Medium) => DPIBasicBypassMode.Medium,
            nameof(DPIBasicBypassMode.High) => DPIBasicBypassMode.High,
            nameof(DPIBasicBypassMode.Extreme) => DPIBasicBypassMode.Extreme,
            nameof(DPIBasicBypassMode.Mode1) => DPIBasicBypassMode.Mode1,
            nameof(DPIBasicBypassMode.Mode2) => DPIBasicBypassMode.Mode2,
            nameof(DPIBasicBypassMode.Mode3) => DPIBasicBypassMode.Mode3,
            nameof(DPIBasicBypassMode.Mode4) => DPIBasicBypassMode.Mode4,
            nameof(DPIBasicBypassMode.Mode5) => DPIBasicBypassMode.Mode5,
            nameof(DPIBasicBypassMode.Mode6) => DPIBasicBypassMode.Mode6,
            _ => DPIBasicBypassMode.Light,
        };
    }

    public static void UpdateGoodbyeDpiBasicModes(CustomComboBox ccb)
    {
        ccb.InvokeIt(() =>
        {
            ccb.Text = "Select a Mode";
            object item = ccb.SelectedItem;
            ccb.Items.Clear();
            List<DPIBasicBypassMode> modeNames = GetAllModes();
            for (int n = 0; n < modeNames.Count; n++)
            {
                DPIBasicBypassMode modeName = modeNames[n];
                ccb.Items.Add(modeName.ToString());
            }
            if (ccb.Items.Count > 0)
            {
                bool exist = false;
                for (int i = 0; i < ccb.Items.Count; i++)
                {
                    object selectedItem = ccb.Items[i];
                    if (item != null && item.Equals(selectedItem))
                    {
                        exist = true; break;
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

    public string Args { get; }
    public string Text { get; }
    private readonly string AutoTTL = "1-4-10";
    private readonly int MinTTL = 3;

    public DPIBasicBypass(DPIBasicBypassMode? dpiBasicBypassMode, decimal sslFragment, string fallbackDNS, int fallbackDNSPort)
    {
        dpiBasicBypassMode ??= DPIBasicBypassMode.Light;

        string fallbackDnsIPv6 = SecureDNS.BootstrapDnsIPv6.ToString();
        int fallbackDnsIPv6Port = SecureDNS.BootstrapDnsPort;
        string Mode1Args = $"-1 --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";
        string Mode2Args = $"-2 --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";
        string Mode3Args = $"-3 --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";
        string Mode4Args = $"-4 --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";
        string Mode5Args = $"-5 --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";
        string Mode6Args = $"-6 --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";
        string LightArgs = $"-p -r -s -m -e {sslFragment} -w --native-frag --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";
        string MediumArgs = $"-p -r -s -m -e {sslFragment} -w --auto-ttl {AutoTTL} --min-ttl {MinTTL} --native-frag --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";
        string HighArgs = $"-p -r -s -m -e {sslFragment} -w --auto-ttl {AutoTTL} --min-ttl {MinTTL} --native-frag --wrong-seq --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";
        string ExtremeArgs = $"-p -r -s -m -f 2 -e {sslFragment} -w --auto-ttl {AutoTTL} --min-ttl {MinTTL} --native-frag --wrong-chksum --wrong-seq --max-payload --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort} --dnsv6-addr {fallbackDnsIPv6} --dnsv6-port {fallbackDnsIPv6Port}";

        switch (dpiBasicBypassMode)
        {
            case DPIBasicBypassMode.Mode1: Text = "Mode 1"; Args = Mode1Args; break;
            case DPIBasicBypassMode.Mode2: Text = "Mode 2"; Args = Mode2Args; break;
            case DPIBasicBypassMode.Mode3: Text = "Mode 3"; Args = Mode3Args; break;
            case DPIBasicBypassMode.Mode4: Text = "Mode 4"; Args = Mode4Args; break;
            case DPIBasicBypassMode.Mode5: Text = "Mode 5"; Args = Mode5Args; break;
            case DPIBasicBypassMode.Mode6: Text = "Mode 6"; Args = Mode6Args; break;
            case DPIBasicBypassMode.Light: Text = "Light"; Args = LightArgs; break;
            case DPIBasicBypassMode.Medium: Text = "Medium"; Args = MediumArgs; break;
            case DPIBasicBypassMode.High: Text = "High"; Args = HighArgs; break;
            case DPIBasicBypassMode.Extreme: Text = "Extreme"; Args = ExtremeArgs; break;
            default: Text = "Light"; Args = LightArgs; break;
        }
    }
}