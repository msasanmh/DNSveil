using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass.Themes;
using System.Globalization;
using System.IO.Compression;

namespace SecureDNSClient;

public partial class FormMain
{
    // Status
    private void CustomButtonProcessMonitor_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormProcessMonitor)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormProcessMonitor formProcessMonitor = new();
        formProcessMonitor.StartPosition = FormStartPosition.Manual;
        formProcessMonitor.Location = new Point(MousePosition.X - formProcessMonitor.Width, MousePosition.Y - formProcessMonitor.Height);
        formProcessMonitor.Show();
    }

    private void CustomButtonBenchmark_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormBenchmark)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        string bootstrapIp = GetBootstrapSetting(out int bootstrapPort).ToString();

        FormBenchmark formBenchmark = new(bootstrapIp, bootstrapPort);
        formBenchmark.StartPosition = FormStartPosition.Manual;
        formBenchmark.Location = new Point(MousePosition.X - formBenchmark.Width, MousePosition.Y - formBenchmark.Height);
        formBenchmark.Show();
    }

    // Secure DNS -> Check
    private void CustomButtonEditCustomServers_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
        {
            TsiEdit.Font = Font;
            TsiEdit.Click -= tsiEdit_Click;
            TsiEdit.Click += tsiEdit_Click;
            void tsiEdit_Click(object? sender, EventArgs e) => edit();

            TsiViewWorkingServers.Font = Font;
            TsiViewWorkingServers.Click -= TsiViewWorkingServers_Click;
            TsiViewWorkingServers.Click += TsiViewWorkingServers_Click;
            void TsiViewWorkingServers_Click(object? sender, EventArgs e) => viewWorkingServers();

            TsiClearWorkingServers.Font = Font;
            TsiClearWorkingServers.Click -= TsiClearWorkingServers_Click;
            TsiClearWorkingServers.Click += TsiClearWorkingServers_Click;
            void TsiClearWorkingServers_Click(object? sender, EventArgs e) => clearWorkingServers();

            CMS.Font = Font;
            CMS.Items.Clear();
            CMS.Items.Add(TsiEdit);
            CMS.Items.Add(TsiViewWorkingServers);
            CMS.Items.Add(TsiClearWorkingServers);
            Theme.SetColors(CMS);
            CMS.RoundedCorners = 5;
            CMS.Show(CustomButtonEditCustomServers, 0, 0);

            void edit()
            {
                if (IsInAction(true, true, true, true, true, true, true, true, false, false, true)) return;

                FormCustomServers formCustomServers = new();
                formCustomServers.StartPosition = FormStartPosition.CenterParent;
                formCustomServers.ShowDialog(this);
            }

            void viewWorkingServers()
            {
                FileDirectory.CreateEmptyFile(SecureDNS.WorkingServersPath);
                int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.WorkingServersPath, false, false, SecureDNS.CurrentPath);
                if (notepad == -1)
                {
                    string msg = "Notepad is not installed on your system.";
                    CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
                }
            }

            void clearWorkingServers()
            {
                try
                {
                    File.Delete(SecureDNS.WorkingServersPath);
                    string msg = $"{NL}Working Servers Cleared.{NL}{NL}";
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg, Color.MediumSeaGreen));
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private async void CustomButtonCheck_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (IsInAction(true, true, true, false, true, true, true, true, true, false, true)) return;
            StartCheck(null);
        }
        else if (e.Button == MouseButtons.Right)
        {
            if (IsInAction(true, true, true, true, true, true, true, true, true, false, true)) return;
            List<string> groupList = await ReadCustomServersXmlGroups(SecureDNS.CustomServersXmlPath);
            ToolStripItem[] subMenuItems = new ToolStripItem[groupList.Count];

            // Add Built-In to DropDown Items
            TsiScanBuiltIn.Font = Font;
            TsiScanBuiltIn.Click -= scanBuiltIn_Click;
            TsiScanBuiltIn.Click += scanBuiltIn_Click;

            // Add Custom Groups to DropDown Items
            for (int n = 0; n < groupList.Count; n++)
            {
                string groupName = groupList[n];
                subMenuItems[n] = new ToolStripMenuItem(groupName);
                subMenuItems[n].Font = Font;
                subMenuItems[n].Name = groupName;
                subMenuItems[n].Click -= scanCustomGroup_Click;
                subMenuItems[n].Click += scanCustomGroup_Click;
            }

            CMS.Font = Font;
            CMS.Items.Clear();
            CMS.Items.Add(TsiScanBuiltIn);
            CMS.Items.AddRange(subMenuItems);
            Theme.SetColors(CMS);
            CMS.RoundedCorners = 5;
            CMS.Show(CustomButtonCheck, 0, 0);
        }

        void scanBuiltIn_Click(object? sender, EventArgs e)
        {
            if (IsExiting) return;
            StartCheck("builtin");
        }

        void scanCustomGroup_Click(object? sender, EventArgs e)
        {
            if (IsExiting) return;
            if (sender is ToolStripMenuItem tsmi) StartCheck(tsmi.Name);
        }
    }

    private async void CustomButtonQuickConnect_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            await StartQuickConnect(null);
        }
        else if (e.Button == MouseButtons.Right)
        {
            List<string> groupList = await ReadCustomServersXmlGroups(SecureDNS.CustomServersXmlPath);
            ToolStripItem[] subMenuItems = new ToolStripItem[groupList.Count];

            // Add Built-In to DropDown Items
            TsiQcToBuiltIn.Font = Font;
            TsiQcToBuiltIn.Click -= QcToBuiltIn_Click;
            TsiQcToBuiltIn.Click += QcToBuiltIn_Click;

            // Add Custom Groups to DropDown Items
            for (int n = 0; n < groupList.Count; n++)
            {
                string groupName = groupList[n];
                subMenuItems[n] = new ToolStripMenuItem(groupName);
                subMenuItems[n].Font = Font;
                subMenuItems[n].Name = groupName;
                subMenuItems[n].Click -= QcToCustomGroups_Click;
                subMenuItems[n].Click += QcToCustomGroups_Click;
            }

            CMS.Font = Font;
            CMS.Items.Clear();
            CMS.Items.Add(TsiQcToBuiltIn);
            CMS.Items.AddRange(subMenuItems);
            Theme.SetColors(CMS);
            CMS.RoundedCorners = 5;
            CMS.Show(CustomButtonQuickConnect, 0, 0);
        }
    }

    private async void CustomButtonDisconnectAll_Click(object sender, EventArgs e)
    {
        await DisconnectAll();
    }

    private async void CustomButtonCheckUpdate_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, false, false, false, false, false, false, false, false, true)) return;
        await CheckUpdate(true);
    }

    // Secure DNS -> Connect
    private async void CustomButtonWriteSavedServersDelay_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, false, true)) return;
        await WriteSavedServersDelayToLog();
    }

    private async void CustomButtonConnect_Click(object? sender, EventArgs? e)
    {
        if (IsInAction(true, true, true, true, false, false, true, true, true, true, true)) return;
        await StartConnect(GetConnectMode());
    }

    // Secure DNS -> Set DNS
    private void CustomButtonUpdateNICs_Click(object sender, EventArgs e)
    {
        // Update NICs
        SecureDNS.UpdateNICs(CustomComboBoxNICs, out _);
    }

    private async void CustomButtonEnableDisableNic_Click(object sender, EventArgs e)
    {
        if (CustomComboBoxNICs.SelectedItem == null) return;
        string? nicName = CustomComboBoxNICs.SelectedItem.ToString();
        if (string.IsNullOrEmpty(nicName)) return;

        // Disable Button
        CustomButtonEnableDisableNic.Enabled = false;

        if (CustomButtonEnableDisableNic.Text.Contains("Enable"))
        {
            CustomButtonEnableDisableNic.Text = "Enabling...";
            await NetworkTool.EnableNICAsync(nicName);
        }
        else
        {
            CustomButtonEnableDisableNic.Text = "Disabling...";
            await NetworkTool.DisableNICAsync(nicName);
        }

        // Update NIC Status
        await Task.Run(() => UpdateStatusNic());

        // Enable Button
        CustomButtonEnableDisableNic.Enabled = true;
    }

    private async void CustomButtonSetDNS_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, false, true)) return;
        await SetDNS();

        // Update NIC Status
        await Task.Run(() => UpdateStatusNic());
    }

    // Secure DNS -> Share + DPI Bypass
    private void CustomButtonPDpiPresetDefault_Click(object sender, EventArgs e)
    {
        CustomNumericUpDownPDpiBeforeSniChunks.Value = (decimal)50;
        CustomComboBoxPDpiSniChunkMode.SelectedIndex = 0;
        CustomNumericUpDownPDpiSniChunks.Value = (decimal)5;
        CustomNumericUpDownPDpiAntiPatternOffset.Value = (decimal)2;
        CustomNumericUpDownPDpiFragDelay.Value = (decimal)1;

        if (IsCheckingStarted) return;
        string msg1 = "Proxy DPI Bypass Mode: ";
        string msg2 = $"Default{NL}";
        CustomRichTextBoxLog.AppendText(msg1, Color.LightGray);
        CustomRichTextBoxLog.AppendText(msg2, Color.DodgerBlue);
    }

    private async void CustomButtonShare_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, true, true)) return;
        await StartProxy();
    }

    private void CustomButtonSetProxy_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, false, false, false, false, false, false, false, true, true, true)) return;
        SetProxy();
    }

    private async void CustomButtonPDpiApplyChanges_Click(object sender, EventArgs e)
    {
        this.InvokeIt(() => CustomButtonPDpiApplyChanges.Text = "Applying");
        UpdateProxyBools = false;
        if (ProcessManager.FindProcessByPID(PIDFakeProxy))
            await ApplyPDpiChangesFakeProxy();
        await ApplyPDpiChanges();
        UpdateProxyBools = true;

        await UpdateBoolProxy();
        UpdateApplyDpiBypassChangesButton();

        await UpdateBoolProxy();
        IsProxySet = UpdateBoolIsProxySet(out bool isAnotherProxySet, out string currentSystemProxy);
        IsAnotherProxySet = isAnotherProxySet;
        CurrentSystemProxy = currentSystemProxy;
        if (IsProxySet)
            await SetProxyInternalAsync(); // Change Proxy HTTP <==> SOCKS
        this.InvokeIt(() => CustomButtonPDpiApplyChanges.Text = "Apply DPI bypass changes");
    }

    private async void CustomButtonPDpiCheck_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, true, true)) return;
        // Get blocked domain
        string blockedDomain = GetBlockedDomainSetting(out _);
        await CheckDPIWorks(blockedDomain);
    }

    // Secure DNS -> GoodbyeDPI -> Basic
    private void CustomButtonDPIBasic_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, true, true)) return;
        // Activate/Reactivate GoodbyeDPI Basic
        GoodbyeDPIBasic();
    }

    private void CustomButtonDPIBasicDeactivate_Click(object sender, EventArgs e)
    {
        // Deactivate GoodbyeDPI Basic
        GoodbyeDPIDeactive(true, false);
    }

    // Secure DNS -> GoodbyeDPI -> Advanced
    private void CustomButtonDPIAdvBlacklist_Click(object sender, EventArgs e)
    {
        // Edit GoodbyeDPI Advanced Blacklist
        FileDirectory.CreateEmptyFile(SecureDNS.DPIBlacklistPath);
        int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.DPIBlacklistPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    private void CustomButtonDPIAdvActivate_Click(object sender, EventArgs e)
    {
        if (IsInAction(true, true, true, true, true, true, true, true, true, true, true)) return;
        // Activate/Reactivate GoodbyeDPI Advanced
        GoodbyeDPIAdvanced();
    }

    private void CustomButtonDPIAdvDeactivate_Click(object sender, EventArgs e)
    {
        // Deactivate GoodbyeDPI Advanced
        GoodbyeDPIDeactive(false, true);
    }

    // Tools
    private void CustomButtonToolsDnsScanner_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormDnsScanner)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormDnsScanner formDnsScanner = new();
        formDnsScanner.StartPosition = FormStartPosition.CenterParent;
        formDnsScanner.ShowDialog(this);
    }

    private void CustomButtonToolsDnsLookup_Click(object sender, EventArgs e)
    {
        FormDnsLookup formDnsLookup = new();
        formDnsLookup.StartPosition = FormStartPosition.Manual;
        formDnsLookup.Location = new Point(MousePosition.X + 50, MousePosition.Y - CustomButtonToolsDnsLookup.Top);
        formDnsLookup.Show(this);
    }

    private void CustomButtonToolsStampReader_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormStampReader)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormStampReader formStampReader = new();
        formStampReader.StartPosition = FormStartPosition.Manual;
        formStampReader.Location = new Point(MousePosition.X + 50, MousePosition.Y - CustomButtonToolsStampReader.Top);
        formStampReader.Show();
    }

    private void CustomButtonToolsStampGenerator_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormStampGenerator)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormStampGenerator formStampGenerator = new();
        formStampGenerator.StartPosition = FormStartPosition.Manual;
        formStampGenerator.Location = new Point(MousePosition.X + 50, MousePosition.Y - CustomButtonToolsStampGenerator.Top);
        formStampGenerator.Show();
    }

    private void CustomButtonToolsIpScanner_Click(object sender, EventArgs e)
    {
        // Check if it's already open
        Form f = Application.OpenForms[nameof(FormIpScanner)];
        if (f != null)
        {
            f.BringToFront();
            return;
        }

        FormIpScanner formIpScanner = new();
        formIpScanner.StartPosition = FormStartPosition.Manual;
        formIpScanner.Location = new Point(MousePosition.X + 50, MousePosition.Y - CustomButtonToolsIpScanner.Top);
        formIpScanner.Show();
    }

    private async void CustomButtonToolsFlushDns_Click(object sender, EventArgs e)
    {
        // Flush Dns
        if (IsInAction(true, false, true, true, true, true, true, true, true, false, true)) return;
        CustomButtonToolsFlushDns.Enabled = false;
        CustomButtonToolsFlushDns.Text = "Flushing...";
        await FlushDnsOnExit(true);
        if (!IsDNSSet) IsDnsFullFlushed = true;
        CustomButtonToolsFlushDns.Text = "Flush DNS";
        CustomButtonToolsFlushDns.Enabled = true;
    }

    // Settings -> Working Mode
    private void CustomButtonSettingUninstallCertificate_Click(object sender, EventArgs e)
    {
        UninstallCertificate();
    }

    // Settings -> Quick Connect
    private void CustomButtonSettingQcUpdateNics_Click(object sender, EventArgs e)
    {
        // Update NICs (Quick Connect Settings)
        SecureDNS.UpdateNICs(CustomComboBoxSettingQcNics, out _);
    }

    private void CustomButtonSettingQcStartup_Click(object sender, EventArgs e)
    {
        if (IsOnStartup)
        {
            if (IsStartupPathOk)
            {
                // Remove From Startup
                ActivateWindowsStartup(false);
                IsOnStartup = IsAppOnWindowsStartup(out bool isStartupPathOk);
                IsStartupPathOk = isStartupPathOk;
                if (!IsOnStartup)
                    CustomMessageBox.Show(this, "Successfully removed from Startup.", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    CustomMessageBox.Show(this, "Couldn't remove from Startup!", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Fix Startup Path
                ActivateWindowsStartup(true);
                IsOnStartup = IsAppOnWindowsStartup(out bool isStartupPathOk);
                IsStartupPathOk = isStartupPathOk;
                if (IsStartupPathOk)
                    CustomMessageBox.Show(this, "Successfully fixed Startup path.", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    CustomMessageBox.Show(this, "Couldn't fix Startup path!", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            // Apply to Startup
            ActivateWindowsStartup(true);
            IsOnStartup = IsAppOnWindowsStartup(out bool isStartupPathOk);
            IsStartupPathOk = isStartupPathOk;
            if (IsOnStartup)
                CustomMessageBox.Show(this, "Successfully applied to Startup.", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                CustomMessageBox.Show(this, "Couldn't apply to Startup!", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Settings -> Share -> Advanced
    private void CustomButtonSettingProxyFakeDNS_Click(object sender, EventArgs e)
    {
        FileDirectory.CreateEmptyFile(SecureDNS.FakeDnsRulesPath);
        int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.FakeDnsRulesPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    private void CustomButtonSettingProxyBlackWhiteList_Click(object sender, EventArgs e)
    {
        FileDirectory.CreateEmptyFile(SecureDNS.BlackWhiteListPath);
        int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.BlackWhiteListPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    private void CustomButtonSettingProxyDontBypass_Click(object sender, EventArgs e)
    {
        FileDirectory.CreateEmptyFile(SecureDNS.DontBypassListPath);
        int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.DontBypassListPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    // Settings -> Share -> SSL Decryption
    private void CustomButtonSettingProxyFakeSNI_Click(object sender, EventArgs e)
    {
        FileDirectory.CreateEmptyFile(SecureDNS.FakeSniRulesPath);
        int notepad = ProcessManager.ExecuteOnly("notepad", SecureDNS.FakeSniRulesPath, false, false, SecureDNS.CurrentPath);
        if (notepad == -1)
        {
            string msg = "Notepad is not installed on your system.";
            CustomRichTextBoxLog.AppendText(msg + NL, Color.IndianRed);
        }
    }

    // Settings -> Others
    private void CustomButtonSettingRestoreDefault_Click(object sender, EventArgs e)
    {
        if (IsCheckingStarted)
        {
            string msgChecking = "Stop check operation first." + NL;
            CustomRichTextBoxLog.AppendText(msgChecking, Color.IndianRed);
            return;
        }

        if (IsConnected)
        {
            string msgConnected = "Disconnect first." + NL;
            CustomRichTextBoxLog.AppendText(msgConnected, Color.IndianRed);
            return;
        }

        if (IsDNSSet)
        {
            string msgDNSIsSet = "Unset DNS first." + NL;
            CustomRichTextBoxLog.AppendText(msgDNSIsSet, Color.IndianRed);
            return;
        }

        DefaultSettings();

        string msgDefault = "Settings restored to default." + NL;
        CustomRichTextBoxLog.AppendText(msgDefault, Color.MediumSeaGreen);
    }

    private async void CustomButtonExportUserData_Click(object sender, EventArgs e)
    {
        using SaveFileDialog sfd = new();
        sfd.Filter = "SDC User Data (*.sdcud)|*.sdcud";
        sfd.DefaultExt = ".sdcud";
        sfd.AddExtension = true;
        sfd.RestoreDirectory = true;
        sfd.FileName = $"sdc_user_data_{DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss", CultureInfo.InvariantCulture)}";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                // Save Settings
                await SaveSettings();
                await Task.Delay(200);

                ZipFile.CreateFromDirectory(SecureDNS.UserDataDirPath, sfd.FileName);
                CustomMessageBox.Show(this, "Data exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void CustomButtonImportUserData_Click(object sender, EventArgs e)
    {
        using OpenFileDialog ofd = new();
        ofd.Filter = "SDC User Data (*.sdcud)|*.sdcud";
        ofd.DefaultExt = ".sdcud";
        ofd.AddExtension = true;
        ofd.RestoreDirectory = true;
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                ZipFile.ExtractToDirectory(ofd.FileName, SecureDNS.UserDataDirPath, true);
                await Task.Delay(1000);

                try
                {
                    // Load Settings
                    if (File.Exists(SecureDNS.SettingsXmlPath) && XmlTool.IsValidXMLFile(SecureDNS.SettingsXmlPath))
                        AppSettings = new(this, SecureDNS.SettingsXmlPath);
                    else
                        AppSettings = new(this);

                    string msg = "Data imported seccessfully.";
                    CustomMessageBox.Show(this, msg, "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception)
                {
                    string msg = "Failed importing user data.";
                    CustomMessageBox.Show(this, msg, "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}