using MsmhToolsClass;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MsmhToolsClass;

public class JsonTool
{
    public static bool IsValid(string content)
    {
        bool result = false;

        try
        {
            if (!string.IsNullOrEmpty(content))
            {
                _ = JsonDocument.Parse(content);
                result = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("JsonTool IsValid: " + ex.Message);
        }

        return result;
    }

    public static bool IsValidFile(string jsonFilePath)
    {
        bool result = false;

        try
        {
            if (!string.IsNullOrEmpty(jsonFilePath))
            {
                string content = File.ReadAllText(jsonFilePath);
                _ = JsonDocument.Parse(content);
                result = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("JsonTool IsValidFile: " + ex.Message);
        }

        return result;
    }

    public static async Task<bool> IsValidFileAsync(string jsonFilePath)
    {
        bool result = false;

        try
        {
            if (!string.IsNullOrEmpty(jsonFilePath))
            {
                string content = await File.ReadAllTextAsync(jsonFilePath);
                _ = JsonDocument.Parse(content);
                result = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("JsonTool IsValidFileAsync: " + ex.Message);
        }

        return result;
    }

    public static string Serialize(object obj)
    {
        try
        {
            if (obj == null) return string.Empty;
            obj.SetEmptyValuesToNull();

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };

            return JsonSerializer.Serialize(obj, jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("JsonTool Serialize: " + ex.Message);
            return string.Empty;
        }
    }

    public static T? Deserialize<T>(string json)
    {
        try
        {
            JsonDocument jsonDocument = JsonDocument.Parse(json);

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };

            return JsonSerializer.Deserialize<T>(jsonDocument, jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("JsonTool Deserialize: " + ex.Message);
            return default;
        }
    }

    public struct JsonPath
    {
        /// <summary>
        /// Key (Name) To Find
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Break After N Elements
        /// </summary>
        public int Count { get; set; } = 0;
        /// <summary>
        /// Conditions To Match
        /// </summary>
        public List<JsonCondition> Conditions { get; set; } = new();

        public JsonPath(string key)
        {
            Key = key;
        }
        public JsonPath(string key, int count)
        {
            Key = key;
            Count = count;
        }
        public JsonPath(string key, int count, List<JsonCondition> conditions)
        {
            Key = key;
            Count = count;
            Conditions = conditions;
        }
    }

    public struct JsonCondition
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public JsonCondition(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    // e.g.
    // List<JsonTool.JsonPath> path = new()
    // {
    //     new JsonTool.JsonPath("assets", 1) { Conditions = new() { new("prerelease", "true") } },
    //     new JsonTool.JsonPath("browser_download_url") { Conditions = new() }
    // };
    public static List<string> GetValues(string jsonStr, List<JsonPath> paths)
    {
        List<string> values = new();

        try
        {
            jsonStr = jsonStr.Trim();
            if (string.IsNullOrEmpty(jsonStr)) return values;

            JsonDocumentOptions jsonDocumentOptions = new()
            {
                AllowTrailingCommas = true
            };
            using JsonDocument jsonDocument = JsonDocument.Parse(jsonStr, jsonDocumentOptions);
            JsonElement json = jsonDocument.RootElement;

            static bool checkCondition(JsonPath path, JsonElement element)
            {
                bool go = false;

                try
                {
                    int counted = 0;
                    foreach (JsonCondition condition in path.Conditions)
                    {
                        foreach (JsonProperty jp in element.EnumerateObject())
                        {
                            // JSON is case sensitive to both field names and data
                            if (condition.Key.Equals(jp.Name) &&
                                condition.Value.Equals(jp.Value.ToString().Trim().Trim('"'), StringComparison.OrdinalIgnoreCase)) // True False
                            {
                                counted++;
                            }
                        }
                        if (counted == path.Conditions.Count) break;
                    }
                    if (counted == path.Conditions.Count) go = true;
                    if (path.Conditions.Count == 0) go = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("JsonTool GetValues checkCondition: " + ex.Message);
                }

                return go;
            }

            static List<JsonElement> loop(JsonPath path, List<JsonElement> elements)
            {
                List<JsonElement> jsonElements = new();

                try
                {
                    for (int n = 0; n < elements.Count; n++)
                    {
                        JsonElement element = elements[n];

                        if (element.ValueKind == JsonValueKind.Object)
                        {
                            bool go = checkCondition(path, element);
                            if (go)
                            {
                                foreach (JsonProperty jp in element.EnumerateObject())
                                {
                                    if (path.Key.Equals(jp.Name))
                                    {
                                        jsonElements.Add(jp.Value);
                                        break;
                                    }
                                }
                            }
                        }
                        else if (element.ValueKind == JsonValueKind.Array)
                        {
                            List<JsonElement> jsonElements2 = loop(path, element.EnumerateArray().ToList());
                            jsonElements.AddRange(jsonElements2);
                        }

                        if (path.Count > 0 && n + 1 >= path.Count && jsonElements.Count > 0) break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("JsonTool GetValues loop: " + ex.Message);
                }

                return jsonElements;
            }

            if (paths.Count > 0)
            {
                static List<string> loop2(List<JsonElement> elements)
                {
                    List<string> values = new();

                    try
                    {
                        for (int n = 0; n < elements.Count; n++)
                        {
                            JsonElement element = elements[n];

                            if (element.ValueKind == JsonValueKind.Array)
                            {
                                List<string> values2 = loop2(element.EnumerateArray().ToList());
                                values.AddRange(values2);
                            }
                            else if (element.ValueKind == JsonValueKind.String ||
                                element.ValueKind == JsonValueKind.Number ||
                                element.ValueKind == JsonValueKind.True ||
                                element.ValueKind == JsonValueKind.False ||
                                element.ValueKind == JsonValueKind.Undefined)
                            {
                                values.Add(element.GetRawText().Trim().Trim('"'));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("JsonTool GetValues loop2: " + ex.Message);
                    }

                    return values;
                }

                List<JsonElement> jsonElements = new();
                for (int n = 0; n < paths.Count; n++)
                {
                    JsonPath p = paths[n];
                    if (n == 0)
                    {
                        jsonElements = loop(p, new List<JsonElement>() { json });
                    }
                    else
                    {
                        jsonElements = loop(p, jsonElements);
                    }
                }

                values = loop2(jsonElements);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("JsonTool GetValues: " + ex.Message);
        }

        return values;
    }

}
