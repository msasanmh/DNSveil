using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using CustomControls;
using MsmhToolsClass;

namespace MsmhToolsWinFormsClass;

public static class Methods
{
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    internal extern static int SetWindowTheme(IntPtr controlHandle, string appName, string? idList);
}

public static class ExtensionsMethods
{
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
            int attribute = IsWindows10OrGreater(18985) ? DWMWA_USE_IMMERSIVE_DARK_MODE : DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
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
        try
        {
            if (!control.IsHandleCreated) return;
            _ = Methods.SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
            foreach (Control c in Controllers.GetAllControls(control))
            {
                if (!c.IsHandleCreated) continue;
                _ = Methods.SetWindowTheme(c.Handle, "DarkMode_Explorer", null);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SetDarkControl: " + ex.Message);
        }
    }
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
    public static void SetToolTip(this Control control, ToolTip toolTip, string titleMessage, string bodyMessage)
    {
        toolTip.ToolTipIcon = ToolTipIcon.Info;
        toolTip.IsBalloon = false;
        toolTip.ShowAlways = true;
        toolTip.UseAnimation = true;
        toolTip.UseFading = true;
        toolTip.InitialDelay = 1000;
        toolTip.AutoPopDelay = 6000;
        toolTip.AutomaticDelay = 300;
        toolTip.ToolTipTitle = titleMessage;
        toolTip.SetToolTip(control, bodyMessage);
    }
    //-----------------------------------------------------------------------------------
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
        // When the user is typing and the truncation happens in a TextChanged event, already typed text could get lost.
        // Example: Imagine that the string "Hello Worl" would truncate if we add 'd'. Depending on the font the output 
        // could be: "Hello Wor…" (notice the 'l' is missing). This is an undesired effect.
        // To prevent this from happening the ellipsis is included in the initial sizecheck.
        // At this point, the direction is not important so we place ellipsis behind the text.
        truncatedText = text + "…";

        // Get the size of the string in pixels.
        SizeF size = MeasureString(truncatedText, font);

        // Do while the string is bigger than the desired width.
        while (size.Width > width)
        {
            // Go to next char
            charIndex++;
            // If the character index is larger than or equal to the length of the text, the truncation is unachievable.
            if (charIndex >= text.Length)
            {
                // Truncation is unachievable!
                truncated = true;
                truncatedText = string.Empty;
                // Throw exception so the user knows what's going on.
                string msg = "The desired width of the string is too small to truncate to.";
                Console.WriteLine(msg);
                break;
            }
            else
            {
                // Truncation is still applicable!
                // Raise the flag, indicating that text is truncated.
                truncated = true;
                // Check which way to text should be truncated to, then remove one char and add an ellipsis.
                if (direction)
                {
                    // Truncate to the left. Add ellipsis and remove from the left.
                    truncatedText = string.Concat("…", text.AsSpan(charIndex));
                }
                else
                {
                    // Truncate to the right. Remove from the right and add the ellipsis.
                    truncatedText = string.Concat(text.AsSpan(0, text.Length - charIndex), "…");
                }

                // Measure the string again.
                size = MeasureString(truncatedText, font);
            }
        }
        // If the text got truncated, change the return value to the truncated text.
        if (truncated) returnText = truncatedText;
        else returnText = text;
        // Return the desired text.
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
}