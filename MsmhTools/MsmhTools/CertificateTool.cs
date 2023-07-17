using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MsmhTools
{
    public class CertificateTool
    {
        public static void GenerateCertificate(string folderPath, IPAddress gateway, string issuerSubjectName = "CN=MSasanMH Authority", string subjectName = "CN=MSasanMH")
        {
            const string CRT_HEADER = "-----BEGIN CERTIFICATE-----\n";
            const string CRT_FOOTER = "\n-----END CERTIFICATE-----";

            const string KEY_HEADER = "-----BEGIN RSA PRIVATE KEY-----\n";
            const string KEY_FOOTER = "\n-----END RSA PRIVATE KEY-----";

            // Create X509KeyUsageFlags
            const X509KeyUsageFlags x509KeyUsageFlags = X509KeyUsageFlags.CrlSign |
                                                        X509KeyUsageFlags.DataEncipherment |
                                                        X509KeyUsageFlags.DigitalSignature |
                                                        X509KeyUsageFlags.KeyAgreement |
                                                        X509KeyUsageFlags.KeyCertSign |
                                                        X509KeyUsageFlags.KeyEncipherment |
                                                        X509KeyUsageFlags.NonRepudiation;

            // Create SubjectAlternativeNameBuilder
            SubjectAlternativeNameBuilder sanBuilder = new();
            sanBuilder.AddDnsName("localhost"); // Add Localhost
            sanBuilder.AddDnsName(Environment.UserName); // Add Current User
            sanBuilder.AddUserPrincipalName(System.Security.Principal.WindowsIdentity.GetCurrent().Name); // Add User Principal Name
            sanBuilder.AddIpAddress(IPAddress.Parse("127.0.0.1"));
            sanBuilder.AddIpAddress(IPAddress.Parse("0.0.0.0"));
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);

            // Generate IP range for gateway
            if (Network.IsIPv4(gateway))
            {
                string ipString = gateway.ToString();
                string[] ipSplit = ipString.Split('.');
                string ip1 = ipSplit[0] + "." + ipSplit[1] + "." + ipSplit[2] + ".";
                for (int n = 0; n <= 255; n++)
                {
                    string ip2 = ip1 + n.ToString();
                    sanBuilder.AddIpAddress(IPAddress.Parse(ip2));
                }
                // Generate local IP range in case a VPN is active.
                if (!ip1.Equals("192.168.1."))
                {
                    string ipLocal1 = "192.168.1.";
                    for (int n = 0; n <= 255; n++)
                    {
                        string ipLocal2 = ipLocal1 + n.ToString();
                        sanBuilder.AddIpAddress(IPAddress.Parse(ipLocal2));
                    }
                }
            }

            sanBuilder.AddUri(new Uri("https://127.0.0.1"));
            sanBuilder.Build();

            // Create Oid Collection
            OidCollection oidCollection = new();
            oidCollection.Add(new Oid("2.5.29.37.0")); // Any Purpose
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.1")); // Server Authentication
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.2")); // Client Authentication
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.3")); // Code Signing
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.4")); // Email Protection
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.5")); // IPSEC End System Certificate
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.6")); // IPSEC Tunnel
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.7")); // IPSEC User Certificate
            oidCollection.Add(new Oid("1.3.6.1.5.5.7.3.8")); // Time Stamping
            oidCollection.Add(new Oid("1.3.6.1.4.1.311.10.3.2")); // Microsoft Time Stamping
            oidCollection.Add(new Oid("1.3.6.1.4.1.311.10.5.1")); // Digital Rights
            oidCollection.Add(new Oid("1.3.6.1.4.1.311.64.1.1")); // Domain Name System (DNS) Server Trust

            // Create Issuer RSA Private Key
            using RSA issuerRsaKey = RSA.Create(4096);

            // Create Issuer Request
            CertificateRequest issuerReq = new(issuerSubjectName, issuerRsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            issuerReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            issuerReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(issuerReq.PublicKey, false));
            issuerReq.CertificateExtensions.Add(new X509KeyUsageExtension(x509KeyUsageFlags, false));
            issuerReq.CertificateExtensions.Add(sanBuilder.Build());
            issuerReq.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(oidCollection, true));

            // Create Issuer Certificate
            using X509Certificate2 issuerCert = issuerReq.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));

            // Create RSA Private Key
            using RSA rsaKey = RSA.Create(2048);

            // Create Request
            CertificateRequest req = new(subjectName, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));
            req.CertificateExtensions.Add(new X509KeyUsageExtension(x509KeyUsageFlags, false));
            req.CertificateExtensions.Add(sanBuilder.Build());
            req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(oidCollection, true));

            // Create Certificate
            using X509Certificate2 cert = req.Create(issuerCert, DateTimeOffset.Now, DateTimeOffset.Now.AddYears(9), new byte[] { 1, 2, 3, 4 });

            // Export
            // Export Issuer Private Key
            var issuerPrivateKeyExport = issuerRsaKey.ExportRSAPrivateKey();
            var issuerPrivateKeyData = Convert.ToBase64String(issuerPrivateKeyExport, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(Path.Combine(folderPath, "rootCA.key"), KEY_HEADER + issuerPrivateKeyData + KEY_FOOTER);

            // Export Issuer Certificate
            var issuerCertExport = issuerCert.Export(X509ContentType.Cert);
            var issuerCertData = Convert.ToBase64String(issuerCertExport, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(Path.Combine(folderPath, "rootCA.crt"), CRT_HEADER + issuerCertData + CRT_FOOTER);

            // Export Private Key
            var privateKeyExport = rsaKey.ExportRSAPrivateKey();
            var privateKeyData = Convert.ToBase64String(privateKeyExport, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(Path.Combine(folderPath, "localhost.key"), KEY_HEADER + privateKeyData + KEY_FOOTER);

            // Export Certificate
            var certExport = cert.Export(X509ContentType.Cert);
            var certData = Convert.ToBase64String(certExport, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(Path.Combine(folderPath, "localhost.crt"), CRT_HEADER + certData + CRT_FOOTER);
            
        }

        public static void CreateP12(string certPath, string keyPath, string password = "")
        {
            string? folderPath = Path.GetDirectoryName(certPath);
            string fileName = Path.GetFileNameWithoutExtension(certPath);
            using X509Certificate2 certWithKey = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            var certWithKeyExport = certWithKey.Export(X509ContentType.Pfx, password);
            if (!string.IsNullOrEmpty(folderPath))
                File.WriteAllBytes(Path.Combine(folderPath, fileName + ".p12"), certWithKeyExport);
        }

        /// <summary>
        /// Returns false if user don't install certificate, otherwise true.
        /// </summary>
        public static bool InstallCertificate(string certPath, StoreName storeName, StoreLocation storeLocation)
        {
            try
            {
                X509Certificate2 certificate = new(certPath, "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                X509Store store = new(storeName, storeLocation);
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
                store.Close();
                return true;
            }
            catch (Exception ex) // Internal.Cryptography.CryptoThrowHelper.WindowsCryptographicException
            {
                Debug.WriteLine(ex.Message);
                // If ex.Message: (The operation was canceled by the user.)
                return false;
            }
        }

        public static bool IsCertificateInstalled(string subjectName, StoreName storeName, StoreLocation storeLocation)
        {
            X509Store store = new(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);

            if (certificates != null && certificates.Count > 0)
            {
                Debug.WriteLine("Certificate is already installed.");
                return true;
            }
            else
                return false;
        }

        public static void UninstallCertificate(string subjectName, StoreName storeName, StoreLocation storeLocation)
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
        }
    }
}
