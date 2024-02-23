using System.Diagnostics;
using System.Text.Json;

namespace MsmhToolsClass;

public class JsonTool
{
    public static bool IsValidJson(string content)
    {
        bool result = false;

        try
        {
            if (!string.IsNullOrEmpty(content))
            {
                JsonDocument.Parse(content);
                result = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("IsValidJson: " + ex.Message);
        }

        return result;
    }

    public static bool IsValidJsonFile(string jsonFilePath)
    {
        bool result = false;

        try
        {
            if (!string.IsNullOrEmpty(jsonFilePath))
            {
                string content = File.ReadAllText(jsonFilePath);
                JsonDocument.Parse(content);
                result = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("IsValidJsonFile: " + ex.Message);
        }

        return result;
    }

    public static List<string> GetValues(string jsonStr, List<string> path)
    {
        List<string> values = new();

        jsonStr = jsonStr.Trim();
        if (string.IsNullOrEmpty(jsonStr)) return values;

        try
        {
            JsonDocumentOptions jsonDocumentOptions = new()
            {
                AllowTrailingCommas = true
            };
            using JsonDocument jsonDocument = JsonDocument.Parse(jsonStr, jsonDocumentOptions);
            JsonElement json = jsonDocument.RootElement;

            static List<JsonElement> loop(string path, List<JsonElement> elements)
            {
                List<JsonElement> jsonElements = new();

                try
                {
                    for (int n = 0; n < elements.Count; n++)
                    {
                        JsonElement element = elements[n];
                        
                        if (element.ValueKind == JsonValueKind.Object)
                        {
                            foreach (JsonProperty jp in element.EnumerateObject())
                            {
                                if (path.Equals(jp.Name, StringComparison.OrdinalIgnoreCase)) jsonElements.Add(jp.Value);
                            }
                        }
                        else if (element.ValueKind == JsonValueKind.Array)
                        {
                            List<JsonElement> jsonElements2 = loop(path, element.EnumerateArray().ToList());
                            jsonElements.AddRange(jsonElements2);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Json Tool, Get Values: " + ex.Message);
                }

                return jsonElements;
            }

            if (path.Any())
            {
                List<JsonElement> jsonElements = new();
                for (int n = 0; n < path.Count; n++)
                {
                    string p = path[n];
                    if (n == 0)
                    {
                        jsonElements = loop(p, new List<JsonElement>() { json });
                    }
                    else
                    {
                        jsonElements = loop(p, jsonElements);
                    }
                }

                for (int n = 0; n < jsonElements.Count; n++)
                {
                    JsonElement jsonElement = jsonElements[n];
                    string output = jsonElement.GetRawText().Trim().Trim('"');
                    if (!output.StartsWith('{') && !output.StartsWith('[')) values.Add(output);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Json Tool, Get Values: " + ex.Message);
        }

        return values;
    }
}
