using System.Globalization;
using System.Net;
using MsmhToolsClass;

namespace SecureDNSClient;

public partial class FormMain
{
    //============================== Get Settings Vars
    public int GetParallelSizeSetting()
    {
        int parallelSize = 5;
        this.InvokeIt(() => parallelSize = Convert.ToInt32(CustomNumericUpDownCheckInParallel.Value));
        return parallelSize;
    }

    public int GetKillOnCpuUsageSetting()
    {
        int killOnCpuUsage = 40;
        this.InvokeIt(() => killOnCpuUsage = Convert.ToInt32(CustomNumericUpDownSettingCpuKillProxyRequests.Value));
        return killOnCpuUsage;
    }

    public IPAddress GetBootstrapSetting(out int bootstrapPort)
    {
        // Get Bootstrap IP and Port
        IPAddress bootstrap;
        int bootstrapPortD;
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
        bootstrapPort = bootstrapPortD;
        return bootstrap;
    }

    public string GetBlockedDomainSetting(out string blockedDomainNoWww)
    {
        // Get and check blocked domain is valid
        string defaultAddr = "www.youtube.com";

        CustomTextBoxSettingCheckDPIHost.LostFocus -= CustomTextBoxSettingCheckDPIHost_LostFocus;
        CustomTextBoxSettingCheckDPIHost.LostFocus += CustomTextBoxSettingCheckDPIHost_LostFocus;
        void CustomTextBoxSettingCheckDPIHost_LostFocus(object? sender, EventArgs e)
        {
            this.InvokeIt(() => CustomTextBoxSettingCheckDPIHost.Text = defaultAddr);
        }

        bool isBlockedDomainValid = SecureDNS.IsBlockedDomainValid(CustomTextBoxSettingCheckDPIHost, out string blockedDomain);
        if (!isBlockedDomainValid)
        {
            blockedDomainNoWww = defaultAddr[4..];

            return defaultAddr;
        }

        // strip www. from blocked domain
        string blockedDomainNoWwwD = blockedDomain;
        if (blockedDomainNoWwwD.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            blockedDomainNoWwwD = blockedDomainNoWwwD[4..];

        blockedDomainNoWww = blockedDomainNoWwwD.Trim();
        return blockedDomain.Trim();
    }

    public int GetDohPortSetting()
    {
        return Convert.ToInt32(CustomNumericUpDownSettingWorkingModeSetDohPort.Value);
    }

    public int GetProxyPortSetting()
    {
        return Convert.ToInt32(CustomNumericUpDownSettingProxyPort.Value);
    }

    public int GetFakeProxyPortSetting()
    {
        return Convert.ToInt32(CustomNumericUpDownSettingFakeProxyPort.Value);
    }

    public int GetCamouflageDnsPortSetting()
    {
        return Convert.ToInt32(CustomNumericUpDownSettingCamouflageDnsPort.Value);
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