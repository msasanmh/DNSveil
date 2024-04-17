using CustomControls;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;

namespace SecureDNSClient;

public partial class FormMain
{
    public async Task ScreenHighDpiScaleStartup(Form form)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        using Graphics g = form.CreateGraphics();
        int sdpi = Convert.ToInt32(Math.Round(g.DpiX, 0, MidpointRounding.AwayFromZero));
        float factor = sdpi / BaseScreenDpi;
        BaseScreenDpi = sdpi;
        await Task.Run(() => ScreenHighDpiScaleStartup(this, factor));
        ScreenFixControlsLocations(this); // Fix Controls Locations
    }

    public void ScreenHighDpiScaleStartup(Form form, float factor)
    {
        form.InvokeIt(() =>
        {
            List<Control> ctrls = Controllers.GetAllControls(form);
            for (int n = 0; n < ctrls.Count; n++)
            {
                Control c = ctrls[n];

                Font f = new(c.Font.Name, (float)(c.Font.Size * factor), c.Font.Style, c.Font.Unit, c.Font.GdiCharSet, c.Font.GdiVerticalFont);

                //c.Width = Math.Max(c.Width, TextRenderer.MeasureText(c.Text, f).Width + 10);

                if (c is CustomComboBox ccb)
                {
                    ccb.DropDownHeight = ccb.Items.Count * f.Height + 5;
                }

                if (c is CustomDataGridView cdgv)
                {
                    cdgv.ColumnHeadersDefaultCellStyle.Font = form.Font;
                    cdgv.DefaultCellStyle.Font = form.Font;
                }

                if (c is CustomLabel cl)
                {
                    if (cl.Name.Equals(CustomLabelShareInfo.Name) ||
                        cl.Name.Equals(CustomLabelInfoDPIModes.Name) ||
                        cl.Name.Equals(CustomLabelSettingFakeProxyInfo.Name) ||
                        cl.Name.Equals(CustomLabelAboutThis.Name))
                        cl.Font = f;
                }

                if (c is CustomListBox clb)
                {
                    int itemHeight = TextRenderer.MeasureText("MSasanMH", clb.Font).Height;
                    clb.ItemHeight = itemHeight * 2;
                }

                if (c is CustomTabControl ctc)
                {
                    if (!ctc.HideTabHeader)
                    {
                        ctc.SizeMode = TabSizeMode.Fixed; // Manual Size
                        int itemWidth = 0, itemHeight = 0;
                        foreach (TabPage tab in ctc.TabPages)
                        {
                            Size size = TextRenderer.MeasureText(tab.Text, tab.Font);
                            itemWidth = Math.Max(itemWidth, size.Width);
                            itemHeight = Math.Max(itemHeight, size.Height);
                        }

                        if (ctc.Alignment == TabAlignment.Top || ctc.Alignment == TabAlignment.Bottom)
                            ctc.ItemSize = new(itemWidth + 6, itemHeight + (itemHeight / 2)); // Width, Height
                        else
                            ctc.ItemSize = new(itemWidth + (itemHeight / 2), itemHeight + 6); // Height, Width
                    }
                }
            }
        });
    }

    public async void ScreenFixControlsLocations(Form form)
    {
        // Setting Width Of Controls
        await ScreenDPI.SettingWidthOfControls(form);

        // Don't use ComboBox Top, Bottom and Height Property!
        // Spacers
        int shw = TextRenderer.MeasureText("I", Font).Width;
        //Debug.WriteLine("=====> " + shw);
        int spaceBottom = 6, spaceRight = 6, spaceV, spaceH = shw, spaceH2 = spaceH * 2, spaceH3 = spaceH * 3, spaceHH = spaceH * 7;

        form.InvokeIt(() =>
        {
            // Containers
            CustomTabControlMain.Location = new Point(0, 0);
            CustomTabControlSecureDNS.Location = new Point(3, 3);
            CustomTabControlDPIBasicAdvanced.Location = new Point(3, 3);
            CustomTabControlSettings.Location = new Point(3, 3);
            CustomTabControlSettingProxy.Location = new Point(3, 3);

            // Status
            CustomButtonProcessMonitor.Location = new Point(spaceRight, 3);

            CustomButtonExit.Left = SplitContainerStatusMain.Panel2.Width - CustomButtonExit.Width - spaceRight;
            CustomButtonExit.Top = CustomButtonProcessMonitor.Top;

            try
            {
                SplitContainerStatusMain.Panel2MinSize = CustomButtonProcessMonitor.Height + 6;
                SplitContainerStatusMain.SplitterDistance = SplitContainerStatusMain.Height - SplitContainerStatusMain.Panel2MinSize;
            }
            catch (Exception) { }

            // Check
            spaceV = 10;
            CustomRadioButtonBuiltIn.Location = new Point(20, 20);

            CustomRadioButtonCustom.Left = CustomRadioButtonBuiltIn.Left;
            CustomRadioButtonCustom.Top = CustomRadioButtonBuiltIn.Bottom + spaceV;

            CustomLabelCustomServersInfo.Left = CustomRadioButtonCustom.Left + spaceH2;
            CustomLabelCustomServersInfo.Top = CustomRadioButtonCustom.Bottom + spaceV;

            CustomLabelCheckInParallel.Left = CustomRadioButtonCustom.Left;
            CustomLabelCheckInParallel.Top = CustomLabelCustomServersInfo.Bottom + spaceV;

            CustomNumericUpDownCheckInParallel.Left = CustomLabelCheckInParallel.Right + spaceH2;
            CustomNumericUpDownCheckInParallel.Top = CustomLabelCheckInParallel.Top - 2;

            CustomCheckBoxInsecure.Left = CustomLabelCheckInParallel.Left;
            CustomCheckBoxInsecure.Top = CustomNumericUpDownCheckInParallel.Bottom + spaceV;

            try
            {
                int sd = Math.Max(CustomLabelCustomServersInfo.Right, CustomCheckBoxInsecure.Right);
                SplitContainerCheckTop.Panel1MinSize = sd + spaceHH + spaceH3;
                SplitContainerCheckTop.SplitterDistance = SplitContainerCheckTop.Panel1MinSize;
            }
            catch (Exception) { }

            // ---------- Check Buttons
            CustomButtonEditCustomServers.Location = new Point(spaceRight, 3);

            CustomButtonCheck.Left = CustomButtonEditCustomServers.Right + spaceH;
            CustomButtonCheck.Top = CustomButtonEditCustomServers.Top;

            CustomButtonQuickConnect.Left = CustomButtonCheck.Right + spaceH;
            CustomButtonQuickConnect.Top = CustomButtonEditCustomServers.Top;

            CustomButtonDisconnectAll.Left = CustomButtonQuickConnect.Right + spaceH;
            CustomButtonDisconnectAll.Top = CustomButtonQuickConnect.Top;

            CustomProgressBarCheck.Left = CustomButtonDisconnectAll.Right + spaceH;
            CustomProgressBarCheck.Top = CustomButtonEditCustomServers.Top;

            CustomButtonCheckUpdate.Left = SplitContainerCheckMain.Panel2.Width - CustomButtonCheckUpdate.Width - spaceRight;
            CustomButtonCheckUpdate.Top = CustomButtonEditCustomServers.Top;

            CustomProgressBarCheck.Width = CustomButtonCheckUpdate.Left - CustomProgressBarCheck.Left - spaceH;

            try
            {
                SplitContainerCheckMain.Panel2MinSize = CustomButtonCheck.Height + 6;
                SplitContainerCheckMain.SplitterDistance = SplitContainerCheckMain.Height - SplitContainerCheckMain.Panel2MinSize;
            }
            catch (Exception) { }

            // Connect
            spaceV = 30;
            CustomRadioButtonConnectCheckedServers.Location = new Point(20, 20);

            CustomButtonWriteSavedServersDelay.Left = CustomRadioButtonConnectCheckedServers.Right + spaceH2;
            CustomButtonWriteSavedServersDelay.Top = CustomRadioButtonConnectCheckedServers.Top - spaceBottom;

            CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Left = CustomRadioButtonConnectCheckedServers.Left;
            CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Top = CustomButtonWriteSavedServersDelay.Bottom + spaceV;

            CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Left = CustomRadioButtonConnectCheckedServers.Left;
            CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Top = CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Bottom + spaceV;

            CustomRadioButtonConnectDNSCrypt.Left = CustomRadioButtonConnectCheckedServers.Left;
            CustomRadioButtonConnectDNSCrypt.Top = CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Bottom + spaceV;

            spaceV = 5;
            CustomTextBoxHTTPProxy.Left = CustomRadioButtonConnectDNSCrypt.Left + spaceH2;
            CustomTextBoxHTTPProxy.Top = CustomRadioButtonConnectDNSCrypt.Bottom + spaceV;
            CustomTextBoxHTTPProxy.Width = CustomRadioButtonConnectDNSCrypt.Width - spaceH2;

            try
            {
                int sd = Math.Max(CustomButtonWriteSavedServersDelay.Right, CustomTextBoxHTTPProxy.Right);
                SplitContainerConnectTop.Panel1MinSize = sd + spaceHH;
                SplitContainerConnectTop.SplitterDistance = SplitContainerConnectTop.Panel1MinSize;
            }
            catch (Exception) { }

            // ---------- Connect Buttons
            CustomButtonConnect.Location = new Point(spaceHH, 3);

            CustomButtonReconnect.Left = CustomButtonConnect.Right + spaceH;
            CustomButtonReconnect.Top = CustomButtonConnect.Top;

            try
            {
                SplitContainerConnectMain.Panel2MinSize = CustomButtonConnect.Height + 6;
                SplitContainerConnectMain.SplitterDistance = SplitContainerConnectMain.Height - SplitContainerConnectMain.Panel2MinSize;
            }
            catch (Exception) { }

            // Set Dns
            try
            {
                SplitContainerSetDnsTop.Panel1MinSize = CustomButtonUpdateNICs.Right + CustomButtonUpdateNICs.Width + (spaceHH / 2);
                SplitContainerSetDnsTop.SplitterDistance = SplitContainerSetDnsTop.Panel1MinSize;
            }
            catch (Exception) { }

            // ---------- Set DNS Buttons
            CustomButtonSetDNS.Location = new Point(spaceHH, 3);

            CustomButtonUnsetAllDNSs.Top = CustomButtonSetDNS.Top;
            CustomButtonUnsetAllDNSs.Left = CustomButtonSetDNS.Right + spaceH;

            try
            {
                SplitContainerSetDnsMain.Panel2MinSize = CustomButtonSetDNS.Height + 6;
                SplitContainerSetDnsMain.SplitterDistance = SplitContainerSetDnsMain.Height - SplitContainerSetDnsMain.Panel2MinSize;
            }
            catch (Exception) { }

            //// Share
            spaceV = 5;
            CustomLabelShareInfo.Location = new Point(25, 10);

            CustomCheckBoxProxyEventShowRequest.Left = CustomLabelShareInfo.Left;
            CustomCheckBoxProxyEventShowRequest.Top = CustomLabelShareInfo.Bottom + spaceV;

            CustomCheckBoxPDpiEnableFragment.Left = CustomCheckBoxProxyEventShowRequest.Left;
            CustomCheckBoxPDpiEnableFragment.Top = CustomCheckBoxProxyEventShowRequest.Bottom + spaceV;

            CustomCheckBoxProxyEnableSSL.Left = CustomCheckBoxPDpiEnableFragment.Left;
            CustomCheckBoxProxyEnableSSL.Top = CustomCheckBoxPDpiEnableFragment.Bottom + spaceV;

            CustomLabelProxySSLInfo.Left = CustomCheckBoxProxyEnableSSL.Right + spaceH;
            CustomLabelProxySSLInfo.Top = CustomCheckBoxProxyEnableSSL.Top;

            try
            {
                SplitContainerShareContent.Panel1MinSize = CustomLabelProxySSLInfo.Bottom + 3;
                SplitContainerShareContent.SplitterDistance = SplitContainerShareContent.Panel1MinSize;
            }
            catch (Exception) { }

            // ---------- Fragment Options
            spaceV = 12;
            CustomLabelPDpiBeforeSniChunks.Location = new Point(spaceRight, spaceRight);

            CustomNumericUpDownPDpiBeforeSniChunks.Left = CustomLabelPDpiBeforeSniChunks.Right + (spaceHH / 2);
            CustomNumericUpDownPDpiBeforeSniChunks.Top = CustomLabelPDpiBeforeSniChunks.Top - 2;

            CustomLabelPDpiSniChunkMode.Left = CustomLabelPDpiBeforeSniChunks.Left;
            CustomLabelPDpiSniChunkMode.Top = CustomLabelPDpiBeforeSniChunks.Bottom + spaceV;

            CustomComboBoxPDpiSniChunkMode.Left = CustomNumericUpDownPDpiBeforeSniChunks.Left;
            CustomComboBoxPDpiSniChunkMode.Top = CustomLabelPDpiSniChunkMode.Top - 2;

            CustomLabelPDpiSniChunks.Left = CustomLabelPDpiSniChunkMode.Left;
            CustomLabelPDpiSniChunks.Top = CustomLabelPDpiSniChunkMode.Bottom + spaceV;

            CustomNumericUpDownPDpiSniChunks.Left = CustomComboBoxPDpiSniChunkMode.Left;
            CustomNumericUpDownPDpiSniChunks.Top = CustomLabelPDpiSniChunks.Top - 2;

            CustomLabelPDpiAntiPatternOffset.Left = CustomLabelPDpiSniChunks.Left;
            CustomLabelPDpiAntiPatternOffset.Top = CustomLabelPDpiSniChunks.Bottom + spaceV;

            CustomNumericUpDownPDpiAntiPatternOffset.Left = CustomNumericUpDownPDpiSniChunks.Left;
            CustomNumericUpDownPDpiAntiPatternOffset.Top = CustomLabelPDpiAntiPatternOffset.Top - 2;

            CustomLabelPDpiFragDelay.Left = CustomLabelPDpiAntiPatternOffset.Left;
            CustomLabelPDpiFragDelay.Top = CustomLabelPDpiAntiPatternOffset.Bottom + spaceV;

            CustomNumericUpDownPDpiFragDelay.Left = CustomNumericUpDownPDpiAntiPatternOffset.Left;
            CustomNumericUpDownPDpiFragDelay.Top = CustomLabelPDpiFragDelay.Top - 2;

            CustomLabelPDpiPresets.Left = CustomComboBoxPDpiSniChunkMode.Right + spaceH3;
            CustomLabelPDpiPresets.Top = CustomLabelPDpiBeforeSniChunks.Top;

            CustomButtonPDpiPresetDefault.Left = CustomLabelPDpiPresets.Left + spaceH;
            CustomButtonPDpiPresetDefault.Top = CustomLabelPDpiPresets.Bottom + spaceV;

            CustomCheckBoxProxyEventShowChunkDetails.Left = CustomLabelPDpiPresets.Left;
            CustomCheckBoxProxyEventShowChunkDetails.Top = CustomLabelPDpiFragDelay.Top;

            // ---------- SSL Decryption Options
            CustomCheckBoxProxySSLChangeSni.Location = new Point(spaceRight, spaceRight);

            spaceV = 5;
            CustomLabelProxySSLChangeSniInfo.Left = CustomCheckBoxProxySSLChangeSni.Left + spaceH2;
            CustomLabelProxySSLChangeSniInfo.Top = CustomCheckBoxProxySSLChangeSni.Bottom + spaceV;

            spaceV = 12;
            CustomLabelProxySSLDefaultSni.Left = CustomCheckBoxProxySSLChangeSni.Left;
            CustomLabelProxySSLDefaultSni.Top = CustomLabelProxySSLChangeSniInfo.Bottom + spaceV;

            CustomTextBoxProxySSLDefaultSni.Left = CustomLabelProxySSLDefaultSni.Right + spaceH;
            CustomTextBoxProxySSLDefaultSni.Top = CustomLabelProxySSLDefaultSni.Top - 2;

            // ---------- Proxy Read ProxyRules For A Domain
            CustomLabelShareRulesStatus.Location = new Point(4, 2);

            spaceV = 5;
            CustomTextBoxShareRulesStatusDomain.Left = CustomLabelShareRulesStatus.Left;
            CustomTextBoxShareRulesStatusDomain.Top = CustomLabelShareRulesStatus.Bottom + spaceV;
            CustomTextBoxShareRulesStatusDomain.Width = SplitContainerShareTop.Panel2MinSize - 12;

            try
            {
                SplitContainerShareRulesStatus1.Panel1MinSize = CustomTextBoxShareRulesStatusDomain.Bottom + spaceV;
                SplitContainerShareRulesStatus1.SplitterDistance = SplitContainerShareRulesStatus1.Panel1MinSize;
            }
            catch (Exception) { }

            CustomButtonShareRulesStatusRead.Location = new Point(3, 3);

            try
            {
                SplitContainerShareRulesStatus2.Panel2MinSize = CustomButtonShareRulesStatusRead.Height;
                SplitContainerShareRulesStatus2.SplitterDistance = SplitContainerShareRulesStatus2.Height - SplitContainerShareRulesStatus2.Panel2MinSize;
            }
            catch (Exception) { }

            CustomButtonShareRulesStatusRead.Dock = DockStyle.Fill;

            // ---------- Share Buttons
            CustomButtonShare.Location = new Point(spaceHH, 3);

            CustomButtonSetProxy.Left = CustomButtonShare.Right + spaceH;
            CustomButtonSetProxy.Top = CustomButtonShare.Top;

            CustomButtonPDpiApplyChanges.Left = CustomButtonSetProxy.Right + spaceHH;
            CustomButtonPDpiApplyChanges.Top = CustomButtonShare.Top;

            CustomButtonPDpiCheck.Left = CustomButtonPDpiApplyChanges.Right + spaceH;
            CustomButtonPDpiCheck.Top = CustomButtonShare.Top;

            try
            {
                SplitContainerShareMain.Panel2MinSize = CustomButtonShare.Height + 6;
                SplitContainerShareMain.SplitterDistance = SplitContainerShareMain.Height - SplitContainerShareMain.Panel2MinSize;
            }
            catch (Exception) { }

            //// GoodbyeDPI Basic
            spaceV = 12;
            CustomLabelInfoDPIModes.Location = new Point(20, 20);

            CustomLabelDPIModes.Left = CustomLabelInfoDPIModes.Left;
            CustomLabelDPIModes.Top = CustomLabelInfoDPIModes.Bottom + spaceV;

            CustomRadioButtonDPIModeLight.Left = CustomLabelInfoDPIModes.Left;
            CustomRadioButtonDPIModeLight.Top = CustomLabelDPIModes.Bottom + spaceV;

            CustomRadioButtonDPIModeMedium.Left = CustomRadioButtonDPIModeLight.Right + spaceH;
            CustomRadioButtonDPIModeMedium.Top = CustomRadioButtonDPIModeLight.Top;

            CustomRadioButtonDPIModeHigh.Left = CustomRadioButtonDPIModeMedium.Right + spaceH;
            CustomRadioButtonDPIModeHigh.Top = CustomRadioButtonDPIModeMedium.Top;

            CustomRadioButtonDPIModeExtreme.Left = CustomRadioButtonDPIModeHigh.Right + spaceH;
            CustomRadioButtonDPIModeExtreme.Top = CustomRadioButtonDPIModeHigh.Top;

            CustomLabelSSLFragmentSize.Left = CustomRadioButtonDPIModeExtreme.Right + spaceHH;
            CustomLabelSSLFragmentSize.Top = CustomRadioButtonDPIModeExtreme.Top;

            CustomNumericUpDownSSLFragmentSize.Left = CustomLabelSSLFragmentSize.Right + spaceH2;
            CustomNumericUpDownSSLFragmentSize.Top = CustomLabelSSLFragmentSize.Top - 2;

            CustomLabelDPIModesGoodbyeDPI.Left = CustomLabelInfoDPIModes.Left;
            CustomLabelDPIModesGoodbyeDPI.Top = CustomNumericUpDownSSLFragmentSize.Bottom + spaceV;

            CustomRadioButtonDPIMode1.Left = CustomLabelDPIModesGoodbyeDPI.Left;
            CustomRadioButtonDPIMode1.Top = CustomLabelDPIModesGoodbyeDPI.Bottom + spaceV;

            CustomRadioButtonDPIMode2.Left = CustomRadioButtonDPIMode1.Right + spaceH;
            CustomRadioButtonDPIMode2.Top = CustomRadioButtonDPIMode1.Top;

            CustomRadioButtonDPIMode3.Left = CustomRadioButtonDPIMode2.Right + spaceH;
            CustomRadioButtonDPIMode3.Top = CustomRadioButtonDPIMode2.Top;

            CustomRadioButtonDPIMode4.Left = CustomRadioButtonDPIMode3.Right + spaceH;
            CustomRadioButtonDPIMode4.Top = CustomRadioButtonDPIMode3.Top;

            CustomRadioButtonDPIMode5.Left = CustomRadioButtonDPIMode4.Right + spaceH;
            CustomRadioButtonDPIMode5.Top = CustomRadioButtonDPIMode4.Top;

            CustomRadioButtonDPIMode6.Left = CustomRadioButtonDPIMode5.Right + spaceH;
            CustomRadioButtonDPIMode6.Top = CustomRadioButtonDPIMode5.Top;

            // ---------- GoodbyeDPI Basic Buttons
            CustomButtonDPIBasicActivate.Location = new Point(spaceHH, 3);

            CustomButtonDPIBasicDeactivate.Left = CustomButtonDPIBasicActivate.Right + spaceH;
            CustomButtonDPIBasicDeactivate.Top = CustomButtonDPIBasicActivate.Top;

            try
            {
                SplitContainerGoodbyeDpiBasicMain.Panel2MinSize = CustomButtonDPIBasicActivate.Height + 6;
                SplitContainerGoodbyeDpiBasicMain.SplitterDistance = SplitContainerGoodbyeDpiBasicMain.Height - SplitContainerGoodbyeDpiBasicMain.Panel2MinSize;
            }
            catch (Exception) { }

            //// GoodbyeDPI Advanced
            CustomCheckBoxDPIAdvP.Location = new Point(5, 5);

            CustomCheckBoxDPIAdvF.Left = CustomCheckBoxDPIAdvP.Left;
            CustomCheckBoxDPIAdvF.Top = CustomCheckBoxDPIAdvP.Bottom + spaceV;

            CustomNumericUpDownDPIAdvF.Left = CustomCheckBoxDPIAdvF.Right + spaceH;
            CustomNumericUpDownDPIAdvF.Top = CustomCheckBoxDPIAdvF.Top - 2;

            CustomCheckBoxDPIAdvA.Left = CustomCheckBoxDPIAdvF.Left;
            CustomCheckBoxDPIAdvA.Top = CustomCheckBoxDPIAdvF.Bottom + spaceV;

            CustomCheckBoxDPIAdvAllowNoSNI.Left = CustomCheckBoxDPIAdvA.Left;
            CustomCheckBoxDPIAdvAllowNoSNI.Top = CustomCheckBoxDPIAdvA.Bottom + spaceV;

            CustomCheckBoxDPIAdvWrongChksum.Left = CustomCheckBoxDPIAdvAllowNoSNI.Left;
            CustomCheckBoxDPIAdvWrongChksum.Top = CustomCheckBoxDPIAdvAllowNoSNI.Bottom + spaceV;

            CustomCheckBoxDPIAdvMaxPayload.Left = CustomCheckBoxDPIAdvWrongChksum.Left;
            CustomCheckBoxDPIAdvMaxPayload.Top = CustomCheckBoxDPIAdvWrongChksum.Bottom + spaceV;

            CustomNumericUpDownDPIAdvMaxPayload.Left = CustomCheckBoxDPIAdvMaxPayload.Right + spaceH;
            CustomNumericUpDownDPIAdvMaxPayload.Top = CustomCheckBoxDPIAdvMaxPayload.Top - 2;

            CustomCheckBoxDPIAdvR.Left = CustomNumericUpDownDPIAdvMaxPayload.Right + (spaceHH / 2);
            CustomCheckBoxDPIAdvR.Top = CustomCheckBoxDPIAdvP.Top;

            CustomCheckBoxDPIAdvK.Left = CustomCheckBoxDPIAdvR.Left;
            CustomCheckBoxDPIAdvK.Top = CustomCheckBoxDPIAdvR.Bottom + spaceV;

            CustomNumericUpDownDPIAdvK.Left = CustomCheckBoxDPIAdvK.Right + spaceH;
            CustomNumericUpDownDPIAdvK.Top = CustomCheckBoxDPIAdvK.Top - 2;

            CustomCheckBoxDPIAdvW.Left = CustomCheckBoxDPIAdvK.Left;
            CustomCheckBoxDPIAdvW.Top = CustomCheckBoxDPIAdvK.Bottom + spaceV;

            CustomCheckBoxDPIAdvSetTTL.Left = CustomCheckBoxDPIAdvW.Left;
            CustomCheckBoxDPIAdvSetTTL.Top = CustomCheckBoxDPIAdvW.Bottom + spaceV;

            CustomNumericUpDownDPIAdvSetTTL.Left = CustomCheckBoxDPIAdvSetTTL.Right + spaceH;
            CustomNumericUpDownDPIAdvSetTTL.Top = CustomCheckBoxDPIAdvSetTTL.Top - 2;

            CustomCheckBoxDPIAdvWrongSeq.Left = CustomCheckBoxDPIAdvSetTTL.Left;
            CustomCheckBoxDPIAdvWrongSeq.Top = CustomCheckBoxDPIAdvSetTTL.Bottom + spaceV;

            CustomCheckBoxDPIAdvBlacklist.Left = CustomCheckBoxDPIAdvWrongSeq.Left;
            CustomCheckBoxDPIAdvBlacklist.Top = CustomCheckBoxDPIAdvWrongSeq.Bottom + spaceV;

            CustomButtonDPIAdvBlacklist.Left = CustomCheckBoxDPIAdvBlacklist.Right + spaceH;
            CustomButtonDPIAdvBlacklist.Top = CustomCheckBoxDPIAdvBlacklist.Top - 2;

            CustomCheckBoxDPIAdvS.Left = CustomButtonDPIAdvBlacklist.Right + (spaceHH / 2);
            CustomCheckBoxDPIAdvS.Top = CustomCheckBoxDPIAdvP.Top;

            CustomCheckBoxDPIAdvN.Left = CustomCheckBoxDPIAdvS.Left;
            CustomCheckBoxDPIAdvN.Top = CustomCheckBoxDPIAdvS.Bottom + spaceV;

            CustomCheckBoxDPIAdvPort.Left = CustomCheckBoxDPIAdvN.Left;
            CustomCheckBoxDPIAdvPort.Top = CustomCheckBoxDPIAdvN.Bottom + spaceV;

            CustomNumericUpDownDPIAdvPort.Left = CustomCheckBoxDPIAdvPort.Right + spaceH;
            CustomNumericUpDownDPIAdvPort.Top = CustomCheckBoxDPIAdvPort.Top - 2;

            CustomCheckBoxDPIAdvAutoTTL.Left = CustomCheckBoxDPIAdvPort.Left;
            CustomCheckBoxDPIAdvAutoTTL.Top = CustomCheckBoxDPIAdvPort.Bottom + spaceV;

            CustomTextBoxDPIAdvAutoTTL.Left = CustomCheckBoxDPIAdvAutoTTL.Right + spaceH;
            CustomTextBoxDPIAdvAutoTTL.Top = CustomCheckBoxDPIAdvAutoTTL.Top - 2;

            CustomCheckBoxDPIAdvNativeFrag.Left = CustomCheckBoxDPIAdvAutoTTL.Left;
            CustomCheckBoxDPIAdvNativeFrag.Top = CustomCheckBoxDPIAdvAutoTTL.Bottom + spaceV;

            CustomCheckBoxDPIAdvM.Left = CustomTextBoxDPIAdvAutoTTL.Right + (spaceHH / 2);
            CustomCheckBoxDPIAdvM.Top = CustomCheckBoxDPIAdvP.Top;

            CustomCheckBoxDPIAdvE.Left = CustomCheckBoxDPIAdvM.Left;
            CustomCheckBoxDPIAdvE.Top = CustomCheckBoxDPIAdvM.Bottom + spaceV;

            CustomNumericUpDownDPIAdvE.Left = CustomCheckBoxDPIAdvE.Right + spaceH;
            CustomNumericUpDownDPIAdvE.Top = CustomCheckBoxDPIAdvE.Top - 2;

            CustomCheckBoxDPIAdvIpId.Left = CustomCheckBoxDPIAdvE.Left;
            CustomCheckBoxDPIAdvIpId.Top = CustomCheckBoxDPIAdvE.Bottom + spaceV;

            CustomTextBoxDPIAdvIpId.Left = CustomCheckBoxDPIAdvIpId.Right + spaceH;
            CustomTextBoxDPIAdvIpId.Top = CustomCheckBoxDPIAdvIpId.Top - 2;

            CustomCheckBoxDPIAdvMinTTL.Left = CustomCheckBoxDPIAdvIpId.Left;
            CustomCheckBoxDPIAdvMinTTL.Top = CustomCheckBoxDPIAdvIpId.Bottom + spaceV;

            CustomNumericUpDownDPIAdvMinTTL.Left = CustomCheckBoxDPIAdvMinTTL.Right + spaceH;
            CustomNumericUpDownDPIAdvMinTTL.Top = CustomCheckBoxDPIAdvMinTTL.Top - 2;

            CustomCheckBoxDPIAdvReverseFrag.Left = CustomCheckBoxDPIAdvMinTTL.Left;
            CustomCheckBoxDPIAdvReverseFrag.Top = CustomCheckBoxDPIAdvMinTTL.Bottom + spaceH;

            // ---------- GoodbyeDPI Advanced Buttons
            CustomButtonDPIAdvActivate.Location = new Point(spaceHH, 3);

            CustomButtonDPIAdvDeactivate.Left = CustomButtonDPIAdvActivate.Right + spaceH;
            CustomButtonDPIAdvDeactivate.Top = CustomButtonDPIAdvActivate.Top;

            try
            {
                SplitContainerGoodbyeDpiAdvancedMain.Panel2MinSize = CustomButtonDPIAdvActivate.Height + 6;
                SplitContainerGoodbyeDpiAdvancedMain.SplitterDistance = SplitContainerGoodbyeDpiAdvancedMain.Height - SplitContainerGoodbyeDpiAdvancedMain.Panel2MinSize;
            }
            catch (Exception) { }

            //// Tools
            spaceV = 30;
            // ---------- Col 1
            CustomButtonToolsDnsScanner.Location = new Point(50, 50);

            CustomButtonToolsDnsLookup.Left = CustomButtonToolsDnsScanner.Left;
            CustomButtonToolsDnsLookup.Top = CustomButtonToolsDnsScanner.Bottom + spaceV;

            CustomButtonToolsStampReader.Left = CustomButtonToolsDnsLookup.Left;
            CustomButtonToolsStampReader.Top = CustomButtonToolsDnsLookup.Bottom + spaceV;

            CustomButtonToolsStampGenerator.Left = CustomButtonToolsStampReader.Left;
            CustomButtonToolsStampGenerator.Top = CustomButtonToolsStampReader.Bottom + spaceV;

            CustomButtonToolsIpScanner.Left = CustomButtonToolsStampGenerator.Left;
            CustomButtonToolsIpScanner.Top = CustomButtonToolsStampGenerator.Bottom + spaceV;

            // ---------- Col 2
            CustomButtonToolsFlushDns.Left = CustomButtonToolsDnsScanner.Right + spaceHH;
            CustomButtonToolsFlushDns.Top = CustomButtonToolsDnsScanner.Top;

            CustomButtonBenchmark.Left = CustomButtonToolsFlushDns.Left;
            CustomButtonBenchmark.Top = CustomButtonToolsFlushDns.Bottom + spaceV;

            //// Settings
            int settingsMenuWidth = TextRenderer.MeasureText("MSMHSecureDNSClient", Font).Width;
            try
            {
                SplitContainerSettings.Panel1MinSize = settingsMenuWidth;
                SplitContainerSettings.SplitterDistance = SplitContainerSettings.Panel1MinSize;
            }
            catch (Exception) { }

            //// Settings Working Mode
            spaceV = 30;
            CustomLabelSettingInfoWorkingMode1.Location = new Point(50, 35);

            CustomRadioButtonSettingWorkingModeDNS.Left = CustomLabelSettingInfoWorkingMode1.Left;
            CustomRadioButtonSettingWorkingModeDNS.Top = CustomLabelSettingInfoWorkingMode1.Bottom + spaceV;

            CustomRadioButtonSettingWorkingModeDNSandDoH.Left = CustomRadioButtonSettingWorkingModeDNS.Left;
            CustomRadioButtonSettingWorkingModeDNSandDoH.Top = CustomRadioButtonSettingWorkingModeDNS.Bottom + spaceV;

            CustomLabelSettingWorkingModeSetDohPort.Left = CustomRadioButtonSettingWorkingModeDNSandDoH.Left + spaceH2;
            CustomLabelSettingWorkingModeSetDohPort.Top = CustomRadioButtonSettingWorkingModeDNSandDoH.Bottom + (spaceV / 2);

            CustomNumericUpDownSettingWorkingModeSetDohPort.Left = CustomLabelSettingWorkingModeSetDohPort.Right + spaceH2;
            CustomNumericUpDownSettingWorkingModeSetDohPort.Top = CustomLabelSettingWorkingModeSetDohPort.Top - 2;

            CustomButtonSettingUninstallCertificate.Left = CustomRadioButtonSettingWorkingModeDNSandDoH.Left;
            CustomButtonSettingUninstallCertificate.Top = CustomLabelSettingWorkingModeSetDohPort.Bottom + spaceV;

            //// Settings Check
            spaceV = 20;
            CustomLabelSettingCheckTimeout.Location = new Point(15, 25);

            CustomNumericUpDownSettingCheckTimeout.Left = CustomLabelSettingCheckTimeout.Right + spaceH2;
            CustomNumericUpDownSettingCheckTimeout.Top = CustomLabelSettingCheckTimeout.Top - 2;

            CustomCheckBoxSettingCheckClearWorkingServers.Left = CustomLabelSettingCheckTimeout.Left;
            CustomCheckBoxSettingCheckClearWorkingServers.Top = CustomNumericUpDownSettingCheckTimeout.Bottom + spaceV;

            CustomLabelSettingCheckDPIInfo.Left = CustomCheckBoxSettingCheckClearWorkingServers.Left;
            CustomLabelSettingCheckDPIInfo.Top = CustomCheckBoxSettingCheckClearWorkingServers.Bottom + spaceV;

            CustomTextBoxSettingCheckDPIHost.Left = CustomLabelSettingCheckDPIInfo.Right + spaceH2;
            CustomTextBoxSettingCheckDPIHost.Top = CustomLabelSettingCheckDPIInfo.Top - 2;

            CustomGroupBoxSettingCheckDnsProtocol.Left = spaceRight;
            CustomGroupBoxSettingCheckDnsProtocol.Top = CustomLabelSettingCheckDPIInfo.Bottom + spaceV;
            int settingsTabPageWidth = TabPageSettingsCheck.Width - (spaceRight * 2);
            if (settingsTabPageWidth >= CustomTabControlSettings.Width - (spaceRight * 6))
                settingsTabPageWidth -= CustomTabControlSettings.Width - TabPageSettingsCheck.Width;
            CustomGroupBoxSettingCheckDnsProtocol.Width = settingsTabPageWidth;

            CustomCheckBoxSettingProtocolDoH.Location = new Point(15, 25);

            spaceV = 20;
            CustomCheckBoxSettingProtocolDNSCryptRelay.Left = CustomCheckBoxSettingProtocolDoH.Left;
            CustomCheckBoxSettingProtocolDNSCryptRelay.Top = CustomCheckBoxSettingProtocolDoH.Bottom + spaceV;

            CustomCheckBoxSettingProtocolTLS.Left = CustomCheckBoxSettingProtocolDNSCryptRelay.Right + spaceHH;
            CustomCheckBoxSettingProtocolTLS.Top = CustomCheckBoxSettingProtocolDoH.Top;

            CustomCheckBoxSettingProtocolDoQ.Left = CustomCheckBoxSettingProtocolTLS.Left;
            CustomCheckBoxSettingProtocolDoQ.Top = CustomCheckBoxSettingProtocolTLS.Bottom + spaceV;

            CustomCheckBoxSettingProtocolDNSCrypt.Left = CustomCheckBoxSettingProtocolDoQ.Right + spaceHH;
            CustomCheckBoxSettingProtocolDNSCrypt.Top = CustomCheckBoxSettingProtocolTLS.Top;

            CustomCheckBoxSettingProtocolPlainDNS.Left = CustomCheckBoxSettingProtocolDNSCrypt.Left;
            CustomCheckBoxSettingProtocolPlainDNS.Top = CustomCheckBoxSettingProtocolDNSCrypt.Bottom + spaceV;

            CustomGroupBoxSettingCheckDnsProtocol.Height = CustomCheckBoxSettingProtocolPlainDNS.Bottom + CustomCheckBoxSettingProtocolDoH.Top - spaceBottom;

            spaceV = 10;
            CustomGroupBoxSettingCheckSDNS.Left = CustomGroupBoxSettingCheckDnsProtocol.Left;
            CustomGroupBoxSettingCheckSDNS.Top = CustomGroupBoxSettingCheckDnsProtocol.Bottom + spaceV;
            CustomGroupBoxSettingCheckSDNS.Width = CustomGroupBoxSettingCheckDnsProtocol.Width;

            CustomCheckBoxSettingSdnsDNSSec.Location = new Point(15, 25);

            CustomCheckBoxSettingSdnsNoLog.Left = CustomCheckBoxSettingProtocolDoQ.Left;
            CustomCheckBoxSettingSdnsNoLog.Top = CustomCheckBoxSettingSdnsDNSSec.Top;

            CustomCheckBoxSettingSdnsNoFilter.Left = CustomCheckBoxSettingProtocolPlainDNS.Left;
            CustomCheckBoxSettingSdnsNoFilter.Top = CustomCheckBoxSettingSdnsNoLog.Top;

            CustomGroupBoxSettingCheckSDNS.Height = CustomCheckBoxSettingSdnsNoFilter.Bottom + CustomCheckBoxSettingSdnsNoFilter.Top - spaceBottom;

            //// Settings Quick Connect
            spaceV = 20;
            CustomLabelSettingQcInfo.Location = new Point(20, 10);

            CustomLabelSettingQcConnectMode.Left = CustomLabelSettingQcInfo.Left;
            CustomLabelSettingQcConnectMode.Top = CustomLabelSettingQcInfo.Bottom + spaceV;

            CustomComboBoxSettingQcConnectMode.Left = CustomLabelSettingQcConnectMode.Right + (spaceH * 4);
            CustomComboBoxSettingQcConnectMode.Top = CustomLabelSettingQcConnectMode.Top - 2;

            CustomCheckBoxSettingQcUseSavedServers.Left = CustomComboBoxSettingQcConnectMode.Left;
            CustomCheckBoxSettingQcUseSavedServers.Top = CustomLabelSettingQcConnectMode.Bottom + CustomLabelSettingQcConnectMode.Height;

            CustomCheckBoxSettingQcCheckAllServers.Left = CustomCheckBoxSettingQcUseSavedServers.Right + spaceH2;
            CustomCheckBoxSettingQcCheckAllServers.Top = CustomCheckBoxSettingQcUseSavedServers.Top;

            CustomCheckBoxSettingQcSetDnsTo.Left = CustomLabelSettingQcConnectMode.Left;
            CustomCheckBoxSettingQcSetDnsTo.Top = CustomCheckBoxSettingQcUseSavedServers.Bottom + spaceV;

            CustomComboBoxSettingQcNics.Left = CustomComboBoxSettingQcConnectMode.Left;
            CustomComboBoxSettingQcNics.Top = CustomCheckBoxSettingQcSetDnsTo.Top - 2;

            CustomButtonSettingQcUpdateNics.Left = CustomComboBoxSettingQcNics.Right + spaceH;
            CustomButtonSettingQcUpdateNics.Top = CustomComboBoxSettingQcNics.Top - 2;

            CustomCheckBoxSettingQcStartProxyServer.Left = CustomCheckBoxSettingQcSetDnsTo.Left;
            CustomCheckBoxSettingQcStartProxyServer.Top = CustomCheckBoxSettingQcSetDnsTo.Bottom + spaceV;

            CustomCheckBoxSettingQcSetProxy.Left = CustomCheckBoxSettingQcStartProxyServer.Right + spaceHH;
            CustomCheckBoxSettingQcSetProxy.Top = CustomCheckBoxSettingQcStartProxyServer.Top;

            CustomCheckBoxSettingQcStartGoodbyeDpi.Left = CustomCheckBoxSettingQcStartProxyServer.Left;
            CustomCheckBoxSettingQcStartGoodbyeDpi.Top = CustomCheckBoxSettingQcStartProxyServer.Bottom + spaceV;

            spaceV = 10;
            CustomRadioButtonSettingQcGdBasic.Left = CustomCheckBoxSettingQcStartGoodbyeDpi.Left + spaceH;
            CustomRadioButtonSettingQcGdBasic.Top = CustomCheckBoxSettingQcStartGoodbyeDpi.Bottom + spaceV;

            CustomComboBoxSettingQcGdBasic.Left = CustomComboBoxSettingQcNics.Left;
            CustomComboBoxSettingQcGdBasic.Top = CustomRadioButtonSettingQcGdBasic.Top - 2;

            CustomRadioButtonSettingQcGdAdvanced.Left = CustomComboBoxSettingQcGdBasic.Right + spaceHH;
            CustomRadioButtonSettingQcGdAdvanced.Top = CustomRadioButtonSettingQcGdBasic.Top;

            // ---------- Settings Quick Connect Buttons
            CustomButtonSettingQcStartup.Location = new Point(spaceRight, 3);

            CustomCheckBoxSettingQcOnStartup.Left = CustomButtonSettingQcStartup.Right + spaceH2;
            CustomCheckBoxSettingQcOnStartup.Top = CustomButtonSettingQcStartup.Top + 4;

            try
            {
                SplitContainerSettingQcMain.Panel2MinSize = CustomButtonSettingQcStartup.Height + 6;
                SplitContainerSettingQcMain.SplitterDistance = SplitContainerSettingQcMain.Height - SplitContainerSettingQcMain.Panel2MinSize;
            }
            catch (Exception) { }

            //// Settings Connect
            spaceV = 50;
            CustomLabelSettingMaxServers.Location = new Point(spaceHH, 50);

            CustomNumericUpDownSettingMaxServers.Left = CustomLabelSettingMaxServers.Right + spaceH2;
            CustomNumericUpDownSettingMaxServers.Top = CustomLabelSettingMaxServers.Top - 2;

            CustomCheckBoxDnsEventShowRequest.Left = CustomLabelSettingMaxServers.Left;
            CustomCheckBoxDnsEventShowRequest.Top = CustomLabelSettingMaxServers.Bottom + spaceV;

            CustomCheckBoxSettingDnsEnableRules.Left = CustomCheckBoxDnsEventShowRequest.Left;
            CustomCheckBoxSettingDnsEnableRules.Top = CustomCheckBoxDnsEventShowRequest.Bottom + spaceV;

            CustomButtonSettingDnsRules.Left = CustomCheckBoxSettingDnsEnableRules.Right + spaceH2;
            CustomButtonSettingDnsRules.Top = CustomCheckBoxSettingDnsEnableRules.Top - 2;

            //// Settings Set/Unset DNS
            spaceV = 30;
            CustomRadioButtonSettingUnsetDnsToDhcp.Location = new Point(spaceHH, 35);

            CustomRadioButtonSettingUnsetDnsToStatic.Left = CustomRadioButtonSettingUnsetDnsToDhcp.Left;
            CustomRadioButtonSettingUnsetDnsToStatic.Top = CustomRadioButtonSettingUnsetDnsToDhcp.Bottom + spaceV;

            spaceV = 20;
            CustomLabelSettingUnsetDns1.Left = CustomRadioButtonSettingUnsetDnsToStatic.Left + spaceHH;
            CustomLabelSettingUnsetDns1.Top = CustomRadioButtonSettingUnsetDnsToStatic.Bottom + spaceV;

            CustomTextBoxSettingUnsetDns1.Left = CustomLabelSettingUnsetDns1.Right + (spaceHH / 2);
            CustomTextBoxSettingUnsetDns1.Top = CustomLabelSettingUnsetDns1.Top - 2;

            CustomLabelSettingUnsetDns2.Left = CustomLabelSettingUnsetDns1.Left;
            CustomLabelSettingUnsetDns2.Top = CustomLabelSettingUnsetDns1.Bottom + spaceV;

            CustomTextBoxSettingUnsetDns2.Left = CustomTextBoxSettingUnsetDns1.Left;
            CustomTextBoxSettingUnsetDns2.Top = CustomLabelSettingUnsetDns2.Top - 2;

            //// Settings Share Basic
            CustomLabelSettingProxyPort.Location = new Point(spaceRight, 25);

            CustomNumericUpDownSettingProxyPort.Left = CustomLabelSettingProxyPort.Right + spaceH2;
            CustomNumericUpDownSettingProxyPort.Top = CustomLabelSettingProxyPort.Top - 2;

            CustomLabelSettingProxyHandleRequests.Left = CustomNumericUpDownSettingProxyPort.Right + spaceHH;
            CustomLabelSettingProxyHandleRequests.Top = CustomLabelSettingProxyPort.Top;

            CustomNumericUpDownSettingProxyHandleRequests.Left = CustomLabelSettingProxyHandleRequests.Right + spaceH2;
            CustomNumericUpDownSettingProxyHandleRequests.Top = CustomNumericUpDownSettingProxyPort.Top;

            CustomCheckBoxSettingProxyBlockPort80.Left = CustomNumericUpDownSettingProxyHandleRequests.Right + spaceHH;
            CustomCheckBoxSettingProxyBlockPort80.Top = CustomLabelSettingProxyHandleRequests.Top;

            spaceV = 20;
            CustomLabelSettingProxyKillRequestTimeout.Left = CustomLabelSettingProxyPort.Left;
            CustomLabelSettingProxyKillRequestTimeout.Top = CustomLabelSettingProxyPort.Bottom + spaceV;

            CustomNumericUpDownSettingProxyKillRequestTimeout.Left = CustomLabelSettingProxyKillRequestTimeout.Right + spaceH2;
            CustomNumericUpDownSettingProxyKillRequestTimeout.Top = CustomLabelSettingProxyKillRequestTimeout.Top - 2;

            CustomCheckBoxSettingProxyUpstream.Left = CustomLabelSettingProxyKillRequestTimeout.Left;
            CustomCheckBoxSettingProxyUpstream.Top = CustomLabelSettingProxyKillRequestTimeout.Bottom + spaceV;

            spaceV = 10;
            CustomCheckBoxSettingProxyUpstreamOnlyBlockedIPs.Left = CustomCheckBoxSettingProxyUpstream.Left + (spaceRight * 2);
            CustomCheckBoxSettingProxyUpstreamOnlyBlockedIPs.Top = CustomCheckBoxSettingProxyUpstream.Bottom + spaceV;

            CustomComboBoxSettingProxyUpstreamMode.Left = CustomCheckBoxSettingProxyUpstreamOnlyBlockedIPs.Left;
            CustomComboBoxSettingProxyUpstreamMode.Top = CustomCheckBoxSettingProxyUpstreamOnlyBlockedIPs.Bottom + spaceV;

            CustomLabelSettingProxyUpstreamHost.Left = CustomComboBoxSettingProxyUpstreamMode.Right + spaceHH;
            CustomLabelSettingProxyUpstreamHost.Top = CustomComboBoxSettingProxyUpstreamMode.Top + 3;

            CustomTextBoxSettingProxyUpstreamHost.Left = CustomLabelSettingProxyUpstreamHost.Right + spaceH2;
            CustomTextBoxSettingProxyUpstreamHost.Top = CustomLabelSettingProxyUpstreamHost.Top - 3;

            CustomLabelSettingProxyUpstreamPort.Left = CustomTextBoxSettingProxyUpstreamHost.Right + spaceHH;
            CustomLabelSettingProxyUpstreamPort.Top = CustomLabelSettingProxyUpstreamHost.Top;

            CustomNumericUpDownSettingProxyUpstreamPort.Left = CustomLabelSettingProxyUpstreamPort.Right + spaceH2;
            CustomNumericUpDownSettingProxyUpstreamPort.Top = CustomTextBoxSettingProxyUpstreamHost.Top;

            //// Settings Share Advanced
            spaceV = 10;
            CustomCheckBoxSettingProxyCfCleanIP.Location = new Point(spaceRight, 15);

            CustomTextBoxSettingProxyCfCleanIP.Left = CustomCheckBoxSettingProxyCfCleanIP.Right + spaceH;
            CustomTextBoxSettingProxyCfCleanIP.Top = CustomCheckBoxSettingProxyCfCleanIP.Top - 2;

            CustomLabelSettingShareSeparator1.Left = spaceRight;
            CustomLabelSettingShareSeparator1.Top = CustomCheckBoxSettingProxyCfCleanIP.Bottom + spaceV;
            CustomLabelSettingShareSeparator1.Width = TabPageSettingProxyAdvanced.Width - (spaceRight * 2);
            CustomLabelSettingShareSeparator1.Height = 1;

            CustomCheckBoxSettingProxyEnableRules.Left = CustomCheckBoxSettingProxyCfCleanIP.Left;
            CustomCheckBoxSettingProxyEnableRules.Top = CustomLabelSettingShareSeparator1.Bottom + spaceV;

            CustomLabelSettingProxyRules.Left = spaceRight * 4;
            CustomLabelSettingProxyRules.Top = CustomCheckBoxSettingProxyEnableRules.Bottom + (spaceV / 2);

            CustomButtonSettingProxyRules.Left = CustomLabelSettingProxyRules.Right + spaceHH;
            CustomButtonSettingProxyRules.Top = CustomCheckBoxSettingProxyEnableRules.Bottom + 5;

            //// Settings Fake Proxy
            spaceV = 50;
            CustomLabelSettingFakeProxyInfo.Location = new Point(20, 10);

            CustomLabelSettingFakeProxyDohAddress.Left = CustomLabelSettingFakeProxyInfo.Left;
            CustomLabelSettingFakeProxyDohAddress.Top = CustomLabelSettingFakeProxyInfo.Bottom + spaceV;

            CustomTextBoxSettingFakeProxyDohAddress.Left = CustomLabelSettingFakeProxyDohAddress.Right + spaceH;
            CustomTextBoxSettingFakeProxyDohAddress.Top = CustomLabelSettingFakeProxyDohAddress.Top - 2;

            CustomLabelSettingFakeProxyDohCleanIP.Left = CustomLabelSettingFakeProxyDohAddress.Left;
            CustomLabelSettingFakeProxyDohCleanIP.Top = CustomLabelSettingFakeProxyDohAddress.Bottom + spaceV;

            CustomTextBoxSettingFakeProxyDohCleanIP.Left = CustomLabelSettingFakeProxyDohCleanIP.Right + spaceH;
            CustomTextBoxSettingFakeProxyDohCleanIP.Top = CustomLabelSettingFakeProxyDohCleanIP.Top - 2;

            //// Settings CPU
            spaceV = 30;
            CustomLabelSettingInfoCPU.Location = new Point(50, 35);

            CustomRadioButtonSettingCPUHigh.Left = CustomLabelSettingInfoCPU.Left;
            CustomRadioButtonSettingCPUHigh.Top = CustomLabelSettingInfoCPU.Bottom + spaceV;

            spaceV = 10;
            CustomRadioButtonSettingCPUAboveNormal.Left = CustomRadioButtonSettingCPUHigh.Left;
            CustomRadioButtonSettingCPUAboveNormal.Top = CustomRadioButtonSettingCPUHigh.Bottom + spaceV;

            CustomRadioButtonSettingCPUNormal.Left = CustomRadioButtonSettingCPUAboveNormal.Left;
            CustomRadioButtonSettingCPUNormal.Top = CustomRadioButtonSettingCPUAboveNormal.Bottom + spaceV;

            CustomRadioButtonSettingCPUBelowNormal.Left = CustomRadioButtonSettingCPUNormal.Left;
            CustomRadioButtonSettingCPUBelowNormal.Top = CustomRadioButtonSettingCPUNormal.Bottom + spaceV;

            CustomRadioButtonSettingCPULow.Left = CustomRadioButtonSettingCPUBelowNormal.Left;
            CustomRadioButtonSettingCPULow.Top = CustomRadioButtonSettingCPUBelowNormal.Bottom + spaceV;

            spaceV = 20;
            CustomLabelUpdateAutoDelayMS.Left = CustomRadioButtonSettingCPULow.Left;
            CustomLabelUpdateAutoDelayMS.Top = CustomRadioButtonSettingCPULow.Bottom + spaceV;

            CustomNumericUpDownUpdateAutoDelayMS.Left = CustomLabelUpdateAutoDelayMS.Right + spaceH2;
            CustomNumericUpDownUpdateAutoDelayMS.Top = CustomLabelUpdateAutoDelayMS.Top - 2;

            CustomLabelSettingCpuKillProxyRequests.Left = CustomLabelUpdateAutoDelayMS.Left;
            CustomLabelSettingCpuKillProxyRequests.Top = CustomNumericUpDownUpdateAutoDelayMS.Bottom + spaceV;

            CustomNumericUpDownSettingCpuKillProxyRequests.Left = CustomLabelSettingCpuKillProxyRequests.Right + spaceH2;
            CustomNumericUpDownSettingCpuKillProxyRequests.Top = CustomLabelSettingCpuKillProxyRequests.Top - 2;

            //// Settings Others
            spaceV = 30;
            CustomLabelSettingBootstrapDnsIP.Location = new Point(15, 20);

            CustomTextBoxSettingBootstrapDnsIP.Left = CustomLabelSettingBootstrapDnsIP.Right + spaceH2;
            CustomTextBoxSettingBootstrapDnsIP.Top = CustomLabelSettingBootstrapDnsIP.Top - 2;

            CustomLabelSettingBootstrapDnsPort.Left = CustomTextBoxSettingBootstrapDnsIP.Right + spaceHH;
            CustomLabelSettingBootstrapDnsPort.Top = CustomLabelSettingBootstrapDnsIP.Top;

            CustomNumericUpDownSettingBootstrapDnsPort.Left = CustomLabelSettingBootstrapDnsPort.Right + spaceH2;
            CustomNumericUpDownSettingBootstrapDnsPort.Top = CustomLabelSettingBootstrapDnsPort.Top - 2;

            CustomLabelSettingFallbackDnsIP.Left = CustomLabelSettingBootstrapDnsIP.Left;
            CustomLabelSettingFallbackDnsIP.Top = CustomLabelSettingBootstrapDnsIP.Bottom + spaceV;

            CustomTextBoxSettingFallbackDnsIP.Left = CustomTextBoxSettingBootstrapDnsIP.Left;
            CustomTextBoxSettingFallbackDnsIP.Top = CustomLabelSettingFallbackDnsIP.Top - 2;

            CustomLabelSettingFallbackDnsPort.Left = CustomLabelSettingBootstrapDnsPort.Left;
            CustomLabelSettingFallbackDnsPort.Top = CustomLabelSettingFallbackDnsIP.Top;

            CustomNumericUpDownSettingFallbackDnsPort.Left = CustomNumericUpDownSettingBootstrapDnsPort.Left;
            CustomNumericUpDownSettingFallbackDnsPort.Top = CustomLabelSettingFallbackDnsPort.Top - 2;

            CustomCheckBoxSettingDisableAudioAlert.Left = CustomLabelSettingFallbackDnsIP.Left;
            CustomCheckBoxSettingDisableAudioAlert.Top = CustomLabelSettingFallbackDnsIP.Bottom + spaceV;

            spaceV = 20;
            CustomCheckBoxSettingWriteLogWindowToFile.Left = CustomCheckBoxSettingDisableAudioAlert.Left;
            CustomCheckBoxSettingWriteLogWindowToFile.Top = CustomCheckBoxSettingDisableAudioAlert.Bottom + spaceV;

            CustomCheckBoxSettingAlertDisplayChanges.Left = CustomCheckBoxSettingWriteLogWindowToFile.Left;
            CustomCheckBoxSettingAlertDisplayChanges.Top = CustomCheckBoxSettingWriteLogWindowToFile.Bottom + spaceV;

            // ========== Settings Others Buttons
            CustomButtonSettingRestoreDefault.Location = new Point(spaceRight, 3);

            CustomButtonImportUserData.Left = CustomButtonSettingRestoreDefault.Right + spaceH3;
            CustomButtonImportUserData.Top = CustomButtonSettingRestoreDefault.Top;

            CustomButtonExportUserData.Left = CustomButtonImportUserData.Right + spaceH;
            CustomButtonExportUserData.Top = CustomButtonSettingRestoreDefault.Top;

            try
            {
                SplitContainerSettingOthersMain.Panel2MinSize = CustomButtonSettingRestoreDefault.Height + 6;
                SplitContainerSettingOthersMain.SplitterDistance = SplitContainerSettingOthersMain.Height - SplitContainerSettingOthersMain.Panel2MinSize;
            }
            catch (Exception) { }

            //// About
            spaceV = 30;
            PictureBoxAbout.Location = new Point(55, 35);

            CustomLabelAboutCopyright.Left = PictureBoxAbout.Left;
            CustomLabelAboutCopyright.Top = PictureBoxAbout.Bottom + spaceV;

            PictureBoxFarvahar.Left = CustomLabelAboutCopyright.Left;
            PictureBoxFarvahar.Top = CustomLabelAboutCopyright.Bottom + spaceV;

            CustomLabelAboutThis.Left = PictureBoxAbout.Right + 40;
            CustomLabelAboutThis.Top = PictureBoxAbout.Top;

            CustomLabelAboutVersion.Left = CustomLabelAboutThis.Right;
            CustomLabelAboutVersion.Top = CustomLabelAboutThis.Bottom - 10;

            CustomLabelAboutThis2.Left = CustomLabelAboutThis.Left + 25;
            CustomLabelAboutThis2.Top = CustomLabelAboutThis.Bottom + 5;

            CustomLabelAboutUsing.Left = CustomLabelAboutThis2.Left;
            CustomLabelAboutUsing.Top = CustomLabelAboutThis2.Bottom + 40;

            LinkLabelDNSLookup.Left = CustomLabelAboutUsing.Left + 15;
            LinkLabelDNSLookup.Top = CustomLabelAboutUsing.Top + (LinkLabelDNSLookup.Height * 2);

            spaceV = 5;
            LinkLabelGoodbyeDPI.Left = LinkLabelDNSLookup.Left;
            LinkLabelGoodbyeDPI.Top = LinkLabelDNSLookup.Bottom + spaceV;

            CustomLabelAboutSpecialThanks.Left = LinkLabelGoodbyeDPI.Right + 50;
            CustomLabelAboutSpecialThanks.Top = CustomLabelAboutUsing.Top;

            LinkLabelStAlidxdydz.Left = CustomLabelAboutSpecialThanks.Left + 15;
            LinkLabelStAlidxdydz.Top = CustomLabelAboutSpecialThanks.Top + (LinkLabelStAlidxdydz.Height * 2);

            LinkLabelStWolfkingal2000.Left = LinkLabelStAlidxdydz.Left;
            LinkLabelStWolfkingal2000.Top = LinkLabelStAlidxdydz.Bottom + spaceV;

            IsScreenHighDpiScaleApplied = true;
        });
    }

}