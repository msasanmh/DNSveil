using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DnsClient
{
    public static byte[] ConvertQueryBufferProtocol(byte[] buffer, DnsEnums.DnsProtocol fromProtocol, DnsEnums.DnsProtocol toProtocol)
    {
        try
        {
            if (buffer.Length < 3) return buffer;
            if (fromProtocol == toProtocol) return buffer;
            // UDP => UDP = Same
            // UDP => TCP
            if (fromProtocol == DnsEnums.DnsProtocol.UDP && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // UDP => DoH = Same
            // UDP => DoT
            if (fromProtocol == DnsEnums.DnsProtocol.UDP && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // UDP => DNSCrypt = Same

            // TCP => UDP
            else if (fromProtocol == DnsEnums.DnsProtocol.TCP && toProtocol == DnsEnums.DnsProtocol.UDP)
                return RemoveTcpMessageLength(buffer);
            // TCP => TCP = Same
            // TCP => DoH
            else if (fromProtocol == DnsEnums.DnsProtocol.TCP && toProtocol == DnsEnums.DnsProtocol.DoH)
                return RemoveTcpMessageLength(buffer);
            // TCP => DoT = Same
            // TCP => DNSCrypt
            else if (fromProtocol == DnsEnums.DnsProtocol.TCP && toProtocol == DnsEnums.DnsProtocol.DnsCrypt)
                return RemoveTcpMessageLength(buffer);

            // DoH => UDP = Same
            // DoH => TCP
            else if (fromProtocol == DnsEnums.DnsProtocol.DoH && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // DoH => DoH = Same
            // DoH => DoT
            else if (fromProtocol == DnsEnums.DnsProtocol.DoH && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // DoH => DNSCrypt = Same

            // DoT => UDP
            else if (fromProtocol == DnsEnums.DnsProtocol.DoT && toProtocol == DnsEnums.DnsProtocol.UDP)
                return RemoveTcpMessageLength(buffer);
            // DoT => TCP = Same
            // DoT => DoH
            else if (fromProtocol == DnsEnums.DnsProtocol.DoT && toProtocol == DnsEnums.DnsProtocol.DoH)
                return RemoveTcpMessageLength(buffer);
            // DoT => DoT = Same
            // DoT => DNSCrypt
            else if (fromProtocol == DnsEnums.DnsProtocol.DoT && toProtocol == DnsEnums.DnsProtocol.DnsCrypt)
                return RemoveTcpMessageLength(buffer);

            // DNSCrypt => UDP = Same
            // DNSCrypt => TCP
            else if (fromProtocol == DnsEnums.DnsProtocol.DnsCrypt && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // DNSCrypt => DoH = Same
            // DNSCrypt => DoT
            else if (fromProtocol == DnsEnums.DnsProtocol.DnsCrypt && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // DNSCrypt => DNSCrypt = Same
            else return buffer;

            static byte[] AddTcpMessageLength(byte[] buffer)
            {
                ushort tcpMessageLength = Convert.ToUInt16(buffer.Length);
                ByteArrayTool.TryConvertUInt16ToBytes(tcpMessageLength, out byte[] tcpMessageLengthBytes);
                return tcpMessageLengthBytes.Concat(buffer).ToArray();
            }

            static byte[] RemoveTcpMessageLength(byte[] buffer) => buffer[2..];
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS DnsClient ConvertQueryBufferProtocol: " + ex.Message);
            return buffer;
        }
    }

    public async static Task<byte[]> QueryAsync(byte[] queryBuffer, DnsEnums.DnsProtocol bufferProtocol, string dnsServer, bool allowInsecure, IPAddress bootstrapIP, int bootstrapPort, int timeoutMS, CancellationToken ct, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        byte[] result = Array.Empty<byte>();

        try
        {
            if (queryBuffer.Length <= 0) return result;
            if (string.IsNullOrEmpty(dnsServer)) return result;

            // From System
            if (dnsServer.ToLower().Equals("system"))
            {
                // Convert Buffer ListenerProtocol To System Protocol
                queryBuffer = ConvertQueryBufferProtocol(queryBuffer, bufferProtocol, SystemDns.Protocol);

                SystemDns systemDns = new(queryBuffer, timeoutMS, ct);
                result = await systemDns.GetResponseAsync();

                // Convert System Protocol To Buffer ListenerProtocol
                return ConvertQueryBufferProtocol(result, SystemDns.Protocol, bufferProtocol);
            }

            // From Servers
            DnsReader dnsReader = new(dnsServer, null);

            // Convert Buffer ListenerProtocol To Dns Server ListenerProtocol
            queryBuffer = ConvertQueryBufferProtocol(queryBuffer, bufferProtocol, dnsReader.Protocol);

            //Stopwatch sw = Stopwatch.StartNew();
            
            if (dnsReader.Protocol == DnsEnums.DnsProtocol.UDP)
            {
                UdpPlainDns udpPlainDns = new(queryBuffer, dnsReader, timeoutMS, ct);
                result = await udpPlainDns.GetResponseAsync().ConfigureAwait(false);
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.TCP)
            {
                TcpPlainDns tcpPlainDns = new(queryBuffer, dnsReader, timeoutMS, ct, proxyScheme, proxyUser, proxyPass);
                result = await tcpPlainDns.GetResponseAsync().ConfigureAwait(false);
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
            {
                DoHDns doHDns = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, ct, proxyScheme, proxyUser, proxyPass);
                result = await doHDns.GetResponseAsync().ConfigureAwait(false);
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoT)
            {
                DoTDns doTDns = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, ct, proxyScheme, proxyUser, proxyPass);
                result = await doTDns.GetResponseAsync().ConfigureAwait(false);
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
            {
                DNSCryptDns dnsCryptDns = new(queryBuffer, dnsReader, timeoutMS, ct, proxyScheme, proxyUser, proxyPass);
                result = await dnsCryptDns.GetResponseAsync().ConfigureAwait(false);
            }

            //sw.Stop();
            //Debug.WriteLine($"========= {dnsReader.Protocol} => " + sw.ElapsedMilliseconds);

            // Convert Dns Server ListenerProtocol To Buffer ListenerProtocol
            return ConvertQueryBufferProtocol(result, dnsReader.Protocol, bufferProtocol);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DNS DnsClient QueryAsync 1: " + ex.Message);
            return result;
        }
    }

    public async static Task<byte[]> QueryAsync(byte[] queryBuffer, DnsEnums.DnsProtocol bufferProtocol, List<string> dnsServers, bool allowInsecure, IPAddress bootstrapIP, int bootstrapPort, int timeoutMS, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        return await Task.Run(async () =>
        {
            byte[] result = Array.Empty<byte>();
            if (!dnsServers.Any()) return result;

            try
            {
                if (dnsServers.Count > 1)
                {
                    List<Task<byte[]>> tasks = new();

                    CancellationTokenSource cts = new();
                    for (int n = 0; n < dnsServers.Count; n++)
                    {
                        string dns = dnsServers[n];
                        Task<byte[]> task = QueryAsync(queryBuffer, bufferProtocol, dns, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, cts.Token, proxyScheme, proxyUser, proxyPass);
                        tasks.Add(task);
                    }

                    while (true)
                    {
                        if (tasks.Count == 0) break;
                        Task<byte[]> taskResult = await Task.WhenAny(tasks).ConfigureAwait(false);
                        byte[] bytes = taskResult.Result;
                        DnsMessage dm = DnsMessage.Read(bytes, bufferProtocol);
                        if (dm.IsSuccess)
                        {
                            result = bytes;
                            break;
                        }
                        tasks.Remove(taskResult);
                    }
                    _ = Task.Run(() => cts.Cancel());
                }
                else if (dnsServers.Count == 1)
                {
                    return await QueryAsync(queryBuffer, bufferProtocol, dnsServers[0], allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, CancellationToken.None, proxyScheme, proxyUser, proxyPass);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DNS DnsClient QueryAsync 2: " + ex.Message);
            }

            return result;
        }).ConfigureAwait(false);
    }

    public async static Task<byte[]> QueryAsync(byte[] queryBuffer, DnsEnums.DnsProtocol bufferProtocol, AgnosticSettings ds)
    {
        return await QueryAsync(queryBuffer, bufferProtocol, ds.DNSs, ds.AllowInsecure, ds.BootstrapIpAddress, ds.BootstrapPort, ds.DnsTimeoutSec * 1000, ds.UpstreamProxyScheme, ds.UpstreamProxyUser, ds.UpstreamProxyPass);
    }

}
