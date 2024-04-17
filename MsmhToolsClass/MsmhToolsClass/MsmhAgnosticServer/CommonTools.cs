using System.Net;
using System.Text;

namespace MsmhToolsClass.MsmhAgnosticServer;

public static class CommonTools
{
    //public static unsafe void SetReUsePort(this Socket socket)
    //{
    //    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
    //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    //    {
    //        // set SO_REUSEADDR (https://github.com/dotnet/corefx/issues/32027)
    //        var value = 1;
    //        setsockopt(socket.Handle.ToInt32(), 1, 2, &value, sizeof(int));
    //    }
    //    else
    //        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseUnicastPort, 1);

    //}

    //[DllImport("libc", SetLastError = true)]
    //private static extern unsafe int setsockopt(int socket, int level, int option_name, void* option_value, uint option_len);

    public static bool TryConvertToEnum<T>(bool[] bits, out T result) where T : struct, IConvertible
    {
        try
        {
            int len = bits.Length;
            StringBuilder sb = new(len);

            for (int n = 0; n < len; n++) sb.Append(bits[n] ? "1" : "0");

            result = (T)Enum.Parse(typeof(T), sb.ToString());
            return true;
        }
        catch (Exception)
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Get root domain from a given hostname
    /// </summary>
    /// <param name="hostname"></param>
    /// <returns></returns>
    public static string GetWildCardDomainName(string hostname)
    {
        // Only for subdomains we need wild card
        // Example www.google.com or gstatic.google.com
        // But NOT for google.com or IP address

        if (NetworkTool.IsIp(hostname, out _)) return hostname;

        NetworkTool.GetHostDetails(hostname, 443, out _, out _, out string baseHost, out _, out _, out _);

        if (baseHost.Equals(hostname)) return hostname;

        return $"*.{baseHost}";
    }

    public static bool IsCfIP(string ipString)
    {
        try
        {
            List<string> cloudflareIPs = new()
            {
                "103.21.244.0 - 103.21.244.255",
                "103.22.200.0 - 103.22.200.255",
                "103.31.4.0 - 103.31.5.255",
                "104.16.0.0 - 104.31.255.255",
                "108.162.192.0 - 108.162.207.255",
                "131.0.72.0 - 131.0.75.255",
                "141.101.64.0 - 141.101.65.255",
                "162.158.0.0 - 162.158.3.255",
                "172.64.0.0 - 172.67.255.255",
                "173.245.48.0 - 173.245.48.255",
                "188.114.96.0 - 188.114.99.255",
                "190.93.240.0 - 190.93.243.255",
                "197.234.240.0 - 197.234.243.255",
                "198.41.128.0 - 198.41.143.255"
            };

            string[] ips = ipString.Split('.');
            int ip1 = int.Parse(ips[0]);
            int ip2 = int.Parse(ips[1]);
            int ip3 = int.Parse(ips[2]);
            int ip4 = int.Parse(ips[3]);

            for (int n = 0; n < cloudflareIPs.Count; n++)
            {
                string ipRange = cloudflareIPs[n].Trim();

                if (!string.IsNullOrEmpty(ipRange))
                {
                    string[] split = ipRange.Split('-', StringSplitOptions.TrimEntries);
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

    public static bool IsCfIP(IPAddress ipv4)
    {
        return IsCfIP(ipv4.ToString());
    }

}
