using CustomControls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MsmhToolsWinFormsClass
{
    public static class ScreenDPI
    {
        // 96 = 100%
        // 120 = 125%
        // 144 = 150%
        // 192 = 200%

        //===============================================================================

        private enum DeviceCaps
        {
            /// <summary>
            /// Logical pixels inch in X
            /// </summary>
            LOGPIXELSX = 88,

            /// <summary>
            /// Horizontal width in pixels
            /// </summary>
            HORZRES = 8,

            /// <summary>
            /// Horizontal width of entire desktop in pixels
            /// </summary>
            DESKTOPHORZRES = 118
        }

        /// <summary>
        /// Retrieves device-specific information for the specified device.
        /// </summary>
        /// <param name="hdc">A handle to the DC.</param>
        /// <param name="nIndex">The item to be returned.</param>
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, DeviceCaps nIndex);

        public static int GetSystemDpi()
        {
            using Graphics screen = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr hdc = screen.GetHdc();

            int virtualWidth = GetDeviceCaps(hdc, DeviceCaps.HORZRES);
            int physicalWidth = GetDeviceCaps(hdc, DeviceCaps.DESKTOPHORZRES);
            screen.ReleaseHdc(hdc);

            float outDpiF = 96f * physicalWidth / virtualWidth;
            decimal outDpiD = Convert.ToDecimal(outDpiF);
            return Convert.ToInt32(Math.Round(outDpiD, 0, MidpointRounding.AwayFromZero));
        }

        //===============================================================================

        public static void FixDpiBeforeInitializeComponent(Form form, float emSize = 9f)
        {
            form.AutoScaleMode = AutoScaleMode.None;
            form.AutoScaleDimensions = new SizeF(96F, 96F);
            Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
            if (form.DeviceDpi > 96)
            {
                // Make the GUI ignore the DPI setting
                form.Font = new Font(form.Font.Name, emSize * 96f / form.CreateGraphics().DpiX, form.Font.Style, form.Font.Unit, form.Font.GdiCharSet, form.Font.GdiVerticalFont);
            }
        }

        public static void FixDpiAfterInitializeComponent(Form form)
        {
            if (form.DeviceDpi > 96)
            {
                List<Control> ctrls = Controllers.GetAllControls(form);
                for (int n = 0; n < ctrls.Count; n++)
                {
                    Control ctrl = ctrls[n];
                    ctrl.Font = form.Font;
                    if (ctrl is CustomDataGridView cdgv)
                    {
                        cdgv.ColumnHeadersDefaultCellStyle.Font = form.Font;
                        cdgv.DefaultCellStyle.Font = form.Font;
                    }
                    if (ctrl is DataGridView dgv)
                    {
                        dgv.ColumnHeadersDefaultCellStyle.Font = form.Font;
                        dgv.DefaultCellStyle.Font = form.Font;
                    }
                }
            }
        }

    }
}
