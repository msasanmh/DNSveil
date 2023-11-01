using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass.Themes;
using SecureDNSClient.DPIBasic;

namespace SecureDNSClient;

public partial class FormMain
{
    private async void ShowMainContextMenu()
    {
        // Update bool IsGoodbyeDPIBasicActive
        IsGoodbyeDPIBasicActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic);

        // Clear Items
        CustomContextMenuStripIcon.Items.Clear();

        // GoodbyeDPI Basic DropDown Menu
        ToolStripMenuItem goodbyeDpiBasic = new();
        goodbyeDpiBasic.Font = Font;
        goodbyeDpiBasic.Text = "Activate GoodbyeDPI Basic";

        // Add All Modes to DropDown Items
        List<DPIBasicBypassMode> bModes = DPIBasicBypass.GetAllModes();
        ToolStripItem[] gdbSubMenuItems = new ToolStripItem[bModes.Count];
        for (int n = 0; n < bModes.Count; n++)
        {
            DPIBasicBypassMode groupName = bModes[n];
            gdbSubMenuItems[n] = new ToolStripMenuItem(groupName.ToString());
            gdbSubMenuItems[n].Font = Font;
            gdbSubMenuItems[n].Name = groupName.ToString();
            gdbSubMenuItems[n].Click -= GdbModes_Click;
            gdbSubMenuItems[n].Click += GdbModes_Click;
        }
        goodbyeDpiBasic.DropDownItems.AddRange(gdbSubMenuItems);
        CustomContextMenuStripIcon.Items.Add(goodbyeDpiBasic);

        // Update bool IsGoodbyeDPIAdvancedActive
        IsGoodbyeDPIAdvancedActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced);

        // GoodbyeDPI Advanced Menu
        ToolStripMenuItem goodbyeDpiAdvanced = new();
        goodbyeDpiAdvanced.Font = Font;
        goodbyeDpiAdvanced.Text = IsGoodbyeDPIAdvancedActive ? "Reactivate GoodbyeDPI Advanced" : "Activate GoodbyeDPI Advanced";
        goodbyeDpiAdvanced.Click -= GoodbyeDpiAdvanced_Click;
        goodbyeDpiAdvanced.Click += GoodbyeDpiAdvanced_Click;
        CustomContextMenuStripIcon.Items.Add(goodbyeDpiAdvanced);

        // GoodbyeDPI Deactive Menu
        ToolStripMenuItem goodbyeDpiDeactive = new();
        goodbyeDpiDeactive.Font = Font;
        goodbyeDpiDeactive.Text = "Deactive GoodbyeDPI";
        goodbyeDpiDeactive.Enabled = IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive;
        goodbyeDpiDeactive.Click -= GoodbyeDpiDeactive_Click;
        goodbyeDpiDeactive.Click += GoodbyeDpiDeactive_Click;
        CustomContextMenuStripIcon.Items.Add(goodbyeDpiDeactive);

        // Spacer
        CustomContextMenuStripIcon.Items.Add("-");

        // Update Bool
        IsProxyActivated = ProcessManager.FindProcessByPID(PIDProxy);

        // Proxy Server Menu
        ToolStripMenuItem proxy = new();
        proxy.Font = Font;
        proxy.Text = IsProxyActivating ? "Starting Proxy Server" : IsProxyDeactivating ? "Stopping Proxy Server" : IsProxyActivated ? "Stop Proxy Server" : "Start Proxy Server";
        proxy.Enabled = !IsProxyActivating && !IsProxyDeactivating && !IsDNSSetting && !IsDNSUnsetting;
        proxy.Click -= Proxy_Click;
        proxy.Click += Proxy_Click;
        CustomContextMenuStripIcon.Items.Add(proxy);

        // Set Proxy to System
        ToolStripMenuItem proxySet = new();
        proxySet.Font = Font;
        proxySet.Text = IsProxySet ? "Unset Proxy from System" : "Set Proxy to System";
        proxySet.Enabled = IsProxyActivated && !IsProxyActivating && !IsProxyDeactivating;
        proxySet.Click -= ProxySet_Click;
        proxySet.Click += ProxySet_Click;
        CustomContextMenuStripIcon.Items.Add(proxySet);

        // Spacer
        CustomContextMenuStripIcon.Items.Add("-");

        // Quick Connect DropDown Menu
        ToolStripMenuItem quickConnectTo = new();
        quickConnectTo.Font = Font;
        quickConnectTo.Text = "Quick Connect To";

        // Add to User Settings
        ToolStripMenuItem qcToUserSetting = new();
        qcToUserSetting.Font = Font;
        qcToUserSetting.Text = "User Settings";
        qcToUserSetting.Click -= QcToUserSetting_Click;
        qcToUserSetting.Click += QcToUserSetting_Click;
        quickConnectTo.DropDownItems.Add(qcToUserSetting);

        // Add Built-In to DropDown Items
        ToolStripMenuItem qcToBuiltIn = new();
        qcToBuiltIn.Font = Font;
        qcToBuiltIn.Text = "Built-In Servers";
        qcToBuiltIn.Click -= QcToBuiltIn_Click;
        qcToBuiltIn.Click += QcToBuiltIn_Click;
        quickConnectTo.DropDownItems.Add(qcToBuiltIn);

        // Add Custom Groups to DropDown Items
        List<string> groupList = await ReadCustomServersXmlGroups(SecureDNS.CustomServersXmlPath);
        ToolStripItem[] subMenuItems = new ToolStripItem[groupList.Count];
        for (int n = 0; n < groupList.Count; n++)
        {
            string groupName = groupList[n];
            subMenuItems[n] = new ToolStripMenuItem(groupName);
            subMenuItems[n].Font = Font;
            subMenuItems[n].Name = groupName;
            subMenuItems[n].Click -= QcToCustomGroups_Click;
            subMenuItems[n].Click += QcToCustomGroups_Click;
        }
        quickConnectTo.DropDownItems.AddRange(subMenuItems);
        CustomContextMenuStripIcon.Items.Add(quickConnectTo);

        // Disconnect Menu
        ToolStripMenuItem disconnectAll = new();
        disconnectAll.Font = Font;
        disconnectAll.Text = "Disconnect All";
        disconnectAll.Enabled = IsCheckingStarted || IsQuickConnecting || IsConnected || IsConnecting || IsQuickConnecting ||
                                IsDNSSet || IsDNSSetting || IsProxyActivated || IsProxyActivating || IsProxySet ||
                                IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive;
        disconnectAll.Click -= DisconnectAll_Click;
        disconnectAll.Click += DisconnectAll_Click;
        CustomContextMenuStripIcon.Items.Add(disconnectAll);

        // Spacer
        CustomContextMenuStripIcon.Items.Add("-");

        // Exit Menu
        ToolStripMenuItem exit = new();
        exit.Font = Font;
        exit.Text = "Exit";
        exit.Click -= Exit_Click;
        exit.Click += Exit_Click;
        CustomContextMenuStripIcon.Items.Add(exit);

        CustomContextMenuStripIcon.Font = Font;
        Theme.SetColors(CustomContextMenuStripIcon);
        CustomContextMenuStripIcon.Show();
    }

    private void GdbModes_Click(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem tsmi)
        {
            // Activate/Reactivate GoodbyeDPI Basic
            DPIBasicBypassMode mode = DPIBasicBypass.GetGoodbyeDpiModeBasicByName(tsmi.Name);
            GoodbyeDPIBasic(mode);
        }
    }

    private void GoodbyeDpiAdvanced_Click(object? sender, EventArgs e)
    {
        GoodbyeDPIAdvanced();
    }

    private void GoodbyeDpiDeactive_Click(object? sender, EventArgs e)
    {
        GoodbyeDPIDeactive(true, true);
    }

    private async void Proxy_Click(object? sender, EventArgs e)
    {
        await StartProxy();
    }

    private void ProxySet_Click(object? sender, EventArgs e)
    {
        SetProxy();
    }

    private async void QcToUserSetting_Click(object? sender, EventArgs e)
    {
        await StartQuickConnect(null, true);
    }

    private async void QcToBuiltIn_Click(object? sender, EventArgs e)
    {
        await StartQuickConnect("builtin", true);
    }

    private async void QcToCustomGroups_Click(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem tsmi)
        {
            await StartQuickConnect(tsmi.Name, true);
        }
    }

    private async void DisconnectAll_Click(object? sender, EventArgs e)
    {
        await DisconnectAll();
    }

    private async Task DisconnectAll()
    {
        if (IsDisconnectingAll) return;
        IsDisconnectingAll = true;

        // New Line
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(NL, Color.LightGray));

        // Stop Checking Servers
        if (IsCheckingStarted)
        {
            StopChecking = true;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Canceling Check Operation...{NL}", Color.LightGray));

            // Wait
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (!IsCheckingStarted) break;
                    await Task.Delay(100);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

            StopChecking = false;
            this.InvokeIt(() => CustomProgressBarCheck.StopTimer = true);
        }

        // Stop Quick Connect
        if (IsQuickConnecting)
        {
            StopQuickConnect = true;
            IsDisconnecting = true;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Stopping Quick Connect...{NL}", Color.LightGray));

            // Wait
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (!IsQuickConnecting) break;
                    await Task.Delay(100);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromSeconds(20)); } catch (Exception) { }

            StopQuickConnect = false;
            IsDisconnecting = false;
        }

        // Deactivate GoodbyeDPI
        if (IsGoodbyeDPIBasicActive || IsGoodbyeDPIAdvancedActive)
        {
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating GoodbyeDPI...{NL}", Color.LightGray));
            ProcessManager.KillProcessByPID(PIDGoodbyeDPIBasic);
            ProcessManager.KillProcessByPID(PIDGoodbyeDPIAdvanced);

            // Wait
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (!ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic) &&
                        !ProcessManager.FindProcessByPID(PIDGoodbyeDPIAdvanced)) break;
                    await Task.Delay(100);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
        }

        // Deactivate GoodbyeDPIBypass (Connect Method 3)
        if (ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass))
        {
            IsDisconnecting = true;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating GoodbyeDPI Bypass...{NL}", Color.LightGray));
            ProcessManager.KillProcessByPID(PIDGoodbyeDPIBypass);
            BypassFakeProxyDohStop(true, true, true, false);

            // Wait
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (!ProcessManager.FindProcessByPID(PIDGoodbyeDPIBypass)) break;
                    await Task.Delay(100);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

            IsDisconnecting = false;
        }

        // Unset Proxy
        if (IsProxySet)
        {
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Unsetting Proxy...{NL}", Color.LightGray));
            NetworkTool.UnsetProxy(false, true);

            // Wait
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (!IsProxySet) break;
                    await Task.Delay(100);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
        }

        // Deactivate Proxy
        if (IsProxyActivated || IsProxyActivating)
        {
            IsProxyDeactivating = true;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating Proxy...{NL}", Color.LightGray));
            ProcessManager.KillProcessByPID(PIDProxy);

            // Wait
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (!ProcessManager.FindProcessByPID(PIDProxy)) break;
                    await Task.Delay(100);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

            IsProxyDeactivating = false;
        }

        // Deactivate Fake Proxy
        if (ProcessManager.FindProcessByPID(PIDFakeProxy))
        {
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating Fake Proxy...{NL}", Color.LightGray));
            ProcessManager.KillProcessByPID(PIDFakeProxy);

            // Wait
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (!ProcessManager.FindProcessByPID(PIDFakeProxy)) break;
                    await Task.Delay(100);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
        }

        // Unset DNS
        if (IsDNSSet || IsDNSSetting)
        {
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Unsetting DNS...{NL}", Color.LightGray));
            await Task.Run(async () => await UnsetAllDNSs());
        }

        // Disconnect -  Kill all processes
        if (IsConnected || IsConnecting)
        {
            IsDisconnecting = true;
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Disconnecting...{NL}", Color.LightGray));
            await Task.Run(async () => await KillAll());

            // Wait
            Task wait = Task.Run(async () =>
            {
                while (true)
                {
                    if (!IsConnected && !IsConnecting) break;
                    await Task.Delay(100);
                }
            });
            try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }

            IsDisconnecting = false;
        }

        // Flush DNS On Exit
        if (DoesDNSSetOnce)
        {
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Flushing DNS...{NL}", Color.LightGray));
            if (IsExiting)
                await Task.Run(async () => await FlushDnsOnExit());
            else
                await FlushDNS();
        }

        if (!IsExiting)
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Everything Disconnected.{NL}", Color.MediumSeaGreen));

        IsDisconnectingAll = false;
    }

    private async void Exit_Click(object? sender, EventArgs? e)
    {
        if (IsExiting) return;
        IsExiting = true;

        // Write Closing message to log
        string msg = "Exiting...";
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray));
        NotifyIconMain.BalloonTipText = msg;
        NotifyIconMain.ShowBalloonTip(500);

        // Disconnect All
        await DisconnectAll();

        // Set Close Stat
        AppClosedNormally(true);

        // Select Control type and properties to save
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Saving Settings...{NL}", Color.LightGray));
        AppSettings.AddSelectedControlAndProperty(typeof(CustomCheckBox), "Checked");
        AppSettings.AddSelectedControlAndProperty(typeof(CustomNumericUpDown), "Value");
        AppSettings.AddSelectedControlAndProperty(typeof(CustomRadioButton), "Checked");
        AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Text");
        AppSettings.AddSelectedControlAndProperty(typeof(CustomTextBox), "Texts");
        AppSettings.AddSelectedControlAndProperty(typeof(CustomComboBox), "SelectedIndex");

        // Add Settings to save
        AppSettings.AddSelectedSettings(this);

        // Save Application Settings
        await AppSettings.SaveAsync(SecureDNS.SettingsXmlPath);

        // Hide NotifyIcon
        NotifyIconMain.Visible = false;

        // Exit
        this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Goodbye.{NL}", Color.LightGray));
        Environment.Exit(0);
        Application.Exit();
    }
}