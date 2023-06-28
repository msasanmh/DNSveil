using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MsmhTools
{
    public static class StringTool
    {

        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool StartsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[0] == c;
        }

        public static bool EndsWith(this string s, char c)
        {
            return s.Length > 0 && s[^1] == c;
        }

        public static bool EndsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[^1] == c;
        }

        public static bool Contains(this string source, char value)
        {
            return source.Contains(value);
        }

        public static bool Contains(this string source, char[] value)
        {
            return source.IndexOfAny(value) >= 0;
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.Contains(value, comparisonType);
        }

        //============================================================================================
        private static char[] UnicodeControlChars { get; } = { '\u200E', '\u200F', '\u202A', '\u202B', '\u202C', '\u202D', '\u202E' };

        private static char[] PersianChars { get; } = { '\u06CC' };
        //public static char[] PersianChars { get; } = { 'ی' };

        public static bool ContainsUnicodeControlChars(this string s)
        {
            return s.Contains(UnicodeControlChars);
        }

        public static bool ContainsPersianChars(this string s)
        {
            return s.IndexOfAny(PersianChars) >= 0;
        }
        //============================================================================================
        public static bool IsEmpty(this string text)
        {
            text = text.RemoveHtmlTags();
            if (string.IsNullOrWhiteSpace(text) || text == string.Empty)
                return true;

            int countControlChar = 0;
            for (int a = 0; a < text.ToCharArray().Length; a++)
            {
                char ch = text.ToCharArray()[a];
                if (char.IsControl(ch))
                    countControlChar++;
            }

            if (countControlChar == text.ToCharArray().Length)
                return true;

            bool isMatch = Regex.IsMatch(text, @"^[\r\n\s\t\v]+$");
            if (isMatch) return true;

            bool ucc = Regex.IsMatch(text, @"^[\u200E\u200F\u202A\u202B\u202C\u202D\u202E']+$");
            if (ucc) return true;

            return false;
        }
        //============================================================================================
        public static string RemoveUnicodeControlChars(this string s)
        {
            int max = s.Length;
            var newStr = new char[max];
            int newIdx = 0;
            for (int index = 0; index < max; index++)
            {
                char ch = s[index];
                if (!UnicodeControlChars.Contains(ch))
                {
                    newStr[newIdx++] = ch;
                }
            }

            return new string(newStr, 0, newIdx);
        }
        //============================================================================================
        public static string RemoveControlChars(this string s)
        {
            int max = s.Length;
            var newStr = new char[max];
            int newIdx = 0;
            for (int index = 0; index < max; index++)
            {
                char ch = s[index];
                if (!char.IsControl(ch))
                {
                    newStr[newIdx++] = ch;
                }
            }

            return new string(newStr, 0, newIdx);
        }
        //============================================================================================
        public static string RemoveHtmlTags(this string text)
        {
            string? output = HTML.RemoveHtmlTags(text);
            if (output != null)
                return output;
            else
                return string.Empty;
        }
        //============================================================================================
        /// <summary>
        /// Truncates the TextBox.Text property so it will fit in the TextBox. 
        /// </summary>
        public static void Truncate(this TextBox textBox)
        {
            //Determine direction of truncation
            bool direction = false;
            if (textBox.TextAlign == HorizontalAlignment.Right) direction = true;

            //Get text
            string truncatedText = textBox.Text;

            //Truncate text
            truncatedText = truncatedText.Truncate(textBox.Font, textBox.Width, direction);

            //If text truncated
            if (truncatedText != textBox.Text)
            {
                //Set textBox text
                textBox.Text = truncatedText;

                //After setting the text, the cursor position changes. Here we set the location of the cursor manually.
                //First we determine the position, the default value applies to direction = left.

                //This position is when the cursor needs to be behind the last char. (Example:"…My Text|");
                int position = 0;

                //If the truncation direction is to the right the position should be before the ellipsis
                if (!direction)
                {
                    //This position is when the cursor needs to be before the last char (which would be the ellipsis). (Example:"My Text|…");
                    position = 1;
                }

                //Set the cursor position
                textBox.Select(textBox.Text.Length - position, 0);
            }
        }

        /// <summary>
        /// Truncates the string to be smaller than the desired width.
        /// </summary>
        /// <param name="font">The font used to determine the size of the string.</param>
        /// <param name="width">The maximum size the string should be after truncating.</param>
        /// <param name="direction">The direction of the truncation. True for left (…ext), False for right(Tex…).</param>
        static public string Truncate(this string text, Font font, int width, bool direction)
        {
            string truncatedText, returnText;
            int charIndex = 0;
            bool truncated = false;
            //When the user is typing and the truncation happens in a TextChanged event, already typed text could get lost.
            //Example: Imagine that the string "Hello Worl" would truncate if we add 'd'. Depending on the font the output 
            //could be: "Hello Wor…" (notice the 'l' is missing). This is an undesired effect.
            //To prevent this from happening the ellipsis is included in the initial sizecheck.
            //At this point, the direction is not important so we place ellipsis behind the text.
            truncatedText = text + "…";

            //Get the size of the string in pixels.
            SizeF size = MeasureString(truncatedText, font);

            //Do while the string is bigger than the desired width.
            while (size.Width > width)
            {
                //Go to next char
                charIndex++;
                //If the character index is larger than or equal to the length of the text, the truncation is unachievable.
                if (charIndex >= text.Length)
                {
                    //Truncation is unachievable!
                    truncated = true;
                    truncatedText = string.Empty;
                    //Throw exception so the user knows what's going on.
                    string msg = "The desired width of the string is too small to truncate to.";
                    Console.WriteLine(msg);
                    break;
                    //throw new IndexOutOfRangeException(msg);
                }
                else
                {
                    //Truncation is still applicable!
                    //Raise the flag, indicating that text is truncated.
                    truncated = true;
                    //Check which way to text should be truncated to, then remove one char and add an ellipsis.
                    if (direction)
                    {
                        //Truncate to the left. Add ellipsis and remove from the left.
                        truncatedText = string.Concat("…", text.AsSpan(charIndex));
                    }
                    else
                    {
                        //Truncate to the right. Remove from the right and add the ellipsis.
                        truncatedText = string.Concat(text.AsSpan(0, text.Length - charIndex), "…");
                    }

                    //Measure the string again.
                    size = MeasureString(truncatedText, font);
                }
            }
            //If the text got truncated, change the return value to the truncated text.
            if (truncated) returnText = truncatedText;
            else returnText = text;
            //Return the desired text.
            return returnText;
        }

        /// <summary>
        /// Measures the size of this string object.
        /// </summary>
        /// <param name="text">The string that will be measured.</param>
        /// <param name="font">The font that will be used to measure to size of the string.</param>
        /// <returns>A SizeF object containing the height and size of the string.</returns>
        private static SizeF MeasureString(string text, Font font)
        {
            //To measure the string we use the Graphics.MeasureString function, which is a method that can be called from a PaintEventArgs instance.
            //To call the constructor of the PaintEventArgs class, we must pass a Graphics object. We'll use a PictureBox object to achieve this. 
            PictureBox pb = new();

            //Create the PaintEventArgs with the correct parameters.
            PaintEventArgs pea = new(pb.CreateGraphics(), new Rectangle());
            pea.Graphics.PageUnit = GraphicsUnit.Pixel;
            pea.Graphics.PageScale = 1;

            //Call the MeasureString method. This methods calculates what the height and width of a string would be, given the specified font.
            SizeF size = pea.Graphics.MeasureString(text, font);

            //Return the SizeF object.
            return size;
        }
        //============================================================================================

        public static bool LineStartsWithHtmlTag(this string text, bool threeLengthTag, bool includeFont = false)
        {
            if (text == null || !threeLengthTag && !includeFont)
            {
                return false;
            }

            return StartsWithHtmlTag(text, threeLengthTag, includeFont);
        }

        public static bool LineEndsWithHtmlTag(this string text, bool threeLengthTag, bool includeFont = false)
        {
            if (text == null)
            {
                return false;
            }

            var len = text.Length;
            if (len < 6 || text[len - 1] != '>')
            {
                return false;
            }

            // </font> </i>
            if (threeLengthTag && len > 3 && text[len - 4] == '<' && text[len - 3] == '/')
            {
                return true;
            }

            if (includeFont && len > 8 && text[len - 7] == '<' && text[len - 6] == '/')
            {
                return true;
            }

            return false;
        }

        public static bool LineBreakStartsWithHtmlTag(this string text, bool threeLengthTag, bool includeFont = false)
        {
            if (text == null || (!threeLengthTag && !includeFont))
            {
                return false;
            }

            var newLineIdx = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (newLineIdx < 0 || text.Length < newLineIdx + 5)
            {
                return false;
            }

            text = text[(newLineIdx + 2)..];
            return StartsWithHtmlTag(text, threeLengthTag, includeFont);
        }

        private static bool StartsWithHtmlTag(string text, bool threeLengthTag, bool includeFont)
        {
            if (threeLengthTag && text.Length >= 3 && text[0] == '<' && text[2] == '>' && (text[1] == 'i' || text[1] == 'I' || text[1] == 'u' || text[1] == 'U' || text[1] == 'b' || text[1] == 'B'))
            {
                return true;
            }

            if (includeFont && text.Length > 5 && text.StartsWith("<font", StringComparison.OrdinalIgnoreCase))
            {
                return text.IndexOf('>', 5) >= 5; // <font> or <font color="#000000">
            }

            return false;
        }

        public static int CountWords(this string source)
        {
            return HTML.RemoveHtmlTags(source).Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        // http://www.codeproject.com/Articles/43726/Optimizing-string-operations-in-C
        public static int FastIndexOf(this string source, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return -1;
            }

            char c0 = pattern[0];
            if (pattern.Length == 1)
            {
                return source.IndexOf(c0);
            }

            int limit = source.Length - pattern.Length + 1;
            if (limit < 1)
            {
                return -1;
            }

            char c1 = pattern[1];

            // Find the first occurrence of the first character
            int first = source.IndexOf(c0, 0, limit);
            while (first != -1)
            {
                // Check if the following character is the same like
                // the 2nd character of "pattern"
                if (source[first + 1] != c1)
                {
                    first = source.IndexOf(c0, ++first, limit - first);
                    continue;
                }

                // Check the rest of "pattern" (starting with the 3rd character)
                var found = true;
                for (int j = 2; j < pattern.Length; j++)
                {
                    if (source[first + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }

                // If the whole word was found, return its index, otherwise try again
                if (found)
                {
                    return first;
                }

                first = source.IndexOf(c0, ++first, limit - first);
            }

            return -1;
        }

        public static int IndexOfAny(this string s, string[] words, StringComparison comparisonType)
        {
            if (words == null || string.IsNullOrEmpty(s))
            {
                return -1;
            }

            for (int i = 0; i < words.Length; i++)
            {
                var idx = s.IndexOf(words[i], comparisonType);
                if (idx >= 0)
                {
                    return idx;
                }
            }

            return -1;
        }

        public static string FixExtraSpaces(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            const char whiteSpace = ' ';
            int k = -1;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                char ch = s[i];
                if (k < 2)
                {
                    if (ch == whiteSpace)
                    {
                        k = i + 1;
                    }
                }
                else if (ch != whiteSpace)
                {
                    // only keep white space if it doesn't succeed/precede CRLF
                    int skipCount = (ch == '\n' || ch == '\r') || (k < s.Length && (s[k] == '\n' || s[k] == '\r')) ? 1 : 2;

                    // extra space found
                    if (k - (i + skipCount) >= 1)
                    {
                        s = s.Remove(i + 1, k - (i + skipCount));
                    }

                    // Reset remove length.
                    k = -1;
                }
            }

            return s;
        }

        public static bool ContainsLetter(this string s)
        {
            if (s != null)
            {
                foreach (var index in StringInfo.ParseCombiningCharacters(s))
                {
                    var uc = CharUnicodeInfo.GetUnicodeCategory(s, index);
                    if (uc == UnicodeCategory.LowercaseLetter || uc == UnicodeCategory.UppercaseLetter || uc == UnicodeCategory.TitlecaseLetter || uc == UnicodeCategory.ModifierLetter || uc == UnicodeCategory.OtherLetter)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool ContainsNumber(this string s)
        {
            if (s == null)
            {
                return false;
            }

            int max = s.Length;
            for (int index = 0; index < max; index++)
            {
                var ch = s[index];
                if (char.IsNumber(ch))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsOnlyControlCharsOrWhiteSpace(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return true;
            }

            int max = s.Length;
            for (int index = 0; index < max; index++)
            {
                char ch = s[index];
                if (!char.IsControl(ch) && !char.IsWhiteSpace(ch) && !UnicodeControlChars.Contains(ch))
                {
                    return false;
                }
            }

            return true;
        }


        public static string RemoveControlCharsButWhiteSpace(this string s)
        {
            int max = s.Length;
            var newStr = new char[max];
            int newIdx = 0;
            for (int index = 0; index < max; index++)
            {
                var ch = s[index];
                if (!char.IsControl(ch) || ch == '\u000d' || ch == '\u000a' || ch == '\u0009')
                {
                    newStr[newIdx++] = ch;
                }
            }

            return new string(newStr, 0, newIdx);
        }

        public static string CapitalizeFirstLetter(this string s, CultureInfo? ci = null)
        {
            var si = new StringInfo(s);
            if (ci == null)
            {
                ci = CultureInfo.CurrentCulture;
            }

            if (si.LengthInTextElements > 0)
            {
                s = si.SubstringByTextElements(0, 1).ToUpper(ci);
            }

            if (si.LengthInTextElements > 1)
            {
                s += si.SubstringByTextElements(1);
            }

            return s;
        }

        public static string RemoveChar(this string value, char charToRemove)
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

        public static string RemoveChar(this string value, char charToRemove, char charToRemove2)
        {
            char[] array = new char[value.Length];
            int arrayIndex = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                if (ch != charToRemove && ch != charToRemove2)
                {
                    array[arrayIndex++] = ch;
                }
            }

            return new string(array, 0, arrayIndex);
        }

        public static string RemoveChar(this string value, params char[] charsToRemove)
        {
            var h = new HashSet<char>(charsToRemove);
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

        /// <summary>
        /// Count characters excl. white spaces, ssa-tags, html-tags, control-characters, normal spaces and
        /// Arabic diacritics depending on parameter.
        /// </summary>
        public static int CountCharacters(this string value, bool removeNormalSpace, bool ignoreArabicDiacritics)
        {
            int length = 0;
            const char zeroWidthSpace = '\u200B';
            const char zeroWidthNoBreakSpace = '\uFEFF';
            char normalSpace = removeNormalSpace ? ' ' : zeroWidthSpace;
            bool ssaTagOn = false;
            bool htmlTagOn = false;
            var max = value.Length;
            for (int i = 0; i < max; i++)
            {
                char ch = value[i];
                if (ssaTagOn)
                {
                    if (ch == '}')
                    {
                        ssaTagOn = false;
                    }
                }
                else if (htmlTagOn)
                {
                    if (ch == '>')
                    {
                        htmlTagOn = false;
                    }
                }
                else if (ch == '{' && i < value.Length - 1 && value[i + 1] == '\\')
                {
                    ssaTagOn = true;
                }
                else if (ch == '<' && i < value.Length - 1 && (value[i + 1] == '/' || char.IsLetter(value[i + 1])) &&
                         value.IndexOf('>', i) > 0 && IsKnownHtmlTag(value, i))
                {
                    htmlTagOn = true;
                }
                else if (!char.IsControl(ch) &&
                         ch != zeroWidthSpace &&
                         ch != zeroWidthNoBreakSpace &&
                         ch != normalSpace &&
                         ch != '\u200E' &&
                         ch != '\u200F' &&
                         ch != '\u202A' &&
                         ch != '\u202B' &&
                         ch != '\u202C' &&
                         ch != '\u202D' &&
                         ch != '\u202E' &&
                         !(ignoreArabicDiacritics && ch >= '\u064B' && ch <= '\u0653'))
                {
                    length++;
                }
            }

            return length;
        }

        private static bool IsKnownHtmlTag(string input, int idx)
        {
            var s = input.Remove(0, idx + 1).ToLowerInvariant();
            return s.StartsWith('/') ||
                   s.StartsWith("i>", StringComparison.Ordinal) ||
                   s.StartsWith("b>", StringComparison.Ordinal) ||
                   s.StartsWith("u>", StringComparison.Ordinal) ||
                   s.StartsWith("font ", StringComparison.Ordinal) ||
                   s.StartsWith("ruby", StringComparison.Ordinal) ||
                   s.StartsWith("span>", StringComparison.Ordinal) ||
                   s.StartsWith("p>", StringComparison.Ordinal) ||
                   s.StartsWith("br>", StringComparison.Ordinal) ||
                   s.StartsWith("div>", StringComparison.Ordinal) ||
                   s.StartsWith("div ", StringComparison.Ordinal);
        }

        public static bool HasSentenceEnding(this string value)
        {
            return value.HasSentenceEnding(string.Empty);
        }

        public static bool HasSentenceEnding(this string value, string twoLetterLanguageCode)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            string s = HTML.RemoveHtmlTags(value).TrimEnd('"').TrimEnd('”');
            if (s == string.Empty)
            {
                return false;
            }

            var last = s[^1];
            return last == '.' || last == '!' || last == '?' || last == ']' || last == ')' || last == '…' || last == '♪' || last == '؟' ||
                   twoLetterLanguageCode == "el" && last == ';' || twoLetterLanguageCode == "el" && last == '\u037E' ||
                   last == '-' && s.Length > 3 && s.EndsWith("--", StringComparison.Ordinal) && char.IsLetter(s[^3]) ||
                   last == '—' && s.Length > 2 && char.IsLetter(s[^2]);
        }

    }
}
