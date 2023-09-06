using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass.Themes;
using System;

namespace SecureDNSClient
{
    public partial class FormMain
    {
        private async void ShowMainContextMenu()
        {
            // Update bool IsGoodbyeDPIBasicActive
            IsGoodbyeDPIBasicActive = ProcessManager.FindProcessByPID(PIDGoodbyeDPIBasic);

            // Clear Items
            CustomContextMenuStripIcon.Items.Clear();

            // GoodbyeDPI Basic Menu
            ToolStripMenuItem goodbyeDpiBasic = new();
            goodbyeDpiBasic.Font = Font;
            goodbyeDpiBasic.Text = IsGoodbyeDPIBasicActive ? "Reactivate GoodbyeDPI Basic" : "Activate GoodbyeDPI Basic";
            goodbyeDpiBasic.Click -= GoodbyeDpiBasic_Click;
            goodbyeDpiBasic.Click += GoodbyeDpiBasic_Click;
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
            IsHttpProxyActivated = ProcessManager.FindProcessByPID(PIDHttpProxy);

            // HTTP Proxy Menu
            ToolStripMenuItem httpProxy = new();
            httpProxy.Font = Font;
            httpProxy.Text = IsHttpProxyActivating ? "Starting HTTP Proxy" : IsHttpProxyDeactivating ? "Stopping HTTP Proxy" : IsHttpProxyActivated ? "Stop HTTP Proxy" : "Start HTTP Proxy";
            httpProxy.Enabled = !IsHttpProxyActivating && !IsHttpProxyDeactivating && !IsDNSSetting && !IsDNSUnsetting;
            httpProxy.Click -= HttpProxy_Click;
            httpProxy.Click += HttpProxy_Click;
            CustomContextMenuStripIcon.Items.Add(httpProxy);

            // Set HTTP Proxy to System
            ToolStripMenuItem httpProxySet = new();
            httpProxySet.Font = Font;
            httpProxySet.Text = IsHttpProxySet ? "Unset Proxy from System" : "Set Proxy to System";
            httpProxySet.Enabled = IsHttpProxyActivated && !IsHttpProxyActivating && !IsHttpProxyDeactivating;
            httpProxySet.Click -= HttpProxySet_Click;
            httpProxySet.Click += HttpProxySet_Click;
            CustomContextMenuStripIcon.Items.Add(httpProxySet);

            // Spacer
            CustomContextMenuStripIcon.Items.Add("-");

            // Quick Connect DropDown Menu
            ToolStripMenuItem quickConnectTo = new();
            quickConnectTo.Font = Font;
            quickConnectTo.Text = "Quick Connect To";

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
            ToolStripMenuItem disconnect = new();
            disconnect.Font = Font;
            disconnect.Text = "Disconnect";
            disconnect.Enabled = IsConnected || IsConnecting || IsQuickConnecting;
            disconnect.Click -= Disconnect_Click;
            disconnect.Click += Disconnect_Click;
            CustomContextMenuStripIcon.Items.Add(disconnect);

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

        private void GoodbyeDpiBasic_Click(object? sender, EventArgs e)
        {
            GoodbyeDPIBasic();
        }

        private void GoodbyeDpiAdvanced_Click(object? sender, EventArgs e)
        {
            GoodbyeDPIAdvanced();
        }

        private void GoodbyeDpiDeactive_Click(object? sender, EventArgs e)
        {
            GoodbyeDPIDeactive(true, true);
        }

        private async void HttpProxy_Click(object? sender, EventArgs e)
        {
            await StartHttpProxy();
        }

        private void HttpProxySet_Click(object? sender, EventArgs e)
        {
            SetProxy();
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

        private async void Disconnect_Click(object? sender, EventArgs e)
        {
            if (IsQuickConnecting)
                await StartQuickConnect();
            else
                await QuickDisconnect(true, true, true);
        }

        private async void Exit_Click(object? sender, EventArgs e)
        {
            if (IsExiting) return;
            IsExiting = true;

            // Write Closing message to log
            string msg = "Exiting...";
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.LightGray));
            NotifyIconMain.BalloonTipText = msg;
            NotifyIconMain.ShowBalloonTip(500);

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
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating GoodbyeDPI Bypass...{NL}", Color.LightGray));
                ProcessManager.KillProcessByPID(PIDGoodbyeDPIBypass);

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
            }

            // Unset Proxy
            if (IsHttpProxySet)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Unsetting Proxy...{NL}", Color.LightGray));
                NetworkTool.UnsetProxy(false, true);

                // Wait
                Task wait = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (!IsHttpProxySet) break;
                        await Task.Delay(100);
                    }
                });
                try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
            }

            // Deactivate Proxy
            if (IsHttpProxyActivated || IsHttpProxyActivating)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating Proxy...{NL}", Color.LightGray));
                ProcessManager.KillProcessByPID(PIDHttpProxy);

                // Wait
                Task wait = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (!ProcessManager.FindProcessByPID(PIDHttpProxy)) break;
                        await Task.Delay(100);
                    }
                });
                try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
            }

            // Deactivate Fake Proxy
            if (ProcessManager.FindProcessByPID(PIDFakeHttpProxy))
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Deactivating Fake Proxy...{NL}", Color.LightGray));
                ProcessManager.KillProcessByPID(PIDFakeHttpProxy);

                // Wait
                Task wait = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (!ProcessManager.FindProcessByPID(PIDFakeHttpProxy)) break;
                        await Task.Delay(100);
                    }
                });
                try { await wait.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
            }

            // Unset DNS
            if (IsDNSSet || IsDNSSetting)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Unsetting DNS...{NL}", Color.LightGray));
                await Task.Run(() => UnsetSavedDNS());
                IsDNSSet = false;
            }

            // Disconnect -  Kill all processes
            if (IsConnected || IsConnecting)
            {
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Disconnecting...{NL}", Color.LightGray));
                await Task.Run(() => KillAll());

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
            }

            // Flush DNS On Exit
            this.InvokeIt(() => CustomRichTextBoxLog.AppendText($"Flushing DNS...{NL}", Color.LightGray));
            await Task.Run(() => FlushDnsOnExit());

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
}
