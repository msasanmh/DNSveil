using System;
using System.Text.RegularExpressions;

namespace MsmhToolsClass;

public class Texts
{
    //-----------------------------------------------------------------------------------
    public static string? GetTextByLineNumber(string text, int lineNo)
    {
        string[] lines = text.Replace("\r", "").Split('\n');
        return lines.Length >= lineNo ? lines[lineNo - 1] : null;
    }
    //-----------------------------------------------------------------------------------
    public static bool IsValidRegex(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return false;

        try
        {
            Regex.Match("", pattern);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }
}