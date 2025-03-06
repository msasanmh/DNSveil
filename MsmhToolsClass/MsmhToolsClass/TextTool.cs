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
            async static Task<List<string>> getLinksInternalAsync(string interLine)
            {
                List<string> interLinks = new();

                await Task.Run(async () =>
                {
                    try
                    {
                        string find = "://";
                        if (interLine.Contains(find))
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
                                    interLinks.Add(interLink);
                                    interLine = interLine.Replace(interLink, string.Empty);
                                    interLinks.AddRange(await GetLinksAsync(interLine));
                                }
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
                links.AddRange(await getLinksInternalAsync(subLine));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("TextTool GetLinksAsync: " + ex.Message);
        }

        return links;
    }

    public static async Task<string> RemoveTextAsync(string text, char startChar, char endChar, bool replaceWithSpace = false)
    {
        await Task.Run(() =>
        {
            if (text.Contains(startChar) && text.Contains(endChar))
            {
                try
                {
                    text = Regex.Replace(text, $"\\{startChar}.*?\\{endChar}", " ");

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
                }
                catch (Exception) { }
            }
        });
        
        return text;
    }

    public static async Task<string> RemoveHtmlTagsAsync(string html, bool replaceTagsWithSpace)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            html = await RemoveTextAsync(html, '<', '>', replaceTagsWithSpace);
            html = await RemoveTextAsync(html, '{', '}', replaceTagsWithSpace);
            html = await RemoveTextAsync(html, '(', ')', replaceTagsWithSpace); // For MarkDown
            html = await RemoveTextAsync(html, '[', ']', replaceTagsWithSpace); // For MarkDown

            string[] lines = html.ReplaceLineEndings().Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            html = WebUtility.HtmlDecode(lines.ToList().ToString(Environment.NewLine));

            // For MarkDown
            html = html.Replace('|', ' ');
            html = html.Replace(":heavy_check_mark:", " ", StringComparison.OrdinalIgnoreCase);
            html = html.ReplaceLineEndings().Split(Environment.NewLine, StringSplitOptions.TrimEntries).ToList().ToString(Environment.NewLine);
        }
        catch (Exception) { }
        return html;
    }

    public static bool IsValidRegex(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return false;

        try
        {
            Regex.Match("", pattern);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}