using System.Net;
using MsmhToolsClass;

namespace SecureDNSClient;

public partial class FormMain
{
    //============================== Get Settings Vars
    public int GetKillOnCpuUsageSetting()
    {
        int killOnCpuUsage = 40;
        this.InvokeIt(() => killOnCpuUsage = Convert.ToInt32(CustomNumericUpDownSettingCpuKillProxyRequests.Value));
        return killOnCpuUsage;
    }

    public IPAddress GetBootstrapSetting(out int bootstrapPort)
    {
        // Get Bootstrap IP and Port
        IPAddress bootstrap = SecureDNS.BootstrapDnsIPv4;
        int bootstrapPortD = SecureDNS.BootstrapDnsPort;
        bool isBootstrap = NetworkTool.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? bootstrapIP);
        if (isBootstrap && bootstrapIP != null)
        {
            bootstrap = bootstrapIP;
            bootstrapPortD = Convert.ToInt32(CustomNumericUpDownSettingBootstrapDnsPort.Value);
        }
        bootstrapPort = bootstrapPortD;
        return bootstrap;
    }

    public string GetBlockedDomainSetting(out string blockedDomainNoWww)
    {
        // Get and check blocked domain is valid
        bool isBlockedDomainValid = SecureDNS.IsBlockedDomainValid(CustomTextBoxSettingCheckDPIHost, CustomRichTextBoxLog, out string blockedDomain);
        if (!isBlockedDomainValid)
        {
            blockedDomainNoWww = string.Empty;
            return string.Empty;
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