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
            // System => System = Same
            // System => UDP = Same
            // System => TCP
            if (fromProtocol == DnsEnums.DnsProtocol.System && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // System => TcpOverUdp = Same
            // System => DoH = Same
            // System => DoT
            if (fromProtocol == DnsEnums.DnsProtocol.System && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // System => DNSCrypt = Same
            // System => AnonymizedDNSCrypt = Same

            // UDP => System = Same
            // UDP => UDP = Same
            // UDP => TCP
            if (fromProtocol == DnsEnums.DnsProtocol.UDP && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // UDP => TcpOverUdp = Same
            // UDP => DoH = Same
            // UDP => DoT
            if (fromProtocol == DnsEnums.DnsProtocol.UDP && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // UDP => DNSCrypt = Same
            // UDP => AnonymizedDNSCrypt = Same

            // TCP => System
            else if (fromProtocol == DnsEnums.DnsProtocol.TCP && toProtocol == DnsEnums.DnsProtocol.System)
                return RemoveTcpMessageLength(buffer);
            // TCP => UDP
            else if (fromProtocol == DnsEnums.DnsProtocol.TCP && toProtocol == DnsEnums.DnsProtocol.UDP)
                return RemoveTcpMessageLength(buffer);
            // TCP => TCP = Same
            // TCP => TcpOverUdp
            else if (fromProtocol == DnsEnums.DnsProtocol.TCP && toProtocol == DnsEnums.DnsProtocol.TcpOverUdp)
                return RemoveTcpMessageLength(buffer);
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

            // TcpOverUdp => System = Same
            // TcpOverUdp => UDP = Same
            // TcpOverUdp => TCP
            if (fromProtocol == DnsEnums.DnsProtocol.TcpOverUdp && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // TcpOverUdp => TcpOverUdp = Same
            // TcpOverUdp => DoH = Same
            // TcpOverUdp => DoT
            if (fromProtocol == DnsEnums.DnsProtocol.TcpOverUdp && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // TcpOverUdp => DNSCrypt = Same
            // TcpOverUdp => AnonymizedDNSCrypt = Same

            // DoT => System
            else if (fromProtocol == DnsEnums.DnsProtocol.DoT && toProtocol == DnsEnums.DnsProtocol.System)
                return RemoveTcpMessageLength(buffer);
            // DoT => UDP
            else if (fromProtocol == DnsEnums.DnsProtocol.DoT && toProtocol == DnsEnums.DnsProtocol.UDP)
                return RemoveTcpMessageLength(buffer);
            // DoT => TCP = Same
            // DoT => TcpOverUdp
            else if (fromProtocol == DnsEnums.DnsProtocol.DoT && toProtocol == DnsEnums.DnsProtocol.TcpOverUdp)
                return RemoveTcpMessageLength(buffer);
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

            // DoH => System = Same
            // DoH => UDP = Same
            // DoH => TCP
            else if (fromProtocol == DnsEnums.DnsProtocol.DoH && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // DoH => TcpOverUdp = Same
            // DoH => DoH = Same
            // DoH => DoT
            else if (fromProtocol == DnsEnums.DnsProtocol.DoH && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // DoH => DNSCrypt = Same
            // DoH => AnonymizedDNSCrypt = Same

            // DNSCrypt => System = Same
            // DNSCrypt => UDP = Same
            // DNSCrypt => TCP
            else if (fromProtocol == DnsEnums.DnsProtocol.DnsCrypt && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // DNSCrypt => TcpOverUdp = Same
            // DNSCrypt => DoH = Same
            // DNSCrypt => DoT
            else if (fromProtocol == DnsEnums.DnsProtocol.DnsCrypt && toProtocol == DnsEnums.DnsProtocol.DoT)
                return AddTcpMessageLength(buffer);
            // DNSCrypt => DNSCrypt = Same
            // DNSCrypt => AnonymizedDNSCrypt = Same

            // AnonymizedDNSCrypt => System = Same
            // AnonymizedDNSCrypt => UDP = Same
            // AnonymizedDNSCrypt => TCP
            else if (fromProtocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt && toProtocol == DnsEnums.DnsProtocol.TCP)
                return AddTcpMessageLength(buffer);
            // AnonymizedDNSCrypt => TcpOverUdp = Same
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

    public async static Task<byte[]> QueryAsync(byte[] queryBuffer, DnsEnums.DnsProtocol bufferProtocol, string dnsServer, bool allowInsecure, IPAddress bootstrapIP, int bootstrapPort, int timeoutMS, CancellationToken ct, List<AgnosticProgram.Rules.Rule>? ruleList = null, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        byte[] result = Array.Empty<byte>();

        try
        {
            if (queryBuffer.Length <= 0) return result;
            if (string.IsNullOrEmpty(dnsServer)) return result;

            // From Servers
            DnsReader dnsReader = new(dnsServer, null);

            // Convert Buffer ListenerProtocol To Dns Server ListenerProtocol
            queryBuffer = ConvertQueryBufferProtocol(queryBuffer, bufferProtocol, dnsReader.Protocol);

            //Stopwatch sw = Stopwatch.StartNew();
            
            if (dnsReader.Protocol == DnsEnums.DnsProtocol.System)
            {
                SystemDnsClient systemDns = new(queryBuffer, timeoutMS, ct);
                result = await systemDns.GetResponseAsync().ConfigureAwait(false);
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.UDP)
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
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.TcpOverUdp)
            {
                TcpOverUdpPlainClient tcpOverUdpPlainClient = new(queryBuffer, dnsReader, timeoutMS, ct);
                result = await tcpOverUdpPlainClient.GetResponseAsync().ConfigureAwait(false);
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoT)
            {
                DoTClient doTClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, ruleList, proxyScheme, proxyUser, proxyPass, ct);
                result = await doTClient.GetResponseAsync().ConfigureAwait(false);

                if (result.Length == 0 && !string.IsNullOrWhiteSpace(proxyScheme)) // Try Without Upstream
                {
                    doTClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, ruleList, null, null, null, ct);
                    result = await doTClient.GetResponseAsync().ConfigureAwait(false);
                }
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.DoH)
            {
                DoHClient doHClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, ruleList, proxyScheme, proxyUser, proxyPass, ct);
                result = await doHClient.GetResponseAsync().ConfigureAwait(false);

                if (result.Length == 0 && !string.IsNullOrWhiteSpace(proxyScheme)) // Try Without Upstream
                {
                    doHClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, ruleList, null, null, null, ct);
                    result = await doHClient.GetResponseAsync().ConfigureAwait(false);
                }
            }
            else if (dnsReader.Protocol == DnsEnums.DnsProtocol.ObliviousDoH) // Not Implemented Yet
            {
                ODoHClient oDoHClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, ruleList, proxyScheme, proxyUser, proxyPass, ct);
                result = await oDoHClient.GetResponseAsync().ConfigureAwait(false);

                if (result.Length == 0 && !string.IsNullOrWhiteSpace(proxyScheme)) // Try Without Upstream
                {
                    oDoHClient = new(queryBuffer, dnsReader, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, ruleList, null, null, null, ct);
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

    public async static Task<byte[]> QueryAsync(byte[] queryBuffer, DnsEnums.DnsProtocol bufferProtocol, List<string> dnsServers, bool allowInsecure, IPAddress bootstrapIP, int bootstrapPort, int timeoutMS, List<AgnosticProgram.Rules.Rule>? ruleList, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
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
                        Task<byte[]> task = QueryAsync(queryBuffer, bufferProtocol, dns, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, cts.Token, ruleList, proxyScheme, proxyUser, proxyPass);
                        tasks.Add(task);
                    }

                    while (true)
                    {
                        if (tasks.Count == 0) break;
                        Task<byte[]> taskResult = await Task.WhenAny(tasks).ConfigureAwait(false);
                        byte[] bytes = await taskResult.ConfigureAwait(false);
                        DnsMessage dm = DnsMessage.Read(bytes, bufferProtocol);
                        if (dm.IsSuccess)
                        {
                            result = bytes;
                            break;
                        }
                        tasks.Remove(taskResult);
                    }
                    tasks.Clear();
                    _ = Task.Run(() => cts.Cancel());
                }
                else if (dnsServers.Count == 1)
                {
                    return await QueryAsync(queryBuffer, bufferProtocol, dnsServers[0], allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, CancellationToken.None, ruleList, proxyScheme, proxyUser, proxyPass).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DNS DnsClient QueryAsync 2: " + ex.Message);
            }

            return result;
        }).ConfigureAwait(false);
    }

    public async static Task<byte[]> QueryAsync(byte[] queryBuffer, DnsEnums.DnsProtocol bufferProtocol, List<string> dnsServers, bool allowInsecure, IPAddress bootstrapIP, int bootstrapPort, int timeoutMS, string? proxyScheme = null, string? proxyUser = null, string? proxyPass = null)
    {
        return await QueryAsync(queryBuffer, bufferProtocol, dnsServers, allowInsecure, bootstrapIP, bootstrapPort, timeoutMS, null, proxyScheme, proxyUser, proxyPass).ConfigureAwait(false);
    }

    public async static Task<byte[]> QueryAsync(byte[] queryBuffer, DnsEnums.DnsProtocol bufferProtocol, AgnosticSettings ds)
    {
        return await QueryAsync(queryBuffer, bufferProtocol, ds.DNSs, ds.AllowInsecure, ds.BootstrapIpAddress, ds.BootstrapPort, ds.DnsTimeoutSec * 1000, ds.UpstreamProxyScheme, ds.UpstreamProxyUser, ds.UpstreamProxyPass).ConfigureAwait(false);
    }

}
