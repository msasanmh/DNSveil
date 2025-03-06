using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace MsmhToolsWpfClass;

public static class ConvertToolWpf
{
    /// <summary>
    /// Convert From HEX String
    /// </summary>
    /// <param name="hexStr">Color In HEX</param>
    public static bool TryConvertColorFromString(string hexStr, out Color color)
    {
        bool success = false;
		color = Colors.Transparent;
		try
		{
            Color? c = ColorConverter.ConvertFromString(hexStr) as Color?;
            if (c != null)
            {
                color = (Color)c;
                success = true;
            }
		}
		catch (Exception) { }
        return success;
    }

    public static bool TryConvertThicknessFromString(string str, out Thickness thickness)
    {
        // Str e.g. 1,1,1,1
        bool success = false;
        thickness = new();
        try
        {
            string[] split = str.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (split.Length == 4)
            {
                thickness = new(double.Parse(split[0]), double.Parse(split[1]), double.Parse(split[2]), double.Parse(split[3]));
                success = true;
            }
        }
        catch (Exception) { }
        return success;
    }
}