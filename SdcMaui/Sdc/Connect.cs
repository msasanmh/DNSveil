using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Ae.Dns.Server;
using Android.App;
using AndroidX.Core.App;
using Microsoft.Maui.Controls.Compatibility;
using MsmhToolsClass.DnsTool;
using MsmhToolsClass.MsmhProxyServer;
using MsmhToolsClass.ProxyServerPrograms;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using SdcMaui.Vpn;
using System.Diagnostics;
using System.Net;
using Application = Microsoft.Maui.Controls.Application;

namespace SdcMaui
{
    public partial class MainPage
    {
        public async void StartConnect()
        {
            try
            {
                if (!IsDnsServerRunning && !IsConnecting)
                {
                    // Connect
                    if (IsConnecting) return;
                    IsConnecting = true;

                    // Connecting Message
                    Log("Connecting...");

                    // Renew DNS Server Cancel Token
                    CancelTokenDnsServer = new();

                    // Stop Check
                    if (IsCheckingStarted)
                    {
                        StartCheck();

                        // Wait until check is done
                        await Task.Run(async () =>
                        {
                            while (true)
                            {
                                if (!IsCheckingStarted) break;
                                await Task.Delay(100);
                            }
                        });
                    }

                    ConnectToWorkingServers();

                    // Wait for Connect
                    Task waitDnsServer = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (IsDnsConnected) break;
                            await Task.Delay(500);
                        }
                    });
                    try { await waitDnsServer.WaitAsync(TimeSpan.FromSeconds(15)); } catch (Exception) { }

                    // Check Local DNS Server Started
                    if (!IsDnsConnected)
                    {
                        disconnect();
                        Log("Couldn't Start Local DNS Server.", Colors.IndianRed);
                    }
                    else
                    {
                        Log("Local DNS Server Started.", Colors.MediumSeaGreen);

                        // Start Proxy
                        StartProxy();

                        // Wait for Proxy
                        Task waitProxy = Task.Run(async () =>
                        {
                            while (true)
                            {
                                if (ProxyServer.IsRunning) break;
                                await Task.Delay(500);
                            }
                        });
                        try { await waitProxy.WaitAsync(TimeSpan.FromSeconds(10)); } catch (Exception) { }

                        // Check Proxy Server Started
                        if (!ProxyServer.IsRunning)
                        {
                            disconnect();
                            Log("Couldn't Start Proxy Server.", Colors.IndianRed);
                        }
                        else
                        {
                            Log("DNS and Proxy Servers are Running.", Colors.MediumSeaGreen);

                            
                            //SdcVpnService.StartVPN();
                        }
                    }

                    IsConnecting = false;
                    Debug.WriteLine("END OF CONNECT");
                }
                else
                {
                    // Disconnect
                    if (IsDisconnecting) return;
                    IsDisconnecting = true;

                    // Write Disconnecting message to log
                    Log("Disconnecting...");

                    // Wait for Disconnect
                    Task wait = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (!IsConnecting && !IsDnsConnected) break;
                            disconnect();
                            await Task.Delay(500);
                        }
                    });
                    try { await wait.WaitAsync(TimeSpan.FromSeconds(30)); } catch (Exception) { }

                    // Write Disconnect message to log
                    if (IsConnecting || IsDnsConnected)
                        Log("Couldn't Disconnect", Colors.IndianRed);
                    else
                    {
                        Log("Disconnected.", Colors.MediumSeaGreen);

                        //SdcVpnService.StopVPN();
                    }

                    IsDisconnecting = false;
                }

                void disconnect()
                {
                    try
                    {
                        // Stop Proxy
                        if (ProxyServer.IsRunning) ProxyServer.Stop();
                        if (ProxyServer.IsDpiBypassActive)
                        {
                            ProxyProgram.DPIBypass bypass = new();
                            bypass.Set(ProxyProgram.DPIBypass.Mode.Disable, 1, DpiChunkMode, 1, 0, 0);
                            ProxyServer.EnableStaticDPIBypass(bypass);
                        }

                        // Dispose DnsClients
                        if (DnsClients.Any())
                        {
                            foreach (IDnsClient dnsClient in DnsClients)
                                dnsClient?.Dispose();
                        }

                        // Stop DNS Server
                        CancelTokenDnsServer?.Cancel();
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.GetBaseException().ToString(), Colors.IndianRed);
#if ANDROID
                Task? da = Application.Current?.MainPage?.DisplayAlert(ex.Message, ex.GetBaseException().ToString(), "Accept", "Cancel");
                if (da != null) await da;
#endif
            }
        }

        public async void ConnectToWorkingServers()
        {
            try
            {
                if (!WorkingDnsList.Any())
                {
                    Log("There is no working server.", Colors.IndianRed);
                    return;
                }

                // Sort by Latency
                WorkingDnsList = WorkingDnsList.OrderBy(x => x.Item1).ToList();

                Array.Resize(ref DnsClients, WorkingDnsList.Count);
                int count = 0;

                for (int n = 0; n < WorkingDnsList.Count; n++)
                {
                    string dns = WorkingDnsList[n].Item2;
                    Debug.WriteLine(WorkingDnsList[n].Item1);
                    DnsReader dnsReader = new(dns, null);

                    try
                    {
                        if (!string.IsNullOrEmpty(dnsReader.IP) && dnsReader.Protocol == DnsReader.DnsProtocol.PlainDNS)
                        {
                            DnsClients[n] = new DnsUdpClient(new IPEndPoint(IPAddress.Parse(dnsReader.IP), dnsReader.Port));
                            count++;
                        }

                        if (!string.IsNullOrEmpty(dnsReader.IP) && dnsReader.Protocol == DnsReader.DnsProtocol.DoH)
                        {
                            string doh = dnsReader.Dns;
                            if (dnsReader.IsDnsCryptStamp)
                                doh = $"https://{dnsReader.Host}:{dnsReader.Port}{dnsReader.Path}";
                            HttpClient httpClient = new();
                            httpClient.BaseAddress = new Uri(doh);
                            DnsClients[n] = new DnsHttpClient(httpClient);
                            count++;
                        }
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

                    if (count >= MaxServersToConnect) break;
                }

                // Remove blank values from array
                DnsClients = DnsClients.Where(x => x != null).ToArray();

                await ConnectToDnsClients();
            }
            catch (Exception ex)
            {
                Log(ex.GetBaseException().ToString(), Colors.IndianRed);
#if ANDROID
                Task? da = Application.Current?.MainPage?.DisplayAlert(ex.Message, ex.GetBaseException().ToString(), "Accept", "Cancel");
                if (da != null) await da;
#endif
            }
        }

        public async Task ConnectToDnsClients()
        {
            try
            {
                // Create DNS Server
                DnsUdpServerOptions udpServerOptions = new();
                udpServerOptions.Endpoint = new IPEndPoint(IPAddress.Any, DnsPort);

                //DnsTcpServerOptions tcpServerOptions = new();
                //tcpServerOptions.Endpoint = new IPEndPoint(IPAddress.Any, DnsPort);

                IDnsRawClient dnsRawClient;
                if (DnsClients.Length > 1)
                {
                    IDnsClient dnsRacerClient = new DnsRacerClient(DnsClients);
                    dnsRawClient = new DnsRawClient(dnsRacerClient);
                }
                else
                {
                    dnsRawClient = new DnsRawClient(DnsClients[0]);
                }

                DnsUdpServer = new DnsUdpServer(dnsRawClient, udpServerOptions);
                //DnsTcpServer = new DnsTcpServer(dnsRawClient, tcpServerOptions);

                ConnectedDnsPort = DnsPort;

                // Start DNS Server
                if (CancelTokenDnsServer != null)
                {
                    await DnsUdpServer.Listen(CancelTokenDnsServer.Token);
                    //Task tcp = DnsTcpServer.Listen(CancelTokenDnsServer.Token);

                    //try
                    //{

                    //    await Task.WhenAll(udp, tcp);
                    //}
                    //catch (Exception e)
                    //{
                    //    Log(e.Message, Colors.IndianRed);
                    //}
                }
            }
            catch (Exception ex)
            {
                Log(ex.GetBaseException().ToString(), Colors.IndianRed);
#if ANDROID
                Task? da = Application.Current?.MainPage?.DisplayAlert(ex.Message, ex.GetBaseException().ToString(), "Accept", "Cancel");
                if (da != null) await da;
#endif
            }
        }
    }
}
