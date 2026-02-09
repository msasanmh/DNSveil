using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Microsoft.CodeAnalysis;
using MsmhToolsClass;
using MsmhToolsClass.V2RayConfigTool;
using MsmhToolsWpfClass;
using static DNSveil.Logic.UpstreamServers.UpstreamModel;

namespace DNSveil.Logic.UpstreamServers;

public partial class UpstreamServersManager
{
    public UpstreamModel Model { get; set; } = new();
    private string DocFilePath_BuiltIn = string.Empty;
    private string DocFilePath = string.Empty;
    private string DocFilePath_Backup => Path.GetFullPath(DocFilePath.Replace(".json", "_Backup.json"));
    private bool IsInitialized = false;
    private Window? Owner = null;
    private bool PauseBackgroundTask => UI_Window.IsWindowOpen<Window>();

    public bool IsBackgroundTaskWorking { get; private set; } = false;
    public Window? UI_Window { get; set; } = null;
    public readonly RegionCache RegionCaches = new();

    // WPF: ObservableCollection<T>      WinUI: ObservableVector<T>

    public UpstreamServersManager()
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
            UpstreamModel? model = await JsonTool.DeserializeAsync<UpstreamModel>(json);
            if (model != null)
            {
                Model = model;
                json = string.Empty;
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServersManager LoadAsync: " + ex.Message);
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
            WpfMessageBox.Show(Owner, ex.Message, "ERROR Initialize Upstream Manager", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return IsInitialized;
    }

    public async Task<bool> Update_Settings_Async(UpstreamSettings settings, bool SaveToFile)
    {
        try
        {
            Model.Settings = settings;
            if (SaveToFile) await SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServersManager Update_Settings_Async: " + ex.Message);
            return false;
        }
    }

    public List<UpstreamGroup> Get_Groups(bool getOnlyEnabledGroups)
    {
        List<UpstreamGroup> groups = new();

        try
        {
            if (getOnlyEnabledGroups)
            {
                for (int n = 0; n < Model.Groups.Count; n++)
                {
                    UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Get_Groups: " + ex.Message);
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
            Debug.WriteLine("UpstreamServersManager Get_Group_Names: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName)) return group.Mode;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServersManager Get_GroupMode_ByName: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Update_Group_As_BuiltIn_Async: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Add Group
    /// </summary>
    /// <returns>Returns Renamed Group Name</returns>
    public async Task<string> Add_Group_Async(UpstreamGroup group, bool addFirst, bool saveToFile)
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
            Debug.WriteLine("UpstreamServersManager Add_Group_Async: " + ex.Message);
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
                    UpstreamModel? model = await JsonTool.DeserializeAsync<UpstreamModel>(json);
                    if (model != null)
                    {
                        // Check Model Has Built-In Groups
                        bool hasBuiltInGroups = false;
                        foreach (UpstreamGroup group in model.Groups)
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
                                    UpstreamGroup existingGroup = Model.Groups[n];
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
                                UpstreamGroup group = model.Groups[n];
                                if (group.IsBuiltIn)
                                {
                                    // Rename: If Existing Group Name Is Equals To Built-In group Name
                                    foreach (UpstreamGroup existingGroup in Model.Groups)
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
            Debug.WriteLine("UpstreamServersManager Reset_BuiltIn_Groups_Async: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Rename_Group_Async: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Remove_Group_Async: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Move_Group_Async: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    return group.Settings;
                }
            }
            return new GroupSettings();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServersManager Get_GroupSettings: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Get_GroupSettings: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                    {
                        return group.Subscription.Source.IsEnabled;
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
            Debug.WriteLine("UpstreamServersManager Get_Source_EnableDisable: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                    {
                        group.Subscription.Source.IsEnabled = newValue;
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
            Debug.WriteLine("UpstreamServersManager Update_Source_EnableDisable_Async: " + ex.Message);
            return false;
        }
    }

    public List<string> Get_Source_URLs(string groupName)
    {
        List<string> urls = new();

        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                UpstreamGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null)
                    {
                        urls.AddRange(group.Subscription.Source.URLs);
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
            Debug.WriteLine("UpstreamServersManager Get_Source_URLs: " + ex.Message);
        }

        return urls;
    }

    public async Task<bool> Update_Source_URLs_Async(string groupName, List<string> urls, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Update_Source_URLs_Async: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Subscription && group.Subscription != null) return group.Subscription.Options;
                }
            }
            return new SubscriptionOptions();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServersManager Get_Subscription_Options: " + ex.Message);
            return new SubscriptionOptions();
        }
    }

    public CustomOptions Get_Custom_Options(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                UpstreamGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    if (group.Mode == GroupMode.Custom && group.Custom != null) return group.Custom.Options;
                }
            }
            return new CustomOptions();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServersManager Get_Custom_Options: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Update_Subscription_Options_Async: " + ex.Message);
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
                UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Update_Custom_Options_Async: " + ex.Message);
            return false;
        }
    }

    public ObservableCollection<UpstreamItem> Get_UpstreamItems(string groupName)
    {
        ObservableCollection<UpstreamItem> items = new();

        try
        {
            groupName = groupName.Trim();
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                UpstreamGroup group = Model.Groups[n];
                if (group.Name.Equals(groupName))
                {
                    items.AddRange(group.Items);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServersManager Get_UpstreamItems: " + ex.Message);
        }

        return items;
    }

    /// <summary>
    /// Returns -1 If Not Found
    /// </summary>
    public int Get_IndexOf_UpstreamItem(string groupName, UpstreamItem item)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        UpstreamItem currentItem = group.Items[j];
                        if (currentItem.IDUniqueWithRemarks == item.IDUniqueWithRemarks)
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
            Debug.WriteLine("UpstreamServersManager Get_UpstreamItems: " + ex.Message);
            return -1;
        }
    }

    private async Task<bool> Internal_UpstreamItems_AddAppend_Async(string groupName, ObservableCollection<string> urlOrJsonList, bool clearFirst, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    if (clearFirst) group.Items.Clear();

                    List<UpstreamItem> items = new();
                    for (int j = 0; j < urlOrJsonList.Count; j++)
                    {
                        string urlOrJson = urlOrJsonList[j];
                        UpstreamItem item = new(urlOrJson);
                        if (item.ConfigInfo.IsSuccess) items.Add(item);
                    }

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
            Debug.WriteLine("UpstreamServersManager Internal_UpstreamItems_AddAppend_Async: " + ex.Message);
            return false;
        }
    }

    private async Task<bool> Internal_UpstreamItems_AddAppend_Async(string groupName, ObservableCollection<UpstreamItem> items, bool clearFirst, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
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
            Debug.WriteLine("UpstreamServersManager Internal_UpstreamItems_AddAppend_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Append_UpstreamItems_Async(string groupName, ObservableCollection<string> urlOrJsonList, bool saveToFile)
    {
        return await Internal_UpstreamItems_AddAppend_Async(groupName, urlOrJsonList, false, saveToFile);
    }

    public async Task<bool> Append_UpstreamItems_Async(string groupName, ObservableCollection<UpstreamItem> items, bool saveToFile)
    {
        return await Internal_UpstreamItems_AddAppend_Async(groupName, items, false, saveToFile);
    }

    public async Task<bool> Add_UpstreamItems_Async(string groupName, ObservableCollection<string> urlOrJsonList, bool saveToFile)
    {
        return await Internal_UpstreamItems_AddAppend_Async(groupName, urlOrJsonList, true, saveToFile);
    }

    public async Task<bool> Add_UpstreamItems_Async(string groupName, ObservableCollection<UpstreamItem> items, bool saveToFile)
    {
        return await Internal_UpstreamItems_AddAppend_Async(groupName, items, true, saveToFile);
    }

    public async Task<bool> Clear_UpstreamItems_Async(string groupName, bool saveToFile)
    {
        try
        {
            for (int n = 0; n < Model.Groups.Count; n++)
            {
                UpstreamGroup group = Model.Groups[n];
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
            Debug.WriteLine("UpstreamServersManager Clear_UpstreamItems_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Clear_UpstreamItems_Result_Async(string groupName, bool clearDescription, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        UpstreamItem item = group.Items[j];
                        item.IsSelected = false;
                        item.Latency = -1;
                        item.StabilityPercent = 0;
                        item.DLSpeed = 0;
                        item.ULSpeed = 0;
                        item.StatusCode = HttpStatusCode.RequestTimeout; // 408
                        item.StatusCodeNumber = 408;
                        item.StatusDescription = string.Empty;
                        item.StatusShortDescription = "Unknown";
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
            Debug.WriteLine("UpstreamServersManager Clear_UpstreamItems_Result_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Update_UpstreamItems_Async(string groupName, ObservableCollection<UpstreamItem> items, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        UpstreamItem item = items[j];
                        for (int k = 0; k < group.Items.Count; k++)
                        {
                            UpstreamItem currentItem = group.Items[k];
                            if (currentItem.IDUniqueWithRemarks == item.IDUniqueWithRemarks)
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
            Debug.WriteLine("UpstreamServersManager Update_UpstreamItems_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Update_UpstreamItems_Async(string groupName, string currentIDUniqueWithRemarks, UpstreamItem newItem, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        UpstreamItem currentItem = group.Items[j];
                        if (currentItem.IDUniqueWithRemarks == currentIDUniqueWithRemarks)
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
            Debug.WriteLine("UpstreamServersManager Update_UpstreamItems_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Remove_UpstreamItems_Async(string groupName, List<UpstreamItem> items, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        UpstreamItem item = items[j];
                        for (int k = 0; k < group.Items.Count; k++)
                        {
                            UpstreamItem currentItem = group.Items[k];
                            if (currentItem.IDUniqueWithRemarks == item.IDUniqueWithRemarks)
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
            Debug.WriteLine("UpstreamServersManager Remove_UpstreamItems_Async: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Sort_UpstreamItems_Async(string groupName, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    // Sort By Stability Then Latency
                    //List<UpstreamItem> withStability = group.Items.Where(_ => _.StabilityPercent > 0).OrderByDescending(_ => _.StabilityPercent).ThenBy(_ => _.Latency).ToList();
                    //List<UpstreamItem> withNoStability = group.Items.Where(_ => _.StabilityPercent <= 0).ToList();
                    //List<UpstreamItem> sortedList = new();
                    //sortedList.AddRange(withStability);
                    //sortedList.AddRange(withNoStability);

                    List<UpstreamItem> sortedList = group.Items.OrderByDescending(_ => _.DLSpeed).ThenByDescending(_ => _.ULSpeed).ThenByDescending(_ => _.StabilityPercent).ThenBy(_ => _.Latency).ToList();

                    group.Items.Clear();
                    group.Items.AddRange(sortedList);
                    sortedList.Clear();

                    if (saveToFile) await SaveAsync();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServersManager Sort_UpstreamItems_Async: " + ex.Message);
            return false;
        }
    }

    public static bool IsUpstreamItemSelectedBySettings(UpstreamItem item, GroupSettings settings)
    {
        bool skipBySettings = settings.DontUseServersWithNoSecurity && item.ConfigInfo.Security == ConfigBuilder.Security.None;
        return !skipBySettings;
    }

    public async Task<bool> Select_UpstreamItems_Async(string groupName, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        UpstreamItem item = group.Items[j];
                        bool isSelectedBySettings = IsUpstreamItemSelectedBySettings(item, group.Settings);
                        bool isSelected = isSelectedBySettings && item.StatusCode == HttpStatusCode.OK;
                        item.IsSelected = isSelected;
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
            Debug.WriteLine("UpstreamServersManager Select_UpstreamItems_Async: " + ex.Message);
            return false;
        }
    }

    public UpstreamItemsInfo Get_UpstreamItems_Info(string groupName)
    {
        UpstreamItemsInfo info = new();

        try
        {
            int sumLatency = 0;
            ObservableCollection<UpstreamItem> items = Get_UpstreamItems(groupName);
            for (int n = 0; n < items.Count; n++)
            {
                UpstreamItem item = items[n];
                info.TotalServers++;
                if (item.StatusCode == HttpStatusCode.OK)
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
            Debug.WriteLine("UpstreamServersManager Get_UpstreamItems_Info: " + ex.Message);
        }

        return info;
    }

    public async Task<bool> DeDup_UpstreamItems_Async(string groupName, bool saveToFile)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    // DeDup By IDUnique
                    ObservableCollection<UpstreamItem> items = group.Items.DistinctBy(_ => _.ConfigInfo.IDUnique).ToObservableCollection();

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
            Debug.WriteLine("UpstreamServersManager DeDup_UpstreamItems_Async: " + ex.Message);
            return false;
        }
    }

    public bool TEMP(string groupName)
    {
        try
        {
            groupName = groupName.Trim();
            for (int i = 0; i < Model.Groups.Count; i++)
            {
                UpstreamGroup group = Model.Groups[i];
                if (group.Name.Equals(groupName))
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        UpstreamItem item = group.Items[j];
                        UpstreamItem item2 = new(item.ConfigInfo.UrlOrJson)
                        {
                            Latency = item.Latency,
                            StabilityPercent = item.StabilityPercent,
                            StatusCode = item.StatusCode,
                            StatusCodeNumber = item.StatusCodeNumber,
                            StatusDescription = item.StatusDescription,
                            StatusShortDescription = item.StatusShortDescription,
                            Region = item.Region
                        };
                        group.Items[j] = item2;
                        //Debug.WriteLine(item2.ConfigInfo.UrlOrJson);
                        //Debug.WriteLine(item2.ConfigInfo.Protocol);
                    }

                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("UpstreamServersManager TEMP: " + ex.GetInnerExceptions());
            return false;
        }
    }

}