using System;
using System.Runtime.InteropServices;

namespace MsmhToolsClass
{
    public static class RegistryTool
    {
        [DllImport("wininet.dll")]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;

        [DllImport("user32.DLL")]
        private static extern bool SendNotifyMessageA(IntPtr hWnd, uint Msg, int wParam, int lParam);
        private static readonly IntPtr HWND_BROADCAST = (IntPtr)0xffff;
        private static readonly uint WM_SETTINGCHANGE = 0x001A;

        public static void ApplyRegistryChanges()
        {
            // They cause the OS to refresh the settings, causing IP to realy update
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);

            SendNotifyMessageA(HWND_BROADCAST, WM_SETTINGCHANGE, 0, 0);
        }
    }
}
