using System;
using System.Net;
using System.Diagnostics;
using MsmhTools;
using System.Net.Sockets;
using CustomControls;
using System.Net.NetworkInformation;
using System.Reflection;
using MsmhTools.DnsTool;

namespace SecureDNSClient
{
    public class SecureDNS
    {
        // App Directory Path
        public static readonly string CurrentPath = AppContext.BaseDirectory;

        // Settings XML path
        public static readonly string SettingsXmlPath = Path.GetFullPath(Info.ApplicationFullPathWithoutExtension + ".xml");

        // Binaries path
        public static readonly string BinaryDirPath = Path.GetFullPath(Path.Combine(CurrentPath, "binary"));
        public static readonly string DnsLookup = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "dnslookup.exe"));
        public static readonly string DnsProxy = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "dnsproxy.exe"));
        public string DnsProxyDll = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        public static readonly string DNSCrypt = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "dnscrypt-proxy.exe"));
        public static readonly string GoodbyeDpi = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "goodbyedpi.exe"));
        public static readonly string WinDivert = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "WinDivert.dll"));
        public static readonly string WinDivert32 = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "WinDivert32.sys"));
        public static readonly string WinDivert64 = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "WinDivert64.sys"));

        // Binaries file version path
        public static readonly string BinariesVersionPath = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "versions.txt"));

        // Bootstrap and Fallback
        public static readonly IPAddress BootstrapDnsIPv4 = IPAddress.Parse("8.8.8.8");
        public static readonly IPAddress BootstrapDnsIPv6 = IPAddress.Parse("2001:4860:4860::8888");
        public static readonly int BootstrapDnsPort = 53;
        public static readonly IPAddress FallbackDnsIPv4 = IPAddress.Parse("8.8.8.8");
        public static readonly IPAddress FallbackDnsIPv6 = IPAddress.Parse("2001:4860:4860::8888");
        public static readonly int FallbackDnsPort = 53;

        // Certificates path
        public static readonly string CertificateDirPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate"));
        public static readonly string IssuerKeyPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate", "rootCA.key"));
        public static readonly string IssuerCertPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate", "rootCA.crt"));
        public static readonly string KeyPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate", "localhost.key"));
        public static readonly string CertPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate", "localhost.crt"));

        // Certificate Subject Names
        public static readonly string CertIssuerSubjectName = "CN=SecureDNSClient Authority";
        public static readonly string CertSubjectName = "CN=SecureDNSClient";

        // HTTP Proxy & Programs
        public static readonly string HttpProxyPath = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "SDCHttpProxy.exe"));
        public static readonly string FakeDnsRulesPath = Path.GetFullPath(Path.Combine(CurrentPath, "FakeDnsRules.txt"));
        public static readonly string BlackWhiteListPath = Path.GetFullPath(Path.Combine(CurrentPath, "BlackWhiteList.txt"));
        public static readonly string DontBypassListPath = Path.GetFullPath(Path.Combine(CurrentPath, "DontBypassList.txt"));

        // Others
        public static readonly string DNSCryptConfigPath = Path.GetFullPath(Path.Combine(CurrentPath, "dnscrypt-proxy.toml"));
        public static readonly string DNSCryptConfigCloudflarePath = Path.GetFullPath(Path.Combine(CurrentPath, "dnscrypt-proxy-fakeproxy.toml"));
        public static readonly string CustomServersPath = Path.GetFullPath(Path.Combine(CurrentPath, "CustomServers.txt"));
        public static readonly string WorkingServersPath = Path.GetFullPath(Path.Combine(CurrentPath, "CustomServers_Working.txt"));
        public static readonly string DPIBlacklistPath = Path.GetFullPath(Path.Combine(CurrentPath, "DPIBlacklist.txt"));
        public static readonly string DPIBlacklistCFPath = Path.GetFullPath(Path.Combine(CurrentPath, "DPIBlacklistCF.txt"));
        public static readonly string NicNamePath = Path.GetFullPath(Path.Combine(CurrentPath, "NicName.txt"));
        public static readonly string HTTPProxyServerErrorLogPath = Path.GetFullPath(Path.Combine(CurrentPath, "HTTPProxyServerError.log"));
        public static readonly string HTTPProxyServerRequestLogPath = Path.GetFullPath(Path.Combine(CurrentPath, "HTTPProxyServerRequest.log"));
        public static readonly string SavedEncodedDnsPath = Path.GetFullPath(Path.Combine(CurrentPath, "SavedEncodedDns.txt"));

        /// <summary>
        /// Check DNS and get latency (ms)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dnsServer"></param>
        /// <param name="timeoutMS"></param>
        /// <param name="processPriorityClass"></param>
        /// <returns>Returns -1 if DNS fail</returns>
        public static int CheckDns(string domain, string dnsServer, int timeoutMS, ProcessPriorityClass processPriorityClass)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            bool isDnsOK = CheckDnsWork(domain, dnsServer, timeoutMS, processPriorityClass);
            stopwatch.Stop();

            return isDnsOK ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;
        }

        /// <summary>
        /// Check DNS and get latency (ms)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dnsServer"></param>
        /// <param name="timeoutMS"></param>
        /// <param name="processPriorityClass"></param>
        /// <returns>Returns -1 if DNS fail</returns>
        public static int CheckDns(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort, ProcessPriorityClass processPriorityClass)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            bool isDnsOK = CheckDnsWork(insecure, domain, dnsServer, timeoutMS, localPort, bootstrap, bootsratPort, processPriorityClass);
            stopwatch.Stop();

            return isDnsOK ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;
        }

        /// <summary>
        /// Check DNS and get latency (ms)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dnsServer"></param>
        /// <param name="timeoutMS"></param>
        /// <param name="processPriorityClass"></param>
        /// <returns>Returns -1 if DNS fail</returns>
        public static async Task<int> CheckDnsAsync(string domain, string dnsServer, int timeoutMS, ProcessPriorityClass processPriorityClass)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            bool isDnsOK = await CheckDnsWorkAsync(domain, dnsServer, timeoutMS, processPriorityClass);
            stopwatch.Stop();

            return isDnsOK ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;
        }

        /// <summary>
        /// Check DNS and get latency (ms)
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dnsServer"></param>
        /// <param name="timeoutMS"></param>
        /// <param name="processPriorityClass"></param>
        /// <returns>Returns -1 if DNS fail</returns>
        public static async Task<int> CheckDnsAsync(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort, ProcessPriorityClass processPriorityClass)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            bool isDnsOK = await CheckDnsWorkAsync(insecure, domain, dnsServer, timeoutMS, localPort, bootstrap, bootsratPort, processPriorityClass);
            stopwatch.Stop();

            return isDnsOK ? Convert.ToInt32(stopwatch.ElapsedMilliseconds) : -1;
        }

        private static bool CheckDnsWork(string domain, string dnsServer, int timeoutMS, ProcessPriorityClass processPriorityClass)
        {
            var task = Task.Run(() =>
            {
                string args = domain + " " + dnsServer;
                string? result = ProcessManager.Execute(out Process _, DnsLookup, args, true, false, CurrentPath, processPriorityClass);

                if (!string.IsNullOrEmpty(result))
                {
                    return result.Contains("ANSWER SECTION");
                }
                else
                    return false;
            });

            if (task.Wait(TimeSpan.FromMilliseconds(timeoutMS)))
                return task.Result;
            else
                return false;
        }

        private static bool CheckDnsWork(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort, ProcessPriorityClass processPriorityClass)
        {
            Task<bool> task = Task.Run(async () =>
            {
                // Start local server
                string dnsProxyArgs = $"-l {IPAddress.Loopback} -p {localPort} ";
                if (insecure) dnsProxyArgs += "--insecure ";
                dnsProxyArgs += $"-u {dnsServer} -b {bootstrap}:{bootsratPort}";
                int localServerPID = ProcessManager.ExecuteOnly(out Process _, DnsProxy, dnsProxyArgs, true, false, CurrentPath, processPriorityClass);

                // Wait for DNSProxy
                Task wait1 = Task.Run(() =>
                {
                    while (!ProcessManager.FindProcessByPID(localServerPID))
                    {
                        if (ProcessManager.FindProcessByPID(localServerPID))
                            break;
                    }
                    return Task.CompletedTask;
                });
                await wait1.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS));

                string args = $"{domain} {IPAddress.Loopback}:{localPort}";
                string? result = ProcessManager.Execute(out Process _, DnsLookup, args, true, false, CurrentPath, processPriorityClass);

                if (!string.IsNullOrEmpty(result))
                {
                    ProcessManager.KillProcessByPID(localServerPID);

                    // Wait for DNSProxy to exit
                    Task wait2 = Task.Run(() =>
                    {
                        while (ProcessManager.FindProcessByPID(localServerPID))
                        {
                            if (!ProcessManager.FindProcessByPID(localServerPID))
                                break;
                        }
                        return Task.CompletedTask;
                    });
                    await wait2.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS));

                    return result.Contains("ANSWER SECTION");
                }
                else
                    return false;
            });

            if (task.Wait(TimeSpan.FromMilliseconds(timeoutMS)))
                return task.Result;
            else
                return false;
        }

        private static async Task<bool> CheckDnsWorkAsync(string domain, string dnsServer, int timeoutMS, ProcessPriorityClass processPriorityClass)
        {
            try
            {
                var task = Task.Run(() =>
                    {
                        string args = domain + " " + dnsServer;
                        string? result = ProcessManager.Execute(out Process _, DnsLookup, args, true, false, CurrentPath, processPriorityClass);

                        if (!string.IsNullOrEmpty(result))
                        {
                            return result.Contains("ANSWER SECTION");
                        }
                        else
                            return false;
                    });

                if (await task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMS)))
                    return task.Result;
                else
                    return false;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        private static async Task<bool> CheckDnsWorkAsync(bool insecure, string domain, string dnsServer, int timeoutMS, int localPort, string bootstrap, int bootsratPort, ProcessPriorityClass processPriorityClass)
        {
            // Start local server
            string dnsProxyArgs = $"-l {IPAddress.Loopback} -p {localPort} ";
            if (insecure) dnsProxyArgs += "--insecure ";
            dnsProxyArgs += $"-u {dnsServer} -b {bootstrap}:{bootsratPort}";
            int localServerPID = ProcessManager.ExecuteOnly(out Process _, DnsProxy, dnsProxyArgs, true, false, CurrentPath, processPriorityClass);

            // Wait for DNSProxy
            await Task.Run(() =>
            {
                while (!ProcessManager.FindProcessByPID(localServerPID))
                {
                    if (ProcessManager.FindProcessByPID(localServerPID))
                        break;
                }
            });

            string args = $"{domain} {IPAddress.Loopback}:{localPort}";
            string? result = ProcessManager.Execute(out Process _, DnsLookup, args, true, false, CurrentPath, processPriorityClass);

            if (!string.IsNullOrEmpty(result))
            {
                ProcessManager.KillProcessByPID(localServerPID);

                // Wait for DNSProxy to exit
                await Task.Run(() =>
                {
                    while (ProcessManager.FindProcessByPID(localServerPID))
                    {
                        if (!ProcessManager.FindProcessByPID(localServerPID))
                            break;
                    }
                });

                return result.Contains("ANSWER SECTION");
            }
            else
                return false;
        }

        public static bool IsDomainValid(string domain)
        {
            if (domain.StartsWith("http:", StringComparison.OrdinalIgnoreCase))
                return false;
            if (domain.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                return false;
            if (domain.Contains('/', StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }

        public static bool IsBlockedDomainValid(CustomTextBox customTextBox, CustomRichTextBox customRichTextBox, out string blockedDomain)
        {
            // Get DNS based blocked domain to check
            string domain = customTextBox.Text;

            // Check blocked domain is valid
            bool isBlockedDomainValid = IsDomainValid(domain);
            if (!isBlockedDomainValid)
            {
                string blockedDomainIsNotValid = $"{domain} is not valid. Fix it in Settings section.{Environment.NewLine}";
                customRichTextBox.InvokeIt(() => customRichTextBox.AppendText(blockedDomainIsNotValid, Color.IndianRed));
                blockedDomain = string.Empty;
                return false;
            }
            else
            {
                blockedDomain = domain;
                return true;
            }
        }

        public static string GetBinariesVersion(string binaryName)
        {
            if (File.Exists(BinariesVersionPath))
            {
                string content = File.ReadAllText(BinariesVersionPath);
                List<string> lines = content.SplitToLines();
                for (int n = 0; n < lines.Count; n++)
                {
                    string line = lines[n];
                    if (line.StartsWith(binaryName, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] split = line.Split(" ");
                        if (split[1].Length > 0)
                            return split[1];
                    }
                }
            }
            return "0.0.0";
        }

        public static string GetBinariesVersionFromResource(string binaryName)
        {
            string? content = NecessaryFiles.Resource1.versions;
            if (!string.IsNullOrWhiteSpace(content))
            {
                List<string> lines = content.SplitToLines();
                for (int n = 0; n < lines.Count; n++)
                {
                    string line = lines[n];
                    if (line.StartsWith(binaryName, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] split = line.Split(" ");
                        if (split[1].Length > 0)
                            return split[1];
                    }
                }
            }
            return "0.0.0";
        }

        public static async Task<string> HostToCompanyOffline(string host)
        {
            string company = "Couldn't retrieve information.";
            if (!string.IsNullOrWhiteSpace(host))
            {
                string? fileContent = await Resource.GetResourceTextFileAsync("SecureDNSClient.HostToCompany.txt", Assembly.GetExecutingAssembly()); // Load from Embedded Resource
                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    List<string> split = fileContent.SplitToLines();
                    for (int n = 0; n < split.Count; n++)
                    {
                        string hostToCom = split[n];
                        if (hostToCom.Contains(host))
                        {
                            string com = hostToCom.Split('|')[1];
                            if (!string.IsNullOrWhiteSpace(com))
                            {
                                company = com;
                                break;
                            }
                        }
                    }
                }
            }
            return company;
        }

        public static async Task<string> UrlToCompanyOffline(string url)
        {
            Network.GetUrlDetails(url, 53, out string host, out int _, out string _, out bool _);
            return await HostToCompanyOffline(host);
        }

        public static async Task<string> StampToCompanyOffline(string stampUrl)
        {
            string company = "Couldn't retrieve information.";
            // Can't always return Address
            try
            {
                DNSCryptStampReader stamp = new(stampUrl);
                if (stamp != null)
                {
                    if (!string.IsNullOrEmpty(stamp.Host))
                        company = await HostToCompanyOffline(stamp.Host);
                    else if (!string.IsNullOrEmpty(stamp.IP))
                        company = await HostToCompanyOffline(stamp.IP);
                    else if (!string.IsNullOrEmpty(stamp.ProviderName))
                        company = await HostToCompanyOffline(stamp.ProviderName);
                    else
                        company = await HostToCompanyOffline(stampUrl);
                }
                else
                    company = await HostToCompanyOffline(stampUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
            return company;
        }

        public static async Task<string> UrlToCompanyAsync(string url, string? proxyScheme = null)
        {
            string company = "Couldn't retrieve information.";
            Network.GetUrlDetails(url, 443, out string host, out int _, out string _, out bool _);
            if (!string.IsNullOrWhiteSpace(host))
            {
                IPAddress? ipAddress = Network.HostToIP(host);
                string? companyFull;
                if (ipAddress != null)
                {
                    if (proxyScheme == null)
                        companyFull = await Network.IpToCompanyAsync(ipAddress);
                    else
                        companyFull = await Network.IpToCompanyAsync(ipAddress, proxyScheme);
                    if (!string.IsNullOrWhiteSpace(companyFull))
                    {
                        company = string.Empty;
                        string[] split = companyFull.Split(" ");
                        for (int n = 0; n < split.Length; n++)
                        {
                            string s = split[n];
                            if (n != 0)
                                company += s + " ";
                        }
                        company = company.Trim();
                    }
                }
            }

            return company;
        }

        public static void UpdateNICs(CustomComboBox customComboBox)
        {
            customComboBox.Items.Clear();
            List<NetworkInterface> nics = Network.GetNetworkInterfacesIPv4();
            if (nics == null || nics.Count < 1)
            {
                Debug.WriteLine("There is no Network Interface.");
                return;
            }
            for (int n = 0; n < nics.Count; n++)
            {
                NetworkInterface nic = nics[n];
                customComboBox.Items.Add(nic.Name);
            }
            if (customComboBox.Items.Count > 0)
                customComboBox.SelectedIndex = 0;
        }

        // HostToCompany file generator
        private static List<object> HostToCompanyList = new();
        public static async Task HostToCompanyAsync(string hostsFilePath)
        {
            HostToCompanyList.Clear();
            string outPath = Path.Combine(CurrentPath, "HostToCompany.txt");

            if (File.Exists(outPath))
            {
                string content = File.ReadAllText(outPath);
                if (content.Length > 0)
                {
                    List<string> split = content.SplitToLines();
                    for (int n = 0; n < split.Count; n++)
                    {
                        object hostToCom = split[n];
                        HostToCompanyList.Add(hostToCom);
                    }
                }
            }

            //string? fileContent = Resource.GetResourceTextFile("SecureDNSClient.DoH-Servers.txt"); // Load from Embedded Resource
            string? fileContent = File.ReadAllText(hostsFilePath);

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                List<string> dnsList = fileContent.SplitToLines().RemoveDuplicates();
                int dnsCount = dnsList.Count;
                for (int n = 0; n < dnsCount; n++)
                {
                    string dns = dnsList[n];
                    string company = await UrlToCompanyAsync(dns);
                    if (!company.Contains("Couldn't retrieve information."))
                    {
                        Network.GetUrlDetails(dns, 443, out string host, out int _, out string _, out bool _);
                        object hostToCom = host + "|" + company;
                        HostToCompanyList.Add(hostToCom);
                        Debug.WriteLine(hostToCom);
                    }
                }
                // Remove Duplicates
                HostToCompanyList = HostToCompanyList.RemoveDuplicates();
                // Sort List
                HostToCompanyList.Sort();
                Task.Delay(500).Wait();
                // Save IpToCompany to file
                HostToCompanyList.SaveToFile(outPath);
                Debug.WriteLine("File saved.");
            }
        }

        



    }
}
