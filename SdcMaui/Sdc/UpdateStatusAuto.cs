using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Microsoft.Maui.Devices.Sensors;
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
                    Dispatcher.Dispatch(() =>
                    {
                        // Fix Word wrap Bug
                        labelStatus.WidthRequest = Width;

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

                        // Update HTTP Proxy Status
                        if (IsHttpProxyActive)
                        {
                            labelStatusHttpProxy.TextColor = Colors.MediumSeaGreen;
                            labelStatusHttpProxy.Text = "Active";
                        }
                        else
                        {
                            labelStatusHttpProxy.TextColor = Colors.IndianRed;
                            labelStatusHttpProxy.Text = "Inactive";
                        }

                        labelStatusHttpProxyRequests.TextColor = Colors.DodgerBlue;
                        labelStatusHttpProxyRequests.Text = $"{HttpProxyRequests} of {HttpProxyMaxRequests}";

                        labelStatusHttpProxyDpiBypass.TextColor = IsHttpProxyDpiBypassActive ? Colors.MediumSeaGreen : Colors.IndianRed;
                        labelStatusHttpProxyDpiBypass.Text = IsHttpProxyDpiBypassActive ? "Active" : "Inactive";

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
                        else BtnConnect.Text = IsDnsConnected ? "Disconnect" : "Connect";
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
                    IsHttpProxyActive = HttpProxy.IsRunning;
                    HttpProxyRequests = HttpProxy.AllRequests;
                    HttpProxyMaxRequests = HttpProxy.MaxRequests;
                    IsHttpProxyDpiBypassActive = HttpProxy.IsDpiActive;
                }
            });
        }

    }
}
