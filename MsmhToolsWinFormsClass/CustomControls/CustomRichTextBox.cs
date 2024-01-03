using MsmhToolsClass;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, January 27, 2023.
*/

namespace CustomControls
{
    [DefaultEvent("TextChanged")]
    public class CustomRichTextBox : UserControl
    {
        private static class Methods
        {
            [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
            private extern static int SetWindowTheme(IntPtr controlHandle, string appName, string? idList);
            internal static void SetDarkControl(Control control)
            {
                _ = SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
                foreach (Control c in control.Controls)
                {
                    _ = SetWindowTheme(c.Handle, "DarkMode_Explorer", null);
                }
            }

            [DllImport("user32.dll", EntryPoint = "ShowCaret")]
            internal static extern long ShowCaret(IntPtr hwnd);
            [DllImport("user32.dll", EntryPoint = "HideCaret")]
            internal static extern long HideCaret(IntPtr hwnd);
        }

        // Disable
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoSizeMode AutoSizeMode { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoValidate AutoValidate { get; set; }

        private readonly RichTextBox richTextBox = new();
        private bool isFocused = false;

        private Color mBorderColor = Color.Blue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Border Color")]
        public Color BorderColor
        {
            get { return mBorderColor; }
            set
            {
                if (mBorderColor != value)
                {
                    mBorderColor = value;
                    Invalidate();
                }
            }
        }

        private bool mBorder = true;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border")]
        public bool Border
        {
            get { return mBorder; }
            set
            {
                if (mBorder != value)
                {
                    mBorder = value;
                    Invalidate();
                }
            }
        }

        private int mBorderSize = 1;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border Size")]
        public int BorderSize
        {
            get { return mBorderSize; }
            set
            {
                if (mBorderSize != value)
                {
                    mBorderSize = value;
                    Invalidate();
                }
            }
        }

        private int mRoundedCorners = 0;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Rounded Corners")]
        public int RoundedCorners
        {
            get { return mRoundedCorners; }
            set
            {
                if (mRoundedCorners != value)
                {
                    mRoundedCorners = value;
                    Invalidate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Cursor")]
        public override Cursor Cursor
        {
            get { return base.Cursor; }
            set
            {
                base.Cursor = value;
                richTextBox.Cursor = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Font")]
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                richTextBox.Font = value;
                if (DesignMode)
                    UpdateControlSize();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("ScrollBar")]
        public ScrollBars ScrollBars
        {
            get { return (ScrollBars)richTextBox.ScrollBars; }
            set { richTextBox.ScrollBars = (RichTextBoxScrollBars)value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return richTextBox.Text; }
            set
            {
                base.Text = value;
                richTextBox.Text = value;
            }
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0," +
            "Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Texts")]
        public string Texts
        {
            get { return richTextBox.Text; }
            set { richTextBox.Text = value; }
        }

        private bool mUnderlinedStyle = false;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border Underlined Style")]
        public bool UnderlinedStyle
        {
            get { return mUnderlinedStyle; }
            set
            {
                if (mUnderlinedStyle != value)
                {
                    mUnderlinedStyle = value;
                    Invalidate();
                }
            }
        }

        // Scroll to the bottom
        private bool mScrollToBottom = false;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Scrolls to the bottom of the CustomRichTextBox on Text Changed.")]
        public bool ScrollToBottom
        {
            get { return mScrollToBottom; }
            set
            {
                if (mScrollToBottom != value)
                {
                    mScrollToBottom = value;
                    Invalidate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Accepts Tab")]
        public bool AcceptsTab
        {
            get { return richTextBox.AcceptsTab; }
            set { richTextBox.AcceptsTab = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Allow Drop")]
        public override bool AllowDrop
        {
            get { return base.AllowDrop; }
            set
            {
                base.AllowDrop = value;
                richTextBox.AllowDrop = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Auto Word Selection")]
        public bool AutoWordSelection
        {
            get { return richTextBox.AutoWordSelection; }
            set { richTextBox.AutoWordSelection = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Bullet Indent")]
        public int BulletIndent
        {
            get { return richTextBox.BulletIndent; }
            set { richTextBox.BulletIndent = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Context Menu Strip")]
        public override ContextMenuStrip ContextMenuStrip
        {
            get { return base.ContextMenuStrip; }
            set
            {
                base.ContextMenuStrip = value;
                richTextBox.ContextMenuStrip = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Detect Urls")]
        public bool DetectUrls
        {
            get { return richTextBox.DetectUrls; }
            set { richTextBox.DetectUrls = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Enable Auto Drag Drop")]
        public bool EnableAutoDragDrop
        {
            get { return richTextBox.EnableAutoDragDrop; }
            set { richTextBox.EnableAutoDragDrop = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Hide Selection")]
        public bool HideSelection
        {
            get { return richTextBox.HideSelection; }
            set { richTextBox.HideSelection = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Input Method Editor")]
        public new ImeMode ImeMode
        {
            get { return richTextBox.ImeMode; }
            set { richTextBox.ImeMode = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Max Length")]
        public int MaxLength
        {
            get { return richTextBox.MaxLength; }
            set { richTextBox.MaxLength = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Multiline Style")]
        public bool Multiline
        {
            get { return richTextBox.Multiline; }
            set
            {
                richTextBox.Multiline = value;
                if (DesignMode)
                    UpdateControlSize();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Read Only")]
        public bool ReadOnly
        {
            get { return richTextBox.ReadOnly; }
            set { richTextBox.ReadOnly = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Right Margin")]
        public int RightMargin
        {
            get { return richTextBox.RightMargin; }
            set { richTextBox.RightMargin = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Shortcuts Enabled")]
        public bool ShortcutsEnabled
        {
            get { return richTextBox.ShortcutsEnabled; }
            set { richTextBox.ShortcutsEnabled = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Tab Index")]
        public new int TabIndex
        {
            get { return richTextBox.TabIndex; }
            set { richTextBox.TabIndex = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Tab Stop")]
        public new bool TabStop
        {
            get { return richTextBox.TabStop; }
            set { richTextBox.TabStop = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Word Wrap Style")]
        public bool WordWrap
        {
            get { return richTextBox.WordWrap; }
            set { richTextBox.WordWrap = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Behavior"), Description("Zoom Factor")]
        public float ZoomFactor
        {
            get { return richTextBox.ZoomFactor; }
            set { richTextBox.ZoomFactor = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(false)]
        public Color SelectionColor
        {
            get { return richTextBox.SelectionColor; }
            set { richTextBox.SelectionColor = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(false)]
        public int SelectionLength
        {
            get { return richTextBox.SelectionLength; }
            set { richTextBox.SelectionLength = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(false)]
        public int SelectionStart
        {
            get { return richTextBox.SelectionStart; }
            set { richTextBox.SelectionStart = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(false)]
        public int TextLength
        {
            get { return richTextBox.TextLength; }
        }

        // Custom Event
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Text Appended")]
        public event EventHandler? TextAppended;

        public override void ResetText()
        {
            richTextBox.ResetText();
        }

        private bool ApplicationIdle = false;
        private bool once = true;

        public void AppendText(string text)
        {
            try
            {
                richTextBox.AppendText(text);
                richTextBox.Refresh();
                TextAppended?.Invoke(text, EventArgs.Empty);
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        public void AppendText(string text, Color color)
        {
            try
            {
                richTextBox.SelectionStart = richTextBox.TextLength;
                richTextBox.SelectionLength = 0;
                richTextBox.SelectionColor = color;
                richTextBox.AppendText(text);
                richTextBox.SelectionColor = richTextBox.ForeColor;
                richTextBox.Refresh();
                TextAppended?.Invoke(text, EventArgs.Empty);
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        public CustomRichTextBox() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Font = new("Segoe UI", 9);
            AutoScaleMode = AutoScaleMode.None;
            Padding = new(0);
            Size = new(100, 23);

            // Default
            BackColor = Color.DimGray;
            ForeColor = Color.White;
            AutoScroll = false;
            AutoScrollMargin = new(0, 0);
            AutoScrollMinSize = new(0, 0);
            AutoSize = false;

            // Disabled
            AutoSizeMode = AutoSizeMode.GrowOnly;
            AutoValidate = AutoValidate.EnablePreventFocusChange;

            Controls.Add(richTextBox);
            richTextBox.BackColor = GetBackColor();
            richTextBox.ForeColor = GetForeColor();

            //richTextBox.Dock = DockStyle.Fill;
            richTextBox.BorderStyle = BorderStyle.None;

            // Events
            Application.Idle += Application_Idle;
            EnabledChanged += CustomRichTextBox_EnabledChanged;
            BackColorChanged += CustomRichTextBox_BackColorChanged;
            ForeColorChanged += CustomRichTextBox_ForeColorChanged;
            richTextBox.Click += RichTextBox_Click;
            richTextBox.MouseClick += RichTextBox_MouseClick;
            richTextBox.MouseDoubleClick += RichTextBox_MouseDoubleClick;
            richTextBox.MouseEnter += RichTextBox_MouseEnter;
            richTextBox.MouseLeave += RichTextBox_MouseLeave;
            richTextBox.KeyPress += RichTextBox_KeyPress;
            richTextBox.Enter += RichTextBox_Enter;
            richTextBox.Leave += RichTextBox_Leave;
            richTextBox.Invalidated += RichTextBox_Invalidated;
            richTextBox.AcceptsTabChanged += RichTextBox_AcceptsTabChanged;
            richTextBox.GotFocus += RichTextBox_GotFocus;
            richTextBox.HideSelectionChanged += RichTextBox_HideSelectionChanged;
            richTextBox.ModifiedChanged += RichTextBox_ModifiedChanged;
            richTextBox.MultilineChanged += RichTextBox_MultilineChanged;
            richTextBox.ReadOnlyChanged += RichTextBox_ReadOnlyChanged;
            richTextBox.SelectionChanged += RichTextBox_SelectionChanged;
            richTextBox.TextChanged += RichTextBox_TextChanged;
        }

        // Events
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? Scroll;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? Load;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? PaddingChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? AutoSizeChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? AutoValidateChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? BackgroundImageChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Mark as deprecated.", true)]
        public new event EventHandler? BackgroundImageLayoutChanged;

        private void Application_Idle(object? sender, EventArgs e)
        {
            ApplicationIdle = true;
            if (Parent != null && FindForm() != null)
            {
                if (once)
                {
                    Control topParent = FindForm();
                    topParent.Move -= TopParent_Move;
                    topParent.Move += TopParent_Move;
                    Parent.Move -= Parent_Move;
                    Parent.Move += Parent_Move;
                    Invalidate();
                    once = false;
                }
            }
        }

        private void TopParent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void Parent_Move(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void CustomRichTextBox_EnabledChanged(object? sender, EventArgs e)
        {
            richTextBox.Enabled = Enabled;
            Invalidate();
            richTextBox.Invalidate();
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Accepts Tab Changed")]
        public event EventHandler? AcceptsTabChanged;
        private void RichTextBox_AcceptsTabChanged(object? sender, EventArgs e)
        {
            AcceptsTabChanged?.Invoke(sender, e);
        }

        private void RichTextBox_GotFocus(object? sender, EventArgs e)
        {
            OnGotFocus(e);
            if (HideSelection)
                Methods.HideCaret(richTextBox.Handle);
            else
                Methods.ShowCaret(richTextBox.Handle);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Hide Selection Changed")]
        public event EventHandler? HideSelectionChanged;
        private void RichTextBox_HideSelectionChanged(object? sender, EventArgs e)
        {
            HideSelectionChanged?.Invoke(sender, e);
            if (HideSelection)
                Methods.HideCaret(richTextBox.Handle);
            else
                Methods.ShowCaret(richTextBox.Handle);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Modified Changed")]
        public event EventHandler? ModifiedChanged;
        private void RichTextBox_ModifiedChanged(object? sender, EventArgs e)
        {
            ModifiedChanged?.Invoke(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Multiline Changed")]
        public event EventHandler? MultilineChanged;
        private bool MultilineChangedBool = false;
        private void RichTextBox_MultilineChanged(object? sender, EventArgs e)
        {
            if (MultilineChanged != null && ApplicationIdle == true)
            {
                MultilineChanged.Invoke(sender, e);
                MultilineChangedBool = true;
                UpdateControlSize();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Read Only Changed")]
        public event EventHandler? ReadOnlyChanged;
        private void RichTextBox_ReadOnlyChanged(object? sender, EventArgs e)
        {
            ReadOnlyChanged?.Invoke(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Selection Changed")]
        public event EventHandler? SelectionChanged;
        private void RichTextBox_SelectionChanged(object? sender, EventArgs e)
        {
            SelectionChanged?.Invoke(sender, e);
            if (HideSelection)
            {
                richTextBox.SelectionLength = 0;
                Methods.HideCaret(richTextBox.Handle);
            }
            else
                Methods.ShowCaret(richTextBox.Handle);
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Text Changed")]
        public new event EventHandler? TextChanged;
        private void RichTextBox_TextChanged(object? sender, EventArgs e)
        {
            TextChanged?.Invoke(sender, e);
            richTextBox.BackColor = GetBackColor();
            richTextBox.ForeColor = GetForeColor();
            if (ScrollToBottom)
            {
                richTextBox.SelectionStart = richTextBox.TextLength;
                richTextBox.SelectionLength = 0;
                richTextBox.ScrollToCaret();
            }
        }

        private void CustomRichTextBox_BackColorChanged(object? sender, EventArgs e)
        {
            richTextBox.BackColor = GetBackColor();
            Invalidate();
        }

        private void CustomRichTextBox_ForeColorChanged(object? sender, EventArgs e)
        {
            richTextBox.ForeColor = GetForeColor();
            Invalidate();
        }

        private void RichTextBox_Click(object? sender, EventArgs e)
        {
            OnClick(e);
        }

        private void RichTextBox_MouseClick(object? sender, MouseEventArgs e)
        {
            OnMouseClick(e);
        }

        private void RichTextBox_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            OnMouseDoubleClick(e);
        }

        private void RichTextBox_MouseEnter(object? sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        private void RichTextBox_MouseLeave(object? sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        private void RichTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void RichTextBox_Enter(object? sender, EventArgs e)
        {
            isFocused = true;
            Invalidate();
        }

        private void RichTextBox_Leave(object? sender, EventArgs e)
        {
            isFocused = false;
            Invalidate();
        }

        private void RichTextBox_Invalidated(object? sender, InvalidateEventArgs e)
        {
            if (BackColor.DarkOrLight() == "Dark")
                Methods.SetDarkControl(richTextBox);
            richTextBox.Enabled = Enabled;
            richTextBox.BackColor = GetBackColor();
            richTextBox.ForeColor = GetForeColor();
        }

        // Overridden Methods
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Visible) return;

            base.OnPaint(e);

            Color borderColor = GetBorderColor();

            e.Graphics.Clear(GetBackColor());

            //Draw border
            using Pen penBorder = new(borderColor, mBorderSize);
            penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;

            if (Border)
            {
                if (mUnderlinedStyle) // Line Style
                    e.Graphics.DrawLine(penBorder, 0, Height - 1, Width, Height - 1);
                else //Normal Style
                {
                    int r = RoundedCorners;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawRoundedRectangle(penBorder, new Rectangle(0, 0, Width - 1, Height - 1), r, r, r, r);
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
            UpdateControlSize();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateControlSize();
        }

        private void UpdateControlSize()
        {
            if (richTextBox.Multiline == false)
            {
                int txtHeight = TextRenderer.MeasureText("Text", Font).Height + 2;
                int padding = 6;
                if (!MultilineChangedBool)
                {
                    richTextBox.Multiline = true;
                    richTextBox.MinimumSize = new Size(0, txtHeight);
                    richTextBox.Multiline = false;
                }
                else
                    MultilineChangedBool = false;
                richTextBox.Height = txtHeight;
                richTextBox.Width = Width - padding;
                Height = richTextBox.Height + padding;
                richTextBox.Location = new(padding / 2, padding / 2);
            }
            else
            {
                int txtHeight = TextRenderer.MeasureText("Text", Font).Height + 2;
                int padding = 6;
                richTextBox.MinimumSize = new Size(0, txtHeight);
                MinimumSize = new Size(0, txtHeight + padding);
                richTextBox.Height = Height - padding;
                richTextBox.Width = Width - padding;
                richTextBox.Location = new(padding / 2, padding / 2);
            }
        }

        private Color GetBackColor()
        {
            if (Enabled)
                return BackColor;
            else
            {
                if (BackColor.DarkOrLight() == "Dark")
                    return BackColor.ChangeBrightness(0.3f);
                else
                    return BackColor.ChangeBrightness(-0.3f);
            }
        }

        private Color GetForeColor()
        {
            if (Enabled)
                return ForeColor;
            else
            {
                if (ForeColor.DarkOrLight() == "Dark")
                    return ForeColor.ChangeBrightness(0.2f);
                else
                    return ForeColor.ChangeBrightness(-0.2f);
            }
        }

        private Color GetBorderColor()
        {
            if (Enabled)
            {
                if (isFocused)
                {
                    // Focused Border Color
                    if (BorderColor.DarkOrLight() == "Dark")
                        return BorderColor.ChangeBrightness(0.4f);
                    else
                        return BorderColor.ChangeBrightness(-0.4f);
                }
                else
                    return BorderColor;
            }
            else
            {
                // Disabled Border Color
                if (BorderColor.DarkOrLight() == "Dark")
                    return BorderColor.ChangeBrightness(0.3f);
                else
                    return BorderColor.ChangeBrightness(-0.3f);
            }
        }

    }
}
