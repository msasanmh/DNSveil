using System.IO;
using System.Net;
using System.Text;
using MsmhToolsClass;

namespace DNSveil.Logic;

public class Pathes
{
    private static readonly bool IsPortable = true;

    // App Name
    private const string AppName = "DNSveil";

    // Binaries Directory Name
    private const string BinariesDirName = "Binary";

    // User Data Directory Name
    private const string UDN = "UserData";

    // Assets Directory In User Data Directory
    private const string AssetDirName = "Assets";

    // Certificates Directory In User Data Directory
    private const string CertificateDirName = "Certificate";

    // App Name Without Extension
    private static readonly string AppNameNoExtension = GetFileNameWithoutExtension(Environment.ProcessPath);

    // App Directory Path
    public static readonly string CurrentPath = GetFullPath(AppContext.BaseDirectory);

    public static readonly string CurrentExecutablePath = GetFullPath(Environment.ProcessPath);

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

    public static string UserDataDir
    {
        get
        {
            if (IsPortable) return GetFullPath(GetParent, UDN);
            else
            {
                try
                {
                    string appDataLocal = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                    return GetFullPath(appDataLocal, AppName, UDN);
                }
                catch (Exception)
                {
                    string msgErr = $"System AppData Folder Is Not Reachable.{Environment.NewLine}";
                    FileDirectory.AppendTextLine(ErrorLog, msgErr, new UTF8Encoding(false));
                    Environment.Exit(0);
                    return string.Empty;
                }
            }
        }
    }

    // Binaries Path
    public static readonly string BinaryDirPath = GetFullPath(CurrentPath, BinariesDirName);
    public static readonly string DnsLookup = GetFullPath(CurrentPath, BinariesDirName, "dnslookup.exe");
    public static readonly string RandomPath = GetRandomPath();
    public static readonly string SDCLookup = GetFullPath(CurrentPath, BinariesDirName, "SDCLookup.exe");
    public static readonly string AgnosticServer = GetFullPath(CurrentPath, BinariesDirName, "SDCAgnosticServer.exe");

    // Settings Path
    public static readonly string FirstRun = GetFullPath(UserDataDir, "FirstRun.txt");
    public static readonly string SettingsXml = GetFullPath(UserDataDir, AppNameNoExtension + ".xml"); // AgnosticSettings XML path
    public static readonly string SettingsXmlDnsLookup = GetFullPath(UserDataDir, "DnsLookupSettings.xml");
    public static readonly string SettingsXmlIpScanner = GetFullPath(UserDataDir, "IpScannerSettings.xml");
    public static readonly string SettingsXmlDnsScanner = GetFullPath(UserDataDir, "DnsScannerSettings.xml");
    public static readonly string UserId = GetFullPath(UserDataDir, "uid.txt");
    public static readonly string DnsServers_BuiltIn = GetFullPath(UserDataDir, "DnsServers_BuiltIn.xml");
    public static readonly string DnsServers_User = GetFullPath(UserDataDir, "DnsServers_User.xml");
    public static readonly string NicName = GetFullPath(UserDataDir, "NicName.txt");
    public static readonly string LogWindow = GetFullPath(UserDataDir, "LogWindow.txt");
    public static readonly string CloseStatus = GetFullPath(UserDataDir, "CloseStatus.txt");
    public static readonly string ErrorLog = GetFullPath(UserDataDir, "ErrorLog.txt");

    // Rules & Assets
    public static readonly string Rules = GetFullPath(UserDataDir, "Rules.txt");
    public static readonly string DnsRules = GetFullPath(UserDataDir, "DnsRules.txt");
    public static readonly string ProxyRules = GetFullPath(UserDataDir, "ProxyRules.txt");
    public static readonly string Rules_Assets_DNS = GetFullPath(UserDataDir, "Rules_Assets_DNS.tmp");
    public static readonly string Rules_Assets_Proxy = GetFullPath(UserDataDir, "Rules_Assets_Proxy.tmp");
    public static readonly string AssetDir = GetFullPath(UserDataDir, AssetDirName);
    public static readonly string Asset_Local_CIDRs = GetFullPath(AssetDir, "Local_CIDRs.txt");
    public static readonly string Asset_Cloudflare_CIDRs = GetFullPath(AssetDir, "Cloudflare_CIDRs.txt");
    public static readonly string Asset_IR_Domains = GetFullPath(AssetDir, "IR_Domains.txt");
    public static readonly string Asset_IR_CIDRs = GetFullPath(AssetDir, "IR_CIDRs.txt");
    public static readonly string Asset_IR_ADS_Domains = GetFullPath(AssetDir, "IR_ADS_Domains.txt");

    // Certificates Path
    public static readonly string CertificateDir = GetFullPath(UserDataDir, CertificateDirName);
    public static readonly string IssuerKey = GetFullPath(CertificateDir, "rootCA.key");
    public static readonly string IssuerCert = GetFullPath(CertificateDir, "rootCA.crt");
    public static readonly string LocalKey = GetFullPath(CertificateDir, "localhost.key");
    public static readonly string LocalCert = GetFullPath(CertificateDir, "localhost.crt");

    // Certificate Subject Names
    public static readonly string CertIssuerSubjectName = $"{AppName} Authority";
    public static readonly string CertSubjectName = AppName;

    // Bootstrap And Fallback
    public static readonly IPAddress BootstrapDnsIPv4 = IPAddress.Parse("8.8.8.8");
    public static readonly IPAddress BootstrapDnsIPv6 = IPAddress.Parse("2001:4860:4860::8888");
    public static readonly int BootstrapDnsPort = 53;
    public static readonly IPAddress FallbackDnsIPv4 = IPAddress.Parse("8.8.8.8");
    public static readonly IPAddress FallbackDnsIPv6 = IPAddress.Parse("2001:4860:4860::8888");
    public static readonly int FallbackDnsPort = 53;

    // Windows Firewall Rule Names
    public static readonly string FirewallRule_DNSveilIn = $"{AppName} IN";
    public static readonly string FirewallRule_DNSveilOut = $"{AppName} OUT";
    public static readonly string FirewallRule_DNSveilLookupIn = $"{AppName} Lookup IN";
    public static readonly string FirewallRule_DNSveilLookupOut = $"{AppName} Lookup OUT";
    public static readonly string FirewallRule_DNSveilAgnosticServerIn = $"{AppName} AgnosticServer IN";
    public static readonly string FirewallRule_DNSveilAgnosticServerOut = $"{AppName} AgnosticServer OUT";
    public static readonly string FirewallRule_DNSveilDnsLookupIn = $"{AppName} DnsLookup IN";
    public static readonly string FirewallRule_DNSveilDnsLookupOut = $"{AppName} DnsLookup OUT";

    private static string GetFileNameWithoutExtension(string? path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                string msgErr = $"GetFileNameWithoutExtension:{Environment.NewLine}Path Is NULL{Environment.NewLine}";
                FileDirectory.AppendTextLine(ErrorLog, msgErr, new UTF8Encoding(false));
                Environment.Exit(0);
                return string.Empty;
            }
            return Path.GetFileNameWithoutExtension(path);
        }
        catch (Exception ex)
        {
            string msgErr = $"GetFileNameWithoutExtension:{Environment.NewLine}{ex.Message}{Environment.NewLine}";
            FileDirectory.AppendTextLine(ErrorLog, msgErr, new UTF8Encoding(false));
            Environment.Exit(0);
            return string.Empty;
        }
    }

    private static string GetFullPath(string? path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                string msgErr = $"GetFullPath1:{Environment.NewLine}Path Is NULL{Environment.NewLine}";
                FileDirectory.AppendTextLine(ErrorLog, msgErr, new UTF8Encoding(false));
                Environment.Exit(0);
                return string.Empty;
            }
            return Path.GetFullPath(path);
        }
        catch (Exception ex)
        {
            string msgErr = $"GetFullPath1:{Environment.NewLine}{ex.Message}{Environment.NewLine}";
            FileDirectory.AppendTextLine(ErrorLog, msgErr, new UTF8Encoding(false));
            Environment.Exit(0);
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
            string msgErr = $"GetFullPath2:{Environment.NewLine}{ex.Message}{Environment.NewLine}";
            FileDirectory.AppendTextLine(ErrorLog, msgErr, new UTF8Encoding(false));
            Environment.Exit(0);
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
            string msgErr = $"GetFullPath3:{Environment.NewLine}{ex.Message}{Environment.NewLine}";
            FileDirectory.AppendTextLine(ErrorLog, msgErr, new UTF8Encoding(false));
            Environment.Exit(0);
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
            string msgErr = $"GetRandomPath:{Environment.NewLine}{ex.Message}{Environment.NewLine}";
            FileDirectory.AppendTextLine(ErrorLog, msgErr, new UTF8Encoding(false));
            Environment.Exit(0);
            return string.Empty;
        }
    }
}