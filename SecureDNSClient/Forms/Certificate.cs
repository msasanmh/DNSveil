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
            if (File.Exists(SecureDNS.IssuerCertPath) && !CustomCheckBoxSettingDontAskCertificate.Checked)
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
                            await StartProxy(true);
                            string msg = "Due to Certificate changes you need to restart Proxy Server.";
                            CustomMessageBox.Show(this, msg, "Certificate Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }

                    bool certInstalled = CertificateTool.InstallCertificate(SecureDNS.IssuerCertPath, StoreName.Root, StoreLocation.CurrentUser);
                    if (!certInstalled)
                    {
                        string msg = "Local DoH Server doesn't work without certificate.\nYou can remove certificate anytime from Windows.\nTry again?";
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
        bool isCertInstalled = CertificateTool.IsCertificateInstalled(SecureDNS.CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);
        if (isCertInstalled)
        {
            if (IsDoHConnected)
            {
                string msg = "You cannot uninstall certificate while DoH Server is active.";
                CustomMessageBox.Show(this, msg, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (IsProxySSLDecryptionActive)
            {
                string msg = "You cannot uninstall certificate while Proxy Server is active and using SSL Decryption.";
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
            catch (Exception)
            {
                // do nothing
            }
        }
        else
        {
            string msg = "Certificate is already uninstalled.";
            CustomMessageBox.Show(this, msg, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

}