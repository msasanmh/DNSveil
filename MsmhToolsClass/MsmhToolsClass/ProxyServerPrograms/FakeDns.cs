namespace MsmhToolsClass.ProxyServerPrograms;

public partial class ProxyProgram
{
    public class FakeDns
    {
        public enum Mode
        {
            File,
            Text,
            Disable
        }

        public Mode FakeDnsMode { get; private set; } = Mode.Disable;
        public string PathOrText { get; private set; } = string.Empty;
        public string TextContent { get; private set; } = string.Empty;
        private List<string> Host_Ip_List { get; set; } = new();

        public FakeDns() { }

        /// <summary>
        /// Set Fake DNS Database
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <param name="filePathOrText">e.g. Each line: dns.google.com|8.8.8.8</param>
        public void Set(Mode mode, string filePathOrText)
        {
            FakeDnsMode = mode;
            PathOrText = filePathOrText;

            if (FakeDnsMode == Mode.Disable) return;

            if (FakeDnsMode == Mode.File)
            {
                try
                {
                    TextContent = File.ReadAllText(Path.GetFullPath(filePathOrText));
                }
                catch (Exception) { }
            }
            else if (FakeDnsMode == Mode.Text)
                TextContent = filePathOrText;

            if (!string.IsNullOrEmpty(TextContent) || !string.IsNullOrWhiteSpace(TextContent))
            {
                TextContent += Environment.NewLine;
                Host_Ip_List = TextContent.SplitToLines();
            }
        }

        public string Get(string destHostname)
        {
            string destHostnameNoWWW = destHostname;
            if (destHostnameNoWWW.StartsWith("www."))
                destHostnameNoWWW = destHostnameNoWWW.TrimStart("www.");

            if (destHostnameNoWWW.EndsWith('/')) destHostnameNoWWW = destHostnameNoWWW[0..^1];

            if (Host_Ip_List.Any())
            {
                for (int n = 0; n < Host_Ip_List.Count; n++)
                {
                    string hostIP = Host_Ip_List[n].Trim();
                    if (!string.IsNullOrEmpty(hostIP))
                        if (split(hostIP, out string destIP))
                            return destIP;
                }
            }

            return destHostname;

            bool split(string hostIP, out string destIP)
            {
                // Add Support Comment //
                if (hostIP.StartsWith("//"))
                {
                    destIP = destHostname; return false;
                }
                else
                {
                    try
                    {
                        if (hostIP.Contains('|'))
                        {
                            string[] split = hostIP.Split('|');
                            string host = split[0].Trim();
                            if (host.StartsWith("www.")) host = host.TrimStart("www.");
                            string ip = split[1].Trim(); // IP or Fake SNI

                            if (!host.StartsWith("*."))
                            {
                                // No Wildcard
                                if (host.Equals(destHostnameNoWWW))
                                {
                                    destIP = ip; return true;
                                }
                            }
                            else
                            {
                                // Wildcard
                                host = host[2..];
                                string hostWithDot = $".{host}";

                                if (!destHostnameNoWWW.Equals(host) && destHostnameNoWWW.EndsWith(hostWithDot))
                                {
                                    destIP = ip;
                                    return true;
                                }
                            }
                        }
                    }
                    catch (Exception) { }

                    destIP = destHostname; return false;
                }
            }
        }
    }
}