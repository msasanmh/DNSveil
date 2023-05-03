using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using MsmhTools;
using System.Net.Sockets;
using System.Management;
using CustomControls;
using System.Net.NetworkInformation;

namespace SecureDNSClient
{
    public class SecureDNS
    {
        // Settings XML path
        public static readonly string SettingsXmlPath = Path.GetFullPath(Info.ApplicationFullPathWithoutExtension + ".xml");

        // Binaries path
        public static readonly string BinaryDirPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary"));
        public static readonly string DnsLookup = Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary", "dnslookup.exe"));
        public static readonly string DnsProxy = Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary", "dnsproxy.exe"));
        public string DnsProxyDll = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        public static readonly string DNSCrypt = Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary", "dnscrypt-proxy.exe"));
        public static readonly string GoodbyeDpi = Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary", "goodbyedpi.exe"));
        public static readonly string WinDivert = Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary", "WinDivert.dll"));
        public static readonly string WinDivert32 = Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary", "WinDivert32.sys"));
        public static readonly string WinDivert64 = Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary", "WinDivert64.sys"));

        // Binaries file version path
        public static readonly string BinariesVersionPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "binary", "versions.txt"));

        // Others
        public static readonly string DNSCryptConfigPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "dnscrypt-proxy.toml"));
        public static readonly string CustomServersPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "CustomServers.txt"));
        public static readonly string WorkingServersPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "CustomServers_Working.txt"));
        public static readonly string DPIBlacklistPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "DPIBlacklist.txt"));
        public static readonly string NicNamePath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "NicName.txt"));
        public static readonly string HTTPProxyServerErrorLogPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "HTTPProxyServerError.log"));
        public static readonly IPAddress BootstrapDNSIPv6 = IPAddress.Parse("2001:4860:4860::8888");

        // Certificates path
        public static readonly string CertificateDirPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "certificate"));
        public static readonly string IssuerKeyPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "certificate", "rootCA.key"));
        public static readonly string IssuerCertPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "certificate", "rootCA.crt"));
        public static readonly string KeyPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "certificate", "localhost.key"));
        public static readonly string CertPath = Path.GetFullPath(Path.Combine(Info.CurrentPath, "certificate", "localhost.crt"));

        public static bool CheckDoH(string domain, string doHServer, int timeoutSeconds, ProcessPriorityClass processPriorityClass)
        {
            var task = Task.Run(() =>
            {
                string args = domain + " " + doHServer;
                string? result = ProcessManager.Execute(DnsLookup, args, true, false, Info.CurrentPath, processPriorityClass);

                if (!string.IsNullOrEmpty(result))
                {
                    return result.Contains("ANSWER SECTION");
                }
                else
                    return false;
            });

            if (task.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
                return task.Result;
            else
                return false;
        }

        public static bool CheckDoHInsecure(string domain, string doHServer, int timeoutSeconds, CustomTextBox bootstrapDNS, ProcessPriorityClass processPriorityClass)
        {
            var task = Task.Run(() =>
            {
                // Start local server
                string bootstrap = SettingsWindow.GetBootstrapDNS(bootstrapDNS).ToString();
                string dnsProxyArgs = "-l " + IPAddress.Loopback.ToString() + " -p 53 --insecure -u " + doHServer + " -b " + bootstrap + ":53";
                int localServerPID = ProcessManager.ExecuteOnly(DnsProxy, dnsProxyArgs, true, false, Info.CurrentPath, processPriorityClass);
                Task.Delay(500).Wait();
                string args = domain + " " + IPAddress.Loopback.ToString();
                string? result = ProcessManager.Execute(DnsLookup, args, true, false, Info.CurrentPath, processPriorityClass);

                if (!string.IsNullOrEmpty(result))
                {
                    ProcessManager.KillProcessByID(localServerPID);
                    Task.Delay(200).Wait();
                    return result.Contains("ANSWER SECTION");
                }
                else
                    return false;
            });

            if (task.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
                return task.Result;
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

        public static async Task<string> UrlToCompanyOffline(string url)
        {
            string company = "Couldn't retrieve information.";
            string? host = Network.UrlToHost(url);
            if (!string.IsNullOrWhiteSpace(host))
            {
                string? fileContent = await Resource.GetResourceTextFileAsync("SecureDNSClient.HostToCompany.txt"); // Load from Embedded Resource
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

        public static async Task<string> UrlToCompanyAsync(string url, string? proxyScheme = null)
        {
            string company = "Couldn't retrieve information.";
            string? host = Network.UrlToHost(url);
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
            string outPath = Path.Combine(Info.CurrentPath, "HostToCompany.txt");

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
                        object hostToCom = Network.UrlToHost(dns) + "|" + company;
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
