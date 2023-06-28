using MsmhTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private async Task Connect()
        {
            // Write Connecting message to log
            string msgConnecting = "Connecting... Please wait..." + NL + NL;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnecting, Color.MediumSeaGreen));

            // Check open ports
            bool port53 = Network.IsPortOpen(IPAddress.Loopback.ToString(), 53, 3);
            bool port443 = Network.IsPortOpen(IPAddress.Loopback.ToString(), 443, 3);
            if (port53)
            {
                string existingProcessName = ProcessManager.GetProcessNameByListeningPort(53);
                existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
                string msg = $"Port 53 is occupied by \"{existingProcessName}\". You need to resolve the conflict." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                IsConnecting = false;
                return;
            }
            if (port443)
            {
                string existingProcessName = ProcessManager.GetProcessNameByListeningPort(443);
                existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
                string msg = $"Port 443 is occupied by \"{existingProcessName}\". You need to resolve the conflict." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                IsConnecting = false;
                return;
            }

            // Flush DNS
            FlushDNS(false);

            // Generate Certificate for DoH
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
                GenerateCertificate();

            // Get Connect mode
            ConnectMode connectMode = GetConnectMode();

            // Connect modes
            if (connectMode == ConnectMode.ConnectToWorkingServers)
            {
                //=== Connect DNSProxy (With working servers)
                int countUsingServers = ConnectToWorkingServers();

                // Wait Until DNS gets Online
                await Task.Run(async () =>
                {
                    Stopwatch stopwatch = new();
                    stopwatch.Start();
                    while (!IsDNSConnected)
                    {
                        if (IsDNSConnected || stopwatch.ElapsedMilliseconds > 30000 || !ProcessManager.FindProcessByID(PIDDNSProxy))
                        {
                            stopwatch.Stop();
                            return Task.CompletedTask;
                        }
                        await Task.Delay(100);
                    }
                    return Task.CompletedTask;
                });

                // Write dnsproxy message to log
                string msgDnsProxy = string.Empty;
                if (ProcessManager.FindProcessByID(PIDDNSProxy))
                {
                    if (countUsingServers > 1)
                        msgDnsProxy = "Local DNS Server started using " + countUsingServers + " fastest servers in parallel." + NL;
                    else
                        msgDnsProxy = "Local DNS Server started using " + countUsingServers + " server." + NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDnsProxy, Color.MediumSeaGreen));

                    // Connected with DNSProxy
                    internalConnect();

                    // Write Connected message to log
                    connectMessage();
                }
                else
                {
                    msgDnsProxy = "Error: Couldn't start DNSProxy!" + NL;
                    if (!IsDisconnecting)
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDnsProxy, Color.IndianRed));
                }
            }
            else if (connectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI || connectMode == ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI)
            {
                //=== Connect Cloudflare
                int camouflagePort = -1;
                int fakeProxyPort = int.Parse(CustomNumericUpDownSettingFakeProxyPort.Value.ToString());
                int camouflageDNSPort = int.Parse(CustomNumericUpDownSettingCamouflageDnsPort.Value.ToString());
                camouflagePort = connectMode == ConnectMode.ConnectToFakeProxyDohViaProxyDPI ? fakeProxyPort : camouflageDNSPort;

                bool connected = await BypassFakeProxyDohStart(camouflagePort);
                if (connected)
                {
                    // Write Connected message to log
                    connectMessage();
                }
                else
                {
                    BypassFakeProxyDohStop(true, true, true, false);
                }
            }
            else if (connectMode == ConnectMode.ConnectToPopularServersWithProxy)
            {
                //=== Connect To Servers Using Proxy
                await ConnectToServersUsingProxy();
                await Task.Delay(1000);

                if (ProcessManager.FindProcessByID(PIDDNSCrypt))
                {
                    // Connected with DNSCrypt
                    internalConnect();
                }
                else
                {
                    // Write DNSCryptProxy Error to log
                    string msg = "DNSCryptProxy couldn't connect, try again.";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
                }
            }

            void internalConnect()
            {
                // To see online status immediately
                Parallel.Invoke(
                    () => UpdateBoolDnsOnce(10000),
                    () => UpdateBoolDohOnce(10000)
                );

                // Update Groupbox Status
                UpdateStatusLong();

                // Go to DPI Tab if DPI is not already active
                if (ConnectAllClicked && !IsDPIActive)
                {
                    this.InvokeIt(() => CustomTabControlMain.SelectedIndex = 0);
                    this.InvokeIt(() => CustomTabControlSecureDNS.SelectedIndex = 2);
                    this.InvokeIt(() => CustomTabControlDPIBasicAdvanced.SelectedIndex = 2);
                }
            }

            void connectMessage()
            {
                // Update Local IP
                LocalIP = Network.GetLocalIPv4();

                // Get Loopback IP
                IPAddress loopbackIP = IPAddress.Loopback;

                // Write local DNS addresses to log
                string msgLocalDNS1 = "Local DNS Proxy:";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDNS1, Color.LightGray));
                string msgLocalDNS2 = NL + loopbackIP;
                if (LocalIP != null)
                    msgLocalDNS2 += NL + LocalIP.ToString();
                msgLocalDNS2 += NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDNS2, Color.DodgerBlue));

                // Write local DoH addresses to log
                if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
                {
                    string msgLocalDoH1 = "Local DNS Over HTTPS:";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH1, Color.LightGray));
                    string msgLocalDoH2 = NL + "https://" + loopbackIP.ToString() + "/dns-query";
                    if (LocalIP != null)
                        msgLocalDoH2 += NL + "https://" + LocalIP.ToString() + "/dns-query";
                    msgLocalDoH2 += NL;
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgLocalDoH2, Color.DodgerBlue));
                }
            }
        }
    }
}
