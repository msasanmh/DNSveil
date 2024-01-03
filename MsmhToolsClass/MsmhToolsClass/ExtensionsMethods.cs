using System.Reflection;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Data;
using System.Xml.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;

namespace MsmhToolsClass;

public static class Methods
{
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    internal extern static int SetWindowTheme(IntPtr controlHandle, string appName, string? idList);
}
public static class ExtensionsMethods
{
    //-----------------------------------------------------------------------------------
    public static string GetInnerExceptions(this Exception ex)
    {
        string result = string.Empty;
        result += ex.Message;
        if (ex.InnerException != null)
        {
            result += Environment.NewLine + ex.InnerException.Message;
            if (ex.InnerException.InnerException != null)
            {
                result += Environment.NewLine + ex.InnerException.InnerException.Message;
                if (ex.InnerException.InnerException.InnerException != null)
                {
                    result += Environment.NewLine + ex.InnerException.InnerException.InnerException.Message;
                    if (ex.InnerException.InnerException.InnerException.InnerException != null)
                        result += Environment.NewLine + ex.InnerException.InnerException.InnerException.InnerException.Message;
                }
            }
        }
        return result;
    }
    //-----------------------------------------------------------------------------------
    public static string TrimStart(this string source, string value)
    {
        string result = source;
        try
        {
            if (result.StartsWith(value))
            {
                result = result[value.Length..];
            }
        }
        catch (Exception) { }
        return result;
    }
    //-----------------------------------------------------------------------------------
    public static string TrimEnd(this string source, string value)
    {
        string result = source;
        try
        {
            if (result.EndsWith(value))
            {
                result = result.Remove(source.LastIndexOf(value, StringComparison.Ordinal));
            }
        }
        catch (Exception) { }
        return result;
    }
    //-----------------------------------------------------------------------------------
    public static bool IsConnected(this Socket socket, SelectMode selectMode = SelectMode.SelectRead)
    {
        bool part1 = socket.Poll(1000, selectMode);
        bool part2 = socket.Available == 0;
        return !part1 || !part2;
    }
    //-----------------------------------------------------------------------------------
    public static async Task SaveAsync(this XDocument xDocument, string xmlFilePath)
    {
        try
        {
            XmlWriterSettings xmlWriterSettings = new();
            xmlWriterSettings.WriteEndDocumentOnClose = true;
            xmlWriterSettings.Async = true;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.OmitXmlDeclaration = true;
            xmlWriterSettings.Encoding = new UTF8Encoding(false);
            using XmlWriter xmlWriter = XmlWriter.Create(xmlFilePath, xmlWriterSettings);
            await xDocument.SaveAsync(xmlWriter, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"XDocument SaveAsync Extension: {ex.Message}");
        }
    }
    //-----------------------------------------------------------------------------------
    public static TimeSpan Round(this TimeSpan timeSpan, int precision)
    {
        return TimeSpan.FromSeconds(Math.Round(timeSpan.TotalSeconds, precision));
    }
    //-----------------------------------------------------------------------------------
    public static bool IsContain<T>(this List<T> list, T t)
    {
        for (int n = 0; n < list.Count; n++)
            if (t != null && t.Equals(list[n])) return true;
        return false;
    }
    //-----------------------------------------------------------------------------------
    public static List<List<T>> SplitToLists<T>(this List<T> list, int nSize)
    {
        List<List<T>> listOut = new();

        for (int n = 0; n < list.Count; n += nSize)
        {
            listOut.Add(list.GetRange(n, Math.Min(nSize, list.Count - n)));
        }

        return listOut;
    }
    //-----------------------------------------------------------------------------------
    public static List<string> SplitToLines(this string s)
    {
        // Original non-optimized version: return source.Replace("\r\r\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');
        List<string> lines = new();
        int start = 0;
        int max = s.Length;
        int i = 0;
        while (i < max)
        {
            var ch = s[i];
            if (ch == '\r')
            {
                if (i < s.Length - 2 && s[i + 1] == '\r' && s[i + 2] == '\n') // \r\r\n
                {
                    if (start < i)
                        lines.Add(s[start..i]); // s[start..i] = s.Substring(start, i - start)
                    i += 3;
                    start = i;
                    continue;
                }

                if (i < s.Length - 1 && s[i + 1] == '\n') // \r\n
                {
                    if (start < i)
                        lines.Add(s[start..i]);
                    i += 2;
                    start = i;
                    continue;
                }

                if (start < i)
                    lines.Add(s[start..i]);
                i++;
                start = i;
                continue;
            }

            if (ch == '\n' || ch == '\u2028')
            {
                if (start < i)
                    lines.Add(s[start..i]);
                i++;
                start = i;
                continue;
            }

            i++;
        }

        if (start < i)
            lines.Add(s[start..i]);
        return lines;
    }

    public static List<string> SplitToLines(this string s, int maxCount)
    {
        var lines = new List<string>();
        int start = 0;
        int max = Math.Min(maxCount, s.Length);
        int i = 0;
        while (i < max)
        {
            var ch = s[i];
            if (ch == '\r')
            {
                if (i < s.Length - 2 && s[i + 1] == '\r' && s[i + 2] == '\n') // \r\r\n
                {
                    lines.Add(start < i ? s[start..i] : string.Empty);
                    i += 3;
                    start = i;
                    continue;
                }

                if (i < s.Length - 1 && s[i + 1] == '\n') // \r\n
                {
                    lines.Add(start < i ? s[start..i] : string.Empty);
                    i += 2;
                    start = i;
                    continue;
                }

                lines.Add(start < i ? s[start..i] : string.Empty);
                i++;
                start = i;
                continue;
            }

            if (ch == '\n' || ch == '\u2028')
            {
                lines.Add(start < i ? s[start..i] : string.Empty);
                i++;
                start = i;
                continue;
            }

            i++;
        }

        lines.Add(start < i ? s[start..i] : string.Empty);
        return lines;
    }
    //-----------------------------------------------------------------------------------
    public static string ToBase64String(this string text)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    }
    //-----------------------------------------------------------------------------------
    public static string FromBase64String(this string base64String)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
    }
    //-----------------------------------------------------------------------------------
    public static string RemoveWhiteSpaces(this string text)
    {
        string findWhat = @"\s+";
        return Regex.Replace(text, findWhat, "");
    }
    //-----------------------------------------------------------------------------------
    public static XmlDocument ToXmlDocument(this XDocument xDocument)
    {
        var xmlDocument = new XmlDocument();
        using var xmlReader = xDocument.CreateReader();
        xmlDocument.Load(xmlReader);
        return xmlDocument;
    }
    //-----------------------------------------------------------------------------------
    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
        using var nodeReader = new XmlNodeReader(xmlDocument);
        nodeReader.MoveToContent();
        return XDocument.Load(nodeReader);
    }
    //-----------------------------------------------------------------------------------
    public static string? AssemblyDescription(this Assembly assembly)
    {
        if (assembly != null && Attribute.IsDefined(assembly, typeof(AssemblyDescriptionAttribute)))
        {
            AssemblyDescriptionAttribute? descriptionAttribute = (AssemblyDescriptionAttribute?)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
            if (descriptionAttribute != null)
            {
                return descriptionAttribute.Description;
            }
        }
        return null;
    }
    //-----------------------------------------------------------------------------------
    public static T IsNotNull<T>([NotNull] this T? value, [CallerArgumentExpression(parameterName: "value")] string? paramName = null)
    {
        if (value == null)
            throw new ArgumentNullException(paramName);
        else
            return value;
    } // Usage: someVariable.IsNotNull();
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Windows Only
    /// </summary>
    public static GraphicsPath? Shrink(this GraphicsPath path, float width)
    {
        if (!OperatingSystem.IsWindows()) return null;
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
        return path;
    }

    /// <summary>
    /// Windows Only
    /// </summary>
    private static int CountNextFigure(PathData data, int position)
    {
        int count = 0;
        if (!OperatingSystem.IsWindows()) return count;
        for (int i = position; i < data?.Types?.Length; i++)
        {
            count++;
            if (0 != (data.Types[i] & (int)PathPointType.CloseSubpath))
                return count;
        }
        return count;
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Windows Only
    /// </summary>
    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
    {
        if (!OperatingSystem.IsWindows()) return;
        GraphicsPath? path = DrawingTool.RoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        if (path != null)
            graphics.DrawPath(pen, path);
        graphics.SmoothingMode = SmoothingMode.Default;
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Windows Only
    /// </summary>
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
    {
        if (!OperatingSystem.IsWindows()) return;
        GraphicsPath? path = DrawingTool.RoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        if (path != null)
            graphics.FillPath(brush, path);
        graphics.SmoothingMode = SmoothingMode.Default;
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Windows Only
    /// </summary>
    public static void DrawCircle(this Graphics g, Pen pen, float centerX, float centerY, float radius)
    {
        if (!OperatingSystem.IsWindows()) return;
        g.DrawEllipse(pen, centerX - radius, centerY - radius, radius + radius, radius + radius);
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Windows Only
    /// </summary>
    public static void FillCircle(this Graphics g, Brush brush, float centerX, float centerY, float radius)
    {
        if (!OperatingSystem.IsWindows()) return;
        g.FillEllipse(brush, centerX - radius, centerY - radius, radius + radius, radius + radius);
    }
    //-----------------------------------------------------------------------------------
    public static string ToXml(this DataSet ds)
    {
        using var memoryStream = new MemoryStream();
        using TextWriter streamWriter = new StreamWriter(memoryStream);
        var xmlSerializer = new XmlSerializer(typeof(DataSet));
        xmlSerializer.Serialize(streamWriter, ds);
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
    //-----------------------------------------------------------------------------------
    public static string ToXmlWithWriteMode(this DataSet ds, XmlWriteMode xmlWriteMode)
    {
        using var ms = new MemoryStream();
        using TextWriter sw = new StreamWriter(ms);
        ds.WriteXml(sw, xmlWriteMode);
        return new UTF8Encoding(false).GetString(ms.ToArray());
    }
    //-----------------------------------------------------------------------------------
    public static DataSet ToDataSet(this DataSet ds, string xmlFile, XmlReadMode xmlReadMode)
    {
        ds.ReadXml(xmlFile, xmlReadMode);
        return ds;
    }
    //-----------------------------------------------------------------------------------
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
        return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
    }
    //-----------------------------------------------------------------------------------
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
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Change Color Hue. (0f - 360f)
    /// </summary>
    /// <returns>
    /// Returns Modified Color.
    /// </returns>
    public static Color ChangeHue(this Color color, float hue)
    {
        //float hueO = color.GetHue();
        float saturationO = color.GetSaturation();
        float lightnessO = color.GetBrightness();
        return ColorsTool.FromHsl(255, hue, saturationO, lightnessO);
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Change Color Saturation. (0f - 1f)
    /// </summary>
    /// <returns>
    /// Returns Modified Color.
    /// </returns>
    public static Color ChangeSaturation(this Color color, float saturation)
    {
        float hueO = color.GetHue();
        //float saturationO = color.GetSaturation();
        float lightnessO = color.GetBrightness();
        return ColorsTool.FromHsl(255, hueO, saturation, lightnessO);
    }
    //-----------------------------------------------------------------------------------
    public static void SaveToFile<T>(this List<T> list, string filePath)
    {
        try
        {
            FileStreamOptions streamOptions = new();
            streamOptions.Access = FileAccess.ReadWrite;
            streamOptions.Share = FileShare.ReadWrite;
            streamOptions.Mode = FileMode.Create;
            streamOptions.Options = FileOptions.RandomAccess;
            using StreamWriter file = new(filePath, streamOptions);
            for (int n = 0; n < list.Count; n++)
                if (list[n] != null)
                {
                    file.WriteLine(list[n]);
                }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Save List to File: {ex.Message}");
        }
    }
    //-----------------------------------------------------------------------------------
    public static void LoadFromFile(this List<string> list, string filePath, bool ignoreEmptyLines, bool trimLines)
    {
        if (!File.Exists(filePath)) return;
        string content = File.ReadAllText(filePath);
        List<string> lines = content.SplitToLines();
        for (int n = 0; n < lines.Count; n++)
        {
            string line = lines[n];
            if (ignoreEmptyLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (trimLines)
                        list.Add(line.Trim());
                    else
                        list.Add(line);
                }
            }
            else
            {
                if (trimLines)
                    list.Add(line.Trim());
                else
                    list.Add(line);
            }
        }
    }
    //-----------------------------------------------------------------------------------
    public static void LoadFromFile(this List<object> list, string filePath)
    {
        if (!File.Exists(filePath)) return;
        string content = File.ReadAllText(filePath);
        List<string> lines = content.SplitToLines();
        for (int n = 0; n < lines.Count; n++)
        {
            string line = lines[n];
            list.Add(line);
        }
    }
    //-----------------------------------------------------------------------------------
    public static int GetIndex<T>(this List<T> list, T value)
    {
        try
        {
            return list.FindIndex(a => a.Equals(value));
            // If the item is not found, it will return -1
        }
        catch (Exception)
        {
            return -1;
        }
    }
    //-----------------------------------------------------------------------------------
    public static void ChangeValue<T>(this List<T> list, T oldValue, T newValue)
    {
        list[list.GetIndex(oldValue)] = newValue;
    }
    //-----------------------------------------------------------------------------------
    public static void RemoveValue<T>(this List<T> list, T value)
    {
        list.RemoveAt(list.GetIndex(value));
    }
    //-----------------------------------------------------------------------------------
    public static List<T> RemoveDuplicates<T>(this List<T> list)
    {
        List<T> NoDuplicates = list.Distinct().ToList();
        return NoDuplicates;
    }
    //-----------------------------------------------------------------------------------
    public static void WriteToFile(this MemoryStream memoryStream, string dstPath)
    {
        using FileStream fs = new(dstPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        memoryStream.Seek(0, SeekOrigin.Begin);
        memoryStream.Position = 0;
        memoryStream.WriteTo(fs);
        fs.Flush();
    }
    //-----------------------------------------------------------------------------------
    public static void InvokeIt(this ISynchronizeInvoke sync, Action action)
    {
        // If the invoke is not required, then invoke here and get out.
        if (!sync.InvokeRequired)
        {
            action();
            return;
        }
        sync.Invoke(action, Array.Empty<object>());
        // Usage:
        // textBox1.InvokeIt(() => textBox1.Text = text);
    }
    //-----------------------------------------------------------------------------------
    public static bool Compare(this List<string> list1, List<string> list2)
    {
        return Enumerable.SequenceEqual(list1, list2);
    }

    public static bool Compare(this string string1, string string2)
    {
        return string1.Equals(string2, StringComparison.Ordinal);
    }
    //-----------------------------------------------------------------------------------
    public static bool IsInteger(this string s)
    {
        if (int.TryParse(s, out _))
            return true;
        return false;
    }
    //-----------------------------------------------------------------------------------
    public static bool IsBool(this string s)
    {
        if (bool.TryParse(s, out _))
            return true;
        return false;
    }
    //-----------------------------------------------------------------------------------
}