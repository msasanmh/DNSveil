using MsmhTools;
using SecureDNSClient.DNSCrypt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private async Task ConnectToServersUsingProxy()
        {
            if (!CustomRadioButtonConnectDNSCrypt.Checked) return;
            string? proxyScheme = CustomTextBoxHTTPProxy.Text;

            void proxySchemeIncorrect()
            {
                string msgWrongProxy = "HTTP(S) proxy scheme must be like: \"https://myproxy.net:8080\"";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgWrongProxy + NL, Color.IndianRed));
            }

            // Check if proxy scheme is correct
            if (string.IsNullOrWhiteSpace(proxyScheme) || !proxyScheme.Contains("//") || proxyScheme.EndsWith('/'))
            {
                proxySchemeIncorrect();
                return;
            }

            // Get Host and Port of Proxy
            string host = Network.UrlToHostAndPort(proxyScheme, 0, out int port, out string _, out bool _);

            // Convert proxy host to IP
            bool isIP = IPAddress.TryParse(host, out IPAddress? _);
            if (!isIP)
            {
                IPAddress? ip = Network.HostToIP(host);
                if (ip != null) host = ip.ToString();
            }

            // Check if proxy works
            bool isProxyOk = Network.CanPing(host, 15);
            if (!isProxyOk)
            {
                string msgWrongProxy = $"Proxy doesn't work.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgWrongProxy + NL, Color.IndianRed));
                return;
            }

            // Check if config file exist
            if (!File.Exists(SecureDNS.DNSCryptConfigPath))
            {
                string msg = "Error: Configuration file doesn't exist";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed));
                return;
            }

            // Get Bootstrap IP & Port
            IPAddress bootstrap = GetBootstrapSetting(out int bootstrapPort);

            // Edit DNSCrypt Config File
            DNSCryptConfigEditor dnsCryptConfig = new(SecureDNS.DNSCryptConfigPath);
            dnsCryptConfig.EditHTTPProxy(proxyScheme);
            dnsCryptConfig.EditBootstrapDNS(bootstrap, bootstrapPort);
            dnsCryptConfig.EditDnsCache(CustomCheckBoxSettingEnableCache.Checked);

            // Edit DNSCrypt Config File: Enable DoH
            if (CustomRadioButtonSettingWorkingModeDNSandDoH.Checked)
            {
                if (File.Exists(SecureDNS.CertPath) && File.Exists(SecureDNS.KeyPath))
                {
                    dnsCryptConfig.EnableDoH();
                    dnsCryptConfig.EditCertKeyPath(SecureDNS.KeyPath);
                    dnsCryptConfig.EditCertPath(SecureDNS.CertPath);
                }
                else
                    dnsCryptConfig.DisableDoH();
            }
            else
                dnsCryptConfig.DisableDoH();

            // Save DNSCrypt Config File
            await dnsCryptConfig.WriteAsync();

            // Args
            string args = $"-config {SecureDNS.DNSCryptConfigPath}";

            if (IsDisconnecting) return;

            // Execute DNSCrypt
            Process process = new();
            //process.PriorityClass = GetCPUPriority(); // Exception: No process is associated with this object.
            process.StartInfo.FileName = SecureDNS.DNSCrypt;
            process.StartInfo.Arguments = args;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = Info.CurrentPath;

            process.OutputDataReceived += (sender, args) =>
            {
                string? data = args.Data;
                if (data != null)
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(data + NL, Color.LightGray));
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                // DNSCrypt writes its output data in error event!
                string? data = args.Data;
                if (data != null)
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(data + NL, Color.LightGray));
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                PIDDNSCrypt = process.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
