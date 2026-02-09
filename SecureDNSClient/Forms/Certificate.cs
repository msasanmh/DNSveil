using CustomControls;
using MsmhToolsClass;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task GenerateCertificate()
    {
        await Task.Run(async () =>
        {
            // Create certificate directory
            FileDirectory.CreateEmptyDirectory(SecureDNS.CertificateDirPath);
            string issuerSubjectName = SecureDNS.CertIssuerSubjectName;
            string subjectName = SecureDNS.CertSubjectName;

            // Generate certificate
            if (!File.Exists(SecureDNS.IssuerCertPath) || !File.Exists(SecureDNS.IssuerKeyPath) || !File.Exists(SecureDNS.CertPath) || !File.Exists(SecureDNS.KeyPath))
            {
                // It is overwritten, no need to delete.
                IPAddress? gateway = NetworkTool.GetDefaultGateway() ?? IPAddress.Loopback;
                await CertificateTool.GenerateCertificateAsync(SecureDNS.CertificateDirPath, gateway, issuerSubjectName, subjectName);
                CertificateTool.CreateP12(SecureDNS.IssuerCertPath, SecureDNS.IssuerKeyPath);
                CertificateTool.CreateP12(SecureDNS.CertPath, SecureDNS.KeyPath);
            }
        });
    }

    private async Task InstallCertificateForDoH()
    {
        await Task.Run(async () =>
        {
            if (File.Exists(SecureDNS.IssuerCertPath))
            {
                bool isCertInstalledBySubject = CertificateTool.IsCertificateInstalled(SecureDNS.CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);
                X509Certificate2 rootCA = new(X509Certificate2.CreateFromCertFile(SecureDNS.IssuerCertPath));
                bool isCertInstalled = CertificateTool.IsCertificateInstalled(rootCA, StoreName.Root, StoreLocation.CurrentUser);
                rootCA.Dispose();

                if (!isCertInstalled)
                {
                    // If Cert Serial Number Changed Uninstall It
                    if (isCertInstalledBySubject)
                    {
                        while (true)
                        {
                            bool uninstalled = CertificateTool.UninstallCertificate(SecureDNS.CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);
                            if (uninstalled) break;
                            if (!uninstalled)
                            {
                                string msgCertChanged = "Certificate Regenerated, You Must Uninstall The Previous One.";
                                CustomMessageBox.Show(this, msgCertChanged, "Certificate Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }

                        // Check If Proxy Server Is Running With Previous Cert
                        if (IsProxyActivated && IsProxySSLDecryptionActive)
                        {
                            // Stop Proxy
                            await StartProxyAsync(true);
                            string msg = "Due To Certificate Changes You Need To Restart Proxy Server.";
                            CustomMessageBox.Show(this, msg, "Certificate Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    bool certInstalled = CertificateTool.InstallCertificate(SecureDNS.IssuerCertPath, StoreName.Root, StoreLocation.CurrentUser);
                    if (!certInstalled)
                    {
                        string msg = "Local DoH Server Doesn't Work Without Certificate.\nYou Can Remove Certificate Anytime From Windows.\nTry Again?";
                        DialogResult dr = CustomMessageBox.Show(this, msg, "Certificate", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dr == DialogResult.Yes)
                            CertificateTool.InstallCertificate(SecureDNS.IssuerCertPath, StoreName.Root, StoreLocation.CurrentUser);
                    }
                }
            }
        });
    }

    private void UninstallCertificate()
    {
        bool isRootCertInstalled = CertificateTool.IsCertificateInstalled(SecureDNS.CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);
        if (isRootCertInstalled)
        {
            if (IsDoHConnected)
            {
                string msg = "You Cannot Uninstall Root Certificate While DoH Server is Active.";
                CustomMessageBox.Show(this, msg, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (IsProxySSLDecryptionActive)
            {
                string msg = "You Cannot Uninstall Root Certificate While Proxy Server Is Active And Using SSL Decryption.";
                CustomMessageBox.Show(this, msg, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Uninstall Certs
            CertificateTool.UninstallCertificate(SecureDNS.CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);

            // Delete Cert Files
            try
            {
                Directory.Delete(SecureDNS.CertificateDirPath, true);
            }
            catch (Exception) { }
        }

        // Fix Users Mistakes
        List<Tuple<string, StoreName, StoreLocation>> stores = new()
        {
            // Add Root Cert
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.AddressBook, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.AddressBook, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.AuthRoot, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.AuthRoot, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.CertificateAuthority, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.CertificateAuthority, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.Disallowed, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.Disallowed, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.My, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.My, StoreLocation.LocalMachine),
            //new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.Root, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.TrustedPeople, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.TrustedPeople, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.TrustedPublisher, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertIssuerSubjectName, StoreName.TrustedPublisher, StoreLocation.LocalMachine),

            // Add Cert
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.AddressBook, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.AddressBook, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.AuthRoot, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.AuthRoot, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.CertificateAuthority, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.CertificateAuthority, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.Disallowed, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.Disallowed, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.My, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.My, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.Root, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.Root, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.TrustedPeople, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.TrustedPeople, StoreLocation.LocalMachine),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.TrustedPublisher, StoreLocation.CurrentUser),
            new Tuple<string, StoreName, StoreLocation>(SecureDNS.CertSubjectName, StoreName.TrustedPublisher, StoreLocation.LocalMachine),
        };

        bool isAnyCertInstalledByMistake = false;
        foreach (Tuple<string, StoreName, StoreLocation> store in stores)
        {
            try
            {
                bool isCertInstalled = CertificateTool.IsCertificateInstalled(store.Item1, store.Item2, store.Item3);
                if (isCertInstalled)
                {
                    isAnyCertInstalledByMistake = true;
                    CertificateTool.UninstallCertificate(store.Item1, store.Item2, store.Item3);
                }
            }
            catch (Exception) { }
        }

        if (!isRootCertInstalled && !isAnyCertInstalledByMistake)
        {
            string msg = "Root Certificate Is Already Uninstalled.";
            CustomMessageBox.Show(this, msg, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

}