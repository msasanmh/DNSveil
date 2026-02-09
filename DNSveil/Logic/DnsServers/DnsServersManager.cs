using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using MsmhToolsWpfClass;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using static DNSveil.Logic.DnsServers.DnsModel;

namespace DNSveil.Logic.DnsServers;

public partial class DnsServersManager
{
    public DnsModel Model { get; set; } = new();
    private string DocFilePath_BuiltIn = string.Empty;
    private string DocFilePath = string.Empty;
    private string DocFilePath_Backup => Path.GetFullPath(DocFilePath.Replace(".json", "_Backup.json"));
    private bool IsInitialized = false;
    private Window? Owner = null;
    private bool PauseBackgroundTask => UI_Window.IsWindowOpen<Window>();

    public bool IsBackgroundTaskWorking { get; private set; } = false;
    public Window? UI_Window { get; set; } = null;

    // WPF: ObservableCollection<T>      WinUI: ObservableVector<T>

    public DnsServersManager()
    {
        BackgroundWorker_AutoUpdate_SettingsAndGroups();
    }

    public async Task<bool> SaveAsync()
    {
        string json = await JsonTool.SerializeAsync(Model, false);
        if (!string.IsNullOrEmpty(json) && JsonTool.IsValid(json)) return await FileDirectory.WriteAllTextAsync(DocFilePath, json, new UTF8Encoding(false));
        return false;
    }

    public async Task<bool> SaveToAsync(string jsonFilePath)
    {
        string json = await JsonTool.SerializeAsync(Model, false);
        if (!string.IsNullOrEmpty(json) && JsonTool.IsValid(json)) return await FileDirectory.WriteAllTextAsync(jsonFilePath, json, new UTF8Encoding(false));
        return false;
    }

    public async Task<bool> LoadAsync()
    {
        try
        {
            string json = await File.ReadAllTextAsync(DocFilePath);
            DnsModel? model = await JsonTool.DeserializeAsync<DnsModel>(json);
            if (model != null)
            {
                Model = model;
                json = string.Empty;
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager LoadAsync: " + ex.Message);
        }
        return false;
    }

    public async Task<bool> InitializeAsync(string builtInServers, string userServers, Window? window = null)
    {
        Owner = window;
        IsInitialized = false;
        bool isSaved;

        try
        {
            DocFilePath_BuiltIn = builtInServers;
            DocFilePath = userServers;

            if (!File.Exists(DocFilePath))
            {
                bool isBuiltInValid = await JsonTool.IsValidFileAsync(DocFilePath_BuiltIn);
                if (!isBuiltInValid)
                {
                    File.Create(DocFilePath).Dispose();
                    isSaved = await SaveAsync();
                }
                else
                {
                    try
                    {
                        await File.WriteAllBytesAsync(DocFilePath, await File.ReadAllBytesAsync(DocFilePath_BuiltIn));
                        isSaved = true;
                    }
                    catch (Exception)
                    {
                        isSaved = false;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(DocFilePath)))
            {
                bool isBuiltInValid = await JsonTool.IsValidFileAsync(DocFilePath_BuiltIn);
                if (!isBuiltInValid)
                {
                    isSaved = await SaveAsync();
                }
                else
                {
                    try
                    {
                        await File.WriteAllBytesAsync(DocFilePath, await File.ReadAllBytesAsync(DocFilePath_BuiltIn));
                        isSaved = true;
                    }
                    catch (Exception)
                    {
                        isSaved = false;
                    }
                }
            }
            else if (!await JsonTool.IsValidFileAsync(DocFilePath))
            {
                if (await JsonTool.IsValidFileAsync(DocFilePath_Backup))
                {
                    try
                    {
                        await File.WriteAllBytesAsync(DocFilePath, await File.ReadAllBytesAsync(DocFilePath_Backup));
                        isSaved = true;

                        string msg = "JSON File Is Not Valid. Backup Restored.";
                        WpfMessageBox.Show(Owner, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception)
                    {
                        isSaved = false;

                        string msg = "JSON File Is Not Valid. Couldn't Restore Backup!";
                        WpfMessageBox.Show(Owner, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    bool isBuiltInValid = await JsonTool.IsValidFileAsync(DocFilePath_BuiltIn);
                    if (!isBuiltInValid)
                    {
                        isSaved = await SaveAsync();

                        if (isSaved)
                        {
                            string msg = "JSON File Is Not Valid. Returned To Default.";
                            WpfMessageBox.Show(Owner, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            string msg = "JSON File Is Not Valid. Couldn't Restore Default!";
                            WpfMessageBox.Show(Owner, msg, "Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        try
                        {
                            await File.WriteAllBytesAsync(DocFilePath, await File.ReadAllBytesAsync(DocFilePath_BuiltIn));
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
                bool isLoaded = await LoadAsync();
                if (isLoaded)
                {
                    IsInitialized = true;
                }
            }
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(Owner, ex.Message, "ERROR Initialize DNS Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return IsInitialized;
    }

    public DnsSettings Get_Settings() { return Model.Settings; }

    public async Task<bool> Update_Settings_Async(DnsSettings settings, bool SaveToFile)
    {
        try
        {
            Model.Settings = settings;
            if (SaveToFile) await SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Settings_Async: " + ex.Message);
            return false;
        }
    }

    public List<DnsGroup> Get_Groups(bool getOnlyEnabledGroups)
    {
        List<DnsGroup> groups = new();

        try
        {
            if (getOnlyEnabledGroups)
            {
                for (int n = 0; n < Model.Groups.Count; n++)
                {
                    DnsGroup group = Model.Groups[n];
                    if (group.Settings.IsEnabled) groups.Add(group);
                }
            }
            else
            {
                groups.AddRange(Model.Groups);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Groups: " + ex.Message);
        }

        return groups;
    }

    public List<string> Get_Group_Names(bool getOnlyEnabledGroups)
    {
        try
        {
            return Get_Groups(getOnlyEnabledGroups).Select(_ => _.Name).ToList();
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
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName)) return group.Mode;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_GroupMode_ByName: " + ex.Message);
        }

        return GroupMode.None;
    }

    /// <summary>
    /// Set As BuiltIn Or User
    /// </summary>
    /// <param name="groupName">Group Name</param>
    /// <param name="setAsBuiltIn">BuiltIn: True, User: False</param>
    public async Task<bool> Update_Group_As_BuiltIn_Async(string groupName, bool setAsBuiltIn, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    group.IsBuiltIn = setAsBuiltIn;
                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Group_As_BuiltIn_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Add Group
    /// </summary>
    /// <returns>Returns Renamed Group Name</returns>
    public async Task<string> Add_Group_Async(DnsGroup group, bool addFirst, bool saveToFile)
    {
        try
        {
            // No Name
            group.Name = group.Name.Trim();
            if (string.IsNullOrEmpty(group.Name)) group.Name = "No_Name";

            // Rename
            List<string> currentGroupNames = Get_Group_Names(false);
            group.Name = group.Name.Rename(currentGroupNames);

            // Add
            if (addFirst) Model.Groups.Insert(0, group);
            else Model.Groups.Add(group);

            if (saveToFile) await SaveAsync();
            return group.Name;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Add_Group_Async: " + ex.Message);
            return string.Empty;
        }
    }

    public async Task<bool> Reset_BuiltIn_Groups_Async(bool saveToFile)
    {
        try
        {
            bool imported = false;
            if (File.Exists(DocFilePath_BuiltIn))
            {
                string json = await File.ReadAllTextAsync(DocFilePath_BuiltIn, new UTF8Encoding(false));
                bool isJsonValid = JsonTool.IsValid(json);
                if (isJsonValid)
                {
                    // Create Model
                    DnsModel? model = await JsonTool.DeserializeAsync<DnsModel>(json);
                    if (model != null)
                    {
                        // Check Model Has Built-In Groups
                        bool hasBuiltInGroups = false;
                        foreach (DnsGroup group in model.Groups)
                        {
                            if (group.IsBuiltIn)
                            {
                                hasBuiltInGroups = true;
                                break;
                            }
                        }

                        if (hasBuiltInGroups)
                        {
                            // Remove Existing Built-In Groups
                            while (true)
                            {
                                bool removed = false;
                                for (int n = 0; n < Model.Groups.Count; n++)
                                {
                                    DnsGroup existingGroup = Model.Groups[n];
                                    if (existingGroup.IsBuiltIn)
                                    {
                                        removed = await Remove_Group_Async(existingGroup.Name, false);
                                        break;
                                    }
                                }

                                if (!removed) break;
                            }

                            // Add Built-In Groups To First
                            List<string> builtInGroupNames = model.Groups.Select(_ => _.Name).ToList();
                            for (int n = model.Groups.Count - 1; n >= 0; n--) // Reverse Add To First
                            {
                                DnsGroup group = model.Groups[n];
                                if (group.IsBuiltIn)
                                {
                                    // Rename: If Existing Group Name Is Equals To Built-In group Name
                                    foreach (DnsGroup existingGroup in Model.Groups)
                                    {
                                        if (existingGroup.Name.Equals(group.Name))
                                        {
                                            existingGroup.Name = existingGroup.Name.Rename(builtInGroupNames);
                                            break;
                                        }
                                    }

                                    // Add To First
                                    Model.Groups.Insert(0, group);
                                    imported = true;
                                }
                            }

                            if (imported)
                            {
                                if (saveToFile) await SaveAsync();
                            }
                        }

                        // Dispose
                        model.Groups.Clear();
                    }
                }

                // Dispose
                json = string.Empty;
            }
            return imported;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Reset_BuiltIn_Groups_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Rename_Group_Async(string oldName, string newName, bool saveToFile)
    {
        try
        {
            oldName = oldName.Trim();
            newName = newName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(oldName))
                {
                    group.Name = newName;
                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Rename_Group_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Remove_Group_Async(string groupName, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    Model.Groups.RemoveAt(n);
                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Remove_Group_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Move_Group_Async(string groupName, int toIndex, bool saveToFile)
    {
        try
        {
            int fromIndex = -1;
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    fromIndex = n;
                }
            }

            if (fromIndex != -1)
            {
                Model.Groups.MoveTo(fromIndex, toIndex);
                if (saveToFile) await SaveAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Move_Group_Async: " + ex.Message);
            return false;
        }
    }

    public GroupSettings Get_GroupSettings(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    return group.Settings;
                }
            }
            return new GroupSettings();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_GroupSettings: " + ex.Message);
            return new GroupSettings();
        }
    }

    public async Task<bool> Update_GroupSettings_Async(string groupName, GroupSettings groupSettings, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    group.Settings = groupSettings;
                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_GroupSettings: " + ex.Message);
            return false;
        }
    }

    public bool Get_Source_EnableDisable(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                    {
                        return group.Subscription.Source.IsEnabled;
                    }
                    else if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        return group.AnonymizedDNSCrypt.Source.IsEnabled;
                    }
                    else if (group.Mode == GroupMode.FragmentDoH && group.FragmentDoH != null)
                    {
                        return group.FragmentDoH.Source.IsEnabled;
                    }
                    else if (group.Mode == GroupMode.Custom && group.Custom != null)
                    {
                        return group.Custom.Source.IsEnabled;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Source_EnableDisable: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Update_Source_EnableDisable_Async(string groupName, bool newValue, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                    {
                        group.Subscription.Source.IsEnabled = newValue;
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                    else if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        group.AnonymizedDNSCrypt.Source.IsEnabled = newValue;
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                    else if (group.Mode == GroupMode.FragmentDoH && group.FragmentDoH != null)
                    {
                        group.FragmentDoH.Source.IsEnabled = newValue;
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                    else if (group.Mode == GroupMode.Custom && group.Custom != null)
                    {
                        group.Custom.Source.IsEnabled = newValue;
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Source_EnableDisable_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Only Subscription, FragmentDoH, Custom
    /// </summary>
    public List<string> Get_Source_URLs(string groupName)
    {
        List<string> urls = new();

        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                    {
                        urls.AddRange(group.Subscription.Source.URLs);
                        break;
                    }
                    else if (group.Mode == GroupMode.FragmentDoH && group.FragmentDoH != null)
                    {
                        urls.AddRange(group.FragmentDoH.Source.URLs);
                        break;
                    }
                    else if (group.Mode == GroupMode.Custom && group.Custom != null)
                    {
                        urls.AddRange(group.Custom.Source.URLs);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Source_URLs: " + ex.Message);
        }

        return urls;
    }

    public List<string> Get_AnonDNSCrypt_Relay_URLs(string groupName)
    {
        List<string> relays = new();

        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        relays.AddRange(group.AnonymizedDNSCrypt.Source.Relay_URLs);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_AnonDNSCrypt_Relay_URLs: " + ex.Message);
        }

        return relays;
    }

    public List<string> Get_AnonDNSCrypt_Target_URLs(string groupName)
    {
        List<string> targets = new();

        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        targets.AddRange(group.AnonymizedDNSCrypt.Source.Target_URLs);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_AnonDNSCrypt_Target_URLs: " + ex.Message);
        }

        return targets;
    }

    public List<string> Get_AnonDNSCrypt_Relays(string groupName)
    {
        List<string> relays = new();

        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        relays.AddRange(group.AnonymizedDNSCrypt.Relays);
                        break;
                    }
                }
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
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        targets.AddRange(group.AnonymizedDNSCrypt.Targets);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_AnonDNSCrypt_Targets: " + ex.Message);
        }

        return targets;
    }

    /// <summary>
    /// Only Subscription, FragmentDoH, Custom
    /// </summary>
    public async Task<bool> Update_Source_URLs_Async(string groupName, List<string> urls, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                    {
                        group.Subscription.Source.URLs.Clear();
                        for (int i = 0; i < urls.Count; i++)
                        {
                            string url = urls[i].Trim();
                            if (string.IsNullOrEmpty(url)) continue;
                            if (!url.Contains("://") && !url.Contains(":\\")) continue; // Support URLs And Paths
                            group.Subscription.Source.URLs.Add(url);
                        }
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                    else if (group.Mode == GroupMode.FragmentDoH && group.FragmentDoH != null)
                    {
                        group.FragmentDoH.Source.URLs.Clear();
                        for (int i = 0; i < urls.Count; i++)
                        {
                            string url = urls[i].Trim();
                            if (string.IsNullOrEmpty(url)) continue;
                            if (!url.Contains("://") && !url.Contains(":\\")) continue; // Support URLs And Paths
                            group.FragmentDoH.Source.URLs.Add(url);
                        }
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                    else if (group.Mode == GroupMode.Custom && group.Custom != null)
                    {
                        group.Custom.Source.URLs.Clear();
                        for (int i = 0; i < urls.Count; i++)
                        {
                            string url = urls[i].Trim();
                            if (string.IsNullOrEmpty(url)) continue;
                            if (!url.Contains("://") && !url.Contains(":\\")) continue; // Support URLs And Paths
                            group.Custom.Source.URLs.Add(url);
                        }
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Source_URLs_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// For Anonymized DNSCrypt
    /// </summary>
    public async Task<bool> Update_Source_URLs_Async(string groupName, List<string> relayURLs, List<string> targetURLs, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        // Relay URLs
                        group.AnonymizedDNSCrypt.Source.Relay_URLs.Clear();
                        for (int i = 0; i < relayURLs.Count; i++)
                        {
                            string relayURL = relayURLs[i].Trim();
                            if (string.IsNullOrEmpty(relayURL)) continue;
                            if (!relayURL.Contains("://") && !relayURL.Contains(":\\")) continue; // Support URLs And Paths
                            group.AnonymizedDNSCrypt.Source.Relay_URLs.Add(relayURL);
                        }

                        // Target URLs
                        group.AnonymizedDNSCrypt.Source.Target_URLs.Clear();
                        for (int i = 0; i < targetURLs.Count; i++)
                        {
                            string targetURL = targetURLs[i].Trim();
                            if (string.IsNullOrEmpty(targetURL)) continue;
                            if (!targetURL.Contains("://") && !targetURL.Contains(":\\")) continue; // Support URLs And Paths
                            group.AnonymizedDNSCrypt.Source.Target_URLs.Add(targetURL);
                        }

                        // Save - Return
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Source_URLs_Async: " + ex.Message);
            return false;
        }
    }

    public SubscriptionOptions Get_Subscription_Options(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null) return group.Subscription.Options;
                }
            }
            return new SubscriptionOptions();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Subscription_Options: " + ex.Message);
            return new SubscriptionOptions();
        }
    }

    public AnonymizedDNSCryptOptions Get_AnonDNSCrypt_Options(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null) return group.AnonymizedDNSCrypt.Options;
                }
            }
            return new AnonymizedDNSCryptOptions();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_AnonDNSCrypt_Options: " + ex.Message);
            return new AnonymizedDNSCryptOptions();
        }
    }

    public FragmentDoHOptions Get_FragmentDoH_Options(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.FragmentDoH && group.FragmentDoH != null) return group.FragmentDoH.Options;
                }
            }
            return new FragmentDoHOptions();
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
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Custom && group.Custom != null) return group.Custom.Options;
                }
            }
            return new CustomOptions();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_Custom_Options: " + ex.Message);
            return new CustomOptions();
        }
    }

    public async Task<bool> Update_Subscription_Options_Async(string groupName, SubscriptionOptions options, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                    {
                        group.Subscription.Options = options;
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Subscription_Options_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Update_AnonDNSCrypt_Options_Async(string groupName, AnonymizedDNSCryptOptions options, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        group.AnonymizedDNSCrypt.Options = options;
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_AnonDNSCrypt_Options_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Update_FragmentDoH_Options_Async(string groupName, FragmentDoHOptions options, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.FragmentDoH && group.FragmentDoH != null)
                    {
                        group.FragmentDoH.Options = options;
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_FragmentDoH_Options_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Update_Custom_Options_Async(string groupName, CustomOptions options, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Custom && group.Custom != null)
                    {
                        group.Custom.Options = options;
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_Custom_Options_Async: " + ex.Message);
            return false;
        }
    }

    public ObservableCollection<DnsItem> Get_DnsItems(string groupName)
    {
        ObservableCollection<DnsItem> items = new();

        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    items.AddRange(group.Items);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_DnsItems: " + ex.Message);
        }

        return items;
    }

    /// <summary>
    /// Returns -1 If Not Found
    /// </summary>
    public int Get_IndexOf_DnsItem(string groupName, DnsItem item)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        DnsItem currentItem = group.Items[j];
                        if (currentItem.IDUnique == item.IDUnique)
                        {
                            return j;
                        }
                    }
                }
            }
            return -1;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_IndexOf_DnsItem: " + ex.Message);
            return -1;
        }
    }

    private async Task<bool> Internal_DnsItems_AddAppend_Async(string groupName, ObservableCollection<string> dnss, bool clearFirst, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    if (clearFirst) group.Items.Clear();

                    // Convert To DnsItem
                    ObservableCollection<DnsItem> items = Tools.Convert_DNSs_To_DnsItem(dnss);

                    group.Items.AddRange(items);
                    items.Clear();

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Internal_DnsItems_AddAppend_Async: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> Internal_DnsItems_AddAppend_Async(string groupName, ObservableCollection<DnsItem> items, bool clearFirst, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    if (clearFirst) group.Items.Clear();
                    group.Items.AddRange(items);
                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Internal_DnsItems_AddAppend_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Append_DnsItems_Async(string groupName, ObservableCollection<string> dnss, bool saveToFile)
    {
        return await Internal_DnsItems_AddAppend_Async(groupName, dnss, false, saveToFile);
    }

    public async Task<bool> Append_DnsItems_Async(string groupName, ObservableCollection<DnsItem> items, bool saveToFile)
    {
        return await Internal_DnsItems_AddAppend_Async(groupName, items, false, saveToFile);
    }

    public async Task<bool> Add_DnsItems_Async(string groupName, ObservableCollection<string> dnss, bool saveToFile)
    {
        return await Internal_DnsItems_AddAppend_Async(groupName, dnss, true, saveToFile);
    }

    public async Task<bool> Add_DnsItems_Async(string groupName, ObservableCollection<DnsItem> items, bool saveToFile)
    {
        return await Internal_DnsItems_AddAppend_Async(groupName, items, true, saveToFile);
    }

    public async Task<bool> Clear_AnonDNSCrypt_RelayItems_Async(string groupName, bool saveToFile)
    {
        try
        {
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        group.AnonymizedDNSCrypt.Relays.Clear();
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Clear_AnonDNSCrypt_RelayItems_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Makes RelayItems Empty First.
    /// </summary>
    public async Task<bool> Add_AnonDNSCrypt_RelayItems_Async(string groupName, List<string> dnss, bool saveToFile)
    {
        try
        {
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        List<string> relays = new();
                        for (int j = 0; j < dnss.Count; j++)
                        {
                            string dns = dnss[j];
                            DnsReader dnsReader = new(dns);
                            if (dnsReader.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCryptRelay ||
                                dnsReader.Protocol == DnsEnums.DnsProtocol.UDP ||
                                dnsReader.Protocol == DnsEnums.DnsProtocol.TCP ||
                                dnsReader.Protocol == DnsEnums.DnsProtocol.TcpOverUdp)
                            {
                                relays.Add(dnsReader.Dns);
                            }
                        }

                        if (relays.Count > 0)
                        {
                            group.AnonymizedDNSCrypt.Relays.Clear();
                            group.AnonymizedDNSCrypt.Relays.AddRange(relays);
                            if (saveToFile) await SaveAsync();
                            relays.Clear();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Add_AnonDNSCrypt_RelayItems_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Clear_AnonDNSCrypt_TargetItems_Async(string groupName, bool saveToFile)
    {
        try
        {
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        group.AnonymizedDNSCrypt.Targets.Clear();
                        if (saveToFile) await SaveAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Clear_AnonDNSCrypt_TargetItems_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Makes TargetItems Empty First.
    /// </summary>
    public async Task<bool> Add_AnonDNSCrypt_TargetItems_Async(string groupName, List<string> dnss, bool saveToFile)
    {
        try
        {
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        List<string> targets = new();
                        for (int j = 0; j < dnss.Count; j++)
                        {
                            string dns = dnss[j];
                            DnsReader dnsReader = new(dns);
                            if (dnsReader.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                            {
                                targets.Add(dnsReader.Dns);
                            }
                        }

                        if (targets.Count > 0)
                        {
                            group.AnonymizedDNSCrypt.Targets.Clear();
                            group.AnonymizedDNSCrypt.Targets.AddRange(targets);
                            if (saveToFile) await SaveAsync();
                            targets.Clear();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Add_AnonDNSCrypt_TargetItems_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Makes DnsItems Empty.
    /// </summary>
    public async Task<bool> Clear_DnsItems_Async(string groupName, bool saveToFile)
    {
        try
        {
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                DnsGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    group.Items.Clear();
                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Clear_DnsItems_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Clear DnsItems Result
    /// </summary>
    public async Task<bool> Clear_DnsItems_Result_Async(string groupName, bool clearDescription, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        DnsItem item = group.Items[j];
                        item.IsSelected = false;
                        item.Status = DnsStatus.Unknown;
                        item.Latency = -1;
                        item.IsGoogleSafeSearchEnabled = DnsFilter.Unknown;
                        item.IsBingSafeSearchEnabled = DnsFilter.Unknown;
                        item.IsYoutubeRestricted = DnsFilter.Unknown;
                        item.IsAdultBlocked = DnsFilter.Unknown;
                        if (clearDescription) item.Description = string.Empty;
                        group.Items[j] = item;
                        group.Items[j].NotifyPropertyChanged();
                    }

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Clear_DnsItems_Result_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Update DnsItems By Unique ID
    /// </summary>
    public async Task<bool> Update_DnsItems_Async(string groupName, ObservableCollection<DnsItem> items, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        DnsItem item = items[j];
                        for (int k = 0; k < group.Items.Count; k++)
                        {
                            DnsItem currentItem = group.Items[k];
                            if (currentItem.IDUnique == item.IDUnique)
                            {
                                currentItem = item;
                                group.Items[k] = currentItem;
                                group.Items[k].NotifyPropertyChanged();
                                break;
                            }
                        }
                    }

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_DnsItems_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Update_DnsItems_Async(string groupName, string currentIDUnique, DnsItem newItem, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        DnsItem currentItem = group.Items[j];
                        if (currentItem.IDUnique == currentIDUnique)
                        {
                            currentItem = newItem;
                            group.Items[j] = currentItem;
                            group.Items[j].NotifyPropertyChanged();
                            break;
                        }
                    }

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Update_DnsItems_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Remove DnsItems By Unique ID
    /// </summary>
    public async Task<bool> Remove_DnsItems_Async(string groupName, List<DnsItem> items, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        DnsItem item = items[j];
                        for (int k = 0; k < group.Items.Count; k++)
                        {
                            DnsItem currentItem = group.Items[k];
                            if (currentItem.IDUnique == item.IDUnique)
                            {
                                group.Items.RemoveAt(k);
                                break;
                            }
                        }
                    }

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Remove_DnsItems_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Sort_DnsItems_Async(string groupName, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    // Sort By Latency - Positive Numbers First
                    List<DnsItem> positive = group.Items.Where(_ => _.Latency >= 0).OrderBy(_ => _.Latency).ToList();
                    List<DnsItem> negative = group.Items.Where(_ => _.Latency < 0).OrderBy(_ => _.Latency).ToList();
                    List<DnsItem> sortedList = new(positive);
                    sortedList.AddRange(negative);

                    group.Items.Clear();
                    group.Items.AddRange(sortedList);
                    sortedList.Clear();
                    negative.Clear();
                    positive.Clear();

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Sort_DnsItems_Async: " + ex.Message);
            return false;
        }
    }

    public static bool IsDnsItemEnabledByProtocols(DnsItem item, FilterByProtocols filterByProtocols)
    {
        return ((item.Protocol == DnsEnums.DnsProtocol.UDP && filterByProtocols.UDP) ||
                (item.Protocol == DnsEnums.DnsProtocol.TCP && filterByProtocols.TCP) ||
                (item.Protocol == DnsEnums.DnsProtocol.TcpOverUdp && filterByProtocols.TcpOverUdp) ||
                (item.Protocol == DnsEnums.DnsProtocol.DnsCrypt && filterByProtocols.DnsCrypt) ||
                (item.Protocol == DnsEnums.DnsProtocol.DoT && filterByProtocols.DoT) ||
                (item.Protocol == DnsEnums.DnsProtocol.DoH && filterByProtocols.DoH) ||
                (item.Protocol == DnsEnums.DnsProtocol.DoQ && filterByProtocols.DoQ) ||
                (item.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt && filterByProtocols.AnonymizedDNSCrypt) ||
                (item.Protocol == DnsEnums.DnsProtocol.ObliviousDoH && filterByProtocols.ObliviousDoH));
    }

    public static bool IsDnsItemEnabledByProperties(DnsItem item, FilterByProperties filterByProperties)
    {
        bool matchGoogle = ((filterByProperties.GoogleSafeSearch == DnsFilter.Yes && item.IsGoogleSafeSearchEnabled == DnsFilter.Yes) ||
                            (filterByProperties.GoogleSafeSearch == DnsFilter.No && item.IsGoogleSafeSearchEnabled == DnsFilter.No) ||
                            filterByProperties.GoogleSafeSearch == DnsFilter.Unknown);

        bool matchBing = ((filterByProperties.BingSafeSearch == DnsFilter.Yes && item.IsBingSafeSearchEnabled == DnsFilter.Yes) ||
                          (filterByProperties.BingSafeSearch == DnsFilter.No && item.IsBingSafeSearchEnabled == DnsFilter.No) ||
                          filterByProperties.BingSafeSearch == DnsFilter.Unknown);

        bool matchYoutube = ((filterByProperties.YoutubeRestricted == DnsFilter.Yes && item.IsYoutubeRestricted == DnsFilter.Yes) ||
                             (filterByProperties.YoutubeRestricted == DnsFilter.No && item.IsYoutubeRestricted == DnsFilter.No) ||
                             filterByProperties.YoutubeRestricted == DnsFilter.Unknown);

        bool matchAdult = ((filterByProperties.AdultBlocked == DnsFilter.Yes && item.IsAdultBlocked == DnsFilter.Yes) ||
                           (filterByProperties.AdultBlocked == DnsFilter.No && item.IsAdultBlocked == DnsFilter.No) ||
                           filterByProperties.AdultBlocked == DnsFilter.Unknown);

        return matchGoogle && matchBing && matchYoutube && matchAdult;
    }

    public static bool IsDnsItemEnabledByOptions(DnsItem item, FilterByProtocols filterByProtocols, FilterByProperties filterByProperties)
    {
        if (item.Status != DnsStatus.Online) return false;
        bool matchProtocol = IsDnsItemEnabledByProtocols(item, filterByProtocols);
        bool matchProperties = IsDnsItemEnabledByProperties(item, filterByProperties);
        return matchProtocol && matchProperties;
    }

    public static bool IsDnsItemEnabledByOptions(DnsItem item, FilterByProperties filterByProperties)
    {
        if (item.Status != DnsStatus.Online) return false;
        bool matchProperties = IsDnsItemEnabledByProperties(item, filterByProperties);
        return matchProperties;
    }

    public async Task<bool> Select_DnsItems_ByOptions_Async(string groupName, FilterByProtocols filterByProtocols, FilterByProperties filterByProperties, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        DnsItem item = group.Items[j];
                        // Select By Options
                        item.IsSelected = IsDnsItemEnabledByOptions(item, filterByProtocols, filterByProperties);
                        group.Items[j] = item;
                        group.Items[j].NotifyPropertyChanged();
                    }

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Select_DnsItems_ByOptions_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Select_DnsItems_ByOptions_Async(string groupName, FilterByProperties filterByProperties, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        DnsItem item = group.Items[j];
                        // Select By Options
                        item.IsSelected = IsDnsItemEnabledByOptions(item, filterByProperties);
                        group.Items[j] = item;
                        group.Items[j].NotifyPropertyChanged();
                    }

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Select_DnsItems_ByOptions_Async: " + ex.Message);
            return false;
        }
    }

    public DnsItemsInfo Get_DnsItems_Info(string groupName)
    {
        DnsItemsInfo info = new();

        try
        {
            int sumLatency = 0;
            ObservableCollection<DnsItem> items = Get_DnsItems(groupName);
            for (int n = 0; n < items.Count; n++)
            {
                DnsItem item = items[n];
                info.TotalServers++;
                if (item.Status == DnsStatus.Online)
                {
                    info.OnlineServers++;
                    if (item.IsSelected)
                    {
                        info.SelectedServers++;
                        sumLatency += item.Latency;
                    }
                }
            }
            if (info.SelectedServers > 0) info.AverageLatency = sumLatency / info.SelectedServers;

            // Dispose
            items.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager Get_DnsItems_Info: " + ex.Message);
        }

        return info;
    }

    public async Task<bool> DeDup_DnsItems_Async(string groupName, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                DnsGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    List<DnsItem> items;
                    if (group.Mode == GroupMode.AnonymizedDNSCrypt && group.AnonymizedDNSCrypt != null)
                    {
                        // DeDup Relays And Targets
                        group.AnonymizedDNSCrypt.Relays = group.AnonymizedDNSCrypt.Relays.Distinct().ToList();
                        group.AnonymizedDNSCrypt.Targets = group.AnonymizedDNSCrypt.Targets.Distinct().ToList();
                        items = Tools.Create_DnsItem_ForAnonDNSCrypt(group.AnonymizedDNSCrypt.Targets, group.AnonymizedDNSCrypt.Relays).ToList();
                    }
                    else if (group.Mode == GroupMode.FragmentDoH)
                    {
                        // DeDup By DNS And IP
                        items = group.Items.DistinctByProperties(_ => new { _.DNS_URL, _.DNS_IP_Str }).ToList();

                        // Remove DoHs Without IP
                        items.RemoveAll(_ => _.DNS_IP.Equals(IPAddress.None));
                        items.RemoveAll(_ => _.DNS_IP.Equals(IPAddress.IPv6None));
                    }
                    else
                    {
                        // DeDup By DNS And Protocol
                        items = group.Items.DistinctByProperties(_ => new { _.DNS_URL, _.ProtocolName }).ToList();
                    }

                    group.Items.Clear();
                    group.Items.AddRange(items);
                    items.Clear();

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DnsServersManager DeDup_DnsItems_Async: " + ex.Message);
            return false;
        }
    }

}