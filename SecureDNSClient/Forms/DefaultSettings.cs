using System.Net;

namespace SecureDNSClient;

public partial class FormMain : Form
{
    private void DefaultSettings()
    {
        // Check
        CustomRadioButtonBuiltIn.Checked = true;
        CustomRadioButtonCustom.Checked = false;
        CustomCheckBoxCheckInParallel.Checked = false;
        CustomCheckBoxInsecure.Checked = false;
        LinkLabelCheckUpdate.Text = string.Empty;

        // Connect
        CustomRadioButtonConnectCheckedServers.Checked = true;
        CustomRadioButtonConnectFakeProxyDohViaProxyDPI.Checked = false;
        CustomRadioButtonConnectFakeProxyDohViaGoodbyeDPI.Checked = false;
        CustomRadioButtonConnectDNSCrypt.Checked = false;
        CustomTextBoxHTTPProxy.Text = string.Empty;

        // Share
        CustomCheckBoxProxyEventShowRequest.Checked = false;
        CustomCheckBoxProxyEventShowChunkDetails.Checked = false;
        CustomCheckBoxPDpiEnableDpiBypass.Checked = true;
        CustomNumericUpDownPDpiBeforeSniChunks.Value = (decimal)50;
        CustomComboBoxPDpiSniChunkMode.SelectedIndex = 0;
        CustomNumericUpDownPDpiSniChunks.Value = (decimal)5;
        CustomNumericUpDownPDpiAntiPatternOffset.Value = (decimal)2;
        CustomNumericUpDownPDpiFragDelay.Value = (decimal)1;

        // DPI Basic
        CustomRadioButtonDPIMode1.Checked = false;
        CustomRadioButtonDPIMode2.Checked = false;
        CustomRadioButtonDPIMode3.Checked = false;
        CustomRadioButtonDPIMode4.Checked = false;
        CustomRadioButtonDPIMode5.Checked = false;
        CustomRadioButtonDPIMode6.Checked = false;
        CustomRadioButtonDPIModeLight.Checked = true;
        CustomRadioButtonDPIModeMedium.Checked = false;
        CustomRadioButtonDPIModeHigh.Checked = false;
        CustomRadioButtonDPIModeExtreme.Checked = false;
        CustomNumericUpDownSSLFragmentSize.Value = (decimal)40;

        // DPI Advanced
        CustomCheckBoxDPIAdvP.Checked = true;
        CustomCheckBoxDPIAdvR.Checked = true;
        CustomCheckBoxDPIAdvS.Checked = true;
        CustomCheckBoxDPIAdvM.Checked = true;
        CustomCheckBoxDPIAdvF.Checked = false;
        CustomNumericUpDownDPIAdvF.Value = (decimal)2;
        CustomCheckBoxDPIAdvK.Checked = false;
        CustomNumericUpDownDPIAdvK.Value = (decimal)2;
        CustomCheckBoxDPIAdvN.Checked = false;
        CustomCheckBoxDPIAdvE.Checked = true;
        CustomNumericUpDownDPIAdvE.Value = (decimal)40;
        CustomCheckBoxDPIAdvA.Checked = false;
        CustomCheckBoxDPIAdvW.Checked = true;
        CustomCheckBoxDPIAdvPort.Checked = false;
        CustomNumericUpDownDPIAdvPort.Value = (decimal)80;
        CustomCheckBoxDPIAdvIpId.Checked = false;
        CustomTextBoxDPIAdvIpId.Text = string.Empty;
        CustomCheckBoxDPIAdvAllowNoSNI.Checked = false;
        CustomCheckBoxDPIAdvSetTTL.Checked = false;
        CustomNumericUpDownDPIAdvSetTTL.Value = (decimal)1;
        CustomCheckBoxDPIAdvAutoTTL.Checked = false;
        CustomTextBoxDPIAdvAutoTTL.Text = "1-4-10";
        CustomCheckBoxDPIAdvMinTTL.Checked = false;
        CustomNumericUpDownDPIAdvMinTTL.Value = (decimal)3;
        CustomCheckBoxDPIAdvWrongChksum.Checked = false;
        CustomCheckBoxDPIAdvWrongSeq.Checked = false;
        CustomCheckBoxDPIAdvNativeFrag.Checked = true;
        CustomCheckBoxDPIAdvReverseFrag.Checked = false;
        CustomCheckBoxDPIAdvMaxPayload.Checked = true;
        CustomNumericUpDownDPIAdvMaxPayload.Value = (decimal)1200;

        // Settings Working Mode
        CustomRadioButtonSettingWorkingModeDNS.Checked = true;
        CustomRadioButtonSettingWorkingModeDNSandDoH.Checked = false;
        CustomNumericUpDownSettingWorkingModeSetDohPort.Value = (decimal)443;

        // Settings Check
        CustomNumericUpDownSettingCheckTimeout.Value = (decimal)5;
        CustomTextBoxSettingCheckDPIHost.Text = "www.youtube.com";
        CustomCheckBoxSettingProtocolDoH.Checked = true;
        CustomCheckBoxSettingProtocolTLS.Checked = true;
        CustomCheckBoxSettingProtocolDNSCrypt.Checked = true;
        CustomCheckBoxSettingProtocolDNSCryptRelay.Checked = true;
        CustomCheckBoxSettingProtocolDoQ.Checked = true;
        CustomCheckBoxSettingProtocolPlainDNS.Checked = false;
        CustomCheckBoxSettingSdnsDNSSec.Checked = false;
        CustomCheckBoxSettingSdnsNoLog.Checked = false;
        CustomCheckBoxSettingSdnsNoFilter.Checked = true;

        // Settings Quick Connect
        CustomComboBoxSettingQcConnectMode.SelectedIndex = 0; // Working Servers Mode
        CustomCheckBoxSettingQcUseSavedServers.Checked = false;
        CustomCheckBoxSettingQcCheckAllServers.Checked = false;
        CustomCheckBoxSettingQcSetDnsTo.Checked = true;
        // Update NICs (Quick Connect Settings)
        SecureDNS.UpdateNICs(CustomComboBoxSettingQcNics, false, out _);
        CustomComboBoxSettingQcNics.SelectedIndex = CustomComboBoxSettingQcNics.Items.Count > 0 ? 0 : -1;
        CustomCheckBoxSettingQcStartProxyServer.Checked = false;
        CustomCheckBoxSettingQcSetProxy.Checked = false;
        CustomCheckBoxSettingQcStartGoodbyeDpi.Checked = false;
        CustomRadioButtonSettingQcGdBasic.Checked = true;
        CustomComboBoxSettingQcGdBasic.SelectedIndex = 0; // Light Mode
        CustomRadioButtonSettingQcGdAdvanced.Checked = false;

        // Settings Connect
        CustomCheckBoxSettingEnableCache.Checked = true;
        CustomNumericUpDownSettingMaxServers.Value = (decimal)5;
        CustomNumericUpDownSettingCamouflageDnsPort.Value = (decimal)5380;

        // Settings Set/Unset DNS
        CustomRadioButtonSettingUnsetDnsToDhcp.Checked = false;
        CustomRadioButtonSettingUnsetDnsToStatic.Checked = true;
        CustomTextBoxSettingUnsetDns1.Text = "8.8.8.8";
        CustomTextBoxSettingUnsetDns2.Text = "8.8.4.4";

        // Settings Share Basic
        CustomNumericUpDownSettingProxyPort.Value = (decimal)8080;
        CustomNumericUpDownSettingProxyHandleRequests.Value = (decimal)2000;
        CustomCheckBoxSettingProxyBlockPort80.Checked = true;
        CustomNumericUpDownSettingProxyKillRequestTimeout.Value = (decimal)20;
        CustomCheckBoxSettingProxyUpstream.Checked = false;
        CustomCheckBoxSettingProxyUpstreamOnlyBlockedIPs.Checked = true;
        CustomComboBoxSettingProxyUpstreamMode.SelectedIndex = 1;
        CustomTextBoxSettingProxyUpstreamHost.Text = IPAddress.Loopback.ToString();
        CustomNumericUpDownSettingProxyUpstreamPort.Value = (decimal)1090;

        // Settings Share Advanced
        CustomCheckBoxSettingProxyEnableFakeProxy.Checked = false;
        CustomCheckBoxSettingProxyCfCleanIP.Checked = false;
        CustomTextBoxSettingProxyCfCleanIP.Text = string.Empty;
        CustomCheckBoxSettingProxyEnableFakeDNS.Checked = false;
        CustomCheckBoxSettingProxyEnableBlackWhiteList.Checked = false;
        CustomCheckBoxSettingProxyEnableDontBypass.Checked = false;

        // Settings Fake Proxy
        CustomNumericUpDownSettingFakeProxyPort.Value = (decimal)8070;
        CustomTextBoxSettingFakeProxyDohAddress.Text = "https://dns.cloudflare.com/dns-query";
        CustomTextBoxSettingFakeProxyDohCleanIP.Text = "104.16.132.229";

        // Settings CPU
        CustomRadioButtonSettingCPUHigh.Checked = false;
        CustomRadioButtonSettingCPUAboveNormal.Checked = false;
        CustomRadioButtonSettingCPUNormal.Checked = true;
        CustomRadioButtonSettingCPUBelowNormal.Checked = false;
        CustomRadioButtonSettingCPULow.Checked = false;
        CustomNumericUpDownSettingCpuKillProxyRequests.Value = (decimal)40;

        // Settings Others
        CustomTextBoxSettingBootstrapDnsIP.Text = "8.8.8.8";
        CustomNumericUpDownSettingBootstrapDnsPort.Value = (decimal)53;
        CustomTextBoxSettingFallbackDnsIP.Text = "8.8.8.8";
        CustomNumericUpDownSettingFallbackDnsPort.Value = (decimal)53;
        CustomCheckBoxSettingDontAskCertificate.Checked = false;
        CustomCheckBoxSettingDisableAudioAlert.Checked = false;
        CustomCheckBoxSettingWriteLogWindowToFile.Checked = false;
    }
}