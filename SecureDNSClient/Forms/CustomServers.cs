using System.Xml.Linq;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task<List<string>> ReadCustomServersXmlGroups(string xmlPath)
    {
        List<string> groupList = new();
        Task rcs = Task.Run(() =>
        {
            if (!File.Exists(xmlPath)) return;

            XDocument doc = XDocument.Load(xmlPath, LoadOptions.None);
            if (doc.Root == null) return;
            var groups = doc.Root.Elements().Elements();

            for (int a = 0; a < groups.Count(); a++)
            {
                if (StopChecking) return;
                XElement group = groups.ToList()[a];

                XElement? groupNameElement = group.Element("Name");
                if (groupNameElement == null) continue;

                XElement? groupEnabledElement = group.Element("Enabled");
                if (groupEnabledElement == null) continue;

                if (groupEnabledElement.Value.ToLower().Trim().Equals("true")) // If Group Enabled
                {
                    var dnss = group.Elements("DnsItem");
                    for (int b = 0; b < dnss.Count(); b++)
                    {
                        if (StopChecking) return;
                        XElement dnsItem = dnss.ToList()[b];
                        XElement? isDnsEnabled = dnsItem.Element("Enabled");
                        if (isDnsEnabled == null) continue;
                        if (isDnsEnabled.Value.ToLower().Trim().Equals("true")) // If DNS Enabled
                        {
                            // If group is not empty
                            groupList.Add(groupNameElement.Value.Trim());
                            break;
                        }
                    }
                }
            }
        });

        await rcs.WaitAsync(CancellationToken.None);
        return groupList;
    }

    public static async Task<string> ReadCustomServersXml(string? xml, string? groupName = null, bool isXmlFile = true)
    {
        Task<string> rcs = Task.Run(() =>
        {
            string output = string.Empty;
            if (string.IsNullOrEmpty(xml)) return output;

            XDocument doc;
            if (isXmlFile)
            {
                if (!File.Exists(xml)) return output;
                doc = XDocument.Load(xml, LoadOptions.None);
            }
            else
            {
                doc = XDocument.Parse(xml, LoadOptions.None);
            }

            if (doc.Root == null) return output;
            var groups = doc.Root.Elements().Elements();

            for (int a = 0; a < groups.Count(); a++)
            {
                if (StopChecking) return output;
                XElement group = groups.ToList()[a];

                XElement? groupNameElement = group.Element("Name");
                if (groupNameElement == null) continue;

                // Skip if group name is not match
                if (isXmlFile && !string.IsNullOrEmpty(groupName) && !groupName.Trim().Equals(groupNameElement.Value.Trim())) continue;

                XElement? groupEnabledElement = group.Element("Enabled");
                if (groupEnabledElement == null) continue;

                if (groupEnabledElement.Value.ToLower().Trim().Equals("true")) // If Group Enabled
                {
                    var dnss = group.Elements("DnsItem");
                    for (int b = 0; b < dnss.Count(); b++)
                    {
                        if (StopChecking) return output;
                        XElement dnsItem = dnss.ToList()[b];
                        XElement? isDnsEnabled = dnsItem.Element("Enabled");
                        if (isDnsEnabled == null) continue;
                        if (isDnsEnabled.Value.ToLower().Trim().Equals("true")) // If DNS Enabled
                        {
                            XElement? dnsElement = dnsItem.Element("Dns");
                            if (dnsElement == null) continue;
                            string dnsAddress = dnsElement.Value;
                            output += dnsAddress + NL;
                        }
                    }
                }
            }

            output = output.Trim(NL.ToCharArray());
            return output;
        });

        return await rcs.WaitAsync(CancellationToken.None);
    }

}