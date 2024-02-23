using CustomControls;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using MsmhToolsClass;
using MsmhToolsClass.ProxyServerPrograms;
using MsmhToolsWinFormsClass;
using MsmhToolsWinFormsClass.Themes;
using SecureDNSClient.DPIBasic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Task = System.Threading.Tasks.Task;

namespace SecureDNSClient;

public partial class FormMain
{
    private async Task LoadTheme()
    {
        if (!IsThemeApplied || !IsScreenHighDpiScaleApplied)
        {
            Theme.LoadTheme(this, Theme.Themes.Dark);
            Theme.SetColors(LabelMain);
            CustomMessageBox.FormIcon = Properties.Resources.SecureDNSClient_Icon_Multi;
            await Task.Delay(100);
            List<Control> controls = Controllers.GetAllControls(this);
            for (int i = 0; i < controls.Count; i++)
            {
                Control c = controls[i];
                if (c is SplitContainer sc)
                    if (sc.Name.EndsWith("Main"))
                        this.InvokeIt(() => sc.Panel2.BackColor = BackColor.ChangeBrightness(-0.2f));
            }
            this.InvokeIt(() => CustomCheckBoxSettingQcOnStartup.BackColor = BackColor.ChangeBrightness(-0.2f));
            await ScreenHighDpiScaleStartup(this);

            // Add colors and texts to About page
            this.InvokeIt(() =>
            {
                CustomLabelAboutThis.ForeColor = Color.DodgerBlue;
                string aboutVer = $"v{Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductVersion} ({ArchProcess.ToString().ToLower()})";
                CustomLabelAboutVersion.Text = aboutVer;
                CustomLabelAboutThis2.ForeColor = Color.IndianRed;
            });

            // Wait
            //Debug.WriteLine("All Controls: " + controls.Count);
            await Task.Delay(controls.Count * 5);

            IsThemeApplied = true;
        }
    }

    public bool IsEverythingDisconnected()
    {
        return !IsCheckingStarted && !IsQuickConnecting &&
               !IsConnected && !IsConnecting &&
               !ProcessManager.FindProcessByPID(PIDDNSProxy) &&
               !ProcessManager.FindProcessByPID(PIDDNSProxyBypass) &&
               !ProcessManager.FindProcessByPID(PIDDNSCrypt) &&
               !ProcessManager.FindProcessByPID(PIDDNSCryptBypass) &&
               !ProcessManager.FindProcessByPID(PIDFakeProxy) &&
               !ProcessManager.FindProcessByPID(PIDCamouflageProxy) &&
               !ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass) &&
               !ProcessManager.FindProcessByPID(PIDProxy) &&
               !ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic) &&
               !ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced) &&
               !IsDNSSet && !IsDNSSetting &&
               !IsProxyActivated && !IsProxyActivating && !IsProxyRunning &&
               !IsProxySet &&
               !IsGoodbyeDPIBasicActive && !IsGoodbyeDPIAdvancedActive;
    }

    public async void GetAppReady()
    {
        await Task.Run(async () =>
        {
            IsAppReady = false;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Waiting for Network...{NL}", Color.Gray));
            while (true)
            {
                await Task.Delay(1000);
                IPAddress bootstrapIP = GetBootstrapSetting(out _);
                IsAppReady = IsInternetOnline = await NetworkTool.IsInternetAliveAsync(bootstrapIP, 3000);
                if (IsAppReady) break;
            }
            string msgReady = $"Network Detected (Up Time: {ConvertTool.TimeSpanToHumanRead(AppUpTime.Elapsed, true)}){NL}";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgReady, Color.Gray));
        });
    }

    public async void StartupTask()
    {
        StartupTaskExecuted = true;

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
        await SetDnsOnNic_.UpdateNICs(CustomComboBoxNICs, GetBootstrapSetting(out int port), port);

        // Start Quick Connect (To User Settings)
        QcToUserSetting_Click(null, EventArgs.Empty);
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
                            string separator = "\" ";
                            if (existStr.Contains(separator))
                                existStr = $"{existStr.Split(separator)[0]}\"";
                            string appPathOnTaskScheduler = existStr;
                            if (appPathOnTaskScheduler.Equals(appPath) && td.Principal.RunLevel == TaskRunLevel.Highest) isPathOk = true;
                            else
                            {
                                appPathOnTaskScheduler = appPathOnTaskScheduler.Trim('"');
                                try { appPathOnTaskScheduler = Path.GetFullPath(appPathOnTaskScheduler); } catch (Exception) { }
                                if (!File.Exists(appPathOnTaskScheduler))
                                {
                                    // Remove Startup: App Path On TaskScheduler Does Not Exist.
                                    ActivateWindowsStartup(false);
                                }
                            }
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
            string appArgs = $"-IsPortable={Program.IsPortable} -IsStartup=True -StartupDelaySec={startupDelay}";
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

        try { registry.Dispose(); } catch (Exception) { }
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

            try { registry.Dispose(); } catch (Exception) { }
        }
    }

    private async Task FillComboBoxes(bool nics = true, bool qcConnectModes = true, bool qcNics = true, bool qcGoodbyeDpiModes = true)
    {
        IPAddress bootstrapIP = GetBootstrapSetting(out int bootstrapPort);

        // Update NICs
        if (nics) await SetDnsOnNic_.UpdateNICs(CustomComboBoxNICs, bootstrapIP, bootstrapPort);

        // Update Connect Modes (Quick Connect Settings)
        if (qcConnectModes) UpdateConnectModes(CustomComboBoxSettingQcConnectMode);

        // Update NICs (Quick Connect Settings)
        if (qcNics) await SetDnsOnNic_.UpdateNICs(CustomComboBoxSettingQcNics, bootstrapIP, bootstrapPort);

        // Update GoodbyeDPI Basic Modes (Quick Connect Settings)
        if (qcGoodbyeDpiModes) DPIBasicBypass.UpdateGoodbyeDpiBasicModes(CustomComboBoxSettingQcGdBasic);
    }

    private static async Task<bool> DoesAppClosedNormallyAsync()
    {
        if (!File.Exists(SecureDNS.CloseStatusPath)) return true;
        string statusStr = string.Empty;
        try
        {
            statusStr = await File.ReadAllTextAsync(SecureDNS.CloseStatusPath);
            statusStr = statusStr.Replace(NL, string.Empty).Trim();
        }
        catch (Exception) { }

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
        catch (Exception) { }
    }

    public bool IsInAction(bool showMsg, bool isCheckingForUpdate, bool isQuickConnectWorking, bool isCheckingStarted,
                           bool isConnecting, bool isDisconnecting, bool isDNSSetting, bool isDNSUnsetting,
                           bool isProxyActivating, bool isProxyDeactivating, bool isDisconnectingAll, out string reason)
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
                          (isDisconnectingAll && IsDisconnectingAll) ||
                          IsExiting;

        reason = "In Action...";

        if (isInAction)
        {
            if (!IsAppReady) reason = "App is not ready or there's no Internet connection.";
            else if (IsCheckingForUpdate) reason = "App is checking for update.";
            else if (IsCheckingStarted) reason = "App is checking DNS servers.";
            else if (IsConnecting) reason = "App is connecting.";
            else if (IsDisconnecting) reason = "App is disconnecting.";
            else if (IsDNSSetting) reason = "DNS is setting.";
            else if (IsDNSUnsetting) reason = "DNS is unsetting.";
            else if (IsProxyActivating) reason = "Proxy Server is activating.";
            else if (IsProxyDeactivating) reason = "Proxy Server is deactivating.";
            else if (IsQuickConnectWorking) reason = "Quick Connect is in action.";
            else if (IsDisconnectingAll) reason = "App is disconnecting everything.";

            if (showMsg)
            {
                string msg = IsQuickConnectWorking ? "Quick Connect is in action." : reason;
                CustomMessageBox.Show(this, msg, "Wait...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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

    private async Task<bool> IsInternetAlive(bool writeToLog = true, int timeoutMS = 3000)
    {
        bool isAlive = false;

        try
        {
            bool bootstrapCondition = AppUpTime.Elapsed < TimeSpan.FromSeconds(30) && !IsConnected && !IsDNSConnected;

            if (bootstrapCondition)
            {
                IPAddress bootstrapIP = GetBootstrapSetting(out _);
                isAlive = await NetworkTool.IsInternetAliveAsync(bootstrapIP, timeoutMS);
            }
            else
            {
                isAlive = NetworkTool.IsInternetAlive();
                if (!isAlive)
                {
                    // Read Bootstrap If Internet Is Not Based On Adapter
                    IPAddress bootstrapIP = GetBootstrapSetting(out _);
                    isAlive = await NetworkTool.IsInternetAliveAsync(bootstrapIP, timeoutMS);
                }
            }
        }
        catch (Exception) { }
        
        if (!isAlive)
        {
            if (writeToLog)
            {
                string msgNet = $"There is no Internet connectivity.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgNet, Color.IndianRed));
            }
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

    private static bool IsThereAnyLeftovers()
    {
        return ProcessManager.FindProcessByName("SDCProxyServer") ||
               ProcessManager.FindProcessByName("dnslookup") ||
               ProcessManager.FindProcessByName("dnsproxy") ||
               ProcessManager.FindProcessByName("dnscrypt-proxy") ||
               ProcessManager.FindProcessByName("goodbyedpi");
    }

    private static async Task KillAll(bool killByName = false)
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
            !File.Exists(SecureDNS.ProxyServerPath) || !File.Exists(SecureDNS.GoodbyeDpi) ||
            !File.Exists(SecureDNS.WinDivert) || !File.Exists(SecureDNS.WinDivert32) || !File.Exists(SecureDNS.WinDivert64))
        {
            if (showMessage)
            {
                string msg = "ERROR: Some of binary files are missing!" + NL;
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.IndianRed));
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

                string msgWB = $"{Info.GetAppInfo(Assembly.GetExecutingAssembly()).ProductName} is ready.{NL}";
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msgWB, Color.LightGray));

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
            if (IsInAction(false, false, false, true, true, true, true, true, true, true, true, out _)) return;

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
                    ProxyStaticFragmentMode != ProxyProgram.Fragment.Mode.Disable)
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

                if (isProxyPortOpen && IsProxyDpiBypassActive)
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

    private static async Task<bool> WarmUpProxy(string host, string proxyScheme, int timeoutSec, CancellationToken cancellationToken)
    {
        string url = $"https://{host}/";
        Uri uri = new(url, UriKind.Absolute);

        using HttpClientHandler handler = new();
        handler.Proxy = new WebProxy(proxyScheme, true);
        handler.UseProxy = true;
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

    // ============================== Old Content To New

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

    public void OldProxyRulesToNew()
    {
        try
        {
            bool start = false;
            if (!File.Exists(SecureDNS.ProxyRulesPath)) start = true;
            else
            {
                string content = File.ReadAllText(SecureDNS.ProxyRulesPath);
                if (content.Length < 5) start = true;
            }
            if (!start) return;

            List<string> blackListList = new();
            blackListList.LoadFromFile(SecureDNS.BlackWhiteListPath, true, true);

            List<string> fakeDnsList = new();
            fakeDnsList.LoadFromFile(SecureDNS.FakeDnsRulesPath, true, true);

            List<string> fakeSniList = new();
            fakeSniList.LoadFromFile(SecureDNS.FakeSniRulesPath, true, true);

            List<string> dontBypassList = new();
            dontBypassList.LoadFromFile(SecureDNS.DontBypassListPath, true, true);

            // Create ProxyRules
            List<string> proxyRules = new();

            // Get All Domains And Put Them Into ProxyRules
            foreach (string blackList in blackListList)
            {
                if (string.IsNullOrEmpty(blackList)) continue;
                if (blackList.StartsWith("//")) continue;
                proxyRules.Add($"{blackList}|");
            }

            foreach (string fakeDns in fakeDnsList)
            {
                if (fakeDns.StartsWith("//")) continue;
                if (fakeDns.Contains('|'))
                {
                    string[] split0 = fakeDns.Split('|');
                    string domain0 = split0[0].Trim();
                    if (string.IsNullOrEmpty(domain0)) continue;
                    string fd = split0[1].Trim();
                    if (string.IsNullOrEmpty(fd)) continue;
                    if (!proxyRules.IsContain(domain0)) proxyRules.Add($"{domain0}|");
                }
            }

            foreach (string fakeSni in fakeSniList)
            {
                if (fakeSni.StartsWith("//")) continue;
                if (fakeSni.Contains('|'))
                {
                    string[] split0 = fakeSni.Split('|');
                    string domain0 = split0[0].Trim();
                    if (string.IsNullOrEmpty(domain0)) continue;
                    string fs = split0[1].Trim();
                    if (string.IsNullOrEmpty(fs)) continue;
                    if (!proxyRules.IsContain(domain0)) proxyRules.Add($"{domain0}|");
                }
            }

            foreach (string dontBypass in dontBypassList)
            {
                if (string.IsNullOrEmpty(dontBypass)) continue;
                if (dontBypass.StartsWith("//")) continue;
                if (!proxyRules.IsContain(dontBypass)) proxyRules.Add($"{dontBypass}|");
            }

            // DeDup
            proxyRules = proxyRules.Distinct().ToList();
            if (!proxyRules.Any()) return;

            // Apply Black List
            for (int n = 0; n < proxyRules.Count; n++)
            {
                string domain = proxyRules[n];
                if (domain.Contains('|'))
                {
                    string[] split = domain.Split('|');
                    string domain0 = split[0].Trim();
                    if (!string.IsNullOrEmpty(domain0))
                        domain = domain0;
                }

                foreach (string blackList in blackListList)
                {
                    if (string.IsNullOrEmpty(blackList)) continue;
                    if (blackList.StartsWith("//")) continue;
                    string domain0 = blackList;
                    if (domain0.Equals("*") || domain.Equals(domain0))
                    {
                        proxyRules[n] += "-;";
                    }
                }
            }

            // Apply Fake Dns List
            for (int n = 0; n < proxyRules.Count; n++)
            {
                string domain = proxyRules[n];
                if (domain.Contains('|'))
                {
                    string[] split = domain.Split('|');
                    string domain0 = split[0].Trim();
                    if (!string.IsNullOrEmpty(domain0))
                        domain = domain0;
                }

                foreach (string fakeDns in fakeDnsList)
                {
                    if (fakeDns.StartsWith("//")) continue;
                    if (fakeDns.Contains('|'))
                    {
                        string[] split0 = fakeDns.Split('|');
                        string domain0 = split0[0].Trim();
                        if (string.IsNullOrEmpty(domain0)) continue;
                        string fd = split0[1].Trim();
                        if (string.IsNullOrEmpty(fd)) continue;
                        if (domain0.Equals("*") || domain.Equals(domain0))
                        {
                            if (!proxyRules[n].Contains("-;"))
                                proxyRules[n] += $"{fd};";
                        }
                    }
                }
            }

            // Apply Fake Sni List
            for (int n = 0; n < proxyRules.Count; n++)
            {
                string domain = proxyRules[n];
                if (domain.Contains('|'))
                {
                    string[] split = domain.Split('|');
                    string domain0 = split[0].Trim();
                    if (!string.IsNullOrEmpty(domain0))
                        domain = domain0;
                }

                foreach (string fakeSni in fakeSniList)
                {
                    if (fakeSni.StartsWith("//")) continue;
                    if (fakeSni.Contains('|'))
                    {
                        string[] split0 = fakeSni.Split('|');
                        string domain0 = split0[0].Trim();
                        if (string.IsNullOrEmpty(domain0)) continue;
                        string fs = split0[1].Trim();
                        if (string.IsNullOrEmpty(fs)) continue;
                        if (domain0.Equals("*") || domain.Equals(domain0))
                        {
                            if (!proxyRules[n].Contains("-;") && !proxyRules[n].Contains("sni:"))
                                proxyRules[n] += $"sni:{fs};";
                        }
                    }
                }
            }

            // Apply DontBypassList
            for (int n = 0; n < proxyRules.Count; n++)
            {
                string domain = proxyRules[n];
                if (domain.Contains('|'))
                {
                    string[] split = domain.Split('|');
                    string domain0 = split[0].Trim();
                    if (!string.IsNullOrEmpty(domain0))
                        domain = domain0;
                }

                foreach (string dontBypass in dontBypassList)
                {
                    if (string.IsNullOrEmpty(dontBypass)) continue;
                    if (dontBypass.StartsWith("//")) continue;
                    string domain0 = dontBypass;
                    if (string.IsNullOrEmpty(domain0)) continue;
                    if (domain0.Equals("*") || domain.Equals(domain0))
                    {
                        if (!proxyRules[n].Contains("-;") && !proxyRules[n].Contains("--;"))
                            proxyRules[n] += "--;";
                    }
                }
            }

            if (proxyRules.Any())
            {
                // Save ProxyRules To File
                proxyRules.SaveToFile(SecureDNS.ProxyRulesPath);

                // Enable ProxyRules CheckBox
                this.InvokeIt(() => CustomCheckBoxSettingProxyEnableRules.Checked = true);
            }


            try
            {
                File.Delete(SecureDNS.BlackWhiteListPath);
                File.Delete(SecureDNS.FakeDnsRulesPath);
                File.Delete(SecureDNS.FakeSniRulesPath);
                File.Delete(SecureDNS.DontBypassListPath);
            }
            catch (Exception) { }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("OldProxyRulesToNew: " + ex.Message);
        }
    }

}