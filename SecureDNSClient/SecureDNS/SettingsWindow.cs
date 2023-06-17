using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using MsmhTools;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        //============================== Get Settings Vars
        public IPAddress GetBootstrapSetting(out int bootstrapPort)
        {
            // Get Bootstrap IP and Port
            IPAddress bootstrap = SecureDNS.BootstrapDnsIPv4;
            int bootstrapPortD = SecureDNS.BootstrapDnsPort;
            bool isBootstrap = Network.IsIPv4Valid(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? bootstrapIP);
            if (isBootstrap && bootstrapIP != null)
            {
                bootstrap = bootstrapIP;
                bootstrapPortD = int.Parse(CustomNumericUpDownSettingBootstrapDnsPort.Value.ToString());
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

            blockedDomainNoWww = blockedDomainNoWwwD;
            return blockedDomain;
        }

        public int GetHTTPProxyPortSetting()
        {
            return int.Parse(CustomNumericUpDownSettingHTTPProxyPort.Value.ToString());
        }

        public int GetFakeProxyPortSetting()
        {
            return int.Parse(CustomNumericUpDownSettingFakeProxyPort.Value.ToString());
        }

        public int GetCamouflageDnsPortSetting()
        {
            return int.Parse(CustomNumericUpDownSettingCamouflageDnsPort.Value.ToString());
        }

        /// <summary>
        /// Get listening port
        /// </summary>
        /// <param name="portToCheck">Port</param>
        /// <returns>Returns True if everything's ok</returns>
        public bool GetListeningPort(int portToCheck, string message, Color color)
        {
            bool isPortOpen = Network.IsPortOpen(IPAddress.Loopback.ToString(), portToCheck, 3);
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
}
