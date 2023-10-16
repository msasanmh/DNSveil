using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace MsmhToolsClass;

public class HtmlTool
{
    public static string RemoveHtmlFontTag(string s)
    {
        s = s.Replace("</font>", string.Empty);
        s = s.Replace("</FONT>", string.Empty);
        s = s.Replace("</Font>", string.Empty);
        s = s.Replace("<font>", string.Empty);
        s = s.Replace("<FONT>", string.Empty);
        s = s.Replace("<Font>", string.Empty);

        while (s.ToLower().Contains("<font"))
        {
            int startIndex = s.ToLower().IndexOf("<font");
            int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
            s = s.Remove(startIndex, (endIndex - startIndex) + 1);
        }
        return s;
    }
    //-----------------------------------------------------------------------------------
    public static string? RemoveHtmlTags(string s)
    {
        if (string.IsNullOrEmpty(s))
            return null;
        //if (s == null)
        //    return null;
        if (!s.Contains('<'))
            return s;
        s = s.Replace("<i>", string.Empty);
        s = s.Replace("</i>", string.Empty);
        s = s.Replace("<b>", string.Empty);
        s = s.Replace("</b>", string.Empty);
        s = s.Replace("<u>", string.Empty);
        s = s.Replace("</u>", string.Empty);
        s = s.Replace("<I>", string.Empty);
        s = s.Replace("</I>", string.Empty);
        s = s.Replace("<B>", string.Empty);
        s = s.Replace("</B>", string.Empty);
        s = s.Replace("<U>", string.Empty);
        s = s.Replace("</U>", string.Empty);
        s = RemoveParagraphTag(s);
        return RemoveHtmlFontTag(s).Trim();
    }
    //-----------------------------------------------------------------------------------
    internal static string GetHtmlColorCode(Color color)
    {
        return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
    }
    //-----------------------------------------------------------------------------------
    internal static string RemoveBrackets(string inputString)
    {
        string pattern = @"^[\[\{\(]|[\]\}\)]$";
        return Regex.Replace(inputString, pattern, string.Empty).Trim();
    }
    //-----------------------------------------------------------------------------------
    internal static string RemoveParagraphTag(string s)
    {
        s = s.Replace("</p>", string.Empty);
        s = s.Replace("</P>", string.Empty);
        s = s.Replace("<P>", string.Empty);
        s = s.Replace("<P>", string.Empty);

        while (s.ToLower().Contains("<p "))
        {
            int startIndex = s.ToLower().IndexOf("<p ");
            int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
            s = s.Remove(startIndex, endIndex - startIndex + 1);
        }
        return s;
    }
    //===================================================================
    public static string TagItalic => "i";
    public static string TagBold => "b";
    public static string TagUnderline => "u";
    public static string TagFont => "font";
    public static string TagCyrillicI => "\u0456"; // Cyrillic Small Letter Byelorussian-Ukrainian i (http://graphemica.com/%D1%96)

    private static readonly Regex TagOpenRegex = new(@"<\s*(?:/\s*)?(\w+)[^>]*>", RegexOptions.Compiled);

    /// <summary>
    /// Remove all of the specified opening and closing tags from the source HTML string.
    /// </summary>
    /// <param name="source">The source string to search for specified HTML tags.</param>
    /// <param name="tags">The HTML tags to remove.</param>
    /// <returns>A new string without the specified opening and closing tags.</returns>
    public static string RemoveOpenCloseTags(string source, params string[] tags)
    {
        if (string.IsNullOrEmpty(source) || source.IndexOf('<') < 0)
        {
            return source;
        }

        // This pattern matches these tag formats:
        // <tag*>
        // < tag*>
        // </tag*>
        // < /tag*>
        // </ tag*>
        // < / tag*>
        return TagOpenRegex.Replace(source, m => tags.Contains(m.Groups[1].Value, StringComparer.OrdinalIgnoreCase) ? string.Empty : m.Value);
    }

    /// <summary>
    /// Converts a string to an HTML-encoded string using named character references.
    /// </summary>
    /// <param name="source">The string to encode.</param>
    /// <returns>An encoded string.</returns>
    public static string EncodeNamed(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        var encoded = new StringBuilder(source.Length);
        foreach (var ch in source)
        {
            switch (ch)
            {
                case '<':
                    encoded.Append("&lt;");
                    break;
                case '>':
                    encoded.Append("&gt;");
                    break;
                case '"':
                    encoded.Append("&quot;");
                    break;
                case '&':
                    encoded.Append("&amp;");
                    break;
                case '\'':
                    encoded.Append("&apos;");
                    break;
                case ' ':
                    encoded.Append("&nbsp;");
                    break;
                case '–':
                    encoded.Append("&ndash;");
                    break;
                case '—':
                    encoded.Append("&mdash;");
                    break;
                case '¡':
                    encoded.Append("&iexcl;");
                    break;
                case '¿':
                    encoded.Append("&iquest;");
                    break;
                case '“':
                    encoded.Append("&ldquo;");
                    break;
                case '”':
                    encoded.Append("&rdquo;");
                    break;
                case '‘':
                    encoded.Append("&lsquo;");
                    break;
                case '’':
                    encoded.Append("&rsquo;");
                    break;
                case '«':
                    encoded.Append("&laquo;");
                    break;
                case '»':
                    encoded.Append("&raquo;");
                    break;
                case '¢':
                    encoded.Append("&cent;");
                    break;
                case '©':
                    encoded.Append("&copy;");
                    break;
                case '÷':
                    encoded.Append("&divide;");
                    break;
                case 'µ':
                    encoded.Append("&micro;");
                    break;
                case '·':
                    encoded.Append("&middot;");
                    break;
                case '¶':
                    encoded.Append("&para;");
                    break;
                case '±':
                    encoded.Append("&plusmn;");
                    break;
                case '€':
                    encoded.Append("&euro;");
                    break;
                case '£':
                    encoded.Append("&pound;");
                    break;
                case '®':
                    encoded.Append("&reg;");
                    break;
                case '§':
                    encoded.Append("&sect;");
                    break;
                case '™':
                    encoded.Append("&trade;");
                    break;
                case '¥':
                    encoded.Append("&yen;");
                    break;
                case 'á':
                    encoded.Append("&aacute;");
                    break;
                case 'Á':
                    encoded.Append("&Aacute;");
                    break;
                case 'à':
                    encoded.Append("&agrave;");
                    break;
                case 'À':
                    encoded.Append("&Agrave;");
                    break;
                case 'â':
                    encoded.Append("&acirc;");
                    break;
                case 'Â':
                    encoded.Append("&Acirc;");
                    break;
                case 'å':
                    encoded.Append("&aring;");
                    break;
                case 'Å':
                    encoded.Append("&Aring;");
                    break;
                case 'ã':
                    encoded.Append("&atilde;");
                    break;
                case 'Ã':
                    encoded.Append("&Atilde;");
                    break;
                case 'ä':
                    encoded.Append("&auml;");
                    break;
                case 'Ä':
                    encoded.Append("&Auml;");
                    break;
                case 'æ':
                    encoded.Append("&aelig;");
                    break;
                case 'Æ':
                    encoded.Append("&AElig;");
                    break;
                case 'ç':
                    encoded.Append("&ccedil;");
                    break;
                case 'Ç':
                    encoded.Append("&Ccedil;");
                    break;
                case 'é':
                    encoded.Append("&eacute;");
                    break;
                case 'É':
                    encoded.Append("&Eacute;");
                    break;
                case 'è':
                    encoded.Append("&egrave;");
                    break;
                case 'È':
                    encoded.Append("&Egrave;");
                    break;
                case 'ê':
                    encoded.Append("&ecirc;");
                    break;
                case 'Ê':
                    encoded.Append("&Ecirc;");
                    break;
                case 'ë':
                    encoded.Append("&euml;");
                    break;
                case 'Ë':
                    encoded.Append("&Euml;");
                    break;
                case 'í':
                    encoded.Append("&iacute;");
                    break;
                case 'Í':
                    encoded.Append("&Iacute;");
                    break;
                case 'ì':
                    encoded.Append("&igrave;");
                    break;
                case 'Ì':
                    encoded.Append("&Igrave;");
                    break;
                case 'î':
                    encoded.Append("&icirc;");
                    break;
                case 'Î':
                    encoded.Append("&Icirc;");
                    break;
                case 'ï':
                    encoded.Append("&iuml;");
                    break;
                case 'Ï':
                    encoded.Append("&Iuml;");
                    break;
                case 'ñ':
                    encoded.Append("&ntilde;");
                    break;
                case 'Ñ':
                    encoded.Append("&Ntilde;");
                    break;
                case 'ó':
                    encoded.Append("&oacute;");
                    break;
                case 'Ó':
                    encoded.Append("&Oacute;");
                    break;
                case 'ò':
                    encoded.Append("&ograve;");
                    break;
                case 'Ò':
                    encoded.Append("&Ograve;");
                    break;
                case 'ô':
                    encoded.Append("&ocirc;");
                    break;
                case 'Ô':
                    encoded.Append("&Ocirc;");
                    break;
                case 'ø':
                    encoded.Append("&oslash;");
                    break;
                case 'Ø':
                    encoded.Append("&Oslash;");
                    break;
                case 'õ':
                    encoded.Append("&otilde;");
                    break;
                case 'Õ':
                    encoded.Append("&Otilde;");
                    break;
                case 'ö':
                    encoded.Append("&ouml;");
                    break;
                case 'Ö':
                    encoded.Append("&Ouml;");
                    break;
                case 'ß':
                    encoded.Append("&szlig;");
                    break;
                case 'ú':
                    encoded.Append("&uacute;");
                    break;
                case 'Ú':
                    encoded.Append("&Uacute;");
                    break;
                case 'ù':
                    encoded.Append("&ugrave;");
                    break;
                case 'Ù':
                    encoded.Append("&Ugrave;");
                    break;
                case 'û':
                    encoded.Append("&ucirc;");
                    break;
                case 'Û':
                    encoded.Append("&Ucirc;");
                    break;
                case 'ü':
                    encoded.Append("&uuml;");
                    break;
                case 'Ü':
                    encoded.Append("&Uuml;");
                    break;
                case 'ÿ':
                    encoded.Append("&yuml;");
                    break;
                default:
                    if (ch > 127)
                    {
                        encoded.Append("&#").Append((int)ch).Append(';');
                    }
                    else
                    {
                        encoded.Append(ch);
                    }
                    break;
            }
        }
        return encoded.ToString();
    }

    /// <summary>
    /// Converts a string to an HTML-encoded string using numeric character references.
    /// </summary>
    /// <param name="source">The string to encode.</param>
    /// <returns>An encoded string.</returns>
    public static string EncodeNumeric(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        var encoded = new StringBuilder(source.Length);
        foreach (var ch in source)
        {
            if (ch == ' ')
            {
                encoded.Append("&#");
                encoded.Append(160); // &nbsp;
                encoded.Append(';');
            }
            else if (ch > 127 || ch == '<' || ch == '>' || ch == '"' || ch == '&' || ch == '\'')
            {
                encoded.Append("&#");
                encoded.Append((int)ch);
                encoded.Append(';');
            }
            else
            {
                encoded.Append(ch);
            }
        }
        return encoded.ToString();
    }

    /// <summary>
    /// Optimized method to remove common html tags, like <i>, <b>, <u>, and <font>
    /// </summary>
    /// <param name="s">Text to remove html tags from</param>
    /// <returns>Text stripped from common html tags</returns>
    private static string RemoveCommonHtmlTags(string s)
    {
        char[] array = new char[s.Length];
        int arrayIndex = 0;
        bool inside = false;

        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];
            if (ch == '<' && i < s.Length - 2)
            {
                var next = s[i + 1];
                var nextNext = s[i + 2];
                if (nextNext == '>' &&
                    (next == 'i' || // <i>
                     next == 'I' || // <I>
                     next == 'b' || // <b>
                     next == 'B' || // <B>
                     next == 'u' || // <u>
                     next == 'U'))  // <U>
                {
                    inside = true;
                    continue;
                }

                if (next == '/' && i < s.Length - 3)
                {
                    var nextNextNext = s[i + 3];
                    if (nextNextNext == '>' &&
                        (nextNext == 'i' || // </i>
                         nextNext == 'I' || // </I>
                         nextNext == 'b' || // </b>
                         nextNext == 'B' || // </B>
                         nextNext == 'u' || // </u>
                         nextNext == 'U'))  // </U>
                    {
                        inside = true;
                        continue;
                    }

                    if (nextNext == 'c' && nextNextNext == '.')
                    {
                        inside = true;
                        continue;
                    }
                }

                if (nextNext == '/' && i < s.Length - 3)
                { // some bad end tags sometimes seen
                    var nextNextNext = s[i + 3];
                    if (nextNextNext == '>' &&
                        (next == 'i' || // <i/>
                         next == 'I' || // <I/>
                         next == 'b' || // <b/>
                         next == 'B' || // <B/>
                         next == 'u' || // <u/>
                         next == 'U'))  // <U/>
                    {
                        inside = true;
                        continue;
                    }
                }

                if ((next == 'f' || next == 'F') && s[i..].StartsWith("<font", StringComparison.OrdinalIgnoreCase) || // <font
                    next == '/' && (nextNext == 'f' || nextNext == 'F') && s[i..].StartsWith("</font>", StringComparison.OrdinalIgnoreCase) ||  // </font>                        
                    next == ' ' && nextNext == '/' && s[i..].StartsWith("< /font>", StringComparison.OrdinalIgnoreCase) ||  // < /font>
                    next == '/' && nextNext == ' ' && s[i..].StartsWith("</ font>", StringComparison.OrdinalIgnoreCase))  // </ font>
                {
                    inside = true;
                    continue;
                }

                if (next == 'c' && nextNext == '.')
                {
                    inside = true;
                    continue;
                }
            }
            if (inside && ch == '>')
            {
                inside = false;
                continue;
            }
            if (!inside)
            {
                array[arrayIndex] = ch;
                arrayIndex++;
            }
        }
        return new string(array, 0, arrayIndex);
    }

    public static bool IsUrl(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 6 || text.IndexOf('.') < 0 || text.Contains(' '))
        {
            return false;
        }

        var allLower = text.ToLowerInvariant();
        if (allLower.StartsWith("http://", StringComparison.Ordinal) || allLower.StartsWith("https://", StringComparison.Ordinal) ||
            allLower.StartsWith("www.", StringComparison.Ordinal) || allLower.EndsWith(".org", StringComparison.Ordinal) ||
            allLower.EndsWith(".com", StringComparison.Ordinal) || allLower.EndsWith(".net", StringComparison.Ordinal))
        {
            return true;
        }

        if (allLower.Contains(".org/") || allLower.Contains(".com/") || allLower.Contains(".net/"))
        {
            return true;
        }

        return false;
    }

    public static bool StartsWithUrl(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var arr = text.Trim().TrimEnd('.').TrimEnd().Split();
        if (arr.Length == 0)
        {
            return false;
        }

        return IsUrl(arr[0]);
    }

    private static readonly string[] UppercaseTags = { "<I>", "<U>", "<B>", "<FONT", "</I>", "</U>", "</B>", "</FONT>" };

    public static string FixUpperTags(string input)
    {
        if (string.IsNullOrEmpty(input) || input.IndexOf('<') < 0)
        {
            return input;
        }

        var text = input;
        var idx = text.IndexOfAny(UppercaseTags, StringComparison.Ordinal);
        while (idx >= 0)
        {
            var endIdx = text.IndexOf('>', idx + 2);
            if (endIdx < idx)
            {
                break;
            }

            var tag = text[idx..endIdx].ToLowerInvariant();
            text = text.Remove(idx, endIdx - idx).Insert(idx, tag);
            idx = text.IndexOfAny(UppercaseTags, StringComparison.Ordinal);
        }
        return text;
    }

    public static string ToggleTag(string input, string tag, bool wholeLine, bool assa)
    {
        var text = input;

        if (assa)
        {
            var onOffTags = new List<string> { "i", "b", "u", "s", "be" };
            if (onOffTags.Contains(tag))
            {
                if (text.Contains($"\\{tag}1"))
                {
                    text = text.Replace($"{{\\{tag}1}}", string.Empty);
                    text = text.Replace($"{{\\{tag}0}}", string.Empty);
                    text = text.Replace($"\\{tag}1", string.Empty);
                    text = text.Replace($"\\{tag}0", string.Empty);
                }
                else
                {
                    text = wholeLine ? $"{{\\{tag}1}}{text}" : $"{{\\{tag}1}}{text}{{\\{tag}0}}";
                }
            }
            else
            {
                if (text.Contains($"\\{tag}"))
                {
                    text = text.Replace($"{{\\{tag}}}", string.Empty);
                    text = text.Replace($"\\{tag}", string.Empty);
                }
                else
                {
                    text = $"{{\\{tag}}}{text}";
                }
            }

            return text;
        }

        if (text.Contains("<" + tag + ">", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("</" + tag + ">", StringComparison.OrdinalIgnoreCase))
        {
            text = text.Replace("<" + tag + ">", string.Empty);
            text = text.Replace("</" + tag + ">", string.Empty);
            text = text.Replace("<" + tag.ToUpperInvariant() + ">", string.Empty);
            text = text.Replace("</" + tag.ToUpperInvariant() + ">", string.Empty);
        }
        else
        {
            int indexOfEndBracket = text.IndexOf('}');
            if (text.StartsWith("{\\", StringComparison.Ordinal) && indexOfEndBracket > 1 && indexOfEndBracket < 6)
            {
                text = $"{text[..(indexOfEndBracket + 1)]}<{tag}>{text.Remove(0, indexOfEndBracket + 1)}</{tag}>";
            }
            else
            {
                text = $"<{tag}>{text}</{tag}>";
            }
        }
        return text;
    }

    public static Color GetColorFromString(string color)
    {
        Color c = Color.White;
        try
        {
            if (color.StartsWith("rgb(", StringComparison.Ordinal))
            {
                string[] arr = color.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                c = Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
            }
            else
            {
                c = ColorTranslator.FromHtml(color);
            }
        }
        catch
        {
            // ignored
        }

        return c;
    }
}