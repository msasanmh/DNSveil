using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MsmhToolsClass.MsmhProxyServer;

public class SettingsSSL
{
    public bool EnableSSL { get; set; } = false;
    public X509Certificate2 RootCA { get; private set; }
    public string? RootCA_Path { get; set; }
    public string? RootCA_KeyPath { get; set; }
    public bool ChangeSniToIP { get; set; } = true;

    public SettingsSSL(bool enableSSL)
    {
        EnableSSL = enableSSL;
        RootCA = new(Array.Empty<byte>());
        if (!EnableSSL)
        {
            RootCA.Dispose();
            return;
        }
    }

    public async Task Build()
    {
        if (!EnableSSL) return;

        try
        {
            if (!string.IsNullOrEmpty(RootCA_Path) && File.Exists(RootCA_Path))
            {
                // Read From File
                X509Certificate2? rootCACert = BuildByFile(RootCA_Path, RootCA_KeyPath);
                if (rootCACert != null) RootCA = new(rootCACert);
            }
            else
            {
                string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "RootCA"));
                string rootCA_Path = path + ".crt";
                string rootCA_KeyPath = path + ".key";

                string issuerSubjectName = "Msmh Proxy Server Authority";
                X509Certificate2? rootCACert;

                // Check IF Cert Exist
                if (File.Exists(rootCA_Path) && File.Exists(rootCA_KeyPath))
                {
                    // Read From File
                    rootCACert = BuildByFile(rootCA_Path, rootCA_KeyPath);
                }
                else
                {
                    try
                    {
                        if (File.Exists(rootCA_Path)) File.Delete(rootCA_Path);
                        if (File.Exists(rootCA_KeyPath)) File.Delete(rootCA_KeyPath);
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }

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
                    IPAddress gateway = NetworkTool.GetDefaultGateway() ?? IPAddress.Loopback;
                    rootCACert = CertificateTool.GenerateRootCertificate(gateway, issuerSubjectName, out RSA privateKey);

                    // Save Cert To File
                    await rootCACert.SaveToFileAsCrt(path);

                    // Save Private Key To File
                    await privateKey.SavePrivateKeyToFile(path);

                    string pass = Guid.NewGuid().ToString();
                    if (!rootCACert.HasPrivateKey)
                        rootCACert = rootCACert.CopyWithPrivateKey(privateKey);
                    rootCACert = new(rootCACert.Export(X509ContentType.Pfx, pass), pass);
                }

                if (rootCACert != null)
                {
                    RootCA_Path = rootCA_Path;
                    RootCA_KeyPath = rootCA_KeyPath;

                    RootCA = new(rootCACert);
                }
            }
        }
        catch (Exception ex)
        {
            RootCA.Dispose();
            EnableSSL = false;
            Debug.WriteLine("SettingsSSL: " + ex.Message);
        }

        // Check for "m_safeCertContext is an invalid handle"
        try
        {
            _ = RootCA.Subject;
        }
        catch (Exception)
        {
            RootCA.Dispose();
            EnableSSL = false;
        }

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
                    EnableSSL = false;
                }
            }
        }
    }

    private X509Certificate2? BuildByFile(string rootCA_Path, string? rootCA_KeyPath)
    {
        try
        {
            X509Certificate2 rootCACert = new(X509Certificate2.CreateFromCertFile(rootCA_Path));

            string pass = Guid.NewGuid().ToString();
            if (!rootCACert.HasPrivateKey && !string.IsNullOrEmpty(rootCA_KeyPath) && File.Exists(rootCA_KeyPath))
            {
                RSA rootCAKey = RSA.Create();
                rootCAKey.ImportFromPem(File.ReadAllText(rootCA_KeyPath).ToCharArray());
                rootCACert = rootCACert.CopyWithPrivateKey(rootCAKey);
            }
            rootCACert = new(rootCACert.Export(X509ContentType.Pfx, pass), pass);
            return new(rootCACert);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SettingsSSL BuildByFile: " + ex.Message);
            return null;
        }
    }
}