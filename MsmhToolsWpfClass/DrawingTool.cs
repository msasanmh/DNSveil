using System.Drawing;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace MsmhToolsWpfClass;

public static class DrawingTool
{
    public static BitmapSource IconToBitmapSource(Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }
}