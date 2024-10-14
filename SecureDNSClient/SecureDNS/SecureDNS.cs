using System.Net;
using System.Diagnostics;
using CustomControls;
using System.Reflection;
using MsmhToolsClass;
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
    public static readonly string SDCLookupPath = GetFullPath(CurrentPath, "binary", "SDCLookup.exe");
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

    public static readonly string FirstRun = GetFullPath(UserDataDirPath, "FirstRun.txt");
    public static readonly string SettingsXmlPath = GetFullPath(UserDataDirPath, appNameWithoutExtension + ".xml"); // AgnosticSettings XML path
    public static readonly string SettingsXmlDnsLookup = GetFullPath(UserDataDirPath, "DnsLookupSettings.xml");
    public static readonly string SettingsXmlIpScanner = GetFullPath(UserDataDirPath, "IpScannerSettings.xml");
    public static readonly string SettingsXmlDnsScanner = GetFullPath(UserDataDirPath, "DnsScannerSettings.xml");
    public static readonly string SettingsXmlDnsScannerExport = GetFullPath(UserDataDirPath, "DnsScannerExportSettings.xml");
    public static readonly string UserIdPath = GetFullPath(UserDataDirPath, "uid.txt");
    public static readonly string BuiltInServersSecureUpdateUrl = "https://github.com/msasanmh/SecureDNSClient/raw/main/Subs/sdc-secure.txt";
    public static readonly string BuiltInServersSecurePath = GetFullPath(UserDataDirPath, "BuiltInServers_Secure.txt");
    public static readonly string BuiltInServersInsecureUpdateUrl = "https://github.com/msasanmh/SecureDNSClient/raw/main/Subs/sdc-insecure.txt";
    public static readonly string BuiltInServersInsecurePath = GetFullPath(UserDataDirPath, "BuiltInServers_Insecure.txt");
    public static readonly string CustomServersPath = GetFullPath(UserDataDirPath, "CustomServers.txt");
    public static readonly string CustomServersXmlPath = GetFullPath(UserDataDirPath, "CustomServers.xml");
    public static readonly string WorkingServersPath = GetFullPath(UserDataDirPath, "CustomServers_Working.txt");
    public static readonly string DPIBlacklistPath = GetFullPath(UserDataDirPath, "DPIBlacklist.txt"); // GoodbyeDPI Black List
    public static readonly string NicNamePath = GetFullPath(UserDataDirPath, "NicName.txt");
    public static readonly string SavedEncodedDnsPath = GetFullPath(UserDataDirPath, "SavedEncodedDns.txt");
    public static readonly string LogWindowPath = GetFullPath(UserDataDirPath, "LogWindow.txt");
    public static readonly string CloseStatusPath = GetFullPath(UserDataDirPath, "CloseStatus.txt");
    public static readonly string ErrorLogPath = GetFullPath(UserDataDirPath, "ErrorLog.txt");

    // Rules & Assets
    public static readonly string RulesPath = GetFullPath(UserDataDirPath, "Rules.txt");
    public static readonly string DnsRulesPath = GetFullPath(UserDataDirPath, "DnsRules.txt");
    public static readonly string ProxyRulesPath = GetFullPath(UserDataDirPath, "ProxyRules.txt");
    public static readonly string Rules_Assets_DNS = GetFullPath(UserDataDirPath, "Rules_Assets_DNS.tmp");
    public static readonly string Rules_Assets_Proxy = GetFullPath(UserDataDirPath, "Rules_Assets_Proxy.tmp");
    public static readonly string AssetDirPath = GetFullPath(UserDataDirPath, "Assets");
    public static readonly string Asset_Local_CIDRs = GetFullPath(AssetDirPath, "Local_CIDRs.txt");
    public static readonly string Asset_Cloudflare_CIDRs = GetFullPath(AssetDirPath, "Cloudflare_CIDRs.txt");
    public static readonly string Asset_IR_Domains = GetFullPath(AssetDirPath, "IR_Domains.txt");
    public static readonly string Asset_IR_CIDRs = GetFullPath(AssetDirPath, "IR_CIDRs.txt");
    public static readonly string Asset_IR_ADS_Domains = GetFullPath(AssetDirPath, "IR_ADS_Domains.txt");

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
    public static readonly string FirewallRule_SdcSDCLookupIn = "SDC SDCLookup IN";
    public static readonly string FirewallRule_SdcSDCLookupOut = "SDC SDCLookup OUT";
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

    // SDC Admin
    public static readonly string SDCPublicKey = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SDCPublicKey.bin"));
    public static readonly string SDCPrivateKey = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SDCPrivateKey.bin"));

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

}