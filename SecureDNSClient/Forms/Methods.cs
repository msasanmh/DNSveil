using CustomControls;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using MsmhToolsClass;
using MsmhToolsClass.ProxyServerPrograms;
using MsmhToolsWinFormsClass.Themes;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Task = System.Threading.Tasks.Task;

namespace SecureDNSClient;

public partial class FormMain
{
    public async void GetAppReady()
    {
        await Task.Run(async () =>
        {
            IsAppReady = false;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Waiting for Network...{NL}", Color.Gray));
            while (true)
            {
                await Task.Delay(1000);
                IsAppReady = IsInternetOnline = await IsInternetAlive(false, true);
                if (IsAppReady) break;
            }
            string msgReady = $"Network Detected (Up Time: {ConvertTool.TimeSpanToHumanRead(AppUpTime.Elapsed, true)}){NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgReady, Color.Gray));
        });
    }

    public async void StartupTask()
    {
        string msgStartup = $"Startup Task Executed (Up Time: {ConvertTool.TimeSpanToHumanRead(AppUpTime.Elapsed, true)}){NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgStartup, Color.Orange));

        Hide();
        Opacity = 0;
        bool cancel = false;

        // Wait for Startup Delay
        Task wait = Task.Run(async () =>
        {
            while (true)
            {
                if (IsCheckingStarted || IsConnecting || IsQuickConnecting || IsExiting)
                {
                    cancel = true;
                    break;
                }
                await Task.Delay(100);
            }
        });
        if (Program.StartupDelaySec > 0)
            try { await wait.WaitAsync(TimeSpan.FromSeconds(Program.StartupDelaySec)); } catch (Exception) { }

        if (cancel) return;

        // Wait Until App Is Ready
        Task waitNet = Task.Run(async () =>
        {
            while (true)
            {
                if (IsAppReady) break;
                if (IsCheckingStarted || IsConnecting || IsQuickConnecting || IsExiting)
                {
                    cancel = true;
                    break;
                }
                await Task.Delay(500);
            }
        });
        try { await waitNet.WaitAsync(CancellationToken.None); } catch (Exception) { }

        if (cancel) return;

        // Update NICs
        SecureDNS.UpdateNICs(CustomComboBoxNICs, false, out _);

        // Start Quick Connect
        await StartQuickConnect(null);
    }

    public static bool IsAppOnWindowsStartup(out bool isPathOk)
    {
        isPathOk = false;
        bool isTaskExist = false;

        try
        {
            string taskName = "SecureDnsClientStartup";
            string appPath = $"\"{Path.GetFullPath(Application.ExecutablePath)}\"";
            using TaskService ts = new();
            TaskCollection tasks = ts.RootFolder.GetTasks(new System.Text.RegularExpressions.Regex(taskName));
            for (int n = 0; n < tasks.Count; n++)
            {
                Microsoft.Win32.TaskScheduler.Task task = tasks[n];
                if (task.Name.Equals(taskName))
                {
                    task.Enabled = true;
                    TaskDefinition td = task.Definition;
                    ActionCollection actions = td.Actions;
                    for (int n2 = 0; n2 < actions.Count; n2++)
                    {
                        Microsoft.Win32.TaskScheduler.Action action = actions[n2];
                        string existStr = action.ToString(System.Globalization.CultureInfo.InvariantCulture);

                        if (!string.IsNullOrEmpty(existStr))
                        {
                            if (existStr.Contains("\" "))
                                existStr = $"{existStr.Split("\" ")[0]}\"";
                            if (existStr.Equals(appPath) && td.Principal.RunLevel == TaskRunLevel.Highest) isPathOk = true;
                        }
                    }

                    isTaskExist = true;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("IsAppOnWindowsStartup: " + ex.Message);
        }

        return isTaskExist;
    }

    public static void ActivateWindowsStartup(bool active)
    {
        try
        {
            int startupDelay = 0;
            string taskName = "SecureDnsClientStartup";
            string appPath = $"\"{Path.GetFullPath(Application.ExecutablePath)}\"";
            string appArgs = $"startup {startupDelay}";
            using TaskService ts = new();
            TaskDefinition td = ts.NewTask();
            td.RegistrationInfo.Description = "Secure DNS Client Startup";
            td.Triggers.Add(new LogonTrigger()); // Trigger at Logon
            td.Actions.Add(appPath, appArgs);
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Settings.Enabled = active;
            td.Settings.AllowDemandStart = true;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.Hidden = false; // Don't Hide App
            if (active)
                ts.RootFolder.RegisterTaskDefinition(taskName, td); // Add Task
            else
                ts.RootFolder.DeleteTask(taskName); // Remove Task
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ActivateWindowsStartup: " + ex.Message);
        }
    }

    public static bool IsAppOnWindowsStartupRegistry(out bool isPathOk)
    {
        isPathOk = false;
        string appName = "SecureDnsClient";
        string prefix = "cmd /c start \"SDC\" /b ";
        string appPath = $"\"{Path.GetFullPath(Application.ExecutablePath)}\"";
        string regPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        RegistryKey? registry = Registry.CurrentUser.OpenSubKey(regPath, false);
        if (registry == null) return false;
        object? exist = registry.GetValue(appName, "false");
        if (exist == null) return false;
        if (exist.Equals("false")) return false;
        string? existStr = exist as string;
        if (!string.IsNullOrEmpty(existStr))
        {
            if (existStr.StartsWith(prefix)) existStr = existStr[prefix.Length..];
            if (existStr.Contains("\" "))
                existStr = $"{existStr.Split("\" ")[0]}\"";
            if (existStr.Equals(appPath)) isPathOk = true;
        }
        return true;
    }

    public static void ActivateWindowsStartupRegistry(bool active)
    {
        int startupDelay = 5;
        string appName = "SecureDnsClient";
        string prefix = "cmd /c start \"SDC\" /b ";
        string args = $"{prefix}\"{Path.GetFullPath(Application.ExecutablePath)}\" startup {startupDelay}";
        string regPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        RegistryKey? registry = Registry.CurrentUser.OpenSubKey(regPath, true);
        if (registry != null)
        {
            if (active)
                registry.SetValue(appName, args);
            else
                registry.DeleteValue(appName, false);
        }
    }

    public enum ConnectMode
    {
        ConnectToWorkingServers,
        ConnectToFakeProxyDohViaProxyDPI,
        ConnectToFakeProxyDohViaGoodbyeDPI,
        ConnectToPopularServersWithProxy,
        Unknown
    }

    public struct ConnectModeName
    {
        public const string WorkingServers = "Working Servers";
        public const string FakeProxyViaProxyDPI = "Fake Proxy Via Proxy DPI Bypass";
        public const string FakeProxyViaGoodbyeDPI = "Fake Proxy Via GoodbyeDPI";
        public const string PopularServersWithProxy = "Popular Servers With Proxy";
        public const string Unknown = "Unknown";
    }

    public ConnectMode GetConnectModeByName(string? name)
    {
        return name switch
        {
            ConnectModeName.WorkingServers => ConnectMode.ConnectToWorkingServers,
            ConnectModeName.FakeProxyViaProxyDPI => ConnectMode.ConnectToFakeProxyDohViaProxyDPI,
            ConnectModeName.FakeProxyViaGoodbyeDPI => ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI,
            ConnectModeName.PopularServersWithProxy => ConnectMode.ConnectToPopularServersWithProxy,
            ConnectModeName.Unknown => ConnectMode.Unknown,
            _ => ConnectMode.Unknown
        };
    }

    public string GetConnectModeNameByConnectMode(ConnectMode mode)
    {
        return mode switch
        {
            ConnectMode.ConnectToWorkingServers => ConnectModeName.WorkingServers,
            ConnectMode.ConnectToFakeProxyDohViaProxyDPI => ConnectModeName.FakeProxyViaProxyDPI,
            ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI => ConnectModeName.FakeProxyViaGoodbyeDPI,
            ConnectMode.ConnectToPopularServersWithProxy => ConnectModeName.PopularServersWithProxy,
            ConnectMode.Unknown => ConnectModeName.Unknown,
            _ => ConnectModeName.Unknown
        };
    }

    public void UpdateConnectModes(CustomComboBox ccb)
    {
        ccb.Text = "Select a Connect Mode";
        object item = ccb.SelectedItem;
        ccb.Items.Clear();
        List<string> connectModeNames = new()
        {
            ConnectModeName.WorkingServers,
            ConnectModeName.FakeProxyViaProxyDPI,
            ConnectModeName.FakeProxyViaGoodbyeDPI,
            ConnectModeName.PopularServersWithProxy
        };
        for (int n = 0; n < connectModeNames.Count; n++)
        {
            string connectModeName = connectModeNames[n];
            ccb.Items.Add(connectModeName);
        }
        if (ccb.Items.Count > 0)
        {
            bool exist = false;
            for (int i = 0; i < ccb.Items.Count; i++)
            {
                object selectedItem = ccb.Items[i];
                if (item != null && item.Equals(selectedItem))
                {
                    exist = true;
                    break;
                }
            }
            if (exist)
                ccb.SelectedItem = item;
            else
                ccb.SelectedIndex = 0;
            ccb.DropDownHeight = connectModeNames.Count * ccb.Height;
        }
        else ccb.SelectedIndex = -1;
    }

    public ConnectMode GetConnectMode()
    {
        // Get Connect modes
        bool a = CustomRadioButtonConnectCheckedServers.Checked;
        bool b = CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Checked;
        bool c = CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Checked;
        bool d = CustomRadioButtonConnectDNSCrypt.Checked;

        ConnectMode connectMode = ConnectMode.ConnectToWorkingServers;
        if (a) connectMode = ConnectMode.ConnectToWorkingServers;
        else if (b) connectMode = ConnectMode.ConnectToFakeProxyDohViaProxyDPI;
        else if (c) connectMode = ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI;
        else if (d) connectMode = ConnectMode.ConnectToPopularServersWithProxy;
        return connectMode;
    }

    private async Task<bool> DoesAppClosedNormallyAsync()
    {
        if (!File.Exists(SecureDNS.CloseStatusPath)) return true;
        string statusStr = string.Empty;
        try
        {
            statusStr = await File.ReadAllTextAsync(SecureDNS.CloseStatusPath);
            statusStr = statusStr.Replace(NL, string.Empty).Trim();
        }
        catch (Exception)
        {
            // do nothing
        }
        if (string.IsNullOrEmpty(statusStr)) return false;
        try
        {
            return Convert.ToBoolean(statusStr);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void AppClosedNormally(bool status)
    {
        try
        {
            File.WriteAllText(SecureDNS.CloseStatusPath, status.ToString());
        }
        catch (Exception)
        {
            // do nothing
        }
    }

    private static async Task MoveToNewLocation()
    {
        try
        {
            if (Directory.Exists(SecureDNS.OldUserDataDirPath))
                await FileDirectory.MoveDirectory(SecureDNS.OldUserDataDirPath, SecureDNS.UserDataDirPath, true, CancellationToken.None);
            
            if (Directory.Exists(SecureDNS.OldCertificateDirPath))
                await FileDirectory.MoveDirectory(SecureDNS.OldCertificateDirPath, SecureDNS.CertificateDirPath, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MoveToNewLocation: " + ex.Message);
        }
    }

    public bool IsInAction(bool showMsg, bool isCheckingForUpdate, bool isQuickConnectWorking, bool isCheckingStarted,
                           bool isConnecting, bool isDisconnecting, bool isDNSSetting, bool isDNSUnsetting,
                           bool isProxyActivating, bool isProxyDeactivating, bool isDisconnectingAll)
    {
        bool isInAction = !IsAppReady ||
                          (isCheckingForUpdate && IsCheckingForUpdate) ||
                          (isQuickConnectWorking && IsQuickConnectWorking) ||
                          (isCheckingStarted && IsCheckingStarted) ||
                          (isConnecting && IsConnecting) ||
                          (isDisconnecting && IsDisconnecting) ||
                          (isDNSSetting && IsDNSSetting) ||
                          (isDNSUnsetting && IsDNSUnsetting) ||
                          (isProxyActivating && IsProxyActivating) ||
                          (isProxyDeactivating && IsProxyDeactivating) ||
                          (isDisconnectingAll && IsDisconnectingAll);

        if (isInAction && showMsg)
        {
            if (!IsAppReady)
                CustomMessageBox.Show(this, "App is not ready or there's no Internet connection.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (IsCheckingForUpdate)
                CustomMessageBox.Show(this, "App is checking for update.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            else if (IsQuickConnectWorking)
                CustomMessageBox.Show(this, "Quick Connect is in action.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            else if (IsCheckingStarted)
                CustomMessageBox.Show(this, "App is checking DNS servers.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            else if (IsConnecting)
                CustomMessageBox.Show(this, "App is connecting.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            else if (IsDisconnecting)
                CustomMessageBox.Show(this, "App is disconnecting.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            else if (IsDNSSetting)
                CustomMessageBox.Show(this, "Let DNS set.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            else if (IsDNSUnsetting)
                CustomMessageBox.Show(this, "Let DNS unset.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            else if (IsProxyActivating)
                CustomMessageBox.Show(this, "Let Proxy Server activate.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            else if (IsProxyDeactivating)
                CustomMessageBox.Show(this, "Let Proxy Server deactivate.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            else if (IsDisconnectingAll)
                CustomMessageBox.Show(this, "App is disconnecting everything.", "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        return isInAction;
    }

    private static void DeleteFileOnSize(string filePath, int sizeKB)
    {
        try
        {
            if (File.Exists(filePath))
            {
                long lenth = new FileInfo(filePath).Length;
                if (ConvertTool.ConvertSize(lenth, ConvertTool.SizeUnits.Byte, ConvertTool.SizeUnits.KB, out _) > sizeKB)
                    File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Delete {Path.GetFileName(filePath)} File: {ex.Message}");
        }
    }

    private async Task<bool> IsInternetAlive(bool writeToLog = true, bool alsoCheckByBootstrapIp = true, int timeoutMS = 3000)
    {
        IPAddress bootstrapIP = GetBootstrapSetting(out _);
        bool isAlive = NetworkTool.IsInternetAlive();
        if (isAlive)
        {
            if (alsoCheckByBootstrapIp)
            {
                bool isAliveByUrl = await NetworkTool.IsInternetAliveAsync(bootstrapIP, timeoutMS);
                isAlive = isAlive && isAliveByUrl;
            }
        }
        else
        {
            isAlive = await NetworkTool.IsInternetAliveAsync(bootstrapIP, timeoutMS);
        }
        
        if (!isAlive)
        {
            string msgNet = "There is no Internet connectivity." + NL;
            if (writeToLog)
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNet, Color.IndianRed));
            return false;
        }
        else
            return true;
    }

    private async Task FlushDNS(bool showMsgHeaderAndFooter, bool showMsg)
    {
        if (IsFlushingDns) return;
        IsFlushingDns = true;
        string remove = "Windows IP Configuration";
        await Task.Run(async () =>
        {
            string msg = $"{NL}Flushing Dns...{NL}";
            if (showMsgHeaderAndFooter) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));

            msg = await ProcessManager.ExecuteAsync("ipconfig", null, "/flushdns", true, true);
            if (showMsg)
            {
                if (msg.Contains(remove)) msg = msg.Replace(remove, string.Empty);
                msg = msg.Replace(Environment.NewLine, string.Empty);
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Flush: ", Color.DodgerBlue));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"{msg}{NL}", Color.LightGray));
            }

            msg = await ProcessManager.ExecuteAsync("ipconfig", null, "/registerdns", true, true);
            if (showMsg)
            {
                if (msg.Contains(remove)) msg = msg.Replace(remove, string.Empty);
                msg = msg.Replace(Environment.NewLine, string.Empty);
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Register: ", Color.DodgerBlue));
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"{msg}{NL}", Color.LightGray));
            }

            msg = $"Dns flushed successfully.{NL}";
            if (showMsgHeaderAndFooter) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
        });
        IsFlushingDns = false;
    }

    private async Task FlushDnsOnExit(bool showMsg)
    {
        if (IsFlushingDns) return;
        await Task.Run(async () =>
        {
            string msg = $"{NL}Full Flushing Dns...{NL}";
            if (showMsg) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));

            await FlushDNS(false, showMsg);
            IsFlushingDns = true;

            msg = await ProcessManager.ExecuteAsync("ipconfig", null, "/release", true, true);
            if (showMsg) this.InvokeIt(() => CustomRichTextBoxLog.AppendText("Release:", Color.DodgerBlue));
            if (showMsg) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));

            msg = await ProcessManager.ExecuteAsync("ipconfig", null, "/renew", true, true);
            if (showMsg) this.InvokeIt(() => CustomRichTextBoxLog.AppendText("Renew:", Color.DodgerBlue));
            if (showMsg) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.LightGray));

            msg = $"Dns flushed successfully.{NL}";
            if (showMsg) this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
            //ProcessManager.Execute("netsh", "winsock reset"); // Needs PC Restart
        });
        IsFlushingDns = false;
    }

    public static List<int> GetPids(bool includeGoodbyeDpi)
    {
        List<int> list = new();
        int[] pids = { Environment.ProcessId, PIDProxy, PIDFakeProxy, PIDDNSProxy, PIDDNSProxyBypass, PIDDNSCrypt, PIDDNSCryptBypass };
        int[] pidsGD = { PIDGoodbyeDPIBasic, PIDGoodbyeDPIAdvanced, PIDGoodbyeDPIBypass };
        list.AddRange(pids);
        if (includeGoodbyeDpi) list.AddRange(pidsGD);
        return list.Distinct().ToList();
    }

    private async Task KillAll(bool killByName = false)
    {
        await Task.Run(() =>
        {
            try
            {
                if (killByName)
                {
                    ProcessManager.KillProcessByName("SDCProxyServer");
                    ProcessManager.KillProcessByName("dnslookup");
                    ProcessManager.KillProcessByName("dnsproxy");
                    ProcessManager.KillProcessByName("dnscrypt-proxy");
                    ProcessManager.KillProcessByName("goodbyedpi");
                }
                else
                {
                    ProcessManager.KillProcessByPID(PIDProxy);
                    ProcessManager.KillProcessByPID(PIDFakeProxy);
                    ProcessManager.KillProcessByName("dnslookup");
                    ProcessManager.KillProcessByPID(PIDDNSProxy);
                    ProcessManager.KillProcessByPID(PIDDNSProxyBypass);
                    ProcessManager.KillProcessByPID(PIDDNSCrypt);
                    ProcessManager.KillProcessByPID(PIDDNSCryptBypass);
                    ProcessManager.KillProcessByPID(PIDGoodbyeDPIBasic);
                    ProcessManager.KillProcessByPID(PIDGoodbyeDPIAdvanced);
                    ProcessManager.KillProcessByPID(PIDGoodbyeDPIBypass);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("KillAll: " + ex.Message);
            }
        });
    }

    public ProcessPriorityClass GetCPUPriority()
    {
        if (CustomRadioButtonSettingCPUHigh.Checked)
            return ProcessPriorityClass.High;
        else if (CustomRadioButtonSettingCPUAboveNormal.Checked)
            return ProcessPriorityClass.AboveNormal;
        else if (CustomRadioButtonSettingCPUNormal.Checked)
            return ProcessPriorityClass.Normal;
        else if (CustomRadioButtonSettingCPUBelowNormal.Checked)
            return ProcessPriorityClass.BelowNormal;
        else if (CustomRadioButtonSettingCPULow.Checked)
            return ProcessPriorityClass.Idle;
        else
            return ProcessPriorityClass.Normal;
    }

    public bool CheckNecessaryFiles(bool showMessage = true)
    {
        if (!File.Exists(SecureDNS.DnsLookup) || !File.Exists(SecureDNS.DnsProxy) || !File.Exists(SecureDNS.DNSCrypt) ||
            !File.Exists(SecureDNS.DNSCryptConfigPath) || !File.Exists(SecureDNS.DNSCryptConfigFakeProxyPath) ||
            !File.Exists(SecureDNS.GoodbyeDpi) || !File.Exists(SecureDNS.ProxyServerPath) || !File.Exists(SecureDNS.WinDivert) ||
            !File.Exists(SecureDNS.WinDivert32) || !File.Exists(SecureDNS.WinDivert64))
        {
            if (showMessage)
            {
                string msg = "ERROR: Some of binary files are missing!" + NL;
                CustomRichTextBoxLog.AppendText(msg, Color.IndianRed);
            }
            return false;
        }
        else
            return true;
    }

    private async Task<bool> WriteNecessaryFilesToDisk()
    {
        bool success = true;
        Architecture arch = RuntimeInformation.ProcessArchitecture;
        // Get New Versions
        string dnslookupNewVer = SecureDNS.GetBinariesVersionFromResource("dnslookup", arch);
        string dnsproxyNewVer = SecureDNS.GetBinariesVersionFromResource("dnsproxy", arch);
        string dnscryptNewVer = SecureDNS.GetBinariesVersionFromResource("dnscrypt-proxy", arch);
        string sdcproxyserverNewVer = SecureDNS.GetBinariesVersionFromResource("sdcproxyserver", arch);
        string goodbyedpiNewVer = SecureDNS.GetBinariesVersionFromResource("goodbyedpi", arch);

        // Get Old Versions
        string dnslookupOldVer = SecureDNS.GetBinariesVersion("dnslookup", arch);
        string dnsproxyOldVer = SecureDNS.GetBinariesVersion("dnsproxy", arch);
        string dnscryptOldVer = SecureDNS.GetBinariesVersion("dnscrypt-proxy", arch);
        string sdcproxyserverOldVer = SecureDNS.GetBinariesVersion("sdcproxyserver", arch);
        string goodbyedpiOldVer = SecureDNS.GetBinariesVersion("goodbyedpi", arch);

        // Get Version Result
        int dnslookupResult = Info.VersionCompare(dnslookupNewVer, dnslookupOldVer);
        int dnsproxyResult = Info.VersionCompare(dnsproxyNewVer, dnsproxyOldVer);
        int dnscryptResult = Info.VersionCompare(dnscryptNewVer, dnscryptOldVer);
        int sdcproxyserverResult = Info.VersionCompare(sdcproxyserverNewVer, sdcproxyserverOldVer);
        int goodbyedpiResult = Info.VersionCompare(goodbyedpiNewVer, goodbyedpiOldVer);

        // Check Missing/Update Binaries
        if (!CheckNecessaryFiles(false) || dnslookupResult == 1 || dnsproxyResult == 1 || dnscryptResult == 1 ||
                                           sdcproxyserverResult == 1 || goodbyedpiResult == 1)
        {
            string msg1 = $"Creating/Updating {arch} binaries. Please Wait..." + NL;
            CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);

            success = await writeBinariesAsync();
        }

        return success;

        async Task<bool> writeBinariesAsync()
        {
            try
            {
                if (!Directory.Exists(SecureDNS.BinaryDirPath))
                    Directory.CreateDirectory(SecureDNS.BinaryDirPath);

                if (!File.Exists(SecureDNS.DnsLookup) || dnslookupResult == 1)
                {
                    if (arch == Architecture.X64)
                        await File.WriteAllBytesAsync(SecureDNS.DnsLookup, NecessaryFiles.Resource1.dnslookup_X64);
                    if (arch == Architecture.X86)
                        await File.WriteAllBytesAsync(SecureDNS.DnsLookup, NecessaryFiles.Resource1.dnslookup_X86);
                }


                if (!File.Exists(SecureDNS.DnsProxy) || dnsproxyResult == 1)
                {
                    if (arch == Architecture.X64)
                        await File.WriteAllBytesAsync(SecureDNS.DnsProxy, NecessaryFiles.Resource1.dnsproxy_X64);
                    if (arch == Architecture.X86)
                        await File.WriteAllBytesAsync(SecureDNS.DnsProxy, NecessaryFiles.Resource1.dnsproxy_X86);
                }

                if (!File.Exists(SecureDNS.DNSCrypt) || dnscryptResult == 1)
                {
                    if (arch == Architecture.X64)
                        await File.WriteAllBytesAsync(SecureDNS.DNSCrypt, NecessaryFiles.Resource1.dnscrypt_proxy_X64);
                    if (arch == Architecture.X86)
                        await File.WriteAllBytesAsync(SecureDNS.DNSCrypt, NecessaryFiles.Resource1.dnscrypt_proxy_X86);
                }

                if (!File.Exists(SecureDNS.DNSCryptConfigPath))
                    await File.WriteAllBytesAsync(SecureDNS.DNSCryptConfigPath, NecessaryFiles.Resource1.dnscrypt_proxyTOML);

                if (!File.Exists(SecureDNS.DNSCryptConfigFakeProxyPath))
                    await File.WriteAllBytesAsync(SecureDNS.DNSCryptConfigFakeProxyPath, NecessaryFiles.Resource1.dnscrypt_proxy_fakeproxyTOML);

                if (!File.Exists(SecureDNS.ProxyServerPath) || sdcproxyserverResult == 1)
                {
                    if (arch == Architecture.X64)
                        await File.WriteAllBytesAsync(SecureDNS.ProxyServerPath, NecessaryFiles.Resource1.SDCProxyServer_X64);
                    if (arch == Architecture.X86)
                        await File.WriteAllBytesAsync(SecureDNS.ProxyServerPath, NecessaryFiles.Resource1.SDCProxyServer_X86);
                }

                if (!File.Exists(SecureDNS.GoodbyeDpi) || goodbyedpiResult == 1)
                    if (arch == Architecture.X64 || arch == Architecture.X86)
                        await File.WriteAllBytesAsync(SecureDNS.GoodbyeDpi, NecessaryFiles.Resource1.goodbyedpi);

                if (!File.Exists(SecureDNS.WinDivert))
                    if (arch == Architecture.X64 || arch == Architecture.X86)
                        await File.WriteAllBytesAsync(SecureDNS.WinDivert, NecessaryFiles.Resource1.WinDivert);

                if (!File.Exists(SecureDNS.WinDivert32))
                    if (arch == Architecture.X64 || arch == Architecture.X86)
                        await File.WriteAllBytesAsync(SecureDNS.WinDivert32, NecessaryFiles.Resource1.WinDivert32);

                if (!File.Exists(SecureDNS.WinDivert64))
                    if (arch == Architecture.X64 || arch == Architecture.X86)
                        await File.WriteAllBytesAsync(SecureDNS.WinDivert64, NecessaryFiles.Resource1.WinDivert64);

                // Update old version numbers
                await File.WriteAllTextAsync(SecureDNS.BinariesVersionPath, NecessaryFiles.Resource1.versions);

                return true;
            }
            catch (Exception ex)
            {
                string msg = $"{ex.Message}{NL}";
                msg += $"Couldn't write binaries to disk.{NL}";
                msg += "Please End Task the problematic process from Task Manager and Restart the Application.";
                CustomMessageBox.Show(this, msg, "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }

    public static bool IsDnsProtocolSupported(string dns)
    {
        dns = dns.Trim();
        StringComparison sc = StringComparison.OrdinalIgnoreCase;
        if (dns.StartsWith("tcp://", sc) || dns.StartsWith("udp://", sc) || dns.StartsWith("http://", sc) || dns.StartsWith("https://", sc) ||
            dns.StartsWith("tls://", sc) || dns.StartsWith("quic://", sc) || dns.StartsWith("h3://", sc) || dns.StartsWith("sdns://", sc))
            return true;
        else
            return isPlainDnsWithUnusualPort(dns);

        static bool isPlainDnsWithUnusualPort(string dns) // Support for plain DNS with unusual port
        {
            if (dns.Contains(':'))
            {
                string[] split = dns.Split(':');
                string ip = split[0];
                string port = split[1];
                if (NetworkTool.IsIPv4Valid(ip, out IPAddress? _))
                {
                    bool isPortValid = int.TryParse(port, out int outPort);
                    if (isPortValid && outPort >= 1 && outPort <= 65535)
                        return true;
                }
            }
            return false;
        }
    }

    private void InitializeStatus(CustomDataGridView dgv)
    {
        dgv.SelectionChanged += (s, e) => dgv.ClearSelection();
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
        dgv.BorderStyle = BorderStyle.None;
        List<DataGridViewRow> rList = new();
        for (int n = 0; n < 14; n++)
        {
            DataGridViewRow row = new();
            row.CreateCells(dgv, "cell0", "cell1");
            row.Height = TextRenderer.MeasureText("It doesn't matter what we write here!", dgv.Font).Height + 7;

            string cellName = n switch
            {
                0 => "Working Servers",
                1 => "Is Connected",
                2 => "Local DNS",
                3 => "Local DNS Latency",
                4 => "Local DoH",
                5 => "Local DoH Latency",
                6 => "Is DNS Set",
                7 => "Is Sharing",
                8 => "Proxy Requests",
                9 => "Is Proxy Set",
                10 => "Proxy DPI Bypass",
                11 => "GoodbyeDPI",
                12 => "CPU",
                13 => "",
                _ => string.Empty
            };

            if (n % 2 == 0)
                row.DefaultCellStyle.BackColor = BackColor.ChangeBrightness(-0.2f);
            else
                row.DefaultCellStyle.BackColor = BackColor;

            row.Cells[0].Value = cellName;
            row.Cells[1].Value = string.Empty;
            rList.Add(row);
        }
        
        dgv.Rows.AddRange(rList.ToArray());
    }

    private void InitializeNicStatus(CustomDataGridView dgv)
    {
        ToolStripMenuItem toolStripMenuItemCopy = new();
        toolStripMenuItemCopy.Text = "Copy Value";
        toolStripMenuItemCopy.Click += (s, e) =>
        {
            if (dgv.SelectedCells.Count > 0)
            {
                string? value = dgv.CurrentRow.Cells[1].Value.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    Clipboard.SetText(value);
                }
            }
        };
        CustomContextMenuStrip cms = new();
        cms.Items.Add(toolStripMenuItemCopy);

        dgv.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                dgv.Select(); // Set Focus on Control
                int currentMouseOverRow = dgv.HitTest(e.X, e.Y).RowIndex;
                if (currentMouseOverRow != -1)
                {
                    dgv.Rows[currentMouseOverRow].Cells[0].Selected = true;
                    dgv.Rows[currentMouseOverRow].Selected = true;

                    Theme.SetColors(cms);
                    cms.RoundedCorners = 5;
                    cms.Show(dgv, e.X, e.Y);
                }

            }
        };

        //dgv.SelectionChanged += (s, e) => dgv.ClearSelection();
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
        dgv.BorderStyle = BorderStyle.None;
        dgv.ShowCellToolTips = true;
        List<DataGridViewRow> rList = new();
        for (int n = 0; n < 14; n++)
        {
            DataGridViewRow row = new();
            row.CreateCells(dgv, "cell0", "cell1");
            row.Height = TextRenderer.MeasureText("It doesn't matter what we write here!", dgv.Font).Height + 4;

            string cellName = n switch
            {
                0 => "Name",
                1 => "Description",
                2 => "Adapter Type",
                3 => "Availability",
                4 => "Status",
                5 => "Net Status",
                6 => "DNS Addresses",
                7 => "GUID",
                8 => "MAC Address",
                9 => "Manufacturer",
                10 => "Is Physical Adapter",
                11 => "ServiceName",
                12 => "Max Speed",
                13 => "Time Of Last Reset",
                _ => string.Empty
            };

            if (n % 2 == 0)
                row.DefaultCellStyle.BackColor = BackColor.ChangeBrightness(-0.2f);
            else
                row.DefaultCellStyle.BackColor = BackColor;

            row.Cells[0].Value = cellName;
            row.Cells[1].Value = string.Empty;
            row.Cells[0].ToolTipText = string.Empty;
            row.Cells[1].ToolTipText = string.Empty;
            rList.Add(row);
        }

        dgv.Rows.AddRange(rList.ToArray());
    }

    private async Task CheckDPIWorks(string host, int timeoutSec = 30) //Default timeout: 100 sec
    {
        if (IsDisconnecting || IsDisconnectingAll || StopQuickConnect) return;
        if (CheckDpiBypassCTS.IsCancellationRequested) return;
        UpdateProxyBools = false;

        // Cancel Previous Task
        CheckDpiBypassCTS.Cancel();
        Task cancelWait = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(100);
                if (CheckDpiBypass == null) break;
                if (CheckDpiBypass.Status == TaskStatus.Canceled) break;
                if (CheckDpiBypass.Status == TaskStatus.Faulted) break;
                if (CheckDpiBypass.Status == TaskStatus.RanToCompletion) break;
            }
            
            if (CheckDpiBypass != null)
            {
                string msgCancel = $"Previous Check DPI Bypass: {CheckDpiBypass.Status}{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCancel, Color.DodgerBlue));
            }
        });
        try { await cancelWait.WaitAsync(TimeSpan.FromSeconds(10)); } catch (Exception) { }

        CheckDpiBypassCTS = new();
        CheckDpiBypass = Task.Run(async () =>
        {
            if (string.IsNullOrWhiteSpace(host)) return;

            // If there is no internet conectivity return
            if (!IsInternetOnline) return;

            // If In Action return
            if (IsInAction(false, false, false, true, true, true, true, true, true, true, true)) return;

            // Write start DPI checking to log
            string msgDPI = $"Checking DPI Bypass ({host})...{NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI, Color.LightGray));

            // Don't Update Bools Here!!

            // Wait for IsDPIActive
            Task wait1 = Task.Run(async () =>
            {
                while (true)
                {
                    if (IsDPIActive) break;
                    await Task.Delay(100);
                }
            });
            try { await wait1.WaitAsync(TimeSpan.FromSeconds(5), CheckDpiBypassCTS.Token); } catch (Exception) { }

            try
            {
                if (!IsDPIActive)
                {
                    // Write activate DPI first to log
                    string msgDPI1 = $"Check DPI Bypass: ";
                    string msgDPI2 = $"Activate DPI Bypass to check.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.IndianRed));
                    
                    return;
                }

                // Is Proxy Direct DNS Set?!
                bool isProxyDnsSet = false;
                if (ProcessManager.FindProcessByPID(PIDFakeProxy) &&
                    IsProxyRunning &&
                    ProxyDNSMode == ProxyProgram.Dns.Mode.DoH &&
                    ProxyStaticDPIBypassMode != ProxyProgram.DPIBypass.Mode.Disable)
                    isProxyDnsSet = true;

                if (!IsDNSSet && !isProxyDnsSet)
                {
                    // Write set DNS first to log
                    string msgDPI1 = $"Check DPI Bypass: ";
                    string msgDPI2 = $"Set DNS to check.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.Orange));

                    return;
                }

                string url = $"https://{host}/";
                Uri uri = new(url, UriKind.Absolute);

                bool isProxyPortOpen = NetworkTool.IsPortOpen(IPAddress.Loopback.ToString(), ProxyPort, 5);
                Debug.WriteLine($"Is Proxy Port Open: {isProxyPortOpen}, Port: {ProxyPort}");

                if (isProxyPortOpen && (IsProxyDpiBypassActive || IsProxySSLChangeSniToIpActive))
                {
                    Debug.WriteLine("Proxy");

                    UpdateProxyBools = false;
                    // Kill all requests before check
                    await ProxyConsole.SendCommandAsync("KillAll");
                    await Task.Delay(500);
                    UpdateProxyBools = true;

                    string proxyScheme = $"socks5://{IPAddress.Loopback}:{ProxyPort}";

                    using HttpClientHandler handler = new ();
                    handler.Proxy = new WebProxy(proxyScheme, true);
                    // Ignore Cert Check To Make It Faster
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;
                    
                    using HttpClient httpClientWithProxy = new(handler);
                    httpClientWithProxy.Timeout = TimeSpan.FromSeconds(timeoutSec);

                    // Get Only Header
                    using HttpRequestMessage message = new(HttpMethod.Head, uri);
                    message.Headers.TryAddWithoutValidation("User-Agent", "Other");

                    StopWatchCheckDPIWorks.Restart();
                    HttpResponseMessage r = await httpClientWithProxy.SendAsync(message, CheckDpiBypassCTS.Token);
                    StopWatchCheckDPIWorks.Stop();

                    if (r.IsSuccessStatusCode || r.StatusCode == HttpStatusCode.NotFound || r.StatusCode == HttpStatusCode.Forbidden)
                    {
                        msgSuccess();
                        r.Dispose();
                    }
                    else
                        msgFailed(r);
                }
                else
                {
                    Debug.WriteLine("No Proxy");

                    using HttpClientHandler handler = new();
                    handler.UseProxy = false;
                    // Ignore Cert Check To Make It Faster
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;

                    using HttpClient httpClient = new(handler);
                    httpClient.Timeout = TimeSpan.FromSeconds(timeoutSec);

                    // Get Only Header
                    using HttpRequestMessage message = new(HttpMethod.Head, uri);
                    message.Headers.TryAddWithoutValidation("User-Agent", "Other");

                    StopWatchCheckDPIWorks.Restart();
                    HttpResponseMessage r = await httpClient.SendAsync(message, CheckDpiBypassCTS.Token);
                    StopWatchCheckDPIWorks.Stop();

                    if (r.IsSuccessStatusCode || r.StatusCode == HttpStatusCode.NotFound || r.StatusCode == HttpStatusCode.Forbidden)
                    {
                        msgSuccess();
                        r.Dispose();
                    }
                    else
                        msgFailed(r);
                }

                void msgSuccess()
                {
                    // Write Success to log
                    if (IsDPIActive)
                    {
                        TimeSpan eTime = StopWatchCheckDPIWorks.Elapsed;
                        eTime = TimeSpan.FromMilliseconds(Math.Round(eTime.TotalMilliseconds, 2));
                        string eTimeStr = eTime.Seconds > 9 ? $"{eTime:ss\\.ff}" : $"{eTime:s\\.ff}";
                        string msgDPI1 = $"Check DPI Bypass: ";
                        string msgDPI2 = $"Successfully opened {host} in {eTimeStr} seconds.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.MediumSeaGreen));
                    }
                    else
                    {
                        string msgCancel = $"Check DPI Bypass: Canceled.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCancel, Color.LightGray));
                    }
                }

                void msgFailed(HttpResponseMessage r)
                {
                    // Write Status to log
                    if (IsDPIActive)
                    {
                        string msgDPI1 = $"Check DPI Bypass: ";
                        string msgDPI2 = $"Status {r.StatusCode}: {r.ReasonPhrase}.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.LightGray));
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI2, Color.DodgerBlue));
                    }
                    else
                    {
                        string msgCancel = $"Check DPI Bypass: Canceled.{NL}";
                        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCancel, Color.LightGray));
                    }

                    r.Dispose();
                }
            }
            catch (Exception ex)
            {
                // Write Failed to log
                if (IsDPIActive)
                {
                    string msgDPI1 = $"Check DPI Bypass:{ex.Message}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgDPI1, Color.IndianRed));
                }
                else
                {
                    string msgCancel = $"Check DPI Bypass: Canceled.{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgCancel, Color.LightGray));
                }
            }

            UpdateProxyBools = true;
        });

        try { await CheckDpiBypass.WaitAsync(CheckDpiBypassCTS.Token); } catch (Exception) { }

        string msgTask = $"Check DPI Bypass: {CheckDpiBypass.Status}{NL}";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgTask, Color.DodgerBlue));

        StopWatchCheckDPIWorks.Reset();
        UpdateProxyBools = true;
    }

    private async Task<bool> WarmUpProxy(string host, string proxyScheme, int timeoutSec, CancellationToken cancellationToken)
    {
        string url = $"https://{host}/";
        Uri uri = new(url, UriKind.Absolute);

        using HttpClientHandler handler = new();
        handler.Proxy = new WebProxy(proxyScheme, true);
        // Ignore Cert Check To Make It Faster
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;

        using HttpClient httpClientWithProxy = new(handler);
        httpClientWithProxy.Timeout = TimeSpan.FromSeconds(timeoutSec);

        // Get Only Header
        using HttpRequestMessage message = new(HttpMethod.Head, uri);
        message.Headers.TryAddWithoutValidation("User-Agent", "Other");

        try
        {
            await httpClientWithProxy.SendAsync(message, cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    //private object DetectProxyChanges()
    //{

    //}

}