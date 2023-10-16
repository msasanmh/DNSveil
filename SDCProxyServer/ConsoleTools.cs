using System.Reflection;
using System.Runtime.InteropServices;

namespace SDCProxyServer;

public static class ConsoleTools
{
    public static void AddCommand(this List<string> list, string baseCmd, string cmd)
    {
        bool isExist = false;
        for (int n = 0; n < list.Count; n++)
        {
            if (list[n].StartsWith(baseCmd))
            {
                list[n] = cmd;
                isExist = true;
            }
        }
        if (!isExist) list.Add(cmd);
    }

    public static string? GetCommandsPath()
    {
        try
        {
            string? al = Assembly.GetExecutingAssembly().GetName().Name;
            string? n = Path.GetFileNameWithoutExtension(al);
            return !string.IsNullOrEmpty(n) ? Path.GetFullPath(n + ".txt") : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<object> ReadValue(string msg, object value, Type type)
    {
        await Task.Run(() =>
        {
            if (!string.IsNullOrEmpty(msg)) Console.Out.WriteLine(msg);
            if (type == typeof(bool))
            {
                while (true)
                {
                    string? cmd = Console.ReadLine();
                    if (cmd == null) continue;
                    if (cmd == string.Empty) break;

                    bool isBool = bool.TryParse(cmd, out bool result);
                    if (isBool)
                    {
                        value = result;
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Out.WriteLine("Wrong Value, Use True or False. Try Again.");
                        Console.ResetColor();
                    }
                }
            }
            else if (type == typeof(float))
            {
                while (true)
                {
                    string? cmd = Console.ReadLine();
                    if (cmd == null) continue;
                    if (cmd == string.Empty) break;

                    bool isFloat = float.TryParse(cmd, out float f);
                    if (isFloat)
                    {
                        value = f;
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Out.WriteLine("Wrong Number. Try Again.");
                        Console.ResetColor();
                    }
                }
            }
            else if (type == typeof(int))
            {
                while (true)
                {
                    string? cmd = Console.ReadLine();
                    if (cmd == null) continue;
                    if (cmd == string.Empty) break;

                    bool isInt = int.TryParse(cmd, out int n);
                    if (isInt)
                    {
                        value = n;
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Out.WriteLine("Wrong Number. Try Again.");
                        Console.ResetColor();
                    }
                }
            }
            else if (type == typeof(string))
            {
                while (true)
                {
                    string? cmd = Console.ReadLine();
                    if (cmd == null) continue;

                    if (!string.IsNullOrEmpty(cmd) || !string.IsNullOrWhiteSpace(cmd))
                        value = cmd.Trim();
                    break;
                }
            }
        });
        
        return value;
    }

    public static bool GetValueByKey(string input, string keyName, bool isKeyMandatory, bool requireDoubleQuotes, out string value)
    {
        value = string.Empty;
        string origKeyName = keyName;
        string key = $"-{keyName}=";
        int startIndex = input.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
        if (startIndex == -1)
        {
            if (isKeyMandatory)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine($"\"{origKeyName}\" Key Is Missing.");
                Console.ResetColor();
                return false;
            }
            else return true;
        }

        int skip = startIndex + key.Length;
        input = input[skip..];

        if (requireDoubleQuotes)
        {
            if (input.StartsWith('"'))
                input = input[1..];
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine($"The Value Of \"{origKeyName}\" Require Double Quotes.");
                Console.ResetColor();
                return false;
            }
        }

        int endIndex;
        if (requireDoubleQuotes)
        {
            endIndex = input.IndexOf("\"", StringComparison.InvariantCultureIgnoreCase);
            if (endIndex == -1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine($"The Value Of \"{origKeyName}\" Require Double Quotes.");
                Console.ResetColor();
                return false;
            }
        }
        else
        {
            endIndex = input.IndexOf(" -", StringComparison.InvariantCultureIgnoreCase);
        }

        if (endIndex != -1)
        {
            input = input[..endIndex];
        }

        value = input.Trim();
        return true;
    }

    public static bool GetBool(string key, string value, bool isValueMandatory, out bool result)
    {
        result = false;
        value = value.Trim();
        if (!isValueMandatory && string.IsNullOrEmpty(value)) return true;

        bool isBool = bool.TryParse(value, out bool b);

        if (!isBool)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine($"Value Of \"{key}\" Is Wrong, Use True or False.");
            Console.ResetColor();
            return false;
        }

        result = b;
        return true;
    }

    public static bool GetFloat(string key, string value, bool isValueMandatory, int min, int max, out float result)
    {
        result = -1;
        value = value.Trim();
        if (!isValueMandatory && string.IsNullOrEmpty(value)) return true;

        bool isFloat = float.TryParse(value, out float number);

        if (!isFloat)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (string.IsNullOrEmpty(value))
                Console.Out.WriteLine($"The Value Of \"{key}\" Is Empty.");
            else
                Console.Out.WriteLine($"\"{value}\" It's Not A Number.");
            Console.ResetColor();
            return false;
        }

        if (number < min || number > max)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine($"The Value of \"{key}\" Must Be Between {min} and {max}.");
            Console.ResetColor();
            return false;
        }

        result = number;
        return true;
    }

    public static bool GetInt(string key, string value, bool isValueMandatory, int min, int max, out int result)
    {
        result = -1;
        value = value.Trim();
        if (!isValueMandatory && string.IsNullOrEmpty(value)) return true;

        bool isInt = int.TryParse(value, out int number);

        if (!isInt)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (string.IsNullOrEmpty(value))
                Console.Out.WriteLine($"The Value Of \"{key}\" Is Empty.");
            else
                Console.Out.WriteLine($"\"{value}\" It's Not A Number.");
            Console.ResetColor();
            return false;
        }

        if (number < min || number > max)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine($"The Value of \"{key}\" Must Be Between {min} and {max}.");
            Console.ResetColor();
            return false;
        }

        result = number;
        return true;
    }

    public static bool GetString(string key, string value, bool isValueMandatory, out string result)
    {
        result = string.Empty;
        value = value.Trim();
        if (!isValueMandatory && string.IsNullOrEmpty(value)) return true;

        if (string.IsNullOrEmpty(value))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine($"The Value of \"{key}\" Cannot Be Empty.");
            Console.ResetColor();
            return false;
        }
        else
        {
            result = value;
            return true;
        }
    }

    public static bool GetString(string key, string value, bool isValueMandatory, KeyValues keyValues, out string result)
    {
        result = string.Empty;
        value = value.Trim();
        if (!isValueMandatory && string.IsNullOrEmpty(value)) return true;

        List<KeyValue> list = keyValues.Get();

        for (int n = 0; n < list.Count; n++)
        {
            KeyValue kv = list[n];
            if (kv.Key.ToLower().Equals(value.ToLower()))
            {
                result = value;
                return true;
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.Out.WriteLine($"The Value of \"{key}\" Must Be One Of:\n");
        for (int n = 0; n < list.Count; n++)
        {
            KeyValue kv = list[n];
            Console.Out.WriteLine(kv.Key);
        }
        Console.ResetColor();
        return false;
    }

    //=============================================== Console Quick Editr
    // Save Original on Startup
    // GetConsoleMode(GetConsoleWindow(), ref saveConsoleMode);
    // Restore at Exit
    // SetConsoleMode(GetConsoleWindow(), saveConsoleMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, int ioMode);

    /// <summary>
    /// This flag enables the user to use the mouse to select and edit text. To enable
    /// this option, you must also set the ExtendedFlags flag.
    /// </summary>
    const int QuickEditMode = 64;

    // ExtendedFlags must be combined with
    // InsertMode and QuickEditMode when setting
    /// <summary>
    /// ExtendedFlags must be enabled in order to enable InsertMode or QuickEditMode.
    /// </summary>
    const int ExtendedFlags = 128;

    public static void DisableQuickEdit()
    {
        IntPtr conHandle = GetConsoleWindow();

        if (!GetConsoleMode(conHandle, out int mode))
        {
            // error getting the console mode. Exit.
            return;
        }

        mode &= ~(QuickEditMode | ExtendedFlags);

        if (!SetConsoleMode(conHandle, mode))
        {
            // error setting console mode.
        }
    }

    public static void EnableQuickEdit()
    {
        IntPtr conHandle = GetConsoleWindow();

        if (!GetConsoleMode(conHandle, out int mode))
        {
            // error getting the console mode. Exit.
            return;
        }

        mode |= (QuickEditMode | ExtendedFlags);

        if (!SetConsoleMode(conHandle, mode))
        {
            // error setting console mode.
        }
    }
}