using MsmhToolsClass.DnsTool;

namespace MsmhToolsClass.ProxyServerPrograms;

public partial class ProxyProgram
{
    public class Dns
    {
        public enum Mode
        {
            System,
            DoH,
            PlainDNS,
            Disable
        }

        public Mode DNSMode { get; private set; } = Mode.Disable;
        public string? DNS { get; private set; }
        public string? DnsCleanIp { get; private set; }
        public int TimeoutSec { get; private set; } = 10;
        public bool ChangeCloudflareIP { get; set; } = false;

        /// <summary>
        /// Only for DoH Mode
        /// </summary>
        public string? ProxyScheme { get; private set; }
        public string? Host { get; protected set; }
        public string? CloudflareCleanIP { get; private set; }
        public List<string> CloudflareIPs { get; private set; } = new();

        public Dns() { }

        public void Set(Mode mode, string? dns, string? dnsCleanIp, int timeoutSec, string? proxyScheme = null)
        {
            // Set
            DNSMode = mode;
            DNS = dns;
            DnsCleanIp = dnsCleanIp ?? string.Empty;
            TimeoutSec = timeoutSec;
            ProxyScheme = proxyScheme;

            if (DNSMode == Mode.Disable) return;
            if (string.IsNullOrEmpty(dns)) return;

            // Get Host
            string host = dns;
            if (DNSMode == Mode.DoH)
            {
                if (host.StartsWith("https://")) host = host[8..];
                if (host.Contains('/'))
                {
                    string[] split = host.Split('/');
                    host = split[0];
                }
            }
            else if (DNSMode == Mode.PlainDNS)
            {
                if (host.Contains(':'))
                {
                    string[] split = host.Split(':');
                    host = split[0];
                }
            }
            Host = host;
        }

        /// <summary>
        /// Redirect all Cloudflare IPs to a clean IP
        /// </summary>
        /// <param name="cfCleanIP">CF Clean IP</param>
        /// <param name="cfIpRange">e.g. 103.21.244.0 - 103.21.244.255\n198.41.128.0 - 198.41.143.255</param>
        public void SetCloudflareIPs(string cfCleanIP, string? cfIpRange = null)
        {
            if (!string.IsNullOrEmpty(cfIpRange))
                cfIpRange += Environment.NewLine;
            ChangeCloudflareIP = true;
            CloudflareCleanIP = cfCleanIP;

            // Built-in CF IPs
            string defaultCfIPs = "103.21.244.0 - 103.21.244.255\n";
            defaultCfIPs += "103.22.200.0 - 103.22.200.255\n";
            defaultCfIPs += "103.31.4.0 - 103.31.5.255\n";
            defaultCfIPs += "104.16.0.0 - 104.31.255.255\n";
            defaultCfIPs += "108.162.192.0 - 108.162.207.255\n";
            defaultCfIPs += "131.0.72.0 - 131.0.75.255\n";
            defaultCfIPs += "141.101.64.0 - 141.101.65.255\n";
            defaultCfIPs += "162.158.0.0 - 162.158.3.255\n";
            defaultCfIPs += "172.64.0.0 - 172.67.255.255\n";
            defaultCfIPs += "173.245.48.0 - 173.245.48.255\n";
            defaultCfIPs += "188.114.96.0 - 188.114.99.255\n";
            defaultCfIPs += "190.93.240.0 - 190.93.243.255\n";
            defaultCfIPs += "197.234.240.0 - 197.234.243.255\n";
            defaultCfIPs += "198.41.128.0 - 198.41.143.255";

            if (string.IsNullOrEmpty(cfIpRange) || string.IsNullOrWhiteSpace(cfIpRange))
                CloudflareIPs = defaultCfIPs.SplitToLines();
            else
                CloudflareIPs = cfIpRange.SplitToLines();
        }

        private bool IsCfIP(string ipString)
        {
            try
            {
                string[] ips = ipString.Split('.');
                int ip1 = int.Parse(ips[0]);
                int ip2 = int.Parse(ips[1]);
                int ip3 = int.Parse(ips[2]);
                int ip4 = int.Parse(ips[3]);

                for (int n = 0; n < CloudflareIPs.Count; n++)
                {
                    string ipRange = CloudflareIPs[n].Trim();

                    if (!string.IsNullOrEmpty(ipRange))
                    {
                        string[] split = ipRange.Split('-');
                        string ipMin = split[0].Trim();
                        string ipMax = split[1].Trim();

                        string[] ipMins = ipMin.Split('.');
                        int ipMin1 = int.Parse(ipMins[0]);
                        int ipMin2 = int.Parse(ipMins[1]);
                        int ipMin3 = int.Parse(ipMins[2]);
                        int ipMin4 = int.Parse(ipMins[3]);

                        string[] ipMaxs = ipMax.Split('.');
                        int ipMax1 = int.Parse(ipMaxs[0]);
                        int ipMax2 = int.Parse(ipMaxs[1]);
                        int ipMax3 = int.Parse(ipMaxs[2]);
                        int ipMax4 = int.Parse(ipMaxs[3]);

                        if (ip1 >= ipMin1 && ip1 <= ipMax1)
                            if (ip2 >= ipMin2 && ip2 <= ipMax2)
                                if (ip3 >= ipMin3 && ip3 <= ipMax3)
                                    if (ip4 >= ipMin4 && ip4 <= ipMax4)
                                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> Get(string destHostname)
        {
            if (string.IsNullOrEmpty(destHostname)) return string.Empty;

            // Don't resolve current Dns to avoid loop
            if (destHostname.Equals(Host)) return destHostname;
            if (destHostname.Equals(DnsCleanIp)) return destHostname;

            // Get
            if (DNSMode == Mode.System)
            {
                string ipString = GetIP.GetIpFromSystem(destHostname, false); // Try Ipv4
                if (string.IsNullOrEmpty(ipString))
                    ipString = GetIP.GetIpFromSystem(destHostname, true); // Try Ipv6
                if (!ChangeCloudflareIP)
                    return string.IsNullOrEmpty(ipString) ? destHostname : ipString;
                else
                {
                    if (string.IsNullOrEmpty(ipString)) return destHostname;
                    else
                    {
                        return IsCfIP(ipString) ? CloudflareCleanIP ?? ipString : ipString;
                    }
                }
            }
            else if (DNSMode == Mode.DoH)
            {
                if (string.IsNullOrEmpty(DNS)) return string.Empty;

                string ipString = await GetIP.GetIpFromDohUsingWireFormat(destHostname, DNS, TimeoutSec, ProxyScheme);
                if (!ChangeCloudflareIP)
                    return string.IsNullOrEmpty(ipString) ? destHostname : ipString;
                else
                {
                    if (string.IsNullOrEmpty(ipString)) return destHostname;
                    else
                    {
                        return IsCfIP(ipString) ? CloudflareCleanIP ?? ipString : ipString;
                    }
                }
            }
            else if (DNSMode == Mode.PlainDNS)
            {
                if (string.IsNullOrEmpty(DNS)) return string.Empty;

                string plainDnsIP = DNS;
                int plainDnsPort = 53;

                if (DNS.Contains(':'))
                {
                    string[] dnsIpPort = DNS.Split(':');
                    plainDnsIP = dnsIpPort[0];
                    plainDnsPort = Convert.ToInt32(dnsIpPort[1]);
                }

                string ipString = await GetIP.GetIpFromPlainDNS(destHostname, plainDnsIP, plainDnsPort, TimeoutSec);
                if (!ChangeCloudflareIP)
                    return string.IsNullOrEmpty(ipString) ? destHostname : ipString;
                else
                {
                    if (string.IsNullOrEmpty(ipString)) return destHostname;
                    else
                    {
                        return IsCfIP(ipString) ? CloudflareCleanIP ?? ipString : ipString;
                    }
                }
            }
            else if (DNSMode == Mode.Disable) return destHostname;
            else return destHostname;
        }
    }
}