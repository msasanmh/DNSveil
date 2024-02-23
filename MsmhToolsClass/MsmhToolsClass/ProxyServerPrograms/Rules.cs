using System.Diagnostics;

namespace MsmhToolsClass.ProxyServerPrograms;

public partial class ProxyProgram
{
    public partial class Rules
    {
        public enum Mode
        {
            File,
            Text,
            Disable
        }

        public Mode RulesMode { get; private set; } = Mode.Disable;
        public string PathOrText { get; private set; } = string.Empty;
        public string TextContent { get; private set; } = string.Empty;

        private List<string> Rules_List { get; set; } = new();
        private List<Tuple<string, string>> Variables { get; set; } = new(); // x = domain.com;
        private List<int> Default_BlockPort { get; set; } = new(); // blockport:80,53;
        private List<string> Default_Dnss { get; set; } = new(); // dns:
        private string Default_DnsDomain { get; set; } = string.Empty; // dnsdomain:;
        private string Default_DnsProxyScheme { get; set; } = string.Empty; // dnsproxy:;
        private string Default_DnsProxyUser { get; set; } = string.Empty; // &user:
        private string Default_DnsProxyPass { get; set; } = string.Empty; // &pass:
        private string Default_Sni { get; set; } = string.Empty; // sni:;
        private string Default_ProxyScheme { get; set; } = string.Empty; // proxy:;
        private bool Default_ProxyIfBlock { get; set; } = false; // &ifblock:1
        private string Default_ProxyUser { get; set; } = string.Empty; // &user:
        private string Default_ProxyPass { get; set; } = string.Empty; // &pass:
        private List<MainRules> MainRules_List { get; set; } = new();

        private readonly struct KEYS
        {
            public static readonly string FirstKey = "FirstKey";
            public static readonly string AllClients = "AllClients";
            public static readonly string BlockPort = "blockport:";
            public static readonly string Dns = "dns:";
            public static readonly string DnsDomain = "dnsdomain:";
            public static readonly string DnsProxy = "dnsproxy:";
            public static readonly string Sni = "sni:";
            public static readonly string Proxy = "proxy:";
        }

        private readonly struct SUB_KEYS
        {
            public static readonly string FirstKey = "FirstKey";
            public static readonly string IfBlock = "&ifblock:";
            public static readonly string User = "&user:";
            public static readonly string Pass = "&pass:";
        }

        private class MainRules
        {
            public string Client { get; set; } = string.Empty;
            public string Domain { get; set; } = string.Empty;
            public bool IsBlock { get; set; } = false;
            public List<int> BlockPort { get; set; } = new();
            public bool NoBypass { get; set; } = false;
            public string FakeDns { get; set; } = string.Empty;
            public List<string> Dnss { get; set; } = new();
            public string DnsDomain { get; set; } = string.Empty;
            public string DnsProxyScheme { get; set; } = string.Empty;
            public string DnsProxyUser { get; set; } = string.Empty;
            public string DnsProxyPass { get; set; } = string.Empty;
            public string Sni { get; set; } = string.Empty;
            public string ProxyScheme { get; set; } = string.Empty;
            public bool ProxyIfBlock { get; set; } = false;
            public string ProxyUser { get; set; } = string.Empty;
            public string ProxyPass { get; set; } = string.Empty;
        }

        public Rules() { }

        public void Set(Mode mode, string filePathOrText)
        {
            Rules_List.Clear();
            Variables.Clear();
            Default_BlockPort.Clear();
            Default_Dnss.Clear();
            Default_DnsDomain = string.Empty;
            Default_DnsProxyScheme = string.Empty;
            Default_DnsProxyUser = string.Empty;
            Default_DnsProxyPass = string.Empty;
            Default_Sni = string.Empty;
            Default_ProxyScheme = string.Empty;
            Default_ProxyIfBlock = false;
            Default_ProxyUser = string.Empty;
            Default_ProxyPass = string.Empty;
            MainRules_List.Clear();

            RulesMode = mode;
            PathOrText = filePathOrText;

            if (RulesMode == Mode.Disable) return;

            if (RulesMode == Mode.File)
            {
                try
                {
                    TextContent = File.ReadAllText(Path.GetFullPath(filePathOrText));
                }
                catch (Exception) { }
            }
            else if (RulesMode == Mode.Text) TextContent = filePathOrText;

            if (string.IsNullOrEmpty(TextContent) || string.IsNullOrWhiteSpace(TextContent)) return;

            TextContent += Environment.NewLine;
            Rules_List = TextContent.SplitToLines();
            
            for (int n = 0; n < Rules_List.Count; n++)
            {
                string line = Rules_List[n].Trim();
                if (line.StartsWith("//")) continue; // Support Comment //
                if (!line.EndsWith(';')) continue; // Must Have ; At The End
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue; // Line Cannot Be Empty

                // Get Variables
                if (line.Contains('=') && !line.Contains(',') && !line.Contains('&'))
                {
                    line = line.TrimEnd(';');
                    string[] split = line.Split('=');
                    if (split.Length == 2)
                    {
                        string item1 = split[0].Trim();
                        string item2 = split[1].Trim();
                        if (!string.IsNullOrEmpty(item1) && !string.IsNullOrEmpty(item2))
                            Variables.Add(new Tuple<string, string>(item1, item2));
                    }
                }

                // Get Defaults
                else if (line.StartsWith(KEYS.BlockPort, StringComparison.InvariantCultureIgnoreCase))
                {
                    string ports = GetValue(line, KEYS.BlockPort, null, out bool isList, out List<string> list);
                    if (!isList) // One Port
                    {
                        bool success = int.TryParse(ports, out int port);
                        if (success) Default_BlockPort.Add(port);
                    }
                    else // Multiple Ports
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string portStr = list[i];
                            bool success = int.TryParse(portStr, out int port);
                            if (success) Default_BlockPort.Add(port);
                        }
                    }
                    continue;
                }
                else if (line.StartsWith(KEYS.Dns, StringComparison.InvariantCultureIgnoreCase))
                {
                    string dnss = GetValue(line, KEYS.Dns, null, out bool isList, out List<string> list);
                    if (!isList) // One Dns
                    {
                        if (!string.IsNullOrEmpty(dnss))
                            Default_Dnss.Add(dnss);
                    }
                    else // Multiple Dnss
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string dns = list[i];
                            if (!string.IsNullOrEmpty(dns))
                                Default_Dnss.Add(dns);
                        }
                    }
                }
                else if (line.StartsWith(KEYS.DnsDomain, StringComparison.InvariantCultureIgnoreCase))
                {
                    Default_DnsDomain = GetValue(line, KEYS.DnsDomain, null, out _, out _);
                }
                else if (line.StartsWith(KEYS.DnsProxy, StringComparison.InvariantCultureIgnoreCase))
                {
                    Default_DnsProxyScheme = GetValue(line, KEYS.DnsProxy, SUB_KEYS.FirstKey, out _, out _);
                    Default_DnsProxyUser = GetValue(line, KEYS.DnsProxy, SUB_KEYS.User, out _, out _);
                    Default_DnsProxyPass = GetValue(line, KEYS.DnsProxy, SUB_KEYS.Pass, out _, out _);
                }
                else if (line.StartsWith(KEYS.Sni, StringComparison.InvariantCultureIgnoreCase))
                {
                    Default_Sni = GetValue(line, KEYS.Sni, null, out _, out _);
                }
                else if (line.StartsWith(KEYS.Proxy, StringComparison.InvariantCultureIgnoreCase))
                {
                    Default_ProxyScheme = GetValue(line, KEYS.Proxy, SUB_KEYS.FirstKey, out _, out _);
                    string ifBlock = GetValue(line, KEYS.Proxy, SUB_KEYS.IfBlock, out _, out _).ToLower().Trim();
                    if (!string.IsNullOrEmpty(ifBlock))
                        Default_ProxyIfBlock = ifBlock.Equals("1") || ifBlock.Equals("true");
                    Default_ProxyUser = GetValue(line, KEYS.Proxy, SUB_KEYS.User, out _, out _);
                    Default_ProxyPass = GetValue(line, KEYS.Proxy, SUB_KEYS.Pass, out _, out _);
                }

                // Get MainRules (Client|Domain|Rules)
                else if (line.Contains('|'))
                {
                    string[] split = line.Split('|');
                    if (split.Length == 2) SetDomainRules(KEYS.AllClients, split[0].Trim(), split[1].Trim());
                    if (split.Length == 3) SetDomainRules(split[0].Trim(), split[1].Trim(), split[2].Trim());
                }
            }
        }

        private void SetDomainRules(string client, string domain, string rules) // rules Ends With ;
        {
            try
            {
                MainRules mr = new();
                mr.Client = client; // Client
                mr.Domain = domain; // Domain

                // Block
                if (rules.Equals("block;", StringComparison.InvariantCultureIgnoreCase)) mr.IsBlock = true;
                if (rules.Equals("-;")) mr.IsBlock = true;

                if (!mr.IsBlock)
                {
                    // No Bypass
                    if (rules.Contains("nobypass;", StringComparison.InvariantCultureIgnoreCase)) mr.NoBypass = true;
                    if (rules.Contains("--;")) mr.NoBypass = true;

                    // Fake DNS
                    string fakeDnsIpStr = GetValue(rules, KEYS.FirstKey, null, out _, out _);
                    bool isIp = NetworkTool.IsIp(fakeDnsIpStr, out _);
                    if (isIp) mr.FakeDns = fakeDnsIpStr;
                    
                    // BlockPort
                    string ports = GetValue(rules, KEYS.BlockPort, null, out bool isList, out List<string> list);
                    if (!isList) // One Port
                    {
                        bool success = int.TryParse(ports, out int port);
                        if (success) mr.BlockPort.Add(port);
                    }
                    else // Multiple Ports
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string portStr = list[i];
                            bool success = int.TryParse(portStr, out int port);
                            if (success) mr.BlockPort.Add(port);
                        }
                    }
                    if (Default_BlockPort.Any())
                    {
                        try
                        {
                            mr.BlockPort = mr.BlockPort.Concat(Default_BlockPort).ToList();
                            mr.BlockPort = mr.BlockPort.Distinct().ToList();
                        }
                        catch (Exception) { }
                    }

                    // Dnss
                    string dnss = GetValue(rules, KEYS.Dns, null, out isList, out list);
                    if (!isList) // One Dns
                    {
                        if (!string.IsNullOrEmpty(dnss))
                            mr.Dnss.Add(dnss);
                    }
                    else // Multiple Dnss
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string dns = list[i];
                            if (!string.IsNullOrEmpty(dns))
                                mr.Dnss.Add(dns);
                        }
                    }
                    if (Default_Dnss.Any())
                    {
                        try
                        {
                            mr.Dnss = mr.Dnss.Concat(Default_Dnss).ToList();
                            mr.Dnss = mr.Dnss.Distinct().ToList();
                        }
                        catch (Exception) { }
                    }

                    // DnsDomain
                    mr.DnsDomain = GetValue(rules, KEYS.DnsDomain, null, out _, out _);
                    if (string.IsNullOrEmpty(mr.DnsDomain)) mr.DnsDomain = Default_DnsDomain;

                    // DnsProxy e.g. socks5://127.0.0.1:6666&user:UserName&pass:PassWord
                    mr.DnsProxyScheme = GetValue(rules, KEYS.DnsProxy, SUB_KEYS.FirstKey, out _, out _);
                    mr.DnsProxyUser = GetValue(rules, KEYS.DnsProxy, SUB_KEYS.User, out _, out _);
                    mr.DnsProxyPass = GetValue(rules, KEYS.DnsProxy, SUB_KEYS.Pass, out _, out _);
                    if (string.IsNullOrEmpty(mr.DnsProxyScheme))
                    {
                        mr.DnsProxyScheme = Default_DnsProxyScheme;
                        mr.DnsProxyUser = Default_DnsProxyUser;
                        mr.DnsProxyPass = Default_DnsProxyPass;
                    }

                    // SNI
                    mr.Sni = GetValue(rules, KEYS.Sni, null, out _, out _);
                    if (string.IsNullOrEmpty(mr.Sni)) mr.Sni = Default_Sni;

                    // Proxy e.g. socks5://127.0.0.1:6666&ifblock:1&user:UserName&pass:PassWord
                    mr.ProxyScheme = GetValue(rules, KEYS.Proxy, SUB_KEYS.FirstKey, out _, out _);
                    string ifBlock = GetValue(rules, KEYS.Proxy, SUB_KEYS.IfBlock, out _, out _).ToLower().Trim();
                    if (!string.IsNullOrEmpty(ifBlock))
                        mr.ProxyIfBlock = ifBlock.Equals("1") || ifBlock.Equals("true");
                    mr.ProxyUser = GetValue(rules, KEYS.Proxy, SUB_KEYS.User, out _, out _);
                    mr.ProxyPass = GetValue(rules, KEYS.Proxy, SUB_KEYS.Pass, out _, out _);
                    if (string.IsNullOrEmpty(mr.ProxyScheme))
                    {
                        mr.ProxyScheme = Default_ProxyScheme;
                        mr.ProxyIfBlock = Default_ProxyIfBlock;
                        mr.ProxyUser = Default_ProxyUser;
                        mr.ProxyPass = Default_ProxyPass;
                    }
                }

                MainRules_List.Add(mr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Proxy Rules_SetDomainRules: " + ex.Message);
            }
        }

        private string GetValue(string line, string key, string? subKey, out bool isList, out List<string> list)
        {
            string result = line.Trim();
            isList = false;
            list = new();

            try
            {
                if (result.Contains(key, StringComparison.InvariantCultureIgnoreCase) || key == KEYS.FirstKey)
                {
                    try
                    {
                        if (key == KEYS.FirstKey)
                        {
                            result = result.Remove(result.IndexOf(';'));
                            result = result.Trim();
                        }
                        else
                        {
                            result = result.Remove(0, result.IndexOf(key, StringComparison.InvariantCultureIgnoreCase) + key.Length);
                            result = result.Remove(result.IndexOf(';'));
                            result = result.Trim();
                        }
                    }
                    catch (Exception) { }

                    if (!string.IsNullOrEmpty(subKey))
                    {
                        if (subKey.Equals(SUB_KEYS.FirstKey, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (result.Contains('&'))
                            {
                                try
                                {
                                    result = result.Remove(result.IndexOf('&'));
                                }
                                catch (Exception) { }
                            }
                        }
                        else
                        {
                            if (result.Contains(subKey, StringComparison.InvariantCultureIgnoreCase))
                            {
                                try
                                {
                                    result = result.Remove(0, result.IndexOf(subKey, StringComparison.InvariantCultureIgnoreCase) + subKey.Length);
                                }
                                catch (Exception) { }

                                if (result.Contains('&') && result.Contains(':'))
                                {
                                    try
                                    {
                                        result = result.Remove(result.IndexOf('&'));
                                    }
                                    catch (Exception) { }
                                }
                            }
                        }
                    }

                    if (!result.Contains(','))
                    {
                        // Not A List
                        return ApplyVariables(result);
                    }
                    else
                    {
                        // It's A List
                        isList = true;
                        string[] split = result.Split(',');
                        for (int n = 0; n < split.Length; n++)
                        {
                            string value = split[n].Trim();
                            list.Add(ApplyVariables(value));
                        }
                        if (list.Any()) return list[0];
                    }
                }
            }
            catch (Exception) { }

            return string.Empty;
        }

        private string ApplyVariables(string vari)
        {
            string result = vari;
            try
            {
                List<Tuple<string, string>> variables = Variables.ToList();
                for (int n = 0; n < variables.Count; n++)
                {
                    Tuple<string, string> tuple = variables[n];
                    if (vari.Equals(tuple.Item1))
                    {
                        result = tuple.Item2; break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Proxy Rules_ApplyVariables: " + ex.Message);
            }
            return result;
        }

    }
}