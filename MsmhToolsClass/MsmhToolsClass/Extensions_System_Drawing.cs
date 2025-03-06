using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace MsmhToolsClass;

public static class Extensions_System_Drawing
{
    /// <summary>
    /// Windows Only
    /// </summary>
    public static GraphicsPath? Shrink(this GraphicsPath path, float width)
    {
        try
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return null;
            using GraphicsPath gp = new();
            gp.AddPath(path, false);
            gp.CloseAllFigures();
            gp.Widen(new Pen(Color.Black, width * 2));
            int position = 0;
            GraphicsPath result = new();
            while (position < gp.PointCount)
            {
                // skip outer edge
                position += CountNextFigure(gp.PathData, position);
                // count inner edge
                int figureCount = CountNextFigure(gp.PathData, position);
                var points = new PointF[figureCount];
                var types = new byte[figureCount];

                Array.Copy(gp.PathPoints, position, points, 0, figureCount);
                Array.Copy(gp.PathTypes, position, types, 0, figureCount);
                position += figureCount;
                result.AddPath(new GraphicsPath(points, types), false);
            }
            path.Reset();
            path.AddPath(result, false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_System_Drawing Shrink: " + ex.Message);
        }

        return path;
    }

    /// <summary>
    /// Windows Only
    /// </summary>
    private static int CountNextFigure(PathData data, int position)
    {
        int count = 0;

        try
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return count;
            for (int i = position; i < data?.Types?.Length; i++)
            {
                count++;
                if (0 != (data.Types[i] & (int)PathPointType.CloseSubpath)) return count;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_System_Drawing CountNextFigure: " + ex.Message);
        }

        return count;
    }

    /// <summary>
    /// Windows Only
    /// </summary>
    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
    {
        try
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return;
            GraphicsPath? path = DrawingTool.RoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (path != null) graphics.DrawPath(pen, path);
            graphics.SmoothingMode = SmoothingMode.Default;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_System_Drawing DrawRoundedRectangle: " + ex.Message);
        }
    }

    /// <summary>
    /// Windows Only
    /// </summary>
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
    {
        try
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return;
            GraphicsPath? path = DrawingTool.RoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (path != null) graphics.FillPath(brush, path);
            graphics.SmoothingMode = SmoothingMode.Default;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_System_Drawing FillRoundedRectangle: " + ex.Message);
        }
    }

    /// <summary>
    /// Windows Only
    /// </summary>
    public static void DrawCircle(this Graphics g, Pen pen, float centerX, float centerY, float radius)
    {
        try
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return;
            g.DrawEllipse(pen, centerX - radius, centerY - radius, radius + radius, radius + radius);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_System_Drawing DrawCircle: " + ex.Message);
        }
    }

    /// <summary>
    /// Windows Only
    /// </summary>
    public static void FillCircle(this Graphics g, Brush brush, float centerX, float centerY, float radius)
    {
        try
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return;
            g.FillEllipse(brush, centerX - radius, centerY - radius, radius + radius, radius + radius);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_System_Drawing FillCircle: " + ex.Message);
        }
    }

    /// <summary>
    /// Creates color with corrected brightness.
    /// </summary>
    /// <param name="color">Color to correct.</param>
    /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
    /// Negative values produce darker colors.</param>
    /// <returns>
    /// Corrected <see cref="Color"/> structure.
    /// </returns>
    public static Color ChangeBrightness(this Color color, float correctionFactor)
    {
        try
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }
            if (red < 0) red = 0; if (red > 255) red = 255;
            if (green < 0) green = 0; if (green > 255) green = 255;
            if (blue < 0) blue = 0; if (blue > 255) blue = 255;
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_System_Drawing ChangeBrightness: " + ex.Message);
            return color;
        }
    }

    /// <summary>
    /// Check Color is Light or Dark.
    /// </summary>
    /// <returns>
    /// Returns "Dark" or "Light" as string.
    /// </returns>
    public static string DarkOrLight(this Color color)
    {
        if (color.R * 0.2126 + color.G * 0.7152 + color.B * 0.0722 < 255 / 2)
        {
            return "Dark";
        }
        else
        {
            return "Light";
        }
    }

    /// <summary>
    /// Change Color Hue. (0f - 360f)
    /// </summary>
    /// <returns>
    /// Returns Modified Color.
    /// </returns>
    public static Color ChangeHue(this Color color, float hue)
    {
        try
        {
            //float hueO = color.GetHue();
            float saturationO = color.GetSaturation();
            float lightnessO = color.GetBrightness();
            return ColorsTool.FromHsl(255, hue, saturationO, lightnessO);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_System_Drawing ChangeHue: " + ex.Message);
            return color;
        }
    }

    /// <summary>
    /// Change Color Saturation. (0f - 1f)
    /// </summary>
    /// <returns>
    /// Returns Modified Color.
    /// </returns>
    public static Color ChangeSaturation(this Color color, float saturation)
    {
        try
        {
            float hueO = color.GetHue();
            //float saturationO = color.GetSaturation();
            float lightnessO = color.GetBrightness();
            return ColorsTool.FromHsl(255, hueO, saturation, lightnessO);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions_System_Drawing ChangeSaturation: " + ex.Message);
            return color;
        }
    }


}