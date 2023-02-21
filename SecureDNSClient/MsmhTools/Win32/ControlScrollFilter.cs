using System.Drawing;
using System.Windows.Forms;

namespace MsmhTools.Win32
{
    public class ControlScrollFilter : IMessageFilter
    {
        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)WindowsMessages.MOUSEWHEEL:
                case (int)WindowsMessages.MOUSEHWHEEL:
                    var hControlUnderMouse = NativeMethods.WindowFromPoint(new Point((int)m.LParam));

                    if (hControlUnderMouse == m.HWnd)
                        return false;

                    NativeMethods.SendMessage(hControlUnderMouse, m.Msg, m.WParam, m.LParam);
                    return true;
            }

            return false;
        }
    }
}
