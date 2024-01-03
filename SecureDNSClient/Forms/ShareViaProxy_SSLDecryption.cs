using CustomControls;
using MsmhToolsClass;
using System.Security.Cryptography.X509Certificates;

namespace SecureDNSClient;

public partial class FormMain
{
    public async Task ApplySSLDecryption()
    {
        if (!CustomCheckBoxProxyEnableSSL.Checked)
        {
            string msg = "Applied.";
            msg += $"{NL}SSL Status: Disable";
            CustomMessageBox.Show(this, msg, "SSL Decryption", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await GenerateCertificate();

        if (!File.Exists(SecureDNS.IssuerCertPath) || !File.Exists(SecureDNS.IssuerKeyPath))
        {
            string msg = "Couldn't Generate Certificate.";
            msg += $"{NL}SSL Status: Disable";
            this.InvokeIt(() => CustomCheckBoxProxyEnableSSL.Checked = false);
            CustomMessageBox.Show(this, msg, "SSL Decryption", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool isCertInstalled = false;
        if (File.Exists(SecureDNS.IssuerCertPath) && File.Exists(SecureDNS.IssuerKeyPath))
        {
            try
            {
                X509Certificate2 rootCA = new(X509Certificate2.CreateFromCertFile(SecureDNS.IssuerCertPath));
                isCertInstalled = CertificateTool.IsCertificateInstalled(rootCA, StoreName.Root, StoreLocation.CurrentUser);
                rootCA.Dispose();
            }
            catch (Exception) { }
        }
        if (isCertInstalled) return;

        if (!isCertInstalled)
        {
            bool isCertInstalledBySubject = CertificateTool.IsCertificateInstalled(SecureDNS.CertIssuerSubjectName, StoreName.Root, StoreLocation.CurrentUser);

            // If Cert Serial Number Changed Uninstall It
            if (isCertInstalledBySubject)
            {
                await Task.Run(() =>
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

                    // Check If DoH Server Is Running With Previous Cert
                    if (IsConnected && IsDoHConnected)
                    {
                        string msg = "Due to Certificate changes you need to restart DoH Server.";
                        CustomMessageBox.Show(this, msg, "Certificate Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                });
            }

            // Install Certificate
            bool certInstalled = CertificateTool.InstallCertificate(SecureDNS.IssuerCertPath, StoreName.Root, StoreLocation.CurrentUser);
            if (!certInstalled)
            {
                string msg = "You Must Install The Certificate Authority.";
                msg += $"{NL}SSL Status: Disable";
                this.InvokeIt(() => CustomCheckBoxProxyEnableSSL.Checked = false);
                CustomMessageBox.Show(this, msg, "SSL Decryption", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                this.InvokeIt(() => CustomCheckBoxProxyEnableSSL.Checked = true);

                string msg = "Applied.";
                msg += $"{NL}SSL Status: Enable";
                if (IsProxyActivated || IsProxyActivating)
                    msg += $"{NL}You need to restart Proxy Server.";
                CustomMessageBox.Show(this, msg, "SSL Decryption", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }
    }

    private bool IsSSLDecryptionEnable()
    {
        bool isSSLDecryptionEnable = false;
        if (CustomCheckBoxProxyEnableSSL.Checked)
        {
            if (File.Exists(SecureDNS.IssuerCertPath) && File.Exists(SecureDNS.IssuerKeyPath))
            {
                try
                {
                    X509Certificate2 rootCA = new(X509Certificate2.CreateFromCertFile(SecureDNS.IssuerCertPath));
                    bool isCertInstalled = CertificateTool.IsCertificateInstalled(rootCA, StoreName.Root, StoreLocation.CurrentUser);
                    rootCA.Dispose();
                    isSSLDecryptionEnable = isCertInstalled;
                }
                catch (Exception) { }
            }
        }

        if (!isSSLDecryptionEnable)
        {
            this.InvokeIt(() =>
            {
                if (CustomCheckBoxProxyEnableSSL.Tag == null)
                {
                    CustomCheckBoxProxyEnableSSL.Checked = false;
                    if (!CustomCheckBoxPDpiEnableDpiBypass.Enabled)
                        CustomCheckBoxPDpiEnableDpiBypass.Enabled = true;
                }
                CustomCheckBoxProxyEnableSSL.Tag = null;
            });
        }

        return isSSLDecryptionEnable;
    }
}