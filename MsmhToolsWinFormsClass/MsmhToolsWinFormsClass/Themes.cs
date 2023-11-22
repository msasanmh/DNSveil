using CustomControls;
using MsmhToolsClass;

namespace MsmhToolsWinFormsClass.Themes;

public static class Theme
{
    // DodgerBlue: HEX value: #1E90FF RGB value: 30,144,255
    // IndianRed: HEX value: #CD5C5C RGB value: 205,92,92
    //=======================================================================================
    public struct Themes
    {
        public const string Light = "Light";
        public const string Dark = "Dark";
    }
    //=======================================================================================
    public static void LoadTheme(Form form, string theme)
    {
        if (theme.Equals(Themes.Light))
        {
            // Load Light Theme
            Colors.InitializeLight();
            foreach (Control c in Controllers.GetAllControls(form))
            {
                SetColors(c);
            }
            SetColorsByType(form);
        }
        else if (theme.Equals(Themes.Dark))
        {
            // Load Dark Theme
            Colors.InitializeDark();
            form.SetDarkTitleBar(true); // Make TitleBar Black
            foreach (Control c in Controllers.GetAllControls(form))
            {
                c.SetDarkControl();
                SetColors(c);
            }
            SetColorsByType(form);
        }

        // Set Static CustomMessageBox Parent
        CustomMessageBox.SetParent = form;
    }
    //=======================================================================================
    internal static bool OverrideColors { get; set; } = false;
    internal static Color OverrideBorderColor { get; set; }
    public static void ChangeBorderColor(Form form, Color color)
    {
        OverrideColors = true;
        OverrideBorderColor = color;
        Colors.Border = OverrideBorderColor;
        foreach (Control c in Controllers.GetAllControls(form)) SetColors(c);
        SetColorsByType(form);
    }
    //=======================================================================================
    private static void SetColorsByType(Form form)
    {
        // Find ContextMenu Controls
        foreach (var ccms in Controllers.GetAllControlsByType<CustomContextMenuStrip>(form))
        {
            ccms.BackColor = Colors.BackColor;
            ccms.ForeColor = Colors.ForeColor;
            ccms.BorderColor = Colors.Border;
            ccms.SelectionColor = Colors.Selection;
        }
        // Find LinkLabel
        foreach (var ll in Controllers.GetAllControlsByType<LinkLabel>(form))
        {
            ll.BackColor = Colors.BackColor;
            ll.ForeColor = Colors.ForeColor;
            ll.LinkColor = Colors.Link;
            ll.ActiveLinkColor = Colors.LinkActive;
            ll.VisitedLinkColor = Colors.LinkVisited;
        }
        // Find ToolStrip Controls
        foreach (var ctscb in Controllers.GetAllControlsByType<CustomToolStripComboBox>(form))
        {
            ctscb.BackColor = Colors.BackColor;
            ctscb.ForeColor = Colors.ForeColor;
            ctscb.BorderColor = Colors.Border;
            ctscb.SelectionColor = Colors.Selection;
        }
    }
    //=======================================================================================
    public static void SetColors(Control c)
    {
        if (c is TabPage) return;

        c.BackColor = Colors.BackColor;
        c.ForeColor = Colors.ForeColor;
        if (c is CustomButton customButton)
        {
            customButton.BorderColor = Colors.Border;
            customButton.SelectionColor = Colors.SelectionRectangle;
        }
        else if (c is CustomCheckBox customCheckBox)
        {
            customCheckBox.BorderColor = Colors.Border;
            customCheckBox.CheckColor = Colors.Tick;
            customCheckBox.SelectionColor = Colors.SelectionRectangle;
        }
        else if (c is CustomComboBox customComboBox)
        {
            customComboBox.BorderColor = Colors.Border;
            customComboBox.SelectionColor = Colors.Selection;
        }
        else if (c is CustomContextMenuStrip customContextMenuStrip)
        {
            customContextMenuStrip.BorderColor = Colors.Border;
            customContextMenuStrip.SelectionColor = Colors.Selection;
        }
        else if (c is CustomDataGridView customDataGridView)
        {
            customDataGridView.BorderColor = Colors.Border;
            customDataGridView.SelectionColor = Colors.Selection;
            customDataGridView.GridColor = Colors.GridLines;
            customDataGridView.CheckColor = Colors.Tick;
        }
        else if (c is CustomGroupBox customGroupBox)
        {
            customGroupBox.BorderColor = Colors.Border;
        }
        else if (c is CustomLabel customLabel)
        {
            customLabel.BorderColor = Colors.Border;
            customLabel.BackColor = Colors.BackColor;
            customLabel.ForeColor = Colors.ForeColor;
        }
        else if (c is CustomMenuStrip customMenuStrip)
        {
            customMenuStrip.BorderColor = Colors.Border;
            customMenuStrip.SelectionColor = Colors.Selection;
        }
        else if (c is CustomNumericUpDown customNumericUpDown)
        {
            customNumericUpDown.BorderColor = Colors.Border;
        }
        else if (c is CustomPanel customPanel)
        {
            customPanel.BorderColor = Colors.Border;
        }
        else if (c is CustomProgressBar customProgressBar)
        {
            customProgressBar.BorderColor = Colors.Border;
            customProgressBar.ChunksColor = Colors.Chunks;
        }
        else if (c is CustomRadioButton customRadioButton)
        {
            customRadioButton.BorderColor = Colors.Border;
            customRadioButton.CheckColor = Colors.Tick;
            customRadioButton.SelectionColor = Colors.SelectionRectangle;
        }
        else if (c is CustomRichTextBox customRichTextBox)
        {
            customRichTextBox.BorderColor = Colors.Border;
        }
        else if (c is CustomStatusStrip customStatusStrip)
        {
            customStatusStrip.BorderColor = Colors.Border;
            customStatusStrip.SelectionColor = Colors.Selection;
        }
        else if (c is CustomTabControl customTabControl)
        {
            customTabControl.BackColor = Colors.BackColor;
            customTabControl.ForeColor = Colors.ForeColor;
            customTabControl.BorderColor = Colors.Border;
        }
        else if (c is CustomTextBox customTextBox)
        {
            customTextBox.BorderColor = Colors.Border;
        }
        else if (c is CustomTimeUpDown customTimeUpDown)
        {
            customTimeUpDown.BorderColor = Colors.Border;
        }
        else if (c is CustomToolStrip customToolStrip)
        {
            customToolStrip.BorderColor = Colors.Border;
            customToolStrip.SelectionColor = Colors.Selection;
            foreach (ToolStripItem item in customToolStrip.Items)
            {
                var items = Controllers.GetAllToolStripItems(item);
                foreach (ToolStripItem? toolItem in items)
                    if (toolItem is ToolStripSeparator tss)
                    {
                        tss.ForeColor = Colors.Border;
                    }
            }
        }
        else if (c is CustomVScrollBar customVScrollBar)
        {
            customVScrollBar.BorderColor = Colors.Border;
        }
        else if (c is Label label)
        {
            label.BackColor = Colors.BackColor;
            label.ForeColor = Colors.ForeColor;
        }
        else if (c is LinkLabel linkLabel)
        {
            linkLabel.LinkColor = Colors.Link;
            linkLabel.ActiveLinkColor = Colors.LinkActive;
            linkLabel.VisitedLinkColor = Colors.LinkVisited;
        }
    }
    //=======================================================================================
    internal sealed class Colors
    {
        internal static Color BackColor { get; set; }
        internal static Color BackColorDisabled { get; set; }
        internal static Color BackColorDarker { get; set; }
        internal static Color BackColorDarkerDisabled { get; set; }
        internal static Color BackColorMouseHover { get; set; }
        internal static Color BackColorMouseDown { get; set; }
        internal static Color ForeColor { get; set; }
        internal static Color ForeColorDisabled { get; set; }
        internal static Color Border { get; set; }
        internal static Color BorderDisabled { get; set; }
        internal static Color Chunks { get; set; }
        internal static Color GridLines { get; set; }
        internal static Color GridLinesDisabled { get; set; }
        internal static Color Link { get; set; }
        internal static Color LinkActive { get; set; }
        internal static Color LinkVisited { get; set; }
        internal static Color Selection { get; set; }
        internal static Color SelectionRectangle { get; set; }
        internal static Color SelectionUnfocused { get; set; }
        internal static Color Tick { get; set; }
        internal static Color TickDisabled { get; set; }
        internal static Color TitleBarBackColor { get; set; }
        internal static Color TitleBarForeColor { get; set; }
        internal static void InitializeLight()
        {
            BackColor = SystemColors.Control;
            BackColorDisabled = BackColor.ChangeBrightness(-0.3f);
            BackColorDarker = BackColor.ChangeBrightness(-0.3f);
            BackColorDarkerDisabled = BackColorDarker.ChangeBrightness(-0.3f);
            BackColorMouseHover = BackColor.ChangeBrightness(-0.1f);
            BackColorMouseDown = BackColorMouseHover.ChangeBrightness(-0.1f);
            ForeColor = Color.Black;
            ForeColorDisabled = ForeColor.ChangeBrightness(0.3f);
            Border = Color.DodgerBlue;
            if (OverrideColors) Border = OverrideBorderColor;
            BorderDisabled = Border.ChangeBrightness(0.3f);
            Chunks = Color.DodgerBlue;
            GridLines = ForeColor.ChangeBrightness(0.5f);
            GridLinesDisabled = GridLines.ChangeBrightness(0.3f);
            Link = ForeColor;
            LinkActive = Color.IndianRed;
            LinkVisited = Link;
            Selection = Color.FromArgb(104, 151, 187);
            SelectionRectangle = Selection;
            SelectionUnfocused = Selection.ChangeBrightness(0.3f);
            Tick = Border;
            TickDisabled = Tick.ChangeBrightness(0.3f);
            TitleBarBackColor = Color.LightBlue;
            TitleBarForeColor = Color.Black;
            CC();
        }
        internal static void InitializeDark()
        {
            BackColor = Color.DarkGray.ChangeBrightness(-0.8f);
            BackColorDisabled = BackColor.ChangeBrightness(0.3f);
            BackColorDarker = BackColor.ChangeBrightness(-0.3f);
            BackColorDarkerDisabled = BackColorDarker.ChangeBrightness(0.3f);
            BackColorMouseHover = BackColor.ChangeBrightness(0.1f);
            BackColorMouseDown = BackColorMouseHover.ChangeBrightness(0.1f);
            ForeColor = Color.LightGray;
            ForeColorDisabled = ForeColor.ChangeBrightness(-0.3f);
            Border = Color.DodgerBlue;
            if (OverrideColors) Border = OverrideBorderColor;
            BorderDisabled = Border.ChangeBrightness(-0.3f);
            Chunks = Color.DodgerBlue;
            GridLines = ForeColor.ChangeBrightness(-0.5f);
            GridLinesDisabled = GridLines.ChangeBrightness(-0.3f);
            Link = ForeColor;
            LinkActive = Color.IndianRed;
            LinkVisited = Link;
            Selection = Color.Black;
            SelectionRectangle = Selection;
            SelectionUnfocused = Selection.ChangeBrightness(0.3f);
            Tick = Border;
            TickDisabled = Tick.ChangeBrightness(-0.3f);
            TitleBarBackColor = Color.DarkBlue;
            TitleBarForeColor = Color.White;
            CC();
        }

        private static void CC()
        {
            // MessageBox
            //CustomMessageBox
            CustomMessageBox.BackColor = BackColor;
            CustomMessageBox.ForeColor = ForeColor;
            CustomMessageBox.BorderColor = Border;

            // InputBox
            CustomInputBox.BackColor = BackColor;
            CustomInputBox.ForeColor = ForeColor;
            CustomInputBox.BorderColor = Border;
        }
    }
}