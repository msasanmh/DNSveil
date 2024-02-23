using CustomControls;
using MsmhToolsClass;
using MsmhToolsClass.ProxyServerPrograms;
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
            this.InvokeIt(() => isRulesEnabled = CustomCheckBoxSettingProxyEnableRules.Checked);
            if (!isRulesEnabled && ProxyRulesMode == ProxyProgram.Rules.Mode.Disable)
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

            NetworkTool.GetHostDetails(url, 443, out string host, out _, out _, out int port, out _, out _);

            string rulesPath = string.IsNullOrEmpty(LastProxyRulesPath) ? SecureDNS.ProxyRulesPath : LastProxyRulesPath;
            if (!File.Exists(rulesPath))
            {
                this.InvokeIt(() => CustomButtonShareRulesStatusRead.Enabled = true);
                this.InvokeIt(() => CustomButtonShareRulesStatusRead.Text = "Read");
                return;
            }

            string content = string.Empty;
            try { content = await File.ReadAllTextAsync(rulesPath); } catch (Exception) { }

            if (string.IsNullOrEmpty(LastProxyRulesPath) || !content.Equals(LastProxyRulesContent))
            {
                LastProxyRulesContent = content;

                // Reapply Rules To Proxy Server
                if (IsProxyActivated && !IsProxyActivating) await ApplyPRules();

                CheckProxyRules.Set(ProxyProgram.Rules.Mode.Text, LastProxyRulesContent);
            }

            ProxyProgram.Rules.RulesResult rr = await CheckProxyRules.GetAsync(IPAddress.Loopback.ToString(), host, port);

            this.InvokeIt(() => log.AppendText($"Domain:{NL}"));
            this.InvokeIt(() => log.AppendText($"{host}{NL}", Color.DodgerBlue));

            Color isMatchColor = rr.IsMatch ? Color.MediumSeaGreen : Color.DarkOrange;
            this.InvokeIt(() => log.AppendText($"{NL}Is Match: "));
            this.InvokeIt(() => log.AppendText($"{rr.IsMatch.ToString().CapitalizeFirstLetter()}{NL}", isMatchColor));

            if (rr.IsMatch)
            {
                this.InvokeIt(() => log.AppendText($"Is Black List: "));
                this.InvokeIt(() => log.AppendText($"{rr.IsBlackList.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                if (!rr.IsBlackList)
                {
                    this.InvokeIt(() => log.AppendText($"Is Port Block: "));
                    this.InvokeIt(() => log.AppendText($"{rr.IsPortBlock.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                    if (!rr.IsPortBlock)
                    {
                        // DNS
                        if (!string.IsNullOrEmpty(rr.DnsServer))
                        {
                            this.InvokeIt(() => log.AppendText($"{NL}DNS Server:{NL}"));
                            this.InvokeIt(() => log.AppendText($"{rr.DnsServer}{NL}", Color.DodgerBlue));

                            this.InvokeIt(() => log.AppendText($"Query Domain:{NL}"));
                            this.InvokeIt(() => log.AppendText($"{rr.DnsCustomDomain}{NL}", Color.DodgerBlue));

                            if (!string.IsNullOrEmpty(rr.DnsProxyScheme))
                            {
                                this.InvokeIt(() => log.AppendText($"DNS Proxy Scheme:{NL}"));
                                this.InvokeIt(() => log.AppendText($"{rr.DnsProxyScheme}{NL}", Color.DodgerBlue));
                            }

                            CheckDns checkDns = new(false, false, GetCPUPriority());

                            string bootstrapIP = GetBootstrapSetting(out int bootstrapPort).ToString();
                            bool isSmart = await checkDns.CheckAsSmartDns($"tcp://{bootstrapIP}:{bootstrapPort}", rr.DnsCustomDomain, rr.DnsServer);
                            if (!isSmart && IsDNSConnected)
                            {
                                isSmart = await checkDns.CheckAsSmartDns(IPAddress.Loopback.ToString(), rr.DnsCustomDomain, rr.DnsServer);
                            }

                            this.InvokeIt(() => log.AppendText($"Act As Smart DNS: "));
                            this.InvokeIt(() => log.AppendText($"{isSmart}{NL}", Color.DodgerBlue));
                        }

                        bool isIp = NetworkTool.IsIp(rr.Dns, out _);
                        if (isIp)
                        {
                            this.InvokeIt(() => log.AppendText($"IP: "));
                            this.InvokeIt(() => log.AppendText($"{rr.Dns}{NL}", Color.DodgerBlue));
                        }
                        else
                        {
                            this.InvokeIt(() => log.AppendText($"{NL}IP: "));
                            this.InvokeIt(() => log.AppendText($"Couldn't Get DNS Message{NL}", Color.DarkOrange));
                        }

                        // DPI Bypass
                        this.InvokeIt(() => log.AppendText($"{NL}Apply DPI Bypass: "));
                        this.InvokeIt(() => log.AppendText($"{rr.ApplyDpiBypass.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                        if (!host.Equals(rr.Sni))
                        {
                            this.InvokeIt(() => log.AppendText($"SNI: "));
                            this.InvokeIt(() => log.AppendText($"{rr.Sni}{NL}", Color.DodgerBlue));
                        }

                        // UpStream Proxy
                        this.InvokeIt(() => log.AppendText($"{NL}Apply UpStream Proxy: "));
                        this.InvokeIt(() => log.AppendText($"{rr.ApplyUpStreamProxy.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                        if (rr.ApplyUpStreamProxy)
                        {
                            this.InvokeIt(() => log.AppendText($"Only Apply To Blocked IPs: "));
                            this.InvokeIt(() => log.AppendText($"{rr.ApplyUpStreamProxyToBlockedIPs.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));

                            this.InvokeIt(() => log.AppendText($"UpStream Proxy Scheme: "));
                            this.InvokeIt(() => log.AppendText($"{rr.ProxyScheme}{NL}", Color.DodgerBlue));

                            bool upstreamWorks = await NetworkTool.IsWebsiteOnlineAsync($"{host}:{port}", rr.Dns, 5000, false, rr.ProxyScheme);
                            this.InvokeIt(() => log.AppendText($"Does UpStream Works: "));
                            this.InvokeIt(() => log.AppendText($"{upstreamWorks.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));
                        }

                        // Can Open
                        bool canOpen = false;
                        if (IsProxyRunning)
                            canOpen = await NetworkTool.IsWebsiteOnlineAsync(url, rr.Dns, 5000, false, $"socks5://{IPAddress.Loopback}:{ProxyPort}");
                        else
                            canOpen = await NetworkTool.IsWebsiteOnlineAsync(url, rr.Dns, 5000, true);
                        this.InvokeIt(() => log.AppendText($"{NL}Can Get Headers: "));
                        this.InvokeIt(() => log.AppendText($"{canOpen.ToString().CapitalizeFirstLetter()}{NL}", Color.DodgerBlue));
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