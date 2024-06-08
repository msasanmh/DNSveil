using System.Xml.Linq;

namespace SecureDNSClient;

public partial class FormMain
{
    public class ReadDnsResult
    {
        public string DNS { get; set; } = string.Empty;
        public CheckMode CheckMode { get; set; } = CheckMode.Unknown;
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private async Task<List<string>> ReadCustomServersXmlGroups(string xmlPath)
    {
        List<string> groupList = new();
        Task rcs = Task.Run(() =>
        {
            try
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
            }
            catch (Exception) { }
        });

        await rcs.WaitAsync(CancellationToken.None);
        return groupList;
    }

    public static async Task<List<ReadDnsResult>> ReadCustomServersXml(string? xml, CheckRequest checkRequest, bool isXmlFile = true)
    {
        Task<List<ReadDnsResult>> rcs = Task.Run(() =>
        {
            List<ReadDnsResult> output = new();
            if (string.IsNullOrEmpty(xml)) return output;

            try
            {
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
                    if (StopChecking) return output.ToList();
                    XElement group = groups.ToList()[a];

                    XElement? dnsGroupNameElement = group.Element("Name");
                    if (dnsGroupNameElement == null) continue;
                    string dnsGroupName = dnsGroupNameElement.Value.Trim();

                    // Skip if group name is not match
                    bool isGroupNameMatch = !string.IsNullOrEmpty(checkRequest.GroupName.Trim()) &&
                                            checkRequest.GroupName.Trim().Equals(dnsGroupName);
                    if (checkRequest.HasUserGroupName && !isGroupNameMatch) continue;

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
                                string dnsAddress = dnsElement.Value.Trim();

                                ReadDnsResult rdr = new()
                                {
                                    DNS = dnsAddress,
                                    CheckMode = checkRequest.CheckMode
                                };
                                rdr.GroupName = rdr.CheckMode == CheckMode.BuiltIn || rdr.CheckMode == CheckMode.SavedServers
                                                ? checkRequest.GroupName : dnsGroupName;

                                XElement? dnsDescriptionElement = dnsItem.Element("Description");
                                if (dnsDescriptionElement != null)
                                {
                                    string dnsDescription = dnsDescriptionElement.Value.Trim();
                                    rdr.Description = dnsDescription;
                                }

                                output.Add(rdr);
                            }
                        }
                    }
                }
            }
            catch (Exception) { }

            return output;
        });

        return await rcs.WaitAsync(CancellationToken.None);
    }

}