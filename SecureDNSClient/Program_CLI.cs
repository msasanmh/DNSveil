namespace SecureDNSClient;

internal static partial class Program
{
    public readonly struct Key
    {
        public static readonly string IsPortable = "IsPortable";
        public static readonly string IsStartup = "IsStartup";
        public static readonly string StartupDelaySec = "StartupDelaySec";
    }

    public class KeyValue
    {
        public KeyValue() { }

        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool ValueBool { get; set; } = false;
        public float ValueFloat { get; set; } = 0;
        public int ValueInt { get; set; } = 0;
        public Type Type { get; set; } = typeof(object);
    }

    private static KeyValue GetValue(string arg) // arg e.g. -IsStartup=True
    {
        KeyValue keyValue = new();
        try
        {
            arg = arg[1..];
            char separator = '=';
            if (arg.Contains(separator))
            {
                string[] split = arg.Split(separator);
                if (split.Length == 2)
                {
                    string argKey = split[0];
                    string argValue = split[1];
                    if (!string.IsNullOrEmpty(argKey) && !string.IsNullOrEmpty(argValue))
                    {
                        keyValue.Key = argKey;
                        keyValue.Value = argValue;

                        // Bool
                        bool isOk = bool.TryParse(keyValue.Value, out bool outBool);
                        if (isOk)
                        {
                            keyValue.ValueBool = outBool;
                            keyValue.Type = typeof(bool);
                        }

                        // Int
                        isOk = int.TryParse(keyValue.Value, out int outInt);
                        if (isOk)
                        {
                            keyValue.ValueInt = outInt;
                            keyValue.Type = typeof(int);
                        }
                    }
                }
            }
        }
        catch (Exception) { }
        return keyValue;
    }

}
