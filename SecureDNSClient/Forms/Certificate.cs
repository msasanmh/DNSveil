using CustomControls;
using MsmhTools;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private void GenerateCertificate()
        {
            // Create certificate directory
            FileDirectory.CreateEmptyDirectory(SecureDNS.CertificateDirPath);
            string issuerSubjectName = SecureDNS.CertIssuerSubjectName;
            string subjectName = SecureDNS.CertSubjectName;

            // Generate certificate
            if (!File.Exists(SecureDNS.IssuerCertPath) || !File.Exists(SecureDNS.CertPath) || !File.Exists(SecureDNS.KeyPath))
            {
                IPAddress? gateway = Network.GetDefaultGateway();
                if (gateway != null)
                {
                    CertificateTool.GenerateCertificate(SecureDNS.CertificateDirPath, gateway, issuerSubjectName, subjectName);
                    CertificateTool.CreateP12(SecureDNS.IssuerCertPath, SecureDNS.IssuerKeyPath);
                    CertificateTool.CreateP12(SecureDNS.CertPath, SecureDNS.KeyPath);
                }
            }

            // Install certificate
            if (File.Exists(SecureDNS.IssuerCertPath) && !CustomCheckBoxSettingDontAskCertificate.Checked)
            {
                bool certInstalled = CertificateTool.InstallCertificate(SecureDNS.IssuerCertPath, StoreName.Root, StoreLocation.CurrentUser);
                if (!certInstalled)
                {
                    string msg = "Local DoH Server doesn't work without certificate.\nYou can remove certificate anytime from Windows.\nTry again?";
                    using (new CenterWinDialog(this))
                    {
                        DialogResult dr = CustomMessageBox.Show(msg, "Certificate", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dr == DialogResult.Yes)
                            CertificateTool.InstallCertificate(SecureDNS.IssuerCertPath, StoreName.Root, StoreLocation.CurrentUser);
                    }
                }
            }
        }

        private void UninstallCertificate()
        {
            string issuerSubjectName = SecureDNS.CertIssuerSubjectName.Replace("CN=", string.Empty);
            bool isCertInstalled = CertificateTool.IsCertificateInstalled(issuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);
            if (isCertInstalled)
            {
                if (IsDoHConnected)
                {
                    string msg = "Disconnect local DoH first.";
                    CustomMessageBox.Show(msg, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Uninstall Certs
                CertificateTool.UninstallCertificate(issuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);

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
                CustomMessageBox.Show(msg, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
