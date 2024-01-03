using MsmhToolsClass;
using MsmhToolsClass.DnsTool;
using System.Diagnostics;
using System.Net;

namespace SecureDNSClient;

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
        NetworkTool.GetUrlDetails(proxyScheme, 0, out _, out string host, out _, out _, out int port, out string _, out bool _);

        // Convert proxy host to IP
        string ipStr = string.Empty;
        bool isIP = IPAddress.TryParse(host, out IPAddress? _);
        if (isIP)
            ipStr = host;
        else
            ipStr = GetIP.GetIpFromSystem(host);

        // Check if proxy works
        if (!string.IsNullOrEmpty(ipStr))
        {
            bool isProxyOk = await NetworkTool.CanPing(ipStr, 15);
            if (!isProxyOk)
            {
                string msgWrongProxy = $"Proxy doesn't work.";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgWrongProxy + NL, Color.IndianRed));
                return;
            }
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
                // Get DoH Port
                int dohPort = GetDohPortSetting();

                // Set Connected DoH Port
                ConnectedDohPort = dohPort;

                dnsCryptConfig.EnableDoH(dohPort);
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
        string args = $"-config \"{SecureDNS.DNSCryptConfigPath}\"";

        if (IsDisconnecting) return;

        // Execute DNSCrypt
        ProcessConsole pc = new();
        PIDDNSCrypt = pc.Execute(SecureDNS.DNSCrypt, args, true, true, SecureDNS.CurrentPath, GetCPUPriority());
        pc.StandardDataReceived -= Pc_StandardDataReceived;
        pc.StandardDataReceived += Pc_StandardDataReceived;
        void Pc_StandardDataReceived(object? sender, DataReceivedEventArgs e)
        {
            string? msg = e.Data;
            if (msg != null)
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray));
        }

        pc.ErrorDataReceived -= Pc_ErrorDataReceived;
        pc.ErrorDataReceived += Pc_ErrorDataReceived;
        void Pc_ErrorDataReceived(object? sender, DataReceivedEventArgs e)
        {
            string? msg = e.Data;
            if (msg != null)
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray));
        }
    }

}