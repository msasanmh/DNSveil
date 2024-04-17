using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class AgnosticProgram
{
    public partial class ProxyRules
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
        private List<ProxyMainRules> MainRules_List { get; set; } = new();

        private class ProxyMainRules
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

        public ProxyRules() { }

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
                else if (line.StartsWith(Rules.KEYS.BlockPort, StringComparison.InvariantCultureIgnoreCase))
                {
                    string ports = Rules.GetValue(line, Rules.KEYS.BlockPort, null, out bool isList, out List<string> list, Variables);
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
                else if (line.StartsWith(Rules.KEYS.Dns, StringComparison.InvariantCultureIgnoreCase))
                {
                    string dnss = Rules.GetValue(line, Rules.KEYS.Dns, null, out bool isList, out List<string> list, Variables);
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
                else if (line.StartsWith(Rules.KEYS.DnsDomain, StringComparison.InvariantCultureIgnoreCase))
                {
                    Default_DnsDomain = Rules.GetValue(line, Rules.KEYS.DnsDomain, null, out _, out _, Variables);
                }
                else if (line.StartsWith(Rules.KEYS.DnsProxy, StringComparison.InvariantCultureIgnoreCase))
                {
                    Default_DnsProxyScheme = Rules.GetValue(line, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.FirstKey, out _, out _, Variables);
                    Default_DnsProxyUser = Rules.GetValue(line, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.User, out _, out _, Variables);
                    Default_DnsProxyPass = Rules.GetValue(line, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.Pass, out _, out _, Variables);
                }
                else if (line.StartsWith(Rules.KEYS.Sni, StringComparison.InvariantCultureIgnoreCase))
                {
                    Default_Sni = Rules.GetValue(line, Rules.KEYS.Sni, null, out _, out _, Variables);
                }
                else if (line.StartsWith(Rules.KEYS.Proxy, StringComparison.InvariantCultureIgnoreCase))
                {
                    Default_ProxyScheme = Rules.GetValue(line, Rules.KEYS.Proxy, Rules.SUB_KEYS.FirstKey, out _, out _, Variables);
                    string ifBlock = Rules.GetValue(line, Rules.KEYS.Proxy, Rules.SUB_KEYS.IfBlock, out _, out _, Variables).ToLower().Trim();
                    if (!string.IsNullOrEmpty(ifBlock))
                        Default_ProxyIfBlock = ifBlock.Equals("1") || ifBlock.Equals("true");
                    Default_ProxyUser = Rules.GetValue(line, Rules.KEYS.Proxy, Rules.SUB_KEYS.User, out _, out _, Variables);
                    Default_ProxyPass = Rules.GetValue(line, Rules.KEYS.Proxy, Rules.SUB_KEYS.Pass, out _, out _, Variables);
                }

                // Get ProxyMainRules (Client|Domain|ProxyRules)
                else if (line.Contains('|'))
                {
                    string[] split = line.Split('|');
                    if (split.Length == 2) SetDomainRules(Rules.KEYS.AllClients, split[0].Trim(), split[1].Trim());
                    if (split.Length == 3) SetDomainRules(split[0].Trim(), split[1].Trim(), split[2].Trim());
                }
            }
        }

        private void SetDomainRules(string client, string domain, string rules) // rules Ends With ;
        {
            try
            {
                ProxyMainRules pmr = new()
                {
                    Client = client, // Client
                    Domain = domain // Domain
                };

                // Block
                if (rules.Equals("block;", StringComparison.InvariantCultureIgnoreCase)) pmr.IsBlock = true;
                if (rules.Equals("-;")) pmr.IsBlock = true;

                if (!pmr.IsBlock)
                {
                    // No Bypass
                    if (rules.Contains("nobypass;", StringComparison.InvariantCultureIgnoreCase)) pmr.NoBypass = true;
                    if (rules.Contains("--;")) pmr.NoBypass = true;

                    // Fake DNS
                    string fakeDnsIpStr = Rules.GetValue(rules, Rules.KEYS.FirstKey, null, out _, out _, Variables);
                    bool isIp = NetworkTool.IsIp(fakeDnsIpStr, out _);
                    if (isIp) pmr.FakeDns = fakeDnsIpStr;
                    
                    // BlockPort
                    string ports = Rules.GetValue(rules, Rules.KEYS.BlockPort, null, out bool isList, out List<string> list, Variables);
                    if (!isList) // One Port
                    {
                        bool success = int.TryParse(ports, out int port);
                        if (success) pmr.BlockPort.Add(port);
                    }
                    else // Multiple Ports
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string portStr = list[i];
                            bool success = int.TryParse(portStr, out int port);
                            if (success) pmr.BlockPort.Add(port);
                        }
                    }
                    if (Default_BlockPort.Any())
                    {
                        try
                        {
                            pmr.BlockPort = pmr.BlockPort.Concat(Default_BlockPort).ToList();
                            pmr.BlockPort = pmr.BlockPort.Distinct().ToList();
                        }
                        catch (Exception) { }
                    }

                    // Dnss
                    string dnss = Rules.GetValue(rules, Rules.KEYS.Dns, null, out isList, out list, Variables);
                    if (!isList) // One Dns
                    {
                        if (!string.IsNullOrEmpty(dnss))
                            pmr.Dnss.Add(dnss);
                    }
                    else // Multiple Dnss
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string dns = list[i];
                            if (!string.IsNullOrEmpty(dns))
                                pmr.Dnss.Add(dns);
                        }
                    }

                    if (!pmr.Dnss.Any() && Default_Dnss.Any()) pmr.Dnss = Default_Dnss;

                    // DnsDomain
                    pmr.DnsDomain = Rules.GetValue(rules, Rules.KEYS.DnsDomain, null, out _, out _, Variables);
                    if (string.IsNullOrEmpty(pmr.DnsDomain)) pmr.DnsDomain = Default_DnsDomain;

                    // DnsProxy e.g. socks5://127.0.0.1:6666&user:UserName&pass:PassWord
                    pmr.DnsProxyScheme = Rules.GetValue(rules, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.FirstKey, out _, out _, Variables);
                    pmr.DnsProxyUser = Rules.GetValue(rules, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.User, out _, out _, Variables);
                    pmr.DnsProxyPass = Rules.GetValue(rules, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.Pass, out _, out _, Variables);
                    if (string.IsNullOrEmpty(pmr.DnsProxyScheme))
                    {
                        pmr.DnsProxyScheme = Default_DnsProxyScheme;
                        pmr.DnsProxyUser = Default_DnsProxyUser;
                        pmr.DnsProxyPass = Default_DnsProxyPass;
                    }

                    // SNI
                    pmr.Sni = Rules.GetValue(rules, Rules.KEYS.Sni, null, out _, out _, Variables);
                    if (string.IsNullOrEmpty(pmr.Sni)) pmr.Sni = Default_Sni;

                    // Proxy e.g. socks5://127.0.0.1:6666&ifblock:1&user:UserName&pass:PassWord
                    pmr.ProxyScheme = Rules.GetValue(rules, Rules.KEYS.Proxy, Rules.SUB_KEYS.FirstKey, out _, out _, Variables);
                    string ifBlock = Rules.GetValue(rules, Rules.KEYS.Proxy, Rules.SUB_KEYS.IfBlock, out _, out _, Variables).ToLower().Trim();
                    if (!string.IsNullOrEmpty(ifBlock))
                        pmr.ProxyIfBlock = ifBlock.Equals("1") || ifBlock.Equals("true");
                    pmr.ProxyUser = Rules.GetValue(rules, Rules.KEYS.Proxy, Rules.SUB_KEYS.User, out _, out _, Variables);
                    pmr.ProxyPass = Rules.GetValue(rules, Rules.KEYS.Proxy, Rules.SUB_KEYS.Pass, out _, out _, Variables);
                    if (string.IsNullOrEmpty(pmr.ProxyScheme))
                    {
                        pmr.ProxyScheme = Default_ProxyScheme;
                        pmr.ProxyIfBlock = Default_ProxyIfBlock;
                        pmr.ProxyUser = Default_ProxyUser;
                        pmr.ProxyPass = Default_ProxyPass;
                    }
                }

                MainRules_List.Add(pmr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Proxy Rules_SetDomainRules: " + ex.Message);
            }
        }

    }
}