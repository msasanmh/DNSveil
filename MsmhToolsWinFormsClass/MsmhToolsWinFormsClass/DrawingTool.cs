using System;
using System.Runtime.InteropServices;

namespace MsmhToolsWinFormsClass;

public static class DrawingTool
{
    //-----------------------------------------------------------------------------------
    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
    private const int WM_SETREDRAW = 11;
    public static void SuspendDrawing(Control parent)
    {
        _ = SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
    }
    public static void ResumeDrawing(Control parent)
    {
        _ = SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
        parent.Refresh();
    }
    //-----------------------------------------------------------------------------------
}