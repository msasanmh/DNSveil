using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DnsDeDup
{
    private List<string> DnsList { get; set; } = new();
    private List<string> DedupDnsList { get; set; } = new();
    private CancellationTokenSource CTS = new();

    public string Status { get; set; } = string.Empty;

    public DnsDeDup(List<string> dnsList)
    {
        DnsList = dnsList;
    }

    public void Cancel()
    {
        CTS.Cancel();
    }

    public async Task<List<string>> GetDedupList()
    {
        CTS = new();
        await Task.Run(async () =>
        {
            for (int n = 0; n < DnsList.Count; n++)
            {
                if (CTS.IsCancellationRequested) break;

                Status = $"Working on {n + 1} of {DnsList.Count}";

                string dns1 = DnsList[n].Trim();
                bool isSame = false;
                for (int n2 = 0; n2 < DedupDnsList.Count; n2++)
                {
                    if (CTS.IsCancellationRequested) break;

                    string dns2 = DedupDnsList[n2].Trim();
                    if (await IsTheSame(dns1, dns2))
                    {
                        Debug.WriteLine(dns1);
                        Debug.WriteLine(dns2);
                        Debug.WriteLine("--------------------");
                        isSame = true;
                        break;
                    }
                }

                if (!isSame) DedupDnsList.Add(dns1);
            }
        });

        return DedupDnsList;
    }

    private async Task<bool> IsTheSame(string dns1, string dns2)
    {
        return await Task.Run(() =>
        {
            IPAddress ip1 = IPAddress.None;
            DnsReader dr1 = new(dns1, null);
            NetworkTool.URL urid1 = NetworkTool.GetUrlOrDomainDetails(dns1, dr1.Port);
            string scheme1 = urid1.Scheme;
            string host1 = urid1.Host;
            int port1 = urid1.Port;
            string path1 = urid1.Path;

            if (dr1.IsDnsCryptStamp)
            {
                host1 = dr1.Host;
                ip1 = dr1.IP;
                port1 = dr1.Port;
                path1 = dr1.Path;
            }

            IPAddress ip2 = IPAddress.None;
            DnsReader dr2 = new(dns2, null);
            NetworkTool.URL urid2 = NetworkTool.GetUrlOrDomainDetails(dns2, dr2.Port);
            string scheme2 = urid2.Scheme;
            string host2 = urid2.Host;
            int port2 = urid2.Port;
            string path2 = urid2.Path;

            if (dr2.IsDnsCryptStamp)
            {
                if (!dr1.IsDnsCryptStamp)
                    if (dr1.Protocol == dr2.Protocol) scheme2 = scheme1;

                host2 = dr2.Host;
                ip2 = dr2.IP;
                port2 = dr2.Port;
                path2 = dr2.Path;
            }
            else
            {
                if (dr1.IsDnsCryptStamp)
                    if (dr1.Protocol == dr2.Protocol) scheme1 = scheme2;
            }

            scheme1 = scheme1.Trim(); host1 = host1.Trim(); path1 = path1.Trim();
            scheme2 = scheme2.Trim(); host2 = host2.Trim(); path2 = path2.Trim();

            bool result = false;
            if (!dr1.IsDnsCryptStamp && !dr2.IsDnsCryptStamp)
                result = scheme1.Equals(scheme2) && host1.Equals(host2) && port1.Equals(port2) && path1.Equals(path2);

            if (dr1.IsDnsCryptStamp && dr2.IsDnsCryptStamp)
                result = scheme1.Equals(scheme2) && host1.Equals(host2) && ip1.Equals(ip2) && port1.Equals(port2) && path1.Equals(path2);

            if (dr1.IsDnsCryptStamp && !dr2.IsDnsCryptStamp)
            {
                if (string.IsNullOrEmpty(host1)) host1 = dr1.IP.ToString();
                result = scheme1.Equals(scheme2) && host1.Equals(host2) && port1.Equals(port2) && path1.Equals(path2);
            }

            if (!dr1.IsDnsCryptStamp && dr2.IsDnsCryptStamp)
            {
                if (string.IsNullOrEmpty(host2)) host2 = dr2.IP.ToString();
                result = scheme1.Equals(scheme2) && host1.Equals(host2) && port1.Equals(port2) && path1.Equals(path2);
            }

            return result;
        });
    }

}