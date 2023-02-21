using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SecureDNSClient.DPIBasic
{
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
        public string Args { get; }
        public string Text { get; }

        public DPIBasicBypass(DPIBasicBypassMode dpiBasicBypassMode, decimal sslFragment, string fallbackDNS)
        {
            string fallbackDNSIPv6 = SecureDNS.BootstrapDNSIPv6.ToString();
            string Mode1Args = "-1 --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            string Mode2Args = "-2 --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            string Mode3Args = "-3 --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            string Mode4Args = "-4 --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            string Mode5Args = "-5 --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            string Mode6Args = "-6 --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            string LightArgs = "-p -r -s -m -e " + sslFragment.ToString() + " -w --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            string MediumArgs = "-p -r -s -m -f 2 -e " + sslFragment.ToString() + " -w --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            string HighArgs = "-p -r -s -m -f 2 -e " + sslFragment.ToString() + " -w --wrong-chksum --wrong-seq --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            string ExtremeArgs = "-p -r -s -m -f 2 -e " + sslFragment.ToString() + " -w --allow-no-sni --auto-ttl --wrong-chksum --wrong-seq --max-payload --dns-addr " + fallbackDNS + " --dns-port 53 --dnsv6-addr " + fallbackDNSIPv6 + " --dnsv6-port 53";
            
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
}
