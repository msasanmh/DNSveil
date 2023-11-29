using ARSoft.Tools.Net.Dns;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient;

// Must be a DoH or DoT, not a Plain DNS
public class SmartDnsServer
{
    public bool IsRunning { get; private set; } = false;
    public int Port { get; set; } = 53;
    private DnsServer? DNSServer;

    public SmartDnsServer(int port)
    {
        Port = port;
    }

    public void Start()
    {
        UdpServerTransport udpServerTransport = new(new IPEndPoint(IPAddress.Any, Port));
        TcpServerTransport tcpServerTransport = new(new IPEndPoint(IPAddress.Any, Port));
        IServerTransport[] serverTransports = new IServerTransport[] { udpServerTransport, tcpServerTransport };
        if (DNSServer == null)
        {
            try
            {
                DNSServer = new(serverTransports);
                DNSServer.QueryReceived -= DnsServer_QueryReceived;
                DNSServer.QueryReceived += DnsServer_QueryReceived;
                DNSServer.Start();
                IsRunning = true;
            }
            catch (Exception)
            {
                try
                {
                    DNSServer = new(udpServerTransport);
                    DNSServer.QueryReceived -= DnsServer_QueryReceived;
                    DNSServer.QueryReceived += DnsServer_QueryReceived;
                    DNSServer.Start();
                    IsRunning = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    IsRunning = false;
                }
            }
        }
    }

    public void Stop()
    {
        if (DNSServer != null)
        {
            DNSServer.Stop();
            IsRunning = false;
        }
    }

    private async Task DnsServer_QueryReceived(object sender, QueryReceivedEventArgs eventArgs)
    {
        if (eventArgs.Query is not DnsMessage message) return;

        DnsMessage response = message.CreateResponseInstance();

        if ((message.Questions.Count == 1))
        {
            // send query to upstream server
            DnsQuestion question = message.Questions[0];
            DnsMessage? upstreamResponse = await DnsClient.Default.ResolveAsync(question.Name, question.RecordType, question.RecordClass);

            // if got an answer, copy it to the message sent to the client
            if (upstreamResponse != null)
            {
                // Adding Records
                foreach (DnsRecordBase record in (upstreamResponse.AnswerRecords))
                {
                    response.AnswerRecords.Add(record);
                    Debug.WriteLine("========> Record: " + record.Name);
                }
                foreach (DnsRecordBase record in (upstreamResponse.AdditionalRecords))
                {
                    response.AdditionalRecords.Add(record);
                }

                response.ReturnCode = ReturnCode.NoError;

                response.AnswerRecords.Clear();
                if (message.Questions[0].RecordType == RecordType.A)
                {
                    ARecord aRecord1 = new(message.Questions[0].Name, 60, IPAddress.Loopback);
                    response.AnswerRecords.Add(aRecord1);
                }
                if (message.Questions[0].RecordType == RecordType.Aaaa)
                {
                    ARecord aRecord1 = new(message.Questions[0].Name, 60, IPAddress.IPv6Loopback);
                    response.AnswerRecords.Add(aRecord1);
                }

                // set the response
                eventArgs.Response = response;
            }
        }
    }
}