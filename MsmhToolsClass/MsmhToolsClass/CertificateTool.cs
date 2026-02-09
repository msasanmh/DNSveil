using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

#nullable enable
namespace MsmhToolsClass;

public static class CertificateTool
{
    private const string CRT_HEADER = "-----BEGIN CERTIFICATE-----\n";
    private const string CRT_FOOTER = "\n-----END CERTIFICATE-----";
    private const string KEY_HEADER_PRIVATE = "-----BEGIN RSA PRIVATE KEY-----\n";
    private const string KEY_FOOTER_PRIVATE = "\n-----END RSA PRIVATE KEY-----";
    private const string KEY_HEADER_PUBLIC = "-----BEGIN RSA PUBLIC KEY-----\n";
    private const string KEY_FOOTER_PUBLIC = "\n-----END RSA PUBLIC KEY-----";

    //================= Extensions Begin
    public static async Task SaveToFileAsCrt(this X509Certificate2 x509Certificate2, string filePathWithoutExt)
    {
        try
        {
            byte[] certExport = x509Certificate2.Export(X509ContentType.Cert);
            string certData = Convert.ToBase64String(certExport, Base64FormattingOptions.InsertLineBreaks);
            await File.WriteAllTextAsync(Path.GetFullPath($"{filePathWithoutExt}.crt"), CRT_HEADER + certData + CRT_FOOTER);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SaveToFileAsCrt: " + ex.Message);
        }
    }

    public static async Task SaveToFileAsP12(this X509Certificate2 x509Certificate2, RSA? privateKey, string password, string filePathWithoutExt)
    {
        try
        {
            if (!x509Certificate2.HasPrivateKey && privateKey != null)
                x509Certificate2 = x509Certificate2.CopyWithPrivateKey(privateKey);

            byte[] certExport = x509Certificate2.Export(X509ContentType.Pfx, password);
            await File.WriteAllBytesAsync(Path.GetFullPath($"{filePathWithoutExt}.p12"), certExport);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SaveToFileAsP12: " + ex.Message);
        }
    }

    public static async Task SaveToFileAsP12(this X509Certificate2 x509Certificate2, ECDsa? privateKey, string password, string filePathWithoutExt)
    {
        try
        {
            if (!x509Certificate2.HasPrivateKey && privateKey != null)
                x509Certificate2 = x509Certificate2.CopyWithPrivateKey(privateKey);

            byte[] certExport = x509Certificate2.Export(X509ContentType.Pfx, password);
            await File.WriteAllBytesAsync(Path.GetFullPath($"{filePathWithoutExt}.p12"), certExport);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SaveToFileAsP12: " + ex.Message);
        }
    }

    public static async Task SaveToFileAsP12(this X509Certificate2 x509Certificate2, DSA? privateKey, string password, string filePathWithoutExt)
    {
        try
        {
            if (!x509Certificate2.HasPrivateKey && privateKey != null)
                x509Certificate2 = x509Certificate2.CopyWithPrivateKey(privateKey);

            byte[] certExport = x509Certificate2.Export(X509ContentType.Pfx, password);
            await File.WriteAllBytesAsync(Path.GetFullPath($"{filePathWithoutExt}.p12"), certExport);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SaveToFileAsP12: " + ex.Message);
        }
    }

    public static async Task SaveToFileAsP12(this X509Certificate2 x509Certificate2, ECDiffieHellman? privateKey, string password, string filePathWithoutExt)
    {
        try
        {
            if (!x509Certificate2.HasPrivateKey && privateKey != null)
                x509Certificate2 = x509Certificate2.CopyWithPrivateKey(privateKey);

            byte[] certExport = x509Certificate2.Export(X509ContentType.Pfx, password);
            await File.WriteAllBytesAsync(Path.GetFullPath($"{filePathWithoutExt}.p12"), certExport);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SaveToFileAsP12: " + ex.Message);
        }
    }

    public static async Task SavePrivateKeyToFile(this RSA rsa, string filePathWithoutExt)
    {
        try
        {
            byte[] privateKeyExport = rsa.ExportRSAPrivateKey();
            string privateKeyData = Convert.ToBase64String(privateKeyExport, Base64FormattingOptions.InsertLineBreaks);
            await File.WriteAllTextAsync(Path.GetFullPath($"{filePathWithoutExt}.key"), KEY_HEADER_PRIVATE + privateKeyData + KEY_FOOTER_PRIVATE);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SavePrivateKeyToFile: " + ex.Message);
        }
    }

    public static async Task SavePublicKeyToFile(this RSA rsa, string filePathWithoutExt)
    {
        try
        {
            byte[] publicKeyExport = rsa.ExportRSAPublicKey();
            string publicKeyData = Convert.ToBase64String(publicKeyExport, Base64FormattingOptions.InsertLineBreaks);
            await File.WriteAllTextAsync(Path.GetFullPath($"{filePathWithoutExt}.key"), KEY_HEADER_PUBLIC + publicKeyData + KEY_FOOTER_PUBLIC);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SavePublicKeyToFile: " + ex.Message);
        }
    }
    //================= Extensions End

    private static SubjectAlternativeNameBuilder GetSanBuilderForGateway(IPAddress gateway)
    {
        // Create SubjectAlternativeNameBuilder
        SubjectAlternativeNameBuilder sanBuilder = new();

        try
        {
            sanBuilder.AddDnsName("localhost"); // Add Localhost
            sanBuilder.AddDnsName(Environment.UserName); // Add Current User
            if (OperatingSystem.IsWindows())
                sanBuilder.AddUserPrincipalName(System.Security.Principal.WindowsIdentity.GetCurrent().Name); // Add User Principal Name
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddIpAddress(IPAddress.Any);
            sanBuilder.AddIpAddress(IPAddress.IPv6Any);

            // Add All Machine IPv4 And IPv6 Configuration To SAN Extension
            foreach (var ipAddress in Dns.GetHostAddresses(Dns.GetHostName()))
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork || ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    sanBuilder.AddIpAddress(ipAddress);

            // Generate IP range for gateway
            if (NetworkTool.IsIPv4(gateway))
            {
                string ipString = gateway.ToString();
                string[] ipSplit = ipString.Split('.');
                string ip1 = ipSplit[0] + "." + ipSplit[1] + "." + ipSplit[2] + ".";
                for (int n = 0; n <= 255; n++)
                {
                    string ip2 = ip1 + n.ToString();
                    sanBuilder.AddIpAddress(IPAddress.Parse(ip2));
                    sanBuilder.AddUri(new Uri($"https://{ip2}"));
                }
                // Generate local IP range in case a VPN is active.
                if (!ip1.Equals("192.168.1."))
                {
                    string ipLocal1 = "192.168.1.";
                    for (int n = 0; n <= 255; n++)
                    {
                        string ipLocal2 = ipLocal1 + n.ToString();
                        sanBuilder.AddIpAddress(IPAddress.Parse(ipLocal2));
                        sanBuilder.AddUri(new Uri($"https://{ipLocal2}"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CertificateTool GetSanBuilderForGateway: " + ex.Message);
        }

        return sanBuilder;
    }

    private static SubjectAlternativeNameBuilder GetSanBuilderForDomain(List<string> domains)
    {
        // Create SubjectAlternativeNameBuilder
        SubjectAlternativeNameBuilder sanBuilder = new();

        for (int n = 0; n < domains.Count; n++)
        {
            string domain = domains[n];
            bool isIp = IPAddress.TryParse(domain, out IPAddress? ip);
            if (isIp && ip != null)
                sanBuilder.AddIpAddress(ip);
            else
                sanBuilder.AddDnsName(domain);
        }

        return sanBuilder;
    }

    private const X509KeyUsageFlags GetX509KeyUsageFlags = X509KeyUsageFlags.CrlSign |
                                                           X509KeyUsageFlags.DataEncipherment |
                                                           X509KeyUsageFlags.DigitalSignature |
                                                           X509KeyUsageFlags.KeyAgreement |
                                                           X509KeyUsageFlags.KeyCertSign |
                                                           X509KeyUsageFlags.KeyEncipherment |
                                                           X509KeyUsageFlags.NonRepudiation;

    private static OidCollection GetOidCollection()
    {
        // Create Oid Collection
        OidCollection oidCollection = new()
        {
            new Oid("2.5.29.37.0"), // Any Purpose
            new Oid("1.3.6.1.5.5.7.3.1"), // Server Authentication
            new Oid("1.3.6.1.5.5.7.3.2"), // Client Authentication
            new Oid("1.3.6.1.5.5.7.3.3"), // Code Signing
            new Oid("1.3.6.1.5.5.7.3.4"), // Email Protection
            new Oid("1.3.6.1.5.5.7.3.5"), // IPSEC End System Certificate
            new Oid("1.3.6.1.5.5.7.3.6"), // IPSEC Tunnel
            new Oid("1.3.6.1.5.5.7.3.7"), // IPSEC User Certificate
            new Oid("1.3.6.1.5.5.7.3.8"), // Time Stamping
            new Oid("1.3.6.1.4.1.311.10.3.2"), // Microsoft Time Stamping
            new Oid("1.3.6.1.4.1.311.10.5.1"), // Digital Rights
            new Oid("1.3.6.1.4.1.311.64.1.1") // Domain Name System (DNS) Server Trust
        };
        return oidCollection;
    }

    public static X509Certificate2 GenerateRootCertificate(SubjectAlternativeNameBuilder sanBuilder, string issuerSubjectName, out RSA privateKey)
    {
        issuerSubjectName = $"CN={issuerSubjectName}";

        // Create Issuer RSA Private Key
        RSA issuerRsaKey = RSA.Create(4096);
        privateKey = issuerRsaKey;

        // Create Issuer Request
        CertificateRequest issuerReq = new(issuerSubjectName, issuerRsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        issuerReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, int.MaxValue, true));
        issuerReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(issuerReq.PublicKey, false));
        issuerReq.CertificateExtensions.Add(new X509KeyUsageExtension(GetX509KeyUsageFlags, false));
        issuerReq.CertificateExtensions.Add(sanBuilder.Build());
        issuerReq.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(GetOidCollection(), true));

        // Create Issuer Certificate
        return issuerReq.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));
    }

    public static X509Certificate2 GenerateRootCertificate(List<string> domains, string issuerSubjectName, out RSA privateKey)
    {
        // Create SubjectAlternativeNameBuilder
        SubjectAlternativeNameBuilder sanBuilder = GetSanBuilderForDomain(domains);

        return GenerateRootCertificate(sanBuilder, issuerSubjectName, out privateKey);
    }

    public static X509Certificate2 GenerateRootCertificate(IPAddress gateway, string issuerSubjectName, out RSA privateKey)
    {
        // Create SubjectAlternativeNameBuilder
        SubjectAlternativeNameBuilder sanBuilder = GetSanBuilderForGateway(gateway);

        return GenerateRootCertificate(sanBuilder, issuerSubjectName, out privateKey);
    }

    public static X509Certificate2 GenerateCertificateByIssuer(X509Certificate2 issuerCert, SubjectAlternativeNameBuilder sanBuilder, string subjectName, out RSA privateKey)
    {
        string cn = "CN=";
        string o = "O=";
        string issuerSubject = issuerCert.Subject;
        if (issuerSubject.StartsWith(cn)) issuerSubject = issuerSubject.TrimStart(cn);
        subjectName = $"{cn}{subjectName},{o}{issuerSubject}";

        DateTime issuerNotAfter = issuerCert.NotAfter;
        DateTime notAfter = issuerNotAfter.AddDays(-1);

        // Create RSA Private Key
        RSA rsaKey = RSA.Create(4096);
        privateKey = rsaKey;

        // Create Request
        CertificateRequest req = new(subjectName, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));
        req.CertificateExtensions.Add(new X509KeyUsageExtension(GetX509KeyUsageFlags, false));
        req.CertificateExtensions.Add(sanBuilder.Build());
        req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(GetOidCollection(), true));

        // Create Certificate (Serial Number Must Be Unique)
        byte[] sn = Guid.NewGuid().ToByteArray();
        return req.Create(issuerCert, DateTimeOffset.Now, notAfter, sn);
    }

    public static X509Certificate2 GenerateCertificateByIssuer(X509Certificate2 issuerCert, List<string> domains, string subjectName, out RSA privateKey)
    {
        // Create SubjectAlternativeNameBuilder
        SubjectAlternativeNameBuilder sanBuilder = GetSanBuilderForDomain(domains);

        return GenerateCertificateByIssuer(issuerCert, sanBuilder, subjectName, out privateKey);
    }

    public static X509Certificate2 GenerateCertificateByIssuer(X509Certificate2 issuerCert, IPAddress gateway, string subjectName, out RSA privateKey)
    {
        // Create SubjectAlternativeNameBuilder
        SubjectAlternativeNameBuilder sanBuilder = GetSanBuilderForGateway(gateway);

        return GenerateCertificateByIssuer(issuerCert, sanBuilder, subjectName, out privateKey);
    }

    public static async Task GenerateCertificateAsync(string folderPath, IPAddress gateway, string issuerSubjectName = "MSasanMH Authority", string subjectName = "MSasanMH")
    {
        // Create SubjectAlternativeNameBuilder
        SubjectAlternativeNameBuilder sanBuilder = GetSanBuilderForGateway(gateway);

        // Create Issuer Certificate
        X509Certificate2 issuerCert = GenerateRootCertificate(sanBuilder, issuerSubjectName, out RSA issuerRsaKey);
        //if (!issuerCert.HasPrivateKey) issuerCert = issuerCert.CopyWithPrivateKey(issuerRsaKey);
        //string pass = Guid.NewGuid().ToString();
        //issuerCert = new(issuerCert.Export(X509ContentType.Pfx, pass), pass);

        // Create Certificate
        X509Certificate2 cert = GenerateCertificateByIssuer(issuerCert, sanBuilder, subjectName, out RSA rsaKey);
        //if (!cert.HasPrivateKey) cert = cert.CopyWithPrivateKey(rsaKey);
        //cert = new(cert.Export(X509ContentType.Pfx, pass), pass);

        //== Export to Files
        // Export Issuer Private Key
        string issuerFilePathNoExt = Path.GetFullPath(Path.Combine(folderPath, "rootCA"));
        await issuerRsaKey.SavePrivateKeyToFile(issuerFilePathNoExt);
        issuerRsaKey.Dispose();

        // Export Issuer Certificate
        await issuerCert.SaveToFileAsCrt(issuerFilePathNoExt);
        issuerCert.Dispose();

        // Export Private Key
        string filePathNoExt = Path.GetFullPath(Path.Combine(folderPath, "localhost"));
        await rsaKey.SavePrivateKeyToFile(filePathNoExt);
        rsaKey.Dispose();

        // Export Certificate
        await cert.SaveToFileAsCrt(filePathNoExt);
        cert.Dispose();
    }

    public static async void CreateP12(string certPath, string keyPath, string password = "")
    {
        try
        {
            string? folderPath = Path.GetDirectoryName(certPath);
            string fileName = Path.GetFileNameWithoutExtension(certPath);
            using X509Certificate2 certWithKey = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            byte[] certWithKeyExport = certWithKey.Export(X509ContentType.Pfx, password);
            if (!string.IsNullOrEmpty(folderPath))
                await File.WriteAllBytesAsync(Path.Combine(folderPath, fileName + ".p12"), certWithKeyExport);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CreateP12: " + ex.Message);
        }
    }

    /// <summary>
    /// Returns false if user don't install certificate, otherwise true.
    /// </summary>
    public static bool InstallCertificate(X509Certificate2 certificate, StoreName storeName, StoreLocation storeLocation)
    {
        try
        {
            bool isCertInstalled = IsCertificateInstalled(certificate, storeName, storeLocation);
            if (!isCertInstalled)
            {
                X509Store store = new(storeName, storeLocation);
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
                store.Close();
            }

            certificate.Dispose();
            return true;
        }
        catch (Exception ex) // Internal.Cryptography.CryptoThrowHelper.WindowsCryptographicException
        {
            Debug.WriteLine("InstallCertificate: " + ex.Message);
            // If ex.Message: (The operation was canceled by the user.)
            return false;
        }
    }

    /// <summary>
    /// Returns false if user don't install certificate, otherwise true.
    /// </summary>
    public static bool InstallCertificate(string certPath, StoreName storeName, StoreLocation storeLocation)
    {
        try
        {
            X509Certificate2 certificate = new(certPath, "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            bool installed = InstallCertificate(certificate, storeName, storeLocation);
            certificate.Dispose();
            return installed;
        }
        catch (Exception ex) // Internal.Cryptography.CryptoThrowHelper.WindowsCryptographicException
        {
            Debug.WriteLine("InstallCertificate: " + ex.Message);
            // If ex.Message: (The operation was canceled by the user.)
            return false;
        }
    }

    public static bool IsCertificateInstalled(string subjectName, StoreName storeName, StoreLocation storeLocation)
    {
        try
        {
            string cn = "CN=";
            if (subjectName.StartsWith(cn)) subjectName = subjectName.TrimStart(cn);

            X509Store store = new(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);

            if (certificates != null && certificates.Count > 0)
            {
                //Debug.WriteLine("Certificate is already installed.");
                return true;
            }
            else return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("IsCertificateInstalled: " + ex.Message);
            return false;
        }
    }

    public static bool IsCertificateInstalled(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
    {
        try
        {
            string subjectName = cert.Subject;
            string cn = "CN=";
            if (subjectName.StartsWith(cn)) subjectName = subjectName.TrimStart(cn);

            X509Store store = new(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certsBySubject = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);
            X509Certificate2Collection certsBySN = store.Certificates.Find(X509FindType.FindBySerialNumber, cert.SerialNumber, false);

            if (certsBySubject != null && certsBySubject.Any() && certsBySN != null && certsBySN.Any())
            {
                Debug.WriteLine("Certificate is already installed.");
                return true;
            }
            else return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("IsCertificateInstalled: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Returns false if user don't uninstall certificate, otherwise true.
    /// </summary>
    public static bool UninstallCertificate(string subjectName, StoreName storeName, StoreLocation storeLocation)
    {
        try
        {
            X509Store store = new(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);

            // You could also use a more specific find type such as X509FindType.FindByThumbprint
            X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);
            Debug.WriteLine($"Cert Count: {certificates.Count}");

            for (int i = 0; i < certificates.Count; i++)
            {
                X509Certificate2 cert = certificates[i];
                Debug.WriteLine($"Cert SubjectName: {cert.SubjectName.Name}");

                X509Chain chain = new();
                chain.Build(cert);
                X509Certificate2Collection allCertsInChain = new();
                Debug.WriteLine($"Cert Chain Count: {chain.ChainElements.Count}");

                for (int j = 0; j < chain.ChainElements.Count; j++)
                {
                    X509ChainElement chainElement = chain.ChainElements[j];
                    allCertsInChain.Add(chainElement.Certificate);

                    Debug.WriteLine($"Cert Chain SubjectName: {chainElement.Certificate.SubjectName.Name}");
                }

                store.RemoveRange(allCertsInChain);
                store.Remove(cert);
            }
            store.Close();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UninstallCertificate: " + ex.Message);
            return false;
        }
    }

}