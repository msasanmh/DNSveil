using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MsmhTools.DnsTool.DnsWireformatTools;

namespace MsmhTools.DnsTool
{
    public static class GetIP
    {
        //================================================= Get From DoH: Try Wire Format First, Then Try Json Format

        /// <summary>
        /// Get First IP in Answer Section (Use Wire Then try Json)
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="doh">DoH Server</param>
        /// <param name="timeoutSec">Timeout (Sec)</param>
        /// <param name="proxyScheme">Use Proxy to Get IP</param>
        /// <returns>Returns string.Empty if fail</returns>
        public static async Task<string> GetIpFromDoH(string host, string doh, int timeoutSec, string? proxyScheme = null)
        {
            string ip = await GetIpFromDoHUsingWireFormat(host, doh, timeoutSec, proxyScheme);
            if (string.IsNullOrEmpty(ip))
                ip = await GetIpFromDohUsingJsonFormat(host, doh, timeoutSec, proxyScheme);
            return ip;
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
        public static async Task<string> GetIpFromDoHUsingWireFormat(string host, string doh, int timeoutSec, string? proxyScheme = null)
        {
            List<string> ips = await GetIpsFromDoHUsingWireFormat(host, doh, timeoutSec, proxyScheme);
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
        public static async Task<List<string>> GetIpsFromDoHUsingWireFormat(string host, string doh, int timeoutSec, string? proxyScheme = null)
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

                HttpContent content = new ReadOnlyMemoryContent(queryBuffer.Slice(0, queryBufferLength));
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
                    object? obj = new();
                    object? toString = obj.ToString();
                    if (toString != null)
                        answer.Header.Tags.Add("Resolver", toString);

                    for (int n = 0; n < answer.Answers.Count; n++)
                    {
                        DnsResourceRecord drr = answer.Answers[n];
                        string? ip = drr.Resource.ToString();
                        if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out IPAddress _))
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

                HttpContent content = new ReadOnlyMemoryContent(queryBuffer.Slice(0, queryBufferLength));
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
                    object? obj = new();
                    object? toString = obj.ToString();
                    if (toString != null)
                        answer.Header.Tags.Add("Resolver", toString);

                    for (int n = 0; n < answer.Answers.Count; n++)
                    {
                        DnsResourceRecord drr = answer.Answers[n];
                        string? ip = drr.Resource.ToString();
                        if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out IPAddress _))
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
        /// Convert Host to IPv4 Using JSON Format
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="dohs">A List of DoH Addresses</param>
        /// <param name="timeoutSec">Timeout (Sec)</param>
        /// <param name="proxyScheme">Use Proxy to Get IP</param>
        /// <returns>Returns string.Empty if fail</returns>
        public static async Task<string> GetIpFromDohUsingJsonFormat(string host, List<string> dohs, int timeoutSec, string? proxyScheme = null)
        {
            for (int n = 0; n < dohs.Count; n++)
            {
                string doh = dohs[n];
                string ip = await GetIpFromDohUsingJsonFormat(host, doh, timeoutSec, proxyScheme);
                if (!string.IsNullOrEmpty(ip)) return ip;
            }
            return string.Empty;
        }

        /// <summary>
        /// Convert Host to IPv4 Using JSON Format
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="doh">DoH Address</param>
        /// <param name="timeoutSec">Timeout (Sec)</param>
        /// <param name="proxyScheme">Use Proxy to Get IP</param>
        /// <returns>Returns string.Empty if fail</returns>
        public static async Task<string> GetIpFromDohUsingJsonFormat(string host, string doh, int timeoutSec, string? proxyScheme = null)
        {
            string apiPath1 = "/dns-query";
            string apiPath2 = "/resolve";

            doh = doh.Trim();
            if (doh.EndsWith(apiPath1))
                doh = doh.Replace(apiPath1, string.Empty);
            else if (doh.EndsWith(apiPath2))
                doh = doh.Replace(apiPath2, string.Empty);

            string jsonString = await GetIpFromDohUsingJsonFormatInternal(host, doh, timeoutSec, apiPath1, proxyScheme);
            if (string.IsNullOrEmpty(jsonString))
            {
                jsonString = await GetIpFromDohUsingJsonFormatInternal(host, doh, timeoutSec, apiPath2, proxyScheme);
            }
            
            return GetIpFromJson(jsonString);
        }

        private static async Task<string> GetIpFromDohUsingJsonFormatInternal(string host, string dohWithoutPath, int timeoutSec, string apiPath, string? proxyScheme = null)
        {
            if (string.IsNullOrEmpty(proxyScheme))
                return await GetIpFromDohUsingJsonFormatInternalNoProxy(host, dohWithoutPath, timeoutSec, apiPath);
            else
                return await GetIpFromDohUsingJsonFormatInternalProxy(host, dohWithoutPath, timeoutSec, apiPath, proxyScheme);
        }

        private static async Task<string> GetIpFromDohUsingJsonFormatInternalNoProxy(string host, string dohWithoutPath, int timeoutSec, string apiPath)
        {
            try
            {
                host = Uri.EscapeDataString(host);
                string path = $"{apiPath}?name={host}&type=A";
                Uri uri = new(dohWithoutPath + path);

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
                    return contents;
                }
                httpClient.Dispose();
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        private static async Task<string> GetIpFromDohUsingJsonFormatInternalProxy(string host, string dohWithoutPath, int timeoutSec, string apiPath, string? proxyScheme = null)
        {
            try
            {
                host = Uri.EscapeDataString(host);
                string path = $"{apiPath}?name={host}&type=A";
                Uri uri = new(dohWithoutPath + path);

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
                    return contents;
                }
                httpClient.Dispose();
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        private static string GetIpFromJson(string jsonContent)
        {
            string ip = string.Empty;
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
                                            ip = answerProperty.Value.ToString();
                                            if (IPAddress.TryParse(ip, out IPAddress _))
                                                return ip;
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
            return ip;
        }

        //================================================= Get From Plain DNS

        private struct Question
        {
            public string qName;
            public int qType;
            public int qClass;
        }

        private struct Answer
        {
            //public List<byte> aName;
            public int aType;
            public int aClass;
            public int aTtl;
            public int rdLength;
            public byte[] rData;
        }

        /// <summary>
        /// Get First IP in Answer Section
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="dnsIP">Plain DNS IP</param>
        /// <param name="dnsPort">Plain DNS Port</param>
        /// <param name="timeoutMS">Timeout (ms)</param>
        /// <returns>Returns string.Empty if fail</returns>
        public static string GetIpFromPlainDNS(string host, string dnsIP, int dnsPort, int timeoutMS)
        {
            List<string> ips = GetIpsFromPlainDNS(host, dnsIP, dnsPort, timeoutMS);
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
        public static List<string> GetIpsFromPlainDNS(string host, string dnsIP, int dnsPort, int timeoutMS)
        {
            List<string> ips = new();
            try
            {
                var task = Task.Run(() =>
                {
                    Socket sock = new(SocketType.Dgram, ProtocolType.Udp);

                    IPEndPoint ep = new(IPAddress.Parse(dnsIP), dnsPort);
                    sock.Connect(ep);

                    byte[] hostnameLength = new byte[1];
                    byte[] hostdomainLength = new byte[1];

                    byte[] tranactionID1 = { 0x46, 0x62 };
                    byte[] queryType1 = { 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    byte[] hostname = Encoding.Default.GetBytes(host.Split('.')[0]);
                    hostnameLength[0] = (byte)hostname.Length;
                    byte[] hostdomain = Encoding.Default.GetBytes(host.Split('.')[1]);
                    hostdomainLength[0] = (byte)hostdomain.Length;
                    byte[] queryEnd = { 0x00, 0x00, 0x01, 0x00, 0x01 };
                    byte[] dnsQueryString = tranactionID1.Concat(queryType1).Concat(hostnameLength).Concat(hostname).Concat(hostdomainLength).Concat(hostdomain).Concat(queryEnd).ToArray();

                    sock.Send(dnsQueryString);

                    byte[] rBuffer = new byte[1000];

                    int receivedLength = sock.Receive(rBuffer);

                    //ushort transId = (ushort)BitConverter.ToInt16(new[] { rBuffer[1], rBuffer[0] }, 0);
                    ushort queCount = (ushort)BitConverter.ToInt16(new[] { rBuffer[5], rBuffer[4] }, 0);
                    ushort ansCount = (ushort)BitConverter.ToInt16(new[] { rBuffer[7], rBuffer[6] }, 0);
                    //ushort authCount = (ushort)BitConverter.ToInt16(new[] { rBuffer[9], rBuffer[8] }, 0);
                    //ushort addCount = (ushort)BitConverter.ToInt16(new[] { rBuffer[11], rBuffer[10] }, 0);

                    // Header read, now on to handling questions
                    int byteCount = 12;

                    Question[] questions = new Question[queCount];

                    for (int i = 0; i < queCount; i++)
                    {
                        // Read Name
                        while (true)
                        {
                            int stringLength = rBuffer[byteCount];
                            byteCount++;

                            if (stringLength == 0)
                            {
                                if (questions[i].qName[^1] == '.')
                                {
                                    questions[i].qName = new string(questions[i].qName.Take(questions[i].qName.Length - 1).ToArray());
                                }

                                break;
                            }

                            byte[] tempName = new byte[stringLength];

                            for (int k = 0; k < stringLength; k++)
                            {
                                tempName[k] = rBuffer[byteCount];
                                byteCount++;
                            }

                            questions[i].qName += Encoding.ASCII.GetString(tempName) + '.';
                        }

                        // Name read now read Type
                        questions[i].qType = rBuffer[byteCount] + rBuffer[byteCount + 1];
                        byteCount += 2;

                        questions[i].qClass = rBuffer[byteCount] + rBuffer[byteCount + 1];
                        byteCount += 2;
                    }

                    Answer[] answers = new Answer[ansCount];

                    for (int i = 0; i < ansCount; i++)
                    {
                        // Skip reading Name, since it points to the Name given in question
                        byteCount += 2;

                        answers[i].aType = rBuffer[byteCount] + rBuffer[byteCount + 1];
                        byteCount += 2;

                        answers[i].aClass = rBuffer[byteCount] + rBuffer[byteCount + 1];
                        byteCount += 2;

                        answers[i].aTtl = BitConverter.ToInt32(rBuffer.Skip(byteCount).Take(4).Reverse().ToArray());
                        byteCount += 4;

                        answers[i].rdLength = BitConverter.ToInt16(rBuffer.Skip(byteCount).Take(2).Reverse().ToArray());
                        byteCount += 2;

                        answers[i].rData = rBuffer.Skip(byteCount).Take(answers[i].rdLength).ToArray();
                        byteCount += answers[i].rdLength;
                    }

                    foreach (var a in answers)
                    {
                        // the canonical name for an alias
                        if (a.aType == 5)
                        {
                            string namePortion = "";
                            for (int bytePosition = 0; bytePosition < a.rData.Length;)
                            {
                                int length = a.rData[bytePosition];
                                bytePosition++;

                                if (length == 0) continue;

                                namePortion += Encoding.ASCII.GetString(a.rData.Skip(bytePosition).Take(length).ToArray()) + ".";

                                bytePosition += length;
                            }

                            Debug.WriteLine(new string(namePortion.Take(namePortion.Length - 1).ToArray()));
                        }

                        // Skip any answer that's not IP adress since it's irrelevant for this excercise
                        if (a.aType == 1)
                        {
                            // First byte tells the lenghth of data (Usually length of 4 since type 1 describes IP4 adresses)
                            string ipString = "";

                            foreach (var b in a.rData.ToArray())
                            {
                                int number = b;

                                ipString += number + ".";
                            }

                            string ip = new(ipString.Take(ipString.Length - 1).ToArray());
                            ips.Add(ip);
                        }
                    }

                    sock.Close();
                });

                if (task.Wait(TimeSpan.FromMilliseconds(timeoutMS)))
                    return ips;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return ips;
        }




    }
}
