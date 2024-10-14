using System.Diagnostics;
using System.Globalization;
using System.Net;
using CustomControls;
using MsmhToolsClass;

namespace SecureDNSClient;

public partial class FormMain
{
    //============================== Get Settings Vars
    public SetDnsOnNic.ActiveNICs GetNicNameSetting(CustomComboBox ccb)
    {
        SetDnsOnNic.ActiveNICs nicsList = new();

        try
        {
            ccb.InvokeIt(() =>
            {
                if (ccb != null && ccb.SelectedItem != null)
                {
                    string? nicName = ccb.SelectedItem as string;
                    if (!string.IsNullOrEmpty(nicName))
                    {
                        if (nicName.Equals(SetDnsOnNic.DefaultNicName.Auto))
                        {
                            IsDNSSet = SetDnsOnNic_.IsDnsSet(CustomComboBoxNICs, out bool isDnsSetOn, out SetDnsOnNic.ActiveNICs activeNICs);
                            IsDNSSetOn = isDnsSetOn;
                            nicsList = activeNICs;
                        }
                        else
                        {
                            nicsList.NICs.Add(nicName);
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SettingWindow GetNicNameSetting: " + ex.Message);
        }

        return nicsList;
    }

    public string GetCfCleanIpSetting()
    {
        string result = string.Empty;

        try
        {
            bool isEnable = false;
            this.InvokeIt(() => isEnable = CustomCheckBoxSettingProxyCfCleanIP.Checked);
            if (!isEnable) return result;

            this.InvokeIt(() => result = CustomTextBoxSettingProxyCfCleanIP.Text.Trim());
            bool isCfCleanIpValid = NetworkTool.IsIP(result, out IPAddress? _);
            if (!isCfCleanIpValid) result = string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SettingWindow GetCfCleanIpSetting: " + ex.Message);
        }

        return result;
    }

    public string GetDefaultSniSetting()
    {
        string result = string.Empty;
        this.InvokeIt(() => result = CustomTextBoxProxySSLDefaultSni.Text);
        return result.Trim();
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

    public int GetMaxRequestsSetting()
    {
        int maxRequests = 1000;
        try
        {
            this.InvokeIt(() => maxRequests = Convert.ToInt32(CustomNumericUpDownSettingProxyHandleRequests.Value));
        }
        catch (Exception) { }
        return maxRequests;
    }

    public int GetProxyTimeoutSetting()
    {
        int timeoutSec = 40;
        try
        {
            this.InvokeIt(() => timeoutSec = Convert.ToInt32(CustomNumericUpDownSettingProxyKillRequestTimeout.Value));
        }
        catch (Exception) { }
        return timeoutSec;
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

    public bool GetBlockPort80Setting()
    {
        bool result = false;
        this.InvokeIt(() => result = CustomCheckBoxSettingProxyBlockPort80.Checked);
        return result;
    }

    public IPAddress GetBootstrapSetting(out int bootstrapPort)
    {
        // Get Bootstrap IP and Port
        IPAddress bootstrap = SecureDNS.BootstrapDnsIPv4;
        int bootstrapPortD = SecureDNS.BootstrapDnsPort;
        try
        {
            bool isBootstrap = NetworkTool.IsIP(CustomTextBoxSettingBootstrapDnsIP.Text, out IPAddress? bootstrapIP);
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

        // Strip www. from blocked domain
        string blockedDomainNoWwwD = blockedDomain;

        try
        {
            if (blockedDomainNoWwwD.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                blockedDomainNoWwwD = blockedDomainNoWwwD[4..];
        }
        catch (Exception) { }

        blockedDomainNoWww = blockedDomainNoWwwD.Trim();
        return blockedDomain.Trim();
    }

    public async Task<(bool IsSuccess, string ProxyScheme, string ProxyUser, string ProxyPass, bool OnlyBlockedIPs)> GetUpStreamProxySettingAsync()
    {
        try
        {
            if (!CustomCheckBoxSettingProxyUpstream.Checked) return (true, string.Empty, string.Empty, string.Empty, false);

            // Get Upstream Mode
            string? mode = null;
            this.InvokeIt(() => mode = CustomComboBoxSettingProxyUpstreamMode.SelectedItem as string);

            // Check If Mode Is Empty
            if (string.IsNullOrEmpty(mode))
            {
                string msg = "Select The Mode Of Upstream Proxy." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return (false, string.Empty, string.Empty, string.Empty, false);
            }

            // Get Upstream Host
            string upstreamHost = string.Empty;
            this.InvokeIt(() => upstreamHost = CustomTextBoxSettingProxyUpstreamHost.Text.Trim());

            // Check If Host Is Empty
            if (string.IsNullOrEmpty(upstreamHost))
            {
                string msg = "Upstream proxy host cannot be empty." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return (false, string.Empty, string.Empty, string.Empty, false);
            }

            // Get Upstream Port
            int upstreamPort = -1;
            this.InvokeIt(() => upstreamPort = Convert.ToInt32(CustomNumericUpDownSettingProxyUpstreamPort.Value));

            // Get Blocked Domain
            string blockedDomain = GetBlockedDomainSetting(out string _);
            if (string.IsNullOrEmpty(blockedDomain)) return (false, string.Empty, string.Empty, string.Empty, false);

            // Get Upstream Proxy Scheme
            string proxyScheme = $"{mode.ToLower().Trim()}://{upstreamHost}:{upstreamPort}";

            // Check Upstream Proxy Works
            bool isUpstreamProxyOk = await NetworkTool.IsWebsiteOnlineAsync($"https://{blockedDomain}", null, 5000, false, proxyScheme);
            if (!isUpstreamProxyOk)
            {
                string msg = $"Upstream Proxy Cannot Open {blockedDomain}." + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
                return (false, string.Empty, string.Empty, string.Empty, false);
            }

            // Get Apply Only To Blocked IPs
            bool onlyBlockedIPs = false;
            this.InvokeIt(() => onlyBlockedIPs = CustomCheckBoxSettingProxyUpstreamOnlyBlockedIPs.Checked);

            return (true, proxyScheme, string.Empty, string.Empty, onlyBlockedIPs);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SettingWindow GetUpStreamProxySettingAsync: " + ex.Message);
            return (false, string.Empty, string.Empty, string.Empty, false);
        }
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

    /// <summary>
    /// Get listening port
    /// </summary>
    /// <param name="portToCheck">Port</param>
    /// <returns>Returns True if everything's ok</returns>
    public bool GetListeningPort(int portToCheck, string message, Color color)
    {
        List<int> pids = ProcessManager.GetProcessPidsByUsingPort(portToCheck);
        bool isPortOpen = pids.Any();
        if (isPortOpen)
        {
            try
            {
                List<string> names = new();
                foreach (int pid in pids)
                {
                    string name = ProcessManager.GetProcessNameByPID(pid);
                    if (!string.IsNullOrEmpty(name)) names.Add(name);
                }
                string namesStr = names.ToString(", ");
                namesStr = string.IsNullOrEmpty(namesStr) ? "Unknown" : namesStr;

                string msg = $"Port {portToCheck} Is Occupied By \"{namesStr}\". {message}{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, color));
            }
            catch (Exception) { }

            return false;
        }
        else return true;
    }

}