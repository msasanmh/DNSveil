using System.Collections.Concurrent;
using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class DnsCache
{
    private readonly ConcurrentDictionary<string, DnsMessage> Caches = new();

    public bool TryGet(DnsMessage dmQ, out DnsMessage dmR)
    {
        dmR = new DnsMessage();
        bool success = false;

        try
        {
            success = Caches.TryGetValue(dmQ.Questions.ToString(), out DnsMessage? dmROut);
            if (success && dmROut != null)
            {
                dmR = CreateFromCache(dmQ, dmROut);
            };
        }
        catch (Exception) { }

        return success;
    }

    public bool TryRemove(DnsMessage dmQ)
    {
        try
        {
            return Caches.TryRemove(dmQ.Questions.ToString(), out _);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool TryAdd(DnsMessage dmQ, DnsMessage dmR)
    {
        try
        {
            bool canCache = CanCache(dmR);
            //Debug.WriteLine("CAN CACHE: " + canCache);
            if (canCache) return Caches.TryAdd(dmQ.Questions.ToString(), AddTTL(dmR));
            else return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public int CachedRequests
    {
        get
        {
            try
            {
                return Caches.ToList().Count;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }

    public void Flush()
    {
        try
        {
            Caches.Clear();
        }
        catch (Exception) { }
    }

    private static DnsMessage AddTTL(DnsMessage dmR)
    {
        try
        {
            // Only For DNS Messages With TTL < 3 (It's A Fix For Smart DNS Servers Which Sends RR Records With TTL 0)
            uint ttl = 60;
            modifyTTL(dmR.Answers.AnswerRecords);
            modifyTTL(dmR.Authorities.AuthorityRecords);
            modifyTTL(dmR.Additionals.AdditionalRecords);
            return dmR;

            void modifyTTL(List<IResourceRecord> rrs)
            {
                try
                {
                    foreach (ResourceRecord rr in rrs.Cast<ResourceRecord>())
                    {
                        if (rr.TimeToLive < 3)
                        {
                            rr.TimeToLive = ttl;
                            rr.TTLDateTime = DateTime.UtcNow;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DnsMessage AddTTL modifyTTL: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsMessage AddTTL: " + ex.Message);
            return dmR;
        }
    }

    private static DnsMessage CreateFromCache(DnsMessage dmQ, DnsMessage dmR)
    {
        try
        {
            dmR.DnsProtocol = dmQ.DnsProtocol;
            dmR.Header.ID = dmQ.Header.ID;
            modifyTTL(dmR.Answers.AnswerRecords);
            modifyTTL(dmR.Authorities.AuthorityRecords);
            modifyTTL(dmR.Additionals.AdditionalRecords);
            return dmR;

            void modifyTTL(List<IResourceRecord> rrs)
            {
                try
                {
                    uint newTTL = 0;
                    foreach (ResourceRecord rr in rrs.Cast<ResourceRecord>())
                    {
                        try
                        {
                            //Debug.WriteLine("-=-==-= " + rr.TTLDateTime);
                            //Debug.WriteLine("-=-==-= " + rr.TimeToLive);

                            if (newTTL != 0)
                            {
                                rr.TTLDateTime = DateTime.UtcNow;
                                rr.TimeToLive = newTTL;
                            }

                            if (newTTL == 0 && rr.TimeToLive > 0)
                            {
                                double ttlDifferDouble = (DateTime.UtcNow - rr.TTLDateTime).TotalSeconds;
                                //Debug.WriteLine("----------- " + ttlDifferDouble);
                                uint ttlDiffer = ttlDifferDouble > 0 ? Convert.ToUInt32(ttlDifferDouble) : 0;
                                if (rr.TimeToLive > ttlDiffer)
                                {
                                    rr.TimeToLive -= ttlDiffer;
                                    rr.TTLDateTime = DateTime.UtcNow;
                                    //newTTL = rr.TimeToLive; // Sometimes TTLs Are Different
                                }
                                else
                                {
                                    rr.TimeToLive = 0;
                                    rr.TTLDateTime = DateTime.UtcNow;
                                }
                            }

                            if (rr.TimeToLive <= 0) dmR.IsSuccess = false;

                            //Debug.WriteLine("-=-==-= " + rr.TTLDateTime);
                            //Debug.WriteLine("-=-==-= " + rr.TimeToLive);
                        }
                        catch (Exception)
                        {
                            rr.TimeToLive = 0;
                            rr.TTLDateTime = DateTime.UtcNow;
                            dmR.IsSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DnsMessage CreateFromCache modifyTTL: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsMessage CreateFromCache: " + ex.Message);
            return dmR;
        }
    }

    private static bool CanCache(DnsMessage dmR)
    {
        try
        {
            // https://datatracker.ietf.org/doc/html/rfc1035#section-7.4
            List<IResourceRecord> rrs = new();
            rrs.AddRange(dmR.Answers.AnswerRecords);
            rrs.AddRange(dmR.Authorities.AuthorityRecords);
            rrs.AddRange(dmR.Additionals.AdditionalRecords);

            bool c1 = dmR.Header.QuestionsCount > 0;
            //bool c2 = dmR.Header.TC != Enums.TC.Truncated; // DnsProxy By Adguard Doesn't Following This Rule!!
            bool c3 = dmR.Header.OperationalCode != DnsEnums.OperationalCode.IQUERY;

            bool c4 = true;
            foreach (Question question in dmR.Questions.QuestionRecords)
            {
                if (question.QNAME.Contains('*'))
                {
                    c4 = false;
                    break;
                }
            }

            bool c5 = false;
            foreach (Question question in dmR.Questions.QuestionRecords)
            {
                foreach (IResourceRecord rr in rrs)
                {
                    if (question.QTYPE == rr.TYPE || question.QTYPE == DnsEnums.RRType.ANY)
                    {
                        c5 = true;
                        break;
                    }
                }
                if (c5) break;
            }

            bool c6 = dmR.IsSuccess && dmR.Header.IsSuccess && dmR.Questions.IsSuccess;
            bool c7 = dmR.Header.AnswersCount > 0 || dmR.Header.AuthoritiesCount > 0 || dmR.Header.AdditionalsCount > 0;

            bool c8 = true;
            foreach (IResourceRecord rr in rrs)
            {
                if (rr is ARecord aRecord)
                {
                    if (aRecord.IP.ToString().StartsWith("10."))
                    {
                        c8 = false;
                        break;
                    }
                }
            }

            return c1 && c3 && c4 && c5 && c6 && c7 && c8;
        }
        catch (Exception)
        {
            return false;
        }
    }
}