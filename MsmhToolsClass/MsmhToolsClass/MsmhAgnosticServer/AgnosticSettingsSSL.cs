using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class AgnosticSettingsSSL
{
    public bool EnableSSL { get; set; } = false;
    /// <summary>
    /// Server Domain Name To Distinguish DoH Requests From SNI Requests On DnsAndProxy Mode.
    /// </summary>
    public string ServerDomainName { get; set; } = IPAddress.Loopback.ToString();
    public X509Certificate2? RootCA { get; private set; }
    public string? RootCA_Path { get; set; }
    public string? RootCA_KeyPath { get; set; }
    public X509Certificate2? Cert { get; private set; }
    public string? Cert_Path { get; set; }
    public string? Cert_KeyPath { get; set; }
    public bool ChangeSni { get; set; } = true;
    public string DefaultSni { get; set; } = string.Empty;

    public AgnosticSettingsSSL(bool enableSSL)
    {
        EnableSSL = enableSSL;
    }

    public async Task BuildAsync()
    {
        if (ChangeSni)
        {
            await BuildSelfSigned_Async();
        }
        else
        {
            await BuildCertsByUser_Async();
        }
    }

    private async Task BuildSelfSigned_Async()
    {
        if (!EnableSSL) return;

        try
        {
            if (!string.IsNullOrEmpty(RootCA_Path) && File.Exists(RootCA_Path) && !string.IsNullOrEmpty(Cert_Path) && File.Exists(Cert_Path))
            {
                // Read From File
                X509Certificate2? rootCA = await BuildByFileAsync(RootCA_Path, RootCA_KeyPath, true);
                if (rootCA != null) RootCA = new(rootCA);

                X509Certificate2? cert = await BuildByFileAsync(Cert_Path, Cert_KeyPath, false);
                if (cert != null) Cert = new(cert);
            }
            else
            {
                string certificateDirPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "certificate"));
                string issuerCertPath = Path.GetFullPath(Path.Combine(certificateDirPath, "rootCA.crt"));
                string issuerKeyPath = Path.GetFullPath(Path.Combine(certificateDirPath, "rootCA.key"));
                string certPath = Path.GetFullPath(Path.Combine(certificateDirPath, "localhost.crt"));
                string keyPath = Path.GetFullPath(Path.Combine(certificateDirPath, "localhost.key"));

                string issuerSubjectName = "Msmh Agnostic Server Authority";
                string subjectName = "Msmh Agnostic Server";

                X509Certificate2? rootCACert = null;
                X509Certificate2? cert = null;

                // Check IF Cert Exist
                if (File.Exists(issuerCertPath) && File.Exists(issuerKeyPath) && File.Exists(certPath) && File.Exists(keyPath))
                {
                    // Read From File
                    rootCACert = await BuildByFileAsync(issuerCertPath, issuerKeyPath, true);
                    cert = await BuildByFileAsync(certPath, keyPath, false);
                }
                else
                {
                    try
                    {
                        if (File.Exists(issuerCertPath)) File.Delete(issuerCertPath);
                        if (File.Exists(issuerKeyPath)) File.Delete(issuerKeyPath);
                        if (File.Exists(certPath)) File.Delete(certPath);
                        if (File.Exists(keyPath)) File.Delete(keyPath);
                    }
                    catch (Exception) { }

                    bool isInstalled = CertificateTool.IsCertificateInstalled(issuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);
                    if (isInstalled)
                    {
                        while (true)
                        {
                            bool uninstalled = CertificateTool.UninstallCertificate(issuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);
                            if (uninstalled) break;
                        }
                    }

                    // Generate
                    // Create Certificate Directory
                    FileDirectory.CreateEmptyDirectory(certificateDirPath);
                    // It Is Overwritten, No Need To Delete.
                    IPAddress? gateway = NetworkTool.GetDefaultGateway() ?? IPAddress.Loopback;
                    await CertificateTool.GenerateCertificateAsync(certificateDirPath, gateway, issuerSubjectName, subjectName);
                    CertificateTool.CreateP12(issuerCertPath, issuerKeyPath);
                    CertificateTool.CreateP12(certPath, keyPath);

                    if (File.Exists(issuerCertPath) && File.Exists(issuerKeyPath) && File.Exists(certPath) && File.Exists(keyPath))
                    {
                        // Read From File
                        rootCACert = await BuildByFileAsync(issuerCertPath, issuerKeyPath, true);
                        cert = await BuildByFileAsync(certPath, keyPath, false);
                    }
                }

                if (rootCACert != null && cert != null)
                {
                    RootCA_Path = issuerCertPath;
                    RootCA_KeyPath = issuerKeyPath;
                    Cert_Path = certPath;
                    Cert_KeyPath = keyPath;

                    RootCA = new(rootCACert);
                    Cert = new(cert);
                }
            }
        }
        catch (Exception ex)
        {
            EnableSSL = false;
            Debug.WriteLine("AgnosticSettingsSSL BuildSelfSigned_Async: " + ex.Message);
        }

        // Check for "m_safeCertContext is an invalid handle"
        try
        {
            _ = RootCA?.Subject;
            _ = Cert?.Subject;
        }
        catch (Exception)
        {
            EnableSSL = false;
        }

        try
        {
            // Install RootCA
            if (EnableSSL && RootCA != null && !string.IsNullOrEmpty(RootCA_Path) && File.Exists(RootCA_Path))
            {
                // Check If Cert Is Installed
                bool isInstalled = CertificateTool.IsCertificateInstalled(RootCA.Subject, StoreName.Root, StoreLocation.CurrentUser);
                if (!isInstalled)
                {
                    // Install Cert
                    bool certInstalled = CertificateTool.InstallCertificate(RootCA_Path, StoreName.Root, StoreLocation.CurrentUser);
                    if (!certInstalled)
                    {
                        // User Refused To Install Root Cert
                        EnableSSL = false;
                    }
                }
            }
        }
        catch (Exception) { }

        if (!EnableSSL)
        {
            try
            {
                RootCA?.Dispose();
                Cert?.Dispose();
            }
            catch (Exception) { }
        }
    }

    private async Task BuildCertsByUser_Async()
    {
        if (!EnableSSL) return;

        try
        {
            if (!string.IsNullOrEmpty(Cert_Path) && File.Exists(Cert_Path))
            {
                // Read From File
                X509Certificate2? cert = await BuildByFileAsync(Cert_Path, Cert_KeyPath, false);
                if (cert != null) Cert = new(cert);
                else EnableSSL = false;
            }
            else
            {
                EnableSSL = false;
                Debug.WriteLine("AgnosticSettingsSSL BuildCertsByUser_Async: Cert File Not Exist.");
            }
        }
        catch (Exception ex)
        {
            EnableSSL = false;
            Debug.WriteLine("AgnosticSettingsSSL BuildCertsByUser_Async: " + ex.Message);
        }

        if (!EnableSSL)
        {
            try
            {
                RootCA?.Dispose();
                Cert?.Dispose();
            }
            catch (Exception) { }
        }
    }

    private static async Task<X509Certificate2?> BuildByFileAsync(string cert_Path, string? cert_KeyPath, bool isRootCA)
    {
        try
        {
            X509Certificate2 cert = new(X509Certificate2.CreateFromCertFile(cert_Path));

            if (!cert.HasPrivateKey && !string.IsNullOrEmpty(cert_KeyPath) && File.Exists(cert_KeyPath))
            {
                RSA key = RSA.Create();
                string keyStr = await File.ReadAllTextAsync(cert_KeyPath);
                key.ImportFromPem(keyStr.ToCharArray());
                cert = cert.CopyWithPrivateKey(key);
            }

            string pass = Guid.NewGuid().ToString();
            cert = new(cert.Export(X509ContentType.Pfx, pass), pass);
            //cert = new(cert.Export(X509ContentType.Pfx, pass), pass, X509KeyStorageFlags.MachineKeySet);
            return new(cert);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AgnosticSettingsSSL BuildByFileAsync: IsRootCA: {isRootCA}");
            Debug.WriteLine("AgnosticSettingsSSL BuildByFileAsync: " + ex.Message);
            return null;
        }
    }

}