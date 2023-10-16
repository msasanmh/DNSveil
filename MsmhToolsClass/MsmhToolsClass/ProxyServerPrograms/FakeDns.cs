using System;

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
        private List<string> HostIpList { get; set; } = new();

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
                catch (Exception)
                {
                    // do nothing
                }
            }
            else if (FakeDnsMode == Mode.Text)
                TextContent = filePathOrText;

            if (!string.IsNullOrEmpty(TextContent) || !string.IsNullOrWhiteSpace(TextContent))
            {
                TextContent += Environment.NewLine;
                HostIpList = TextContent.SplitToLines();
            }
        }

        public string Get(string destHostname)
        {
            string destHostnameNoWWW = destHostname;
            if (destHostnameNoWWW.StartsWith("www."))
                destHostnameNoWWW = destHostnameNoWWW.Replace("www.", string.Empty);

            if (HostIpList.Any())
            {
                for (int n = 0; n < HostIpList.Count; n++)
                {
                    string hostIP = HostIpList[n].Trim();
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
                    if (hostIP.Contains('|'))
                    {
                        string[] split = hostIP.Split('|');
                        string host = split[0].Trim();
                        if (host.StartsWith("www."))
                            host = host.Replace("www.", string.Empty);
                        string ip = split[1].Trim();

                        if (!host.StartsWith("*."))
                        {
                            // No Wildcard
                            if (destHostnameNoWWW.Equals(host))
                            {
                                destIP = ip; return true;
                            }
                            else
                            {
                                destIP = destHostname; return false;
                            }
                        }
                        else
                        {
                            // Wildcard
                            string destMainHost = string.Empty;
                            string[] splitByDot = destHostnameNoWWW.Split('.');

                            if (splitByDot.Length >= 3)
                            {
                                host = host[2..];

                                for (int n = 1; n < splitByDot.Length; n++)
                                    destMainHost += $"{splitByDot[n]}.";
                                if (destMainHost.EndsWith('.')) destMainHost = destMainHost[0..^1];

                                if (destMainHost.Equals(host))
                                {
                                    destIP = ip;
                                    return true;
                                }
                                else
                                {
                                    destIP = destHostname; return false;
                                }
                            }
                            else
                            {
                                destIP = destHostname; return false;
                            }
                        }
                    }
                    else
                    {
                        destIP = destHostname; return false;
                    }
                }
            }
        }
    }
}