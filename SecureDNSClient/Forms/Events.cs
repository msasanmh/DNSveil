using CustomControls;
using Microsoft.Win32;
using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace SecureDNSClient;

public partial class FormMain : Form
{
    private readonly Stopwatch StopWatchWriteDnsOutputDelay = new();
    private readonly Stopwatch StopWatchWriteProxyOutputDelay = new();

    private void DnsConsole_ErrorDataReceived(object? sender, DataReceivedEventArgs e)
    {
        string? msg = e.Data;
        if (!string.IsNullOrEmpty(msg) && CustomCheckBoxDnsEventShowRequest.Checked)
        {
            if (!StopWatchWriteDnsOutputDelay.IsRunning) StopWatchWriteDnsOutputDelay.Start();
            if (StopWatchWriteDnsOutputDelay.ElapsedMilliseconds < 100) return;
            StopWatchWriteDnsOutputDelay.Restart();
            if (!IsInternetOnline) return;

            bool writeToLog = true;
            _ = GetBlockedDomainSetting(out string blockedDomainNoWww);
            if (msg.Contains(blockedDomainNoWww) || msg.Contains($"{IPAddress.Loopback}:53")) writeToLog = false;

            // Write To Log
            Color reqColor = msg.Contains("Request Denied") ? Color.Orange : Color.Gray;
            if (writeToLog && !IsExiting && IsConnected && IsDNSConnected && !IsInActionState)
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, reqColor));
        }
    }

    private void ProxyConsole_ErrorDataReceived(object? sender, DataReceivedEventArgs e)
    {
        string? msg = e.Data;
        if (!string.IsNullOrEmpty(msg))
        {
            if (!StopWatchWriteProxyOutputDelay.IsRunning) StopWatchWriteProxyOutputDelay.Start();
            if (StopWatchWriteProxyOutputDelay.ElapsedMilliseconds < 100) return;
            StopWatchWriteProxyOutputDelay.Restart();
            if (!IsInternetOnline) return;
            
            // Write To Log
            Color reqColor = msg.Contains("Request Denied") ? Color.Orange : Color.Gray;
            if (!IsExiting && IsProxyActivated && IsProxyRunning && !IsInActionState)
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, reqColor));
        }
    }

    private void CustomRichTextBoxLog_TextAppended(object? sender, EventArgs e)
    {
        if (sender is string text)
        {
            // Write Log To File
            try
            {
                if (CustomCheckBoxSettingWriteLogWindowToFile.Checked)
                    FileDirectory.AppendText(SecureDNS.LogWindowPath, text, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Write Log To File: {ex.Message}");
            }
        }
    }

    private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
    {
        bool alertChanges = false;
        this.InvokeIt(() => alertChanges = CustomCheckBoxSettingAlertDisplayChanges.Checked);

        if (alertChanges)
        {
            if (ScreenDPI.GetSystemDpi() != 96)
            {
                using Graphics g = CreateGraphics();
                PaintEventArgs args = new(g, DisplayRectangle);
                OnPaint(args);
                string msg = "Display Settings Changed.\n";
                msg += "You may need restart the app to fix display blurriness.";
                CustomMessageBox.Show(this, msg);
            }
        }

        if (LabelMainStopWatch.IsRunning) HideLabelMain();
    }

    private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        // Reconnect After System Awake From Sleep
        if (e.Mode == PowerModes.Resume)
        {
            Debug.WriteLine(e.Mode);
            if (LastConnectMode == ConnectMode.ConnectToWorkingServers || LastConnectMode == ConnectMode.ConnectToFakeProxyDohViaGoodbyeDPI)
            {
                await UpdateBoolInternetAccess();
                await UpdateBools(200);
                if (!IsConnecting && !IsDisconnecting && !IsDisconnectingAll && !IsQuickConnectWorking &&
                    IsInternetOnline && IsConnected && !IsDNSConnected)
                {
                    IsReconnecting = true;
                    this.InvokeIt(() => CustomButtonReconnect.Enabled = false);
                    await StartConnect(LastConnectMode, true);
                    IsReconnecting = false;
                }
            }
        }
    }

}
