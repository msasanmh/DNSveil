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
            // UDP => AnonymizedDNSCrypt = Same

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
            // TCP => AnonymizedDNSCrypt
            else if (fromProtocol == DnsEnums.DnsProtocol.TCP && toProtocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt)
                return RemoveTcpMessageLength(buffer);

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
            // DoT => AnonymizedDNSCrypt
            else if (fromProtocol == DnsEnums.DnsProtocol.DoT && toProtocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt)
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
            // DoH => AnonymizedDNSCrypt = Same

            // DNSCrypt => UDP = Same
            // DNSCrypt => TCP
            else if (fromProtocol == DnsEnums.DnsProtocol.DnsCrypt && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // DNSCrypt => DoH = Same
            // DNSCrypt => DoT
            else if (fromProtocol == DnsEnums.DnsProtocol.DnsCrypt && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // DNSCrypt => DNSCrypt = Same
            // DNSCrypt => AnonymizedDNSCrypt = Same

            // AnonymizedDNSCrypt => UDP = Same
            // AnonymizedDNSCrypt => TCP
            else if (fromProtocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // AnonymizedDNSCrypt => DoH = Same
            // AnonymizedDNSCrypt => DoT
            else if (fromProtocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // AnonymizedDNSCrypt => DNSCrypt = Same
            // AnonymizedDNSCrypt => AnonymizedDNSCrypt = Same
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
                queryBuffer = ConvertQueryBufferProtocol(queryBuffer, bufferProtocol, SystemDnsClient.Protocol);

                SystemDnsClient systemDns = new(queryBuffer, timeoutMS, ct);
                result = await systemDns.GetResponseAsync();

                // Convert System Protocol To Buffer ListenerProtocol
                return ConvertQueryBufferProtocol(result, SystemDnsClient.Protocol, bufferProtocol);
            }

            // From Servers
            DnsReader dnsReader = new(dnsServer, null);

            // Convert Buffer ListenerProtocol To Dns Server ListenerProtocol
            queryBuffer = ConvertQueryBufferProtocol(queryBuffer, bufferProtocol, dnsReader.Protocol);

            //Stopwatch sw = Stopwatch.StartNew();
            
            if (dnsReader.Protocol == DnsEnums.DnsProtocol.UDP)
            {
                UdpPlainClient udpPlainClient = new(queryBuffer, dnsReader, timeoutMS, ct);
                result = await udpPlainClient.GetResponseAsync().ConfigureAwait(false);
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.TCP)
            {
                TcpPlainClient tcpPlainClient = new(queryBuffer, dnsReader, timeoutMS, proxyScheme, proxyUser, proxyPass, ct);
                result = await tcpPlainClient.GetResponseAsync().ConfigureAwait(false);

                if (result.Length == 0 && !string.IsNullOrWhiteSpace(proxyScheme)) // Try Without Upstream
                {
                    tcpPlainClient = new(queryBuffer, dnsReader, timeoutMS, null, null, null, ct);
                    result = await tcpPlainClient.GetResponseAsync().ConfigureAwait(false);
                }
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoT)
            {
                DoTClient doTClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, proxyScheme, proxyUser, proxyPass, ct);
                result = await doTClient.GetResponseAsync().ConfigureAwait(false);

                if (result.Length == 0 && !string.IsNullOrWhiteSpace(proxyScheme)) // Try Without Upstream
                {
                    doTClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, null, null, null, ct);
                    result = await doTClient.GetResponseAsync().ConfigureAwait(false);
                }
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
            {
                DoHClient doHClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, proxyScheme, proxyUser, proxyPass, ct);
                result = await doHClient.GetResponseAsync().ConfigureAwait(false);

                if (result.Length == 0 && !string.IsNullOrWhiteSpace(proxyScheme)) // Try Without Upstream
                {
                    doHClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, null, null, null, ct);
                    result = await doHClient.GetResponseAsync().ConfigureAwait(false);
                }
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.ObliviousDohTarget) // Not Implemented Yet
            {
                ODoHClient oDoHClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, proxyScheme, proxyUser, proxyPass, ct);
                result = await oDoHClient.GetResponseAsync().ConfigureAwait(false);

                if (result.Length == 0 && !string.IsNullOrWhiteSpace(proxyScheme)) // Try Without Upstream
                {
                    oDoHClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, null, null, null, ct);
                    result = await oDoHClient.GetResponseAsync().ConfigureAwait(false);
                }
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DnsCrypt || dnsReader.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt)
            {
                DNSCryptClient dnsCryptClient = new(queryBuffer, dnsReader, timeoutMS, proxyScheme, proxyUser, proxyPass, ct);
                result = await dnsCryptClient.GetResponseAsync().ConfigureAwait(false);

                if (result.Length == 0 && !string.IsNullOrWhiteSpace(proxyScheme)) // Try Without Upstream
                {
                    dnsCryptClient = new(queryBuffer, dnsReader, timeoutMS, null, null, null, ct);
                    result = await dnsCryptClient.GetResponseAsync().ConfigureAwait(false);
                }
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
