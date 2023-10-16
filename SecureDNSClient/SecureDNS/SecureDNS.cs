using System.Net;
using System.Diagnostics;
using CustomControls;
using System.Net.NetworkInformation;
using System.Reflection;
using MsmhToolsClass;
using MsmhToolsClass.DnsTool;

namespace SecureDNSClient;

public class SecureDNS
{
    // App Name Without Extension
    private static readonly string appNameWithoutExtension = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
    // App Directory Path
    public static readonly string CurrentPath = Path.GetFullPath(AppContext.BaseDirectory);

    // Binaries path
    public static readonly string BinaryDirPath = Path.GetFullPath(Path.Combine(CurrentPath, "binary"));
    public static readonly string DnsLookup = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "dnslookup.exe"));
    public static readonly string DnsProxy = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "dnsproxy.exe"));
    public string DnsProxyDll = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
    public static readonly string DNSCrypt = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "dnscrypt-proxy.exe"));
    public static readonly string ProxyServerPath = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "SDCProxyServer.exe"));
    public static readonly string GoodbyeDpi = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "goodbyedpi.exe"));
    public static readonly string WinDivert = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "WinDivert.dll"));
    public static readonly string WinDivert32 = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "WinDivert32.sys"));
    public static readonly string WinDivert64 = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "WinDivert64.sys"));

    // Binaries file version path
    public static readonly string BinariesVersionPath = Path.GetFullPath(Path.Combine(CurrentPath, "binary", "versions.txt"));

    // Certificates path
    public static readonly string CertificateDirPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate"));
    public static readonly string IssuerKeyPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate", "rootCA.key"));
    public static readonly string IssuerCertPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate", "rootCA.crt"));
    public static readonly string KeyPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate", "localhost.key"));
    public static readonly string CertPath = Path.GetFullPath(Path.Combine(CurrentPath, "certificate", "localhost.crt"));

    // Certificate Subject Names
    public static readonly string CertIssuerSubjectName = "CN=SecureDNSClient Authority";
    public static readonly string CertSubjectName = "CN=SecureDNSClient";

    // Others
    public static readonly string DNSCryptConfigPath = Path.GetFullPath(Path.Combine(CurrentPath, "dnscrypt-proxy.toml"));
    public static readonly string DNSCryptConfigFakeProxyPath = Path.GetFullPath(Path.Combine(CurrentPath, "dnscrypt-proxy-fakeproxy.toml"));
    public static readonly string DPIBlacklistFPPath = Path.GetFullPath(Path.Combine(CurrentPath, "DPIBlacklistFP.txt"));
    public static readonly string ProxyServerErrorLogPath = Path.GetFullPath(Path.Combine(CurrentPath, "ProxyServerError.log"));
    public static readonly string ProxyServerRequestLogPath = Path.GetFullPath(Path.Combine(CurrentPath, "ProxyServerRequest.log"));

    // User Data
    private const string UDN = "user";
    public static readonly string UserDataDirPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN));
    public static readonly string SettingsXmlPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, appNameWithoutExtension + ".xml")); // Settings XML path
    public static readonly string SettingsXmlDnsLookup = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "DnsLookupSettings.xml"));
    public static readonly string SettingsXmlIpScanner = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "IpScannerSettings.xml"));
    public static readonly string SettingsXmlDnsScanner = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "DnsScannerSettings.xml"));
    public static readonly string SettingsXmlDnsScannerExport = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "DnsScannerExportSettings.xml"));
    public static readonly string UserIdPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "uid.txt"));
    public static readonly string FakeDnsRulesPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "FakeDnsRules.txt"));
    public static readonly string BlackWhiteListPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "BlackWhiteList.txt"));
    public static readonly string DontBypassListPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "DontBypassList.txt"));
    public static readonly string CustomServersPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "CustomServers.txt"));
    public static readonly string CustomServersXmlPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "CustomServers.xml"));
    public static readonly string WorkingServersPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "CustomServers_Working.txt"));
    public static readonly string DPIBlacklistPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "DPIBlacklist.txt"));
    public static readonly string NicNamePath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "NicName.txt"));
    public static readonly string SavedEncodedDnsPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "SavedEncodedDns.txt"));
    public static readonly string LogWindowPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "LogWindow.txt"));
    public static readonly string CloseStatusPath = Path.GetFullPath(Path.Combine(CurrentPath, UDN, "CloseStatus.txt"));

    // User Data Old Path
    public static readonly string OldSettingsXmlPath = Path.GetFullPath(appNameWithoutExtension + ".xml");
    public static readonly string OldSettingsXmlDnsLookup = Path.GetFullPath(CurrentPath + "DnsLookupSettings.xml");
    public static readonly string OldSettingsXmlIpScanner = Path.GetFullPath(CurrentPath + "IpScannerSettings.xml");
    public static readonly string OldUserIdPath = Path.GetFullPath(Path.Combine(CurrentPath, "uid.txt"));
    public static readonly string OldFakeDnsRulesPath = Path.GetFullPath(Path.Combine(CurrentPath, "FakeDnsRules.txt"));
    public static readonly string OldBlackWhiteListPath = Path.GetFullPath(Path.Combine(CurrentPath, "BlackWhiteList.txt"));
    public static readonly string OldDontBypassListPath = Path.GetFullPath(Path.Combine(CurrentPath, "DontBypassList.txt"));
    public static readonly string OldCustomServersPath = Path.GetFullPath(Path.Combine(CurrentPath, "CustomServers.txt"));
    public static readonly string OldWorkingServersPath = Path.GetFullPath(Path.Combine(CurrentPath, "CustomServers_Working.txt"));
    public static readonly string OldDPIBlacklistPath = Path.GetFullPath(Path.Combine(CurrentPath, "DPIBlacklist.txt"));
    public static readonly string OldNicNamePath = Path.GetFullPath(Path.Combine(CurrentPath, "NicName.txt"));
    public static readonly string OldSavedEncodedDnsPath = Path.GetFullPath(Path.Combine(CurrentPath, "SavedEncodedDns.txt"));

    // Bootstrap and Fallback
    public static readonly IPAddress BootstrapDnsIPv4 = IPAddress.Parse("8.8.8.8");
    public static readonly IPAddress BootstrapDnsIPv6 = IPAddress.Parse("2001:4860:4860::8888");
    public static readonly int BootstrapDnsPort = 53;
    public static readonly IPAddress FallbackDnsIPv4 = IPAddress.Parse("8.8.8.8");
    public static readonly IPAddress FallbackDnsIPv6 = IPAddress.Parse("2001:4860:4860::8888");
    public static readonly int FallbackDnsPort = 53;

    public static void GenerateUid(Control control)
    {
        Task.Run(() =>
        {
            try
            {
                string uid;
                if (File.Exists(UserIdPath))
                    uid = File.ReadAllText(UserIdPath);
                else
                {
                    uid = Info.GetUniqueIdString(true);
                    File.WriteAllText(UserIdPath, uid);
                }

                uid = uid.Trim();
                if (string.IsNullOrEmpty(uid)) return;
                string counterUrl = "https://msmh.html-5.me/counter.php";
                string productVersion = Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion ?? "0.0.0";
                string args = $"{counterUrl}?uid={uid}&sdcver={productVersion}";

                if (!NetworkTool.IsInternetAlive()) return;

                control.InvokeIt(() =>
                {
                    WebBrowser webBrowser = new();
                    webBrowser.Navigate(new Uri(args));
                    webBrowser.Refresh(WebBrowserRefreshOption.Completely);
                    webBrowser.DocumentCompleted -= WebBrowser_DocumentCompleted;
                    webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
                    void WebBrowser_DocumentCompleted(object? sender, WebBrowserDocumentCompletedEventArgs e)
                    {
                        // To make sure page is fully loaded
                        webBrowser.Dispose();
                        Debug.WriteLine("Counter Success.");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GenerateUid: " + ex.Message);
            }
        });
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
            string? fileContent = await ResourceTool.GetResourceTextFileAsync("SecureDNSClient.HostToCompany.txt", Assembly.GetExecutingAssembly()); // Load from Embedded Resource
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
        NetworkTool.GetUrlDetails(url, 53, out _, out string host, out _, out int _, out string _, out bool _);
        return await HostToCompanyOffline(host);
    }

    public static async Task<string> StampToCompanyOffline(string stampUrl)
    {
        string company = "Couldn't retrieve information.";

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
        NetworkTool.GetUrlDetails(url, 443, out _, out string host, out _, out int _, out string _, out bool _);
        if (!string.IsNullOrWhiteSpace(host))
        {
            string ipStr = GetIP.GetIpFromSystem(host);
            string? companyFull;
            if (!string.IsNullOrEmpty(ipStr))
            {
                if (proxyScheme == null)
                    companyFull = await NetworkTool.IpToCompanyAsync(ipStr);
                else
                    companyFull = await NetworkTool.IpToCompanyAsync(ipStr, proxyScheme);
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

    public static void UpdateNICs(CustomComboBox ccb, out List<NetworkInterface> availableNICs)
    {
        object item = ccb.SelectedItem;
        ccb.Items.Clear();
        List<NetworkInterface> nics = NetworkTool.GetNetworkInterfacesIPv4(true);
        availableNICs = new(nics);
        if (nics == null || nics.Count < 1)
        {
            Debug.WriteLine("There is no Network Interface.");
            return;
        }
        for (int n = 0; n < nics.Count; n++)
        {
            NetworkInterface nic = nics[n];
            ccb.Items.Add(nic.Name);
        }
        if (ccb.Items.Count > 0)
        {
            bool exist = false;
            for (int i = 0; i < ccb.Items.Count; i++)
            {
                object selectedItem = ccb.Items[i];
                if (item != null && item.Equals(selectedItem))
                {
                    exist = true;
                    break;
                }
            }
            if (exist)
                ccb.SelectedItem = item;
            else
                ccb.SelectedIndex = 0;
            ccb.DropDownHeight = nics.Count * ccb.Height;
        }
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
                    NetworkTool.GetUrlDetails(dns, 443, out _, out string host, out _, out int _, out string _, out bool _);
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