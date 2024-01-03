using System.Diagnostics;
using System.Globalization;
using System.Net;
using MsmhToolsClass;

namespace SecureDNSClient;

public partial class FormMain
{
    //============================== Get Settings Vars
    public string GetDefaultSniSetting()
    {
        string defaultSni = string.Empty;
        this.InvokeIt(() => defaultSni = CustomTextBoxProxySSLDefaultSni.Text);
        return defaultSni.Trim();
    }

    public int GetCheckTimeoutSetting()
    {
        int timeoutMS = 5000;
        try
        {
            decimal timeoutSec = 1;
            this.InvokeIt(() => timeoutSec = CustomNumericUpDownSettingCheckTimeout.Value);
            timeoutMS = decimal.ToInt32(timeoutSec * 1000);
        }
        catch (Exception) { }
        return timeoutMS;
    }

    public int GetParallelSizeSetting()
    {
        int parallelSize = 5;
        try
        {
            this.InvokeIt(() => parallelSize = Convert.ToInt32(CustomNumericUpDownCheckInParallel.Value));
        }
        catch (Exception) { }
        return parallelSize;
    }

    public int GetMaxServersToConnectSetting()
    {
        int max = 5;
        try
        {
            this.InvokeIt(() => max = decimal.ToInt32(CustomNumericUpDownSettingMaxServers.Value));
        }
        catch (Exception) { }
        return max;
    }

    public int GetUpdateAutoDelaySetting()
    {
        int updateAutoDelayMS = 500;
        try
        {
            this.InvokeIt(() => updateAutoDelayMS = Convert.ToInt32(CustomNumericUpDownUpdateAutoDelayMS.Value));
        }
        catch (Exception) { }
        return updateAutoDelayMS;
    }

    public int GetKillOnCpuUsageSetting()
    {
        int killOnCpuUsage = 40;
        try
        {
            this.InvokeIt(() => killOnCpuUsage = Convert.ToInt32(CustomNumericUpDownSettingCpuKillProxyRequests.Value));
        }
        catch (Exception) { }
        return killOnCpuUsage;
    }

    public IPAddress GetBootstrapSetting(out int bootstrapPort)
    {
        // Get Bootstrap IP and Port
        IPAddress bootstrap = SecureDNS.BootstrapDnsIPv4;
        int bootstrapPortD = SecureDNS.BootstrapDnsPort;
        try
        {
            bool isBootstrap = NetworkTool.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? bootstrapIP);
            if (isBootstrap && bootstrapIP != null)
            {
                bootstrap = bootstrapIP;
                bootstrapPortD = Convert.ToInt32(CustomNumericUpDownSettingBootstrapDnsPort.Value);
            }
            else
            {
                bootstrap = CultureInfo.InstalledUICulture switch
                {
                    { Name: string n } when n.ToLower().StartsWith("fa") => IPAddress.Parse("8.8.8.8"), // Iran
                    { Name: string n } when n.ToLower().StartsWith("ru") => IPAddress.Parse("77.88.8.7"), // Russia
                    { Name: string n } when n.ToLower().StartsWith("zh") => IPAddress.Parse("223.6.6.6"), // China
                    _ => SecureDNS.BootstrapDnsIPv4 // Others
                };
                if (bootstrap.Equals(SecureDNS.BootstrapDnsIPv4))
                    bootstrapPortD = SecureDNS.BootstrapDnsPort;
                else
                    bootstrapPortD = 53;

                this.InvokeIt(() =>
                {
                    if (!CustomTextBoxSettingBootstrapDnsIP.Focused)
                    {
                        CustomTextBoxSettingBootstrapDnsIP.Text = bootstrap.ToString();
                        CustomNumericUpDownSettingBootstrapDnsPort.Value = bootstrapPortD;
                    }
                });
            }
        }
        catch (Exception) { }
        bootstrapPort = bootstrapPortD;
        return bootstrap;
    }

    public string GetBlockedDomainSetting(out string blockedDomainNoWww)
    {
        // Get and check blocked domain is valid
        string defaultAddr = "www.youtube.com";

        bool isBlockedDomainValid = SecureDNS.IsBlockedDomainValid(CustomTextBoxSettingCheckDPIHost, out string blockedDomain);
        
        try
        {
            if (!isBlockedDomainValid)
            {
                this.InvokeIt(() =>
                {
                    if (!CustomTextBoxSettingCheckDPIHost.Focused)
                        CustomTextBoxSettingCheckDPIHost.Text = defaultAddr;
                });

                blockedDomainNoWww = defaultAddr[4..];
                return defaultAddr;
            }

            this.InvokeIt(() =>
            {
                if (!CustomTextBoxSettingCheckDPIHost.Text.Equals(blockedDomain))
                    if (!CustomTextBoxSettingCheckDPIHost.Focused)
                        CustomTextBoxSettingCheckDPIHost.Text = blockedDomain;
            });
        }
        catch (Exception) { }

        // strip www. from blocked domain
        string blockedDomainNoWwwD = blockedDomain;
        if (blockedDomainNoWwwD.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            blockedDomainNoWwwD = blockedDomainNoWwwD[4..];

        blockedDomainNoWww = blockedDomainNoWwwD.Trim();
        return blockedDomain.Trim();
    }

    public int GetDohPortSetting()
    {
        int dohPort = 443;
        try
        {
            this.InvokeIt(() => dohPort = Convert.ToInt32(CustomNumericUpDownSettingWorkingModeSetDohPort.Value));
        }
        catch (Exception) { }
        return dohPort;
    }

    public int GetProxyPortSetting()
    {
        int proxyPort = 8080;
        try
        {
            this.InvokeIt(() => proxyPort = Convert.ToInt32(CustomNumericUpDownSettingProxyPort.Value));
        }
        catch (Exception) { }
        return proxyPort;
    }

    public int GetFakeProxyPortSetting()
    {
        int fakeProxyPort = 8070;
        try
        {
            this.InvokeIt(() => fakeProxyPort = Convert.ToInt32(CustomNumericUpDownSettingFakeProxyPort.Value));
        }
        catch (Exception) { }
        return fakeProxyPort;
    }

    public int GetCamouflageDnsPortSetting()
    {
        int camouflageDnsPort = 5380;
        try
        {
            this.InvokeIt(() => camouflageDnsPort = Convert.ToInt32(CustomNumericUpDownSettingCamouflageDnsPort.Value));
        }
        catch (Exception) { }
        return camouflageDnsPort;
    }

    /// <summary>
    /// Get listening port
    /// </summary>
    /// <param name="portToCheck">Port</param>
    /// <returns>Returns True if everything's ok</returns>
    public bool GetListeningPort(int portToCheck, string message, Color color)
    {
        bool isPortOpen = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), portToCheck, 3);
        if (isPortOpen)
        {
            string existingProcessName = ProcessManager.GetProcessNameByListeningPort(portToCheck);
            existingProcessName = existingProcessName == string.Empty ? "Unknown" : existingProcessName;
            string msg = $"Port {portToCheck} is occupied by \"{existingProcessName}\". {message}{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, color));
            return false;
        }
        else
            return true;
    }

}