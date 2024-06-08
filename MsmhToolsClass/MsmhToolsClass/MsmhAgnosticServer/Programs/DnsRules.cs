using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class AgnosticProgram
{
    public partial class DnsRules
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
        private string Default_DnsProxyScheme { get; set; } = string.Empty; // dnsproxy:;
        private string Default_DnsProxyUser { get; set; } = string.Empty; // &user:
        private string Default_DnsProxyPass { get; set; } = string.Empty; // &pass:
        private List<DnsMainRules> MainRules_List { get; set; } = new();

        private class DnsMainRules
        {
            public string Client { get; set; } = string.Empty;
            public string Domain { get; set; } = string.Empty;
            public bool IsBlock { get; set; } = false;
            public string FakeDns { get; set; } = string.Empty;
            public List<string> Dnss { get; set; } = new();
            public string DnsDomain { get; set; } = string.Empty;
            public string DnsProxyScheme { get; set; } = string.Empty;
            public string DnsProxyUser { get; set; } = string.Empty;
            public string DnsProxyPass { get; set; } = string.Empty;
        }

        public DnsRules() { }

        public async void Set(Mode mode, string filePathOrText)
        {
            try
            {
                Rules_List.Clear();
                Variables.Clear();
                Default_DnsProxyScheme = string.Empty;
                Default_DnsProxyUser = string.Empty;
                Default_DnsProxyPass = string.Empty;
                MainRules_List.Clear();

                RulesMode = mode;
                PathOrText = filePathOrText;

                if (RulesMode == Mode.Disable) return;

                if (RulesMode == Mode.File)
                {
                    try
                    {
                        TextContent = await File.ReadAllTextAsync(Path.GetFullPath(PathOrText));
                    }
                    catch (Exception) { }
                }
                else if (RulesMode == Mode.Text) TextContent = PathOrText;

                if (string.IsNullOrEmpty(TextContent) || string.IsNullOrWhiteSpace(TextContent)) return;

                TextContent += Environment.NewLine;
                Rules_List = TextContent.SplitToLines();

                List<string> list = Rules_List.ToList();
                for (int n = 0; n < list.Count; n++)
                {
                    string line = list[n].Trim();
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
                    else if (line.StartsWith(Rules.KEYS.DnsProxy, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Default_DnsProxyScheme = Rules.GetValue(line, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.FirstKey, out _, out _, Variables);
                        Default_DnsProxyUser = Rules.GetValue(line, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.User, out _, out _, Variables);
                        Default_DnsProxyPass = Rules.GetValue(line, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.Pass, out _, out _, Variables);
                    }

                    // Get DnsMainRules (Client|Domain|DnsRules)
                    else if (line.Contains('|'))
                    {
                        string[] split = line.Split('|');
                        if (split.Length == 2) SetDomainRules(Rules.KEYS.AllClients, split[0].Trim(), split[1].Trim());
                        if (split.Length == 3) SetDomainRules(split[0].Trim(), split[1].Trim(), split[2].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DnsRules Set: " + ex.Message);
            }
        }

        private void SetDomainRules(string client, string domain, string rules) // rules Ends With ;
        {
            try
            {
                DnsMainRules dmr = new()
                {
                    Client = client, // Client
                    Domain = domain // Domain
                };

                // Block
                if (rules.Equals("block;", StringComparison.InvariantCultureIgnoreCase)) dmr.IsBlock = true;
                if (rules.Equals("-;")) dmr.IsBlock = true;

                if (!dmr.IsBlock)
                {
                    // Fake DNS
                    string fakeDnsIpStr = Rules.GetValue(rules, Rules.KEYS.FirstKey, null, out _, out _, Variables);
                    bool isIp = NetworkTool.IsIp(fakeDnsIpStr, out _);
                    if (isIp) dmr.FakeDns = fakeDnsIpStr;
                }

                // Dnss
                string dnss = Rules.GetValue(rules, Rules.KEYS.Dns, null, out bool isList, out List<string> list, Variables);
                if (!isList) // One Dns
                {
                    if (!string.IsNullOrEmpty(dnss))
                        dmr.Dnss.Add(dnss);
                }
                else // Multiple Dnss
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        string dns = list[i];
                        if (!string.IsNullOrEmpty(dns))
                            dmr.Dnss.Add(dns);
                    }
                }

                // DnsDomain
                dmr.DnsDomain = Rules.GetValue(rules, Rules.KEYS.DnsDomain, null, out _, out _, Variables);

                // DnsProxy e.g. socks5://127.0.0.1:6666&user:UserName&pass:PassWord
                dmr.DnsProxyScheme = Rules.GetValue(rules, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.FirstKey, out _, out _, Variables);
                dmr.DnsProxyUser = Rules.GetValue(rules, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.User, out _, out _, Variables);
                dmr.DnsProxyPass = Rules.GetValue(rules, Rules.KEYS.DnsProxy, Rules.SUB_KEYS.Pass, out _, out _, Variables);
                if (string.IsNullOrEmpty(dmr.DnsProxyScheme))
                {
                    dmr.DnsProxyScheme = Default_DnsProxyScheme;
                    dmr.DnsProxyUser = Default_DnsProxyUser;
                    dmr.DnsProxyPass = Default_DnsProxyPass;
                }

                MainRules_List.Add(dmr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DnsRules SetDomainRules: " + ex.Message);
            }
        }

    }
}