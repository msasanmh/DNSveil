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

        private List<string> Rules_List { get; set; } = new();
        private List<Tuple<string, string>> Variables { get; set; } = new(); // x = domain.com;
        private Defaults Default { get; set; } = new();
        private List<MainRules> MainRules_List { get; set; } = new();

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

        public class MainRules
        {
            public string Client { get; set; } = string.Empty;
            public AddressType AddressType { get; set; } = AddressType.None;
            public string Address { get; set; } = string.Empty;
            public bool IsBlock { get; set; } = false;
            public List<int> BlockPort { get; set; } = new();
            public bool IsDirect { get; set; } = false;
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

        public async Task SetAsync(Mode mode, string filePathOrText)
        {
            try
            {
                Rules_List.Clear();
                Variables.Clear();
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
                        string ports = Rules_Init.GetValue(line, Rules_Init.KEYS.BlockPort, null, out bool isList, out List<string> list, Variables);
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
                        string dnss = Rules_Init.GetValue(line, Rules_Init.KEYS.Dns, null, out bool isList, out List<string> list, Variables);
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
                        Default.DnsDomain = Rules_Init.GetValue(line, Rules_Init.KEYS.DnsDomain, null, out _, out _, Variables);
                    }
                    else if (line.StartsWith(Rules_Init.KEYS.DnsProxy, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Default.DnsProxyScheme = Rules_Init.GetValue(line, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.FirstKey, out _, out _, Variables);
                        Default.DnsProxyUser = Rules_Init.GetValue(line, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.User, out _, out _, Variables);
                        Default.DnsProxyPass = Rules_Init.GetValue(line, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.Pass, out _, out _, Variables);
                    }
                    else if (line.StartsWith(Rules_Init.KEYS.Sni, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Default.Sni = Rules_Init.GetValue(line, Rules_Init.KEYS.Sni, null, out _, out _, Variables);
                    }
                    else if (line.StartsWith(Rules_Init.KEYS.Proxy, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Default.ProxyScheme = Rules_Init.GetValue(line, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.FirstKey, out _, out _, Variables);
                        string ifBlock = Rules_Init.GetValue(line, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.IfBlock, out _, out _, Variables).ToLower().Trim();
                        if (!string.IsNullOrEmpty(ifBlock))
                            Default.ProxyIfBlock = ifBlock.Equals("1") || ifBlock.Equals("true", StringComparison.OrdinalIgnoreCase);
                        Default.ProxyUser = Rules_Init.GetValue(line, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.User, out _, out _, Variables);
                        Default.ProxyPass = Rules_Init.GetValue(line, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.Pass, out _, out _, Variables);
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
                MainRules mr = new()
                {
                    Client = client, // Client
                    AddressType = GetAddressType(address), // Address Type
                    Address = address // Domain Or IP
                };

                // Block
                if (rules.Equals("block;", StringComparison.InvariantCultureIgnoreCase)) mr.IsBlock = true;
                if (rules.Equals("-;")) mr.IsBlock = true;

                if (!mr.IsBlock)
                {
                    // Direct
                    if (rules.Contains("nobypass;", StringComparison.InvariantCultureIgnoreCase)) mr.IsDirect = true;
                    if (rules.Contains("direct;", StringComparison.InvariantCultureIgnoreCase)) mr.IsDirect = true;
                    if (rules.Contains("--;")) mr.IsDirect = true;

                    // Fake DNS
                    string fakeDnsIpStr = Rules_Init.GetValue(rules, Rules_Init.KEYS.FirstKey, null, out _, out _, Variables);
                    bool isIp = NetworkTool.IsIP(fakeDnsIpStr, out _);
                    if (isIp) mr.FakeDns = fakeDnsIpStr;

                    // BlockPort
                    string ports = Rules_Init.GetValue(rules, Rules_Init.KEYS.BlockPort, null, out bool isList, out List<string> list, Variables);
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
                    if (Default.BlockPort.Any())
                    {
                        try
                        {
                            mr.BlockPort.AddRange(Default.BlockPort);
                            mr.BlockPort = mr.BlockPort.Distinct().ToList();
                        }
                        catch (Exception) { }
                    }

                    if (mr.AddressType == AddressType.Domain) // DNSs, DnsDomain, DnsProxy & SNI
                    {
                        // Dnss
                        string dnss = Rules_Init.GetValue(rules, Rules_Init.KEYS.Dns, null, out isList, out list, Variables);
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

                        if (!mr.Dnss.Any() && Default.Dnss.Any()) mr.Dnss = Default.Dnss;

                        // DnsDomain
                        mr.DnsDomain = Rules_Init.GetValue(rules, Rules_Init.KEYS.DnsDomain, null, out _, out _, Variables);
                        if (string.IsNullOrEmpty(mr.DnsDomain)) mr.DnsDomain = Default.DnsDomain;

                        // DnsProxy e.g. socks5://127.0.0.1:6666&user:UserName&pass:PassWord
                        mr.DnsProxyScheme = Rules_Init.GetValue(rules, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.FirstKey, out _, out _, Variables);
                        mr.DnsProxyUser = Rules_Init.GetValue(rules, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.User, out _, out _, Variables);
                        mr.DnsProxyPass = Rules_Init.GetValue(rules, Rules_Init.KEYS.DnsProxy, Rules_Init.SUB_KEYS.Pass, out _, out _, Variables);
                        if (string.IsNullOrEmpty(mr.DnsProxyScheme))
                        {
                            mr.DnsProxyScheme = Default.DnsProxyScheme;
                            mr.DnsProxyUser = Default.DnsProxyUser;
                            mr.DnsProxyPass = Default.DnsProxyPass;
                        }

                        // SNI
                        mr.Sni = Rules_Init.GetValue(rules, Rules_Init.KEYS.Sni, null, out _, out _, Variables);
                        if (string.IsNullOrEmpty(mr.Sni)) mr.Sni = Default.Sni;
                    }

                    // Proxy e.g. socks5://127.0.0.1:6666&ifblock:1&user:UserName&pass:PassWord
                    mr.ProxyScheme = Rules_Init.GetValue(rules, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.FirstKey, out _, out _, Variables);
                    string ifBlock = Rules_Init.GetValue(rules, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.IfBlock, out _, out _, Variables).ToLower().Trim();
                    if (!string.IsNullOrEmpty(ifBlock))
                        mr.ProxyIfBlock = ifBlock.Equals("1") || ifBlock.Equals("true");
                    mr.ProxyUser = Rules_Init.GetValue(rules, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.User, out _, out _, Variables);
                    mr.ProxyPass = Rules_Init.GetValue(rules, Rules_Init.KEYS.Proxy, Rules_Init.SUB_KEYS.Pass, out _, out _, Variables);
                    if (string.IsNullOrEmpty(mr.ProxyScheme))
                    {
                        mr.ProxyScheme = Default.ProxyScheme;
                        mr.ProxyIfBlock = Default.ProxyIfBlock;
                        mr.ProxyUser = Default.ProxyUser;
                        mr.ProxyPass = Default.ProxyPass;
                    }
                }

                lock (MainRules_List) // Just In Case
                {
                    MainRules_List.Add(mr);
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

        public static async Task<(List<Tuple<string, string>> Variables, Defaults Defaults, List<MainRules> MainRulesList)> MergeAsync(Mode mode1, string filePathOrText1, Mode mode2, string filePathOrText2)
        {
            // Create Merged Rules
            List<Tuple<string, string>> variables = new();
            Defaults defaults = new();
            List<MainRules> mainRules = new();

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
                    for (int n1 = 0; n1 < rules1.MainRules_List.Count; n1++)
                    {
                        MainRules mr1 = rules1.MainRules_List[n1];
                        for (int n2 = 0; n2 < rules2.MainRules_List.Count; ++n2)
                        {
                            MainRules mr2 = rules2.MainRules_List[n2];
                            if (mr1.Client.Equals(mr2.Client) && mr1.Address.Equals(mr2.Address))
                            {
                                if (mr2.IsBlock) mr1.IsBlock = true;
                                if (mr2.IsDirect) mr1.IsDirect = true;
                                if (!string.IsNullOrWhiteSpace(mr2.FakeDns)) mr1.FakeDns = mr2.FakeDns;
                                mr1.BlockPort.AddRange(mr2.BlockPort);
                                mr1.BlockPort = mr1.BlockPort.Distinct().ToList();
                                mr1.BlockPort.Sort();
                                mr1.Dnss.AddRange(mr2.Dnss);
                                mr1.Dnss = mr1.Dnss.Distinct().ToList();
                                if (!string.IsNullOrWhiteSpace(mr2.DnsDomain)) mr1.DnsDomain = mr2.DnsDomain;
                                if (!string.IsNullOrWhiteSpace(mr2.DnsProxyScheme))
                                {
                                    mr1.DnsProxyScheme = mr2.DnsProxyScheme;
                                    mr1.DnsProxyUser = mr2.DnsProxyUser;
                                    mr1.DnsProxyPass = mr2.DnsProxyPass;
                                }
                                if (!string.IsNullOrWhiteSpace(mr2.Sni)) mr1.Sni = mr2.Sni;
                                if (!string.IsNullOrWhiteSpace(mr2.ProxyScheme))
                                {
                                    mr1.ProxyScheme = mr2.ProxyScheme;
                                    if (mr2.ProxyIfBlock) mr1.ProxyIfBlock = true;
                                    mr1.ProxyUser = mr2.ProxyUser;
                                    mr1.ProxyPass = mr2.ProxyPass;
                                }

                                // Set MR2 Address To string.empty
                                mr2.Address = string.Empty;
                                break;
                            }
                        }
                    }

                    // Add All Rules1
                    mainRules.AddRange(rules1.MainRules_List);

                    // Add Rules2 If Address Is Not Empty
                    for (int n2 = 0; n2 < rules2.MainRules_List.Count; ++n2)
                    {
                        MainRules mr2 = rules2.MainRules_List[n2];
                        if (!string.IsNullOrEmpty(mr2.Address)) mainRules.Add(mr2);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("AgnosticProgram Rules MergeAsync: " + ex.Message);
                }
            });

            return (variables, defaults, mainRules);
        }

        public static async Task<List<string>> ConvertToTextRulesAsync(List<Tuple<string, string>>? variables, Defaults? defaults, List<MainRules> mainRulesList)
        {
            List<string> textRules = new();

            await Task.Run(() =>
            {
                try
                {
                    // Add Comment
                    textRules.Add("// Generated Automatically By SDC - Secure DNS Client");
                    textRules.Add("// Please Review And Modify If Needed.");
                    textRules.Add(string.Empty);

                    // Add Variables
                    if (variables != null && variables.Count > 0)
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
                                string text = $"{Rules_Init.KEYS.BlockPort}{Rules_Init.GetReplacedValues(defaults.BlockPort.ToString(','), variables)};";
                                textRules.Add(text);
                            }
                            // dns:;
                            if (defaults.Dnss.Count > 0)
                            {
                                string text = $"{Rules_Init.KEYS.Dns}{Rules_Init.GetReplacedValues(defaults.Dnss.ToString(','), variables)};";
                                textRules.Add(text);
                            }
                            // dnsdomain:;
                            if (!string.IsNullOrEmpty(defaults.DnsDomain))
                            {
                                string text = $"{Rules_Init.KEYS.DnsDomain}{Rules_Init.GetReplacedValues(defaults.DnsDomain, variables)};";
                                textRules.Add(text);
                            }
                            // dnsproxy:;
                            if (!string.IsNullOrEmpty(defaults.DnsProxyScheme))
                            {
                                string text = $"{Rules_Init.KEYS.DnsProxy}{Rules_Init.GetReplacedValues(defaults.DnsProxyScheme, variables)}";
                                // &user:
                                if (!string.IsNullOrWhiteSpace(defaults.DnsProxyUser))
                                    text += $"{Rules_Init.SUB_KEYS.User}{Rules_Init.GetReplacedValues(defaults.DnsProxyUser, variables)}";
                                // &pass:
                                if (!string.IsNullOrEmpty(defaults.DnsProxyPass))
                                    text += $"{Rules_Init.SUB_KEYS.Pass}{Rules_Init.GetReplacedValues(defaults.DnsProxyPass, variables)}";
                                // Add ;
                                text += ";";
                                textRules.Add(text);
                            }
                            // sni:;
                            if (!string.IsNullOrWhiteSpace(defaults.Sni))
                            {
                                string text = $"{Rules_Init.KEYS.Sni}{Rules_Init.GetReplacedValues(defaults.Sni, variables)};";
                                textRules.Add(text);
                            }
                            // proxy:;
                            if (!string.IsNullOrEmpty(defaults.ProxyScheme))
                            {
                                string text = $"{Rules_Init.KEYS.Proxy}{Rules_Init.GetReplacedValues(defaults.ProxyScheme, variables)}";
                                // &user:
                                if (!string.IsNullOrWhiteSpace(defaults.ProxyUser))
                                    text += $"{Rules_Init.SUB_KEYS.User}{Rules_Init.GetReplacedValues(defaults.ProxyUser, variables)}";
                                // &pass:
                                if (!string.IsNullOrEmpty(defaults.ProxyPass))
                                    text += $"{Rules_Init.SUB_KEYS.Pass}{Rules_Init.GetReplacedValues(defaults.ProxyPass, variables)}";
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
                    if (mainRulesList.Count > 0) textRules.Add("// Rules");
                    for (int n = 0; n < mainRulesList.Count; n++)
                    {
                        MainRules mr = mainRulesList[n];
                        if (string.IsNullOrWhiteSpace(mr.Address)) continue;
                        string text = string.Empty;
                        // Client
                        if (!string.IsNullOrWhiteSpace(mr.Client) && !mr.Client.Equals(Rules_Init.KEYS.AllClients))
                            text += $"{mr.Client}|";
                        // Address
                        text += $"{mr.Address}|";
                        // Is Block
                        if (mr.IsBlock)
                        {
                            text += "-;";
                            continue;
                        }
                        // Fake DNS
                        bool isIP = NetworkTool.IsIP(mr.FakeDns, out _);
                        if (isIP) text += $"{Rules_Init.GetReplacedValues(mr.FakeDns, variables)};";
                        // Is Direct
                        if (mr.IsDirect) text += "--;";
                        // Block Ports
                        if (defaults != null)
                        {
                            foreach (int blockPort in defaults.BlockPort)
                            {
                                if (mr.BlockPort.IsContain(blockPort))
                                    mr.BlockPort.Remove(blockPort);
                            }
                        }
                        if (mr.BlockPort.Count > 0) text += $"{Rules_Init.KEYS.BlockPort}{Rules_Init.GetReplacedValues(mr.BlockPort.ToString(','), variables)};";
                        // DNSs
                        if (defaults != null)
                        {
                            foreach (string dns in defaults.Dnss)
                            {
                                if (mr.Dnss.IsContain(dns))
                                    mr.Dnss.Remove(dns);
                            }
                        }
                        if (mr.Dnss.Count > 0) text += $"{Rules_Init.KEYS.Dns}{Rules_Init.GetReplacedValues(mr.Dnss.ToString(','), variables)};";
                        // DNS Domain
                        if (defaults != null)
                        {
                            if (defaults.DnsDomain.Equals(mr.DnsDomain))
                                mr.DnsDomain = string.Empty;
                        }
                        if (!string.IsNullOrWhiteSpace(mr.DnsDomain)) text += $"{Rules_Init.KEYS.DnsDomain}{Rules_Init.GetReplacedValues(mr.DnsDomain, variables)};";
                        // DNS Proxy
                        if (defaults != null)
                        {
                            if (defaults.DnsProxyScheme.Equals(mr.DnsProxyScheme))
                                mr.DnsProxyScheme = string.Empty;
                        }
                        if (!string.IsNullOrWhiteSpace(mr.DnsProxyScheme))
                        {
                            // Scheme
                            text += $"{Rules_Init.KEYS.DnsProxy}{Rules_Init.GetReplacedValues(mr.DnsProxyScheme, variables)}";
                            // &user:
                            if (!string.IsNullOrWhiteSpace(mr.DnsProxyUser))
                                text += $"{Rules_Init.SUB_KEYS.User}{Rules_Init.GetReplacedValues(mr.DnsProxyUser, variables)}";
                            // &pass:
                            if (!string.IsNullOrEmpty(mr.DnsProxyPass))
                                text += $"{Rules_Init.SUB_KEYS.Pass}{Rules_Init.GetReplacedValues(mr.DnsProxyPass, variables)}";
                            // Add ;
                            text += ";";
                        }
                        // SNI
                        if (defaults != null)
                        {
                            if (defaults.Sni.Equals(mr.Sni))
                                mr.Sni = string.Empty;
                        }
                        if (!string.IsNullOrWhiteSpace(mr.Sni)) text += $"{Rules_Init.KEYS.Sni}{Rules_Init.GetReplacedValues(mr.Sni, variables)};";
                        // Proxy
                        if (defaults != null)
                        {
                            if (defaults.ProxyScheme.Equals(mr.ProxyScheme))
                                mr.ProxyScheme = string.Empty;
                        }
                        if (!string.IsNullOrWhiteSpace(mr.ProxyScheme))
                        {
                            // Scheme
                            text += $"{Rules_Init.KEYS.Proxy}{Rules_Init.GetReplacedValues(mr.ProxyScheme, variables)}";
                            // &user:
                            if (!string.IsNullOrWhiteSpace(mr.ProxyUser))
                                text += $"{Rules_Init.SUB_KEYS.User}{Rules_Init.GetReplacedValues(mr.ProxyUser, variables)}";
                            // &pass:
                            if (!string.IsNullOrEmpty(mr.ProxyPass))
                                text += $"{Rules_Init.SUB_KEYS.Pass}{Rules_Init.GetReplacedValues(mr.ProxyPass, variables)}";
                            // &ifblock:1
                            if (mr.ProxyIfBlock) text += $"{Rules_Init.SUB_KEYS.IfBlock}1";
                            // Add ;
                            text += ";";
                        }

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

    }
}