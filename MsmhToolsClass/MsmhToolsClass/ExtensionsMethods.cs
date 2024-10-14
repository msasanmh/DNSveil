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
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Globalization;

namespace MsmhToolsClass;

public static class Methods
{
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    internal extern static int SetWindowTheme(IntPtr controlHandle, string appName, string? idList);
}
public static class ExtensionsMethods
{
    //-----------------------------------------------------------------------------------
    public static bool TryUpdate<K, V>(this ConcurrentDictionary<K, V> ccDic, K key, V newValue) where K : notnull
    {
        try
        {
            if (key == null) return false;
            bool isKeyExist = ccDic.TryGetValue(key, out V? oldValue);
            if (isKeyExist && oldValue != null)
                return ccDic.TryUpdate(key, newValue, oldValue);
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods TryUpdate: " + ex.Message);
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
    public static V? AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> ccDic, K key, V newValue) where K : notnull
    {
        try
        {
            if (key == null) return default;
            return ccDic.AddOrUpdate(key, newValue, (oldkey, oldvalue) => newValue);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods AddOrUpdate: " + ex.Message);
            return default;
        }
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// To List
    /// </summary>
    public static List<Tuple<string, string>> ToList(this NameValueCollection nvc)
    {
        List<Tuple<string, string>> result = new();
        
        try
        {
            for (int n = 0; n < nvc.Count; n++)
            {
                string? key = nvc.GetKey(n);
                string? val = nvc.Get(n);
                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(val)) continue;
                result.Add(new Tuple<string, string>(key, val));
            }
        }
        catch (Exception) { }

        return result;
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// Get Value By Key
    /// </summary>
    /// <returns>Returns string.Empty If Key Not Exist Or Value Is Empty.</returns>
    public static string GetValueByKey(this NameValueCollection nvc, string? key)
    {
        string result = string.Empty;
        if (string.IsNullOrWhiteSpace(key)) return result;
        
        try
        {
            string? value = nvc[key];
            result = value ?? string.Empty;
        }
        catch (Exception) { }

        return result;
    }
    //-----------------------------------------------------------------------------------
    /// <summary>
    /// If Key Exist Adds The Value (Comma-Separated)
    /// </summary>
    public static void AddAndUpdate(this NameValueCollection nvc, string? key, string? value)
    {
        try
        {
            if (string.IsNullOrEmpty(key)) return;
            if (string.IsNullOrEmpty(value)) return;

            string? theKey = nvc[key];
            if (!string.IsNullOrEmpty(theKey)) // Key Exist
            {
                string tempVal = theKey;
                tempVal += "," + value;
                nvc.Remove(key);
                nvc.Add(key, tempVal);
            }
            else
            {
                nvc.Add(key, value);
            }
        }
        catch (Exception) { }
    }
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
    public static string CapitalizeFirstLetter(this string s, CultureInfo? ci = null)
    {
        try
        {
            StringInfo si = new(s);
            ci ??= CultureInfo.CurrentCulture;

            if (si.LengthInTextElements > 0)
            {
                s = si.SubstringByTextElements(0, 1).ToUpper(ci);
            }

            if (si.LengthInTextElements > 1)
            {
                s += si.SubstringByTextElements(1);
            }
        }
        catch (Exception) { }

        return s;
    }
    //-----------------------------------------------------------------------------------
    public static string RemoveChar(this string value, char charToRemove)
    {
        try
        {
            char[] array = new char[value.Length];
            int arrayIndex = 0;

            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                if (ch != charToRemove)
                {
                    array[arrayIndex++] = ch;
                }
            }

            return new string(array, 0, arrayIndex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("RemoveChar 1: " + ex.Message);
            return value;
        }
    }

    public static string RemoveChar(this string value, params char[] charsToRemove)
    {
        try
        {
            HashSet<char> h = new(charsToRemove);
            char[] array = new char[value.Length];
            int arrayIndex = 0;

            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                if (!h.Contains(ch))
                {
                    array[arrayIndex++] = ch;
                }
            }

            return new string(array, 0, arrayIndex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("RemoveChar 2: " + ex.Message);
            return value;
        }
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
        try
        {
            bool part1 = socket.Poll(1000, selectMode);
            bool part2 = socket.Available == 0;
            return !part1 || !part2;
        }
        catch (Exception)
        {
            return false;
        }
    }
    //-----------------------------------------------------------------------------------
    public static async Task SaveAsync(this XDocument xDocument, string xmlFilePath)
    {
        try
        {
            XmlWriterSettings xmlWriterSettings = new()
            {
                WriteEndDocumentOnClose = true,
                Async = true,
                Indent = true,
                OmitXmlDeclaration = true,
                Encoding = new UTF8Encoding(false)
            };
            using XmlWriter xmlWriter = XmlWriter.Create(xmlFilePath, xmlWriterSettings);
            await xDocument.SaveAsync(xmlWriter, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods SaveAsync: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static string ToString<T>(this List<T> list, char separator)
    {
        string result = string.Empty;

        try
        {
            if (list.Count > 0) result = string.Join(separator, list);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods ToString<T> Char: " + ex.Message);
        }

        return result;
    }
    //-----------------------------------------------------------------------------------
    public static string ToString<T>(this List<T> list, string separator)
    {
        string result = string.Empty;

        try
        {
            //for (int n = 0; n < list.Count; n++)
            //{
            //    T t = list[n];
            //    result += $"{t}{separator}";
            //}
            //if (result.EndsWith(separator)) result = result.TrimEnd(separator);
            if (list.Count > 0) result = string.Join(separator, list);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods ToString<T> String: " + ex.Message);
        }

        return result;
    }
    //-----------------------------------------------------------------------------------
    public static bool IsContain<T>(this List<T> list, T t)
    {
        try
        {
            for (int n = 0; n < list.Count; n++)
                if (t != null && t.Equals(list[n])) return true;
        }
        catch (Exception) { }
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
        List<string> lines = new();

        try
        {
            s = s.ReplaceLineEndings();
            string[] split = s.Split(Environment.NewLine);
            if (split.Length > 0) lines = split.ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods SplitToLines: " + ex.Message);
        }

        return lines;
    }

    public static List<string> SplitToLinesOld(this string s)
    {
        // Original non-optimized version: return source.Replace("\r\r\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');
        List<string> lines = new();

        try
        {
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

            if (start < i) lines.Add(s[start..i]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods SplitToLinesOld: " + ex.Message);
        }

        return lines;
    }
    //-----------------------------------------------------------------------------------
    public static string RemoveWhiteSpaces(this string text)
    {
        try
        {
            string findWhat = @"\s+";
            return Regex.Replace(text, findWhat, "");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods RemoveWhiteSpaces: " + ex.Message);
            return text;
        }
    }
    //-----------------------------------------------------------------------------------
    public static XmlDocument ToXmlDocument(this XDocument xDocument)
    {
        XmlDocument xmlDocument = new();
        using XmlReader xmlReader = xDocument.CreateReader();
        xmlDocument.Load(xmlReader);
        return xmlDocument;
    }
    //-----------------------------------------------------------------------------------
    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
        using XmlNodeReader nodeReader = new(xmlDocument);
        nodeReader.MoveToContent();
        return XDocument.Load(nodeReader);
    }
    //-----------------------------------------------------------------------------------
    public static string AssemblyDescription(this Assembly assembly)
    {
        try
        {
            if (assembly != null && Attribute.IsDefined(assembly, typeof(AssemblyDescriptionAttribute)))
            {
                AssemblyDescriptionAttribute? descriptionAttribute = (AssemblyDescriptionAttribute?)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                if (descriptionAttribute != null) return descriptionAttribute.Description;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods AssemblyDescription: " + ex.Message);
        }

        return string.Empty;
    }
    //-----------------------------------------------------------------------------------
    public static T IsNotNull<T>([NotNull] this T? value, [CallerArgumentExpression(parameterName: "value")] string? paramName = null)
    {
        if (value == null) throw new ArgumentNullException(paramName);
        else return value;
    } // Usage: someVariable.IsNotNull();
    //-----------------------------------------------------------------------------------
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
            Debug.WriteLine("ExtensionsMethods Shrink: " + ex.Message);
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
            Debug.WriteLine("ExtensionsMethods CountNextFigure: " + ex.Message);
        }

        return count;
    }
    //-----------------------------------------------------------------------------------
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
            Debug.WriteLine("ExtensionsMethods DrawRoundedRectangle: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
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
            Debug.WriteLine("ExtensionsMethods FillRoundedRectangle: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
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
            Debug.WriteLine("ExtensionsMethods DrawCircle: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
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
            Debug.WriteLine("ExtensionsMethods FillCircle: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static string ToXml(this DataSet ds)
    {
        try
        {
            using MemoryStream memoryStream = new();
            using TextWriter streamWriter = new StreamWriter(memoryStream);
            XmlSerializer xmlSerializer = new(typeof(DataSet));
            xmlSerializer.Serialize(streamWriter, ds);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods ToXml: " + ex.Message);
            return string.Empty;
        }
    }
    //-----------------------------------------------------------------------------------
    public static string ToXml(this DataSet ds, XmlWriteMode xmlWriteMode)
    {
        try
        {
            using MemoryStream ms = new();
            using TextWriter sw = new StreamWriter(ms);
            ds.WriteXml(sw, xmlWriteMode);
            return new UTF8Encoding(false).GetString(ms.ToArray());
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods ToXml: " + ex.Message);
            return string.Empty;
        }
    }
    //-----------------------------------------------------------------------------------
    public static DataSet ToDataSet(this DataSet ds, string xmlFile, XmlReadMode xmlReadMode)
    {
        try
        {
            ds.ReadXml(xmlFile, xmlReadMode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods ToDataSet: " + ex.Message);
        }

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
            Debug.WriteLine("ExtensionsMethods ChangeBrightness: " + ex.Message);
            return color;
        }
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
        try
        {
            //float hueO = color.GetHue();
            float saturationO = color.GetSaturation();
            float lightnessO = color.GetBrightness();
            return ColorsTool.FromHsl(255, hue, saturationO, lightnessO);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods ChangeHue: " + ex.Message);
            return color;
        }
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
        try
        {
            float hueO = color.GetHue();
            //float saturationO = color.GetSaturation();
            float lightnessO = color.GetBrightness();
            return ColorsTool.FromHsl(255, hueO, saturation, lightnessO);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods ChangeSaturation: " + ex.Message);
            return color;
        }
    }
    //-----------------------------------------------------------------------------------
    public static async Task SaveToFileAsync(this List<string> list, string filePath)
    {
        try
        {
            FileStreamOptions streamOptions = new()
            {
                Access = FileAccess.ReadWrite,
                Share = FileShare.ReadWrite,
                Mode = FileMode.Create,
                Options = FileOptions.RandomAccess
            };
            using StreamWriter file = new(filePath, streamOptions);
            for (int n = 0; n < list.Count; n++)
                if (list[n] != null)
                {
                    await file.WriteLineAsync(list[n]);
                }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ExtensionsMethods SaveToFileAsync: {ex.Message}");
        }
    }
    //-----------------------------------------------------------------------------------
    public static async Task LoadFromFileAsync(this List<string> list, string filePath, bool ignoreEmptyLines, bool trimLines)
    {
        try
        {
            if (!File.Exists(filePath)) return;
            string content = await File.ReadAllTextAsync(filePath);
            List<string> lines = content.SplitToLines();
            for (int n = 0; n < lines.Count; n++)
            {
                string line = lines[n];
                if (ignoreEmptyLines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (trimLines) list.Add(line.Trim());
                        else list.Add(line);
                    }
                }
                else
                {
                    if (trimLines) list.Add(line.Trim());
                    else list.Add(line);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods LoadFromFileAsync: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static int GetIndex<T>(this List<T> list, T value)
    {
        try
        {
            return list.FindIndex(a => a != null && a.Equals(value));
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
        try
        {
            list[list.GetIndex(oldValue)] = newValue;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods ChangeValue<T>: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static void RemoveValue<T>(this List<T> list, T value)
    {
        try
        {
            list.RemoveAt(list.GetIndex(value));
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods RemoveValue<T>: " + ex.Message);
        }
    }
    //-----------------------------------------------------------------------------------
    public static List<T> RemoveDuplicates<T>(this List<T> list)
    {
        try
        {
            List<T> NoDuplicates = list.Distinct().ToList();
            return NoDuplicates;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ExtensionsMethods RemoveDuplicates: " + ex.Message);
            return list;
        }
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
    //-----------------------------------------------------------------------------------
    public static bool IsInteger(this string s)
    {
        if (int.TryParse(s, out _)) return true;
        return false;
    }
    //-----------------------------------------------------------------------------------
    public static bool IsBool(this string s)
    {
        if (bool.TryParse(s, out _)) return true;
        return false;
    }
    //-----------------------------------------------------------------------------------
}