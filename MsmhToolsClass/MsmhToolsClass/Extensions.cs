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
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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
        try
        {
            string remove = ", see inner exception";
            result += ex.Message;
            try { result = result.Replace(remove, string.Empty, StringComparison.OrdinalIgnoreCase).Trim(); } catch (Exception) { }

            Exception? exception = ex.InnerException;
            while (true)
            {
                if (exception == null) break;
                result += Environment.NewLine + exception.Message;
                exception = exception.InnerException;
            }
        }
        catch (Exception) { }
        return result;
    }

    public static T? Clone<T>(this T obj)
    {
        try
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
            };
            string json = JsonSerializer.Serialize(obj, jsonSerializerOptions);
            return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions Clone: " + ex.Message);
            return default;
        }
    }

    public static async Task<T?> CloneAsync<T>(this T obj)
    {
        try
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
            };
            using MemoryStream memoryStream1 = new();
            await JsonSerializer.SerializeAsync(memoryStream1, obj, jsonSerializerOptions);
            memoryStream1.Position = 0;
            using StreamReader streamReader = new(memoryStream1, new UTF8Encoding(false));
            string json = await streamReader.ReadToEndAsync();

            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            using MemoryStream memoryStream2 = new(jsonBytes);
            memoryStream2.Position = 0;
            return await JsonSerializer.DeserializeAsync<T>(memoryStream2, jsonSerializerOptions, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions CloneAsync: " + ex.Message);
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
            Debug.WriteLine("Extensions SetEmptyValuesToNull: " + ex.Message);
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

    /// <summary>
    /// If Name Exist In The Collection It gets Renamed Like Example (1), Example (2)
    /// </summary>
    public static string Rename(this string name, List<string> names)
    {
        try
        {
            int countName = 1;
            string newName = name;
            while (names.IsContain(newName))
            {
                bool hasNumber = false;
                if (newName.EndsWith(')'))
                {
                    int firstIndex = newName.LastIndexOf('(');
                    if (firstIndex != -1)
                    {
                        int lastIndex = newName.LastIndexOf(')');
                        if (lastIndex != -1)
                        {
                            string numberStr = newName.Substring(firstIndex + 1, newName.Length - lastIndex);
                            bool isInt = int.TryParse(numberStr, out int number);
                            if (isInt)
                            {
                                hasNumber = true;
                                newName = newName.Replace($" ({number})", "");
                                newName = string.Format("{0} ({1})", newName, number + 1);
                            }
                        }
                    }
                }

                if (!hasNumber) newName = string.Format("{0} ({1})", newName, countName++);
            }
            return newName;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions Rename: " + ex.Message);
            return name;
        }
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

    public static string TrimMiddle(this string source, char value)
    {
        string result = source;
        try
        {
            string find = $"{value}{value}";
            while (true)
            {
                if (result.Contains(find))
                {
                    result = result.Replace(find, value.ToString());
                }
                else break;
            }
        }
        catch (Exception) { }
        return result;
    }

    public static string TrimMiddle(this string source, string value)
    {
        string result = source;
        try
        {
            string find = $"{value}{value}";
            while (true)
            {
                if (result.Contains(find))
                {
                    result = result.Replace(find, value);
                }
                else break;
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

    public static string ToXmlString(this XDocument xDocument)
    {
        try
        {
            // Beautify XDocument
            xDocument = XDocument.Parse(xDocument.ToString(), LoadOptions.None);
            return xDocument.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToXmlString: " + ex.Message);
            return string.Empty;
        }
    }

    public static XmlDocument ToXmlDocument(this XDocument xDocument)
    {
        XmlDocument xmlDocument = new();

        try
        {
            using XmlReader xmlReader = xDocument.CreateReader();
            xmlDocument.Load(xmlReader);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToXmlDocument: " + ex.Message);
        }

        return xmlDocument;
    }
    
    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
        XDocument xDocument = new();

        try
        {
            using XmlNodeReader nodeReader = new(xmlDocument);
            nodeReader.MoveToContent();
            xDocument = XDocument.Load(nodeReader, LoadOptions.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToXDocument: " + ex.Message);
        }

        return xDocument;
    }

    public static async Task<XDocument> ToXDocumentAsync(this XmlDocument xmlDocument)
    {
        XDocument xDocument = new();

        try
        {
            using XmlNodeReader nodeReader = new(xmlDocument);
            nodeReader.MoveToContent();
            xDocument = await XDocument.LoadAsync(nodeReader, LoadOptions.None, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToXDocumentAsync: " + ex.Message);
        }

        return xDocument;
    }

    public static async Task<bool> SaveAsync(this JsonDocument jsonDocument, string jsonFilePath)
    {
        try
        {
            // Create Writer And Beautify JsonDocument
            MemoryStream memoryStream = new();
            Utf8JsonWriter jsonWriter = new(memoryStream, new JsonWriterOptions { Indented = true });

            // Write JsonDocument To The Writer
            jsonDocument.WriteTo(jsonWriter);
            await jsonWriter.FlushAsync();
            memoryStream.Position = 0;
            await memoryStream.FlushAsync();

            // Save To File
            FileStream fileStream = new(jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            await fileStream.WriteAsync(memoryStream.ToArray());

            // Dispose
            await fileStream.FlushAsync();
            await fileStream.DisposeAsync();
            await jsonWriter.DisposeAsync();
            await memoryStream.DisposeAsync();

            // Return
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions SaveAsync: " + ex.Message);
            return false;
        }
    }

    public static async Task<bool> SaveAsync(this JsonNode jsonNode, string jsonFilePath)
    {
        try
        {
            // Create Writer And Beautify JsonNode
            MemoryStream memoryStream = new();
            Utf8JsonWriter jsonWriter = new(memoryStream, new JsonWriterOptions { Indented = true });

            // Write JsonNode To The Writer
            jsonNode.WriteTo(jsonWriter);
            await jsonWriter.FlushAsync();
            memoryStream.Position = 0;
            await memoryStream.FlushAsync();

            // Save To File
            FileStream fileStream = new(jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            await fileStream.WriteAsync(memoryStream.ToArray());

            // Dispose
            await fileStream.FlushAsync();
            await fileStream.DisposeAsync();
            await jsonWriter.DisposeAsync();
            await memoryStream.DisposeAsync();

            // Return
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions SaveAsync: " + ex.Message);
            return false;
        }
    }

    public static async Task<string> ToJsonStringAsync(this JsonDocument jsonDocument)
    {
        try
        {
            // Create Writer And Beautify JsonDocument
            MemoryStream memoryStream = new();
            Utf8JsonWriter jsonWriter = new(memoryStream, new JsonWriterOptions { Indented = true });

            // Write JsonDocument To The Writer
            jsonDocument.WriteTo(jsonWriter);
            await jsonWriter.FlushAsync();
            memoryStream.Position = 0;
            await memoryStream.FlushAsync();

            // Get String
            string jsonStr = Encoding.UTF8.GetString(memoryStream.ToArray());
            
            // Dispose
            await jsonWriter.DisposeAsync();
            await memoryStream.DisposeAsync();

            // Return
            return jsonStr;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToJsonStringAsync: " + ex.Message);
            return string.Empty;
        }
    }

    public static async Task<string> ToJsonStringAsync(this JsonNode jsonNode)
    {
        try
        {
            // Create Writer And Beautify JsonNode
            MemoryStream memoryStream = new();
            Utf8JsonWriter jsonWriter = new(memoryStream, new JsonWriterOptions { Indented = true });

            // Write JsonNode To The Writer
            jsonNode.WriteTo(jsonWriter);
            await jsonWriter.FlushAsync();
            memoryStream.Position = 0;
            await memoryStream.FlushAsync();

            // Get String
            string jsonStr = Encoding.UTF8.GetString(memoryStream.ToArray());

            // Dispose
            await jsonWriter.DisposeAsync();
            await memoryStream.DisposeAsync();

            // Return
            return jsonStr;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToJsonStringAsync: " + ex.Message);
            return string.Empty;
        }
    }

    public static JsonNode? ToJsonNode(this JsonDocument jsonDocument)
    {
        try
        {
            return jsonDocument.Deserialize<JsonNode>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToJsonNode: " + ex.Message);
            return null;
        }
    }

    public static async Task<JsonNode?> ToJsonNodeAsync(this JsonDocument jsonDocument)
    {
        try
        {
            string? jsonStr = await jsonDocument.ToJsonStringAsync();
            if (string.IsNullOrEmpty(jsonStr))
            {
                Debug.WriteLine("Extensions ToJsonNodeAsync: jsonDocument.ToString() Is NULL.");
                return null;
            }
            return JsonNode.Parse(jsonStr);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToJsonNodeAsync: " + ex.Message);
            return null;
        }
    }

    public static JsonDocument? ToJsonDocument(this JsonNode jsonNode)
    {
        try
        {
            return jsonNode.Deserialize<JsonDocument>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToJsonDocument: " + ex.Message);
            return null;
        }
    }

    public static async Task<JsonDocument?> ToJsonDocumentAsync(this JsonNode jsonNode)
    {
        try
        {
            jsonNode.Deserialize<JsonDocument>();

            // Create Writer And Beautify JsonNode
            MemoryStream memoryStream = new();
            Utf8JsonWriter jsonWriter = new(memoryStream, new JsonWriterOptions { Indented = true });

            // Write JsonNode To The Writer
            jsonNode.WriteTo(jsonWriter);
            await jsonWriter.FlushAsync();
            memoryStream.Position = 0;
            await memoryStream.FlushAsync();

            // Create Options - Comments cannot be stored in a JsonDocument, only the Skip and Disallow comment handling modes are supported.
            JsonDocumentOptions jsonDocumentOptions = new()
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
                MaxDepth = 0
            };

            // Convert
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(memoryStream, jsonDocumentOptions);

            // Dispose
            await jsonWriter.DisposeAsync();
            await memoryStream.DisposeAsync();

            // Return
            return jsonDocument;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToJsonDocumentAsync: " + ex.Message);
            return null;
        }
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
    
    public static async Task<string> ToXmlAsync(this DataSet ds, XmlWriteMode xmlWriteMode)
    {
        try
        {
            using MemoryStream memoryStream = new();
            using TextWriter textWriter = new StreamWriter(memoryStream);
            ds.WriteXml(textWriter, xmlWriteMode);
            await textWriter.FlushAsync();
            memoryStream.Position = 0;
            await memoryStream.FlushAsync();
            return new UTF8Encoding(false).GetString(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToXmlAsync: " + ex.Message);
            return string.Empty;
        }
    }
    
    public static async Task<DataSet> ToDataSetAsync(this DataSet ds, string xmlFilePath, XmlReadMode xmlReadMode)
    {
        try
        {
            byte[] buffer = await File.ReadAllBytesAsync(xmlFilePath);
            using MemoryStream memoryStream = new();
            await memoryStream.WriteAsync(buffer);
            memoryStream.Position = 0;
            await memoryStream.FlushAsync();
            ds.ReadXml(memoryStream, xmlReadMode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions ToDataSetAsync: " + ex.Message);
        }

        return ds;
    }
    
    public static async Task<bool> WriteToFileAsync(this MemoryStream memoryStream, string filePath)
    {
        try
        {
            using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.Position = 0;
            await fileStream.WriteAsync(memoryStream.ToArray());
            fileStream.Flush();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Extensions WriteToFileAsync: " + ex.Message);
            return false;
        }
    }
    
}