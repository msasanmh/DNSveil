using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Net;

namespace SecureDNSClient;

public partial class FormMain
{
    private async void ReadProxyRules()
    {
        this.InvokeIt(() => CustomButtonShareRulesStatusRead.Enabled = false);
        this.InvokeIt(() => CustomButtonShareRulesStatusRead.Text = "Reading...");

        try
        {
            CustomRichTextBox log = CustomRichTextBoxShareRulesStatusResult;
            this.InvokeIt(() => log.ResetText());

            bool isRulesEnabled = false;
            this.InvokeIt(() => isRulesEnabled = CustomCheckBoxSettingEnableRules.Checked);
            if (!isRulesEnabled && RulesMode == AgnosticProgram.Rules.Mode.Disable)
            {
                this.InvokeIt(() => log.AppendText("Proxy Rules Are Disabled.", Color.DarkOrange));

                this.InvokeIt(() => CustomButtonShareRulesStatusRead.Enabled = true);
                this.InvokeIt(() => CustomButtonShareRulesStatusRead.Text = "Read");
                return;
            }

            string url = string.Empty;
            this.InvokeIt(() => url = CustomTextBoxShareRulesStatusDomain.Text);
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
                url = GetBlockedDomainSetting(out _);

            url = url.ToLower().Trim();

            NetworkTool.GetUrlDetails(url, 443, out _, out string host, out _, out _, out int port, out _, out _);

            string rulesPath = string.IsNullOrEmpty(LastRulesPath) ? SecureDNS.RulesPath : LastRulesPath;
            if (!File.Exists(rulesPath))
            {
                this.InvokeIt(() => CustomButtonShareRulesStatusRead.Enabled = true);
                this.InvokeIt(() => CustomButtonShareRulesStatusRead.Text = "Read");
                return;
            }

            string content = string.Empty;
            try { content = await File.ReadAllTextAsync(rulesPath); } catch (Exception) { }

            if (string.IsNullOrEmpty(LastRulesPath) || !content.Equals(LastRulesContent))
            {
                LastRulesContent = content;

                // Reapply ProxyRules To Proxy Server
                if (IsProxyActivated && !IsProxyActivating) await ApplyRulesToProxyAsync();

                await CheckRules.SetAsync(AgnosticProgram.Rules.Mode.Text, LastRulesContent);
            }

            AgnosticSettings agnosticSettings = new()
            {
                AllowInsecure = true,
                DnsTimeoutSec = 5,
                CloudflareCleanIP = GetCfCleanIpSetting()
            };
            AgnosticProgram.Rules.RulesResult prr = await CheckRules.GetAsync(IPAddress.Loopback.ToString(), host, port, agnosticSettings);

            this.InvokeIt(() => log.AppendText($"Domain:{NL}"));
            this.InvokeIt(() => log.AppendText($"{host}{NL}", Color.DodgerBlue));

            Color isMatchColor = prr.IsMatch ? Color.MediumSeaGreen : Color.DarkOrange;
            this.InvokeIt(() => log.AppendText($"{NL}Is Match: "));
            this.InvokeIt(() => log.AppendText($"{prr.IsMatch.ToString().CapitalizeFirstLetter()}{NL}", isMatchColor));

            if (prr.IsMatch)
            {
                this.InvokeIt(() => log.AppendText($"Is Black List: "));
                this.InvokeIt(() => log.AppendText($"{prr.IsBlackList.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                if (!prr.IsBlackList)
                {
                    this.InvokeIt(() => log.AppendText($"Is Port Block: "));
                    this.InvokeIt(() => log.AppendText($"{prr.IsPortBlock.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                    if (!prr.IsPortBlock)
                    {
                        // DNS
                        if (prr.Dnss.Any())
                        {
                            this.InvokeIt(() => log.AppendText($"{NL}DNS Servers:{NL}"));
                            foreach (string dns in prr.Dnss)
                                this.InvokeIt(() => log.AppendText($"{dns}{NL}", Color.DodgerBlue));

                            this.InvokeIt(() => log.AppendText($"Query Domain:{NL}"));
                            this.InvokeIt(() => log.AppendText($"{prr.DnsCustomDomain}{NL}", Color.DodgerBlue));
                            
                            if (!string.IsNullOrEmpty(prr.ProxyScheme))
                            {
                                this.InvokeIt(() => log.AppendText($"DNS Proxy Scheme:{NL}"));
                                this.InvokeIt(() => log.AppendText($"{prr.ProxyScheme}{NL}", Color.DodgerBlue));
                            }

                            if (prr.Dnss.Count == 1)
                            {
                                string dns = prr.Dnss[0];
                                CheckDns checkDns = new(false, false);

                                string bootstrapIP = GetBootstrapSetting(out int bootstrapPort).ToString();
                                string uncensoredDns = IsDNSConnected ? $"udp://{IPAddress.Loopback}" : $"tcp://{bootstrapIP}:{bootstrapPort}";
                                bool isSmart = await checkDns.CheckAsSmartDnsAsync(uncensoredDns, prr.DnsCustomDomain, dns);

                                this.InvokeIt(() => log.AppendText($"Act As Smart DNS: "));
                                this.InvokeIt(() => log.AppendText($"{isSmart}{NL}", Color.DodgerBlue));
                            }
                            else
                            {
                                this.InvokeIt(() => log.AppendText($"Act As Smart DNS: "));
                                this.InvokeIt(() => log.AppendText($"There Are Multiple DNSs!{NL}", Color.DarkOrange));
                            }
                        }
                        else
                        {
                            this.InvokeIt(() => log.AppendText($"{NL}DNS Servers: "));
                            this.InvokeIt(() => log.AppendText($"None{NL}", Color.DodgerBlue));
                        }
                        
                        bool isIp = NetworkTool.IsIP(prr.Dns, out _);
                        if (isIp)
                        {
                            this.InvokeIt(() => log.AppendText($"IP: "));
                            this.InvokeIt(() => log.AppendText($"{prr.Dns}{NL}", Color.DodgerBlue));
                        }
                        else
                        {
                            this.InvokeIt(() => log.AppendText($"{NL}IP: "));
                            this.InvokeIt(() => log.AppendText($"Couldn't Get DNS Message{NL}", Color.DarkOrange));
                        }

                        // Is Direct
                        this.InvokeIt(() => log.AppendText($"{NL}Is Direct: "));
                        this.InvokeIt(() => log.AppendText($"{prr.IsDirect.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                        if (!host.Equals(prr.Sni))
                        {
                            this.InvokeIt(() => log.AppendText($"SNI: "));
                            this.InvokeIt(() => log.AppendText($"{prr.Sni}{NL}", Color.DodgerBlue));
                        }

                        // UpStream Proxy
                        this.InvokeIt(() => log.AppendText($"{NL}Apply UpStream Proxy: "));
                        this.InvokeIt(() => log.AppendText($"{prr.ApplyUpStreamProxy.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                        if (prr.ApplyUpStreamProxy)
                        {
                            this.InvokeIt(() => log.AppendText($"Only Apply To Blocked IPs: "));
                            this.InvokeIt(() => log.AppendText($"{prr.ApplyUpStreamProxyToBlockedIPs.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                            this.InvokeIt(() => log.AppendText($"UpStream Proxy Scheme: "));
                            this.InvokeIt(() => log.AppendText($"{prr.ProxyScheme}{NL}", Color.DodgerBlue));
                        }

                        // Http Status Code
                        HttpStatusCode hsc = HttpStatusCode.RequestTimeout;
                        if (IsProxyRunning)
                            hsc = await NetworkTool.GetHttpStatusCodeAsync(url, null, 5000, false, false, $"socks5://{IPAddress.Loopback}:{ProxyPort}");
                        else
                            hsc = await NetworkTool.GetHttpStatusCodeAsync(url, prr.Dns, 5000, true);
                        
                        this.InvokeIt(() => log.AppendText($"{NL}HTTP Status Code: "));
                        this.InvokeIt(() => log.AppendText($"{hsc}{NL}", Color.DodgerBlue));
                    }
                }
            }

            this.InvokeIt(() => CustomButtonShareRulesStatusRead.Enabled = true);
            this.InvokeIt(() => CustomButtonShareRulesStatusRead.Text = "Read");
        }
        catch (Exception)
        {
            this.InvokeIt(() => CustomButtonShareRulesStatusRead.Enabled = true);
            this.InvokeIt(() => CustomButtonShareRulesStatusRead.Text = "Read");
        }
    }
}