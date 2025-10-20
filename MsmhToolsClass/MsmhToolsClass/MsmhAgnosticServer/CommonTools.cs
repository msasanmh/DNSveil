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

        if (NetworkTool.IsIP(hostname, out _)) return hostname;
        NetworkTool.URL hostDetails = NetworkTool.GetUrlOrDomainDetails(hostname, 443);
        if (hostDetails.BaseHost.Equals(hostname)) return hostname;
        return $"*.{hostDetails.BaseHost}";
    }

    public static bool IsCfIP(string ipStr)
    {
        try
        {
            bool isIp = NetworkTool.IsIP(ipStr, out _);
            if (!isIp) return false;

            List<string> cloudflareCIDRs = new()
            {
                "103.21.244.0/22",
                "103.22.200.0/22",
                "103.31.4.0/22",
                "104.16.0.0/13",
                "104.24.0.0/14",
                "108.162.192.0/18",
                "131.0.72.0/22",
                "141.101.64.0/18",
                "162.158.0.0/15",
                "172.64.0.0/13",
                "173.245.48.0/20",
                "188.114.96.0/20",
                "190.93.240.0/20",
                "197.234.240.0/22",
                "198.41.128.0/17",
                "2400:cb00::/32",
                "2405:8100::/32",
                "2405:b500::/32",
                "2606:4700::/32",
                "2803:f800::/32",
                "2a06:98c0::/29",
                "2c0f:f248::/32"
            };

            for (int n = 0; n < cloudflareCIDRs.Count; n++)
            {
                string cidr = cloudflareCIDRs[n].Trim();
                bool isInRange = NetworkTool.IsIpInRange(ipStr, cidr);
                if (isInRange) return true;
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
        return IsCfIP(ipv4.ToStringNoScopeId());
    }

}
