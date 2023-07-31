using System;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Data;
using System.Xml.Serialization;
using CustomControls;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Diagnostics;

namespace MsmhTools
{
    public static class Methods
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal extern static int SetWindowTheme(IntPtr controlHandle, string appName, string? idList);
    }
    public static class Extensions
    {
        //-----------------------------------------------------------------------------------
        public static TimeSpan Round(this TimeSpan timeSpan, int precision)
        {
            return TimeSpan.FromSeconds(Math.Round(timeSpan.TotalSeconds, precision));
        }
        //-----------------------------------------------------------------------------------
        public static void AppendText(this RichTextBox richTextBox, string text, Color color)
        {
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionColor = color;
            richTextBox.AppendText(text);
            richTextBox.SelectionColor = richTextBox.ForeColor;
        }
        //-----------------------------------------------------------------------------------
        public static List<List<T>> SplitToLists<T>(this List<T> list, int nSize)
        {
            var listOut = new List<List<T>>();

            for (int n = 0; n < list.Count; n += nSize)
            {
                listOut.Add(list.GetRange(n, Math.Min(nSize, list.Count - n)));
            }

            return listOut;
        }
        //-----------------------------------------------------------------------------------
        public static List<string> SplitToLines(this string s)
        {
            // Original non-optimized version: return source.Replace("\r\r\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');
            var lines = new List<string>();
            int start = 0;
            int max = s.Length;
            int i = 0;
            while (i < max)
            {
                var ch = s[i];
                if (ch == '\r')
                {
                    if (i < s.Length - 2 && s[i + 1] == '\r' && s[i + 2] == '\n') // \r\r\n
                    {
                        lines.Add(start < i ? s[start..i] : string.Empty); // s[start..i] = s.Substring(start, i - start)
                        i += 3;
                        start = i;
                        continue;
                    }

                    if (i < s.Length - 1 && s[i + 1] == '\n') // \r\n
                    {
                        lines.Add(start < i ? s[start..i] : string.Empty);
                        i += 2;
                        start = i;
                        continue;
                    }

                    lines.Add(start < i ? s[start..i] : string.Empty);
                    i++;
                    start = i;
                    continue;
                }

                if (ch == '\n' || ch == '\u2028')
                {
                    lines.Add(start < i ? s[start..i] : string.Empty);
                    i++;
                    start = i;
                    continue;
                }

                i++;
            }

            lines.Add(start < i ? s[start..i] : string.Empty);
            return lines;
        }

        public static List<string> SplitToLines(this string s, int maxCount)
        {
            var lines = new List<string>();
            int start = 0;
            int max = Math.Min(maxCount, s.Length);
            int i = 0;
            while (i < max)
            {
                var ch = s[i];
                if (ch == '\r')
                {
                    if (i < s.Length - 2 && s[i + 1] == '\r' && s[i + 2] == '\n') // \r\r\n
                    {
                        lines.Add(start < i ? s[start..i] : string.Empty);
                        i += 3;
                        start = i;
                        continue;
                    }

                    if (i < s.Length - 1 && s[i + 1] == '\n') // \r\n
                    {
                        lines.Add(start < i ? s[start..i] : string.Empty);
                        i += 2;
                        start = i;
                        continue;
                    }

                    lines.Add(start < i ? s[start..i] : string.Empty);
                    i++;
                    start = i;
                    continue;
                }

                if (ch == '\n' || ch == '\u2028')
                {
                    lines.Add(start < i ? s[start..i] : string.Empty);
                    i++;
                    start = i;
                    continue;
                }

                i++;
            }

            lines.Add(start < i ? s[start..i] : string.Empty);
            return lines;
        }
        //-----------------------------------------------------------------------------------
        public static string ToBase64String(this string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }
        //-----------------------------------------------------------------------------------
        public static string FromBase64String(this string base64String)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
        }
        //-----------------------------------------------------------------------------------
        public static string RemoveWhiteSpaces(this string text)
        {
            string findWhat = @"\s+";
            return Regex.Replace(text, findWhat, "");
        }
        //-----------------------------------------------------------------------------------
        public static void SetDarkTitleBar(this Control form, bool darkMode)
        {
            UseImmersiveDarkMode(form.Handle, darkMode);
        }
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return NativeMethods.DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }
            return false;
        }
        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }
        //-----------------------------------------------------------------------------------
        public static void SetDarkControl(this Control control)
        {
            _ = Methods.SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
            foreach (Control c in Controllers.GetAllControls(control))
            {
                _ = Methods.SetWindowTheme(c.Handle, "DarkMode_Explorer", null);
            }
        }
        //-----------------------------------------------------------------------------------
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using var xmlReader = xDocument.CreateReader();
            xmlDocument.Load(xmlReader);
            return xmlDocument;
        }
        //-----------------------------------------------------------------------------------
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using var nodeReader = new XmlNodeReader(xmlDocument);
            nodeReader.MoveToContent();
            return XDocument.Load(nodeReader);
        }
        //-----------------------------------------------------------------------------------
        public static string? AssemblyDescription(this Assembly assembly)
        {
            if (assembly != null && Attribute.IsDefined(assembly, typeof(AssemblyDescriptionAttribute)))
            {
                AssemblyDescriptionAttribute? descriptionAttribute = (AssemblyDescriptionAttribute?)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                if (descriptionAttribute != null)
                {
                    return descriptionAttribute.Description;
                }
            }
            return null;
        }
        //-----------------------------------------------------------------------------------
        public static T IsNotNull<T>([NotNull] this T? value, [CallerArgumentExpression(parameterName: "value")] string? paramName = null)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
            else
                return value;
        } // Usage: someVariable.IsNotNull();
        //-----------------------------------------------------------------------------------
        public static void EnableDoubleBuffer(this Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, control, new object[] { true });
            }
        }
        //-----------------------------------------------------------------------------------
        public static GraphicsPath Shrink(this GraphicsPath path, float width)
        {
            using GraphicsPath gp = new();
            gp.AddPath(path, false);
            gp.CloseAllFigures();
            gp.Widen(new Pen(Color.Black, width * 2));
            int position = 0;
            GraphicsPath result = new();
            while (position < gp.PointCount)
            {
                // skip outer edge
                position += CountNextFigure(gp.PathData, position);
                // count inner edge
                int figureCount = CountNextFigure(gp.PathData, position);
                var points = new PointF[figureCount];
                var types = new byte[figureCount];

                Array.Copy(gp.PathPoints, position, points, 0, figureCount);
                Array.Copy(gp.PathTypes, position, types, 0, figureCount);
                position += figureCount;
                result.AddPath(new GraphicsPath(points, types), false);
            }
            path.Reset();
            path.AddPath(result, false);
            return path;
        }

        private static int CountNextFigure(PathData data, int position)
        {
            int count = 0;
            for (int i = position; i < data?.Types?.Length; i++)
            {
                count++;
                if (0 != (data.Types[i] & (int)PathPointType.CloseSubpath))
                    return count;
            }
            return count;
        }
        //-----------------------------------------------------------------------------------
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
        {
            GraphicsPath path;
            path = Drawing.RoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.DrawPath(pen, path);
            graphics.SmoothingMode = SmoothingMode.Default;
        }
        //-----------------------------------------------------------------------------------
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
        {
            GraphicsPath path;
            path = Drawing.RoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillPath(brush, path);
            graphics.SmoothingMode = SmoothingMode.Default;
        }
        //-----------------------------------------------------------------------------------
        public static void DrawCircle(this Graphics g, Pen pen, float centerX, float centerY, float radius)
        {
            g.DrawEllipse(pen, centerX - radius, centerY - radius, radius + radius, radius + radius);
        }
        //-----------------------------------------------------------------------------------
        public static void FillCircle(this Graphics g, Brush brush, float centerX, float centerY, float radius)
        {
            g.FillEllipse(brush, centerX - radius, centerY - radius, radius + radius, radius + radius);
        }
        //-----------------------------------------------------------------------------------
        public static string ToXml(this DataSet ds)
        {
            using var memoryStream = new MemoryStream();
            using TextWriter streamWriter = new StreamWriter(memoryStream);
            var xmlSerializer = new XmlSerializer(typeof(DataSet));
            xmlSerializer.Serialize(streamWriter, ds);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
        //-----------------------------------------------------------------------------------
        public static string ToXmlWithWriteMode(this DataSet ds, XmlWriteMode xmlWriteMode)
        {
            using var ms = new MemoryStream();
            using TextWriter sw = new StreamWriter(ms);
            ds.WriteXml(sw, xmlWriteMode);
            return new UTF8Encoding(false).GetString(ms.ToArray());
        }
        //-----------------------------------------------------------------------------------
        public static DataSet ToDataSet(this DataSet ds, string xmlFile, XmlReadMode xmlReadMode)
        {
            ds.ReadXml(xmlFile, xmlReadMode);
            return ds;
        }
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------
        public static void AddVScrollBar(this DataGridView dataGridView, CustomVScrollBar customVScrollBar)
        {
            customVScrollBar.Dock = DockStyle.Right;
            customVScrollBar.Visible = true;
            customVScrollBar.BringToFront();
            dataGridView.Controls.Add(customVScrollBar);
            dataGridView.ScrollBars = ScrollBars.None;
            dataGridView.SelectionChanged += (object? sender, EventArgs e) =>
            {
                // To update ScrollBar position
                customVScrollBar.Value = dataGridView.FirstDisplayedScrollingRowIndex;
            };
            dataGridView.SizeChanged += (object? sender, EventArgs e) =>
            {
                // To update LargeChange on form resize
                customVScrollBar.LargeChange = dataGridView.DisplayedRowCount(false);
            };
            dataGridView.Invalidated += (object? sender, InvalidateEventArgs e) =>
            {
                // To update LargeChange on invalidation
                customVScrollBar.LargeChange = dataGridView.DisplayedRowCount(false);
            };
            dataGridView.RowsAdded += (object? sender, DataGridViewRowsAddedEventArgs e) =>
            {
                customVScrollBar.Maximum = dataGridView.RowCount;
                customVScrollBar.LargeChange = dataGridView.DisplayedRowCount(false);
                customVScrollBar.SmallChange = 1;
            };
            dataGridView.Scroll += (object? sender, ScrollEventArgs e) =>
            {
                if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
                {
                    if (dataGridView.Rows.Count > 0)
                    {
                        customVScrollBar.Value = e.NewValue;
                        // To update LargeChange on scroll
                        customVScrollBar.LargeChange = dataGridView.DisplayedRowCount(false);
                    }
                }
            };
            customVScrollBar.Scroll += (object? sender, EventArgs e) =>
            {
                if (dataGridView.Rows.Count > 0)
                    if (customVScrollBar.Value < dataGridView.Rows.Count)
                        dataGridView.FirstDisplayedScrollingRowIndex = customVScrollBar.Value;
            };
        }
        //-----------------------------------------------------------------------------------
        public static Icon? GetApplicationIcon(this Form _)
        {
            return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        //-----------------------------------------------------------------------------------
        public static Icon? GetDefaultIcon(this Form _)
        {
            return (Icon?)typeof(Form).GetProperty("DefaultIcon", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null, null);
        }
        //-----------------------------------------------------------------------------------
        public static void SetDefaultIcon(this Form _, Icon icon)
        {
            if (icon != null)
                typeof(Form).GetField("defaultIcon", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, icon);
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Invalidate Controls. Use on Form_SizeChanged event.
        /// </summary>
        public static void Invalidate(this Control.ControlCollection controls)
        {
            foreach (Control c in controls)
                c.Invalidate();
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static Color ChangeBrightness(this Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Check Color is Light or Dark.
        /// </summary>
        /// <returns>
        /// Returns "Dark" or "Light" as string.
        /// </returns>
        public static string DarkOrLight(this Color color)
        {
            if (color.R * 0.2126 + color.G * 0.7152 + color.B * 0.0722 < 255 / 2)
            {
                return "Dark";
            }
            else
            {
                return "Light";
            }
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Change Color Hue. (0f - 360f)
        /// </summary>
        /// <returns>
        /// Returns Modified Color.
        /// </returns>
        public static Color ChangeHue(this Color color, float hue)
        {
            //float hueO = color.GetHue();
            float saturationO = color.GetSaturation();
            float lightnessO = color.GetBrightness();
            return Colors.FromHsl(255, hue, saturationO, lightnessO);
        }
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// Change Color Saturation. (0f - 1f)
        /// </summary>
        /// <returns>
        /// Returns Modified Color.
        /// </returns>
        public static Color ChangeSaturation(this Color color, float saturation)
        {
            float hueO = color.GetHue();
            //float saturationO = color.GetSaturation();
            float lightnessO = color.GetBrightness();
            return Colors.FromHsl(255, hueO, saturation, lightnessO);
        }
        //-----------------------------------------------------------------------------------
        public static void AutoSizeLastColumn(this ListView listView)
        {
            if (listView.Columns.Count > 1)
            {
                //ListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                //ListView1.Columns[ListView1.Columns.Count - 1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                //ListView1.Columns[ListView1.Columns.Count - 1].Width = -2; // -2 = Fill remaining space
                int cs = 0;
                for (int n = 0; n < listView.Columns.Count - 1; n++)
                {
                    var column = listView.Columns[n];
                    cs += column.Width;
                }
                listView.BeginUpdate();
                listView.Columns[^1].Width = Math.Max(400, listView.ClientRectangle.Width - cs);
                listView.EndUpdate();
            }
        }
        //-----------------------------------------------------------------------------------
        public static void AutoSizeLastColumn(this DataGridView dataGridView)
        {
            if (dataGridView.Columns.Count > 0)
            {
                int cs = 0;
                for (int n = 0; n < dataGridView.Columns.Count - 1; n++)
                {
                    var columnWidth = dataGridView.Columns[n].Width;
                    var columnDivider = dataGridView.Columns[n].DividerWidth;
                    cs += columnWidth + columnDivider;
                }
                cs += (dataGridView.Margin.Left + dataGridView.Margin.Right) * 2;
                foreach (var scroll in dataGridView.Controls.OfType<VScrollBar>())
                {
                    if (scroll.Visible == true)
                        cs += SystemInformation.VerticalScrollBarWidth;
                }
                dataGridView.Columns[dataGridView.Columns.Count - 1].Width = Math.Max(400, dataGridView.ClientRectangle.Width - cs);
            }
        }
        //-----------------------------------------------------------------------------------
        public static void SaveToFile<T>(this List<T> list, string filePath)
        {
            try
            {
                FileStreamOptions streamOptions = new();
                streamOptions.Access = FileAccess.ReadWrite;
                streamOptions.Share = FileShare.ReadWrite;
                streamOptions.Mode = FileMode.Create;
                streamOptions.Options = FileOptions.RandomAccess;
                using StreamWriter file = new(filePath, streamOptions);
                for (int n = 0; n < list.Count; n++)
                    if (list[n] != null)
                    {
                        file.WriteLine(list[n]);
                    }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save List to File: {ex.Message}");
            }
        }
        //-----------------------------------------------------------------------------------
        public static void LoadFromFile(this List<string> list, string filePath, bool ignoreEmptyLines, bool trimLines)
        {
            if (!File.Exists(filePath)) return;
            string content = File.ReadAllText(filePath);
            List<string> lines = content.SplitToLines();
            for (int n = 0; n < lines.Count; n++)
            {
                string line = lines[n];
                if (ignoreEmptyLines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (trimLines)
                            list.Add(line.Trim());
                        else
                            list.Add(line);
                    }
                }
                else
                {
                    if (trimLines)
                        list.Add(line.Trim());
                    else
                        list.Add(line);
                }
            }
        }
        //-----------------------------------------------------------------------------------
        public static void LoadFromFile(this List<object> list, string filePath)
        {
            if (!File.Exists(filePath)) return;
            string content = File.ReadAllText(filePath);
            List<string> lines = content.SplitToLines();
            for (int n = 0; n < lines.Count; n++)
            {
                string line = lines[n];
                list.Add(line);
            }
        }
        //-----------------------------------------------------------------------------------
        public static int GetIndex<T>(this List<T> list, T value)
        {
            return list.FindIndex(a => a.Equals(value));
            // If the item is not found, it will return -1
        }
        //-----------------------------------------------------------------------------------
        public static void ChangeValue<T>(this List<T> list, T oldValue, T newValue)
        {
            list[list.GetIndex(oldValue)] = newValue;
        }
        //-----------------------------------------------------------------------------------
        public static void RemoveValue<T>(this List<T> list, T value)
        {
            list.RemoveAt(list.GetIndex(value));
        }
        //-----------------------------------------------------------------------------------
        public static List<T> RemoveDuplicates<T>(this List<T> list)
        {
            List<T> NoDuplicates = list.Distinct().ToList();
            return NoDuplicates;
        }
        //-----------------------------------------------------------------------------------
        public static void WriteToFile(this MemoryStream memoryStream, string dstPath)
        {
            using FileStream fs = new(dstPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.Position = 0;
            memoryStream.WriteTo(fs);
            fs.Flush();
        }
        //-----------------------------------------------------------------------------------
        public static void SetToolTip(this Control control, string titleMessage, string bodyMessage)
        {
            ToolTip tt = new();
            tt.ToolTipIcon = ToolTipIcon.Info;
            tt.IsBalloon = false;
            tt.ShowAlways = true;
            tt.UseAnimation = true;
            tt.UseFading = true;
            tt.InitialDelay = 1000;
            tt.AutoPopDelay = 6000;
            tt.AutomaticDelay = 300;
            tt.ToolTipTitle = titleMessage;
            tt.SetToolTip(control, bodyMessage);
        }
        //-----------------------------------------------------------------------------------
        public static void InvokeIt(this ISynchronizeInvoke sync, Action action)
        {
            // If the invoke is not required, then invoke here and get out.
            if (!sync.InvokeRequired)
            {
                action();
                return;
            }
            sync.Invoke(action, Array.Empty<object>());
            // Usage:
            // textBox1.InvokeIt(() => textBox1.Text = text);
        }
        //-----------------------------------------------------------------------------------
        public static bool Compare(this List<string> list1, List<string> list2)
        {
            return Enumerable.SequenceEqual(list1, list2);
        }

        public static bool Compare(this string string1, string string2)
        {
            return string1.Equals(string2, StringComparison.Ordinal);
        }
        //-----------------------------------------------------------------------------------
        public static bool IsInteger(this string s)
        {
            if (int.TryParse(s, out _))
                return true;
            return false;
        }
        //-----------------------------------------------------------------------------------
        public static bool IsBool(this string s)
        {
            if (bool.TryParse(s, out _))
                return true;
            return false;
        }
        //-----------------------------------------------------------------------------------
    }
}
