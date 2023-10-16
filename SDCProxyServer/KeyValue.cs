namespace SDCProxyServer;

public class KeyValue
{
    public KeyValue(string key, bool isKeyMandatory, bool requireDoubleQuotes, Type type, int min, int max)
    {
        Key = key;
        IsKeyMandatory = isKeyMandatory;
        RequireDoubleQuotes = requireDoubleQuotes;
        Type = type;
        Min = min;
        Max = max;
    }

    public string Key { get; set; }
    public bool IsKeyMandatory { get; set; }
    public bool RequireDoubleQuotes { get; set; }
    public object Value { get; set; } = string.Empty;
    public bool ValueBool { get; set; } = false;
    public float ValueFloat { get; set; } = -1;
    public int ValueInt { get; set; } = -1;
    public string ValueString { get; set; } = string.Empty;
    public Type Type { get; set; }
    public int Min { get; set; }
    public int Max { get; set; }
}

public class KeyValues
{
    private readonly List<KeyValue> KeyValueList = new();

    public KeyValues()
    {
        KeyValueList.Clear();
    }

    public void Add(string key, bool isKeyMandatory, bool requireDoubleQuotes, Type type, int min, int max)
    {
        KeyValue keyValue = new(key, isKeyMandatory, requireDoubleQuotes, type, min, max);
        KeyValueList.Add(keyValue);
    }

    public void Add(string key, bool isKeyMandatory, bool requireDoubleQuotes, Type type)
    {
        KeyValue keyValue = new(key, isKeyMandatory, requireDoubleQuotes, type, -1, -1);
        KeyValueList.Add(keyValue);
    }

    public List<KeyValue> Get() => KeyValueList;

    public bool GetValuesByKeys(string input, out List<KeyValue> listOut)
    {
        List<KeyValue> list = KeyValueList;
        int count = 0;
        for (int n1 = 0; n1 < list.Count; n1++)
        {
            KeyValue kv1 = list[n1];
            for (int n2 = 0; n2 < list.Count; n2++)
            {
                KeyValue kv2 = list[n2];
                if (kv1.Key.Equals(kv2.Key))
                {
                    bool isValueOk;

                    if (kv1.Type == typeof(bool))
                    {
                        isValueOk = ConsoleTools.GetValueByKey(input, kv1.Key, kv1.IsKeyMandatory, kv1.RequireDoubleQuotes, out string value);
                        if (!isValueOk) continue;
                        isValueOk = ConsoleTools.GetBool(kv1.Key, value, kv1.IsKeyMandatory, out bool result);
                        if (!isValueOk) continue;
                        kv1.Value = result;
                        kv1.ValueBool = result;
                        count++;
                    }

                    if (kv1.Type == typeof(float))
                    {
                        isValueOk = ConsoleTools.GetValueByKey(input, kv1.Key, kv1.IsKeyMandatory, kv1.RequireDoubleQuotes, out string value);
                        if (!isValueOk) continue;
                        isValueOk = ConsoleTools.GetFloat(kv1.Key, value, kv1.IsKeyMandatory, kv1.Min, kv1.Max, out float n);
                        if (!isValueOk) continue;
                        kv1.Value = n;
                        kv1.ValueFloat = n;
                        count++;
                    }

                    if (kv1.Type == typeof(int))
                    {
                        isValueOk = ConsoleTools.GetValueByKey(input, kv1.Key, kv1.IsKeyMandatory, kv1.RequireDoubleQuotes, out string value);
                        if (!isValueOk) continue;
                        isValueOk = ConsoleTools.GetInt(kv1.Key, value, kv1.IsKeyMandatory, kv1.Min, kv1.Max, out int n);
                        if (!isValueOk) continue;
                        kv1.Value = n;
                        kv1.ValueInt = n;
                        count++;
                    }

                    if (kv1.Type == typeof(string))
                    {
                        isValueOk = ConsoleTools.GetValueByKey(input, kv1.Key, kv1.IsKeyMandatory, kv1.RequireDoubleQuotes, out string value);
                        if (!isValueOk) continue;
                        isValueOk = ConsoleTools.GetString(kv1.Key, value, kv1.IsKeyMandatory, out value);
                        if (!isValueOk) continue;
                        kv1.Value = value;
                        kv1.ValueString = value;
                        count++;
                    }
                }
            }
        }

        listOut = list;
        return count == list.Count;
    }
}