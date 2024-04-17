using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MsmhToolsClass.MsmhAgnosticServer;

public class AgnosticSettingsSSL
{
    public bool EnableSSL { get; set; } = false;
    public X509Certificate2 RootCA { get; private set; }
    public string? RootCA_Path { get; set; }
    public string? RootCA_KeyPath { get; set; }
    public X509Certificate2 Cert { get; private set; }
    public string? Cert_Path { get; set; }
    public string? Cert_KeyPath { get; set; }
    public bool ChangeSni { get; set; } = true;
    public string DefaultSni { get; set; } = string.Empty;

    public AgnosticSettingsSSL(bool enableSSL)
    {
        EnableSSL = enableSSL;
        RootCA = new(Array.Empty<byte>());
        Cert = new(Array.Empty<byte>());
        if (!EnableSSL)
        {
            try
            {
                RootCA.Dispose();
                Cert.Dispose();
            }
            catch (Exception) { }
            return;
        }
    }

    public async Task Build()
    {
        if (!EnableSSL) return;

        try
        {
            if (!string.IsNullOrEmpty(RootCA_Path) && File.Exists(RootCA_Path) &&
                !string.IsNullOrEmpty(Cert_Path) && File.Exists(Cert_Path))
            {
                // Read From File
                X509Certificate2? rootCA = BuildByFile(RootCA_Path, RootCA_KeyPath, true);
                if (rootCA != null) RootCA = new(rootCA);

                X509Certificate2? cert = BuildByFile(Cert_Path, Cert_KeyPath, false);
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
                if (File.Exists(issuerCertPath) && File.Exists(issuerKeyPath) &&
                    File.Exists(certPath) && File.Exists(keyPath))
                {
                    // Read From File
                    rootCACert = BuildByFile(issuerCertPath, issuerKeyPath, true);
                    cert = BuildByFile(certPath, keyPath, false);
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
                    // Create certificate directory
                    FileDirectory.CreateEmptyDirectory(certificateDirPath);
                    // It is overwritten, no need to delete.
                    IPAddress? gateway = NetworkTool.GetDefaultGateway() ?? IPAddress.Loopback;
                    await CertificateTool.GenerateCertificateAsync(certificateDirPath, gateway, issuerSubjectName, subjectName);
                    CertificateTool.CreateP12(issuerCertPath, issuerKeyPath);
                    CertificateTool.CreateP12(certPath, keyPath);

                    if (File.Exists(issuerCertPath) && File.Exists(issuerKeyPath) &&
                        File.Exists(certPath) && File.Exists(keyPath))
                    {
                        // Read From File
                        rootCACert = BuildByFile(issuerCertPath, issuerKeyPath, true);
                        cert = BuildByFile(certPath, keyPath, false);
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
            RootCA.Dispose();
            EnableSSL = false;
            Debug.WriteLine("AgnosticSettingsSSL: " + ex.Message);
        }

        // Check for "m_safeCertContext is an invalid handle"
        try
        {
            _ = RootCA.Subject;
            _ = Cert.Subject;
        }
        catch (Exception)
        {
            RootCA.Dispose();
            Cert.Dispose();
            EnableSSL = false;
        }

        try
        {
            // Install RootCA
            if (EnableSSL && !string.IsNullOrEmpty(RootCA_Path) && File.Exists(RootCA_Path))
            {
                // Check If Cert is Installed
                bool isInstalled = CertificateTool.IsCertificateInstalled(RootCA.Subject, StoreName.Root, StoreLocation.CurrentUser);
                if (!isInstalled)
                {
                    // Install Cert
                    bool certInstalled = CertificateTool.InstallCertificate(RootCA_Path, StoreName.Root, StoreLocation.CurrentUser);
                    if (!certInstalled)
                    {
                        // User refused to install cert
                        RootCA.Dispose();
                        Cert.Dispose();
                        EnableSSL = false;
                    }
                }
            }
        }
        catch (Exception) { }
    }

    private static X509Certificate2? BuildByFile(string cert_Path, string? cert_KeyPath, bool isRootCA)
    {
        try
        {
            X509Certificate2 cert = new(X509Certificate2.CreateFromCertFile(cert_Path));

            if (!cert.HasPrivateKey && !string.IsNullOrEmpty(cert_KeyPath) && File.Exists(cert_KeyPath))
            {
                RSA key = RSA.Create();
                key.ImportFromPem(File.ReadAllText(cert_KeyPath).ToCharArray());
                cert = cert.CopyWithPrivateKey(key);
            }
            string pass = Guid.NewGuid().ToString();
            cert = new(cert.Export(X509ContentType.Pfx, pass), pass);
            return new(cert);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AgnosticSettingsSSL BuildByFile: " + ex.Message);
            return null;
        }
    }

}