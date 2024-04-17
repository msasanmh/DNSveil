using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MsmhToolsClass;

public static class DrawingTool
{
    //-----------------------------------------------------------------------------------
    [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);
    /// <summary>
    /// Extract Icon From DLL (Windows Only)
    /// </summary>
    /// <returns>Returns null if fail</returns>
    public static Icon? ExtractIcon(string dllPath, int index, bool largeIcon)
    {
        Icon? icon = null;
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return icon;
        _ = ExtractIconEx(dllPath, index, out IntPtr large, out IntPtr small, 1);

        try
        {
            icon = Icon.FromHandle(largeIcon ? large : small);
        }
        catch { }

        return icon;
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Windows Only
    /// </summary>
    public static GraphicsPath? RoundedRectangle(Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return null;
        int diameterTopLeft = radiusTopLeft * 2;
        int diameterTopRight = radiusTopRight * 2;
        int diameterBottomRight = radiusBottomRight * 2;
        int diameterBottomLeft = radiusBottomLeft * 2;

        Rectangle arc1 = new(bounds.Location, new Size(diameterTopLeft, diameterTopLeft));
        Rectangle arc2 = new(bounds.Location, new Size(diameterTopRight, diameterTopRight));
        Rectangle arc3 = new(bounds.Location, new Size(diameterBottomRight, diameterBottomRight));
        Rectangle arc4 = new(bounds.Location, new Size(diameterBottomLeft, diameterBottomLeft));
        GraphicsPath path = new();

        // Top Left Arc  
        if (radiusTopLeft == 0)
        {
            path.AddLine(arc1.Location, arc1.Location);
        }
        else
        {
            path.AddArc(arc1, 180, 90);
        }
        // Top Right Arc  
        arc2.X = bounds.Right - diameterTopRight;
        if (radiusTopRight == 0)
        {
            path.AddLine(arc2.Location, arc2.Location);
        }
        else
        {
            path.AddArc(arc2, 270, 90);
        }
        // Bottom Right Arc
        arc3.X = bounds.Right - diameterBottomRight;
        arc3.Y = bounds.Bottom - diameterBottomRight;
        if (radiusBottomRight == 0)
        {
            path.AddLine(arc3.Location, arc3.Location);
        }
        else
        {
            path.AddArc(arc3, 0, 90);
        }
        // Bottom Left Arc 
        arc4.X = bounds.Right - diameterBottomLeft;
        arc4.Y = bounds.Bottom - diameterBottomLeft;
        arc4.X = bounds.Left;
        if (radiusBottomLeft == 0)
        {
            path.AddLine(arc4.Location, arc4.Location);
        }
        else
        {
            path.AddArc(arc4, 90, 90);
        }
        path.CloseFigure();
        return path;
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Windows Only
    /// </summary>
    public static Bitmap? Invert(Bitmap source)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return null;
        //create a blank bitmap the same size as original
        Bitmap newBitmap = new(source.Width, source.Height);
        //get a graphics object from the new image
        Graphics g = Graphics.FromImage(newBitmap);
        // create the negative color matrix
        ColorMatrix colorMatrix = new(new float[][]
        {
                    new float[] {-1, 0, 0, 0, 0},
                    new float[] {0, -1, 0, 0, 0},
                    new float[] {0, 0, -1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {1, 1, 1, 0, 1}
        });
        // create some image attributes
        ImageAttributes attributes = new();
        attributes.SetColorMatrix(colorMatrix);
        g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                    0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
        //dispose the Graphics object
        g.Dispose();
        return newBitmap;
    }
    //-----------------------------------------------------------------------------------
}