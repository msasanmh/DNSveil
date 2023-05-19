using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using System.Net;

namespace SecureDNSClient
{
    public class CamouflageDNSServer
    {
        public bool IsRunning { get; private set; } = false;
        public int Port { get; set; } = 5380;
        private DnsServer? DNSServer;

        public CamouflageDNSServer(int port)
        {
            Port = port;
        }

        public void Start()
        {
            UdpServerTransport udpServerTransport = new(new IPEndPoint(IPAddress.Any, Port));
            if (DNSServer == null)
            {
                DNSServer = new(udpServerTransport);
                DNSServer.QueryReceived += DnsServer_QueryReceived;
                DNSServer.Start();
                IsRunning = true;
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
            if (eventArgs.Query is not DnsMessage message)
                return;

            DnsMessage response = message.CreateResponseInstance();

            if ((message.Questions.Count == 1))
            {
                // send query to upstream server
                DnsQuestion question = message.Questions[0];
                DnsMessage? upstreamResponse = await DnsClient.Default.ResolveAsync(question.Name, question.RecordType, question.RecordClass);

                // if got an answer, copy it to the message sent to the client
                if (upstreamResponse != null)
                {
                    foreach (DnsRecordBase record in (upstreamResponse.AnswerRecords))
                    {
                        response.AnswerRecords.Add(record);
                    }
                    foreach (DnsRecordBase record in (upstreamResponse.AdditionalRecords))
                    {
                        response.AdditionalRecords.Add(record);
                    }

                    response.ReturnCode = ReturnCode.NoError;

                    if (message.Questions[0].Name.Equals(DomainName.Parse("dns.cloudflare.com")))
                    {
                        response.AnswerRecords.Clear();
                        if (message.Questions[0].RecordType == RecordType.A)
                        {
                            ARecord aRecord1 = new(DomainName.Parse("dns.cloudflare.com"), 60, IPAddress.Parse("104.16.132.229"));
                            ARecord aRecord2 = new(DomainName.Parse("dns.cloudflare.com"), 60, IPAddress.Parse("104.16.133.229"));
                            response.AnswerRecords.Add(aRecord1);
                            response.AnswerRecords.Add(aRecord2);
                        }
                    }

                    if (message.Questions[0].Name.Equals(DomainName.Parse("dns.cloudflare-dns.com")))
                    {
                        response.AnswerRecords.Clear();

                        if (message.Questions[0].RecordType == RecordType.A)
                        {
                            ARecord aRecord1 = new(DomainName.Parse("dns.cloudflare-dns.com"), 60, IPAddress.Parse("104.18.106.66"));
                            ARecord aRecord2 = new(DomainName.Parse("dns.cloudflare-dns.com"), 60, IPAddress.Parse("104.18.107.66"));
                            response.AnswerRecords.Add(aRecord1);
                            response.AnswerRecords.Add(aRecord2);
                        }

                        if (message.Questions[0].RecordType == RecordType.CName)
                        {
                            CNameRecord cNameRecord = new(DomainName.Parse("dns.cloudflare-dns.com"), 60, DomainName.Parse("every1dns.com"));
                            response.AnswerRecords.Add(cNameRecord);
                        }
                    }

                    if (message.Questions[0].Name.Equals(DomainName.Parse("every1dns.com")))
                    {
                        response.AnswerRecords.Clear();

                        if (message.Questions[0].RecordType == RecordType.A)
                        {
                            ARecord aRecord1 = new(DomainName.Parse("every1dns.com"), 60, IPAddress.Parse("104.18.106.66"));
                            ARecord aRecord2 = new(DomainName.Parse("every1dns.com"), 60, IPAddress.Parse("104.18.107.66"));
                            response.AnswerRecords.Add(aRecord1);
                            response.AnswerRecords.Add(aRecord2);
                        }
                    }

                    // set the response
                    eventArgs.Response = response;
                }
            }
        }
    }
}
