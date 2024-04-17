using System.Net;
using System.Diagnostics;
using CustomControls;
using System.Reflection;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Runtime.InteropServices;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SecureDNSClient;

public class SecureDNS
{
    // App Name Without Extension
    private static readonly string appNameWithoutExtension = GetFileNameWithoutExtension(Application.ExecutablePath);
    
    // App Directory Path
    public static readonly string CurrentPath = GetFullPath(AppContext.BaseDirectory);

    public static readonly string CurrentExecutablePath = GetFullPath(Application.ExecutablePath);

    public static string GetParent
    {
        get
        {
            string parent = CurrentPath;
            try
            {
                DirectoryInfo info = new(CurrentPath);
                if (info.Parent != null) parent = info.Parent.FullName;
            }
            catch (Exception) { }
            return parent;
        }
    }

    // Binaries path
    public static readonly string BinaryDirPath = GetFullPath(CurrentPath, "binary");
    public static readonly string DnsLookup = GetFullPath(CurrentPath, "binary", "dnslookup.exe");
    public static readonly string RandomPath = GetRandomPath();
    public static readonly string AgnosticServerPath = GetFullPath(CurrentPath, "binary", "SDCAgnosticServer.exe");
    public static readonly string GoodbyeDpi = GetFullPath(CurrentPath, "binary", "goodbyedpi.exe");
    public static readonly string WinDivert = GetFullPath(CurrentPath, "binary", "WinDivert.dll");
    public static readonly string WinDivert32 = GetFullPath(CurrentPath, "binary", "WinDivert32.sys");
    public static readonly string WinDivert64 = GetFullPath(CurrentPath, "binary", "WinDivert64.sys");
    public static readonly string BinariesVersionPath = GetFullPath(CurrentPath, "binary", "versions.txt");

    // Others
    public static readonly string DPIBlacklistFPPath = GetFullPath(CurrentPath, "DPIBlacklistFP.txt");

    // User Data
    private const string UDN = "UserData";

    public static string UserDataDirPath
    {
        get
        {
            if (Program.IsPortable) return GetFullPath(GetParent, UDN);
            else
            {
                try
                {
                    string appDataLocal = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                    return GetFullPath(appDataLocal, "SecureDNSClient", UDN);
                }
                catch (Exception)
                {
                    string msgErr = "System AppData Folder Is Not Reachable.";
                    MessageBox.Show(msgErr, "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    Application.Exit();
                    return GetFullPath(GetParent, UDN);
                }
            }
        }
    }

    public static readonly string SettingsXmlPath = GetFullPath(UserDataDirPath, appNameWithoutExtension + ".xml"); // AgnosticSettings XML path
    public static readonly string SettingsXmlDnsLookup = GetFullPath(UserDataDirPath, "DnsLookupSettings.xml");
    public static readonly string SettingsXmlIpScanner = GetFullPath(UserDataDirPath, "IpScannerSettings.xml");
    public static readonly string SettingsXmlDnsScanner = GetFullPath(UserDataDirPath, "DnsScannerSettings.xml");
    public static readonly string SettingsXmlDnsScannerExport = GetFullPath(UserDataDirPath, "DnsScannerExportSettings.xml");
    public static readonly string UserIdPath = GetFullPath(UserDataDirPath, "uid.txt");
    public static readonly string DnsRulesPath = GetFullPath(UserDataDirPath, "DnsRules.txt");
    public static readonly string ProxyRulesPath = GetFullPath(UserDataDirPath, "ProxyRules.txt");
    public static readonly string CustomServersPath = GetFullPath(UserDataDirPath, "CustomServers.txt");
    public static readonly string CustomServersXmlPath = GetFullPath(UserDataDirPath, "CustomServers.xml");
    public static readonly string WorkingServersPath = GetFullPath(UserDataDirPath, "CustomServers_Working.txt");
    public static readonly string DPIBlacklistPath = GetFullPath(UserDataDirPath, "DPIBlacklist.txt"); // GoodbyeDPI Black List
    public static readonly string NicNamePath = GetFullPath(UserDataDirPath, "NicName.txt");
    public static readonly string SavedEncodedDnsPath = GetFullPath(UserDataDirPath, "SavedEncodedDns.txt");
    public static readonly string LogWindowPath = GetFullPath(UserDataDirPath, "LogWindow.txt");
    public static readonly string CloseStatusPath = GetFullPath(UserDataDirPath, "CloseStatus.txt");
    public static readonly string ErrorLogPath = GetFullPath(UserDataDirPath, "ErrorLog.txt");

    // Old Proxy ProxyRules
    public static readonly string BlackWhiteListPath = GetFullPath(UserDataDirPath, "BlackWhiteList.txt");
    public static readonly string FakeDnsRulesPath = GetFullPath(UserDataDirPath, "FakeDnsRules.txt");
    public static readonly string FakeSniRulesPath = GetFullPath(UserDataDirPath, "FakeSniRules.txt");
    public static readonly string DontBypassListPath = GetFullPath(UserDataDirPath, "DontBypassList.txt");

    // User Data Old Path
    private const string OldUDN = "user";
    public static readonly string OldUserDataDirPath = GetFullPath(CurrentPath, OldUDN);

    // Certificates Path
    public static readonly string CertificateDirPath = GetFullPath(UserDataDirPath, "certificate");
    public static readonly string IssuerKeyPath = GetFullPath(CertificateDirPath, "rootCA.key");
    public static readonly string IssuerCertPath = GetFullPath(CertificateDirPath, "rootCA.crt");
    public static readonly string KeyPath = GetFullPath(CertificateDirPath, "localhost.key");
    public static readonly string CertPath = GetFullPath(CertificateDirPath, "localhost.crt");

    // Certificates Old Path
    public static readonly string OldCertificateDirPath = GetFullPath(CurrentPath, "certificate");

    // Certificate Subject Names
    public static readonly string CertIssuerSubjectName = "SecureDNSClient Authority";
    public static readonly string CertSubjectName = "SecureDNSClient";

    // Bootstrap and Fallback
    public static readonly IPAddress BootstrapDnsIPv4 = IPAddress.Parse("8.8.8.8");
    public static readonly IPAddress BootstrapDnsIPv6 = IPAddress.Parse("2001:4860:4860::8888");
    public static readonly int BootstrapDnsPort = 53;
    public static readonly IPAddress FallbackDnsIPv4 = IPAddress.Parse("8.8.8.8");
    public static readonly IPAddress FallbackDnsIPv6 = IPAddress.Parse("2001:4860:4860::8888");
    public static readonly int FallbackDnsPort = 53;

    // Windows Firewall Rule Names
    public static readonly string FirewallRule_SdcIn = "SDC IN";
    public static readonly string FirewallRule_SdcOut = "SDC OUT";
    public static readonly string FirewallRule_SdcDnsLookupIn = "SDC DnsLookup IN";
    public static readonly string FirewallRule_SdcDnsLookupOut = "SDC DnsLookup OUT";
    public static readonly string FirewallRule_SdcAgnosticServerIn = "SDC AgnosticServer IN";
    public static readonly string FirewallRule_SdcAgnosticServerOut = "SDC AgnosticServer OUT";
    public static readonly string FirewallRule_SdcGoodbyeDpiIn = "SDC GoodbyeDpi IN";
    public static readonly string FirewallRule_SdcGoodbyeDpiOut = "SDC GoodbyeDpi OUT";
    public static readonly string FirewallRule_SdcWinDivertIn = "SDC WinDivert IN";
    public static readonly string FirewallRule_SdcWinDivertOut = "SDC WinDivert OUT";
    public static readonly string FirewallRule_SdcWinDivert32In = "SDC WinDivert32 IN";
    public static readonly string FirewallRule_SdcWinDivert32Out = "SDC WinDivert32 OUT";
    public static readonly string FirewallRule_SdcWinDivert64In = "SDC WinDivert64 IN";
    public static readonly string FirewallRule_SdcWinDivert64Out = "SDC WinDivert64 OUT";

    private static string GetFileNameWithoutExtension(string path)
    {
        try
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
            Application.Exit();
            return string.Empty;
        }
    }

    private static string GetFullPath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
            Application.Exit();
            return string.Empty;
        }
    }

    private static string GetFullPath(string path1, string path2)
    {
        try
        {
            return Path.GetFullPath(Path.Combine(path1, path2));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
            Application.Exit();
            return string.Empty;
        }
    }

    private static string GetFullPath(string path1, string path2, string path3)
    {
        try
        {
            return Path.GetFullPath(Path.Combine(path1, path2, path3));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
            Application.Exit();
            return string.Empty;
        }
    }

    private static string GetRandomPath()
    {
        try
        {
            return GetFullPath(Path.GetTempPath(), Path.GetRandomFileName());
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "System", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
            Application.Exit();
            return string.Empty;
        }
    }

    public static void GenerateUid(Control control)
    {
        Task.Run(() =>
        {
            try
            {
                // Disable Cert Check
                static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
                
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

                control.InvokeIt(() =>
                {
                    WebBrowser webBrowser = new();
                    webBrowser.ScriptErrorsSuppressed = true; // Disable Dialog Boxes
                    webBrowser.Navigate(new Uri(args));
                    webBrowser.Refresh(WebBrowserRefreshOption.Completely);
                    webBrowser.DocumentCompleted -= WebBrowser_DocumentCompleted;
                    webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
                    void WebBrowser_DocumentCompleted(object? sender, WebBrowserDocumentCompletedEventArgs e)
                    {
                        // To make sure page is fully loaded
                        webBrowser.Dispose();

                        // Enable Cert Check
                        ServicePointManager.ServerCertificateValidationCallback = null;

                        Debug.WriteLine("Counter Success.");
                    }
                });
            }
            catch (Exception ex)
            {
                // Enable Cert Check
                ServicePointManager.ServerCertificateValidationCallback = null;

                Debug.WriteLine("GenerateUid: " + ex.Message);
            }
        });
    }

    public static bool IsDomainValid(string domain)
    {
        try
        {
            if (string.IsNullOrEmpty(domain)) return false;
            if (domain.StartsWith("http:", StringComparison.OrdinalIgnoreCase)) return false;
            if (domain.StartsWith("https:", StringComparison.OrdinalIgnoreCase)) return false;
            if (domain.Contains('/', StringComparison.OrdinalIgnoreCase)) return false;
            if (!domain.Contains('.', StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SecureDNS IsDomainValid: " + ex.Message);
            return false;
        }
    }

    public static bool IsBlockedDomainValid(CustomTextBox customTextBox, out string blockedDomain)
    {
        try
        {
            // Get DNS based blocked domain to check
            string domain = customTextBox.Text;
            domain = domain.Trim();
            if (domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) domain = domain[7..];
            if (domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) domain = domain[8..];
            if (domain.EndsWith('/')) domain = domain.TrimEnd('/');

            // Check blocked domain is valid
            bool isBlockedDomainValid = IsDomainValid(domain);
            if (!isBlockedDomainValid)
            {
                blockedDomain = string.Empty;
                return false;
            }
            else
            {
                blockedDomain = domain;
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SecureDNS IsBlockedDomainValid: " + ex.Message);
            blockedDomain = string.Empty;
            return false;
        }
    }

    public static string GetBinariesVersion(string binaryName, Architecture arch)
    {
        try
        {
            if (File.Exists(BinariesVersionPath))
            {
                string content = File.ReadAllText(BinariesVersionPath);
                List<string> lines = content.SplitToLines();
                for (int n = 0; n < lines.Count; n++)
                {
                    string line = lines[n];
                    string appName = $"{binaryName}-{arch}";
                    if (line.StartsWith(appName, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] split = line.Split(" ");
                        if (split[1].Length > 0) return split[1];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SecureDNS GetBinariesVersion: " + ex.Message);
        }

        return "0.0.0";
    }

    public static string GetBinariesVersionFromResource(string binaryName, Architecture arch)
    {
        try
        {
            string? content = NecessaryFiles.Resource1.versions;
            if (!string.IsNullOrWhiteSpace(content))
            {
                List<string> lines = content.SplitToLines();
                for (int n = 0; n < lines.Count; n++)
                {
                    string line = lines[n];
                    string appName = $"{binaryName}-{arch}";
                    if (line.StartsWith(appName, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] split = line.Split(" ");
                        if (split[1].Length > 0) return split[1];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SecureDNS GetBinariesVersionFromResource: " + ex.Message);
        }

        return "99.99.99";
    }

    public static async Task<string> HostToCompanyOffline(string host)
    {
        string company = "Couldn't retrieve information.";

        try
        {
            if (!string.IsNullOrWhiteSpace(host))
            {
                string? fileContent = await ResourceTool.GetResourceTextFileAsync("SecureDNSClient.HostToCompany.txt", Assembly.GetExecutingAssembly()); // Load From Embedded Resource
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SecureDNS HostToCompanyOffline: " + ex.Message);
        }

        return company;
    }

    public static async Task<string> UrlToCompanyOffline(string url)
    {
        NetworkTool.GetUrlDetails(url, 53, out _, out string host, out _, out _, out int _, out string _, out bool _);
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
                else if (!stamp.IP.Equals(IPAddress.None))
                    company = await HostToCompanyOffline(stamp.IP.ToString());
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
            Debug.WriteLine("SecureDNS StampToCompanyOffline: " + ex.Message);
        }

        return company;
    }

    public static async Task<string> UrlToCompanyAsync(string url, string? proxyScheme = null)
    {
        string company = "Couldn't Retrieve Information.";
        NetworkTool.GetUrlDetails(url, 443, out _, out string host, out _, out _, out int _, out string _, out bool _);
        if (!string.IsNullOrWhiteSpace(host))
        {
            IPAddress ip = GetIP.GetIpFromSystem(host);
            string? companyFull;
            if (!ip.Equals(IPAddress.None))
            {
                if (proxyScheme == null)
                    companyFull = await NetworkTool.IpToCompanyAsync(ip.ToString());
                else
                    companyFull = await NetworkTool.IpToCompanyAsync(ip.ToString(), proxyScheme);
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

    // HostToCompany file Generator
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
                    NetworkTool.GetUrlDetails(dns, 443, out _, out string host, out _, out _, out int _, out string _, out bool _);
                    object hostToCom = host + "|" + company;
                    HostToCompanyList.Add(hostToCom);
                    Debug.WriteLine(hostToCom);
                }
            }
            // Remove Duplicates
            HostToCompanyList = HostToCompanyList.RemoveDuplicates();
            // Sort List
            HostToCompanyList.Sort();
            await Task.Delay(500);
            // Save IpToCompany to file
            HostToCompanyList.SaveToFile(outPath);
            Debug.WriteLine("File Saved.");
        }
    }

}