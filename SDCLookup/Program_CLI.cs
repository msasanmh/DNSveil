namespace SDCLookup;

internal static partial class Program
{
    public readonly struct Key
    {
        public static readonly string Domain = "Domain";
        public static readonly string Type = "Type";
        public static readonly string Class = "Class";
        public static readonly string DNSs = "DNSs";
        public static readonly string TimeoutMS = "TimeoutMS";
        public static readonly string Insecure = "Insecure";
        public static readonly string BootstrapIP = "BootstrapIP";
        public static readonly string BootstrapPort = "BootstrapPort";
        public static readonly string ProxyScheme = "ProxyScheme";
        public static readonly string ProxyUser = "ProxyUser";
        public static readonly string ProxyPass = "ProxyPass";
        public static readonly string DoubleCheck = "DoubleCheck";
    }

    public class KeyValue
    {
        public KeyValue() { }

        public bool IsSuccess { get; set; } = false;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string ValueString { get; set; } = string.Empty;
        public bool ValueBool { get; set; } = false;
        public int ValueInt { get; set; } = 0;
    }

    private static KeyValue GetValue(string arg, string key, Type keyType) // arg e.g. -Domain=example.com
    {
        KeyValue keyValue = new();
        try
        {
            keyValue.Key = key;
            key = $"-{key}=";
            if (arg.Contains(key, StringComparison.OrdinalIgnoreCase) && arg.Length > key.Length)
            {
                keyValue.Value = arg[key.Length..];
                
                if (keyType == typeof(string))
                {
                    keyValue.ValueString = keyValue.Value;
                    keyValue.IsSuccess = true;
                }
                else if (keyType == typeof(bool))
                {
                    bool isBool = bool.TryParse(keyValue.Value, out bool outBool);
                    if (isBool)
                    {
                        keyValue.ValueBool = outBool;
                        keyValue.IsSuccess = true;
                    }
                }
                else if (keyType == typeof(int))
                {
                    bool isInt = int.TryParse(keyValue.Value, out int outInt);
                    if (isInt)
                    {
                        keyValue.ValueInt = outInt;
                        keyValue.IsSuccess = true;
                    }
                }
            }
        }
        catch (Exception) { }
        return keyValue;
    }
}