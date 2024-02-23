using CustomControls;
using MsmhToolsClass;
using System.Runtime.InteropServices;

namespace MsmhToolsWinFormsClass;

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

    public static async Task SettingWidthOfControls(Form form)
    {
        await Task.Run(() =>
        {
            // Setting Width Of Controls
            List<Control> ctrls = Controllers.GetAllControls(form);
            for (int n = 0; n < ctrls.Count; n++)
            {
                try
                {
                    Control ctrl = ctrls[n];
                    if (ctrl.Dock == DockStyle.Fill) continue;

                    // Filter Controls
                    bool apply = ctrl is CustomButton ||
                                 ctrl is CustomCheckBox ||
                                 ctrl is CustomLabel ||
                                 ctrl is CustomNumericUpDown ||
                                 ctrl is CustomRadioButton;

                    if (!apply) continue;

                    string text = ctrl.Text;
                    if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                    {
                        int linesCount = 1;
                        if (text.Contains(Environment.NewLine)) // Handle Multiline
                        {
                            List<string> lines = text.Split(Environment.NewLine).ToList(); // SplitToLines() will remove empty lines
                            linesCount = lines.Count;
                            text = lines[0];
                            for (int i = 0; i < lines.Count; i++)
                            {
                                string line = lines[i];
                                if (line.Length > text.Length) text = line;
                            }
                        }

                        form.InvokeIt(() =>
                        {
                            string pad = "MSM";
                            if (ctrl is CustomButton cb)
                            {
                                cb.AutoSize = false;
                                pad = "MS";
                            }
                            else if (ctrl is CustomCheckBox ccb)
                            {
                                ccb.AutoSize = false;
                                pad = "MSI";
                            }
                            else if (ctrl is CustomLabel cl)
                            {
                                cl.AutoSize = false;
                                pad = "I";
                            }
                            else if (ctrl is CustomNumericUpDown cnud)
                            {
                                cnud.AutoSize = false;
                                pad = "MSMI";
                            }
                            else if (ctrl is CustomRadioButton crb)
                            {
                                crb.AutoSize = false;
                                pad = "MSI";
                            }
                            Size size = TextRenderer.MeasureText(text + pad, ctrl.Font);
                            int width = size.Width;
                            int height = size.Height;
                            int modifiedHeight = Convert.ToInt32(Math.Round(height * 1.2)) * linesCount;

                            if (ctrl is CustomButton)
                            {
                                if (width > ctrl.Width)
                                {
                                    ctrl.InvokeIt(() => ctrl.Width = width);
                                }
                            }
                            else
                            {
                                ctrl.InvokeIt(() =>
                                {
                                    ctrl.Width = width;
                                    ctrl.Height = modifiedHeight;
                                });
                            }
                        });
                    }
                }
                catch (Exception) { }
            }
        });
    }

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
        form.InvokeIt(() =>
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
        });
    }

}