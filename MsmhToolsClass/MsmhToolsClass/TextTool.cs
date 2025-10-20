using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace MsmhToolsClass;

public class TextTool
{
    public async static Task<List<string>> GetLinksAsync(string line)
    {
        line = line.Trim();
        List<string> links = new();
        
        try
        {
            string find = "://";

            async Task<List<string>> getLinksInternalAsync(string interLine)
            {
                List<string> interLinks = new();
                if (!interLine.Contains(find)) return interLinks;
                
                await Task.Run(async () =>
                {
                    try
                    {
                        int start = interLine.IndexOf(find);
                        int end = start;
                        if (start != -1)
                        {
                            while (true)
                            {
                                start--;
                                if (start != -1)
                                {
                                    char startChar = interLine[start];
                                    if (startChar.Equals(' '))
                                    {
                                        start++;
                                        break;
                                    }
                                }
                                else
                                {
                                    start++;
                                    break;
                                }
                            }
                            
                            while (true)
                            {
                                end++;
                                if (end < interLine.Length)
                                {
                                    char endChar = interLine[end];
                                    if (endChar.Equals(' ')) break;
                                }
                                else break;
                            }

                            if (end > start)
                            {
                                string interLink = interLine[start..end];
                                if (interLink.EndsWith('\\')) interLink = interLink.TrimEnd('\\');
                                interLinks.Add(interLink);
                                interLine = interLine.Replace(interLink, string.Empty);
                                interLinks.AddRange(await GetLinksAsync(interLine));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("TextTool GetLinksAsync getLinksInternalAsync: " + ex.Message);
                    }
                });
                
                return interLinks;
            }

            string[] lines = line.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Split Line By Space Saves Memory Usage
            for (int n = 0; n < lines.Length; n++)
            {
                string subLine = lines[n].Trim();
                if (subLine.Contains(find))
                    links.AddRange(await getLinksInternalAsync(subLine));
            }
            
            links = links.Distinct().ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TextTool GetLinksAsync: " + ex.Message);
        }

        return links;
    }

    /// <summary>
    /// Scrap IPv4, IPv4:Port, [IPv6], [IPv6]:Port
    /// </summary>
    /// <param name="text">Text Without HTML Or MD Tags</param>
    /// <returns>A List Of IPs/End Points</returns>
    public static List<string> GetEndPoints(string text)
    {
        List<string> endPoints = new();

        try
        {
            text = text.Replace('/', ' ');
            text = text.ReplaceLineEndings();
            text = text.Replace(Environment.NewLine, ' '.ToString());

            List<string> split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int n = 0; n < split.Count; n++)
            {
                string str = split[n].Trim();
                if (NetworkTool.IsEndPoint(str, out IPEndPoint? ep1) && ep1 != null)
                {
                    endPoints.Add(ep1.ToString(true));
                }
            }
        }
        catch (Exception) { }

        return endPoints;
    }

    public static async Task<string> RemoveTextAsync(string text, char startChar, char endChar, bool replaceWithSpace)
    {
        await Task.Run(() =>
        {
            if (text.Contains(startChar) && text.Contains(endChar))
            {
                try
                {
                    string escapedStart = Regex.Escape(startChar.ToString());
                    string escapedEnd = Regex.Escape(endChar.ToString());
                    string pattern = $"{escapedStart}.*?{escapedEnd}";
                    text = Regex.Replace(text, pattern, " ", RegexOptions.Singleline);

                    while (true)
                    {
                        int start = text.IndexOf(startChar);
                        int end = text.IndexOf(endChar);

                        if (start != -1 && end != -1)
                        {
                            if (end > start)
                            {
                                text = text.Remove(start, end - start + 1);
                                if (replaceWithSpace) text = text.Insert(start, " ");
                            }
                            else text = text.Remove(end, 1);
                        }
                        else break;
                    }

                    text = text.Replace(startChar.ToString(), " ").Replace(endChar.ToString(), " ");
                }
                catch (Exception) { }
            }
        });
        
        return text;
    }

    public static async Task<List<string>> RemoveHtmlAndMarkDownTagsAsync(string html, bool replaceTagsWithSpace)
    {
        List<string> extractedStrings = new();

        try
        {
            html = html.Trim();
            if (string.IsNullOrEmpty(html)) return extractedStrings;

            html = html.ReplaceLineEndings(" "); // One Liner
            html = WebUtility.HtmlDecode(html); // Decode HTML
            html = WebUtility.UrlDecode(html); // Decode URL

            bool isHTML = html.Contains("<!DOCTYPE html>", StringComparison.OrdinalIgnoreCase) || html.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
            
            html = await RemoveTextAsync(html, '<', '>', replaceTagsWithSpace); // Global
            
            // For Embeded Scripts In HTML
            html = Regex.Replace(html, @"\\n", " "); // Replace NewLine With Space
            html = Regex.Replace(html, @"\\u[a-fA-F0-9]{4}", " "); // Replace Unicode Chars With Space

            html = html.Replace("[", " ").Replace("]", " "); // Can Be IPv6
            html = html.Replace("{", " ").Replace("}", " ");
            html = html.Replace("\'", " ").Replace("\"", " ");
            html = html.Replace('|', ' ');
            html = html.Replace(":heavy_check_mark:", " ", StringComparison.OrdinalIgnoreCase);

            if (isHTML)
            {
                // For HTML
                html = html.Replace("(", " ").Replace(")", " ");
            }
            else
            {
                // For MarkDown
                html = await RemoveTextAsync(html, '(', ')', replaceTagsWithSpace);
                html = await RemoveTextAsync(html, '\u0060', '\u0060', replaceTagsWithSpace); // `
            }
            
            // Split To Lines By Space
            extractedStrings = html.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

            // DeDup Lines
            extractedStrings = extractedStrings.Distinct().ToList();
        }
        catch (Exception) { }

        return extractedStrings;
    }

    public static bool IsValidRegex(string pattern)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;
            Regex.Match("", pattern);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}