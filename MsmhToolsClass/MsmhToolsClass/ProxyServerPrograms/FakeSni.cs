namespace MsmhToolsClass.ProxyServerPrograms;

public partial class ProxyProgram
{
    public class FakeSni
    {
        public enum Mode
        {
            File,
            Text,
            Disable
        }

        public Mode FakeSniMode { get; private set; } = Mode.Disable;
        public string PathOrText { get; private set; } = string.Empty;
        public string TextContent { get; private set; } = string.Empty;
        private List<string> Host_Sni_List { get; set; } = new();

        public FakeSni() { }

        /// <summary>
        /// Set Fake DNS Database
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <param name="filePathOrText">e.g. Each line: dns.google.com|8.8.8.8</param>
        public void Set(Mode mode, string filePathOrText)
        {
            FakeSniMode = mode;
            PathOrText = filePathOrText;

            if (FakeSniMode == Mode.Disable) return;

            if (FakeSniMode == Mode.File)
            {
                try
                {
                    TextContent = File.ReadAllText(Path.GetFullPath(filePathOrText));
                }
                catch (Exception) { }
            }
            else if (FakeSniMode == Mode.Text)
                TextContent = filePathOrText;

            if (!string.IsNullOrEmpty(TextContent) || !string.IsNullOrWhiteSpace(TextContent))
            {
                TextContent += Environment.NewLine;
                Host_Sni_List = TextContent.SplitToLines();
            }
        }

        public string Get(string destHostname)
        {
            string destHostnameNoWWW = destHostname;
            if (destHostnameNoWWW.StartsWith("www."))
                destHostnameNoWWW = destHostnameNoWWW.TrimStart("www.");

            if (destHostnameNoWWW.EndsWith('/')) destHostnameNoWWW = destHostnameNoWWW[0..^1];

            if (Host_Sni_List.Any())
            {
                for (int n = 0; n < Host_Sni_List.Count; n++)
                {
                    string hostSni = Host_Sni_List[n].Trim();
                    if (!string.IsNullOrEmpty(hostSni))
                        if (split(hostSni, out string destSni))
                            return destSni;
                }
            }

            return destHostname;

            bool split(string hostSni, out string destSni)
            {
                // Add Support Comment //
                if (hostSni.StartsWith("//"))
                {
                    destSni = destHostname; return false;
                }
                else
                {
                    try
                    {
                        if (hostSni.Contains('|'))
                        {
                            string[] split = hostSni.Split('|');
                            string host = split[0].Trim();
                            if (host.StartsWith("www.")) host = host.TrimStart("www.");
                            string sni = split[1].Trim(); // IP or Fake SNI

                            if (string.IsNullOrEmpty(sni))
                            {
                                destSni = destHostname; return false;
                            }

                            if (!host.StartsWith("*."))
                            {
                                // No Wildcard
                                if (host.Equals(destHostnameNoWWW))
                                {
                                    destSni = sni; return true;
                                }
                            }
                            else
                            {
                                // Wildcard
                                host = host[2..];
                                string hostWithDot = $".{host}";

                                if (!destHostnameNoWWW.Equals(host) && destHostnameNoWWW.EndsWith(hostWithDot))
                                {
                                    if (!sni.StartsWith("*."))
                                    {
                                        destSni = sni;
                                        return true;
                                    }
                                    else
                                    {
                                        // Support: xxxx.example.com -> xxxx.domain.com
                                        string ipOrSniWithDot = sni[1..];
                                        destSni = destHostnameNoWWW.Replace(hostWithDot, ipOrSniWithDot);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) { }

                    destSni = destHostname; return false;
                }
            }
        }
    }
}