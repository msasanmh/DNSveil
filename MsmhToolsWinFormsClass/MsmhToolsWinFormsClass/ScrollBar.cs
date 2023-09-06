using System;
using MsmhToolsClass;

namespace MsmhToolsWinFormsClass
{
    public class ScrollBar
    {
        //-----------------------------------------------------------------------------------
        // Is Vertical Scrollbar Visible
        private const int WS_VSCROLL = 0x200000;
        private const int GWL_STYLE = -16;
        public static bool IsVScrollbarVisible(IntPtr hWnd)
        {
            int nMessage = WS_VSCROLL;
            int nStyle = NativeMethods.GetWindowLong(hWnd, GWL_STYLE);
            bool bVisible = (nStyle & nMessage) != 0;
            return bVisible;
        } // Usage: IsVScrollbarVisible(ListView1.Handle);
          //-----------------------------------------------------------------------------------
          // Is Horizontal Scrollbar Visible
        private const int WS_HSCROLL = 0x100000;
        public static bool IsHScrollbarVisible(IntPtr hWnd)
        {
            int nMessage = WS_HSCROLL;
            int nStyle = NativeMethods.GetWindowLong(hWnd, GWL_STYLE);
            bool bVisible = (nStyle & nMessage) != 0;
            return bVisible;
        } // Usage: IsHScrollbarVisible(ListView1.Handle);
          //-----------------------------------------------------------------------------------
    }
}
