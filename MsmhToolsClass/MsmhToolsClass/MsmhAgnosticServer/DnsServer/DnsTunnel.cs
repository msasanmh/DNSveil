using System.Diagnostics;
using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DnsTunnel
{
    public static async Task Process(AgnosticResult aResult, AgnosticProgram.Rules rulesProgram, AgnosticProgram.DnsLimit dnsLimitProgram, DnsCache dnsCaches, AgnosticSettings settings, EventHandler<EventArgs>? onRequestReceived)
    {
        DnsEnums.DnsProtocol dnsProtocol = aResult.Protocol switch
        {
            RequestProtocol.UDP => DnsEnums.DnsProtocol.UDP,
            RequestProtocol.TCP => DnsEnums.DnsProtocol.TCP,
            RequestProtocol.DoH => DnsEnums.DnsProtocol.DoH,
            _ => DnsEnums.DnsProtocol.Unknown
        };
        
        // Event
        string msgReqEvent = $"[{aResult.Local_EndPoint.Address}] [{dnsProtocol}] ";

        if (dnsProtocol == DnsEnums.DnsProtocol.Unknown)
        {
            msgReqEvent += "Request Denied - Unsupported Protocol";
            onRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
            return;
        }

        // Apply DnsLimit Program
        if (dnsLimitProgram.EnableDnsLimit)
        {
            AgnosticProgram.DnsLimit.DnsLimitResult dlr = dnsLimitProgram.Get(dnsProtocol, aResult.DoHPath);

            if (dlr.IsPlainDnsDisable)
            {
                msgReqEvent += "Request Denied - Protocol Is Blocked By DnsLimit Program";
                onRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                return;
            }

            if (!dlr.IsDoHPathAllowed)
            {
                msgReqEvent += $"Request Denied - DoH Path Is Blocked By DnsLimit Program (Path: {aResult.DoHPath})";
                onRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                return;
            }
        }

        // Create Request
        DnsMessage dmQ = DnsMessage.Read(aResult.FirstBuffer, dnsProtocol);
        if (dmQ.IsSuccess && dmQ.Header.QuestionsCount > 0 && dmQ.Questions.QuestionRecords.Count > 0)
        {
            DnsRequest dnsRequest = new(aResult.Socket, aResult.Ssl_Stream, aResult.Ssl_Kind, aResult.Local_EndPoint, aResult.Remote_EndPoint, aResult.FirstBuffer, dnsProtocol);
            string addressQ = dmQ.Questions.QuestionRecords[0].QNAME;
            DnsEnums.RRType typeQ = dmQ.Questions.QuestionRecords[0].QTYPE;

            msgReqEvent += $"Q: {addressQ}, A: ";

            bool isCached = dnsCaches.TryGet(dmQ, out DnsMessage dmR);
            bool usedCache = false;
            if (isCached)
            {
                if (dmR.IsSuccess)
                {
                    bool isTryWriteSuccess = DnsMessage.TryWrite(dmR, out byte[] responseCached);
                    //Debug.WriteLine("========== IsTryWriteSuccess: " + isTryWriteSuccess);
                    if (isTryWriteSuccess)
                    {
                        DnsMessage validate = DnsMessage.Read(responseCached, dnsRequest.Protocol);
                        if (validate.IsSuccess)
                        {
                            await dnsRequest.SendToAsync(responseCached).ConfigureAwait(false);
                            usedCache = true;
                        }
                    }
                }

                if (!usedCache)
                {
                    // TTL Expired - Remove From Cache
                    dnsCaches.TryRemove(dmQ);
                }
            }
            //Debug.WriteLine("========== Used Cache: " + usedCache);

            if (!usedCache)
            {
                // Apply DnsRules Program
                AgnosticProgram.Rules.RulesResult rr = new();
                if (rulesProgram.RulesMode != AgnosticProgram.Rules.Mode.Disable)
                {
                    rr = await rulesProgram.GetAsync(aResult.Local_EndPoint.Address.ToString(), addressQ, 0, settings).ConfigureAwait(false);
                }
                
                bool usedFakeOrCustom = false;
                if (rr.IsMatch)
                {
                    // Black List
                    if (rr.IsBlackList)
                    {
                        await dnsRequest.SendFailedResponseAsync().ConfigureAwait(false);
                        usedFakeOrCustom = true;

                        msgReqEvent += "Request Denied - Black List";
                        onRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                        return;
                    }

                    // If Custom Dns Couldn't Get An IP
                    if (string.IsNullOrEmpty(rr.Dns))
                    {
                        await dnsRequest.SendFailedResponseAsync().ConfigureAwait(false);
                        usedFakeOrCustom = true;

                        msgReqEvent += "Request Denied - Your Dns Rule Couldn't Get An IP!";
                        onRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
                        return;
                    }

                    // Fake DNS / Dns Domain / Custom Dns Or Smart DNS
                    bool isDnsIp = NetworkTool.IsIP(rr.Dns, out IPAddress? dnsIp);
                    if (isDnsIp && dnsIp != null)
                    {
                        bool isDnsIPv6 = NetworkTool.IsIPv6(dnsIp);
                        if (isDnsIPv6)
                        {
                            // IPv6
                            if (typeQ == DnsEnums.RRType.AAAA)
                            {
                                dmR = DnsMessage.CreateResponse(dmQ, 1, 0, 0);
                                dmR.Answers.AnswerRecords.Clear();
                                dmR.Answers.AnswerRecords.Add(new AaaaRecord(addressQ, 60, dnsIp));

                                bool isTryWriteSuccess = DnsMessage.TryWrite(dmR, out byte[] aBuffer);
                                if (isTryWriteSuccess)
                                {
                                    await dnsRequest.SendToAsync(aBuffer).ConfigureAwait(false);
                                    usedFakeOrCustom = true;
                                    bool cacheSuccess = dnsCaches.TryAdd(dmQ, dmR);
                                    Debug.WriteLine("Custom DNS IPv6 ADDED TO CACHE: " + cacheSuccess);
                                }
                            }
                        }
                        else
                        {
                            // IPv4
                            if (typeQ == DnsEnums.RRType.A)
                            {
                                dmR = DnsMessage.CreateResponse(dmQ, 1, 0, 0);
                                dmR.Answers.AnswerRecords.Clear();
                                dmR.Answers.AnswerRecords.Add(new ARecord(addressQ, 60, dnsIp));

                                bool isTryWriteSuccess = DnsMessage.TryWrite(dmR, out byte[] aBuffer);
                                if (isTryWriteSuccess)
                                {
                                    await dnsRequest.SendToAsync(aBuffer).ConfigureAwait(false);
                                    usedFakeOrCustom = true;
                                    bool cacheSuccess = dnsCaches.TryAdd(dmQ, dmR);
                                    Debug.WriteLine("Custom DNS IPv4 ADDED TO CACHE: " + cacheSuccess);
                                }
                            }
                        }
                    }
                }

                if (!usedFakeOrCustom && settings.DNSs.Count > 0)
                {
                    byte[] response = await DnsClient.QueryAsync(dnsRequest.Buffer, dnsRequest.Protocol, settings).ConfigureAwait(false);
                    dmR = DnsMessage.Read(response, dnsRequest.Protocol);
                    if (dmR.IsSuccess)
                    {
                        bool wasCfIP = false;
                        if (!string.IsNullOrWhiteSpace(settings.CloudflareCleanIP))
                        {
                            if (NetworkTool.IsIP(settings.CloudflareCleanIP, out IPAddress? cfIP) && cfIP != null)
                            {
                                if (IsCfIP(dmR))
                                {
                                    // Return CF IP
                                    bool isCfIPv6 = NetworkTool.IsIPv6(cfIP);
                                    if (isCfIPv6)
                                    {
                                        // IPv6
                                        if (typeQ == DnsEnums.RRType.AAAA)
                                        {
                                            dmR = DnsMessage.CreateResponse(dmQ, 1, 0, 0);
                                            dmR.Answers.AnswerRecords.Clear();
                                            dmR.Answers.AnswerRecords.Add(new AaaaRecord(addressQ, 60, cfIP));

                                            bool isTryWriteSuccess = DnsMessage.TryWrite(dmR, out byte[] aBuffer);
                                            if (isTryWriteSuccess)
                                            {
                                                await dnsRequest.SendToAsync(aBuffer).ConfigureAwait(false);
                                                wasCfIP = true;
                                                bool cacheSuccess = dnsCaches.TryAdd(dmQ, dmR);
                                                Debug.WriteLine("CF IPv6 ADDED TO CACHE: " + cacheSuccess);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // IPv4
                                        if (typeQ == DnsEnums.RRType.A)
                                        {
                                            dmR = DnsMessage.CreateResponse(dmQ, 1, 0, 0);
                                            dmR.Answers.AnswerRecords.Clear();
                                            dmR.Answers.AnswerRecords.Add(new ARecord(addressQ, 60, cfIP));

                                            bool isTryWriteSuccess = DnsMessage.TryWrite(dmR, out byte[] aBuffer);
                                            if (isTryWriteSuccess)
                                            {
                                                await dnsRequest.SendToAsync(aBuffer).ConfigureAwait(false);
                                                wasCfIP = true;
                                                bool cacheSuccess = dnsCaches.TryAdd(dmQ, dmR);
                                                Debug.WriteLine("CF IPv4 ADDED TO CACHE: " + cacheSuccess);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (!wasCfIP)
                        {
                            await dnsRequest.SendToAsync(response).ConfigureAwait(false);
                            bool cacheSuccess = dnsCaches.TryAdd(dmQ, dmR);
                            Debug.WriteLine("ADDED TO CACHE: " + cacheSuccess);
                        }
                    }
                    else
                    {
                        await dnsRequest.SendFailedResponseAsync().ConfigureAwait(false);
                    }
                }
            }

            // Event
            List<string> answers = new();
            if (dmR.IsSuccess)
            {
                foreach (IResourceRecord answer in dmR.Answers.AnswerRecords)
                {
                    if (answer is ARecord aRecord) answers.Add(aRecord.IP.ToString());
                    else if (answer is AaaaRecord aaaaRecord) answers.Add(aaaaRecord.IP.ToString());
                    //else if (answer is CNameRecord cNameRecord) answers.Add(cNameRecord.CName);
                    else
                    {
                        string a = answer.TYPE.ToString();
                        if (!answers.IsContain(a))
                        {
                            answers.Add(answer.CLASS.ToString());
                            answers.Add(answer.TimeToLive.ToString());
                            answers.Add(a);
                        }
                    }
                }
            }

            if (answers.Count != 0)
            {
                // Show Only 5 Records
                if (answers.Count > 5) answers = answers.Take(5).ToList();
                msgReqEvent += answers.ToString(", ");
                onRequestReceived?.Invoke(msgReqEvent, EventArgs.Empty);
            }

            //Debug.WriteLine(dmR.ToString());
        }
    }

    private static bool IsCfIP(DnsMessage dmR)
    {
        bool result = false;

        try
        {
            for (int n = 0; n < dmR.Answers.AnswerRecords.Count; n++)
            {
                IResourceRecord rr = dmR.Answers.AnswerRecords[n];
                if (rr is ARecord aRecord)
                {
                    if (CommonTools.IsCfIP(aRecord.IP))
                    {
                        result = true;
                        break;
                    }
                }
                else if (rr is AaaaRecord aaaaRecord)
                {
                    if (CommonTools.IsCfIP(aaaaRecord.IP))
                    {
                        result = true;
                        break;
                    }
                }
            }
        }
        catch (Exception) { }

        return result;
    }

}