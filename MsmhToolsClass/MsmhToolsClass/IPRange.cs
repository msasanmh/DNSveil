using System.Diagnostics;
using System.Net;
using System.Numerics;

namespace MsmhToolsClass;

public class IPRange : IDisposable
{
    private readonly List<string> CIDR_List = new();
    private string CIDR = string.Empty;
    private bool PPause = false;
    private bool Cancel = false;
    private List<IPAddress>? PIPs = new();

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    public List<IPAddress> IPs
    {
        get
        {
            List<IPAddress> ips = new();
            if (PIPs == null) return ips;
            lock (PIPs)
            {
                ips = PIPs.ToList();
            }
            return ips;
        }
    }
    public BigInteger NubmerOfGeneratedIPs { get; private set; } = 0;

    /// <summary>
    /// Get All IPs In The CIDR Range
    /// </summary>
    /// <param name="cidrList">IPv4 CIDR List Or IPv6 CIDR List</param>
    public IPRange(List<string> cidrList)
    {
        CIDR_List = cidrList;
    }

    /// <summary>
    /// Get All IPs In The CIDR Range
    /// </summary>
    /// <param name="cidrList">IPv4 CIDR Or IPv6 CIDR</param>
    public IPRange(string cidr)
    {
        CIDR = cidr;
    }

    public async void StartGenerateIPs()
    {
        await Task.Run(async () =>
        {
            try
            {
                Cancel = false;
                if (PIPs == null) return;
                IsRunning = true;
                IsPaused = false;
                PIPs.Clear();
                NubmerOfGeneratedIPs = 0;

                for (int n = 0; n < CIDR_List.Count; n++)
                {
                    if (Cancel) break;
                    while (true)
                    {
                        if (Cancel) break;
                        if (!PPause) break;
                        IsPaused = PPause;
                        await Task.Delay(50);
                    }
                    IsPaused = PPause;

                    CIDR = CIDR_List[n];


                    foreach (IPAddress? ip in GetIPsInCIDR(CIDR))
                    {
                        if (Cancel) break;
                        while (true)
                        {
                            if (Cancel) break;
                            if (!PPause) break;
                            IsPaused = PPause;
                            await Task.Delay(50);
                        }
                        IsPaused = PPause;

                        if (ip != null)
                        {
                            lock (PIPs) // Just In Case
                            {
                                try
                                {
                                    PIPs.Add(ip);
                                    NubmerOfGeneratedIPs++;
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }

                IsRunning = false;
                IsPaused = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IPRange StartGenerateIPs: " + ex.Message);
                IsRunning = false;
                IsPaused = false;
            }
        }).ConfigureAwait(false);
    }

    public void Pause(bool pause)
    {
        PPause = pause;
    }

    public void Stop()
    {
        Cancel = true;
    }

    public void Dispose()
    {
        try
        {
            Cancel = true;
            PIPs = null;
            NubmerOfGeneratedIPs = 0;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.SuppressFinalize(this);
        }
        catch (Exception) { }
    }

    public static IEnumerable<IPAddress?> GetIPsInCIDR(string cidr)
    {
        if (cidr.Contains('/'))
        {
            // Split CIDR Into Base IP And Prefix Length
            string[] split = cidr.Split('/', StringSplitOptions.TrimEntries);
            if (split.Length == 2)
            {
                string cidrBase = split[0];
                string prefix = split[1];
                bool isInt = int.TryParse(prefix, out int prefixLength);
                if (isInt)
                {
                    bool isCidrBaseIP = IPAddress.TryParse(cidrBase, out IPAddress? cidrIP);
                    if (isCidrBaseIP && cidrIP != null)
                    {
                        byte[] cidrBytes = cidrIP.GetAddressBytes();
                        bool isCidrBaseIPv6 = NetworkTool.IsIPv6(cidrIP);
                        if (!isCidrBaseIPv6)
                        {
                            // IPv4 // Calculate The Number Of Hosts
                            int numberOfHosts = -1;

                            try
                            {
                                numberOfHosts = (int)Math.Pow(2, 32 - prefixLength);
                            }
                            catch (Exception) { }

                            if (numberOfHosts == -1) yield break;

                            // Generate All IPs In Range
                            for (int i = 0; i < numberOfHosts; i++)
                            {
                                byte[] currentIpBytes = new byte[cidrBytes.Length];

                                try
                                {
                                    Buffer.BlockCopy(cidrBytes, 0, currentIpBytes, 0, cidrBytes.Length);
                                }
                                catch (Exception)
                                {
                                    yield break;
                                }

                                bool isSuccess = addToIP(currentIpBytes, i);
                                if (!isSuccess) yield break;

                                yield return new IPAddress(currentIpBytes);
                            }

                            // Add An Int Value To An IP Byte Array
                            static bool addToIP(byte[] ip, int value)
                            {
                                try
                                {
                                    for (int n = ip.Length - 1; n >= 0; n--)
                                    {
                                        int result = ip[n] + value;
                                        ip[n] = (byte)(result & 0xFF);
                                        value = result >> 8;
                                    }
                                    return true;
                                }
                                catch (Exception)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            // IPv6 // Calculate The Number Of Hosts
                            BigInteger numberOfHosts = -1;

                            try
                            {
                                numberOfHosts = BigInteger.Pow(2, 128 - prefixLength);
                            }
                            catch (Exception) { }

                            if (numberOfHosts == -1) yield break;

                            // Convert Base IP To BigInteger
                            BigInteger baseIpBigInt = new(cidrBytes, isUnsigned: true, isBigEndian: true);

                            // Generate All IPs In Range
                            for (BigInteger i = 0; i < numberOfHosts; i++)
                            {
                                byte[] currentIpBytes;
                                
                                try
                                {
                                    BigInteger currentIpBigInt = baseIpBigInt + i;
                                    currentIpBytes = currentIpBigInt.ToByteArray(isUnsigned: true, isBigEndian: true);

                                    // Ensure The Array Is Always 16 Bytes Long (IPv6 Is 128 Bits Or 16 Bytes)
                                    if (currentIpBytes.Length < 16)
                                    {
                                        //Array.Resize(ref currentIpBytes, 16);
                                        byte[] paddedBytes = new byte[16];
                                        Buffer.BlockCopy(currentIpBytes, 0, paddedBytes, 16 - currentIpBytes.Length, currentIpBytes.Length);
                                        currentIpBytes = paddedBytes;
                                    }
                                }
                                catch (Exception)
                                {
                                    yield break;
                                }

                                yield return new IPAddress(currentIpBytes);
                            }
                        }
                    }
                }
            }
        }
    }

}