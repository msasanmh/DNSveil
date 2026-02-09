using System.Diagnostics;

namespace MsmhToolsClass.MsmhAgnosticServer;

public partial class AgnosticProgram
{
    public class DnsLimit
    {
        public class DnsLimitResult
        {
            public bool IsPlainDnsDisable { get; set; } = false;
            public bool IsDoHPathAllowed { get; set; } = false;
        }

        public enum LimitDoHPathsMode
        {
            File,
            Text,
            Disable
        }

        public bool EnableDnsLimit { get; private set; } = false;
        public bool DisablePlainDns { get; private set; } = false;
        public LimitDoHPathsMode LimitDoHMode { get; private set; } = LimitDoHPathsMode.Disable;
        public string PathOrText { get; private set; } = string.Empty;
        public string TextContent { get; private set; } = string.Empty;

        private List<string> AllowedDoHPaths_List { get; set; } = new();

        public DnsLimit() { }

        public async void Set(bool enableDnsLimit, bool disablePlainDns, LimitDoHPathsMode mode, string filePathOrText)
        {
            try
            {
                EnableDnsLimit = enableDnsLimit;
                DisablePlainDns = disablePlainDns;
                LimitDoHMode = mode;
                PathOrText = filePathOrText;
                AllowedDoHPaths_List.Clear();

                if (!EnableDnsLimit)
                {
                    DisablePlainDns = false;
                    LimitDoHMode = LimitDoHPathsMode.Disable;
                    PathOrText = string.Empty;
                    return;
                }

                if (LimitDoHMode == LimitDoHPathsMode.Disable) return;

                if (LimitDoHMode == LimitDoHPathsMode.File)
                {
                    try
                    {
                        TextContent = await File.ReadAllTextAsync(Path.GetFullPath(PathOrText));
                    }
                    catch (Exception) { }
                }
                else if (LimitDoHMode == LimitDoHPathsMode.Text) TextContent = PathOrText;

                if (string.IsNullOrEmpty(TextContent) || string.IsNullOrWhiteSpace(TextContent)) return;

                TextContent += Environment.NewLine;

                List<string> list = TextContent.SplitToLines();
                for (int n = 0; n < list.Count; n++)
                {
                    string line = list[n].Trim();
                    if (line.StartsWith("//")) continue; // Support Comment //
                    AllowedDoHPaths_List.Add(line);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DnsLimit Set: " + ex.Message);
            }
        }

        public DnsLimitResult Get(DnsEnums.DnsProtocol dnsProtocol, string dohPath)
        {
            DnsLimitResult dlr = new();

            try
            {
                if (EnableDnsLimit)
                {
                    if (dnsProtocol == DnsEnums.DnsProtocol.UDP || dnsProtocol == DnsEnums.DnsProtocol.TCP)
                        dlr.IsPlainDnsDisable = DisablePlainDns;
                    else
                        dlr.IsPlainDnsDisable = false;

                    if (dnsProtocol == DnsEnums.DnsProtocol.DoH && !string.IsNullOrWhiteSpace(dohPath) && LimitDoHMode != LimitDoHPathsMode.Disable)
                    {
                        List<string> list = AllowedDoHPaths_List.ToList();
                        dlr.IsDoHPathAllowed = list.IsContain(dohPath.Trim());
                    }
                    else
                        dlr.IsDoHPathAllowed = true;
                }
                else
                {
                    dlr.IsPlainDnsDisable = false;
                    dlr.IsDoHPathAllowed = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DnsLimit Get: " + ex.Message);
            }

            return dlr;
        }
    }
}
