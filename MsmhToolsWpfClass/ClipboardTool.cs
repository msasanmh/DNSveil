using System.Diagnostics;
using System.Windows;

namespace MsmhToolsWpfClass;

public static class ClipboardTool
{
    public static void SetText(string str)
    {
        try
        {
            Clipboard.SetText(str, TextDataFormat.UnicodeText);
        }
        catch { }
    }

    public static string GetText()
    {
        try
        {
            string str = string.Empty;
            IDataObject? data = Clipboard.GetDataObject();
            if (data != null && data.GetDataPresent(DataFormats.UnicodeText))
            {
                object strObj = data.GetData(DataFormats.UnicodeText);
                if (strObj != null) str = strObj?.ToString() ?? string.Empty;
            }
            return str;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("MsmhToolsWpfClass ClipboardTool GetText: " + ex.Message);
            return string.Empty;
        }
    }

}