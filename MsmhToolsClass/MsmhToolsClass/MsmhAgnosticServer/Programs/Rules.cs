using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class AgnosticProgram
{
    public partial class Rules
    {
        public enum Mode
        {
            File,
            Text,
            Disable
        }

        public enum AddressType
        {
            Domain,
            IP,
            CIDR,
            None
        }

        public Mode RulesMode { get; private set; } = Mode.Disable;
        public string PathOrText { get; private set; } = string.Empty;
        public string TextContent { get; private set; } = string.Empty;

        public List<Tuple<string, string>> Variables { get; set; } = new(); // x = domain.com;
        public Defaults Default { get; set; } = new();
        public List<Rule> RuleList { get; set; } = new();

        private List<string> RuleLineList { get; set; } = new();

        public class Defaults
        {
            public List<int> BlockPort { get; set; } = new(); // blockport:80,53;
            public List<string> Dnss { get; set; } = new(); // dns:;
            public string DnsDomain { get; set; } = string.Empty; // dnsdomain:;
            public string DnsProxyScheme { get; set; } = string.Empty; // dnsproxy:;
            public string DnsProxyUser { get; set; } = string.Empty; // &user:
            public string DnsProxyPass { get; set; } = string.Empty; // &pass:
            public string Sni { get; set; } = string.Empty; // sni:;
            public string ProxyScheme { get; set; } = string.Empty; // proxy:;
            public bool ProxyIfBlock { get; set; } = false; // &ifblock:1
            public string ProxyUser { get; set; } = string.Empty; // &user:
            public string ProxyPass { get; set; } = string.Empty; // &pass:
        }

        public class Rule
        {
            public string Client { get; set; } = Rules_Init.KEYS.AllClients;
            public string Address { get; set; } = string.Empty;
            public AddressType AddressType => GetAddressType(Address);
            public bool IsBlock { get; set; } = false;
            public List<int> BlockPort { get; set; } = new();
            public string BlockPortString => BlockPort.ToString(", ");
            public string FakeDnsIP { get; set; } = string.Empty;
            public List<string> Dnss { get; set; } = new();
            public string DnssString => Dnss.ToString(Environment.NewLine);
            public string DnsDomain { get; set; } = string.Empty;
            public string DnsProxyScheme { get; set; } = string.Empty;
            public string DnsProxyUser { get; set; } = string.Empty;
            public string DnsProxyPass { get; set; } = string.Empty;
            public bool IsDirect { get; set; } = false;
            public string Sni { get; set; } = string.Empty;
            public string ProxyScheme { get; set; } = string.Empty;
            public bool ProxyIfBlock { get; set; } = false;
            public string ProxyUser { get; set; } = string.Empty;
            public string ProxyPass { get; set; } = string.Empty;
            public string ToString(bool includeAddress)
            {
                string result = string.Empty;
                try
                {
                    string nl = Environment.NewLine;
                    if (includeAddress)
                    {
                        if (!string.IsNullOrEmpty(Client)) result += $"Client: {Client}";
                        if (!string.IsNullOrEmpty(Address))
                        {
                            if (!string.IsNullOrEmpty(result)) result += ", ";
                            result += $"Address: {Address}, Type: {AddressType}";
                        }
                        if (!string.IsNullOrEmpty(result)) result += nl;
                    }

                    if (IsBlock) result += $"Is Block: {IsBlock}";
                    else
                    {
                        AddressType addressType = AddressType;

                        if (BlockPort.Count > 0) result += $"Block Ports: {BlockPort.ToString(", ")}{nl}";
                        if (NetworkTool.IsIP(FakeDnsIP, out _))
                        {
                            result += $"Fake DNS IP: {FakeDnsIP}{nl}";
                        }
                        else
                        {
                            if (addressType == AddressType.Domain)
                            {
                                if (Dnss.Count > 0)
                                {
                                    result += $"DNS Addresses: {Dnss.ToString(", ")}{nl}";
                                }
                                if (!string.IsNullOrEmpty(DnsDomain))
                                {
                                    result += $"DNS Custom Domain: {DnsDomain}{nl}";
                                }
                                if (!string.IsNullOrEmpty(DnsProxyScheme))
                                {
                                    result += $"DNS Proxy: {DnsProxyScheme}";
                                    if (!string.IsNullOrEmpty(DnsProxyUser)) result += $", User: {DnsProxyUser}";
                                    if (!string.IsNullOrEmpty(DnsProxyPass)) result += $", Pass: {DnsProxyPass}";
                                    result += nl;
                                }
                            }
                        }

                        if (IsDirect) result += $"Is Direct: {IsDirect}";
                        else
                        {
                            if (addressType == AddressType.Domain && !string.IsNullOrEmpty(Sni)) result += $"Fake SNI: {Sni}{nl}";
                            if (!string.IsNullOrEmpty(ProxyScheme))
                            {
                                result += $"Up Stream Proxy: {ProxyScheme}";
                                if (!string.IsNullOrEmpty(ProxyUser)) result += $", User: {ProxyUser}";
                                if (!string.IsNullOrEmpty(ProxyPass)) result += $", Pass: {ProxyPass}";
                            }
                        }
                    }

                    result = result.TrimEnd(nl);
                }
                catch (Exception) { }
                return result;
            }
            public string Report => ToString(false);
        }

        public Rules(Mode mode)
        {
            RulesMode = mode;
        }

        public Rules() { }

        public async Task SetAsync(Mode mode, string filePathOrText)
        {
            try
            {
                RuleLineList.Clear();
                Variables.Clear();
                RuleList.Clear();

                RulesMode = mode;
                PathOrText = filePathOrText;

                if (RulesMode == Mode.Disable) return;

                if (RulesMode == Mode.File)
                {
                    try
                    {
                        TextContent = await File.ReadAllTextAsync(Path.GetFullPath(filePathOrText));
                    }
                    catch (Exception) { }
                }
                else if (RulesMode == Mode.Text) TextContent = filePathOrText;

                if (string.IsNullOrEmpty(TextContent) || string.IsNullOrWhiteSpace(TextContent)) return;

                TextContent += Environment.NewLine;
                RuleLineList = TextContent.SplitToLines();

                for (int n = 0; n < RuleLineList.Count; n++)
                {
                    string line = RuleLineList[n].Trim();
                    if (line.StartsWith("//")) continue; // Support Comment //
                    if (!line.EndsWith(';')) continue; // Must Have ; At The End
                    if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue; // Line Cannot Be Empty

                    // Get Variables
                    if (line.Contains('=') && !line.Contains(',') && !line.Contains('&') && !line.Contains('|'))
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
                    else if (line.StartsWith(Rules_Init.KEYS.BlockPort, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string ports = Rules_Init.GetValue(line, Rules_Init.KEYS.BlockPort, null, true, out bool isList, out List<string> list, Variables);
                        if (!isList) // One Port
                        {
                            bool success = int.TryParse(ports, out int port);
                            if (success) Default.BlockPort.Add(port);
                        }
                        else // Multiple Ports
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                string portStr = list[i];
                                bool success = int.TryParse(portStr, out int port);
                                if (success) Default.BlockPort.Add(port);
                            }
                        }
                        continue;
                    }
                    else if (line.StartsWith(Rules_Init.KEYS.Dns, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string dnss = Rules_Init.GetValue(line, Rules_Init.KEYS.Dns, null, true, out bool isList, out List<string> list, Variables);
                        if (!isList) // One Dns
                        {
                            if (!string.IsNullOrEmpty(dnss))
                                Default.Dnss.Add(dnss);
                        }
                        else // Multiple Dnss
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                string dns = list[i];
                                if (!string.IsNullOrEmpty(dns))
                                    Default.Dnss.Add(dns);
                            }
                        }
                    }
                    else if (line.StartsWith(Rules_Init.KEYS.DnsDomain, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Default.DnsDomain = Rules_Init.GetValue(line, Rules_Init.KEYS.DnsDomain, null, false, out _, out _, Variables);
                    }
                    else if (line.StartsWith(Rules_Init.KEYS.DnsProxy, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Default.DnsProxyScheme = Rules_Init.GetValue(line, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.FirstKey, false, out _, out _, Variables);
                        Default.DnsProxyUser = Rules_Init.GetValue(line, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.User, false, out _, out _, Variables);
                        Default.DnsProxyPass = Rules_Init.GetValue(line, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.Pass, false, out _, out _, Variables);
                    }
                    else if (line.StartsWith(Rules_Init.KEYS.Sni, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Default.Sni = Rules_Init.GetValue(line, Rules_Init.KEYS.Sni, null, false, out _, out _, Variables);
                    }
                    else if (line.StartsWith(Rules_Init.KEYS.Proxy, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Default.ProxyScheme = Rules_Init.GetValue(line, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.FirstKey, false, out _, out _, Variables);
                        string ifBlock = Rules_Init.GetValue(line, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.IfBlock, false, out _, out _, Variables).ToLower().Trim();
                        if (!string.IsNullOrEmpty(ifBlock))
                            Default.ProxyIfBlock = ifBlock.Equals("1") || ifBlock.Equals("true", StringComparison.OrdinalIgnoreCase);
                        Default.ProxyUser = Rules_Init.GetValue(line, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.User, false, out _, out _, Variables);
                        Default.ProxyPass = Rules_Init.GetValue(line, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.Pass, false, out _, out _, Variables);
                    }

                    // Get ProxyMainRules (Client|Address|ProxyRules)
                    // Address: Domain, Subdomain, IP, CIDR
                    else if (line.Contains('|'))
                    {
                        string[] split = line.Split('|');
                        if (split.Length == 2) await Set_FullAddress_Rules_Async(Rules_Init.KEYS.AllClients, split[0].Trim(), split[1].Trim());
                        if (split.Length == 3) await Set_FullAddress_Rules_Async(split[0].Trim(), split[1].Trim(), split[2].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Rules SetAsync: " + ex.Message);
            }
        }

        private async Task Set_FullAddress_Rules_Async(string client, string fullAddress, string rules) // rules Ends With ;
        {
            await Task.Run(async () =>
            {
                try
                {
                    if (fullAddress.StartsWith('"')) fullAddress = fullAddress.TrimStart('"');
                    if (fullAddress.EndsWith('"')) fullAddress = fullAddress.TrimEnd('"');
                    if (fullAddress.StartsWith('/')) fullAddress = fullAddress.TrimStart('/');
                    if (fullAddress.EndsWith('/')) fullAddress = fullAddress.TrimEnd('/');
                    if (fullAddress.StartsWith('[') && fullAddress.EndsWith(']'))
                    {
                        fullAddress = fullAddress.TrimStart('[');
                        fullAddress = fullAddress.TrimEnd(']');
                    }

                    bool isPath = false;
                    try
                    {
                        string fullPath = Path.GetFullPath(fullAddress);
                        isPath = File.Exists(fullPath);
                    }
                    catch (Exception) { }

                    if (isPath)
                    {
                        // Is Path
                        List<string> addressList = new();
                        await addressList.LoadFromFileAsync(fullAddress, true, true);
                        if (addressList.Count > 0)
                        {
                            for (int n = 0; n < addressList.Count; n++)
                            {
                                string address = addressList[n];
                                if (!address.StartsWith("//"))
                                    Set_DomainIP_Rules(client, address, rules);
                            }
                        }
                    }
                    else
                    {
                        // Is Address (Domain, Subdomain, IP, CIDR)
                        string address = fullAddress;
                        if (!address.StartsWith("//"))
                            Set_DomainIP_Rules(client, address, rules);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Rules Set_DomainIpPath_Rules: " + ex.Message);
                }
            });
        }

        private void Set_DomainIP_Rules(string client, string address, string rules) // rules Ends With ;
        {
            try
            {
                Rule rule = new()
                {
                    Client = client, // Client
                    Address = address // Domain Or IP
                };

                // Block
                if (rules.Equals("block;", StringComparison.InvariantCultureIgnoreCase)) rule.IsBlock = true;
                if (rules.Equals("-;")) rule.IsBlock = true;

                if (!rule.IsBlock)
                {
                    // Direct
                    if (rules.Contains("nobypass;", StringComparison.InvariantCultureIgnoreCase)) rule.IsDirect = true;
                    if (rules.Contains("direct;", StringComparison.InvariantCultureIgnoreCase)) rule.IsDirect = true;
                    if (rules.Contains("--;")) rule.IsDirect = true;

                    // Fake DNS
                    string fakeDnsIpStr = Rules_Init.GetValue(rules, Rules_Init.KEYS.FirstKey, null, false, out _, out _, Variables);
                    bool isIp = NetworkTool.IsIP(fakeDnsIpStr, out _);
                    if (isIp) rule.FakeDnsIP = fakeDnsIpStr;

                    // BlockPort
                    string ports = Rules_Init.GetValue(rules, Rules_Init.KEYS.BlockPort, null, true, out bool isList, out List<string> list, Variables);
                    if (!isList) // One Port
                    {
                        bool success = int.TryParse(ports, out int port);
                        if (success) rule.BlockPort.Add(port);
                    }
                    else // Multiple Ports
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string portStr = list[i];
                            bool success = int.TryParse(portStr, out int port);
                            if (success) rule.BlockPort.Add(port);
                        }
                    }
                    if (Default.BlockPort.Any())
                    {
                        try
                        {
                            rule.BlockPort.AddRange(Default.BlockPort);
                            rule.BlockPort = rule.BlockPort.Distinct().ToList();
                        }
                        catch (Exception) { }
                    }

                    if (rule.AddressType == AddressType.Domain) // DNSs, DnsDomain, DnsProxy & SNI
                    {
                        // Dnss
                        string dnss = Rules_Init.GetValue(rules, Rules_Init.KEYS.Dns, null, true, out isList, out list, Variables);
                        if (!isList) // One Dns
                        {
                            if (!string.IsNullOrEmpty(dnss))
                                rule.Dnss.Add(dnss);
                        }
                        else // Multiple Dnss
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                string dns = list[i];
                                if (!string.IsNullOrEmpty(dns))
                                    rule.Dnss.Add(dns);
                            }
                        }

                        if (!rule.Dnss.Any() && Default.Dnss.Any()) rule.Dnss = Default.Dnss;

                        // DnsDomain
                        rule.DnsDomain = Rules_Init.GetValue(rules, Rules_Init.KEYS.DnsDomain, null, false, out _, out _, Variables);
                        if (string.IsNullOrEmpty(rule.DnsDomain)) rule.DnsDomain = Default.DnsDomain;
                        // DnsProxy e.g. socks5://127.0.0.1:6666&user:UserName&pass:PassWord
                        rule.DnsProxyScheme = Rules_Init.GetValue(rules, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.FirstKey, false, out _, out _, Variables);
                        rule.DnsProxyUser = Rules_Init.GetValue(rules, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.User, false, out _, out _, Variables);
                        rule.DnsProxyPass = Rules_Init.GetValue(rules, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.Pass, false, out _, out _, Variables);
                        if (string.IsNullOrEmpty(rule.DnsProxyScheme))
                        {
                            rule.DnsProxyScheme = Default.DnsProxyScheme;
                            rule.DnsProxyUser = Default.DnsProxyUser;
                            rule.DnsProxyPass = Default.DnsProxyPass;
                        }

                        // SNI
                        rule.Sni = Rules_Init.GetValue(rules, Rules_Init.KEYS.Sni, null, false, out _, out _, Variables);
                        if (string.IsNullOrEmpty(rule.Sni)) rule.Sni = Default.Sni;
                    }

                    // Proxy e.g. socks5://127.0.0.1:6666&ifblock:1&user:UserName&pass:PassWord
                    rule.ProxyScheme = Rules_Init.GetValue(rules, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.FirstKey, false, out _, out _, Variables);
                    string ifBlock = Rules_Init.GetValue(rules, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.IfBlock, false, out _, out _, Variables).ToLower().Trim();
                    if (!string.IsNullOrEmpty(ifBlock))
                        rule.ProxyIfBlock = ifBlock.Equals("1") || ifBlock.Equals("true");
                    rule.ProxyUser = Rules_Init.GetValue(rules, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.User, false, out _, out _, Variables);
                    rule.ProxyPass = Rules_Init.GetValue(rules, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.Pass, false, out _, out _, Variables);
                    if (string.IsNullOrEmpty(rule.ProxyScheme))
                    {
                        rule.ProxyScheme = Default.ProxyScheme;
                        rule.ProxyIfBlock = Default.ProxyIfBlock;
                        rule.ProxyUser = Default.ProxyUser;
                        rule.ProxyPass = Default.ProxyPass;
                    }
                }

                lock (RuleList) // Just In Case
                {
                    RuleList.Add(rule);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Rules Set_DomainIP_Rules: " + ex.Message);
            }
        }

        private static AddressType GetAddressType(string address)
        {
            AddressType addressType = AddressType.None;

            try
            {
                address = address.Trim().Trim('/').Trim();
                if (string.IsNullOrEmpty(address)) return addressType;

                if (address.Contains('/'))
                {
                    addressType = AddressType.CIDR;
                }
                else
                {
                    bool isIP = NetworkTool.IsIP(address, out _);
                    if (isIP) addressType = AddressType.IP;
                    else addressType = AddressType.Domain;
                }
            }
            catch (Exception) { }

            return addressType;
        }

        public static async Task<(List<Tuple<string, string>> Variables, Defaults Defaults, List<Rule> RuleList)> MergeAsync(Mode mode1, string filePathOrText1, Mode mode2, string filePathOrText2)
        {
            // Create Merged Rules
            List<Tuple<string, string>> variables = new();
            Defaults defaults = new();
            List<Rule> ruleList = new();

            await Task.Run(async () =>
            {
                try
                {
                    // Read Rules1
                    Rules rules1 = new();
                    await rules1.SetAsync(mode1, filePathOrText1);
                    await Task.Delay(100);

                    // Read Rules2
                    Rules rules2 = new();
                    await rules2.SetAsync(mode2, filePathOrText2);
                    await Task.Delay(100);

                    // Get Variables
                    variables.AddRange(rules1.Variables);
                    variables.AddRange(rules2.Variables);
                    variables = variables.DistinctBy(x => x.Item1).ToList();

                    // Get Default Rules
                    // blockport:80,53;
                    defaults.BlockPort.AddRange(rules1.Default.BlockPort);
                    defaults.BlockPort.AddRange(rules2.Default.BlockPort);
                    defaults.BlockPort = defaults.BlockPort.Distinct().ToList();
                    defaults.BlockPort.Sort();
                    // dns:;
                    defaults.Dnss.AddRange(rules1.Default.Dnss);
                    defaults.Dnss.AddRange(rules2.Default.Dnss);
                    defaults.Dnss = defaults.Dnss.Distinct().ToList();
                    // dnsdomain:;
                    string dnsDomain = !string.IsNullOrWhiteSpace(rules2.Default.DnsDomain) ? rules2.Default.DnsDomain : rules1.Default.DnsDomain;
                    defaults.DnsDomain = dnsDomain.ToLower().Trim();
                    // dnsproxy:;
                    string dnsProxy = rules1.Default.DnsProxyScheme;
                    string dnsProxyUser = rules1.Default.DnsProxyUser;
                    string dnsProxyPass = rules1.Default.DnsProxyPass;
                    if (!string.IsNullOrWhiteSpace(rules2.Default.DnsProxyScheme))
                    {
                        dnsProxy = rules2.Default.DnsProxyScheme;
                        dnsProxyUser = rules2.Default.DnsProxyUser;
                        dnsProxyPass = rules2.Default.DnsProxyPass;
                    }
                    defaults.DnsProxyScheme = dnsProxy.ToLower().Trim();
                    defaults.DnsProxyUser = dnsProxyUser;
                    defaults.DnsProxyPass = dnsProxyPass;
                    // sni:;
                    string sni = !string.IsNullOrWhiteSpace(rules2.Default.Sni) ? rules2.Default.Sni : rules1.Default.Sni;
                    defaults.Sni = sni.Trim();
                    // proxy:;
                    string proxy = rules1.Default.ProxyScheme;
                    string proxyUser = rules1.Default.ProxyUser;
                    string proxyPass = rules1.Default.ProxyPass;
                    bool proxyIfBlock = rules2.Default.ProxyIfBlock ? rules2.Default.ProxyIfBlock : rules1.Default.ProxyIfBlock;
                    if (!string.IsNullOrWhiteSpace(rules2.Default.ProxyScheme))
                    {
                        proxy = rules2.Default.ProxyScheme;
                        proxyUser = rules2.Default.ProxyUser;
                        proxyPass = rules2.Default.ProxyPass;
                    }
                    defaults.ProxyScheme = proxy.ToLower().Trim();
                    defaults.ProxyUser = proxyUser;
                    defaults.ProxyPass = proxyPass;
                    defaults.ProxyIfBlock = proxyIfBlock;

                    // Update Rules1 With The Content Of Rules2
                    for (int n1 = 0; n1 < rules1.RuleList.Count; n1++)
                    {
                        Rule r1 = rules1.RuleList[n1];
                        for (int n2 = 0; n2 < rules2.RuleList.Count; ++n2)
                        {
                            Rule r2 = rules2.RuleList[n2];
                            if (r1.Client.Equals(r2.Client) && r1.Address.Equals(r2.Address))
                            {
                                if (r2.IsBlock) r1.IsBlock = true;
                                if (r2.IsDirect) r1.IsDirect = true;
                                if (!string.IsNullOrWhiteSpace(r2.FakeDnsIP)) r1.FakeDnsIP = r2.FakeDnsIP;
                                r1.BlockPort.AddRange(r2.BlockPort);
                                r1.BlockPort = r1.BlockPort.Distinct().ToList();
                                r1.BlockPort.Sort();
                                r1.Dnss.AddRange(r2.Dnss);
                                r1.Dnss = r1.Dnss.Distinct().ToList();
                                if (!string.IsNullOrWhiteSpace(r2.DnsDomain)) r1.DnsDomain = r2.DnsDomain;
                                if (!string.IsNullOrWhiteSpace(r2.DnsProxyScheme))
                                {
                                    r1.DnsProxyScheme = r2.DnsProxyScheme;
                                    r1.DnsProxyUser = r2.DnsProxyUser;
                                    r1.DnsProxyPass = r2.DnsProxyPass;
                                }
                                if (!string.IsNullOrWhiteSpace(r2.Sni)) r1.Sni = r2.Sni;
                                if (!string.IsNullOrWhiteSpace(r2.ProxyScheme))
                                {
                                    r1.ProxyScheme = r2.ProxyScheme;
                                    if (r2.ProxyIfBlock) r1.ProxyIfBlock = true;
                                    r1.ProxyUser = r2.ProxyUser;
                                    r1.ProxyPass = r2.ProxyPass;
                                }

                                // Set MR2 Address To string.empty
                                r2.Address = string.Empty;
                                break;
                            }
                        }
                    }

                    // Add All Rules1
                    ruleList.AddRange(rules1.RuleList);

                    // Add Rules2 If Address Is Not Empty
                    for (int n2 = 0; n2 < rules2.RuleList.Count; ++n2)
                    {
                        Rule r2 = rules2.RuleList[n2];
                        if (!string.IsNullOrEmpty(r2.Address)) ruleList.Add(r2);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("AgnosticProgram Rules MergeAsync: " + ex.Message);
                }
            });

            return (variables, defaults, ruleList);
        }

        public static async Task<Rules> ReadAsync(Mode mode, string filePathOrText)
        {
            Rules rules = new();

            await Task.Run(async () =>
            {
                try
                {
                    // Read Rules
                    await rules.SetAsync(mode, filePathOrText);
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("AgnosticProgram Rules ReadAsync: " + ex.Message);
                }
            });

            return rules;
        }

        public static async Task<List<string>> ConvertToTextRulesAsync(List<Tuple<string, string>> variables, Defaults? defaults, List<Rule> ruleList)
        {
            List<string> textRules = new();

            await Task.Run(() =>
            {
                try
                {
                    // Add Comment
                    textRules.Add("// Generated Automatically By DNSveil");
                    textRules.Add(string.Empty);

                    // Add Variables
                    if (variables.Count > 0)
                    {
                        textRules.Add("// Variables");
                        for (int n = 0; n < variables.Count; n++)
                        {
                            Tuple<string, string> variable = variables[n];
                            if (!string.IsNullOrWhiteSpace(variable.Item1) && !string.IsNullOrWhiteSpace(variable.Item2))
                            {
                                string text = $"{variable.Item1} = {variable.Item2};";
                                textRules.Add(text);
                            }
                        }
                        textRules.Add(string.Empty);
                    }

                    // Add Defaults
                    if (defaults != null)
                    {
                        bool addDefaults = defaults.BlockPort.Count > 0 || defaults.Dnss.Count > 0 ||
                        !string.IsNullOrWhiteSpace(defaults.DnsDomain) ||
                        !string.IsNullOrWhiteSpace(defaults.DnsProxyScheme) ||
                        !string.IsNullOrWhiteSpace(defaults.Sni) ||
                        !string.IsNullOrWhiteSpace(defaults.ProxyScheme);
                        if (addDefaults)
                        {
                            textRules.Add("// Defaults");

                            // blockport:80,53;
                            if (defaults.BlockPort.Count > 0)
                            {
                                string text = $"{Rules_Init.KEYS.BlockPort}{Rules_Init.Vari_ValuesToNames(defaults.BlockPort.ToString(','), variables)};";
                                textRules.Add(text);
                            }
                            // dns:;
                            if (defaults.Dnss.Count > 0)
                            {
                                string text = $"{Rules_Init.KEYS.Dns}{Rules_Init.Vari_ValuesToNames(defaults.Dnss.ToString(','), variables)};";
                                textRules.Add(text);
                            }
                            // dnsdomain:;
                            if (!string.IsNullOrEmpty(defaults.DnsDomain))
                            {
                                string text = $"{Rules_Init.KEYS.DnsDomain}{Rules_Init.Vari_ValuesToNames(defaults.DnsDomain, variables)};";
                                textRules.Add(text);
                            }
                            // dnsproxy:;
                            if (!string.IsNullOrEmpty(defaults.DnsProxyScheme))
                            {
                                string text = $"{Rules_Init.KEYS.DnsProxy}{Rules_Init.Vari_ValuesToNames(defaults.DnsProxyScheme, variables)}";
                                // &user:
                                if (!string.IsNullOrWhiteSpace(defaults.DnsProxyUser))
                                    text += $"{Rules_Init.SUB_KEYS.User}{Rules_Init.Vari_ValuesToNames(defaults.DnsProxyUser, variables)}";
                                // &pass:
                                if (!string.IsNullOrEmpty(defaults.DnsProxyPass))
                                    text += $"{Rules_Init.SUB_KEYS.Pass}{Rules_Init.Vari_ValuesToNames(defaults.DnsProxyPass, variables)}";
                                // Add ;
                                text += ";";
                                textRules.Add(text);
                            }
                            // sni:;
                            if (!string.IsNullOrWhiteSpace(defaults.Sni))
                            {
                                string text = $"{Rules_Init.KEYS.Sni}{Rules_Init.Vari_ValuesToNames(defaults.Sni, variables)};";
                                textRules.Add(text);
                            }
                            // proxy:;
                            if (!string.IsNullOrEmpty(defaults.ProxyScheme))
                            {
                                string text = $"{Rules_Init.KEYS.Proxy}{Rules_Init.Vari_ValuesToNames(defaults.ProxyScheme, variables)}";
                                // &user:
                                if (!string.IsNullOrWhiteSpace(defaults.ProxyUser))
                                    text += $"{Rules_Init.SUB_KEYS.User}{Rules_Init.Vari_ValuesToNames(defaults.ProxyUser, variables)}";
                                // &pass:
                                if (!string.IsNullOrEmpty(defaults.ProxyPass))
                                    text += $"{Rules_Init.SUB_KEYS.Pass}{Rules_Init.Vari_ValuesToNames(defaults.ProxyPass, variables)}";
                                // &ifblock:1
                                if (defaults.ProxyIfBlock)
                                    text += $"{Rules_Init.SUB_KEYS.IfBlock}1";
                                // Add ;
                                text += ";";
                                textRules.Add(text);
                            }
                            textRules.Add(string.Empty);
                        }
                    }
                    
                    // Add Main Rules
                    if (ruleList.Count > 0) textRules.Add("// Rules");
                    for (int n = 0; n < ruleList.Count; n++)
                    {
                        Rule rule = ruleList[n];
                        if (string.IsNullOrWhiteSpace(rule.Address)) continue;
                        string text = string.Empty;
                        // Client
                        if (!string.IsNullOrWhiteSpace(rule.Client) && !rule.Client.Equals(Rules_Init.KEYS.AllClients))
                            text += $"{rule.Client}|";
                        // Address
                        text += $"{rule.Address}|";
                        // Is Block
                        if (rule.IsBlock)
                        {
                            text += "-;";
                            textRules.Add(text);
                            continue;
                        }
                        // Fake DNS
                        bool isIP = NetworkTool.IsIP(rule.FakeDnsIP, out _);
                        if (isIP) text += $"{Rules_Init.Vari_ValuesToNames(rule.FakeDnsIP, variables)};";
                        // Is Direct
                        if (rule.IsDirect) text += "--;";
                        // Block Ports
                        if (defaults != null)
                        {
                            foreach (int blockPort in defaults.BlockPort)
                            {
                                if (rule.BlockPort.IsContain(blockPort))
                                    rule.BlockPort.Remove(blockPort);
                            }
                        }
                        if (rule.BlockPort.Count > 0) text += $"{Rules_Init.KEYS.BlockPort}{Rules_Init.Vari_ValuesToNames(rule.BlockPort.ToString(','), variables)};";
                        // DNSs
                        if (defaults != null)
                        {
                            foreach (string dns in defaults.Dnss)
                            {
                                if (rule.Dnss.IsContain(dns))
                                    rule.Dnss.Remove(dns);
                            }
                        }
                        if (rule.Dnss.Count > 0) text += $"{Rules_Init.KEYS.Dns}{Rules_Init.Vari_ValuesToNames(rule.Dnss.ToString(','), variables)};";
                        // DNS Domain
                        if (defaults != null)
                        {
                            if (defaults.DnsDomain.Equals(rule.DnsDomain))
                                rule.DnsDomain = string.Empty;
                        }
                        if (!string.IsNullOrWhiteSpace(rule.DnsDomain)) text += $"{Rules_Init.KEYS.DnsDomain}{Rules_Init.Vari_ValuesToNames(rule.DnsDomain, variables)};";
                        // DNS Proxy
                        if (defaults != null)
                        {
                            if (defaults.DnsProxyScheme.Equals(rule.DnsProxyScheme))
                                rule.DnsProxyScheme = string.Empty;
                        }
                        if (!string.IsNullOrWhiteSpace(rule.DnsProxyScheme))
                        {
                            // Scheme
                            text += $"{Rules_Init.KEYS.DnsProxy}{Rules_Init.Vari_ValuesToNames(rule.DnsProxyScheme, variables)}";
                            // &user:
                            if (!string.IsNullOrWhiteSpace(rule.DnsProxyUser))
                                text += $"{Rules_Init.SUB_KEYS.User}{Rules_Init.Vari_ValuesToNames(rule.DnsProxyUser, variables)}";
                            // &pass:
                            if (!string.IsNullOrEmpty(rule.DnsProxyPass))
                                text += $"{Rules_Init.SUB_KEYS.Pass}{Rules_Init.Vari_ValuesToNames(rule.DnsProxyPass, variables)}";
                            // Add ;
                            text += ";";
                        }
                        // SNI
                        if (defaults != null)
                        {
                            if (defaults.Sni.Equals(rule.Sni))
                                rule.Sni = string.Empty;
                        }
                        if (!string.IsNullOrWhiteSpace(rule.Sni)) text += $"{Rules_Init.KEYS.Sni}{Rules_Init.Vari_ValuesToNames(rule.Sni, variables)};";
                        // Proxy
                        if (defaults != null)
                        {
                            if (defaults.ProxyScheme.Equals(rule.ProxyScheme))
                                rule.ProxyScheme = string.Empty;
                        }
                        if (!string.IsNullOrWhiteSpace(rule.ProxyScheme))
                        {
                            // Scheme
                            text += $"{Rules_Init.KEYS.Proxy}{Rules_Init.Vari_ValuesToNames(rule.ProxyScheme, variables)}";
                            // &user:
                            if (!string.IsNullOrWhiteSpace(rule.ProxyUser))
                                text += $"{Rules_Init.SUB_KEYS.User}{Rules_Init.Vari_ValuesToNames(rule.ProxyUser, variables)}";
                            // &pass:
                            if (!string.IsNullOrEmpty(rule.ProxyPass))
                                text += $"{Rules_Init.SUB_KEYS.Pass}{Rules_Init.Vari_ValuesToNames(rule.ProxyPass, variables)}";
                            // &ifblock:1
                            if (rule.ProxyIfBlock) text += $"{Rules_Init.SUB_KEYS.IfBlock}1";
                            // Add ;
                            text += ";";
                        }

                        // Add +;
                        if (!string.IsNullOrWhiteSpace(text) && !text.EndsWith(';')) text += "+;";

                        textRules.Add(text);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("AgnosticProgram Rules ConvertToTextRulesAsync: " + ex.Message);
                }
            });

            return textRules;
        }

        private async Task ApplyVariablesToRulesAsync(Tuple<string, string> oldVar, Tuple<string, string> newVar)
        {
            try
            {
                if (oldVar.Item1 == newVar.Item1)
                {
                    // Search For Old Value And Replace It With The New Value
                    string ov = oldVar.Item2;
                    string nv = newVar.Item2;
                    List<Tuple<string, string>> values = new()
                    {
                        Tuple.Create(ov, nv)
                    };

                    // Defaults
                    string[] dBlockPorts = Rules_Init.Vari_NamesToValues(Default.BlockPort.ToString(','), values).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (dBlockPorts.Length > 0)
                    {
                        Default.BlockPort.Clear();
                        foreach (string blockPort in dBlockPorts)
                        {
                            bool isInt = int.TryParse(blockPort, out int value);
                            if (isInt) Default.BlockPort.Add(value);
                        }
                    }
                    
                    string[] dDnss = Rules_Init.Vari_NamesToValues(Default.Dnss.ToString(','), values).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (dDnss.Length > 0)
                    {
                        Default.Dnss.Clear();
                        foreach (string dns in dDnss)
                        {
                            Default.Dnss.Add(dns);
                        }
                    }

                    Default.DnsDomain = Rules_Init.Vari_NameToValue(Default.DnsDomain, values);
                    Default.DnsProxyScheme = Rules_Init.Vari_NameToValue(Default.DnsProxyScheme, values);
                    Default.DnsProxyUser = Rules_Init.Vari_NameToValue(Default.DnsProxyUser, values);
                    Default.DnsProxyPass = Rules_Init.Vari_NameToValue(Default.DnsProxyPass, values);
                    Default.Sni = Rules_Init.Vari_NameToValue(Default.Sni, values);
                    Default.ProxyScheme = Rules_Init.Vari_NameToValue(Default.ProxyScheme, values);
                    string dIfBlock = Rules_Init.Vari_NameToValue(Default.ProxyIfBlock.ToString().ToLower().Trim(), values);
                    Default.ProxyIfBlock = dIfBlock.Equals("1") || dIfBlock.Equals("true");
                    Default.ProxyUser = Rules_Init.Vari_NameToValue(Default.ProxyUser, values);
                    Default.ProxyPass = Rules_Init.Vari_NameToValue(Default.ProxyPass, values);
                    await Task.Delay(1);

                    // Rules
                    for (int n = 0; n < RuleList.Count; n++)
                    {
                        Rule rule = RuleList[n];

                        string[] blockPorts = Rules_Init.Vari_NamesToValues(rule.BlockPort.ToString(','), values).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        if (blockPorts.Length > 0)
                        {
                            rule.BlockPort.Clear();
                            foreach (string blockPort in blockPorts)
                            {
                                bool isInt = int.TryParse(blockPort, out int value);
                                if (isInt) rule.BlockPort.Add(value);
                            }
                        }

                        rule.FakeDnsIP = Rules_Init.Vari_NameToValue(rule.FakeDnsIP, values);

                        string[] dnss = Rules_Init.Vari_NamesToValues(rule.Dnss.ToString(','), values).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        if (dnss.Length > 0)
                        {
                            rule.Dnss.Clear();
                            foreach (string dns in dnss)
                            {
                                rule.Dnss.Add(dns);
                            }
                        }

                        rule.DnsDomain = Rules_Init.Vari_NameToValue(rule.DnsDomain, values);
                        rule.DnsProxyScheme = Rules_Init.Vari_NameToValue(rule.DnsProxyScheme, values);
                        rule.DnsProxyUser = Rules_Init.Vari_NameToValue(rule.DnsProxyUser, values);
                        rule.DnsProxyPass = Rules_Init.Vari_NameToValue(rule.DnsProxyPass, values);
                        rule.Sni = Rules_Init.Vari_NameToValue(rule.Sni, values);
                        rule.ProxyScheme = Rules_Init.Vari_NameToValue(rule.ProxyScheme, values);
                        string ifBlock = Rules_Init.Vari_NameToValue(rule.ProxyIfBlock.ToString().ToLower().Trim(), values);
                        rule.ProxyIfBlock = ifBlock.Equals("1") || ifBlock.Equals("true");
                        rule.ProxyUser = Rules_Init.Vari_NameToValue(rule.ProxyUser, values);
                        rule.ProxyPass = Rules_Init.Vari_NameToValue(rule.ProxyPass, values);
                    }
                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules ApplyVariablesToRulesAsync: " + ex.Message);
            }
        }

        public async Task<bool> IsVariableNameExistAsync(string variableName)
        {
            try
            {
                for (int n = 0; n < Variables.Count; n++)
                {
                    Tuple<string, string> tuple = Variables[n];
                    if (variableName.Equals(tuple.Item1))
                    {
                        await Task.Delay(1);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules IsVariableNameExistAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> IsVariableValueExistAsync(string variableValue)
        {
            try
            {
                for (int n = 0; n < Variables.Count; n++)
                {
                    Tuple<string, string> tuple = Variables[n];
                    if (variableValue.Equals(tuple.Item2))
                    {
                        await Task.Delay(1);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules IsVariableValueExistAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> IsRuleExistAsync(Rule rule)
        {
            try
            {
                for (int n = 0; n < RuleList.Count; n++)
                {
                    Rule currentRule = RuleList[n];
                    if (currentRule.Client.Equals(rule.Client) && currentRule.Address.Equals(rule.Address))
                    {
                        await Task.Delay(1);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules IsRuleExistAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveAsync(string variableName)
        {
            try
            {
                for (int n = 0; n < Variables.Count; n++)
                {
                    Tuple<string, string> tuple = Variables[n];
                    if (variableName.Equals(tuple.Item1))
                    {
                        Variables.RemoveAt(n);
                        await Task.Delay(1);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules RemoveAsync Variable: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveAsync(Rule rule)
        {
            try
            {
                for (int n = 0; n < RuleList.Count; n++)
                {
                    Rule currentRule = RuleList[n];
                    if (currentRule.Client.Equals(rule.Client) && currentRule.Address.Equals(rule.Address))
                    {
                        RuleList.RemoveAt(n);
                        await Task.Delay(1);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules RemoveAsync Rule: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Add Variable
        /// </summary>
        /// <param name="variable">New Variable</param>
        /// <returns>-1: Added, 0: Name Exist, 1: Value Exist, 2: Error</returns>
        public async Task<int> AddAsync(Tuple<string, string> variable)
        {
            try
            {
                bool isNameExist = await IsVariableNameExistAsync(variable.Item1);
                if (isNameExist) return 0;
                bool isValueExist = await IsVariableValueExistAsync(variable.Item2);
                if (isValueExist) return 1;
                Variables.Add(variable);
                await Task.Delay(1);
                return -1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules AddAsync Variable: " + ex.Message);
                return 2;
            }
        }

        public async Task<bool> AddAsync(Rule rule)
        {
            try
            {
                bool isRuleExist = await IsRuleExistAsync(rule);
                if (!isRuleExist)
                {
                    RuleList.Add(rule);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules AddAsync Rule: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Modify Variable
        /// </summary>
        /// <param name="variable">Modified Variable</param>
        /// <returns>-1: Modified, 0: Name Or Value Already Exist, 1: Variable Doesn't Exist, 2: Error</returns>
        public async Task<int> ModifyAsync(Tuple<string, string> variable)
        {
            try
            {
                bool isNameExist = await IsVariableNameExistAsync(variable.Item1);
                bool isValueExist = await IsVariableValueExistAsync(variable.Item2);

                for (int n = 0; n < Variables.Count; n++)
                {
                    Tuple<string, string> tuple = Variables[n];
                    bool nameNoValue = variable.Item1.Equals(tuple.Item1) && !isValueExist;
                    bool valueNoName = variable.Item2.Equals(tuple.Item2) && !isNameExist;
                    if (nameNoValue || valueNoName)
                    {
                        Variables[n] = variable;
                        await Task.Delay(1);
                        if (nameNoValue && !valueNoName)
                            await ApplyVariablesToRulesAsync(tuple, variable);
                        return -1;
                    }
                }

                if (isNameExist || isValueExist) return 0; // We Don't Know Which One Comes First
                return 1; // Variable Doesn't Exist
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules ModifyAsync Variable: " + ex.Message);
                return 2;
            }
        }

        public async Task<bool> ModifyAsync(Rule rule)
        {
            try
            {
                for (int n = 0; n < RuleList.Count; n++)
                {
                    Rule currentRule = RuleList[n];
                    if (currentRule.Client.Equals(rule.Client) && currentRule.Address.Equals(rule.Address))
                    {
                        RuleList[n] = rule;
                        await Task.Delay(1);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules ModifyAsync Rule: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> MoveAsync(Rule rule, int toIndex)
        {
            try
            {
                for (int n = 0; n < RuleList.Count; n++)
                {
                    Rule currentRule = RuleList[n];
                    if (currentRule.Client.Equals(rule.Client) && currentRule.Address.Equals(rule.Address))
                    {
                        RuleList.MoveTo(n, toIndex);
                        await Task.Delay(1);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules MoveAsync Rule: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Save To File If RulesMode == Mode.File
        /// </summary>
        /// <returns>Returns True If Success.</returns>
        public async Task<bool> SaveAsync()
        {
            try
            {
                if (RulesMode == Mode.File)
                {
                    if (File.Exists(PathOrText))
                    {
                        List<string> lines = await ConvertToTextRulesAsync(Variables, Default, RuleList);
                        await lines.SaveToFileAsync(PathOrText);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules SaveAsync: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Save To File If RulesMode == Mode.File
        /// </summary>
        /// <returns>Returns True If Success.</returns>
        public async Task<bool> SaveToAsync(string filePath)
        {
            try
            {
                if (RulesMode == Mode.File)
                {
                    List<string> lines = await ConvertToTextRulesAsync(Variables, Default, RuleList);
                    await lines.SaveToFileAsync(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AgnosticProgram Rules SaveToAsync: " + ex.Message);
                return false;
            }
        }

    }
}