using MsmhToolsClass;
using static MsmhToolsClass.WindowsFirewall;

namespace SecureDNSClient;

public partial class FormMain
{
    public async Task AddOrUpdateFirewallRules()
    {
        await Task.Run(async () =>
        {
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL));

            bool isFwEnabled = await IsWindowsFirewallEnabledAsync();
            if (!isFwEnabled)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Windows Firewall Is Not Enabled.{NL}", Color.OrangeRed));
                return;
            }

            List<RuleSet> rules = new()
            {
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcIn, ExePath = SecureDNS.CurrentExecutablePath, Direction = RuleDirection.IN, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcOut, ExePath = SecureDNS.CurrentExecutablePath, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcDnsLookupIn, ExePath = SecureDNS.DnsLookup, Direction = RuleDirection.IN, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcDnsLookupOut, ExePath = SecureDNS.DnsLookup, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcAgnosticServerIn, ExePath = SecureDNS.AgnosticServerPath, Direction = RuleDirection.IN, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcAgnosticServerOut, ExePath = SecureDNS.AgnosticServerPath, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcGoodbyeDpiIn, ExePath = SecureDNS.GoodbyeDpi, Direction = RuleDirection.IN, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcGoodbyeDpiOut, ExePath = SecureDNS.GoodbyeDpi, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivertIn, ExePath = SecureDNS.WinDivert, Direction = RuleDirection.IN, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivertOut, ExePath = SecureDNS.WinDivert, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivert32In, ExePath = SecureDNS.WinDivert32, Direction = RuleDirection.IN, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivert32Out, ExePath = SecureDNS.WinDivert32, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivert64In, ExePath = SecureDNS.WinDivert64, Direction = RuleDirection.IN, Action = RuleAction.Allow},
                new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivert64Out, ExePath = SecureDNS.WinDivert64, Direction = RuleDirection.OUT, Action = RuleAction.Allow}
            };

            for (int n = 0; n < rules.Count; n++)
            {
                if (IsExiting) break;

                RuleSet rule = rules[n];
                string ruleName = rule.RuleName;
                string exePath = rule.ExePath;
                RuleDirection dir = rule.Direction;
                RuleAction action = rule.Action;

                bool re = await IsRuleExistAsync(ruleName);
                string msg = re ? $"Updating Firewall" : $"Creating Firewall";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                msg = dir == RuleDirection.IN ? " Inbound " : " Outbound ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.Orange));
                msg = "Rule for ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                msg = $"{Path.GetFileName(exePath)}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.DodgerBlue));
                msg = $"... ";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));
                bool success = await AddOrUpdateRuleAsync(ruleName, exePath, dir, action);
                msg = success ? $"Success{NL}" : $"Failed{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, success ? Color.MediumSeaGreen : Color.IndianRed));
            }
        });
    }

    public static async void AddOrUpdateFirewallRulesNoLog()
    {
        if (!await IsWindowsFirewallEnabledAsync()) return;

        List<RuleSet> rules = new()
        {
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcIn, ExePath = SecureDNS.CurrentExecutablePath, Direction = RuleDirection.IN, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcOut, ExePath = SecureDNS.CurrentExecutablePath, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcDnsLookupIn, ExePath = SecureDNS.DnsLookup, Direction = RuleDirection.IN, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcDnsLookupOut, ExePath = SecureDNS.DnsLookup, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcAgnosticServerIn, ExePath = SecureDNS.AgnosticServerPath, Direction = RuleDirection.IN, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcAgnosticServerOut, ExePath = SecureDNS.AgnosticServerPath, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcGoodbyeDpiIn, ExePath = SecureDNS.GoodbyeDpi, Direction = RuleDirection.IN, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcGoodbyeDpiOut, ExePath = SecureDNS.GoodbyeDpi, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivertIn, ExePath = SecureDNS.WinDivert, Direction = RuleDirection.IN, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivertOut, ExePath = SecureDNS.WinDivert, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivert32In, ExePath = SecureDNS.WinDivert32, Direction = RuleDirection.IN, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivert32Out, ExePath = SecureDNS.WinDivert32, Direction = RuleDirection.OUT, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivert64In, ExePath = SecureDNS.WinDivert64, Direction = RuleDirection.IN, Action = RuleAction.Allow},
            new RuleSet{ RuleName = SecureDNS.FirewallRule_SdcWinDivert64Out, ExePath = SecureDNS.WinDivert64, Direction = RuleDirection.OUT, Action = RuleAction.Allow}
        };

        AddOrUpdateRule(rules);
    }

}