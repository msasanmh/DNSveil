using MsmhToolsClass.DnsTool;
using System.Diagnostics;

namespace MsmhToolsClass.MsmhToolsClass.DnsTool;

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
            string ip1 = string.Empty;
            DnsReader dr1 = new(dns1, null);
            NetworkTool.GetUrlDetails(dns1, dr1.Port, out string scheme1, out string host1, out _, out int port1, out string path1, out _);
            if (dr1.IsDnsCryptStamp)
            {
                host1 = dr1.Host;
                ip1 = dr1.IP ?? string.Empty;
                port1 = dr1.Port;
                path1 = dr1.Path;
            }

            string ip2 = string.Empty;
            DnsReader dr2 = new(dns2, null);
            NetworkTool.GetUrlDetails(dns2, dr2.Port, out string scheme2, out string host2, out _, out int port2, out string path2, out _);
            if (dr2.IsDnsCryptStamp)
            {
                if (!dr1.IsDnsCryptStamp)
                    if (dr1.Protocol == dr2.Protocol) scheme2 = scheme1;

                host2 = dr2.Host;
                ip2 = dr2.IP ?? string.Empty;
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
                if (string.IsNullOrEmpty(host1)) host1 = dr1.IP ?? string.Empty;
                result = scheme1.Equals(scheme2) && host1.Equals(host2) && port1.Equals(port2) && path1.Equals(path2);
            }

            if (!dr1.IsDnsCryptStamp && dr2.IsDnsCryptStamp)
            {
                if (string.IsNullOrEmpty(host2)) host2 = dr2.IP ?? string.Empty;
                result = scheme1.Equals(scheme2) && host1.Equals(host2) && port1.Equals(port2) && path1.Equals(path2);
            }

            return result;
        });
    }

}