using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text.Json;
using MsmhToolsClass.DnsTool.DnsWireformatTools;

namespace MsmhToolsClass.DnsTool
{
    public static class GetIP
    {
        //================================================= Get From System

        /// <summary>
        /// Get First IP in Answer Section
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="getIPv6">Look for IPv6</param>
        /// <returns>Returns string.Empty if fail</returns>
        public static string GetIpFromSystem(string host, bool getIPv6 = false)
        {
            List<string> ips = GetIpsFromSystem(host, getIPv6);
            if (ips.Any()) return ips[0];
            return string.Empty;
        }

        /// <summary>
        /// Get a List of IPs
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="getIPv6">Look for IPv6</param>
        /// <returns>Returns Empty List if fail</returns>
        public static List<string> GetIpsFromSystem(string host, bool getIPv6 = false)
        {
            List<string> result = new();

            try
            {
                //IPAddress[] ipAddresses = Dns.GetHostEntry(host).AddressList;
                IPAddress[] ipAddresses = System.Net.Dns.GetHostAddresses(host);

                if (ipAddresses == null || ipAddresses.Length == 0)
                    return result;

                if (!getIPv6)
                {
                    for (int n = 0; n < ipAddresses.Length; n++)
                    {
                        var addressFamily = ipAddresses[n].AddressFamily;
                        if (addressFamily == AddressFamily.InterNetwork)
                        {
                            result.Add(ipAddresses[n].ToString());
                        }
                    }
                }
                else
                {
                    for (int n = 0; n < ipAddresses.Length; n++)
                    {
                        var addressFamily = ipAddresses[n].AddressFamily;
                        if (addressFamily == AddressFamily.InterNetworkV6)
                        {
                            result.Add(ipAddresses[n].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return result;
        }

        //================================================= Get From DoH Using Wire Format

        /// <summary>
        /// Get First IP in Answer Section (Using Wire Format)
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="doh">DoH Server</param>
        /// <param name="timeoutSec">Timeout (Sec)</param>
        /// <param name="proxyScheme">Use Proxy to Get IP</param>
        /// <returns>Returns string.Empty if fail</returns>
        public static async Task<string> GetIpFromDohUsingWireFormat(string host, string doh, int timeoutSec, string? proxyScheme = null)
        {
            List<string> ips = await GetIpsFromDohUsingWireFormat(host, doh, timeoutSec, proxyScheme);
            if (ips.Any()) return ips[0];
            return string.Empty;
        }

        /// <summary>
        /// Get A List of IPs (Using Wire Format)
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="doh">DoH Server</param>
        /// <param name="timeoutSec">Timeout (Sec)</param>
        /// <param name="proxyScheme">Use Proxy to Get IPs</param>
        /// <returns>Returns an Empty List if fail</returns>
        public static async Task<List<string>> GetIpsFromDohUsingWireFormat(string host, string doh, int timeoutSec, string? proxyScheme = null)
        {
            if (string.IsNullOrEmpty(proxyScheme))
                return await GetIpsFromDohUsingWireFormatNoProxy(host, doh, timeoutSec);
            else
                return await GetIpsFromDohUsingWireFormatProxy(host, doh, timeoutSec, proxyScheme);
        }

        private static async Task<List<string>> GetIpsFromDohUsingWireFormatNoProxy(string host, string doh, int timeoutSec)
        {
            List<string> ips = new();

            try
            {
                DnsMessage dnsMessage = DnsQueryFactory.CreateQuery(host);
                var queryBuffer = DnsByteExtensions.AllocatePinnedNetworkBuffer();
                int queryBufferLength = 0;
                dnsMessage.WriteBytes(queryBuffer, ref queryBufferLength);

                Uri uri = new(doh);

                using HttpClient httpClient = new();
                httpClient.Timeout = new TimeSpan(0, 0, timeoutSec);

                HttpContent content = new ReadOnlyMemoryContent(queryBuffer[..queryBufferLength]);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/dns-message");

                HttpRequestMessage message = new(HttpMethod.Post, "/dns-query");
                message.RequestUri = uri;
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/dns-message"));
                message.Content = content;

                using HttpResponseMessage r = await httpClient.SendAsync(message);

                if (r.IsSuccessStatusCode)
                {
                    byte[] buffer = await r.Content.ReadAsByteArrayAsync();

                    DnsMessage answer = DnsByteExtensions.FromBytes<DnsMessage>(buffer);
                    if (httpClient.BaseAddress != null)
                        answer.Header.Tags.Add("Resolver", httpClient.BaseAddress.ToString());

                    for (int n = 0; n < answer.Answers.Count; n++)
                    {
                        DnsResourceRecord drr = answer.Answers[n];
                        string? ip = drr.Resource?.ToString();
                        if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out IPAddress? _))
                            ips.Add(ip);
                    }
                }
            }
            catch (Exception)
            {
                // do nothing
            }

            return ips;
        }

        private static async Task<List<string>> GetIpsFromDohUsingWireFormatProxy(string host, string doh, int timeoutSec, string? proxyScheme = null)
        {
            List<string> ips = new();

            try
            {
                DnsMessage dnsMessage = DnsQueryFactory.CreateQuery(host);
                var queryBuffer = DnsByteExtensions.AllocatePinnedNetworkBuffer();
                int queryBufferLength = 0;
                dnsMessage.WriteBytes(queryBuffer, ref queryBufferLength);

                Uri uri = new(doh);

                using SocketsHttpHandler socketsHttpHandler = new();
                socketsHttpHandler.Proxy = new WebProxy(proxyScheme, true);

                using HttpClient httpClient = new(socketsHttpHandler);
                httpClient.Timeout = new TimeSpan(0, 0, timeoutSec);

                HttpContent content = new ReadOnlyMemoryContent(queryBuffer[..queryBufferLength]);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/dns-message");

                HttpRequestMessage message = new(HttpMethod.Post, "/dns-query");
                message.RequestUri = uri;
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/dns-message"));
                message.Content = content;

                using HttpResponseMessage r = await httpClient.SendAsync(message);

                if (r.IsSuccessStatusCode)
                {
                    byte[] buffer = await r.Content.ReadAsByteArrayAsync();

                    DnsMessage answer = DnsByteExtensions.FromBytes<DnsMessage>(buffer);
                    if (httpClient.BaseAddress != null)
                        answer.Header.Tags.Add("Resolver", httpClient.BaseAddress.ToString());

                    for (int n = 0; n < answer.Answers.Count; n++)
                    {
                        DnsResourceRecord drr = answer.Answers[n];
                        string? ip = drr.Resource?.ToString();
                        if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out IPAddress? _))
                            ips.Add(ip);
                    }
                }
            }
            catch (Exception)
            {
                // do nothing
            }

            return ips;
        }

        //================================================= Get From DoH Using JSON Format

        /// <summary>
        /// Get first IP Using JSON Format
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="doh">DoH Address</param>
        /// <param name="timeoutSec">Timeout (Sec)</param>
        /// <param name="proxyScheme">Use Proxy to Get IP</param>
        /// <returns>Returns Empty List if fail</returns>
        public static async Task<string> GetIpFromDohUsingJsonFormat(string host, string doh, int timeoutSec, string? proxyScheme = null)
        {
            List<string> ips = await GetIpsFromDohUsingJsonFormat(host, doh, timeoutSec, proxyScheme);
            if (ips.Any()) return ips[0];
            return string.Empty;
        }

        /// <summary>
        /// Get IPs Using JSON Format
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="doh">DoH Address</param>
        /// <param name="timeoutSec">Timeout (Sec)</param>
        /// <param name="proxyScheme">Use Proxy to Get IP</param>
        /// <returns>Returns Empty List if fail</returns>
        public static async Task<List<string>> GetIpsFromDohUsingJsonFormat(string host, string doh, int timeoutSec, string? proxyScheme = null)
        {
            if (string.IsNullOrEmpty(proxyScheme))
                return await GetIpFromDohUsingJsonFormatNoProxy(host, doh, timeoutSec);
            else
                return await GetIpFromDohUsingJsonFormatProxy(host, doh, timeoutSec, proxyScheme);
        }

        private static async Task<List<string>> GetIpFromDohUsingJsonFormatNoProxy(string host, string doh, int timeoutSec)
        {
            List<string> ips = new();

            try
            {
                doh = doh.Trim();
                if (doh.EndsWith('/')) doh = doh.TrimEnd('/');

                host = Uri.EscapeDataString(host);
                string path = $"?name={host}&type=A";
                Uri uri = new(doh + path);

                HttpClient httpClient = new();
                httpClient.Timeout = new TimeSpan(0, 0, timeoutSec);

                HttpRequestMessage message = new();
                message.Method = HttpMethod.Get;
                message.RequestUri = uri;
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/dns-json"));

                using HttpResponseMessage r = await httpClient.SendAsync(message);

                if (r.IsSuccessStatusCode)
                {
                    string contents = await r.Content.ReadAsStringAsync();
                    httpClient.Dispose();
                    ips = GetIpFromJson(contents);
                }
                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return ips;
        }

        private static async Task<List<string>> GetIpFromDohUsingJsonFormatProxy(string host, string doh, int timeoutSec, string? proxyScheme = null)
        {
            List<string> ips = new();

            try
            {
                doh = doh.Trim();
                if (doh.EndsWith('/')) doh = doh.TrimEnd('/');

                host = Uri.EscapeDataString(host);
                string path = $"?name={host}&type=A";
                Uri uri = new(doh + path);

                using SocketsHttpHandler socketsHttpHandler = new();
                socketsHttpHandler.Proxy = new WebProxy(proxyScheme, true);

                using HttpClient httpClient = new(socketsHttpHandler);
                httpClient.Timeout = new TimeSpan(0, 0, timeoutSec);

                HttpRequestMessage message = new();
                message.Method = HttpMethod.Get;
                message.RequestUri = uri;
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/dns-json"));

                using HttpResponseMessage r = await httpClient.SendAsync(message);

                if (r.IsSuccessStatusCode)
                {
                    string contents = await r.Content.ReadAsStringAsync();
                    httpClient.Dispose();
                    ips = GetIpFromJson(contents);
                }
                httpClient.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return ips;
        }

        private static List<string> GetIpFromJson(string jsonContent)
        {
            List<string> ips = new();
            try
            {
                using JsonDocument jd = JsonDocument.Parse(jsonContent);
                JsonElement rootElement = jd.RootElement;
                if (rootElement.ValueKind == JsonValueKind.Object)
                {
                    JsonProperty[] properties = rootElement.EnumerateObject().ToArray();
                    for (int n = 0; n < properties.Length; n++)
                    {
                        JsonProperty property = properties[n];
                        if (property.Name == "Answer")
                        {
                            JsonElement[] elementsProperties = property.Value.EnumerateArray().ToArray();
                            for (int n2 = 0; n2 < elementsProperties.Length; n2++)
                            {
                                JsonElement elementProperty = elementsProperties[n2];
                                if (elementProperty.ValueKind == JsonValueKind.Object)
                                {
                                    JsonProperty[] answerProperties = elementProperty.EnumerateObject().ToArray();
                                    for (int n3 = 0; n3 < answerProperties.Length; n3++)
                                    {
                                        JsonProperty answerProperty = answerProperties[n3];
                                        if (answerProperty.Name == "data")
                                        {
                                            string ip = answerProperty.Value.ToString();
                                            if (IPAddress.TryParse(ip, out IPAddress? _))
                                                ips.Add(ip);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return ips;
        }

        //================================================= Get From Plain DNS

        /// <summary>
        /// Get First IP in Answer Section
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="dnsIP">Plain DNS IP</param>
        /// <param name="dnsPort">Plain DNS Port</param>
        /// <param name="timeoutMS">Timeout (ms)</param>
        /// <returns>Returns string.Empty if fail</returns>
        public static async Task<string> GetIpFromPlainDNS(string host, string dnsIP, int dnsPort, int timeoutSec)
        {
            List<string> ips = await GetIpsFromPlainDNS(host, dnsIP, dnsPort, timeoutSec);
            if (ips.Any()) return ips[0];
            return string.Empty;
        }

        /// <summary>
        /// Get a List of IPs
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="dnsIP">Plain DNS IP</param>
        /// <param name="dnsPort">Plain DNS Port</param>
        /// <param name="timeoutMS">Timeout (ms)</param>
        /// <returns>Returns an Empty List if fail</returns>
        public async static Task<List<string>> GetIpsFromPlainDNS(string host, string dnsIP, int dnsPort, int timeoutSec)
        {
            List<string> ips;

            ips = await GetIpsFromPlainDnsUdp(host, dnsIP, dnsPort, timeoutSec);
            if (!ips.Any())
                ips = await GetIpsFromPlainDnsTcp(host, dnsIP, dnsPort, timeoutSec);

            return ips;
        }

        /// <summary>
        /// Get a List of IPs
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="dnsIP">Plain DNS IP</param>
        /// <param name="dnsPort">Plain DNS Port</param>
        /// <param name="timeoutMS">Timeout (ms)</param>
        /// <returns>Returns an Empty List if fail</returns>
        private async static Task<List<string>> GetIpsFromPlainDnsUdp(string host, string dnsIP, int dnsPort, int timeoutSec)
        {
            List<string> ips = new();

            Task task = Task.Run(async () =>
            {
                try
                {
                    DnsMessage dnsMessage = DnsQueryFactory.CreateQuery(host);
                    IPEndPoint ep = new(IPAddress.Parse(dnsIP), dnsPort);

                    Memory<byte> queryBuffer = DnsByteExtensions.AllocatePinnedNetworkBuffer();
                    int queryBufferLength = 0;
                    dnsMessage.WriteBytes(queryBuffer, ref queryBufferLength);
                    byte[] raw = queryBuffer[..queryBufferLength].ToArray();

                    using Socket socket = new(ep.AddressFamily, SocketType.Dgram, ProtocolType.Udp | ProtocolType.Tcp);
                    await socket.SendToAsync(raw, SocketFlags.None, ep);
                    byte[] buffer = new byte[65536];
                    int receivedLength = await socket.ReceiveAsync(buffer, SocketFlags.None);
                    buffer = buffer.Take(receivedLength).ToArray();

                    socket.Close();

                    DnsMessage answer = DnsByteExtensions.FromBytes<DnsMessage>(buffer);
                    answer.Header.Id = dnsMessage.Header.Id;
                    answer.Header.Tags.Add("Resolver", $"udp://{ep}/");
                    Debug.WriteLine("UDP Answers: " + answer.Answers.Count);

                    for (int n = 0; n < answer.Answers.Count; n++)
                    {
                        DnsResourceRecord drr = answer.Answers[n];
                        string? ip = drr.Resource?.ToString();
                        if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out IPAddress? _))
                            ips.Add(ip);
                    }
                }
                catch (Exception)
                {
                    // do nothing
                }
            });
            try { await task.WaitAsync(TimeSpan.FromSeconds(timeoutSec)); } catch (Exception) { }

            return ips;
        }

        /// <summary>
        /// Get a List of IPs
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="dnsIP">Plain DNS IP</param>
        /// <param name="dnsPort">Plain DNS Port</param>
        /// <param name="timeoutMS">Timeout (ms)</param>
        /// <returns>Returns an Empty List if fail</returns>
        private async static Task<List<string>> GetIpsFromPlainDnsTcp(string host, string dnsIP, int dnsPort, int timeoutSec)
        {
            List<string> ips = new();

            Task task = Task.Run(async () =>
            {
                try
                {
                    DnsMessage dnsMessage = DnsQueryFactory.CreateQuery(host);
                    IPEndPoint ep = new(IPAddress.Parse(dnsIP), dnsPort);

                    using Socket socket = new(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    await socket.ConnectAsync(ep);

                    Memory<byte> buffer = DnsByteExtensions.AllocatePinnedNetworkBuffer();
                    int sendOffset = sizeof(ushort);
                    dnsMessage.WriteBytes(buffer, ref sendOffset);

                    int fakeOffset = 0;
                    DnsByteExtensions.ToBytes((ushort)(sendOffset - sizeof(ushort)), buffer, ref fakeOffset);

                    Memory<byte> sendBuffer = buffer[..sendOffset];
                    await socket.SendAsync(sendBuffer, SocketFlags.None);
                    int received = await socket.ReceiveAsync(buffer, SocketFlags.None);

                    int offset = 0;
                    ushort answerLength = DnsByteExtensions.ReadUInt16(buffer, ref offset);

                    while (received < answerLength)
                    {
                        received += await socket.ReceiveAsync(buffer[received..], SocketFlags.None);
                    }

                    socket.Close();

                    Memory<byte> answerBuffer = buffer.Slice(offset, answerLength);

                    DnsMessage answer = DnsByteExtensions.FromBytes<DnsMessage>(answerBuffer);
                    answer.Header.Tags.Add("Resolver", $"tcp://{ep}/");
                    Debug.WriteLine("TCP Answers: " + answer.Answers.Count);

                    for (int n = 0; n < answer.Answers.Count; n++)
                    {
                        DnsResourceRecord drr = answer.Answers[n];
                        string? ip = drr.Resource?.ToString();
                        if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out IPAddress? _))
                            ips.Add(ip);
                    }
                }
                catch (Exception)
                {
                    // do nothing
                }
            });
            try { await task.WaitAsync(TimeSpan.FromSeconds(timeoutSec)); } catch (Exception) { }

            return ips;
        }

    }
}
