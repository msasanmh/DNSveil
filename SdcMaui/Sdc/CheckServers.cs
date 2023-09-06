using Ae.Dns.Client;
using Ae.Dns.Protocol;
using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using MsmhToolsClass.HTTPProxyServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace SdcMaui
{
    public partial class MainPage
    {
        public async void StartCheck()
        {
            if (!IsCheckingStarted)
            {
                // Start Checking
                IsCheckingStarted = true;

                try
                {
                    Task taskCheck = Task.Run(async () => await CheckServers());

                    await taskCheck.ContinueWith(_ =>
                    {
                        WorkingDnsList = WorkingDnsList.RemoveDuplicates();

                        string msg = $"Check Task: {taskCheck.Status}";
                        Log(msg);

                        IsCheckingStarted = false;
                        StopChecking = false;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    Log("Task Check: " + ex.Message);
                }
            }
            else
            {
                // Stop Checking
                StopChecking = true;
            }
        }

        public async Task CheckServers()
        {
            try
            {
                string? servers = await ResourceTool.GetResourceTextFileAsync("SdcMaui.SdcDatabase.DNS-Servers.txt", Assembly.GetExecutingAssembly());
                if (string.IsNullOrEmpty(servers)) return;

                string? companyData = await ResourceTool.GetResourceTextFileAsync("SdcMaui.SdcDatabase.HostToCompany.txt", Assembly.GetExecutingAssembly());

                List<string> serversList = servers.SplitToLines();

                WorkingDnsList.Clear();

                int splitTo = 5;
                int count = 0;
                var lists = serversList.SplitToLists(splitTo);

                for (int a = 0; a < lists.Count; a++)
                {
                    count += splitTo;
                    count = Math.Min(count, serversList.Count);
                    Dispatcher.DispatchIt(() => BtnCheck.Text = $"Stop ({count} of {serversList.Count})");
                    List<string> list = lists[a];
                    await Parallel.ForEachAsync(list, async (dns, list) =>
                    {
                        if (StopChecking) return;
                        dns = dns.Trim();
                        if (!string.IsNullOrEmpty(dns))
                        {
                            CheckDnsResult cdr = await CheckDns(dns, CheckTimeoutMS, companyData);

                            // Debug
                            Debug.WriteLine(cdr.IsOnline + " " + cdr.Dns);

                            if (cdr.IsOnline)
                                WorkingDnsList.Add(new Tuple<int, string>(cdr.Latency, cdr.Dns));

                            string msg = $"Checking Servers...{NL}";
                            msg += $"Address: {cdr.Dns}{NL}";
                            string status = cdr.IsOnline ? "Online" : "Offline";
                            msg += $"Status: {status}{NL}";
                            msg += cdr.IsOnline ? $"Latency: {cdr.Latency} ms." : cdr.Reason;

                            Log(msg);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Log(ex.GetBaseException().ToString(), Microsoft.Maui.Graphics.Colors.IndianRed);
#if ANDROID
                Task? da = Application.Current?.MainPage?.DisplayAlert(ex.Message, ex.GetBaseException().ToString(), "Accept", "Cancel");
                if (da != null) await da;
#endif
            }

            //for (int n = 0; n < serversList.Count; n++)
            //{
            //    if (StopChecking) break;
            //    string dns = serversList[n].Trim();
            //    if (string.IsNullOrEmpty(dns)) continue;

            //    CheckDnsResult cdr = await CheckDns(dns, CheckTimeoutMS, companyData);

            //    // Debug
            //    Debug.WriteLine(cdr.Dns);
            //    Debug.WriteLine(cdr.IsOnline);

            //    if (cdr.IsOnline)
            //        WorkingDnsList.Add(new Tuple<int, string>(cdr.Latency, cdr.Dns));

            //    string msg = $"Checking Servers...{NL}";
            //    msg += $"Address: {cdr.Dns}{NL}";
            //    string status = cdr.IsOnline ? "Online" : "Offline";
            //    msg += $"Status: {status}{NL}";
            //    msg += cdr.IsOnline ? $"Latency: {cdr.Latency} ms." : cdr.Reason;

            //    Log(msg);
            //}
        }

        public class CheckDnsResult
        {
            public string Dns { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public bool IsOnline { get; set; } = false;
            public int Latency { get; set; } = -1;
            public string Reason { get; set; } = string.Empty;
        }

        public async Task<CheckDnsResult> CheckDns(string dns, int checkTimeoutMS, string? companyData)
        {
            CheckDnsResult checkDnsResult = new();
            checkDnsResult.Dns = dns;

            DnsReader dnsReader = new(dns, companyData);

            checkDnsResult.CompanyName = dnsReader.CompanyName;

            IDnsClient? dnsClient = null;
            HttpClient? httpClient = null;
            Stopwatch stopwatch = new();
            DnsMessage? answer = null;

            try
            {
                if (dnsReader.Protocol == DnsReader.DnsProtocol.PlainDNS)
                {
                    // Plain DNS UDP
                    DnsUdpClientOptions dnsUdpClientOptions = new();
                    if (!string.IsNullOrEmpty(dnsReader.IP))
                    {
                        dnsUdpClientOptions.Endpoint = new IPEndPoint(IPAddress.Parse(dnsReader.IP), dnsReader.Port);
                        dnsClient = new DnsUdpClient(dnsUdpClientOptions);
                    }
                    else
                    {
                        checkDnsResult.Reason = "IP Address is empty";
                        return checkDnsResult;
                    }
                }
                else if (dnsReader.Protocol == DnsReader.DnsProtocol.DoH)
                {
                    string dnsAddr = dnsReader.Dns;
                    if (dnsReader.IsDnsCryptStamp)
                        dnsAddr = $"https://{dnsReader.Host}:{dnsReader.Port}{dnsReader.Path}";

                    httpClient = new();
                    httpClient.BaseAddress = new Uri(dnsAddr);

                    dnsClient = new DnsHttpClient(httpClient);
                }
                else
                    checkDnsResult.Reason = $"Protocol {dnsReader.ProtocolName} is not supported.";

                if (dnsClient == null) return checkDnsResult;

                try
                {
                    Task task = Task.Run(async () =>
                    {
                        stopwatch.Start();
                        answer = await dnsClient.Query(DnsQueryFactory.CreateQuery(DomainToCheck));
                        stopwatch.Stop();
                    });
                    await task.WaitAsync(TimeSpan.FromMilliseconds(checkTimeoutMS));
                }
                catch (Exception)
                {
                    // Plain DNS TCP
                    if (dnsReader.Protocol == DnsReader.DnsProtocol.PlainDNS)
                    {
                        DnsTcpClientOptions dnsTcpClientOptions = new();
                        if (!string.IsNullOrEmpty(dnsReader.IP))
                        {
                            dnsTcpClientOptions.Endpoint = new IPEndPoint(IPAddress.Parse(dnsReader.IP), dnsReader.Port);
                            dnsClient = new DnsTcpClient(dnsTcpClientOptions);

                            try
                            {
                                Task task = Task.Run(async () =>
                                {
                                    stopwatch.Restart();
                                    answer = await dnsClient.Query(DnsQueryFactory.CreateQuery(DomainToCheck));
                                    stopwatch.Stop();
                                });
                                await task.WaitAsync(TimeSpan.FromMilliseconds(checkTimeoutMS));
                            }
                            catch (Exception)
                            {
                                // do nothing
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.GetBaseException().ToString(), Microsoft.Maui.Graphics.Colors.IndianRed);
#if ANDROID
                Task? da = Application.Current?.MainPage?.DisplayAlert(ex.Message, ex.GetBaseException().ToString(), "Accept", "Cancel");
                if (da != null) await da;
#endif
            }

            if (answer != null && answer.Answers.Any())
            {
                checkDnsResult.IsOnline = true;
                checkDnsResult.Latency = Convert.ToInt32(stopwatch.ElapsedMilliseconds);
                checkDnsResult.Reason = "Success.";
            }
            else
            {
                checkDnsResult.Reason = "Timeout.";
            }

            dnsClient?.Dispose();
            httpClient?.Dispose();
            return checkDnsResult;
        }
    }
}
