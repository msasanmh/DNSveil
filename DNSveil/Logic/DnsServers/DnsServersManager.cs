using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using MsmhToolsWpfClass;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using static DNSveil.Logic.DnsServers.EnumsAndStructs;

namespace DNSveil.Logic.DnsServers;

public partial class DnsServersManager
{
    private XDocument XDoc = new();
    private string XmlFilePath_BultIn = string.Empty;
    private string XmlFilePath = string.Empty;
    private string XmlFilePath_Backup => Path.GetFullPath(XmlFilePath.Replace(".xml", "_Backup.xml"));
    private bool IsInitialized = false;
    private Window? Owner = null;
    private bool PauseBackgroundTask => UI_Window.IsWindowOpen<Window>();

    public bool IsBackgroundTaskWorking { get; private set; } = false;
    public Window? UI_Window { get; set; } = null;

    // WPF: ObservableCollection<T>      WinUI: ObservableVector<T>
    public ObservableCollection<DnsItem> BindDataSource_Subscription { get; set; } = new();
    public ObservableCollection<DnsItem> BindDataSource_AnonDNSCrypt { get; set; } = new();
    public ObservableCollection<DnsItem> BindDataSource_FragmentDoH { get; set; } = new();
    public ObservableCollection<DnsItem> BindDataSource_Custom { get; set; } = new();

    public DnsServersManager()
    {
        BackgroundWorker_AutoUpdate_Groups();
    }

    public async Task SaveAsync()
    {
        await XDoc.SaveAsync(XmlFilePath);
    }

    public static XDocument CreateXDoc()
    {
        XElement root = new("DNSveilDnsGroups");
        XDocument xDoc = new();
        xDoc.Add(root);
        return xDoc;
    }

    /// <summary>
    /// Create Or Load XDocument
    /// </summary>
    /// <param name="userServers">User XML File Path</param>
    /// <returns>Returns True If Success</returns>
    public async Task<bool> InitializeAsync(string builtInServers, string userServers, Window? window = null)
    {
        Owner = window;
        IsInitialized = false;
        bool isSaved;

        try
        {
            XmlFilePath_BultIn = builtInServers;
            XmlFilePath = userServers;

            if (!File.Exists(XmlFilePath))
            {
                bool isBuiltInValid = await XmlTool.IsValidFileAsync(XmlFilePath_BultIn);
                if (!isBuiltInValid)
                {
                    File.Create(XmlFilePath).Dispose();
                    XDoc = CreateXDoc();
                    isSaved = await XDoc.SaveAsync(XmlFilePath);
                }
                else
                {
                    try
                    {
                        await File.WriteAllBytesAsync(XmlFilePath, await File.ReadAllBytesAsync(XmlFilePath_BultIn));
                        isSaved = true;
                    }
                    catch (Exception)
                    {
                        isSaved = false;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(XmlFilePath)))
            {
                bool isBuiltInValid = await XmlTool.IsValidFileAsync(XmlFilePath_BultIn);
                if (!isBuiltInValid)
                {
                    XDoc = CreateXDoc();
                    isSaved = await XDoc.SaveAsync(XmlFilePath);
                }
                else
                {
                    try
                    {
                        await File.WriteAllBytesAsync(XmlFilePath, await File.ReadAllBytesAsync(XmlFilePath_BultIn));
                        isSaved = true;
                    }
                    catch (Exception)
                    {
                        isSaved = false;
                    }
                }
            }
            else if (!await XmlTool.IsValidFileAsync(XmlFilePath))
            {
                if (await XmlTool.IsValidFileAsync(XmlFilePath_Backup))
                {
                    try
                    {
                        await File.WriteAllBytesAsync(XmlFilePath, await File.ReadAllBytesAsync(XmlFilePath_Backup));
                        isSaved = true;

                        string msg = "XML File Is Not Valid. Backup Restored.";
                        WpfMessageBox.Show(Owner, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception)
                    {
                        isSaved = false;

                        string msg = "XML File Is Not Valid. Couldn't Restore Backup!";
                        WpfMessageBox.Show(Owner, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    bool isBuiltInValid = await XmlTool.IsValidFileAsync(XmlFilePath_BultIn);
                    if (!isBuiltInValid)
                    {
                        XDoc = CreateXDoc();
                        isSaved = await XDoc.SaveAsync(XmlFilePath);

                        if (isSaved)
                        {
                            string msg = "XML File Is Not Valid. Returned To Default.";
                            WpfMessageBox.Show(Owner, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            string msg = "XML File Is Not Valid. Couldn't Restore Default!";
                            WpfMessageBox.Show(Owner, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        try
                        {
                            await File.WriteAllBytesAsync(XmlFilePath, await File.ReadAllBytesAsync(XmlFilePath_BultIn));
                            isSaved = true;
                        }
                        catch (Exception)
                        {
                            isSaved = false;
                        }
                    }
                }
            }
            else isSaved = true;

            if (isSaved)
            {
                var doc = await XDoc.LoadAsync(XmlFilePath);
                if (doc.IsLoaded)
                {
                    IsInitialized = true;
                    XDoc = doc.XDoc;
                }
            }
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(Owner, ex.Message, "ERROR Initialize", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return IsInitialized;
    }

    public static XElement Create_GroupSettings_Element(GroupSettings groupSettings)
    {
        XElement element = new(nameof(GroupSettings));
        element.Add(new XElement(nameof(groupSettings.Enabled), groupSettings.Enabled));
        element.Add(new XElement(nameof(groupSettings.LookupDomain), groupSettings.LookupDomain));
        element.Add(new XElement(nameof(groupSettings.TimeoutSec), groupSettings.TimeoutSec));
        element.Add(new XElement(nameof(groupSettings.ParallelSize), groupSettings.ParallelSize));
        element.Add(new XElement(nameof(groupSettings.BootstrapIP), groupSettings.BootstrapIP));
        element.Add(new XElement(nameof(groupSettings.BootstrapPort), groupSettings.BootstrapPort));
        element.Add(new XElement(nameof(groupSettings.MaxServersToConnect), groupSettings.MaxServersToConnect));
        element.Add(new XElement(nameof(groupSettings.AllowInsecure), groupSettings.AllowInsecure));
        return element;
    }

    public static GroupSettings Create_GroupSettings(XElement? groupSettingsElement)
    {
        GroupSettings groupSettings = new();

        try
        {
            if (groupSettingsElement == null) return groupSettings;
            var elements = groupSettingsElement.Elements();
            foreach (XElement element in elements)
            {
                if (element.Name.LocalName.Equals(nameof(groupSettings.Enabled)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool result);
                    if (isBool) groupSettings.Enabled = result;
                }
                else if (element.Name.LocalName.Equals(nameof(groupSettings.LookupDomain)))
                {
                    groupSettings.LookupDomain = element.Value;
                }
                else if (element.Name.LocalName.Equals(nameof(groupSettings.TimeoutSec)))
                {
                    bool isDouble = double.TryParse(element.Value, out double value);
                    if (isDouble) groupSettings.TimeoutSec = value;
                    else
                    {
                        try
                        {
                            double doubleValue = Convert.ToDouble(element.Value);
                            groupSettings.TimeoutSec = doubleValue;
                        }
                        catch (Exception) { }
                    }
                }
                else if (element.Name.LocalName.Equals(nameof(groupSettings.ParallelSize)))
                {
                    bool isInt = int.TryParse(element.Value, out int value);
                    if (isInt) groupSettings.ParallelSize = value;
                }
                else if (element.Name.LocalName.Equals(nameof(groupSettings.BootstrapIP)))
                {
                    bool isIP = IPAddress.TryParse(element.Value, out IPAddress? ip);
                    if (isIP && ip != null) groupSettings.BootstrapIP = ip;
                }
                else if (element.Name.LocalName.Equals(nameof(groupSettings.BootstrapPort)))
                {
                    bool isInt = int.TryParse(element.Value, out int value);
                    if (isInt) groupSettings.BootstrapPort = value;
                }
                else if (element.Name.LocalName.Equals(nameof(groupSettings.MaxServersToConnect)))
                {
                    bool isInt = int.TryParse(element.Value, out int value);
                    if (isInt) groupSettings.MaxServersToConnect = value;
                }
                else if (element.Name.LocalName.Equals(nameof(groupSettings.AllowInsecure)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool value);
                    if (isBool) groupSettings.AllowInsecure = value;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Create_GroupSettings: " + ex.Message);
        }

        return groupSettings;
    }

    public static XElement Create_AutoUpdate_Element(AutoUpdate autoUpdate, LastAutoUpdate lastAutoUpdate)
    {
        XElement element = new(nameof(AutoUpdate));
        element.Add(new XElement(nameof(autoUpdate.UpdateSource), autoUpdate.UpdateSource));
        element.Add(new XElement(nameof(lastAutoUpdate.LastUpdateSource), lastAutoUpdate.LastUpdateSource));
        element.Add(new XElement(nameof(autoUpdate.ScanServers), autoUpdate.ScanServers));
        element.Add(new XElement(nameof(lastAutoUpdate.LastScanServers), lastAutoUpdate.LastScanServers));
        return element;
    }

    public static AutoUpdate Create_AutoUpdate(XElement? autoUpdateElement)
    {
        AutoUpdate autoUpdate = new();

        try
        {
            if (autoUpdateElement == null) return autoUpdate;
            var elements = autoUpdateElement.Elements();
            foreach (XElement element in elements)
            {
                if (element.Name.LocalName.Equals(nameof(autoUpdate.UpdateSource)))
                {
                    bool isInt = int.TryParse(element.Value, out int value);
                    if (isInt) autoUpdate.UpdateSource = value;
                }
                else if (element.Name.LocalName.Equals(nameof(autoUpdate.ScanServers)))
                {
                    bool isInt = int.TryParse(element.Value, out int value);
                    if (isInt) autoUpdate.ScanServers = value;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Create_AutoUpdate: " + ex.Message);
        }

        return autoUpdate;
    }

    public static LastAutoUpdate Create_LastAutoUpdate(XElement? autoUpdateElement)
    {
        LastAutoUpdate lastAutoUpdate = new();

        try
        {
            if (autoUpdateElement == null) return lastAutoUpdate;
            var elements = autoUpdateElement.Elements();
            foreach (XElement element in elements)
            {
                if (element.Name.LocalName.Equals(nameof(lastAutoUpdate.LastUpdateSource)))
                {
                    bool isDateTime = DateTime.TryParse(element.Value, out DateTime value);
                    if (isDateTime) lastAutoUpdate.LastUpdateSource = value;
                }
                else if (element.Name.LocalName.Equals(nameof(lastAutoUpdate.LastScanServers)))
                {
                    bool isDateTime = DateTime.TryParse(element.Value, out DateTime value);
                    if (isDateTime) lastAutoUpdate.LastScanServers = value;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Create_LastAutoUpdate: " + ex.Message);
        }

        return lastAutoUpdate;
    }

    public static XElement Create_FilterByProtocols_Element(FilterByProtocols filterByProtocols)
    {
        XElement element = new(nameof(FilterByProtocols));
        element.Add(new XElement(nameof(filterByProtocols.UDP), filterByProtocols.UDP));
        element.Add(new XElement(nameof(filterByProtocols.TCP), filterByProtocols.TCP));
        element.Add(new XElement(nameof(filterByProtocols.DnsCrypt), filterByProtocols.DnsCrypt));
        element.Add(new XElement(nameof(filterByProtocols.DoT), filterByProtocols.DoT));
        element.Add(new XElement(nameof(filterByProtocols.DoH), filterByProtocols.DoH));
        element.Add(new XElement(nameof(filterByProtocols.DoQ), filterByProtocols.DoQ));
        element.Add(new XElement(nameof(filterByProtocols.AnonymizedDNSCrypt), filterByProtocols.AnonymizedDNSCrypt));
        element.Add(new XElement(nameof(filterByProtocols.ObliviousDoH), filterByProtocols.ObliviousDoH));
        return element;
    }

    public static FilterByProtocols Create_FilterByProtocols(XElement? filterByProtocolsElement)
    {
        FilterByProtocols filterByProtocols = new();
        
        try
        {
            if (filterByProtocolsElement == null) return filterByProtocols;
            var elements = filterByProtocolsElement.Elements();
            foreach (XElement element in elements)
            {
                if (element.Name.LocalName.Equals(nameof(filterByProtocols.UDP)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool result);
                    if (isBool) filterByProtocols.UDP = result;
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProtocols.TCP)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool result);
                    if (isBool) filterByProtocols.TCP = result;
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProtocols.DnsCrypt)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool result);
                    if (isBool) filterByProtocols.DnsCrypt = result;
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProtocols.DoT)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool result);
                    if (isBool) filterByProtocols.DoT = result;
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProtocols.DoH)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool result);
                    if (isBool) filterByProtocols.DoH = result;
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProtocols.DoQ)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool result);
                    if (isBool) filterByProtocols.DoQ = result;
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProtocols.AnonymizedDNSCrypt)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool result);
                    if (isBool) filterByProtocols.AnonymizedDNSCrypt = result;
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProtocols.ObliviousDoH)))
                {
                    bool isBool = bool.TryParse(element.Value, out bool result);
                    if (isBool) filterByProtocols.ObliviousDoH = result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Create_FilterByProtocols: " + ex.Message);
        }

        return filterByProtocols;
    }

    public static XElement Create_FilterByProperties_Element(FilterByProperties filterByProperties)
    {
        XElement element = new(nameof(FilterByProperties));
        element.Add(new XElement(nameof(filterByProperties.GoogleSafeSearch), filterByProperties.GoogleSafeSearch));
        element.Add(new XElement(nameof(filterByProperties.BingSafeSearch), filterByProperties.BingSafeSearch));
        element.Add(new XElement(nameof(filterByProperties.YoutubeRestricted), filterByProperties.YoutubeRestricted));
        element.Add(new XElement(nameof(filterByProperties.AdultBlocked), filterByProperties.AdultBlocked));
        return element;
    }

    public static FilterByProperties Create_FilterByProperties(XElement? filterByPropertiesElement)
    {
        FilterByProperties filterByProperties = new();
        
        try
        {
            if (filterByPropertiesElement == null) return filterByProperties;
            var elements = filterByPropertiesElement.Elements();
            foreach (XElement element in elements)
            {
                if (element.Name.LocalName.Equals(nameof(filterByProperties.GoogleSafeSearch)))
                {
                    filterByProperties.GoogleSafeSearch = Get_DnsFilter_ByName(element.Value);
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProperties.BingSafeSearch)))
                {
                    filterByProperties.BingSafeSearch = Get_DnsFilter_ByName(element.Value);
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProperties.YoutubeRestricted)))
                {
                    filterByProperties.YoutubeRestricted = Get_DnsFilter_ByName(element.Value);
                }
                else if (element.Name.LocalName.Equals(nameof(filterByProperties.AdultBlocked)))
                {
                    filterByProperties.AdultBlocked = Get_DnsFilter_ByName(element.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Create_FilterByProperties: " + ex.Message);
        }

        return filterByProperties;
    }

    public static XElement Create_FragmentSettings_Element(FragmentSettings fragmentSettings)
    {
        XElement element = new(nameof(FragmentSettings));
        element.Add(new XElement(nameof(fragmentSettings.ChunksBeforeSNI), fragmentSettings.ChunksBeforeSNI));
        element.Add(new XElement(nameof(fragmentSettings.SniChunkMode), fragmentSettings.SniChunkMode));
        element.Add(new XElement(nameof(fragmentSettings.ChunksSNI), fragmentSettings.ChunksSNI));
        element.Add(new XElement(nameof(fragmentSettings.AntiPatternOffset), fragmentSettings.AntiPatternOffset));
        element.Add(new XElement(nameof(fragmentSettings.FragmentDelayMS), fragmentSettings.FragmentDelayMS));
        return element;
    }

    public static FragmentSettings Create_FragmentSettings(XElement? fragmentSettingsElement)
    {
        FragmentSettings fragmentSettings = new();

        try
        {
            if (fragmentSettingsElement == null) return fragmentSettings;
            var elements = fragmentSettingsElement.Elements();
            foreach (XElement element in elements)
            {
                if (element.Name.LocalName.Equals(nameof(fragmentSettings.ChunksBeforeSNI)))
                {
                    bool isInt = int.TryParse(element.Value, out int result);
                    if (isInt) fragmentSettings.ChunksBeforeSNI = result;
                }
                else if (element.Name.LocalName.Equals(nameof(fragmentSettings.SniChunkMode)))
                {
                    fragmentSettings.SniChunkMode = AgnosticProgram.Fragment.GetChunkModeByName(element.Value);
                }
                else if (element.Name.LocalName.Equals(nameof(fragmentSettings.ChunksSNI)))
                {
                    bool isInt = int.TryParse(element.Value, out int result);
                    if (isInt) fragmentSettings.ChunksSNI = result;
                }
                else if (element.Name.LocalName.Equals(nameof(fragmentSettings.AntiPatternOffset)))
                {
                    bool isInt = int.TryParse(element.Value, out int result);
                    if (isInt) fragmentSettings.AntiPatternOffset = result;
                }
                else if (element.Name.LocalName.Equals(nameof(fragmentSettings.FragmentDelayMS)))
                {
                    bool isInt = int.TryParse(element.Value, out int result);
                    if (isInt) fragmentSettings.FragmentDelayMS = result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Create_FragmentSettings: " + ex.Message);
        }

        return fragmentSettings;
    }

    public static XElement Create_DnsItem_Element(DnsItem di)
    {
        XElement element = new(nameof(DnsItem), new XAttribute(nameof(di.Enabled), di.Enabled));
        element.Add(new XElement(nameof(di.IDUnique), di.IDUnique));
        element.Add(new XElement(nameof(di.DNS_URL), di.DNS_URL));
        element.Add(new XElement(nameof(di.DNS_IP), di.DNS_IP));
        element.Add(new XElement(nameof(di.Protocol), di.Protocol));
        element.Add(new XElement(nameof(di.Status), di.Status));
        element.Add(new XElement(nameof(di.Latency), di.Latency));
        element.Add(new XElement(nameof(di.IsGoogleSafeSearchEnabled), di.IsGoogleSafeSearchEnabled));
        element.Add(new XElement(nameof(di.IsBingSafeSearchEnabled), di.IsBingSafeSearchEnabled));
        element.Add(new XElement(nameof(di.IsYoutubeRestricted), di.IsYoutubeRestricted));
        element.Add(new XElement(nameof(di.IsAdultBlocked), di.IsAdultBlocked));
        element.Add(new XElement(nameof(di.Description), di.Description));
        return element;
    }

    public static DnsItem Create_DnsItem(XElement dnsItemElement)
    {
        DnsItem dnsItem = new();

        try
        {
            var attributes = dnsItemElement.Attributes();
            foreach (XAttribute attribute in attributes)
            {
                if (attribute.Name.LocalName.Equals(nameof(dnsItem.Enabled)))
                {
                    bool isBool = bool.TryParse(attribute.Value, out bool result);
                    if (isBool) dnsItem.Enabled = result;
                }
            }

            var elements = dnsItemElement.Elements();
            foreach (XElement element in elements)
            {
                if (element.Name.LocalName.Equals(nameof(dnsItem.IDUnique)))
                {
                    dnsItem.IDUnique = element.Value;
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.DNS_URL)))
                {
                    dnsItem.DNS_URL = element.Value;
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.DNS_IP)))
                {
                    bool isIP = IPAddress.TryParse(element.Value, out IPAddress? ip);
                    if (isIP && ip != null)
                    {
                        dnsItem.DNS_IP = ip;
                    }
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.Protocol)))
                {
                    dnsItem.Protocol = element.Value;
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.Status)))
                {
                    dnsItem.Status = GetDnsStatusByName(element.Value);
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.Latency)))
                {
                    bool isInt = int.TryParse(element.Value, out int result);
                    if (isInt) dnsItem.Latency = result;
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.IsGoogleSafeSearchEnabled)))
                {
                    dnsItem.IsGoogleSafeSearchEnabled = Get_DnsFilter_ByName(element.Value);
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.IsBingSafeSearchEnabled)))
                {
                    dnsItem.IsBingSafeSearchEnabled = Get_DnsFilter_ByName(element.Value);
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.IsYoutubeRestricted)))
                {
                    dnsItem.IsYoutubeRestricted = Get_DnsFilter_ByName(element.Value);
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.IsAdultBlocked)))
                {
                    dnsItem.IsAdultBlocked = Get_DnsFilter_ByName(element.Value);
                }
                else if (element.Name.LocalName.Equals(nameof(dnsItem.Description)))
                {
                    dnsItem.Description = element.Value;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Create_DnsItem: " + ex.Message);
        }

        return dnsItem;
    }

    public async Task Create_Group_Async(string groupName, GroupMode groupMode, bool saveToFile = true)
    {
        try
        {
            XElement? root = XDoc.Root;
            if (root == null) return;
            if (groupMode == GroupMode.None) return;

            groupName = groupName.Trim();

            XElement group = new("Group", new XAttribute("BuiltIn", false));
            group.Add(new XElement("Name", groupName));
            group.Add(new XElement("Mode", groupMode));
            group.Add(Create_GroupSettings_Element(new GroupSettings()));

            if (groupMode == GroupMode.Subscription)
            {
                XElement sourceElement = new("Source", new XAttribute("Enabled", true));

                XElement optionsElement = new("Options");
                optionsElement.Add(Create_AutoUpdate_Element(new AutoUpdate(), new LastAutoUpdate()));
                optionsElement.Add(Create_FilterByProtocols_Element(new FilterByProtocols()));
                optionsElement.Add(Create_FilterByProperties_Element(new FilterByProperties()));

                XElement subscriptionElement = new(groupMode.ToString());
                subscriptionElement.Add(sourceElement);
                subscriptionElement.Add(optionsElement);

                group.Add(subscriptionElement);
                group.Add(new XElement("DnsItems"));
            }
            else if (groupMode == GroupMode.AnonymizedDNSCrypt)
            {
                XElement sourceElement = new("Source", new XAttribute("Enabled", true));
                sourceElement.Add(new XElement("Relays"));
                sourceElement.Add(new XElement("Targets"));

                XElement optionsElement = new("Options");
                optionsElement.Add(Create_AutoUpdate_Element(new AutoUpdate(), new LastAutoUpdate()));
                optionsElement.Add(Create_FilterByProperties_Element(new FilterByProperties()));

                XElement anonymizedDNSCryptElement = new(groupMode.ToString());
                anonymizedDNSCryptElement.Add(sourceElement);
                anonymizedDNSCryptElement.Add(optionsElement);

                group.Add(anonymizedDNSCryptElement);
                group.Add(new XElement("RelayItems"));
                group.Add(new XElement("TargetItems"));
                group.Add(new XElement("DnsItems"));
            }
            else if (groupMode == GroupMode.FragmentDoH)
            {
                XElement sourceElement = new("Source", new XAttribute("Enabled", true));

                XElement optionsElement = new("Options");
                optionsElement.Add(Create_AutoUpdate_Element(new AutoUpdate(), new LastAutoUpdate()));
                optionsElement.Add(Create_FilterByProperties_Element(new FilterByProperties()));
                optionsElement.Add(Create_FragmentSettings_Element(new FragmentSettings()));

                XElement fragmentDoHElement = new(groupMode.ToString());
                fragmentDoHElement.Add(sourceElement);
                fragmentDoHElement.Add(optionsElement);

                group.Add(fragmentDoHElement);
                group.Add(new XElement("DnsItems"));
            }
            else if (groupMode == GroupMode.Custom)
            {
                XElement sourceElement = new("Source", new XAttribute("Enabled", true));

                XElement optionsElement = new("Options");
                optionsElement.Add(Create_AutoUpdate_Element(new AutoUpdate(), new LastAutoUpdate()));
                optionsElement.Add(Create_FilterByProtocols_Element(new FilterByProtocols()));
                optionsElement.Add(Create_FilterByProperties_Element(new FilterByProperties()));

                XElement customElement = new(groupMode.ToString());
                customElement.Add(sourceElement);
                customElement.Add(optionsElement);

                group.Add(customElement);
                group.Add(new XElement("DnsItems"));
            }

            root.Add(group);
            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Create_Group_Async: " + ex.Message);
        }
    }

    public XElement? Get_Group_Element(string groupName)
    {
        groupName = groupName.Trim();
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new ("Name", XmlNodeType.Text, groupName) })
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    public List<string> Get_Group_Names(bool getOnlyEnabledGroups)
    {
        try
        {
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group"),
                new XmlTool.XmlPath("Name")
            };

            if (getOnlyEnabledGroups)
            {
                paths = new()
                {
                    new XmlTool.XmlPath("Group", 0, true, new List<XmlTool.XmlChildCondition>() { new(nameof(GroupSettings)), new("Enabled", XmlNodeType.Text, "true") }),
                    new XmlTool.XmlPath("Name")
                };
            }

            List<XElement> elements = XmlTool.GetElements(XDoc, paths);
            return elements.Select(x => x.Value).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Group_Names: " + ex.Message);
            return new List<string>();
        }
    }

    public GroupMode Get_GroupMode_ByName(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
                new XmlTool.XmlPath("Mode", 1)
            };

            List<XElement> elements = XmlTool.GetElements(XDoc, paths);
            if (elements.Count > 0) return Get_GroupMode(elements[0].Value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_GroupMode_ByName: " + ex.Message);
        }

        return GroupMode.None;
    }

    public static GroupItem Get_GroupItem(XElement? groupElement)
    {
        GroupItem groupItem = new();

        try
        {
            if (groupElement == null) return groupItem;

            // Get BuiltIn
            if (groupElement.HasAttributes)
            {
                XAttribute? xAttribute = groupElement.Attribute(nameof(groupItem.BuiltIn));
                if (xAttribute != null)
                {
                    bool isBool = bool.TryParse(xAttribute.Value, out bool result);
                    if (isBool) groupItem.BuiltIn = result;
                }
            }

            // Get Name
            XElement? nameElement = groupElement.Element(nameof(groupItem.Name));
            if (nameElement != null)
            {
                groupItem.Name = nameElement.Value;
            }

            // Get Mode
            XElement? modeElement = groupElement.Element(nameof(GroupItem.Mode));
            if (modeElement != null)
            {
                groupItem.Mode = Get_GroupMode(modeElement.Value);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_GroupItem: " + ex.Message);
        }

        return groupItem;
    }

    /// <summary>
    /// Set As BuiltIn Or User
    /// </summary>
    /// <param name="groupName">Group Name</param>
    /// <param name="newValue">BuiltIn: True, User: False</param>
    public async Task Update_GroupItem_BuiltIn_Async(string groupName, bool newValue, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();
            XElement? groupElement = Get_Group_Element(groupName);
            if (groupElement != null && groupElement.HasAttributes)
            {
                XAttribute? xAttribute = groupElement.Attribute("BuiltIn");
                xAttribute?.SetValue(newValue);
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_GroupItem_BuiltIn_Async: " + ex.Message);
        }
    }

    public List<GroupItem> Get_GroupItems(bool getOnlyEnabledGroups)
    {
        List<GroupItem> groupItems = new();

        try
        {
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group")
            };

            if (getOnlyEnabledGroups)
            {
                paths = new()
                {
                    new XmlTool.XmlPath("Group", 0, true, new List<XmlTool.XmlChildCondition>() { new(nameof(GroupSettings)), new("Enabled", XmlNodeType.Text, "true") })
                };
            }

            List<XElement> groupElements = XmlTool.GetElements(XDoc, paths);
            for (int n = 0; n < groupElements.Count; n++)
            {
                XElement groupElement = groupElements[n];
                groupItems.Add(Get_GroupItem(groupElement)); // Add To List
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_GroupItems: " + ex.Message);
        }

        return groupItems;
    }

    private XElement? Get_Source_Element(string groupName)
    {
        groupName = groupName.Trim();
        GroupMode groupMode = Get_GroupMode_ByName(groupName);
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath($"{groupMode}", 1),
            new XmlTool.XmlPath("Source", 1)
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    private XElement? Get_GroupSettings_Element(string groupName)
    {
        groupName = groupName.Trim();
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath(nameof(GroupSettings), 1)
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    public XDocument? Export_Groups(List<string> groupNames)
    {
        try
        {
            XDocument? xDoc_Export = CreateXDoc();
            if (xDoc_Export != null && xDoc_Export.Root != null)
            {
                for (int n = 0; n < groupNames.Count; n++)
                {
                    string groupName = groupNames[n];
                    XElement? groupElement = Get_Group_Element(groupName);
                    if (groupElement != null)
                    {
                        xDoc_Export.Root.Add(groupElement);
                    }
                }

                // Get Group Elements
                List<XmlTool.XmlPath> paths = new()
                {
                    new XmlTool.XmlPath("Group")
                };

                // Convert Built-In To User
                List<XElement> elements = XmlTool.GetElements(xDoc_Export, paths);
                for (int n = 0; n < elements.Count; n++)
                {
                    XElement groupElement = elements[n];
                    if (groupElement.HasAttributes)
                    {
                        XAttribute? xAttribute = groupElement.Attribute("BuiltIn");
                        xAttribute?.SetValue(false);
                    }
                }
            }
            return xDoc_Export;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Export_Groups: " + ex.Message);
            return null;
        }
    }

    private XElement? Get_AutoUpdate_Element(string groupName)
    {
        groupName = groupName.Trim();
        GroupMode groupMode = Get_GroupMode_ByName(groupName);
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath($"{groupMode}", 1),
            new XmlTool.XmlPath("Options", 1),
            new XmlTool.XmlPath(nameof(AutoUpdate), 1)
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    /// <summary>
    /// Only Subscription And Custom
    /// </summary>
    private XElement? Get_FilterByProtocols_Element(string groupName)
    {
        groupName = groupName.Trim();
        GroupMode groupMode = Get_GroupMode_ByName(groupName);
        if (groupMode == GroupMode.AnonymizedDNSCrypt || groupMode == GroupMode.FragmentDoH || groupMode == GroupMode.None) return null;
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath($"{groupMode}", 1),
            new XmlTool.XmlPath("Options", 1),
            new XmlTool.XmlPath(nameof(FilterByProtocols), 1)
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    private XElement? Get_FilterByProperties_Element(string groupName)
    {
        groupName = groupName.Trim();
        GroupMode groupMode = Get_GroupMode_ByName(groupName);
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath($"{groupMode}", 1),
            new XmlTool.XmlPath("Options", 1),
            new XmlTool.XmlPath(nameof(FilterByProperties), 1)
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    /// <summary>
    /// Only FragmentDoH
    /// </summary>
    private XElement? Get_FragmentSettings_Element(string groupName)
    {
        groupName = groupName.Trim();
        GroupMode groupMode = Get_GroupMode_ByName(groupName);
        if (groupMode != GroupMode.FragmentDoH) return null;
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath($"{groupMode}", 1),
            new XmlTool.XmlPath("Options", 1),
            new XmlTool.XmlPath(nameof(FragmentSettings), 1)
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    private XElement? Get_DnsItems_Element(string groupName)
    {
        groupName = groupName.Trim();
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath("DnsItems", 1)
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    private XElement? Get_AnonDNSCrypt_RelayItems_Element(string groupName)
    {
        groupName = groupName.Trim();
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath("RelayItems", 1)
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    private XElement? Get_AnonDNSCrypt_TargetItems_Element(string groupName)
    {
        groupName = groupName.Trim();
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath("TargetItems", 1)
        };

        List<XElement> elements = XmlTool.GetElements(XDoc, paths);
        if (elements.Count > 0) return elements[0];
        return null;
    }

    /// <summary>
    /// Add Group
    /// </summary>
    /// <returns>Returns Renamed Group Name</returns>
    public async Task<string> Add_Group_Async(XElement groupElement, bool addFirst, bool saveToFile = true)
    {
        try
        {
            if (XDoc.Root == null) return string.Empty;
            GroupItem groupItem = Get_GroupItem(groupElement);

            List<GroupItem> groupItems = Get_GroupItems(false);
            List<string> currentGroupNames = groupItems.Select(x => x.Name).ToList();

            // Rename
            int countName = 1;
            string groupName = groupItem.Name;
            while (currentGroupNames.IsContain(groupName))
            {
                bool hasNumber = false;
                if (groupName.EndsWith(')'))
                {
                    int firstIndex = groupName.LastIndexOf('(');
                    if (firstIndex != -1)
                    {
                        int lastIndex = groupName.LastIndexOf(')');
                        if (lastIndex != -1)
                        {
                            string numberStr = groupName.Substring(firstIndex + 1, groupName.Length - lastIndex);
                            bool isInt = int.TryParse(numberStr, out int number);
                            if (isInt)
                            {
                                hasNumber = true;
                                groupName = groupName.Replace($" ({number})", "");
                                groupName = string.Format("{0} ({1})", groupName, number + 1);
                            }
                        }
                    }
                }

                if (!hasNumber) groupName = string.Format("{0} ({1})", groupName, countName++);
            }

            if (!groupName.Equals(groupItem.Name))
            {
                XElement? nameElement = groupElement.Element(nameof(groupItem.Name));
                if (nameElement != null)
                {
                    nameElement.Value = groupName;
                }
            }
            
            // Add
            if (addFirst) XDoc.Root.AddFirst(groupElement);
            else XDoc.Root.Add(groupElement);

            if (saveToFile) await SaveAsync();
            return groupName;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Add_Group_Async: " + ex.Message);
            return string.Empty;
        }
    }

    public async Task<bool> Reset_BuiltIn_Groups_Async(bool saveToFile = true)
    {
        try
        {
            bool imported = false;
            XDocument? importedBuiltInXML = XDocument.Load(XmlFilePath_BultIn);
            if (importedBuiltInXML.Root != null)
            {
                var importedBuiltInGroupElements = importedBuiltInXML.Root.Elements().Reverse(); // Reverse Add To First
                if (importedBuiltInGroupElements.Any())
                {
                    // Remove Existing Built-In Groups
                    List<GroupItem> existingGroupItems = Get_GroupItems(false);
                    for (int n = 0; n < existingGroupItems.Count; n++)
                    {
                        GroupItem existingGroupItem = existingGroupItems[n];
                        if (existingGroupItem.BuiltIn)
                        {
                            await Remove_Group_Async(existingGroupItem.Name, false);
                        }
                    }

                    // Add Built-In Groups To First
                    foreach (XElement groupElement in importedBuiltInGroupElements)
                    {
                        await Add_Group_Async(groupElement, true, false);
                        imported = true;
                    }

                    if (imported)
                    {
                        if (saveToFile) await SaveAsync();
                        await Task.Delay(100);
                    }
                }
            }

            importedBuiltInXML = null;
            return imported;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Reset_BuiltIn_Groups_Async: " + ex.Message);
            return false;
        }
    }

    public async Task Rename_Group_Async(string oldName, string newName, bool saveToFile = true)
    {
        try
        {
            oldName = oldName.Trim();
            newName = newName.Trim();

            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, oldName) }),
                new XmlTool.XmlPath("Name", 1)
            };

            XDoc = XmlTool.UpdateElementsValue(XDoc, paths, -1, newName);
            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Rename_Group_Async: " + ex.Message);
        }
    }

    public async Task Move_Group_Async(string groupName, int toIndex, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 0, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) })
            };

            List<XElement> elements = XmlTool.GetElements(XDoc, paths);
            if (elements.Count > 0)
            {
                XElement xElement = elements[0];
                int fromIndex = xElement.ElementsBeforeSelf().Count();
                XDoc = XmlTool.UpdateElementsPosition(XDoc, paths, fromIndex, toIndex);
                if (saveToFile) await SaveAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Move_Group_Async: " + ex.Message);
        }
    }

    public void Move_Groups(List<int> fromIndexes, int toIndex) // Test
    {
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group")
        };

        XDoc = XmlTool.UpdateElementsPosition(XDoc, paths, fromIndexes, toIndex);
    }

    public async Task Remove_Group_Async(string groupName, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) })
            };

            XDoc = XmlTool.RemoveElements(XDoc, paths);
            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Remove_Group_Async: " + ex.Message);
        }
    }

    public bool Get_Source_EnableDisable(string groupName)
    {
        bool isEnable = false;

        try
        {
            groupName = groupName.Trim();
            XElement? sourceElement = Get_Source_Element(groupName);
            if (sourceElement != null && sourceElement.HasAttributes)
            {
                XAttribute? xAttribute = sourceElement.Attribute("Enabled");
                if (xAttribute != null)
                {
                    bool isBool = bool.TryParse(xAttribute.Value, out bool result);
                    if (isBool) isEnable = result;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Source_EnableDisable: " + ex.Message);
        }

        return isEnable;
    }

    public async Task Update_Source_EnableDisable_Async(string groupName, bool newValue, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();
            XElement? sourceElement = Get_Source_Element(groupName);
            if (sourceElement != null && sourceElement.HasAttributes)
            {
                XAttribute? xAttribute = sourceElement.Attribute("Enabled");
                xAttribute?.SetValue(newValue);
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Source_EnableDisable_Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Only Subscription And FragmentDoH
    /// </summary>
    public List<string> Get_Source_URLs(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            GroupMode groupMode = Get_GroupMode_ByName(groupName);
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
                new XmlTool.XmlPath($"{groupMode}", 1),
                new XmlTool.XmlPath("Source", 1),
                new XmlTool.XmlPath("URL")
            };

            List<XElement> elements = XmlTool.GetElements(XDoc, paths);
            return elements.Select(x => x.Value).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Source_URLs: " + ex.Message);
            return new List<string>();
        }
    }

    public List<string> Get_AnonDNSCrypt_Relay_URLs(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
                new XmlTool.XmlPath("AnonymizedDNSCrypt", 1),
                new XmlTool.XmlPath("Source", 1),
                new XmlTool.XmlPath("Relays", 1),
                new XmlTool.XmlPath("URL")
            };

            List<XElement> elements = XmlTool.GetElements(XDoc, paths);
            return elements.Select(x => x.Value).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_AnonDNSCrypt_Relay_URLs: " + ex.Message);
            return new List<string>();
        }
    }

    public List<string> Get_AnonDNSCrypt_Target_URLs(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
                new XmlTool.XmlPath("AnonymizedDNSCrypt", 1),
                new XmlTool.XmlPath("Source", 1),
                new XmlTool.XmlPath("Targets", 1),
                new XmlTool.XmlPath("URL")
            };

            List<XElement> elements = XmlTool.GetElements(XDoc, paths);
            return elements.Select(x => x.Value).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_AnonDNSCrypt_Target_URLs: " + ex.Message);
            return new List<string>();
        }
    }

    public List<string> Get_AnonDNSCrypt_Relays(string groupName)
    {
        List<string> relays = new();

        try
        {
            groupName = groupName.Trim();
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
                new XmlTool.XmlPath("RelayItems", 1),
                new XmlTool.XmlPath("URL")
            };

            List<XElement> elements = XmlTool.GetElements(XDoc, paths);
            for (int n = 0; n < elements.Count; n++)
            {
                XElement relayElement = elements[n];
                relays.Add(relayElement.Value);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_AnonDNSCrypt_Relays: " + ex.Message);
        }

        return relays;
    }

    public List<string> Get_AnonDNSCrypt_Targets(string groupName)
    {
        List<string> targets = new();

        try
        {
            groupName = groupName.Trim();
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
                new XmlTool.XmlPath("TargetItems", 1),
                new XmlTool.XmlPath("URL")
            };

            List<XElement> elements = XmlTool.GetElements(XDoc, paths);
            for (int n = 0; n < elements.Count; n++)
            {
                XElement targetElement = elements[n];
                targets.Add(targetElement.Value);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_AnonDNSCrypt_Targets: " + ex.Message);
        }

        return targets;
    }

    public async Task Update_Source_URLs_Async(string groupName, List<string> urls, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();
            GroupMode groupMode = Get_GroupMode_ByName(groupName);
            List<XmlTool.XmlPath> paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
                new XmlTool.XmlPath($"{groupMode}", 1),
                new XmlTool.XmlPath("Source", 1)
            };

            XDoc = XmlTool.RemoveChildElements(XDoc, paths);
            await Task.Delay(50);

            List<XElement> elements = XmlTool.GetElements(XDoc, paths);
            if (elements.Count > 0)
            {
                XElement subscriptionSourceElement = elements[0];
                for (int n = 0; n < urls.Count; n++)
                {
                    string url = urls[n];
                    url = url.Trim();
                    if (string.IsNullOrWhiteSpace(url)) continue;
                    if (!url.Contains("://") && !url.Contains(":\\")) continue; // Support URLs And Paths
                    subscriptionSourceElement.Add(new XElement("URL", url));
                }
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Source_URLs_Async: " + ex.Message);
        }
    }

    /// <summary>
    /// For Anonymized DNSCrypt
    /// </summary>
    public async Task Update_Source_URLs_Async(string groupName, List<string> relayURLs, List<string> targetURLs, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();
            // Relays
            List<XmlTool.XmlPath> relay_Paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
                new XmlTool.XmlPath("AnonymizedDNSCrypt", 1),
                new XmlTool.XmlPath("Source", 1),
                new XmlTool.XmlPath("Relays", 1)
            };

            XDoc = XmlTool.RemoveChildElements(XDoc, relay_Paths);
            await Task.Delay(50);

            List<XElement> relaysElements = XmlTool.GetElements(XDoc, relay_Paths);
            if (relaysElements.Count > 0)
            {
                XElement relaysElement = relaysElements[0];
                for (int n = 0; n < relayURLs.Count; n++)
                {
                    string url = relayURLs[n];
                    url = url.Trim();
                    if (string.IsNullOrWhiteSpace(url)) continue;
                    if (!url.Contains("://") && !url.Contains(":\\")) continue;
                    relaysElement.Add(new XElement("URL", url));
                }
            }

            // Targets
            List<XmlTool.XmlPath> target_Paths = new()
            {
                new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
                new XmlTool.XmlPath("AnonymizedDNSCrypt", 1),
                new XmlTool.XmlPath("Source", 1),
                new XmlTool.XmlPath("Targets", 1)
            };

            XDoc = XmlTool.RemoveChildElements(XDoc, target_Paths);
            await Task.Delay(50);

            List<XElement> targetsElements = XmlTool.GetElements(XDoc, target_Paths);
            if (targetsElements.Count > 0)
            {
                XElement targetsElement = targetsElements[0];
                for (int n = 0; n < targetURLs.Count; n++)
                {
                    string url = targetURLs[n];
                    url = url.Trim();
                    if (string.IsNullOrWhiteSpace(url)) continue;
                    if (!url.Contains("://") && !url.Contains(":\\")) continue;
                    targetsElement.Add(new XElement("URL", url));
                }
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_AnonDNSCrypt_URLs_Async: " + ex.Message);
        }
    }

    public GroupSettings Get_GroupSettings(string groupName)
    {
        groupName = groupName.Trim();
        XElement? groupSettingsElement = Get_GroupSettings_Element(groupName);
        return Create_GroupSettings(groupSettingsElement);
    }

    public async Task Update_GroupSettings_Async(string groupName, GroupSettings groupSettings, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();

            // Update GroupSettings
            XElement? groupSettingsElement = Get_GroupSettings_Element(groupName);
            groupSettingsElement?.ReplaceWith(Create_GroupSettings_Element(groupSettings));

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_GroupSettings_Async: " + ex.Message);
        }
    }

    public AutoUpdate Get_AutoUpdate(string groupName)
    {
        groupName = groupName.Trim();
        XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
        return Create_AutoUpdate(autoUpdateElement);
    }

    public LastAutoUpdate Get_LastAutoUpdate(string groupName)
    {
        groupName = groupName.Trim();
        XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
        return Create_LastAutoUpdate(autoUpdateElement);
    }

    public async Task Update_LastAutoUpdate_Async(string groupName, LastAutoUpdate dateTime, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();

            // Update AutoUpdate
            XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
            AutoUpdate autoUpdate = Create_AutoUpdate(autoUpdateElement);
            LastAutoUpdate lastAutoUpdate = Create_LastAutoUpdate(autoUpdateElement);
            if (dateTime.LastUpdateSource != DateTime.MinValue && dateTime.LastUpdateSource != default)
                lastAutoUpdate.LastUpdateSource = dateTime.LastUpdateSource;
            if (dateTime.LastScanServers != DateTime.MinValue && dateTime.LastScanServers != default)
                lastAutoUpdate.LastScanServers = dateTime.LastScanServers;
            autoUpdateElement?.ReplaceWith(Create_AutoUpdate_Element(autoUpdate, lastAutoUpdate));

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_LastAutoUpdate_Async: " + ex.Message);
        }
    }
    
    public SubscriptionOptions Get_Subscription_Options(string groupName)
    {
        try
        {
            groupName = groupName.Trim();

            // Get AutoUpdate
            XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
            AutoUpdate autoUpdate = Create_AutoUpdate(autoUpdateElement);

            // Get FilterByProtocols
            XElement? filterByProtocolsElement = Get_FilterByProtocols_Element(groupName);
            FilterByProtocols filterByProtocols = Create_FilterByProtocols(filterByProtocolsElement);

            // Get FilterByProperties
            XElement? filterByPropertiesElement = Get_FilterByProperties_Element(groupName);
            FilterByProperties filterByProperties = Create_FilterByProperties(filterByPropertiesElement);

            SubscriptionOptions options = new(autoUpdate, filterByProtocols, filterByProperties);
            return options;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Subscription_Options: " + ex.Message);
            return new SubscriptionOptions();
        }
    }

    public AnonDNSCryptOptions Get_AnonDNSCrypt_Options(string groupName)
    {
        try
        {
            groupName = groupName.Trim();

            // Get AutoUpdate
            XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
            AutoUpdate autoUpdate = Create_AutoUpdate(autoUpdateElement);

            // Get FilterByProperties
            XElement? filterByPropertiesElement = Get_FilterByProperties_Element(groupName);
            FilterByProperties filterByProperties = Create_FilterByProperties(filterByPropertiesElement);

            AnonDNSCryptOptions options = new(autoUpdate, filterByProperties);
            return options;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_AnonDNSCrypt_Options: " + ex.Message);
            return new AnonDNSCryptOptions();
        }
    }

    public FragmentDoHOptions Get_FragmentDoH_Options(string groupName)
    {
        try
        {
            groupName = groupName.Trim();

            // Get AutoUpdate
            XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
            AutoUpdate autoUpdate = Create_AutoUpdate(autoUpdateElement);

            // Get FilterByProperties
            XElement? filterByPropertiesElement = Get_FilterByProperties_Element(groupName);
            FilterByProperties filterByProperties = Create_FilterByProperties(filterByPropertiesElement);

            // Get Fragment Settings
            XElement? fragmentSettingsElement = Get_FragmentSettings_Element(groupName);
            FragmentSettings fragmentSettings = Create_FragmentSettings(fragmentSettingsElement);

            FragmentDoHOptions options = new(autoUpdate, filterByProperties, fragmentSettings);
            return options;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_FragmentDoH_Options: " + ex.Message);
            return new FragmentDoHOptions();
        }
    }

    public CustomOptions Get_Custom_Options(string groupName)
    {
        try
        {
            groupName = groupName.Trim();

            // Get AutoUpdate
            XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
            AutoUpdate autoUpdate = Create_AutoUpdate(autoUpdateElement);

            // Get FilterByProtocols
            XElement? filterByProtocolsElement = Get_FilterByProtocols_Element(groupName);
            FilterByProtocols filterByProtocols = Create_FilterByProtocols(filterByProtocolsElement);

            // Get FilterByProperties
            XElement? filterByPropertiesElement = Get_FilterByProperties_Element(groupName);
            FilterByProperties filterByProperties = Create_FilterByProperties(filterByPropertiesElement);

            CustomOptions options = new(autoUpdate, filterByProtocols, filterByProperties);
            return options;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Custom_Options: " + ex.Message);
            return new CustomOptions();
        }
    }

    public async Task Update_Subscription_Options_Async(string groupName, SubscriptionOptions options, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();

            // Update AutoUpdate
            XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
            LastAutoUpdate lastAutoUpdate = Create_LastAutoUpdate(autoUpdateElement);
            autoUpdateElement?.ReplaceWith(Create_AutoUpdate_Element(options.AutoUpdate, lastAutoUpdate));

            // Update FilterByProtocols
            XElement? filterByProtocolsElement = Get_FilterByProtocols_Element(groupName);
            filterByProtocolsElement?.ReplaceWith(Create_FilterByProtocols_Element(options.FilterByProtocols));

            // Update FilterByProperties
            XElement? filterByPropertiesElement = Get_FilterByProperties_Element(groupName);
            filterByPropertiesElement?.ReplaceWith(Create_FilterByProperties_Element(options.FilterByProperties));

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Subscription_Options_Async: " + ex.Message);
        }
    }

    public async Task Update_AnonDNSCrypt_Options_Async(string groupName, AnonDNSCryptOptions options, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();

            // Update AutoUpdate
            XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
            LastAutoUpdate lastAutoUpdate = Create_LastAutoUpdate(autoUpdateElement);
            autoUpdateElement?.ReplaceWith(Create_AutoUpdate_Element(options.AutoUpdate, lastAutoUpdate));

            // Update FilterByProperties
            XElement? filterByPropertiesElement = Get_FilterByProperties_Element(groupName);
            filterByPropertiesElement?.ReplaceWith(Create_FilterByProperties_Element(options.FilterByProperties));

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_AnonDNSCrypt_Options_Async: " + ex.Message);
        }
    }

    public async Task Update_FragmentDoH_Options_Async(string groupName, FragmentDoHOptions options, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();

            // Update AutoUpdate
            XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
            LastAutoUpdate lastAutoUpdate = Create_LastAutoUpdate(autoUpdateElement);
            autoUpdateElement?.ReplaceWith(Create_AutoUpdate_Element(options.AutoUpdate, lastAutoUpdate));

            // Update FilterByProperties
            XElement? filterByPropertiesElement = Get_FilterByProperties_Element(groupName);
            filterByPropertiesElement?.ReplaceWith(Create_FilterByProperties_Element(options.FilterByProperties));

            // Update Fragment Settings
            XElement? fragmentSettingsElement = Get_FragmentSettings_Element(groupName);
            fragmentSettingsElement?.ReplaceWith(Create_FragmentSettings_Element(options.FragmentSettings));

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_FragmentDoH_Options_Async: " + ex.Message);
        }
    }

    public async Task Update_Custom_Options_Async(string groupName, CustomOptions options, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();

            // Update AutoUpdate
            XElement? autoUpdateElement = Get_AutoUpdate_Element(groupName);
            LastAutoUpdate lastAutoUpdate = Create_LastAutoUpdate(autoUpdateElement);
            autoUpdateElement?.ReplaceWith(Create_AutoUpdate_Element(options.AutoUpdate, lastAutoUpdate));

            // Update FilterByProtocols
            XElement? filterByProtocolsElement = Get_FilterByProtocols_Element(groupName);
            filterByProtocolsElement?.ReplaceWith(Create_FilterByProtocols_Element(options.FilterByProtocols));

            // Update FilterByProperties
            XElement? filterByPropertiesElement = Get_FilterByProperties_Element(groupName);
            filterByPropertiesElement?.ReplaceWith(Create_FilterByProperties_Element(options.FilterByProperties));

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Custom_Options_Async: " + ex.Message);
        }
    }

    public List<XElement> Get_DnsItem_Elements(string groupName)
    {
        groupName = groupName.Trim();
        List<XmlTool.XmlPath> paths = new()
        {
            new XmlTool.XmlPath("Group", 1, new List<XmlTool.XmlChildCondition>() { new("Name", XmlNodeType.Text, groupName) }),
            new XmlTool.XmlPath("DnsItems", 1),
            new XmlTool.XmlPath("DnsItem")
        };

        List<XElement> dnsItemElements = XmlTool.GetElements(XDoc, paths);
        return dnsItemElements;
    }

    public List<DnsItem> Get_DnsItems(string groupName)
    {
        List<DnsItem> dnsItems = new();

        try
        {
            List<XElement> dnsItemElements = Get_DnsItem_Elements(groupName);
            for (int n = 0; n < dnsItemElements.Count; n++)
            {
                XElement dnsItemElement = dnsItemElements[n];
                DnsItem dnsItem = Create_DnsItem(dnsItemElement);
                dnsItems.Add(dnsItem);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_DnsItems: " + ex.Message);
        }

        return dnsItems;
    }

    /// <summary>
    /// Returns -1 If Not Found
    /// </summary>
    public int Get_IndexOf_DnsItem(string groupName, XElement dnsItemElement)
    {
        try
        {
            DnsItem selectedDnsItem = Create_DnsItem(dnsItemElement);
            List<XElement> dnsItemElements = Get_DnsItem_Elements(groupName);
            for (int n = 0; n < dnsItemElements.Count; n++)
            {
                XElement currentDnsItemElement = dnsItemElements[n];
                DnsItem dnsItem = Create_DnsItem(currentDnsItemElement);
                if (dnsItem.IDUnique == selectedDnsItem.IDUnique)
                {
                    return currentDnsItemElement.ElementsBeforeSelf().Count();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_IndexOf_DnsItem: " + ex.Message);
        }
        return -1;
    }

    /// <summary>
    /// Returns -1 If Not Found
    /// </summary>
    public int Get_IndexOf_DnsItem(string groupName, DnsItem dnsItem)
    {
        try
        {
            List<XElement> dnsItemElements = Get_DnsItem_Elements(groupName);
            for (int n = 0; n < dnsItemElements.Count; n++)
            {
                XElement currentDnsItemElement = dnsItemElements[n];
                DnsItem currentDnsItem = Create_DnsItem(currentDnsItemElement);
                if (currentDnsItem.IDUnique == dnsItem.IDUnique)
                {
                    return currentDnsItemElement.ElementsBeforeSelf().Count();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_IndexOf_DnsItem: " + ex.Message);
        }
        return -1;
    }

    private async Task Internal_DnsItems_AddAppend_Async(string groupName, List<string> dnss, bool clearFirst, bool saveToFile)
    {
        try
        {
            if (dnss.Count == 0) return;
            XElement? dnsItemsElement = Get_DnsItems_Element(groupName);
            if (dnsItemsElement == null) return;

            if (clearFirst)
            {
                dnsItemsElement.RemoveNodes();
                await Task.Delay(50);
            }

            // Convert To DnsItem Element
            List<DnsItem> dnsItems = Tools.Convert_DNSs_To_DnsItem(dnss);

            for (int n = 0; n < dnsItems.Count; n++)
            {
                DnsItem di = dnsItems[n];
                dnsItemsElement.Add(Create_DnsItem_Element(di));
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Internal_DnsItems_AddAppend_Async: " + ex.Message);
        }
    }

    private async Task Internal_DnsItems_AddAppend_Async(string groupName, List<XElement> dnsItemElementList, bool clearFirst, bool saveToFile)
    {
        try
        {
            if (dnsItemElementList.Count == 0) return;
            XElement? dnsItemsElement = Get_DnsItems_Element(groupName);
            if (dnsItemsElement == null) return;

            if (clearFirst)
            {
                dnsItemsElement.RemoveNodes();
                await Task.Delay(50);
            }

            // Add To Group => DnsItems Element
            dnsItemsElement.Add(dnsItemElementList);

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Internal_DnsItems_AddAppend_Async: " + ex.Message);
        }
    }

    private async Task Internal_DnsItems_AddAppend_Async(string groupName, List<DnsItem> dnsItemList, bool clearFirst, bool saveToFile)
    {
        try
        {
            if (dnsItemList.Count == 0) return;
            XElement? dnsItemsElement = Get_DnsItems_Element(groupName);
            if (dnsItemsElement == null) return;

            if (clearFirst)
            {
                dnsItemsElement.RemoveNodes();
                await Task.Delay(50);
            }

            // Add To Group => DnsItems Element
            for (int n = 0; n < dnsItemList.Count; n++)
            {
                DnsItem dnsItem = dnsItemList[n];
                dnsItemsElement.Add(Create_DnsItem_Element(dnsItem));
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Internal_DnsItems_AddAppend_Async: " + ex.Message);
        }
    }

    public async Task Append_DnsItems_Async(string groupName, List<string> dnss, bool saveToFile = true)
    {
        await Internal_DnsItems_AddAppend_Async(groupName, dnss, false, saveToFile);
    }

    public async Task Append_DnsItems_Async(string groupName, List<XElement> dnsItemElementList, bool saveToFile = true)
    {
        await Internal_DnsItems_AddAppend_Async(groupName, dnsItemElementList, false, saveToFile);
    }

    public async Task Append_DnsItems_Async(string groupName, List<DnsItem> dnsItemList, bool saveToFile = true)
    {
        await Internal_DnsItems_AddAppend_Async(groupName, dnsItemList, false, saveToFile);
    }

    /// <summary>
    /// Makes DnsItems Element Empty First.
    /// </summary>
    public async Task Add_DnsItems_Async(string groupName, List<string> dnss, bool saveToFile = true)
    {
        await Internal_DnsItems_AddAppend_Async(groupName, dnss, true, saveToFile);
    }

    /// <summary>
    /// Makes DnsItems Element Empty First.
    /// </summary>
    public async Task Add_DnsItems_Async(string groupName, List<XElement> dnsItemElementList, bool saveToFile = true)
    {
        await Internal_DnsItems_AddAppend_Async(groupName, dnsItemElementList, true, saveToFile);
    }

    /// <summary>
    /// Makes DnsItems Element Empty First.
    /// </summary>
    public async Task Add_DnsItems_Async(string groupName, List<DnsItem> dnsItemList, bool saveToFile = true)
    {
        await Internal_DnsItems_AddAppend_Async(groupName, dnsItemList, true, saveToFile);
    }

    public async Task Clear_AnonDNSCrypt_RelayItems_Async(string groupName, bool saveToFile = true)
    {
        try
        {
            XElement? relayItemsElement = Get_AnonDNSCrypt_RelayItems_Element(groupName);
            if (relayItemsElement == null) return;

            relayItemsElement.RemoveNodes();
            await Task.Delay(50);

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Clear_AnonDNSCrypt_RelayItems_Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Makes RelayItems Element Empty First.
    /// </summary>
    public async Task Add_AnonDNSCrypt_RelayItems_Async(string groupName, List<string> dnss, bool saveToFile = true)
    {
        try
        {
            // Convert To RelayItem Element
            List<XElement> relayItemElements = new();
            for (int n = 0; n < dnss.Count; n++)
            {
                string dns = dnss[n];
                DnsReader dnsReader = new(dns);
                if (dnsReader.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCryptRelay ||
                    dnsReader.Protocol == DnsEnums.DnsProtocol.UDP ||
                    dnsReader.Protocol == DnsEnums.DnsProtocol.TCP)
                {
                    relayItemElements.Add(new XElement("URL", dnsReader.Dns));
                }
            }

            if (relayItemElements.Count > 0)
            {
                XElement? relayItemsElement = Get_AnonDNSCrypt_RelayItems_Element(groupName);
                if (relayItemsElement == null) return;
                
                relayItemsElement.RemoveNodes();
                await Task.Delay(50);

                // Add To Group => RelayItems Element
                relayItemsElement.Add(relayItemElements);

                if (saveToFile) await SaveAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Add_AnonDNSCrypt_RelayItems_Async: " + ex.Message);
        }
    }

    public async Task Clear_AnonDNSCrypt_TargetItems_Async(string groupName, bool saveToFile = true)
    {
        try
        {
            XElement? targetItemsElement = Get_AnonDNSCrypt_TargetItems_Element(groupName);
            if (targetItemsElement == null) return;

            targetItemsElement.RemoveNodes();
            await Task.Delay(50);

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Clear_AnonDNSCrypt_TargetItems_Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Makes TargetItems Element Empty First.
    /// </summary>
    public async Task Add_AnonDNSCrypt_TargetItems_Async(string groupName, List<string> dnss, bool saveToFile = true)
    {
        try
        {
            // Convert To TargetItem Element
            List<XElement> targetItemElements = new();
            for (int n = 0; n < dnss.Count; n++)
            {
                string dns = dnss[n];
                DnsReader dnsReader = new(dns);
                if (dnsReader.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                    targetItemElements.Add(new XElement("URL", dnsReader.Dns));
            }

            if (targetItemElements.Count > 0)
            {
                XElement? targetItemsElement = Get_AnonDNSCrypt_TargetItems_Element(groupName);
                if (targetItemsElement == null) return;

                targetItemsElement.RemoveNodes();
                await Task.Delay(50);

                // Add To Group => TargetItems Element
                targetItemsElement.Add(targetItemElements);

                if (saveToFile) await SaveAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Add_AnonDNSCrypt_TargetItems_Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Makes DnsItems Element Empty.
    /// </summary>
    public async Task Clear_DnsItems_Async(string groupName, bool saveToFile = true)
    {
        try
        {
            XElement? dnsItemsElement = Get_DnsItems_Element(groupName);
            if (dnsItemsElement == null) return;

            dnsItemsElement.RemoveNodes();
            await Task.Delay(50);

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Clear_DnsItems_Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Clear DnsItems Result
    /// </summary>
    public async Task Clear_DnsItems_Result_Async(string groupName, bool clearDescription, bool saveToFile = true)
    {
        try
        {
            List<XElement> dnsItemElements = Get_DnsItem_Elements(groupName);

            for (int n = 0; n < dnsItemElements.Count; n++)
            {
                XElement dnsItemElement = dnsItemElements[n];
                DnsItem dnsItem = Create_DnsItem(dnsItemElement);
                dnsItem.Enabled = false;
                dnsItem.Status = DnsStatus.Unknown;
                dnsItem.Latency = -1;
                dnsItem.IsGoogleSafeSearchEnabled = DnsFilter.Unknown;
                dnsItem.IsBingSafeSearchEnabled = DnsFilter.Unknown;
                dnsItem.IsYoutubeRestricted = DnsFilter.Unknown;
                dnsItem.IsAdultBlocked = DnsFilter.Unknown;
                if (clearDescription) dnsItem.Description = string.Empty;
                dnsItemElement.ReplaceWith(Create_DnsItem_Element(dnsItem));
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Clear_DnsItems_Result_Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Update DnsItem Elements By Unique ID
    /// </summary>
    public async Task Update_DnsItems_Async(string groupName, List<XElement> dnsItemElements, bool saveToFile = true)
    {
        try
        {
            List<XElement> dnsItemElements_Current = Get_DnsItem_Elements(groupName);

            for (int n1 = 0; n1 < dnsItemElements.Count; n1++)
            {
                XElement dnsItemElement = dnsItemElements[n1];
                DnsItem dnsItem = Create_DnsItem(dnsItemElement);
                for (int n2 = 0; n2 < dnsItemElements_Current.Count; n2++)
                {
                    XElement dnsItemElement_Current = dnsItemElements_Current[n2];
                    DnsItem dnsItem_Current = Create_DnsItem(dnsItemElement_Current);
                    if (dnsItem.IDUnique.Equals(dnsItem_Current.IDUnique))
                    {
                        dnsItemElement_Current.ReplaceWith(dnsItemElement);
                        break;
                    }
                }
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_DnsItems_Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Update DnsItems By Unique ID
    /// </summary>
    public async Task Update_DnsItems_Async(string groupName, List<DnsItem> dnsItems, bool saveToFile = true)
    {
        try
        {
            List<XElement> dnsItemElements_Current = Get_DnsItem_Elements(groupName);

            for (int n1 = 0; n1 < dnsItems.Count; n1++)
            {
                DnsItem dnsItem = dnsItems[n1];
                for (int n2 = 0; n2 < dnsItemElements_Current.Count; n2++)
                {
                    XElement dnsItemElement_Current = dnsItemElements_Current[n2];
                    DnsItem dnsItem_Current = Create_DnsItem(dnsItemElement_Current);
                    if (dnsItem.IDUnique.Equals(dnsItem_Current.IDUnique))
                    {
                        dnsItemElement_Current.ReplaceWith(Create_DnsItem_Element(dnsItem));
                        break;
                    }
                }
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_DnsItems_Async: " + ex.Message);
        }
    }

    /// <summary>
    /// Remove DnsItems By Unique ID
    /// </summary>
    public async Task Remove_DnsItems_Async(string groupName, List<DnsItem> dnsItems, bool saveToFile = true)
    {
        try
        {
            List<XElement> dnsItemElements_Current = Get_DnsItem_Elements(groupName);

            for (int n1 = 0; n1 < dnsItems.Count; n1++)
            {
                DnsItem dnsItem = dnsItems[n1];
                for (int n2 = 0; n2 < dnsItemElements_Current.Count; n2++)
                {
                    XElement dnsItemElement_Current = dnsItemElements_Current[n2];
                    DnsItem dnsItem_Current = Create_DnsItem(dnsItemElement_Current);
                    if (dnsItem.IDUnique.Equals(dnsItem_Current.IDUnique))
                    {
                        dnsItemElement_Current.Remove();
                        break;
                    }
                }
            }

            if (saveToFile) await SaveAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Remove_DnsItems_Async: " + ex.Message);
        }
    }

    public async Task Sort_DnsItems_ByLatency_Async(string groupName, bool saveToFile = true)
    {
        try
        {
            XElement? dnsItemsElement = Get_DnsItems_Element(groupName);
            if (dnsItemsElement == null) return;

            List<DnsItem> dnsItemList = Get_DnsItems(groupName);

            // Sort By Latency - Positive Numbers First
            List<DnsItem> positive = dnsItemList.Where(x => x.Latency >= 0).OrderBy(x => x.Latency).ToList();
            List<DnsItem> negative = dnsItemList.Where(x => x.Latency < 0).OrderBy(x => x.Latency).ToList();
            dnsItemList = positive.Concat(negative).ToList();

            await Add_DnsItems_Async(groupName, dnsItemList, saveToFile);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Sort_DnsItems_ByLatency_Async: " + ex.Message);
        }
    }

    public static bool IsDnsItemEnabledByProtocols(DnsItem dnsItem, FilterByProtocols filterByProtocols)
    {
        DnsEnums.DnsProtocol protocol = DnsEnums.GetDnsProtocolByName(dnsItem.Protocol);

        return ((protocol == DnsEnums.DnsProtocol.UDP && filterByProtocols.UDP) ||
                (protocol == DnsEnums.DnsProtocol.TCP && filterByProtocols.TCP) ||
                (protocol == DnsEnums.DnsProtocol.DnsCrypt && filterByProtocols.DnsCrypt) ||
                (protocol == DnsEnums.DnsProtocol.DoT && filterByProtocols.DoT) ||
                (protocol == DnsEnums.DnsProtocol.DoH && filterByProtocols.DoH) ||
                (protocol == DnsEnums.DnsProtocol.DoQ && filterByProtocols.DoQ) ||
                (protocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt && filterByProtocols.AnonymizedDNSCrypt) ||
                (protocol == DnsEnums.DnsProtocol.ObliviousDoH && filterByProtocols.ObliviousDoH));
    }

    public static bool IsDnsItemEnabledByProperties(DnsItem dnsItem, FilterByProperties filterByProperties)
    {
        bool matchGoogle = ((filterByProperties.GoogleSafeSearch == DnsFilter.Yes && dnsItem.IsGoogleSafeSearchEnabled == DnsFilter.Yes) ||
                            (filterByProperties.GoogleSafeSearch == DnsFilter.No && dnsItem.IsGoogleSafeSearchEnabled == DnsFilter.No) ||
                            filterByProperties.GoogleSafeSearch == DnsFilter.Unknown);

        bool matchBing = ((filterByProperties.BingSafeSearch == DnsFilter.Yes && dnsItem.IsBingSafeSearchEnabled == DnsFilter.Yes) ||
                          (filterByProperties.BingSafeSearch == DnsFilter.No && dnsItem.IsBingSafeSearchEnabled == DnsFilter.No) ||
                          filterByProperties.BingSafeSearch == DnsFilter.Unknown);

        bool matchYoutube = ((filterByProperties.YoutubeRestricted == DnsFilter.Yes && dnsItem.IsYoutubeRestricted == DnsFilter.Yes) ||
                             (filterByProperties.YoutubeRestricted == DnsFilter.No && dnsItem.IsYoutubeRestricted == DnsFilter.No) ||
                             filterByProperties.YoutubeRestricted == DnsFilter.Unknown);

        bool matchAdult = ((filterByProperties.AdultBlocked == DnsFilter.Yes && dnsItem.IsAdultBlocked == DnsFilter.Yes) ||
                           (filterByProperties.AdultBlocked == DnsFilter.No && dnsItem.IsAdultBlocked == DnsFilter.No) ||
                           filterByProperties.AdultBlocked == DnsFilter.Unknown);

        return matchGoogle && matchBing && matchYoutube && matchAdult;
    }

    public static bool IsDnsItemEnabledByOptions(DnsItem dnsItem, FilterByProtocols filterByProtocols, FilterByProperties filterByProperties)
    {
        if (dnsItem.Status != DnsStatus.Online) return false;
        bool matchProtocol = IsDnsItemEnabledByProtocols(dnsItem, filterByProtocols);
        bool matchProperties = IsDnsItemEnabledByProperties(dnsItem, filterByProperties);
        return matchProtocol && matchProperties;
    }

    public static bool IsDnsItemEnabledByOptions(DnsItem dnsItem, FilterByProperties filterByProperties)
    {
        if (dnsItem.Status != DnsStatus.Online) return false;
        bool matchProperties = IsDnsItemEnabledByProperties(dnsItem, filterByProperties);
        return matchProperties;
    }

    public async Task Select_DnsItems_ByOptions_Async(string groupName, FilterByProtocols filterByProtocols, FilterByProperties filterByProperties, bool saveToFile = true)
    {
        try
        {
            XElement? dnsItemsElement = Get_DnsItems_Element(groupName);
            if (dnsItemsElement == null) return;

            List<DnsItem> dnsItemList = new();
            List<XElement> dnsItemElements = Get_DnsItem_Elements(groupName);
            for (int n = 0; n < dnsItemElements.Count; n++)
            {
                XElement dnsItemElement = dnsItemElements[n];
                DnsItem dnsItem = Create_DnsItem(dnsItemElement);

                // Select By Options
                dnsItem.Enabled = IsDnsItemEnabledByOptions(dnsItem, filterByProtocols, filterByProperties);

                dnsItemList.Add(dnsItem);
            }

            await Add_DnsItems_Async(groupName, dnsItemList, saveToFile);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Select_DnsItems_ByOptions_Async: " + ex.Message);
        }
    }

    public async Task Select_DnsItems_ByOptions_Async(string groupName, FilterByProperties filterByProperties, bool saveToFile = true)
    {
        try
        {
            XElement? dnsItemsElement = Get_DnsItems_Element(groupName);
            if (dnsItemsElement == null) return;

            List<DnsItem> dnsItemList = new();
            List<XElement> dnsItemElements = Get_DnsItem_Elements(groupName);
            for (int n = 0; n < dnsItemElements.Count; n++)
            {
                XElement dnsItemElement = dnsItemElements[n];
                DnsItem dnsItem = Create_DnsItem(dnsItemElement);

                // Select By Options
                dnsItem.Enabled = IsDnsItemEnabledByOptions(dnsItem, filterByProperties);

                dnsItemList.Add(dnsItem);
            }

            await Add_DnsItems_Async(groupName, dnsItemList, saveToFile);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Select_DnsItems_ByOptions_Async: " + ex.Message);
        }
    }

    public DnsItemsInfo Get_DnsItems_Info(string groupName)
    {
        DnsItemsInfo info = new();

        try
        {
            int sumLatency = 0;
            List<DnsItem> dnsItems = Get_DnsItems(groupName);
            for (int n = 0; n < dnsItems.Count; n++)
            {
                DnsItem dnsItem = dnsItems[n];
                info.TotalServers++;
                if (dnsItem.Status == DnsStatus.Online)
                {
                    info.OnlineServers++;
                    if (dnsItem.Enabled)
                    {
                        info.SelectedServers++;
                        sumLatency += dnsItem.Latency;
                    }
                }
            }
            if (info.SelectedServers > 0) info.AverageLatency = sumLatency / info.SelectedServers;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_DnsItems_Info: " + ex.Message);
        }

        return info;
    }

    public async Task DeDup_DnsItems_Async(string groupName, bool saveToFile = true)
    {
        try
        {
            groupName = groupName.Trim();
            GroupMode groupMode = Get_GroupMode_ByName(groupName);
            List<DnsItem> dnsItemList = Get_DnsItems(groupName);

            // DeDup By DNS And Protocol
            dnsItemList = dnsItemList.DistinctByProperties(x => new { x.DNS_URL, x.Protocol });

            if (groupMode == GroupMode.FragmentDoH)
            {
                // Remove DoHs Without IP
                dnsItemList.RemoveAll(x => x.DNS_IP.Equals(IPAddress.None));
                dnsItemList.RemoveAll(x => x.DNS_IP.Equals(IPAddress.IPv6None));
            }

            await Add_DnsItems_Async(groupName, dnsItemList, saveToFile);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager DeDup_DnsItems_Async: " + ex.Message);
        }
    }

}