using System.Diagnostics;

namespace MsmhToolsClass;

public class WindowsFirewall
{
    public enum RuleDirection
    {
        IN, OUT
    }

    public enum RuleAction
    {
        Allow, Block, Bypass
    }

    public class RuleSet
    {
        public string RuleName { get; set; } = string.Empty;
        public string ExePath { get; set; } = string.Empty;
        public RuleDirection Direction { get; set; }
        public RuleAction Action { get; set; }
    }

    public static async Task<bool> IsWindowsFirewallEnabledAsync()
    {
        return await Task.Run(async () =>
        {
            try
            {
                string args = $"/c netsh advfirewall show allprofiles | find \"State\"";
                var p = await ProcessManager.ExecuteAsync("cmd", null, args, true, true);
                return p.IsSeccess && p.Output.Contains("ON");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowsFirewall IsWindowsFirewallEnabledAsync: " + ex.Message);
                return false;
            }
        });
    }

    /// <summary>
    /// Check Firewall Rule Exist
    /// </summary>
    public static async Task<bool> IsRuleExistAsync(string ruleName)
    {
        return await Task.Run(async () =>
        {
            try
            {
                string args = $"advfirewall firewall show rule name=\"{ruleName}\"";
                var p = await ProcessManager.ExecuteAsync("netsh", null, args, true, true);
                return p.IsSeccess && p.Output.Contains("Ok.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowsFirewall IsRuleExistAsync: " + ex.Message);
                return false;
            }
        });
    }

    /// <summary>
    /// Add Or Update Firewall Rule
    /// </summary>
    /// <returns>Returns True If Success</returns>
    public static async Task<bool> AddOrUpdateRuleAsync(string ruleName, string exePath, RuleDirection ruleDirection, RuleAction ruleAction)
    {
        return await Task.Run(async () =>
        {
            try
            {
                string dir = ruleDirection == RuleDirection.IN ? "in" : "out";
                string action = ruleAction == RuleAction.Allow ? "allow" : ruleAction == RuleAction.Block ? "block" : "bypass";

                string args = $"advfirewall firewall add rule name=\"{ruleName}\" program=\"{exePath}\" dir={dir} action={action} enable=yes profile=any localip=any remoteip=any protocol=any interfacetype=any";
                bool isRuleExist = await IsRuleExistAsync(ruleName);
                if (isRuleExist)
                    args = $"advfirewall firewall set rule name=\"{ruleName}\" new program=\"{exePath}\" dir={dir} action={action} enable=yes profile=any localip=any remoteip=any protocol=any interfacetype=any";

                var p = await ProcessManager.ExecuteAsync("netsh", null, args, true, true);
                return p.IsSeccess && p.Output.Contains("Ok.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowsFirewall AddOrUpdateRuleAsync 1: " + ex.Message);
                return false;
            }
        });
    }

    /// <summary>
    /// Add Or Update Firewall ProxyRules
    /// </summary>
    public static async Task AddOrUpdateRuleAsync(List<RuleSet> ruleSets)
    {
        await Task.Run(async () =>
        {
            try
            {
                await Parallel.ForEachAsync(ruleSets, async (rule, ct) =>
                {
                    string ruleName = rule.RuleName;
                    string exePath = rule.ExePath;
                    RuleDirection dir = rule.Direction;
                    RuleAction action = rule.Action;

                    await AddOrUpdateRuleAsync(ruleName, exePath, dir, action);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowsFirewall AddOrUpdateRuleAsync 2: " + ex.Message);
            }
        });
    }

    /// <summary>
    /// Add Or Update Firewall ProxyRules
    /// </summary>
    public static void AddOrUpdateRule(List<RuleSet> ruleSets)
    {
        Task.Run(() =>
        {
            try
            {
                Parallel.ForEach(ruleSets, async (rule) =>
                {
                    string ruleName = rule.RuleName;
                    string exePath = rule.ExePath;
                    RuleDirection dir = rule.Direction;
                    RuleAction action = rule.Action;

                    await AddOrUpdateRuleAsync(ruleName, exePath, dir, action);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowsFirewall AddOrUpdateRule: " + ex.Message);
            }
        });
    }

    /// <summary>
    /// Disable Firewall Rule
    /// </summary>
    /// <returns>Returns True If Success</returns>
    public static async Task<bool> DisableRuleAsync(string ruleName)
    {
        return await Task.Run(async () =>
        {
            try
            {
                string args = $"netsh advfirewall firewall set rule name=\"{ruleName}\" new enable=no";
                var p = await ProcessManager.ExecuteAsync("netsh", null, args, true, true);
                return p.IsSeccess && p.Output.Contains("Ok.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowsFirewall DisableRuleAsync: " + ex.Message);
                return false;
            }
        });
    }

    /// <summary>
    /// Enable Firewall Rule
    /// </summary>
    /// <returns>Returns True If Success</returns>
    public static async Task<bool> EnableRuleAsync(string ruleName)
    {
        return await Task.Run(async () =>
        {
            try
            {
                string args = $"netsh advfirewall firewall set rule name=\"{ruleName}\" new enable=yes";
                var p = await ProcessManager.ExecuteAsync("netsh", null, args, true, true);
                return p.IsSeccess && p.Output.Contains("Ok.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowsFirewall EnableRuleAsync: " + ex.Message);
                return false;
            }
        });
    }

    /// <summary>
    /// Delete Firewall Rule
    /// </summary>
    /// <returns>Returns True If Success</returns>
    public static async Task<bool> DeleteRuleAsync(string ruleName)
    {
        return await Task.Run(async () =>
        {
            try
            {
                string args = $"advfirewall firewall delete rule name=\"{ruleName}\"";
                var p = await ProcessManager.ExecuteAsync("netsh", null, args, true, true);
                return p.IsSeccess && p.Output.Contains("Ok.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowsFirewall DeleteRuleAsync: " + ex.Message);
                return false;
            }
        });
    }

}