using MsmhToolsClass;
using System.Diagnostics;

namespace SecureDNSClient;

public partial class FormMain
{
    private static bool ProcessOutputFilter = false;

    private async Task<int> ConnectToWorkingServersAsync()
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
        int maxServers = GetMaxServersToConnectSetting();

        if (File.Exists(TheDll))
        {
            try
            {
                File.Delete(TheDll);
                Debug.WriteLine("DLL Deleted.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        TheDll = new SecureDNS().DnsProxyDll;
        using StreamWriter theDll = new(TheDll, false);

        // New SavedDnsList and SavedEncodedDnsList
        List<string> savedDnsList = new();
        List<string> savedEncodedDnsList = new();

        // Clear CurrentUsingCustomServers
        CurrentUsingCustomServersList.Clear();

        // Find fastest servers, max 10
        for (int n = 0; n < WorkingDnsList.Count; n++)
        {
            if (IsDisconnecting) return -1;

            Tuple<long, string> latencyHost = WorkingDnsList[n];
            string host = latencyHost.Item2;

            // Add host to UsingDnsList
            savedDnsList.Add(host);

            // Add encoded host to SavedEncodedDnsList
            savedEncodedDnsList.Add(EncodingTool.GetSHA512(host));

            // Update Current Using Custom Servers
            if (!IsBuiltinMode)
                CurrentUsingCustomServersList.Add(host);

            hosts += " -u " + host;
            theDll.WriteLine(host);

            countUsingServers = n + 1;
            if (n >= maxServers - 1) break;
        }

        // Update Saved Dns List
        SavedDnsList = new(savedDnsList);
        SavedEncodedDnsList = new(savedEncodedDnsList);

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
        dnsproxyArgs += " -v";
        if (IsDisconnecting) return -1;

        // Execute DnsProxy
        PIDDNSProxy = ProcessManager.ExecuteOnly(SecureDNS.DnsProxy, dnsproxyArgs, true, true, SecureDNS.CurrentPath, GetCPUPriority());

        // Write DNS Requests to Log (the output is heavy filtering queries causes high cpu usage)
        //process.OutputDataReceived -= process_DataReceived;
        //process.OutputDataReceived += process_DataReceived;
        //process.ErrorDataReceived -= process_DataReceived;
        //process.ErrorDataReceived += process_DataReceived;
        //process.BeginOutputReadLine(); // Redirects Standard Output to Event
        //process.BeginErrorReadLine();
        //void process_DataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    string? msgReq = e.Data + NL;
        //    if (!string.IsNullOrEmpty(msgReq))
        //    {
        //        if (ProcessOutputFilter)
        //            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgReq));
        //        ProcessOutputFilter = msgReq.Contains("ANSWER SECTION");
        //    }
        //}
        
        // Wait for DNSProxy
        Task wait1 = Task.Run(async () =>
        {
            while (true)
            {
                if (IsDisconnecting) break;
                if (ProcessManager.FindProcessByPID(PIDDNSProxy)) break;
                await Task.Delay(50);
            }
        });
        try { await wait1.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

        IsConnected = ProcessManager.FindProcessByPID(PIDDNSProxy);

        if (IsConnected)
        {
            string msgConnected = $"Connected.{NL}Waiting for DNS to get online...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgConnected, Color.MediumSeaGreen));
            
            return countUsingServers;
            // DnsProxy gets terminated if we Wait for Dns to get online here!
        }
        else return -1;
    }

}