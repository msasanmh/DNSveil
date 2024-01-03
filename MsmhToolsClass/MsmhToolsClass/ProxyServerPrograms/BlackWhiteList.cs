namespace MsmhToolsClass.ProxyServerPrograms;

public partial class ProxyProgram
{
    public class BlackWhiteList
    {
        public enum Mode
        {
            BlackListFile,
            BlackListText,
            WhiteListFile,
            WhiteListText,
            Disable
        }

        public Mode ListMode { get; private set; } = Mode.Disable;
        public string PathOrText { get; private set; } = string.Empty;
        public string TextContent { get; private set; } = string.Empty;
        private List<string> BWList { get; set; } = new();

        public BlackWhiteList() { }

        /// <summary>
        /// Set Black White List Database
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <param name="filePathOrText">e.g. Each line: google.com</param>
        public void Set(Mode mode, string filePathOrText)
        {
            ListMode = mode;
            PathOrText = filePathOrText;

            if (ListMode == Mode.Disable) return;

            if (ListMode == Mode.BlackListFile || ListMode == Mode.WhiteListFile)
            {
                try
                {
                    TextContent = File.ReadAllText(Path.GetFullPath(filePathOrText));
                }
                catch (Exception) { }
            }
            else if (ListMode == Mode.BlackListText || ListMode == Mode.WhiteListText)
                TextContent = filePathOrText;

            if (!string.IsNullOrEmpty(TextContent) || !string.IsNullOrWhiteSpace(TextContent))
            {
                TextContent += Environment.NewLine;
                BWList = TextContent.SplitToLines();
            }
        }

        // If True Return, If false Continue
        public bool IsMatch(string destHostname)
        {
            string destHostnameNoWWW = destHostname;
            if (destHostnameNoWWW.StartsWith("www."))
                destHostnameNoWWW = destHostnameNoWWW.TrimStart("www.");

            if (BWList.Any())
            {
                for (int n = 0; n < BWList.Count; n++)
                {
                    string host = BWList[n].Trim();
                    if (!string.IsNullOrEmpty(host) && !host.StartsWith("//")) // Add Support Comment //
                    {
                        if (host.StartsWith("www.")) host = host.TrimStart("www.");

                        // If Match
                        if (!host.StartsWith("*."))
                        {
                            // No Wildcard
                            if (host.Equals(destHostnameNoWWW)) return match();
                        }
                        else
                        {
                            // Wildcard
                            host = host[2..];
                            if (!destHostnameNoWWW.Equals(host) && destHostnameNoWWW.EndsWith(host)) return match();
                        }
                    }
                }
            }

            // If Not Match
            return notMatch();

            bool match()
            {
                return ListMode == Mode.BlackListFile || ListMode == Mode.BlackListText;
            }

            bool notMatch()
            {
                return ListMode == Mode.WhiteListFile || ListMode == Mode.WhiteListText;
            }
        }
    }
}