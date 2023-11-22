using CustomControls;
using MsmhToolsWinFormsClass;

namespace SecureDNSClient;

public partial class FormMain
{
    private void FixScreenDPI(Form form)
    {
        // Old Code
        using Graphics g = form.CreateGraphics();
        if (g.DpiX > 96 || g.DpiY > 96)
        {
            CustomLabelShareInfo.Font = new Font(form.Font.Name, 12f * 96f / form.CreateGraphics().DpiX, form.Font.Style, form.Font.Unit, form.Font.GdiCharSet, form.Font.GdiVerticalFont);
            CustomLabelInfoDPIModes.Font = new Font(form.Font.Name, 10f * 96f / form.CreateGraphics().DpiX, FontStyle.Bold, form.Font.Unit, form.Font.GdiCharSet, form.Font.GdiVerticalFont);
            CustomLabelAboutThis.Font = new Font(form.Font.Name, 19f * 96f / form.CreateGraphics().DpiX, FontStyle.Bold, form.Font.Unit, form.Font.GdiCharSet, form.Font.GdiVerticalFont);
        }
    }

    public async Task ScreenHighDpiScaleStartup(Form form)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        using Graphics g = form.CreateGraphics();
        int sdpi = Convert.ToInt32(Math.Round(g.DpiX, 0, MidpointRounding.AwayFromZero));
        int sdpi2 = ScreenDPI.GetSystemDpi();
        sdpi = Math.Max(sdpi, sdpi2);
        float factor = sdpi / 96f;
        await Task.Run(() => MsmhToolsClass.ExtensionsMethods.InvokeIt(this, () => ScreenHighDpiScaleStartup(this, factor)));
        ScreenFixControlsLocations(); // Fix Controls Locations
    }

    public void ScreenHighDpiScaleStartup(Form form, float factor)
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

            if (c is CustomTabControl ctc)
            {
                int itemWidth = 0, itemHeight = 0;
                foreach (TabPage tab in ctc.TabPages)
                {
                    itemWidth = Math.Max(itemWidth, TextRenderer.MeasureText(tab.Text, tab.Font).Width);
                    itemHeight = Math.Max(itemWidth, TextRenderer.MeasureText(tab.Text, tab.Font).Height);
                }

                int w = Convert.ToInt32(Math.Round(ctc.ItemSize.Width * factor, 2, MidpointRounding.AwayFromZero));
                int h = Convert.ToInt32(Math.Round(ctc.ItemSize.Height * factor, 2, MidpointRounding.AwayFromZero));
                if (ctc.Alignment == TabAlignment.Top || ctc.Alignment == TabAlignment.Bottom)
                    ctc.ItemSize = new(Math.Max(w, itemWidth) + 5, Math.Min(h, itemHeight));
                else
                    ctc.ItemSize = new(Math.Min(w, itemWidth), Math.Max(h, itemHeight) + 5);
            }
        }
    }

    public void ScreenFixControlsLocations()
    {
        // Don't use ComboBox Top, Bottom and Height Property.
        // Spacers
        int spaceBottom = 6, spaceRight = 6, spaceV = 0, spaceH = 8, spaceHH = spaceH * 7;

        // Containers
        CustomTabControlMain.Location = new Point(0, 0);
        CustomTabControlSecureDNS.Location = new Point(3, 3);
        CustomTabControlDPIBasicAdvanced.Location = new Point(3, 3);
        CustomTabControlSettings.Location = new Point(3, 3);
        CustomTabControlSettingProxy.Location = new Point(3, 3);

        // Status
        CustomDataGridViewStatus.Location = new Point(0, LabelScreen.Height);

        CustomButtonProcessMonitor.Left = spaceRight;
        CustomButtonProcessMonitor.Top = CustomGroupBoxStatus.Height - CustomButtonProcessMonitor.Height - spaceBottom;

        // Check
        spaceV = 10;
        CustomRadioButtonBuiltIn.Location = new Point(25, 20);

        CustomRadioButtonCustom.Left = CustomRadioButtonBuiltIn.Left;
        CustomRadioButtonCustom.Top = CustomRadioButtonBuiltIn.Bottom + spaceV;

        CustomLabelCustomServersInfo.Left = CustomRadioButtonCustom.Left + (spaceH * 2);
        CustomLabelCustomServersInfo.Top = CustomRadioButtonCustom.Bottom + spaceV;

        CustomLabelCheckInParallel.Left = CustomRadioButtonCustom.Left;
        CustomLabelCheckInParallel.Top = CustomLabelCustomServersInfo.Bottom + spaceV;

        CustomNumericUpDownCheckInParallel.Left = CustomLabelCheckInParallel.Right + spaceH;
        CustomNumericUpDownCheckInParallel.Top = CustomLabelCheckInParallel.Top - 2;

        CustomCheckBoxInsecure.Left = CustomLabelCheckInParallel.Left;
        CustomCheckBoxInsecure.Top = CustomNumericUpDownCheckInParallel.Bottom + spaceV;

        CustomButtonEditCustomServers.Left = spaceRight;
        CustomButtonEditCustomServers.Top = TabPageCheck.Height - CustomButtonEditCustomServers.Height - spaceBottom;

        CustomButtonCheck.Left = CustomButtonEditCustomServers.Right + spaceH;
        CustomButtonCheck.Top = CustomButtonEditCustomServers.Top;

        CustomButtonQuickConnect.Left = CustomButtonCheck.Right + spaceH;
        CustomButtonQuickConnect.Top = CustomButtonEditCustomServers.Top;

        CustomButtonDisconnectAll.Left = CustomButtonQuickConnect.Right + spaceH;
        CustomButtonDisconnectAll.Top = CustomButtonQuickConnect.Top;

        CustomProgressBarCheck.Left = CustomButtonDisconnectAll.Right + spaceH;
        CustomProgressBarCheck.Top = CustomButtonEditCustomServers.Top;

        CustomButtonCheckUpdate.Left = TabPageCheck.Width - CustomButtonCheckUpdate.Width - spaceRight;
        CustomButtonCheckUpdate.Top = CustomButtonEditCustomServers.Top;

        CustomProgressBarCheck.Width = CustomButtonCheckUpdate.Left - CustomProgressBarCheck.Left - spaceH;

        LinkLabelCheckUpdate.Left = CustomProgressBarCheck.Left;
        LinkLabelCheckUpdate.Top = CustomButtonCheckUpdate.Top - LinkLabelCheckUpdate.Height - spaceBottom;

        // Connect
        spaceV = 40;
        CustomRadioButtonConnectCheckedServers.Location = new Point(35, 25);

        CustomButtonWriteSavedServersDelay.Left = CustomRadioButtonConnectCheckedServers.Right + spaceH;
        CustomButtonWriteSavedServersDelay.Top = CustomRadioButtonConnectCheckedServers.Top - spaceBottom;

        CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Left = CustomRadioButtonConnectCheckedServers.Left;
        CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Top = CustomButtonWriteSavedServersDelay.Bottom + spaceV;

        CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Left = CustomRadioButtonConnectCheckedServers.Left;
        CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Top = CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Bottom + spaceV;

        CustomRadioButtonConnectDNSCrypt.Left = CustomRadioButtonConnectCheckedServers.Left;
        CustomRadioButtonConnectDNSCrypt.Top = CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Bottom + spaceV;

        CustomTextBoxHTTPProxy.Left = CustomRadioButtonConnectDNSCrypt.Right + spaceH;
        CustomTextBoxHTTPProxy.Top = CustomRadioButtonConnectDNSCrypt.Top - 2;

        CustomButtonConnect.Left = spaceHH;
        CustomButtonConnect.Top = TabPageConnect.Height - CustomButtonConnect.Height - spaceBottom;

        // Set Dns
        spaceV = 20;
        CustomLabelSelectNIC.Location = new Point(25, 25);

        CustomComboBoxNICs.Left = CustomLabelSelectNIC.Left;
        CustomComboBoxNICs.Top = CustomLabelSelectNIC.Bottom + spaceV;

        spaceV = 10;
        CustomButtonUpdateNICs.Left = CustomComboBoxNICs.Left;
        CustomButtonUpdateNICs.Top = CustomLabelSelectNIC.Bottom + (CustomLabelSelectNIC.Height * 3) + spaceV;

        CustomButtonEnableDisableNic.Left = CustomButtonUpdateNICs.Right + spaceH;
        CustomButtonEnableDisableNic.Top = CustomButtonUpdateNICs.Top;

        CustomLabelSetDNSInfo.Left = CustomButtonUpdateNICs.Left;
        CustomLabelSetDNSInfo.Top = CustomButtonUpdateNICs.Bottom + spaceV;

        CustomGroupBoxNicStatus.Left = CustomLabelSetDNSInfo.Right + (spaceHH / 2);
        CustomGroupBoxNicStatus.Top = 6;
        CustomGroupBoxNicStatus.Width = TabPageSetDNS.Width - CustomGroupBoxNicStatus.Left - spaceRight;
        CustomGroupBoxNicStatus.Height = TabPageSetDNS.Height - CustomGroupBoxNicStatus.Top - spaceBottom;

        CustomButtonSetDNS.Left = spaceHH;
        CustomButtonSetDNS.Top = TabPageSetDNS.Height - CustomButtonSetDNS.Height - spaceBottom;

        // Proxy Server
        spaceV = 12;
        CustomLabelShareInfo.Location = new Point(25, 10);

        CustomCheckBoxProxyEventShowRequest.Left = CustomLabelShareInfo.Left;
        CustomCheckBoxProxyEventShowRequest.Top = CustomLabelShareInfo.Bottom + spaceV;

        CustomCheckBoxProxyEventShowChunkDetails.Left = CustomCheckBoxProxyEventShowRequest.Right + spaceHH;
        CustomCheckBoxProxyEventShowChunkDetails.Top = CustomLabelShareInfo.Bottom + spaceV;

        CustomLabelShareSeparator1.Left = spaceRight;
        CustomLabelShareSeparator1.Top = CustomCheckBoxProxyEventShowChunkDetails.Bottom + spaceV;
        CustomLabelShareSeparator1.Width = TabPageShare.Width - spaceRight;

        CustomCheckBoxPDpiEnableDpiBypass.Left = CustomLabelShareInfo.Left;
        CustomCheckBoxPDpiEnableDpiBypass.Top = CustomLabelShareSeparator1.Bottom + spaceV;

        CustomLabelPDpiBeforeSniChunks.Left = CustomLabelShareInfo.Left + (spaceH * 2);
        CustomLabelPDpiBeforeSniChunks.Top = CustomCheckBoxPDpiEnableDpiBypass.Bottom + spaceV;

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

        CustomButtonShare.Left = spaceHH;
        CustomButtonShare.Top = TabPageShare.Height - CustomButtonShare.Height - spaceBottom;

        CustomButtonSetProxy.Left = CustomButtonShare.Right + spaceH;
        CustomButtonSetProxy.Top = CustomButtonShare.Top;

        CustomButtonPDpiApplyChanges.Left = CustomButtonSetProxy.Right + spaceHH;
        CustomButtonPDpiApplyChanges.Top = CustomButtonShare.Top;

        CustomButtonPDpiCheck.Left = CustomButtonPDpiApplyChanges.Right + spaceH;
        CustomButtonPDpiCheck.Top = CustomButtonShare.Top;

        CustomLabelPDpiPresets.Left = CustomComboBoxPDpiSniChunkMode.Right + spaceH;
        CustomLabelPDpiPresets.Top = CustomLabelPDpiBeforeSniChunks.Top;

        CustomButtonPDpiPresetDefault.Left = CustomLabelPDpiPresets.Left;
        CustomButtonPDpiPresetDefault.Top = CustomLabelPDpiPresets.Bottom + spaceV;

        CustomLabelShareSeparator2.Left = CustomButtonPDpiPresetDefault.Right + spaceH;
        CustomLabelShareSeparator2.Top = CustomCheckBoxPDpiEnableDpiBypass.Top;
        CustomLabelShareSeparator2.Width = 1;
        CustomLabelShareSeparator2.Height = CustomButtonShare.Top - CustomLabelShareSeparator2.Top - spaceBottom;

        CustomCheckBoxProxyEnableSSL.Left = CustomLabelShareSeparator2.Right + spaceH;
        CustomCheckBoxProxyEnableSSL.Top = CustomCheckBoxPDpiEnableDpiBypass.Top;

        spaceV = 5;
        CustomLabelProxySSLInfo.Left = CustomCheckBoxProxyEnableSSL.Left + (spaceH * 2);
        CustomLabelProxySSLInfo.Top = CustomCheckBoxProxyEnableSSL.Bottom + spaceV;

        spaceV = 12;
        CustomCheckBoxProxySSLChangeSniToIP.Left = CustomCheckBoxProxyEnableSSL.Left;
        CustomCheckBoxProxySSLChangeSniToIP.Top = CustomLabelProxySSLInfo.Bottom + spaceV;

        spaceV = 5;
        CustomLabelProxySSLChangeSniToIpInfo.Left = CustomCheckBoxProxySSLChangeSniToIP.Left + (spaceH * 2);
        CustomLabelProxySSLChangeSniToIpInfo.Top = CustomCheckBoxProxySSLChangeSniToIP.Bottom + spaceV;


        // GoodbyeDPI Basic
        spaceV = 12;
        CustomLabelInfoDPIModes.Location = new Point(25, 15);

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

        CustomNumericUpDownSSLFragmentSize.Left = CustomLabelSSLFragmentSize.Right + spaceH;
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

        CustomButtonDPIBasicActivate.Left = spaceHH;
        CustomButtonDPIBasicActivate.Top = TabPageDPIBasic.Height - CustomButtonDPIBasicActivate.Height - spaceBottom;

        CustomButtonDPIBasicDeactivate.Left = CustomButtonDPIBasicActivate.Right + spaceH;
        CustomButtonDPIBasicDeactivate.Top = CustomButtonDPIBasicActivate.Top;

        // GoodbyeDPI Advanced
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

        CustomButtonDPIAdvActivate.Left = spaceHH;
        CustomButtonDPIAdvActivate.Top = TabPageDPIAdvanced.Height - CustomButtonDPIAdvActivate.Height - spaceBottom;

        CustomButtonDPIAdvDeactivate.Left = CustomButtonDPIAdvActivate.Right + spaceH;
        CustomButtonDPIAdvDeactivate.Top = CustomButtonDPIAdvActivate.Top;

        // Tools
        spaceV = 30;
        CustomButtonToolsDnsScanner.Location = new Point(spaceHH, 50);

        CustomButtonToolsDnsLookup.Left = CustomButtonToolsDnsScanner.Left;
        CustomButtonToolsDnsLookup.Top = CustomButtonToolsDnsScanner.Bottom + spaceV;

        CustomButtonToolsStampReader.Left = CustomButtonToolsDnsLookup.Left;
        CustomButtonToolsStampReader.Top = CustomButtonToolsDnsLookup.Bottom + spaceV;

        CustomButtonToolsStampGenerator.Left = CustomButtonToolsStampReader.Left;
        CustomButtonToolsStampGenerator.Top = CustomButtonToolsStampReader.Bottom + spaceV;

        CustomButtonToolsIpScanner.Left = CustomButtonToolsStampGenerator.Left;
        CustomButtonToolsIpScanner.Top = CustomButtonToolsStampGenerator.Bottom + spaceV;

        CustomButtonToolsFlushDns.Left = CustomButtonToolsDnsScanner.Right + spaceHH;
        CustomButtonToolsFlushDns.Top = CustomButtonToolsDnsScanner.Top;

        // Settings Working Mode
        spaceV = 30;
        CustomLabelSettingInfoWorkingMode1.Location = new Point(50, 35);

        CustomRadioButtonSettingWorkingModeDNS.Left = CustomLabelSettingInfoWorkingMode1.Left;
        CustomRadioButtonSettingWorkingModeDNS.Top = CustomLabelSettingInfoWorkingMode1.Bottom + spaceV;

        CustomRadioButtonSettingWorkingModeDNSandDoH.Left = CustomRadioButtonSettingWorkingModeDNS.Left;
        CustomRadioButtonSettingWorkingModeDNSandDoH.Top = CustomRadioButtonSettingWorkingModeDNS.Bottom + spaceV;

        CustomLabelSettingWorkingModeSetDohPort.Left = CustomRadioButtonSettingWorkingModeDNSandDoH.Left + (spaceH * 2);
        CustomLabelSettingWorkingModeSetDohPort.Top = CustomRadioButtonSettingWorkingModeDNSandDoH.Bottom + (spaceV / 2);

        CustomNumericUpDownSettingWorkingModeSetDohPort.Left = CustomLabelSettingWorkingModeSetDohPort.Right + spaceH;
        CustomNumericUpDownSettingWorkingModeSetDohPort.Top = CustomLabelSettingWorkingModeSetDohPort.Top - 2;

        CustomButtonSettingUninstallCertificate.Left = CustomRadioButtonSettingWorkingModeDNSandDoH.Left;
        CustomButtonSettingUninstallCertificate.Top = CustomLabelSettingWorkingModeSetDohPort.Bottom + spaceV;

        CustomLabelSettingInfoWorkingMode2.Left = CustomButtonSettingUninstallCertificate.Left;
        CustomLabelSettingInfoWorkingMode2.Top = TabPageSettingsWorkingMode.Height - CustomLabelSettingInfoWorkingMode2.Height - spaceBottom;

        // Settings Check
        spaceV = 20;
        CustomLabelSettingCheckTimeout.Location = new Point(15, 25);

        CustomNumericUpDownSettingCheckTimeout.Left = CustomLabelSettingCheckTimeout.Right + spaceH;
        CustomNumericUpDownSettingCheckTimeout.Top = CustomLabelSettingCheckTimeout.Top - 2;

        CustomLabelSettingCheckDPIInfo.Left = CustomLabelSettingCheckTimeout.Left;
        CustomLabelSettingCheckDPIInfo.Top = CustomLabelSettingCheckTimeout.Bottom + spaceV;

        CustomTextBoxSettingCheckDPIHost.Left = CustomLabelSettingCheckDPIInfo.Right + spaceH;
        CustomTextBoxSettingCheckDPIHost.Top = CustomLabelSettingCheckDPIInfo.Top - 2;

        CustomGroupBoxSettingCheckDnsProtocol.Left = spaceRight;
        CustomGroupBoxSettingCheckDnsProtocol.Top = CustomLabelSettingCheckDPIInfo.Bottom + spaceV;
        CustomGroupBoxSettingCheckDnsProtocol.Width = TabPageSettingsCheck.Width - (spaceRight * 2);

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

        // Settings Quick Connect
        spaceV = 30;
        CustomLabelSettingQcInfo.Location = new Point(20, 10);

        CustomLabelSettingQcConnectMode.Left = CustomLabelSettingQcInfo.Left;
        CustomLabelSettingQcConnectMode.Top = CustomLabelSettingQcInfo.Bottom + spaceV;

        CustomComboBoxSettingQcConnectMode.Left = CustomLabelSettingQcConnectMode.Right + (spaceH * 3);
        CustomComboBoxSettingQcConnectMode.Top = CustomLabelSettingQcConnectMode.Top - 2;

        CustomCheckBoxSettingQcUseSavedServers.Left = CustomComboBoxSettingQcConnectMode.Left;
        CustomCheckBoxSettingQcUseSavedServers.Top = CustomLabelSelectNIC.Bottom + (CustomLabelSelectNIC.Height * 3);

        CustomCheckBoxSettingQcCheckAllServers.Left = CustomCheckBoxSettingQcUseSavedServers.Right + spaceH;
        CustomCheckBoxSettingQcCheckAllServers.Top = CustomCheckBoxSettingQcUseSavedServers.Top;

        spaceV = 30;
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

        CustomButtonSettingQcStartup.Left = TabPageSettingsQuickConnect.Width - CustomButtonSettingQcStartup.Width - spaceRight;
        CustomButtonSettingQcStartup.Top = TabPageSettingsQuickConnect.Height - CustomButtonSettingQcStartup.Height - spaceBottom;

        CustomCheckBoxSettingQcOnStartup.Left = CustomButtonSettingQcStartup.Left - CustomCheckBoxSettingQcOnStartup.Width - spaceH;
        CustomCheckBoxSettingQcOnStartup.Top = CustomButtonSettingQcStartup.Top + (CustomButtonSettingQcStartup.Height / 2 - CustomCheckBoxSettingQcOnStartup.Height / 2);

        // Settings Connect
        spaceV = 50;
        CustomCheckBoxSettingEnableCache.Location = new Point(spaceHH, 50);

        CustomLabelSettingMaxServers.Left = CustomCheckBoxSettingEnableCache.Left;
        CustomLabelSettingMaxServers.Top = CustomCheckBoxSettingEnableCache.Bottom + spaceV;

        CustomNumericUpDownSettingMaxServers.Left = CustomLabelSettingMaxServers.Right + spaceH;
        CustomNumericUpDownSettingMaxServers.Top = CustomLabelSettingMaxServers.Top - 2;

        CustomLabelSettingCamouflageDnsPort.Left = CustomLabelSettingMaxServers.Left;
        CustomLabelSettingCamouflageDnsPort.Top = CustomLabelSettingMaxServers.Bottom + spaceV;

        CustomNumericUpDownSettingCamouflageDnsPort.Left = CustomLabelSettingCamouflageDnsPort.Right + spaceH;
        CustomNumericUpDownSettingCamouflageDnsPort.Top = CustomLabelSettingCamouflageDnsPort.Top - 2;

        // Settings Set/Unset DNS
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

        // Settings Share Basic
        CustomLabelSettingProxyPort.Location = new Point(spaceRight, 25);

        CustomNumericUpDownSettingProxyPort.Left = CustomLabelSettingProxyPort.Right + spaceH;
        CustomNumericUpDownSettingProxyPort.Top = CustomLabelSettingProxyPort.Top - 2;

        CustomLabelSettingProxyHandleRequests.Left = CustomNumericUpDownSettingProxyPort.Right + spaceHH;
        CustomLabelSettingProxyHandleRequests.Top = CustomLabelSettingProxyPort.Top;

        CustomNumericUpDownSettingProxyHandleRequests.Left = CustomLabelSettingProxyHandleRequests.Right + spaceH;
        CustomNumericUpDownSettingProxyHandleRequests.Top = CustomNumericUpDownSettingProxyPort.Top;

        CustomCheckBoxSettingProxyBlockPort80.Left = CustomNumericUpDownSettingProxyHandleRequests.Right + spaceHH;
        CustomCheckBoxSettingProxyBlockPort80.Top = CustomLabelSettingProxyHandleRequests.Top;

        CustomLabelSettingProxyKillRequestTimeout.Left = CustomLabelSettingProxyPort.Left;
        CustomLabelSettingProxyKillRequestTimeout.Top = CustomLabelSettingProxyPort.Bottom + spaceV;

        CustomNumericUpDownSettingProxyKillRequestTimeout.Left = CustomLabelSettingProxyKillRequestTimeout.Right + spaceH;
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

        CustomTextBoxSettingProxyUpstreamHost.Left = CustomLabelSettingProxyUpstreamHost.Right + spaceH;
        CustomTextBoxSettingProxyUpstreamHost.Top = CustomLabelSettingProxyUpstreamHost.Top - 3;

        CustomLabelSettingProxyUpstreamPort.Left = CustomTextBoxSettingProxyUpstreamHost.Right + spaceHH;
        CustomLabelSettingProxyUpstreamPort.Top = CustomLabelSettingProxyUpstreamHost.Top;

        CustomNumericUpDownSettingProxyUpstreamPort.Left = CustomLabelSettingProxyUpstreamPort.Right + spaceH;
        CustomNumericUpDownSettingProxyUpstreamPort.Top = CustomTextBoxSettingProxyUpstreamHost.Top;

        // Settings Share Advanced
        spaceV = 10;
        CustomCheckBoxSettingProxyEnableFakeProxy.Location = new Point(spaceRight, 15);

        CustomCheckBoxSettingProxyCfCleanIP.Left = CustomCheckBoxSettingProxyEnableFakeProxy.Left;
        CustomCheckBoxSettingProxyCfCleanIP.Top = CustomCheckBoxSettingProxyEnableFakeProxy.Bottom + spaceV;

        CustomTextBoxSettingProxyCfCleanIP.Left = CustomCheckBoxSettingProxyCfCleanIP.Right + spaceH;
        CustomTextBoxSettingProxyCfCleanIP.Top = CustomCheckBoxSettingProxyCfCleanIP.Top - 2;

        CustomLabelSettingShareSeparator1.Left = spaceRight;
        CustomLabelSettingShareSeparator1.Top = CustomCheckBoxSettingProxyCfCleanIP.Bottom + spaceV;
        CustomLabelSettingShareSeparator1.Width = TabPageSettingProxyAdvanced.Width - (spaceRight * 2);
        CustomLabelSettingShareSeparator1.Height = 1;

        CustomCheckBoxSettingProxyEnableFakeDNS.Left = CustomCheckBoxSettingProxyCfCleanIP.Left;
        CustomCheckBoxSettingProxyEnableFakeDNS.Top = CustomLabelSettingShareSeparator1.Bottom + spaceV;

        CustomLabelSettingProxyFakeDNS.Left = spaceRight * 4;
        CustomLabelSettingProxyFakeDNS.Top = CustomCheckBoxSettingProxyEnableFakeDNS.Bottom + (spaceV / 2);

        CustomButtonSettingProxyFakeDNS.Left = CustomLabelSettingProxyFakeDNS.Right + spaceHH;
        CustomButtonSettingProxyFakeDNS.Top = CustomCheckBoxSettingProxyEnableFakeDNS.Bottom + 5;

        CustomLabelSettingShareSeparator2.Left = CustomLabelSettingShareSeparator1.Left;
        CustomLabelSettingShareSeparator2.Top = CustomLabelSettingProxyFakeDNS.Bottom + spaceV;
        CustomLabelSettingShareSeparator2.Width = CustomLabelSettingShareSeparator1.Width;
        CustomLabelSettingShareSeparator2.Height = 1;

        CustomCheckBoxSettingProxyEnableBlackWhiteList.Left = CustomCheckBoxSettingProxyEnableFakeDNS.Left;
        CustomCheckBoxSettingProxyEnableBlackWhiteList.Top = CustomLabelSettingShareSeparator2.Bottom + spaceV;

        CustomLabelSettingProxyBlackWhiteList.Left = CustomLabelSettingProxyFakeDNS.Left;
        CustomLabelSettingProxyBlackWhiteList.Top = CustomCheckBoxSettingProxyEnableBlackWhiteList.Bottom + (spaceV / 2);

        CustomButtonSettingProxyBlackWhiteList.Left = CustomButtonSettingProxyFakeDNS.Left;
        CustomButtonSettingProxyBlackWhiteList.Top = CustomCheckBoxSettingProxyEnableBlackWhiteList.Bottom + 5;

        CustomLabelSettingShareSeparator3.Left = CustomLabelSettingShareSeparator2.Left;
        CustomLabelSettingShareSeparator3.Top = CustomLabelSettingProxyBlackWhiteList.Bottom + spaceV;
        CustomLabelSettingShareSeparator3.Width = CustomLabelSettingShareSeparator2.Width;
        CustomLabelSettingShareSeparator3.Height = 1;

        CustomCheckBoxSettingProxyEnableDontBypass.Left = CustomCheckBoxSettingProxyEnableBlackWhiteList.Left;
        CustomCheckBoxSettingProxyEnableDontBypass.Top = CustomLabelSettingShareSeparator3.Bottom + spaceV;

        CustomLabelSettingProxyDontBypass.Left = CustomLabelSettingProxyBlackWhiteList.Left;
        CustomLabelSettingProxyDontBypass.Top = CustomCheckBoxSettingProxyEnableDontBypass.Bottom + (spaceV / 2);

        CustomButtonSettingProxyDontBypass.Left = CustomButtonSettingProxyBlackWhiteList.Left;
        CustomButtonSettingProxyDontBypass.Top = CustomCheckBoxSettingProxyEnableDontBypass.Bottom + 5;

        // Settings Fake Proxy
        spaceV = 50;
        CustomLabelSettingFakeProxyInfo.Location = new Point(20, 10);

        CustomLabelSettingFakeProxyPort.Left = CustomLabelSettingFakeProxyInfo.Left;
        CustomLabelSettingFakeProxyPort.Top = CustomLabelSettingFakeProxyInfo.Bottom + spaceV;

        CustomNumericUpDownSettingFakeProxyPort.Left = CustomLabelSettingFakeProxyPort.Right + (spaceHH / 2);
        CustomNumericUpDownSettingFakeProxyPort.Top = CustomLabelSettingFakeProxyPort.Top - 2;

        CustomLabelSettingFakeProxyDohAddress.Left = CustomLabelSettingFakeProxyPort.Left;
        CustomLabelSettingFakeProxyDohAddress.Top = CustomLabelSettingFakeProxyPort.Bottom + spaceV;

        CustomTextBoxSettingFakeProxyDohAddress.Left = CustomNumericUpDownSettingFakeProxyPort.Left;
        CustomTextBoxSettingFakeProxyDohAddress.Top = CustomLabelSettingFakeProxyDohAddress.Top - 2;

        CustomLabelSettingFakeProxyDohCleanIP.Left = CustomLabelSettingFakeProxyDohAddress.Left;
        CustomLabelSettingFakeProxyDohCleanIP.Top = CustomLabelSettingFakeProxyDohAddress.Bottom + spaceV;

        CustomTextBoxSettingFakeProxyDohCleanIP.Left = CustomTextBoxSettingFakeProxyDohAddress.Left;
        CustomTextBoxSettingFakeProxyDohCleanIP.Top = CustomLabelSettingFakeProxyDohCleanIP.Top - 2;

        // Settings CPU
        spaceV = 50;
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

        spaceV = 50;
        CustomLabelSettingCpuKillProxyRequests.Left = CustomRadioButtonSettingCPULow.Left;
        CustomLabelSettingCpuKillProxyRequests.Top = CustomRadioButtonSettingCPULow.Bottom + spaceV;

        CustomNumericUpDownSettingCpuKillProxyRequests.Left = CustomLabelSettingCpuKillProxyRequests.Right + spaceH;
        CustomNumericUpDownSettingCpuKillProxyRequests.Top = CustomLabelSettingCpuKillProxyRequests.Top - 2;

        // Settings Others
        spaceV = 30;
        CustomLabelSettingBootstrapDnsIP.Location = new Point(15, 20);

        CustomTextBoxSettingBootstrapDnsIP.Left = CustomLabelSettingBootstrapDnsIP.Right + spaceH;
        CustomTextBoxSettingBootstrapDnsIP.Top = CustomLabelSettingBootstrapDnsIP.Top - 2;

        CustomLabelSettingBootstrapDnsPort.Left = CustomTextBoxSettingBootstrapDnsIP.Right + spaceHH;
        CustomLabelSettingBootstrapDnsPort.Top = CustomLabelSettingBootstrapDnsIP.Top;

        CustomNumericUpDownSettingBootstrapDnsPort.Left = CustomLabelSettingBootstrapDnsPort.Right + spaceH;
        CustomNumericUpDownSettingBootstrapDnsPort.Top = CustomLabelSettingBootstrapDnsPort.Top - 2;

        CustomLabelSettingFallbackDnsIP.Left = CustomLabelSettingBootstrapDnsIP.Left;
        CustomLabelSettingFallbackDnsIP.Top = CustomLabelSettingBootstrapDnsIP.Bottom + spaceV;

        CustomTextBoxSettingFallbackDnsIP.Left = CustomTextBoxSettingBootstrapDnsIP.Left;
        CustomTextBoxSettingFallbackDnsIP.Top = CustomLabelSettingFallbackDnsIP.Top - 2;

        CustomLabelSettingFallbackDnsPort.Left = CustomLabelSettingBootstrapDnsPort.Left;
        CustomLabelSettingFallbackDnsPort.Top = CustomLabelSettingFallbackDnsIP.Top;

        CustomNumericUpDownSettingFallbackDnsPort.Left = CustomNumericUpDownSettingBootstrapDnsPort.Left;
        CustomNumericUpDownSettingFallbackDnsPort.Top = CustomLabelSettingFallbackDnsPort.Top - 2;

        CustomCheckBoxSettingDontAskCertificate.Left = CustomLabelSettingFallbackDnsIP.Left;
        CustomCheckBoxSettingDontAskCertificate.Top = CustomLabelSettingFallbackDnsIP.Bottom + spaceV;

        spaceV = 20;
        CustomCheckBoxSettingDisableAudioAlert.Left = CustomCheckBoxSettingDontAskCertificate.Left;
        CustomCheckBoxSettingDisableAudioAlert.Top = CustomCheckBoxSettingDontAskCertificate.Bottom + spaceV;

        CustomCheckBoxSettingWriteLogWindowToFile.Left = CustomCheckBoxSettingDisableAudioAlert.Left;
        CustomCheckBoxSettingWriteLogWindowToFile.Top = CustomCheckBoxSettingDisableAudioAlert.Bottom + spaceV;

        CustomButtonSettingRestoreDefault.Left = spaceHH;
        CustomButtonSettingRestoreDefault.Top = TabPageSettingsOthers.Height - CustomButtonSettingRestoreDefault.Height - spaceBottom;

        CustomButtonImportUserData.Left = TabPageSettingsOthers.Width - CustomButtonImportUserData.Width - spaceRight;
        CustomButtonImportUserData.Top = CustomButtonSettingRestoreDefault.Top;

        CustomButtonExportUserData.Left = CustomButtonImportUserData.Left - CustomButtonExportUserData.Width - spaceH;
        CustomButtonExportUserData.Top = CustomButtonSettingRestoreDefault.Top;

        // About
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
        LinkLabelDNSProxy.Left = LinkLabelDNSLookup.Left;
        LinkLabelDNSProxy.Top = LinkLabelDNSLookup.Bottom + spaceV;

        LinkLabelDNSCrypt.Left = LinkLabelDNSProxy.Left;
        LinkLabelDNSCrypt.Top = LinkLabelDNSProxy.Bottom + spaceV;

        LinkLabelGoodbyeDPI.Left = LinkLabelDNSCrypt.Left;
        LinkLabelGoodbyeDPI.Top = LinkLabelDNSCrypt.Bottom + spaceV;

        CustomLabelAboutSpecialThanks.Left = LinkLabelDNSProxy.Right + 15;
        CustomLabelAboutSpecialThanks.Top = CustomLabelAboutUsing.Top;

        LinkLabelStAlidxdydz.Left = CustomLabelAboutSpecialThanks.Left + 15;
        LinkLabelStAlidxdydz.Top = CustomLabelAboutSpecialThanks.Top + (LinkLabelStAlidxdydz.Height * 2) + 10;
    }

}