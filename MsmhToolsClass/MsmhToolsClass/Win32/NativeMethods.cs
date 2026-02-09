using System.Drawing;
using System.Runtime.InteropServices;

namespace MsmhToolsClass;

#pragma warning disable CA1401 // P/Invokes should not be visible
public static class NativeMethods
{
    #region Hunspell

    [DllImport("libhunspell", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern IntPtr Hunspell_create(string affpath, string dpath);

    [DllImport("libhunspell")]

    public static extern IntPtr Hunspell_destroy(IntPtr hunspellHandle);

    [DllImport("libhunspell", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int Hunspell_spell(IntPtr hunspellHandle, string word);

    [DllImport("libhunspell", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern int Hunspell_suggest(IntPtr hunspellHandle, IntPtr slst, string word);

    [DllImport("libhunspell")]
    public static extern void Hunspell_free_list(IntPtr hunspellHandle, IntPtr slst, int n);

    #endregion Hunspell

    #region Win32 API

    // Win32 API functions for dynamically loading DLLs
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AttachConsole(int dwProcessId);
    public const int ATTACH_PARENT_PROCESS = -1;

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeConsole();

    [DllImport("user32.dll")]
    public static extern short GetKeyState(int vKey);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int Index);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);


    [StructLayout(LayoutKind.Sequential)]
    public struct COMBOBOXINFO
    {
        public int cbSize;
        public RECT rcItem;
        public RECT rcButton;
        public ComboBoxButtonState buttonState;
        public IntPtr hwndCombo;
        public IntPtr hwndEdit;
        public IntPtr hwndList;
    }
    public enum ComboBoxButtonState
    {
        STATE_SYSTEM_NONE = 0,
        STATE_SYSTEM_INVISIBLE = 0x00008000,
        STATE_SYSTEM_PRESSED = 0x00000008
    }
    [DllImport("user32.dll")]
    public static extern bool GetComboBoxInfo(IntPtr hWnd, ref COMBOBOXINFO pcbi);


    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner
    }
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);


    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int width, int height, int wFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    public const int WM_SETREDRAW = 0x0b;

    [DllImport("user32.dll", EntryPoint = "SendMessageA")]
    public static extern int SendMessageA(IntPtr hwnd, int wMsg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr WindowFromPoint(Point point);

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);
    // Usage: SetWindowTheme(control.Handle, "DarkMode_Explorer", null);

    // System Context Menu
    public static uint TPM_LEFTALIGN { get; } = 0;
    public static uint TPM_RETURNCMD { get; } = 256;

    [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
    public static extern IntPtr PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = false, SetLastError = true)]
    public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
    public static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
    public static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

    // Enable Default Window Animations
    [Flags]
    public enum WS : long
    {
        WS_BORDER = 0x00800000L,
        WS_CAPTION = 0x00C00000L,
        WS_CHILD = 0x40000000L,
        WS_CHILDWINDOW = 0x40000000L,
        WS_CLIPCHILDREN = 0x02000000L,
        WS_CLIPSIBLINGS = 0x04000000L,
        WS_DISABLED = 0x08000000L,
        WS_DLGFRAME = 0x00400000L,
        WS_GROUP = 0x00020000L,
        WS_HSCROLL = 0x00100000L,
        WS_ICONIC = 0x20000000L,
        WS_MAXIMIZE = 0x01000000L,
        WS_MAXIMIZEBOX = 0x00010000L,
        WS_MINIMIZE = 0x20000000L,
        WS_MINIMIZEBOX = 0x00020000L,
        WS_OVERLAPPED = 0x00000000L,
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_POPUP = 0x80000000L,
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
        WS_SIZEBOX = 0x00040000L,
        WS_SYSMENU = 0x00080000L,
        WS_TABSTOP = 0x00010000L,
        WS_THICKFRAME = 0x00040000L,
        WS_TILED = 0x00000000L,
        WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_VISIBLE = 0x10000000L,
        WS_VSCROLL = 0x00200000L
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    public static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

    #endregion Win32 API

    #region VLC

    // LibVLC Core - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__core.html
    [DllImport("libvlc")]
    public static extern IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray)] string[] argv);

    [DllImport("libvlc")]
    public static extern void libvlc_release(IntPtr libVlc);

    // LibVLC Media - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media.html
    [DllImport("libvlc")]
    public static extern IntPtr libvlc_media_new_path(IntPtr instance, byte[] input);

    [DllImport("libvlc")]
    public static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);

    [DllImport("libvlc")]
    public static extern void libvlc_media_release(IntPtr media);

    // LibVLC Audio Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__audio.html
    [DllImport("libvlc")]
    public static extern int libvlc_audio_get_track_count(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern int libvlc_audio_get_track(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern int libvlc_audio_set_track(IntPtr mediaPlayer, int trackNumber);

    // LibVLC Audio Controls - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__audio.html
    [DllImport("libvlc")]
    public static extern int libvlc_audio_get_volume(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern void libvlc_audio_set_volume(IntPtr mediaPlayer, int volume);

    // LibVLC media player - http://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc__media__player.html
    [DllImport("libvlc")]
    public static extern void libvlc_media_player_play(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern void libvlc_media_player_stop(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern void libvlc_media_player_pause(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern void libvlc_media_player_set_hwnd(IntPtr mediaPlayer, IntPtr windowsHandle);

    [DllImport("libvlc")]
    public static extern Int64 libvlc_media_player_get_time(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern void libvlc_media_player_set_time(IntPtr mediaPlayer, Int64 position);

    [DllImport("libvlc")]
    public static extern byte libvlc_media_player_get_state(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern Int64 libvlc_media_player_get_length(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern void libvlc_media_list_player_release(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern float libvlc_media_player_get_rate(IntPtr mediaPlayer);

    [DllImport("libvlc")]
    public static extern int libvlc_media_player_set_rate(IntPtr mediaPlayer, float rate);

    #endregion VLC

    #region MPV
    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr mpv_create();


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_initialize(IntPtr mpvHandle);


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_command(IntPtr mpvHandle, IntPtr utf8Strings);


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_terminate_destroy(IntPtr mpvHandle);


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr mpv_wait_event(IntPtr mpvHandle, double wait);


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_set_option(IntPtr mpvHandle, byte[] name, int format, ref long data);


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_set_option_string(IntPtr mpvHandle, byte[] name, byte[] value);


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr mpv_get_property_string(IntPtr mpvHandle, byte[] name);


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_get_property(IntPtr mpvHandle, byte[] name, int format, ref double data);


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_set_property(IntPtr mpvHandle, byte[] name, int format, ref byte[] data);


    [DllImport("mpv", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mpv_free(IntPtr data);

    #endregion MPV

    #region Linux System

    public const int LC_NUMERIC = 1;

    public const int RTLD_NOW = 0x0001;
    public const int RTLD_GLOBAL = 0x0100;

    [DllImport("libc.so.6", CharSet = CharSet.Unicode)]
    public static extern IntPtr setlocale(int category, string locale);

    [DllImport("libdl.so.2", CharSet = CharSet.Unicode)]
    public static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libdl.so.2")]
    public static extern IntPtr dlclose(IntPtr handle);

    [DllImport("libdl.so.2", CharSet = CharSet.Unicode)]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);

    #endregion

    #region Cross platform

    public static IntPtr CrossLoadLibrary(string fileName)
    {
        if (Info.IsRunningOnWindows)
        {
            return LoadLibrary(fileName);
        }

        return dlopen(fileName, RTLD_NOW | RTLD_GLOBAL);
    }

    public static void CrossFreeLibrary(IntPtr handle)
    {
        if (Info.IsRunningOnWindows)
        {
            FreeLibrary(handle);
        }
        else
        {
            dlclose(handle);
        }
    }

    public static IntPtr CrossGetProcAddress(IntPtr handle, string name)
    {
        if (Info.IsRunningOnWindows)
        {
            return GetProcAddress(handle, name);
        }
        return dlsym(handle, name);
    }

    #endregion

    #region MSasanMH Methods



    #endregion
}
#pragma warning restore CA1401 // P/Invokes should not be visible
