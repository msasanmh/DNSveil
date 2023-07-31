using MsmhTools;
using System;
using System.Diagnostics;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private int ConnectToWorkingServers()
        {
            // Write Check first to log
            if (NumberOfWorkingServers < 1 && SavedDnsList.Count < 1)
            {
                string msgCheck = "Check servers first." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCheck, Color.IndianRed));
                return -1;
            }

            string hosts = string.Empty;
            int countUsingServers = 0;

            // Sort by latency
            if (WorkingDnsList.Count > 1)
                WorkingDnsList = WorkingDnsList.OrderBy(t => t.Item1).ToList();

            // Get number of max servers
            int maxServers = decimal.ToInt32(CustomNumericUpDownSettingMaxServers.Value);

            TheDll = new SecureDNS().DnsProxyDll;
            using StreamWriter theDll = new(TheDll, false);

            // Clear UsingDnsList & SavedEncodedDnsList
            SavedDnsList.Clear();
            SavedEncodedDnsList.Clear();

            // Find fastest servers, max 10
            for (int n = 0; n < WorkingDnsList.Count; n++)
            {
                if (IsDisconnecting) return -1;

                Tuple<long, string> latencyHost = WorkingDnsList[n];
                string host = latencyHost.Item2;

                // Add host to UsingDnsList
                SavedDnsList.Add(host);

                // Add encoded host to SavedEncodedDnsList
                SavedEncodedDnsList.Add(EncodingTool.GetSHA512(host));

                hosts += " -u " + host;
                theDll.WriteLine(host);

                countUsingServers = n + 1;
                if (n >= maxServers - 1) break;
            }

            // Save encoded hosts to file
            SavedEncodedDnsList.SaveToFile(SecureDNS.SavedEncodedDnsPath);

            // Get Bootstrap IP & Port
            string bootstrap = GetBootstrapSetting(out int bootstrapPort).ToString();

            // Start dnsproxy
            string dnsproxyArgs = "-l 0.0.0.0";

            // Add Legacy DNS args
            dnsproxyArgs += " -p 53";

            // Add DoH args
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
            {
                if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
                {
                    // Get DoH Port
                    int dohPort = GetDohPortSetting();

                    // Set Connected DoH Port
                    ConnectedDohPort = dohPort;

                    dnsproxyArgs += " --https-port=" + dohPort + " --tls-crt=\"" + SecureDNS.CertPath + "\" --tls-key=\"" + SecureDNS.KeyPath + "\"";
                }
            }

            // Add Cache args
            if (CustomCheckBoxSettingEnableCache.Checked)
                dnsproxyArgs += " --cache";

            // Add Insecure
            if (CustomCheckBoxInsecure.Checked)
                dnsproxyArgs += " --insecure";

            // Add upstream args
            //dnsproxyArgs += hosts;
            dnsproxyArgs += $" -u \"{TheDll}\"";
            if (countUsingServers > 1)
                dnsproxyArgs += $" --all-servers -b {bootstrap}:{bootstrapPort}";
            else
                dnsproxyArgs += $" -b {bootstrap}:{bootstrapPort}";

            if (IsDisconnecting) return -1;

            // Execute DnsProxy
            PIDDNSProxy = ProcessManager.ExecuteOnly(out Process _, SecureDNS.DnsProxy, dnsproxyArgs, true, false, SecureDNS.CurrentPath, GetCPUPriority());

            return countUsingServers;
        }
    }
}
