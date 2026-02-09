using MsmhToolsClass;
using System.Diagnostics;

namespace SecureDNSClient;

public partial class FormMain
{
    private async void DnsConsole_StandardDataReceived(object? sender, DataReceivedEventArgs e)
    {
        string? msg = e.Data;
        if (msg != null)
        {
            if (!msg.StartsWith("Confirmed:"))
                this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.DarkGray));
            await LogToDebugFileAsync(msg);
        }
    }

    private async void ProxyConsole_StandardDataReceived(object? sender, DataReceivedEventArgs e)
    {
        string? msg = e.Data;
        if (msg != null)
        {
            if (!msg.StartsWith("details"))
            {
                if (!msg.StartsWith("Confirmed:"))
                    this.InvokeIt(() => CustomRichTextBoxLog.AppendText(msg + NL, Color.DarkGray));
                await LogToDebugFileAsync(msg);
            }
        }
    }
}