using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Microsoft.Maui.Devices.Sensors;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SdcMaui
{
    public partial class MainPage
    {
        public async void Updater()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(500);
                    Dispatcher.Dispatch(async () =>
                    {
                        // Fix Word wrap Bug
                        labelStatus.WidthRequest = Width;

                        // Update bool Is DNS Server Running
                        IsDnsServerRunning = CancelTokenDnsServer != null && !CancelTokenDnsServer.IsCancellationRequested;

                        // Update Working Servers
                        labelStatusWorkingServers.TextColor = Colors.DodgerBlue;
                        labelStatusWorkingServers.Text = WorkingDnsList.Count.ToString();
                        
                        // Update Local DNS Status
                        if (IsDnsConnected)
                        {
                            labelStatusLocalDns.TextColor = Colors.MediumSeaGreen;
                            labelStatusLocalDns.Text = "Online";
                            labelStatusLocalDnsLatency.TextColor = Colors.MediumSeaGreen;
                            labelStatusLocalDnsLatency.Text = $"{LocalDnsLatency} ms";
                            gridStatus.BackgroundColor = Colors.MediumSeaGreen;
                        }
                        else
                        {
                            labelStatusLocalDns.TextColor = Colors.IndianRed;
                            labelStatusLocalDns.Text = "Offline";
                            labelStatusLocalDnsLatency.TextColor = Colors.IndianRed;
                            labelStatusLocalDnsLatency.Text = "-1";
                            gridStatus.BackgroundColor = Colors.IndianRed;
                        }

                        // Update Proxy Status
                        if (IsProxyActive)
                        {
                            labelStatusProxy.TextColor = Colors.MediumSeaGreen;
                            labelStatusProxy.Text = "Active";
                        }
                        else
                        {
                            labelStatusProxy.TextColor = Colors.IndianRed;
                            labelStatusProxy.Text = "Inactive";
                        }

                        labelStatusProxyRequests.TextColor = Colors.DodgerBlue;
                        labelStatusProxyRequests.Text = $"{ProxyRequests} of {ProxyMaxRequests}";

                        labelStatusProxyDpiBypass.TextColor = IsProxyDpiBypassActive ? Colors.MediumSeaGreen : Colors.IndianRed;
                        labelStatusProxyDpiBypass.Text = IsProxyDpiBypassActive ? "Active" : "Inactive";

                        // Check Button Text
                        if (StopChecking)
                        {
                            BtnCheck.Text = "Stopping...";
                            BtnCheck.IsEnabled = false;
                        }
                        else
                        {
                            if (!IsCheckingStarted)
                                BtnCheck.Text = "Scan";
                            BtnCheck.IsEnabled = true;
                        }

                        // Connect Button Text
                        if (IsDisconnecting) BtnConnect.Text = "Disconnecting...";
                        else if (IsConnecting) BtnConnect.Text = "Connecting...";
                        else BtnConnect.Text = IsDnsServerRunning ? "Disconnect" : "Connect";


                        if (IsDnsServerRunning)
                        {
                            string msg = $"DNS Latency: {LocalDnsLatency} ms.{NL}";
                            string proxyActive = IsProxyActive ? "Active." : "Inactive.";
                            msg += $"Proxy: {proxyActive}{NL}";
                            string dpiBypassActive = IsProxyDpiBypassActive ? "Active." : "Inactive.";
                            msg += $"DPI Bypass: {dpiBypassActive}";

                            NotificationRequest request = new()
                            {
                                NotificationId = MainNotificationId,
                                Title = "SDC",
                                Subtitle = "Secure DNS Client",
                                Description = msg,
                                Silent = true,
                                Android = new AndroidOptions
                                {
                                    Ongoing = true,
                                    AutoCancel = false,
                                }
                            };

                            await LocalNotificationCenter.Current.Show(request);
                        }
                        else
                        {
                            LocalNotificationCenter.Current.ClearAll();
                        }
                    });
                }
            });
        }

        public async void UpdateBoolDns()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(2000);
                    if (ConnectedDnsPort != -1)
                    {
                        string dns = $"{IPAddress.Loopback}:{ConnectedDnsPort}";
                        CheckDnsResult cdr = await CheckDns(dns, 5000, null);
                        IsDnsConnected = cdr.IsOnline;
                        LocalDnsLatency = cdr.Latency;
                    }
                }
            });
        }

        public async void UpdateBoolHttpProxy()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    IsProxyActive = ProxyServer.IsRunning;
                    ProxyRequests = ProxyServer.ActiveTunnels;
                    ProxyMaxRequests = ProxyServer.MaxRequests;
                    IsProxyDpiBypassActive = ProxyServer.IsDpiBypassActive;
                }
            });
        }

    }
}
