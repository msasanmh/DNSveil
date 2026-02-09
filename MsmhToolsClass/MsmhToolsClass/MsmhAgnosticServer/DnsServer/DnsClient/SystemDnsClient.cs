using System.Net;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class SystemDnsClient
{
    private byte[] QueryBuffer { get; set; } = Array.Empty<byte>();
    private int TimeoutMS { get; set; } = 5;
    private CancellationToken CT { get; set; }
    private static DnsEnums.DnsProtocol Protocol { get; set; } = DnsEnums.DnsProtocol.UDP;

    public SystemDnsClient(byte[] queryBuffer, int timeoutMS, CancellationToken cT)
    {
        QueryBuffer = queryBuffer;
        TimeoutMS = timeoutMS;
        CT = cT;
    }

    public async Task<byte[]> GetResponseAsync()
    {
        byte[] result = Array.Empty<byte>();

        Task task = Task.Run(() =>
        {
            try
            {
                DnsMessage dmQ = DnsMessage.Read(QueryBuffer, Protocol);
                if (dmQ.IsSuccess && dmQ.Header.QuestionsCount > 0)
                {
                    string host = dmQ.Questions.QuestionRecords[0].QNAME;
                    DnsEnums.RRType typeQ = dmQ.Questions.QuestionRecords[0].QTYPE;
                    bool getIpv6 = typeQ == DnsEnums.RRType.AAAA;
                    List<IPAddress> ips = GetIP.GetIpsFromSystem(host, getIpv6);
                    
                    if (ips.Count != 0)
                    {
                        DnsMessage dmR = DnsMessage.CreateResponse(dmQ, (ushort)ips.Count, 0, 0);
                        if (dmR.IsSuccess)
                        {
                            if (getIpv6) foreach (IPAddress ip in ips) dmR.Answers.AnswerRecords.Add(new AaaaRecord(host, 30, ip));
                            else foreach (IPAddress ip in ips) dmR.Answers.AnswerRecords.Add(new ARecord(host, 30, ip));

                            bool isWriteSuccess = DnsMessage.TryWrite(dmR, out byte[] aBuffer);
                            if (isWriteSuccess) result = aBuffer;
                        }
                    }
                }
            }
            catch (Exception) { }
        });
        try { await task.WaitAsync(TimeSpan.FromMilliseconds(TimeoutMS), CT).ConfigureAwait(false); } catch (Exception) { }

        return result;
    }
}