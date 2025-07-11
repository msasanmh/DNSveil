using System.Reflection;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Xml.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Collections;

namespace MsmhToolsClass;

public static class Methods
{
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    internal extern static int SetWindowTheme(IntPtr controlHandle, string appName, string? idList);
}
public static class Extensions
{
    /// <summary>
    /// Invoke WinForms Controls
    /// </summary>
    public static void InvokeIt(this ISynchronizeInvoke sync, Action action)
    {
        // If Invoke Is Not Required
        if (!sync.InvokeRequired)
        {
            action();
            return;
        }
        sync.Invoke(action, Array.Empty<object>());
        // Usage:
        // textBox1.InvokeIt(() => textBox1.Text = text);
    }
    
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

    public static T? Clone<T>(this T t)
    {
        try
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            };
            string json = JsonSerializer.Serialize(t, jsonSerializerOptions);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions Clone: " + ex.Message);
            return default;
        }
    }

    public static int SetEmptyValuesToNull(this object obj)
    {
        try
        {
            if (obj == null) return -1;
            Type type = obj.GetType();
            if (type.IsPrimitive) return -1;
            if (type.IsEnum) return -1;

            // If It's A List
            if (obj is IList list0)
            {
                foreach (var item in list0) SetEmptyValuesToNull(item);
                return -1;
            }

            // If It's A Dictionary
            else if (obj is IDictionary dict0)
            {
                foreach (var itemValue in dict0.Values) SetEmptyValuesToNull(itemValue);
                return -1;
            }

            // Recursively Check Properties
            PropertyInfo[] propInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            foreach (PropertyInfo prop in propInfos)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                if (prop.PropertyType.IsPrimitive) continue;
                if (prop.PropertyType.IsEnum) continue;
                if (prop.GetIndexParameters().Length > 0) continue; // Skip Indexers
                object? value = prop.GetValue(obj);
                if (value == null) continue;

                // If It's An Empty String
                if (value is string str && string.IsNullOrEmpty(str))
                {
                    prop.SetValue(obj, null);
                }

                // If It's A List
                else if (value is IList list)
                {
                    if (list.Count == 0)
                    {
                        prop.SetValue(obj, null);
                    }
                    else
                    {
                        foreach (var item in list) SetEmptyValuesToNull(item);
                    }
                }

                // If It's A Dictionary
                else if (value is IDictionary dict)
                {
                    if (dict.Count == 0)
                    {
                        prop.SetValue(obj, null);
                    }
                    else
                    {
                        foreach (var itemValue in dict.Values) SetEmptyValuesToNull(itemValue);
                    }
                }

                // Recursive Into Nested Objects
                else
                {
                    int propsCount = SetEmptyValuesToNull(value);
                    if (propsCount == 0)
                    {
                        // Set The Value Of Classes With No Public Instance Properties To NULL (Where propInfos.Length Is 0)
                        prop.SetValue(obj, null);
                    }
                }
            }
            return propInfos.Length;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToInt: " + ex.Message);
            return -1;
        }
    }

    public static int ToInt(this string? Str, int defaultValue)
    {
        int result = defaultValue;
        try
        {
            bool isInt = int.TryParse(Str, out int valueOut);
            if (isInt) result = valueOut;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToInt: " + ex.Message);
        }
        return result;
    }

    public static int ToInt(this double value)
    {
        int result = 0;
        try
        {
            result = Convert.ToInt32(value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToInt: " + ex.Message);
        }
        return result;
    }

    public static string RemoveEmptyLines(this string s)
    {
        try
        {
            string[] split = s.ReplaceLineEndings().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(Environment.NewLine, split);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions RemoveEmptyLines: " + ex.Message);
            return s;
        }
    }

    public static bool Contains(this string s, char[] chars)
    {
        try
        {
            int match = s.IndexOfAny(chars);
            return match != -1;
        }
        catch (Exception) { }
        return false;
    }

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
            Debug.WriteLine("Extensions RemoveChar 1: " + ex.Message);
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
            Debug.WriteLine("Extensions RemoveChar 2: " + ex.Message);
            return value;
        }
    }
    
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
    
    public static string TrimEnd(this string source, string value)
    {
        string result = source;
        try
        {
            if (result.EndsWith(value))
            {
                result = result[..source.LastIndexOf(value, StringComparison.Ordinal)];
            }
        }
        catch (Exception) { }
        return result;
    }
    
    public static string RemoveWhiteSpaces(this string text)
    {
        try
        {
            string findWhat = @"\s+";
            return Regex.Replace(text, findWhat, "");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions RemoveWhiteSpaces: " + ex.Message);
            return text;
        }
    }

    public static string ToStringNoScopeId(this IPAddress ip)
    {
        try
        {
            string ipStr = ip.ToString();
            int percentIndex = ipStr.IndexOf('%');
            return percentIndex >= 0 ? ipStr[..percentIndex] : ipStr;
        }
        catch (Exception)
        {
            return ip.ToString();
        }
    }

    public static string ToString(this IPEndPoint ipEP, bool removePortZero)
    {
        try
        {
            string epStr = ipEP.ToString();
            if (removePortZero && epStr.EndsWith(":0")) epStr = epStr.TrimEnd(":0");
            return epStr;
        }
        catch (Exception)
        {
            return ipEP.ToString();
        }
    }

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

    public static async Task<(bool IsLoaded, XDocument XDoc)> LoadAsync(this XDocument xDoc, string path)
    {
        bool isSuccess = false;

        try
        {
            XmlReaderSettings readerSettings = new()
            {
                Async = true,
                CloseInput = true
            };
            XmlReader xmlReader = XmlReader.Create(path, readerSettings);
            xDoc = await XDocument.LoadAsync(xmlReader, LoadOptions.None, CancellationToken.None);
            xmlReader.Close();
            xmlReader.Dispose();
            isSuccess = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions LoadAsync: " + ex.Message);
        }

        return (isSuccess, xDoc);
    }

    public static async Task<bool> SaveAsync(this XDocument xDocument, string xmlFilePath)
    {
        try
        {
            // Create Writer
            XmlWriterSettings xmlWriterSettings = new()
            {
                Async = true,
                CloseOutput = true,
                Encoding = new UTF8Encoding(false),
                Indent = true,
                IndentChars = "  ",
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = true,
                WriteEndDocumentOnClose = true
            };
            XmlWriter xmlWriter = XmlWriter.Create(xmlFilePath, xmlWriterSettings);

            // Beautify XDocument
            xDocument = XDocument.Parse(xDocument.ToString(), LoadOptions.None);

            // Save
            await xDocument.SaveAsync(xmlWriter, CancellationToken.None);

            // Dispose
            xmlWriter.Close();
            await xmlWriter.FlushAsync();
            await xmlWriter.DisposeAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions SaveAsync: " + ex.Message);
            return false;
        }
    }
    
    public static XmlDocument ToXmlDocument(this XDocument xDocument)
    {
        XmlDocument xmlDocument = new();
        using XmlReader xmlReader = xDocument.CreateReader();
        xmlDocument.Load(xmlReader);
        return xmlDocument;
    }
    
    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
        using XmlNodeReader nodeReader = new(xmlDocument);
        nodeReader.MoveToContent();
        return XDocument.Load(nodeReader);
    }
    
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
            Debug.WriteLine("Extensions AssemblyDescription: " + ex.Message);
        }

        return string.Empty;
    }
    
    public static T IsNotNull<T>([NotNull] this T? value, [CallerArgumentExpression(parameterName: "value")] string? paramName = null)
    {
        if (value == null) throw new ArgumentNullException(paramName);
        else return value;
    } // Usage: someVariable.IsNotNull();
    
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
            Debug.WriteLine("Extensions ToXml: " + ex.Message);
            return string.Empty;
        }
    }
    
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
            Debug.WriteLine("Extensions ToXml: " + ex.Message);
            return string.Empty;
        }
    }
    
    public static DataSet ToDataSet(this DataSet ds, string xmlFile, XmlReadMode xmlReadMode)
    {
        try
        {
            ds.ReadXml(xmlFile, xmlReadMode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToDataSet: " + ex.Message);
        }

        return ds;
    }
    
    public static void WriteToFile(this MemoryStream memoryStream, string dstPath)
    {
        try
        {
            using FileStream fs = new(dstPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.Position = 0;
            memoryStream.WriteTo(fs);
            fs.Flush();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions WriteToFile: " + ex.Message);
        }
    }
    
    public static bool IsInteger(this string s)
    {
        return int.TryParse(s, out _);
    }
    
    public static bool IsBool(this string s)
    {
        return bool.TryParse(s, out _);
    }
    
}